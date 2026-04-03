using System;

namespace UnityEngine.Purchasing
{
    internal static class UnifiedReceiptFormatter
    {
        internal static string FormatUnifiedReceipt(string platformReceipt, string transactionId, AppStore store)
        {
            var unifiedReceipt = new UnifiedReceipt()
            {
                Store = AppStoreToName(store),
                TransactionID = transactionId,
                Payload = platformReceipt
            };
            return JsonUtility.ToJson(unifiedReceipt);

            static string AppStoreToName(AppStore appStore)
            {
                return appStore switch
                {
                    AppStore.GooglePlay => GooglePlay.Name,
                    AppStore.AmazonAppStore => AmazonApps.Name,
                    AppStore.AppleAppStore => AppleAppStore.Name,
                    AppStore.MacAppStore => MacAppStore.Name,
                    AppStore.fake => FakeStore.Name,
                    _ => throw new NotSupportedException("Unsupported store: " + appStore)
                };
            }
        }
    }
}
