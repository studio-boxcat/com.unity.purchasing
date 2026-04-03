using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.Purchasing.Models;

namespace UnityEngine.Purchasing.Utils
{
    class GooglePurchaseBuilder
    {
        readonly GoogleCachedQueryProductDetailsService m_CachedQueryProductDetailsService;

        public GooglePurchaseBuilder(GoogleCachedQueryProductDetailsService cachedQueryProductDetailsService)
        {
            m_CachedQueryProductDetailsService = cachedQueryProductDetailsService;
        }

        public IEnumerable<GooglePurchase> BuildPurchases(IEnumerable<AndroidJavaObject> purchases)
        {
            return purchases.Select(BuildPurchase)
                .IgnoreExceptions<GooglePurchase, ArgumentException>(LogWarningForException).ToList();
        }

        static void LogWarningForException(Exception exception)
        {
            UnityUtil.LogWarning(exception.Message);
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
