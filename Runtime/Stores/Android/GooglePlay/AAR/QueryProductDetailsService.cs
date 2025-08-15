#nullable enable

using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.Purchasing.Extension;
using UnityEngine.Purchasing.Models;
using UnityEngine.Purchasing.Stores.Util;
using UnityEngine.Purchasing.Utils;

namespace UnityEngine.Purchasing
{
    class QueryProductDetailsService 
    {
        readonly GoogleBillingClient m_BillingClient;
        readonly GoogleCachedQueryProductDetailsService m_GoogleCachedQueryProductDetailsService;
        readonly ProductDetailsConverter m_ProductDetailsConverter;
        readonly IRetryPolicy m_RetryPolicy;
        readonly GooglePlayProductCallback m_GoogleProductCallback;

        internal QueryProductDetailsService(GoogleBillingClient billingClient, GoogleCachedQueryProductDetailsService googleCachedQueryProductDetailsService,
            ProductDetailsConverter productDetailsConverter, IRetryPolicy retryPolicy, GooglePlayProductCallback googleProductCallback)
        {
            m_BillingClient = billingClient;
            m_GoogleCachedQueryProductDetailsService = googleCachedQueryProductDetailsService;
            m_ProductDetailsConverter = productDetailsConverter;
            m_RetryPolicy = retryPolicy;
            m_GoogleProductCallback = googleProductCallback;
        }


        public void QueryAsyncProduct(ProductDefinition product, Action<List<AndroidJavaObject>, GoogleBillingResult> onProductDetailsResponse)
        {
            QueryAsyncProduct(new []
            {
                product
            }, onProductDetailsResponse);
        }

        public void QueryAsyncProduct(ProductDefinition[] products, Action<List<ProductDescription>, GoogleBillingResult> onProductDetailsResponse)
        {
            QueryAsyncProduct(products,
                (productDetails, responseCode) => onProductDetailsResponse(m_ProductDetailsConverter.ConvertOnQueryProductDetailsResponse(productDetails), responseCode));
        }

        public void QueryAsyncProduct(ProductDefinition[] products, Action<List<AndroidJavaObject>, GoogleBillingResult> onProductDetailsResponse)
        {
            var retryCount = 0;

            m_RetryPolicy.Invoke(retryAction => QueryAsyncProductWithRetries(products, onProductDetailsResponse, retryAction), OnActionRetry);

            void OnActionRetry()
            {
                m_GoogleProductCallback.NotifyQueryProductDetailsFailed(++retryCount);
            }
        }

        void QueryAsyncProductWithRetries(ProductDefinition[] products, Action<List<AndroidJavaObject>, GoogleBillingResult> onProductDetailsResponse, Action retryQuery)
        {
            try
            {
                TryQueryAsyncProductWithRetries(products, onProductDetailsResponse, retryQuery);
            }
            catch (Exception ex)
            {
                Debug.LogError($"Unity IAP - QueryAsyncProductWithRetries: {ex}");
            }
        }

        void TryQueryAsyncProductWithRetries(ProductDefinition[] products, Action<List<AndroidJavaObject>, GoogleBillingResult> onProductDetailsResponse, Action retryQuery)
        {
            var consolidator = new ProductDetailsResponseConsolidator(productDetailsQueryResponse =>
            {
                m_GoogleCachedQueryProductDetailsService.AddCachedQueriedProductDetails(productDetailsQueryResponse.ProductDetails());
                if (ShouldRetryQuery(products, productDetailsQueryResponse))
                {
                    retryQuery();
                }
                else
                {
                    onProductDetailsResponse(GetCachedProductDetails(products).ToList(), productDetailsQueryResponse.GetGoogleBillingResult());
                }
            });
            QueryInAppsAsync(products, consolidator);
            QuerySubsAsync(products, consolidator);
        }

        bool ShouldRetryQuery(IEnumerable<ProductDefinition> requestedProducts, ProductDetailsQueryResponse queryResponse)
        {
            return !AreAllProductDetailsCached(requestedProducts) && queryResponse.IsRecoverable();
        }

        bool AreAllProductDetailsCached(IEnumerable<ProductDefinition> products)
        {
            return products.Select(m_GoogleCachedQueryProductDetailsService.Contains).All(isCached => isCached);
        }

        IEnumerable<AndroidJavaObject> GetCachedProductDetails(IEnumerable<ProductDefinition> products)
        {
            var cachedProducts = products.Where(m_GoogleCachedQueryProductDetailsService.Contains).ToList();
            return m_GoogleCachedQueryProductDetailsService.GetCachedQueriedProductDetails(cachedProducts);
        }

        void QueryInAppsAsync(IEnumerable<ProductDefinition> products, ProductDetailsResponseConsolidator consolidator)
        {
            var productList = products
                .Where(product => product.type != ProductType.Subscription)
                .Select(product => product.storeSpecificId)
                .ToList();
            QueryProductDetails(productList, GoogleProductTypeEnum.InApp(), consolidator);
        }

        void QuerySubsAsync(IEnumerable<ProductDefinition> products, ProductDetailsResponseConsolidator consolidator)
        {
            var productList = products
                .Where(product => product.type == ProductType.Subscription)
                .Select(product => product.storeSpecificId)
                .ToList();
            QueryProductDetails(productList, GoogleProductTypeEnum.Sub(), consolidator);
        }

        void QueryProductDetails(List<string> productList, string type, ProductDetailsResponseConsolidator consolidator)
        {
            if (productList.Count == 0)
            {
                consolidator.Consolidate(new GoogleBillingResult(null), new List<AndroidJavaObject>());
                return;
            }

            m_BillingClient.QueryProductDetailsAsync(productList, type, consolidator.Consolidate);
        }
    }
}
