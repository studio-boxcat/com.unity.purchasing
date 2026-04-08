using System;
using Stores.Util;
using UnityEngine.Purchasing.Extension;

namespace UnityEngine.Purchasing
{
    /// <summary>
    /// Internal store implementation passing store requests from the user through to the underlaying
    /// native store system, and back again. Binds a native store system binding to a callback.
    /// </summary>
    internal class JSONStore : IStore, IUnityCallback
    {
        protected IStoreCallback unity;
        private INativeStore m_Store;

        public virtual void SetNativeStore(INativeStore native)
        {
            m_Store = native;
        }

        public virtual void Initialize(IStoreCallback callback)
        {
            unity = callback;
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
            unity.OnProductsRetrieved(JsonProductDescriptionsDeserializer.DeserializeProductDescriptions(json));
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
            unity.OnPurchaseFailed(failure);
        }
    }
}
