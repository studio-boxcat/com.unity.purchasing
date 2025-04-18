using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.Purchasing.Extension;
using UnityEngine.Purchasing.Models;
using UnityEngine.Purchasing.Utils;
using UnityEngine.Scripting;

namespace UnityEngine.Purchasing
{
    /// <summary>
    /// This is C# representation of the Java Class PurchasesUpdatedListener
    /// <a href="https://developer.android.com/reference/com/android/billingclient/api/PurchasesUpdatedListener">See more</a>
    /// </summary>
    class GooglePurchaseUpdatedListener : AndroidJavaProxy
    {
        const string k_AndroidPurchaseListenerClassName = "com.android.billingclient.api.PurchasesUpdatedListener";
        readonly GoogleLastKnownProductService m_LastKnownProductService;
        readonly GooglePlayPurchaseCallback m_GooglePurchaseCallback;
        readonly GooglePurchaseBuilder m_PurchaseBuilder;
        readonly GoogleCachedQueryProductDetailsService m_GoogleCachedQueryProductDetailsService;
        readonly GooglePurchaseStateEnumProvider m_GooglePurchaseStateEnumProvider;
        GoogleQueryPurchasesService m_GoogleQueryPurchasesService;

        internal GooglePurchaseUpdatedListener(GoogleLastKnownProductService googleLastKnownProductService,
            GooglePlayPurchaseCallback googlePurchaseCallback, GooglePurchaseBuilder purchaseBuilder,
            GoogleCachedQueryProductDetailsService googleCachedQueryProductDetailsService,
            GooglePurchaseStateEnumProvider googlePurchaseStateEnumProvider,
            GoogleQueryPurchasesService googleQueryPurchasesService = null)
            : base(k_AndroidPurchaseListenerClassName)
        {
            m_LastKnownProductService = googleLastKnownProductService;
            m_GooglePurchaseCallback = googlePurchaseCallback;
            m_GoogleCachedQueryProductDetailsService = googleCachedQueryProductDetailsService;
            m_GooglePurchaseStateEnumProvider = googlePurchaseStateEnumProvider;
            m_GoogleQueryPurchasesService = googleQueryPurchasesService;
            m_PurchaseBuilder = purchaseBuilder;
        }

        public void SetGoogleQueryPurchaseService(GoogleQueryPurchasesService googleFetchPurchases)
        {
            m_GoogleQueryPurchasesService = googleFetchPurchases;
        }

        /// <summary>
        /// Implementation of com.android.billingclient.api.PurchasesUpdatedListener#onPurchasesUpdated
        /// </summary>
        /// <param name="billingResult"></param>
        /// <param name="javaPurchasesList"></param>
        [Preserve]
        public void onPurchasesUpdated(AndroidJavaObject billingResult, AndroidJavaObject javaPurchasesList)
        {
            var purchaseList = javaPurchasesList.Enumerate().ToList();
            GoogleBillingResult result = new GoogleBillingResult(billingResult);
            var purchases = m_PurchaseBuilder.BuildPurchases(purchaseList).ToList();
            OnPurchasesUpdated(result, purchases);

            foreach (var obj in purchaseList)
            {
                obj?.Dispose();
            }

            billingResult.Dispose();
            javaPurchasesList?.Dispose();
        }

        internal void OnPurchasesUpdated(GoogleBillingResult result, List<GooglePurchase> purchases)
        {
            if (result.responseCode == GoogleBillingResponseCode.Ok)
            {
                HandleResultOkCases(result, purchases);
            }
            else if (result.responseCode == GoogleBillingResponseCode.UserCanceled && purchases.Any())
            {
                ApplyOnPurchases(purchases, OnPurchaseCancelled);
            }
            else if (result.responseCode == GoogleBillingResponseCode.ItemAlreadyOwned && purchases.Any())
            {
                ApplyOnPurchases(purchases, OnPurchaseAlreadyOwned);
            }
            else
            {
                HandleErrorCases(result, purchases);
            }
        }

        void HandleResultOkCases(GoogleBillingResult result, List<GooglePurchase> purchases)
        {
            if (purchases.Any())
            {
                ApplyOnPurchases(purchases, OnPurchaseOk);
            }
            else
            {
                HandleErrorCases(result, purchases);
            }
        }

        void HandleErrorCases(GoogleBillingResult billingResult, List<GooglePurchase> purchases)
        {
            if (!purchases.Any())
            {
                HandleErrorGoogleBillingResult(billingResult);
            }
            else
            {
                ApplyOnPurchases(purchases, billingResult, OnPurchaseFailed);
            }
        }

