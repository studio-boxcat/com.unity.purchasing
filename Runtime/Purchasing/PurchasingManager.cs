// ReSharper disable InconsistentNaming
#nullable enable
using System.Collections.Generic;
using System.Linq;
using UnityEngine.Assertions;
using UnityEngine.Purchasing.Extension;

namespace UnityEngine.Purchasing
{
    /// <summary>
    /// The main controller for Applications using Unity Purchasing.
    /// </summary>
    public class PurchasingManager : IStoreCallback
    {
        private readonly AppStore m_StoreType;
        private readonly IStore m_Store;
        private readonly IStoreListener? m_Listener;
        private readonly TransactionLog m_TransactionLog = new(Application.persistentDataPath);
        private readonly HashSet<string> purchasesProcessedInSession = new();

        internal PurchasingManager(AppStore storeType, IStore store, ProductDefinition[] products, IStoreListener? listener)
        {
            Assert.AreEqual(products.Length, products.Select(p => p.id).Distinct().Count(), "Product ids must be unique");

            m_StoreType = storeType;
            m_Store = store;
            this.products = new ProductCollection(products);
            m_Listener = listener;

            m_Store.Initialize(this);
        }

        public void InitiatePurchase(Product product)
        {
            if (!product.availableToPurchase)
            {
                m_Listener?.OnPurchaseFailed(product, new PurchaseFailureDescription(product.definition.id, PurchaseFailureReason.ProductUnavailable,
                    "No products were found when fetching from the store"));
                return;
            }

            m_Store.Purchase(product.definition, developerPayload: "");
        }

        /// <summary>
        /// Where an Application returned ProcessingResult.Pending they can manually
        /// finish the transaction by calling this method.
        /// </summary>
        public void ConfirmPendingPurchase(Product product)
        {
            if (string.IsNullOrEmpty(product.transactionID))
            {
                UnityUtil.LogError("Unable to confirm purchase; Product has missing or empty transactionID");
                return;
            }

            {
                m_TransactionLog.Record(product.transactionID);
            }

            m_Store.FinishTransaction(product.definition, product.transactionID);
        }

        public ProductCollection products { get; }

        /// <summary>
        /// Called by our IStore when a purchase succeeds.
        /// </summary>
        public void OnPurchaseSucceeded(string id, string? receipt, string transactionId)
        {
            var product = products.WithStoreSpecificID(id);
            if (null == product)
            {
                // If is possible for stores to tell us about products we have not yet
                // requested details of.
                // We should still tell the App in this scenario, albeit with incomplete information.
                var definition = new ProductDefinition(id, ProductType.NonConsumable);
                product = new Product(definition, new ProductMetadata());
            }

            UpdateProductReceiptAndTransactionID(product, receipt, transactionId);
            ProcessPurchaseIfNew(product);
        }

        void UpdateProductReceiptAndTransactionID(Product product, string? receipt, string transactionId)
        {
            {
                product.receipt = CreateUnifiedReceipt(receipt, transactionId);
                product.transactionID = transactionId;
            }
        }

        public void OnAllPurchasesRetrieved(List<Product> purchasedProducts)
        {
            {
                foreach (var product in products.all)
                {
                    var purchasedProduct = purchasedProducts?.FirstOrDefault(firstPurchasedProduct => firstPurchasedProduct.definition.id == product.definition.id);
                    if (purchasedProduct != null)
                    {
                        HandlePurchaseRetrieved(product, purchasedProduct);
                    }
                    else
                    {
                        ClearProductReceipt(product);
                    }
                }
            }
        }

        // TODO IAP-2929: Add this to IStoreCallback in a major release
        internal static void OnEntitlementRevoked(Product revokedProduct)
        {
            ClearProductReceipt(revokedProduct);
        }

        void HandlePurchaseRetrieved(Product product, Product purchasedProduct)
        {
            UpdateProductReceiptAndTransactionID(product, purchasedProduct.receipt, purchasedProduct.transactionID);
            if (initialized && !WasPurchaseAlreadyProcessed(purchasedProduct.transactionID))
            {
                ProcessPurchaseIfNew(product);
            }
        }

        bool WasPurchaseAlreadyProcessed(string transactionId)
        {
            return purchasesProcessedInSession.Contains(transactionId);
        }

