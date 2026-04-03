using UnityEngine.Purchasing.Extension;

namespace UnityEngine.Purchasing
{
    /// <summary>
    /// Wraps an IUnityCallback executing methods on
    /// the scripting thread.
    /// </summary>
    internal class ScriptingUnityCallback : IUnityCallback
    {
        private readonly IUnityCallback forwardTo;

        public ScriptingUnityCallback(IUnityCallback forwardTo)
        {
            this.forwardTo = forwardTo;
        }

        public void OnSetupFailed(string json)
        {
            UnityUtil.RunOnMainThread(() => forwardTo.OnSetupFailed(json));
        }

        public void OnProductsRetrieved(string json)
        {
            UnityUtil.RunOnMainThread(() => forwardTo.OnProductsRetrieved(json));
        }

        public void OnPurchaseSucceeded(string id, string receipt, string transactionID)
        {
            UnityUtil.RunOnMainThread(() => forwardTo.OnPurchaseSucceeded(id, receipt, transactionID));
        }

        public void OnPurchaseFailed(string json)
        {
            UnityUtil.RunOnMainThread(() => forwardTo.OnPurchaseFailed(json));
        }
    }
}
