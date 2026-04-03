using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using UnityEngine.Purchasing.Extension;

namespace UnityEngine.Purchasing
{
    internal class FakeStore : JSONStore, INativeStore
    {
        public const string Name = "fake";
        private IStoreCallback m_Biller;
        private readonly List<string> m_PurchasedProducts = new List<string>();
        public string unavailableProductId { get; set; }

        public override void Initialize(IStoreCallback biller)
        {
            m_Biller = biller;
            base.Initialize(biller);
            SetNativeStore(this);
        }

        // INativeStore
        public void RetrieveProducts(string json)
        {
            var jsonList = (List<object>)MiniJson.JsonDecode(json);
            var productDefinitions = jsonList.DecodeJSON(Name);
            StoreRetrieveProducts(new ReadOnlyCollection<ProductDefinition>(productDefinitions.ToList()));
        }

        // This is now being used by the INativeStore implementation
        public void StoreRetrieveProducts(ReadOnlyCollection<ProductDefinition> productDefinitions)
        {
            var products = new List<ProductDescription>();
            foreach (var product in productDefinitions)
            {
                if (unavailableProductId != product.id)
                {
                    var metadata = new ProductMetadata("$9.99", "Fake title for " + product.id, "Fake description", "USD", 9.99m);
                    products.Add(new ProductDescription(product.storeSpecificId, metadata));
                }
            }

            void handleAllowInitializeOrRetrieveProducts(bool allow, InitializationFailureReason failureReason)
            {
                if (allow)
                {
                    m_Biller.OnProductsRetrieved(products);
                }
                else
                {
                    m_Biller.OnSetupFailed(failureReason, null);
                }
            }

            // To mimic typical store behavior, only display RetrieveProducts dialog for developers
            {
                // Default non-UI FakeStore RetrieveProducts behavior is to succeed
                handleAllowInitializeOrRetrieveProducts(true, InitializationFailureReason.AppNotKnown);
            }
        }

        // INativeStore
        public void Purchase(string productJSON, string developerPayload)
        {
            var dic = (Dictionary<string, object>)MiniJson.JsonDecode(productJSON);
            string id, storeId, type;
            ProductType itemType;

            dic.TryGetValue("id", out var obj);
            id = obj.ToString();
            dic.TryGetValue("storeSpecificId", out obj);
            storeId = obj.ToString();
            dic.TryGetValue("type", out obj);
            type = obj.ToString();
            itemType = Enum.IsDefined(typeof(ProductType), type) ? (ProductType)Enum.Parse(typeof(ProductType), type) : ProductType.Consumable;

            // This doesn't currently deal with "enabled" and "payouts" that could be included in the JSON
            var product = new ProductDefinition(id, storeId, itemType);

            FakePurchase(product);
        }

        void FakePurchase(ProductDefinition product)
        {
            // Our billing systems should only keep track of non consumables.
            if (product.type != ProductType.Consumable)
            {
                m_PurchasedProducts.Add(product.storeSpecificId);
            }

            void handleAllowPurchase(bool allow, PurchaseFailureReason failureReason)
            {
                if (allow)
                {
                    base.OnPurchaseSucceeded(product.storeSpecificId, "ThisIsFakeReceiptData", Guid.NewGuid().ToString());
                }
                else
                {
                    if (failureReason == (PurchaseFailureReason)Enum.Parse(typeof(PurchaseFailureReason), "Unknown"))
                    {
                        failureReason = PurchaseFailureReason.UserCancelled;
                    }

                    var failureDescription =
                        new PurchaseFailureDescription(product.storeSpecificId, failureReason, "failed a fake store purchase");

                    OnPurchaseFailed(failureDescription);
                }
            }

            {
                // Default non-UI FakeStore purchase behavior is to succeed
                handleAllowPurchase(true, (PurchaseFailureReason)Enum.Parse(typeof(PurchaseFailureReason), "Unknown"));
            }
        }

        // INativeStore
        public void FinishTransaction(string productJSON, string transactionID)
        {
            // we need this for INativeStore but won't be using
        }

        public override void FinishTransaction(ProductDefinition? product, string transactionId)
        {
        }
    }
}
