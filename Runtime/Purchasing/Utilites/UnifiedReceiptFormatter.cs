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
                    AppStore.GooglePlay => StoreNames.GooglePlay,
                    AppStore.AmazonAppStore => StoreNames.AmazonApps,
                    AppStore.AppleAppStore => StoreNames.AppleAppStore,
                    AppStore.MacAppStore => StoreNames.MacAppStore,
                    AppStore.fake => StoreNames.Fake,
                    _ => throw new NotSupportedException("Unsupported store: " + appStore)
                };
            }
        }
    }
}
