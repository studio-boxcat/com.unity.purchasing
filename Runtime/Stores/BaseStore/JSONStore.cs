using System;
using System.Collections.Generic;
using Stores.Util;
using UnityEngine.Purchasing.Extension;

namespace UnityEngine.Purchasing
{
    /// <summary>
    /// Internal store implementation passing store requests from the user through to the underlaying
    /// native store system, and back again. Binds a native store system binding to a callback.
    /// </summary>
    internal class JSONStore : IStore, IUnityCallback, ITransactionHistoryExtensions
    {
        protected IStoreCallback unity;
        private INativeStore m_Store;

        // m_Module is our StandardPurchasingModule, added via reflection to avoid core changes etc.
        private StandardPurchasingModule m_Module;

        protected ILogger m_Logger;

        protected JsonProductDescriptionsDeserializer m_ProductDescriptionsDeserializer;

        // ITransactionHistoryExtensions stuff
        //
        // Enhanced error information
        protected PurchaseFailureDescription m_LastPurchaseFailureDescription;
        private StoreSpecificPurchaseErrorCode m_LastPurchaseErrorCode = StoreSpecificPurchaseErrorCode.Unknown;

        const string k_StoreSpecificErrorCodeKey = "storeSpecificErrorCode";

        /// <summary>
        /// No arg constructor due to cyclical dependency on IUnityCallback.
        /// </summary>
        public JSONStore()
        {
            m_ProductDescriptionsDeserializer = new JsonProductDescriptionsDeserializer();
        }

        public void SetNativeStore(INativeStore native)
        {
            m_Store = native;
        }

        public void SetModule(StandardPurchasingModule module)
        {
            if (module == null)
            {
                return;
            }
            m_Module = module;
            m_Logger = module.logger ?? Debug.unityLogger;
        }

        public virtual void Initialize(IStoreCallback callback)
        {
            unity = callback;

            if (m_Module != null)
            {
            }
            else
            {
                if (m_Logger != null)
                {
                    m_Logger.LogIAPWarning("JSONStore init has no reference to SPM, can't start managed store");
                }
            }
        }

        public virtual void RetrieveProducts(ProductDefinition[] products)
        {
            m_Store.RetrieveProducts(JSONSerializer.SerializeProductDefs(products));
        }

        public virtual void Purchase(ProductDefinition product, string developerPayload)
        {
            m_Store.Purchase(JSONSerializer.SerializeProductDef(product), developerPayload);
        }

        public virtual void FinishTransaction(ProductDefinition? product, string transactionId)
        {
            // Product definitions may be null if a store tells Unity IAP about an unknown product;
            // Unity IAP will not have a corresponding definition but will still finish the transaction.
            var def = product == null ? null : JSONSerializer.SerializeProductDef(product.Value);
            m_Store.FinishTransaction(def, transactionId);
        }

        public void OnSetupFailed(string reason)
        {
            var r = (InitializationFailureReason)Enum.Parse(typeof(InitializationFailureReason), reason, true);
            unity.OnSetupFailed(r, null);
        }

        public virtual void OnProductsRetrieved(string json)
        {
            // NB: AppleStoreImpl overrides this completely and does not call the base.
            unity.OnProductsRetrieved(m_ProductDescriptionsDeserializer.DeserializeProductDescriptions(json));
        }

        public virtual void OnPurchaseSucceeded(string id, string receipt, string transactionID)
        {
            unity.OnPurchaseSucceeded(id, receipt, transactionID);
        }

        public void OnPurchaseFailed(string json)
        {
            OnPurchaseFailed(JSONSerializer.DeserializeFailureReason(json), json);
        }

        public void OnPurchaseFailed(PurchaseFailureDescription failure, string json = null)
        {
            m_LastPurchaseFailureDescription = failure;
            m_LastPurchaseErrorCode = ParseStoreSpecificPurchaseErrorCode(json);

            unity.OnPurchaseFailed(failure);
        }

        public PurchaseFailureDescription GetLastPurchaseFailureDescription()
        {
            return m_LastPurchaseFailureDescription;
        }

        public StoreSpecificPurchaseErrorCode GetLastStoreSpecificPurchaseErrorCode()
        {
            return m_LastPurchaseErrorCode;
        }

        private StoreSpecificPurchaseErrorCode ParseStoreSpecificPurchaseErrorCode(string json)
        {
            // If we didn't get any JSON just return Unknown.
            if (json == null)
            {
                return StoreSpecificPurchaseErrorCode.Unknown;
            }

            // If the dictionary contains a storeSpecificErrorCode, return it, otherwise return Unknown.
            var purchaseFailureDictionary = MiniJson.JsonDecode(json) as Dictionary<string, object>;
            if (purchaseFailureDictionary != null && purchaseFailureDictionary.ContainsKey(k_StoreSpecificErrorCodeKey) && Enum.IsDefined(typeof(StoreSpecificPurchaseErrorCode), (string)purchaseFailureDictionary[k_StoreSpecificErrorCodeKey]))
            {
                var storeSpecificErrorCodeString = (string)purchaseFailureDictionary[k_StoreSpecificErrorCodeKey];
                return (StoreSpecificPurchaseErrorCode)Enum.Parse(typeof(StoreSpecificPurchaseErrorCode),
                    storeSpecificErrorCodeString);
            }
            return StoreSpecificPurchaseErrorCode.Unknown;
        }
    }
}
