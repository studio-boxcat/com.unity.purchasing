#nullable enable

using UnityEngine.Purchasing.Extension;
using UnityEngine.Purchasing.Models;

namespace UnityEngine.Purchasing
{
    interface IGooglePlayConfigurationInternal
    {
        void NotifyInitializationConnectionFailed();
        void NotifyDeferredPurchase(IStoreCallback? storeCallback, GooglePurchase purchase, string receipt, string transactionId);
        void NotifyDeferredProrationUpgradeDowngradeSubscription(IStoreCallback? storeCallback, string productId);
        bool IsFetchPurchasesAtInitializeSkipped();
        bool DoesRetrievePurchasesExcludeDeferred();
        void NotifyQueryProductDetailsFailed(int retryCount);
    }
}
