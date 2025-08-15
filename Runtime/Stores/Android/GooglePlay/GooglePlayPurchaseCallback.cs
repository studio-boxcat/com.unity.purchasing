#nullable enable

using UnityEngine.Purchasing.Extension;
using UnityEngine.Purchasing.Models;

namespace UnityEngine.Purchasing
{
    class GooglePlayPurchaseCallback
    {
        IStoreCallback? m_StoreCallback;
        IGooglePlayConfigurationInternal? m_GooglePlayConfigurationInternal;
        readonly UnityUtil m_Util;

        public GooglePlayPurchaseCallback(UnityUtil util)
        {
            m_Util = util;
        }

        public void SetStoreCallback(IStoreCallback storeCallback)
        {
            m_StoreCallback = storeCallback;
        }

        public void SetStoreConfiguration(IGooglePlayConfigurationInternal configuration)
        {
            m_GooglePlayConfigurationInternal = configuration;
        }

        public void OnPurchaseSuccessful(GooglePurchase purchase, string receipt, string purchaseToken)
        {
            m_StoreCallback?.OnPurchaseSucceeded(purchase.sku ?? string.Empty, receipt, purchaseToken);
        }

        public void OnPurchaseFailed(PurchaseFailureDescription purchaseFailureDescription)
        {
            m_StoreCallback?.OnPurchaseFailed(purchaseFailureDescription);
        }

        public void NotifyDeferredPurchase(GooglePurchase purchase, string receipt, string purchaseToken)
        {
            m_Util.RunOnMainThread(() =>
                m_GooglePlayConfigurationInternal?.NotifyDeferredPurchase(m_StoreCallback, purchase, receipt,
                    purchaseToken));

        }

        public void NotifyDeferredProrationUpgradeDowngradeSubscription(string sku)
        {
            m_Util.RunOnMainThread(() =>
                m_GooglePlayConfigurationInternal?.NotifyDeferredProrationUpgradeDowngradeSubscription(m_StoreCallback,
                    sku));
        }
    }
}
