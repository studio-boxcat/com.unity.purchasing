using UnityEngine.Purchasing.Extension;

namespace UnityEngine.Purchasing
{
    internal class FakeTransactionHistoryExtensions : ITransactionHistoryExtensions
    {
        public PurchaseFailureDescription GetLastPurchaseFailureDescription()
        {
            return null;
        }

        public StoreSpecificPurchaseErrorCode GetLastStoreSpecificPurchaseErrorCode()
        {
            return StoreSpecificPurchaseErrorCode.Unknown;
        }
    }
}
