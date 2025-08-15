using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.Purchasing.Models;

namespace UnityEngine.Purchasing.Utils
{
    class GooglePurchaseBuilder
    {
        readonly GoogleCachedQueryProductDetailsService m_CachedQueryProductDetailsService;
        readonly ILogger m_Logger;

        public GooglePurchaseBuilder(GoogleCachedQueryProductDetailsService cachedQueryProductDetailsService, ILogger logger)
        {
            m_CachedQueryProductDetailsService = cachedQueryProductDetailsService;
            m_Logger = logger;
        }

        public IEnumerable<GooglePurchase> BuildPurchases(IEnumerable<AndroidJavaObject> purchases)
        {
            return purchases.Select(BuildPurchase)
                .IgnoreExceptions<GooglePurchase, ArgumentException>(LogWarningForException).ToList();
        }

        void LogWarningForException(Exception exception)
        {
            m_Logger.LogIAPWarning(exception.Message);
        }

        public GooglePurchase BuildPurchase(AndroidJavaObject purchase)
        {
            var cachedProductDetails = m_CachedQueryProductDetailsService.GetCachedQueriedProducts();
            using var getProductsObj = purchase.Call<AndroidJavaObject>("getProducts");
            var purchaseSkus = getProductsObj.Enumerate<string>();

            try
            {
                var productDetailsEnum = TryFindAllProductDetails(purchaseSkus, cachedProductDetails);
                return new GooglePurchase(purchase, productDetailsEnum);
            }
            catch (InvalidOperationException)
            {
                var orderId = purchase.Call<string>("getOrderId");
                var purchaseToken = purchase.Call<string>("getPurchaseToken");
                throw new ArgumentException($"Unable to process purchase with order id: {orderId} and purchase token: {purchaseToken} because the product details associated with the purchased products were not found.");
            }
        }

        static IEnumerable<AndroidJavaObject> TryFindAllProductDetails(IEnumerable<string> skus, IEnumerable<AndroidJavaObject> productDetails)
        {
            return skus.Select(sku => productDetails.First(
                productDetail => sku == productDetail.Call<string>("getProductId")));
        }
    }
}
