#nullable enable
using System.Collections.Generic;
using UnityEngine.Purchasing.Extension;

namespace UnityEngine.Purchasing
{
    /// <summary>
    /// Wraps an IStoreCallback executing methods on
    /// the scripting thread.
    /// </summary>
    internal class ScriptingStoreCallback : IStoreCallback
    {
        readonly IStoreCallback m_ForwardTo;

        public ScriptingStoreCallback(IStoreCallback forwardTo)
        {
            m_ForwardTo = forwardTo;
        }

        public ProductCollection products => m_ForwardTo.products;

        public void OnSetupFailed(InitializationFailureReason reason, string? message)
        {
            UnityUtil.RunOnMainThread(() => m_ForwardTo.OnSetupFailed(reason, message));
        }

        public void OnProductsRetrieved(List<ProductDescription> products)
        {
            UnityUtil.RunOnMainThread(() => m_ForwardTo.OnProductsRetrieved(products));
        }

        public void OnPurchaseSucceeded(string id, string? receipt, string transactionID)
        {
            UnityUtil.RunOnMainThread(() => m_ForwardTo.OnPurchaseSucceeded(id, receipt, transactionID));
        }

        public void OnAllPurchasesRetrieved(List<Product> purchasedProducts)
        {
            UnityUtil.RunOnMainThread(() => m_ForwardTo.OnAllPurchasesRetrieved(purchasedProducts));
        }

        public void OnPurchaseFailed(PurchaseFailureDescription desc)
        {
            UnityUtil.RunOnMainThread(() => m_ForwardTo.OnPurchaseFailed(desc));
        }
    }
}
