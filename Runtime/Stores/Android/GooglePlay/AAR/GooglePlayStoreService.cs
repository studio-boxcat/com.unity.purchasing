#nullable enable

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine.Purchasing.Extension;
using UnityEngine.Purchasing.Models;
using UnityEngine.Purchasing.Stores.Util;

namespace UnityEngine.Purchasing
{
    class GooglePlayStoreService
    {
        int m_CurrentConnectionAttempts;
        int m_MaxConnectionAttempts = 3;
        readonly GoogleBillingClient m_BillingClient;
        readonly BillingClientStateListener m_BillingClientStateListener;
        readonly QueryProductDetailsService m_QueryProductDetailsService;
        readonly ConcurrentQueue<ProductDescriptionQuery> m_ProductsToQuery = new ConcurrentQueue<ProductDescriptionQuery>();
        readonly ConcurrentQueue<Action<List<GooglePurchase>>> m_OnPurchaseSucceededQueue = new ConcurrentQueue<Action<List<GooglePurchase>>>();
        readonly GooglePurchaseService m_GooglePurchaseService;
        readonly GoogleFinishTransactionService m_GoogleFinishTransactionService;
        readonly GoogleQueryPurchasesService m_GoogleQueryPurchasesService;
        readonly GoogleLastKnownProductService m_GoogleLastKnownProductService;
        readonly ILogger m_Logger;
        readonly IRetryPolicy m_RetryPolicy;
        readonly UnityUtil m_Util;

        internal GooglePlayStoreService(
            GoogleBillingClient billingClient,
            QueryProductDetailsService queryProductDetailsService,
            GooglePurchaseService purchaseService,
            GoogleFinishTransactionService finishTransactionService,
            GoogleQueryPurchasesService queryPurchasesService,
            BillingClientStateListener billingClientStateListener,
            GoogleLastKnownProductService lastKnownProductService,
            ILogger logger,
            IRetryPolicy retryPolicy,
            UnityUtil util)
        {
            m_BillingClient = billingClient;
            m_QueryProductDetailsService = queryProductDetailsService;
            m_GooglePurchaseService = purchaseService;
            m_GoogleFinishTransactionService = finishTransactionService;
            m_GoogleQueryPurchasesService = queryPurchasesService;
            m_GoogleLastKnownProductService = lastKnownProductService;
            m_BillingClientStateListener = billingClientStateListener;
            m_Logger = logger;
            m_RetryPolicy = retryPolicy;
            m_Util = util;
        }

        internal void InitConnectionWithGooglePlay()
        {
            m_BillingClientStateListener.RegisterOnConnected(OnConnected);
            m_BillingClientStateListener.RegisterOnDisconnected(OnDisconnected);

            StartConnection();
        }

        void StartConnection()
        {
            m_CurrentConnectionAttempts++;
            m_BillingClient.StartConnection(m_BillingClientStateListener);
        }

        public void ResumeConnection()
        {
            if (m_BillingClient.GetConnectionState() == GoogleBillingConnectionState.Disconnected)
            {
                AttemptReconnection();
            }
        }

        void AttemptReconnection()
        {
            if (!AreConnectionAttemptsExhausted())
            {
                m_RetryPolicy.Invoke(retryAction => RetryConnection(retryAction));
            }
        }

        bool AreConnectionAttemptsExhausted()
        {
            return m_CurrentConnectionAttempts >= m_MaxConnectionAttempts;
        }

        void RetryConnection(Action ActionToRetry)
        {
            m_Util.RunOnMainThread(() => RetryConnectionAttempt(ActionToRetry));
        }

        void RetryConnectionAttempt(Action ActionToRetry)
        {
            if (!AreConnectionAttemptsExhausted() && m_BillingClient.GetConnectionState() == GoogleBillingConnectionState.Disconnected)
            {
                StartConnection();
                ActionToRetry();
            }
        }

        public bool IsConnectionReady()
        {
            return m_BillingClient.IsReady();
        }

        void OnConnected()
        {
            m_CurrentConnectionAttempts = 0;

            DequeueQueryProducts(GoogleBillingResponseCode.Ok);
            DequeueFetchPurchases();
        }

        protected virtual void DequeueQueryProducts(GoogleBillingResponseCode googleBillingResponseCode)
        {
            var productsFailedToDequeue = new ConcurrentQueue<ProductDescriptionQuery>();
            var stop = false;

            while (m_ProductsToQuery.Count > 0 && !stop)
            {
                var currentConnectionState = m_BillingClient.GetConnectionState();
                switch (currentConnectionState)
                {
                    case GoogleBillingConnectionState.Connected:
                    {
                        if (m_ProductsToQuery.TryDequeue(out var productDescriptionQuery) &&
                            productDescriptionQuery != null)
                        {
                            m_QueryProductDetailsService.QueryAsyncProduct(productDescriptionQuery.products,
                                productDescriptionQuery.onProductsReceived);
                        }

                        break;
                    }
                    case GoogleBillingConnectionState.Disconnected:
                    {
                        if (m_ProductsToQuery.TryDequeue(out var productDescriptionQuery) &&
                            productDescriptionQuery != null)
                        {
                            var reason = AreConnectionAttemptsExhausted() ? GoogleRetrieveProductsFailureReason.BillingServiceUnavailable : GoogleRetrieveProductsFailureReason.BillingServiceDisconnected;
                            productDescriptionQuery.onRetrieveProductsFailed(reason, googleBillingResponseCode);

                            productsFailedToDequeue.Enqueue(productDescriptionQuery);
                        }

                        break;
                    }
                    case GoogleBillingConnectionState.Connecting:
                    {
                        stop = true;
                        break;
                    }
                    default:
                    {
                        m_Logger.LogIAPError($"GooglePlayStoreService state ({currentConnectionState}) unrecognized, cannot process ProductDescriptionQuery");
                        stop = true;
                        break;
                    }
                }
            }

            foreach (var product in productsFailedToDequeue)
            {
                m_ProductsToQuery.Enqueue(product);
            }
        }