        static void ClearProductReceipt(Product product)
        {
            product.receipt = null;
            product.transactionID = null;
        }

        public void OnSetupFailed(InitializationFailureReason reason, string? message)
        {
            if (initialized)
            {
            }
            else
            {
                m_Listener?.OnInitializeFailed(reason, message);
            }
        }

        public void OnPurchaseFailed(PurchaseFailureDescription description)
        {
            if (description != null)
            {
                var product = products.WithStoreSpecificID(description.productId);
                if (null == product)
                {
                    UnityUtil.LogError("Failed to purchase unknown product {0}", "productId:" + description.productId + " reason:" + description.reason + " message:" + description.message);
                    return;
                }

                UnityUtil.LogWarning("onPurchaseFailedEvent({0})", "productId:" + product.definition.id + " message:" + description.message);
                m_Listener?.OnPurchaseFailed(product, description);
            }
        }

        /// <summary>
        /// Called back by our IStore when it has fetched the latest product data.
        /// </summary>
        public void OnProductsRetrieved(List<ProductDescription> products)
        {
            foreach (var product in products)
            {
                var matchedProduct = this.products.WithStoreSpecificID(product.storeSpecificId);
                if (null == matchedProduct)
                    continue;

                matchedProduct.availableToPurchase = true;
                matchedProduct.metadata = product.metadata;
                matchedProduct.transactionID = product.transactionId;

                if (!string.IsNullOrEmpty(product.receipt))
                {
                    matchedProduct.receipt = CreateUnifiedReceipt(product.receipt, product.transactionId);
                }
            }

            // Fire our initialisation events if this is a first poll.
            CheckForInitialization(products.Count);

            _retrievalVersion++;

            ProcessPurchaseOnStart();
        }

        string CreateUnifiedReceipt(string? rawReceipt, string transactionId)
        {
            return UnifiedReceiptFormatter.FormatUnifiedReceipt(rawReceipt, transactionId, m_StoreType);
        }

        void ProcessPurchaseOnStart()
        {
            foreach (var product in products.all)
            {
                if (!string.IsNullOrEmpty(product.receipt) && !string.IsNullOrEmpty(product.transactionID))
                {
                    ProcessPurchaseIfNew(product);
                }
            }
        }

        /// <summary>
        /// Checks the product's transaction ID for uniqueness
        /// against the transaction log and calls the Application's
        /// ProcessPurchase method if so.
        /// </summary>
        private void ProcessPurchaseIfNew(Product product)
        {
            if (HasRecordedTransaction(product.transactionID))
            {
                m_Store.FinishTransaction(product.definition, product.transactionID);
                return;
            }

            purchasesProcessedInSession.Add(product.transactionID!);

            var p = new PurchaseEventArgs(product);

            // Applications may elect to delay confirmations of purchases,
            // such as when persisting purchase state asynchronously.
            if (m_Listener?.ProcessPurchase(p) == PurchaseProcessingResult.Complete)
            {
                ConfirmPendingPurchase(product);
            }
        }

        bool HasRecordedTransaction(string? transactionId)
        {
            return m_TransactionLog.HasRecordOf(transactionId);
        }

        private bool initialized;

        private void CheckForInitialization(int productCount)
        {
            if (!initialized)
            {
                initialized = true;
                if (productCount > 0)
                {
                    m_Listener?.OnInitialized(this);
                }
                else
                {
                    m_Listener?.OnInitializeFailed(InitializationFailureReason.NoProductsAvailable,
                        "No product returned from the store.");
                }
            }
        }

        internal void Initialize()
        {
            // Start the initialisation process by fetching product metadata.
            m_Store.RetrieveProducts(products.definitions);
        }

        // Incremented only on successful OnProductsRetrieved, never on failure.
        // Callers poll this to detect completion; timeout handles failure/hang cases.
        // This avoids promise-based callbacks which can hang permanently on Google Play
        // when billing disconnects post-init (OnRetrieveProductsFailed silently drops
        // failures after initial initialization).
        private int _retrievalVersion;
        internal int RetrievalVersion => _retrievalVersion;

        /// <summary>
        /// Fire-and-forget re-fetch of product metadata from the store.
        /// Poll <see cref="RetrievalVersion"/> to detect completion.
        /// </summary>
        internal void RefreshProducts()
        {
            m_Store.RetrieveProducts(products.definitions);
        }
    }
}
