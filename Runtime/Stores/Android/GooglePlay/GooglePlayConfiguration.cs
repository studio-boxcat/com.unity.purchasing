#nullable enable

using System;
using UnityEngine.Purchasing.Extension;
using UnityEngine.Purchasing.Models;

namespace UnityEngine.Purchasing
{
    /// <summary>
    /// Access Google Play store specific configurations.
    /// </summary>
    class GooglePlayConfiguration : IGooglePlayConfiguration, IGooglePlayConfigurationInternal
    {
        Action? m_InitializationConnectionLister;
        readonly GooglePlayStoreService m_GooglePlayStoreService;
        Action<Product>? m_DeferredPurchaseAction;
        Action<Product>? m_DeferredProrationUpgradeDowngradeSubscriptionAction;
        Action<int>? m_QueryProductDetailsFailedListener;

        bool m_FetchPurchasesAtInitialize = true;
        bool m_FetchPurchasesExcludeDeferred = true;

        public GooglePlayConfiguration(GooglePlayStoreService googlePlayStoreService)
        {
            m_GooglePlayStoreService = googlePlayStoreService;
        }

        /// <summary>
        /// Set an optional listener for failures when connecting to the base Google Play Billing service. This may be called
        /// after <typeparamref name="UnityPurchasing.Initialize"/> if a user does not have a Google account added to their
        /// Android device.
        /// </summary>
        /// <param name="action">Will be called when <typeparamref name="UnityPurchasing.Initialize"/>
        ///     is interrupted by a disconnection from the Google Play Billing service.</param>
        public void SetServiceDisconnectAtInitializeListener(Action action)
        {
            m_InitializationConnectionLister = action;
        }

        /// <summary>
        /// Internal API, do not use.
        /// </summary>
        public void NotifyInitializationConnectionFailed()
        {
            m_InitializationConnectionLister?.Invoke();
        }

        public void SetQueryProductDetailsFailedListener(Action<int> action)
        {
            m_QueryProductDetailsFailedListener = action;
        }

        public void NotifyQueryProductDetailsFailed(int retryCount)
        {
            m_QueryProductDetailsFailedListener?.Invoke(retryCount);
        }

        /// <summary>
        /// Set listener for deferred purchasing events.
        /// Deferred purchasing is enabled by default and cannot be changed.
        /// </summary>
        /// <param name="action">Deferred purchasing successful events. Do not grant the item here. Instead, record the purchase and remind the user to complete the transaction in the Play Store. </param>
        public void SetDeferredPurchaseListener(Action<Product> action)
        {
            m_DeferredPurchaseAction = action;
        }

        public void NotifyDeferredProrationUpgradeDowngradeSubscription(IStoreCallback? storeCallback, string productId)
        {
            var product = storeCallback.FindProductById(productId);
            if (product != null)
            {
                m_DeferredProrationUpgradeDowngradeSubscriptionAction?.Invoke(product);
            }
        }

        public bool IsFetchPurchasesAtInitializeSkipped()
        {
            return !m_FetchPurchasesAtInitialize;
        }

        public bool DoesRetrievePurchasesExcludeDeferred()
        {
            return m_FetchPurchasesExcludeDeferred;
        }

        public void NotifyDeferredPurchase(IStoreCallback? storeCallback, GooglePurchase? purchase, string? receipt, string? transactionId)
        {
            var product = storeCallback?.FindProductById(purchase?.sku);
            if (product != null)
            {
                ProductPurchaseUpdater.UpdateProductReceiptAndTransactionID(product, receipt, transactionId, GooglePlay.Name);
                m_DeferredPurchaseAction?.Invoke(product);
            }
        }

        /// <summary>
        /// Set listener for deferred subscription change events.
        /// Deferred subscription changes only take effect at the renewal cycle and no transaction is done immediately, therefore there is no receipt nor token.
        /// </summary>
        /// <param name="action">Deferred subscription change event. No payout is granted here. Instead, notify the user that the subscription change will take effect at the next renewal cycle. </param>
        public void SetDeferredProrationUpgradeDowngradeSubscriptionListener(Action<Product> action)
        {
            m_DeferredProrationUpgradeDowngradeSubscriptionAction = action;
        }

        /// <summary>
        /// Optional obfuscation string to detect irregular activities when making a purchase.
        /// For more information please visit <a href="https://developer.android.com/google/play/billing/security">https://developer.android.com/google/play/billing/security</a>
        /// </summary>
        /// <param name="accountId">The obfuscated account id</param>
        public void SetObfuscatedAccountId(string accountId)
        {
            m_GooglePlayStoreService.SetObfuscatedAccountId(accountId);
        }

        /// <summary>
        /// Optional obfuscation string to detect irregular activities when making a purchase
        /// For more information please visit <a href="https://developer.android.com/google/play/billing/security">https://developer.android.com/google/play/billing/security</a>
        /// </summary>
        /// <param name="profileId">The obfuscated profile id</param>
        public void SetObfuscatedProfileId(string? profileId)
        {
            m_GooglePlayStoreService.SetObfuscatedProfileId(profileId);
        }

        public void SetFetchPurchasesAtInitialize(bool enable)
        {
            m_FetchPurchasesAtInitialize = enable;
        }

        public void SetFetchPurchasesExcludeDeferred(bool exclude)
        {
            m_FetchPurchasesExcludeDeferred = exclude;
        }

        public void SetMaxConnectionAttempts(int maxConnectionAttempts)
        {
            m_GooglePlayStoreService.SetMaxConnectionAttempts(maxConnectionAttempts);
        }
    }
}