        protected virtual void DequeueFetchPurchases()
        {
            var purchasesFailedToDequeue = new ConcurrentQueue<Action<List<GooglePurchase>>>();

            while (m_OnPurchaseSucceededQueue.TryDequeue(out var onPurchaseSucceed))
            {
                purchasesFailedToDequeue.Enqueue(onPurchaseSucceed);
            }

            while (purchasesFailedToDequeue.TryDequeue(out var onPurchaseSucceed))
            {
                FetchPurchases(onPurchaseSucceed);
            }
        }

        void OnDisconnected(GoogleBillingResponseCode googleBillingResponseCode)
        {
            DequeueQueryProducts(googleBillingResponseCode);
            AttemptReconnection();
        }

        public virtual void RetrieveProducts(ProductDefinition[] products, Action<List<ProductDescription>, GoogleBillingResult> onProductsReceived, Action<GoogleRetrieveProductsFailureReason, GoogleBillingResponseCode> onRetrieveProductsFailed)
        {
            var currentConnectionState = m_BillingClient.GetConnectionState();
            if (currentConnectionState == GoogleBillingConnectionState.Connected)
            {
                m_QueryProductDetailsService.QueryAsyncProduct(products, onProductsReceived);
            }
            else
            {
                HandleRetrieveProductsNotConnected(products, onProductsReceived, onRetrieveProductsFailed);
            }
        }

        void HandleRetrieveProductsNotConnected(ProductDefinition[] products, Action<List<ProductDescription>, GoogleBillingResult> onProductsReceived, Action<GoogleRetrieveProductsFailureReason, GoogleBillingResponseCode> onRetrieveProductsFailed)
        {
            if (m_BillingClient.GetConnectionState() == GoogleBillingConnectionState.Disconnected)
            {
                if (AreConnectionAttemptsExhausted())
                {
                    onRetrieveProductsFailed(GoogleRetrieveProductsFailureReason.BillingServiceUnavailable, GoogleBillingResponseCode.ServiceUnavailable);
                }
                else
                {
                    onRetrieveProductsFailed(GoogleRetrieveProductsFailureReason.BillingServiceDisconnected, GoogleBillingResponseCode.ServiceDisconnected);
                }

            }

            m_ProductsToQuery.Enqueue(new ProductDescriptionQuery(products, onProductsReceived, onRetrieveProductsFailed));
        }

        public void Purchase(ProductDefinition product)
        {
            Purchase(product, null, null);
        }

        public virtual void Purchase(ProductDefinition product, Product? oldProduct, GooglePlayProrationMode? desiredProrationMode)
        {
            m_GoogleLastKnownProductService.LastKnownOldProductId = oldProduct?.definition.storeSpecificId;
            m_GoogleLastKnownProductService.LastKnownProductId = product.storeSpecificId;
            m_GoogleLastKnownProductService.LastKnownProrationMode = desiredProrationMode;
            m_GooglePurchaseService.Purchase(product, oldProduct, desiredProrationMode);
        }

        public void FinishTransaction(ProductDefinition? product, string purchaseToken, Action<GoogleBillingResult, GooglePurchase> onTransactionFinished)
        {
            m_GoogleFinishTransactionService.FinishTransaction(product, purchaseToken, onTransactionFinished);
        }

        public async void FetchPurchases(Action<List<GooglePurchase>> onQueryPurchaseSucceed)
        {
            try
            {
                await TryFetchPurchases(onQueryPurchaseSucceed);
            }
            catch (Exception ex)
            {
            }
        }

        async Task TryFetchPurchases(Action<List<GooglePurchase>> onQueryPurchaseSucceed)
        {
            if (onQueryPurchaseSucceed == null)
            {
                m_Logger.LogIAPWarning("FetchPurchases called with null callback onQueryPurchaseSucceed");
                return;
            }

            if (m_BillingClient.GetConnectionState() == GoogleBillingConnectionState.Connected)
            {
                var purchases = await m_GoogleQueryPurchasesService.QueryPurchases();
                onQueryPurchaseSucceed(purchases);
            }
            else
            {
                m_OnPurchaseSucceededQueue.Enqueue(onQueryPurchaseSucceed);
            }
        }

        public GooglePurchase? GetPurchase(string purchaseToken, string skuType)
        {
            return m_GoogleQueryPurchasesService.GetPurchaseByToken(purchaseToken, skuType);
        }

        public void SetMaxConnectionAttempts(int maxConnectionAttempts)
        {
            m_MaxConnectionAttempts = maxConnectionAttempts;
        }

        public void SetObfuscatedAccountId(string obfuscatedAccountId)
        {
            m_BillingClient.SetObfuscationAccountId(obfuscatedAccountId);
        }

        public void SetObfuscatedProfileId(string obfuscatedProfileId)
        {
            m_BillingClient.SetObfuscationProfileId(obfuscatedProfileId);
        }
    }
}