        void HandleErrorGoogleBillingResult(GoogleBillingResult billingResult)
        {
            switch (billingResult.responseCode)
            {
                case GoogleBillingResponseCode.ItemAlreadyOwned:
                    m_GooglePurchaseCallback.OnPurchaseFailed(
                        new PurchaseFailureDescription(
                            m_LastKnownProductService.LastKnownProductId,
                            PurchaseFailureReason.DuplicateTransaction,
                            billingResult.debugMessage + " - Google BillingResponseCode = " + billingResult.responseCode
                        )
                    );
                    break;
                case GoogleBillingResponseCode.BillingUnavailable:
                    m_GooglePurchaseCallback.OnPurchaseFailed(
                        new PurchaseFailureDescription(
                            m_LastKnownProductService.LastKnownProductId,
                            PurchaseFailureReason.PurchasingUnavailable,
                            billingResult.debugMessage + " - Google BillingResponseCode = " + billingResult.responseCode
                        )
                    );
                    break;
                case GoogleBillingResponseCode.UserCanceled:
                    HandleUserCancelledPurchaseFailure(billingResult);
                    break;
                default:
                    m_GooglePurchaseCallback.OnPurchaseFailed(
                        new PurchaseFailureDescription(
                            m_LastKnownProductService.LastKnownProductId,
                            PurchaseFailureReason.Unknown,
                            billingResult.debugMessage + " {M: GPUL.HEC} - Google BillingResponseCode = " + billingResult.responseCode
                        )
                    );
                    break;
            }
        }

        async void HandleUserCancelledPurchaseFailure(GoogleBillingResult billingResult)
        {
            var googlePurchases = await m_GoogleQueryPurchasesService.QueryPurchases();
            HandleUserCancelledPurchaseFailure(billingResult, googlePurchases);
        }

        void HandleUserCancelledPurchaseFailure(GoogleBillingResult billingResult,
            List<GooglePurchase> googlePurchases)
        {
            var googlePurchase = googlePurchases.FirstOrDefault(purchase =>
                purchase?.sku == m_LastKnownProductService.LastKnownProductId);

            if (googlePurchase != null && !googlePurchase.IsAcknowledged())
            {
                OnPurchaseOk(googlePurchase);
            }
            else
            {
                OnPurchaseCancelled(billingResult);
            }
        }

        void ApplyOnPurchases(List<GooglePurchase> purchases, Action<GooglePurchase> action)
        {
            foreach (var purchase in purchases)
            {
                action(purchase);
            }
        }

        void ApplyOnPurchases(IEnumerable<GooglePurchase> purchases, GoogleBillingResult billingResult, Action<GooglePurchase, string> action)
        {
            foreach (var purchase in purchases)
            {
                action(purchase, billingResult.debugMessage);
            }
        }

        void OnPurchaseOk(GooglePurchase googlePurchase)
        {
            if (googlePurchase.purchaseState == m_GooglePurchaseStateEnumProvider.Purchased())
            {
                HandlePurchasedProduct(googlePurchase);
            }
            else if (googlePurchase.purchaseState == m_GooglePurchaseStateEnumProvider.Pending())
            {
                m_GooglePurchaseCallback.NotifyDeferredPurchase(googlePurchase, googlePurchase.receipt, googlePurchase.purchaseToken);
            }
            else
            {
                m_GooglePurchaseCallback.OnPurchaseFailed(
                    new PurchaseFailureDescription(
                        googlePurchase.purchaseToken,
                        PurchaseFailureReason.Unknown,
                        GoogleBillingStrings.errorPurchaseStateUnspecified + " {M: GPUL.OPO}"
                    )
                );
            }
        }

        void HandlePurchasedProduct(GooglePurchase googlePurchase)
        {
            if (IsDeferredSubscriptionChange(googlePurchase))
            {
                m_GooglePurchaseCallback.NotifyDeferredProrationUpgradeDowngradeSubscription(m_LastKnownProductService.LastKnownProductId);
            }
            else
            {
                m_GooglePurchaseCallback.OnPurchaseSuccessful(googlePurchase, googlePurchase.receipt, googlePurchase.purchaseToken);
            }
        }

        bool IsDeferredSubscriptionChange(GooglePurchase googlePurchase)
        {
            return IsLastProrationModeDeferred() &&
                   googlePurchase.sku == m_LastKnownProductService.LastKnownOldProductId;
        }

        bool IsLastProrationModeDeferred()
        {
            return m_LastKnownProductService.LastKnownProrationMode == GooglePlayProrationMode.Deferred;
        }

        void OnPurchaseCancelled(GoogleBillingResult billingResult)
        {
            m_GooglePurchaseCallback.OnPurchaseFailed(
                new PurchaseFailureDescription(
                    m_LastKnownProductService.LastKnownProductId,
                    PurchaseFailureReason.UserCancelled,
                    billingResult.debugMessage
                )
            );
        }

        void OnPurchaseCancelled(GooglePurchase googlePurchase)
        {
            m_GooglePurchaseCallback.OnPurchaseFailed(
                new PurchaseFailureDescription(
                    googlePurchase.purchaseToken,
                    PurchaseFailureReason.UserCancelled,
                    GoogleBillingStrings.errorUserCancelled
                )
            );
        }

        void OnPurchaseAlreadyOwned(GooglePurchase googlePurchase)
        {
            m_GooglePurchaseCallback.OnPurchaseFailed(
                new PurchaseFailureDescription(
                    googlePurchase.purchaseToken,
                    PurchaseFailureReason.DuplicateTransaction,
                    GoogleBillingStrings.errorItemAlreadyOwned
                )
            );
        }

        void OnPurchaseFailed(GooglePurchase googlePurchase, string debugMessage)
        {
            m_GooglePurchaseCallback.OnPurchaseFailed(
                new PurchaseFailureDescription(
                    googlePurchase.purchaseToken,
                    PurchaseFailureReason.Unknown,
                    debugMessage + " {M: GPUL.OPF}"
                )
            );
        }
    }
}
