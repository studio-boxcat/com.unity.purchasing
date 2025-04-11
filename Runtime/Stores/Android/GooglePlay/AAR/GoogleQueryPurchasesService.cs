#nullable enable

using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine.Purchasing.Models;
using UnityEngine.Purchasing.Utils;

namespace UnityEngine.Purchasing
{
    class GoogleQueryPurchasesService
    {
        readonly GoogleBillingClient m_BillingClient;
        readonly GooglePurchaseBuilder m_PurchaseBuilder;

        internal GoogleQueryPurchasesService(GoogleBillingClient billingClient, GooglePurchaseBuilder purchaseBuilder)
        {
            m_BillingClient = billingClient;
            m_PurchaseBuilder = purchaseBuilder;
        }

        public async Task<List<GooglePurchase>> QueryPurchases()
        {
            var purchaseResults = await Task.WhenAll(QueryPurchasesWithSkuType(GoogleProductTypeEnum.Sub()), QueryPurchasesWithSkuType(GoogleProductTypeEnum.InApp()));
            return purchaseResults.SelectMany(result => result).ToList();
        }

        Task<IEnumerable<GooglePurchase>> QueryPurchasesWithSkuType(string skuType)
        {
            var taskCompletion = new TaskCompletionSource<IEnumerable<GooglePurchase>>();
            m_BillingClient.QueryPurchasesAsync(skuType,
                (billingResult, purchases) =>
                {
                    var result = IsResultOk(billingResult) ? m_PurchaseBuilder.BuildPurchases(purchases) : Enumerable.Empty<GooglePurchase>();
                    taskCompletion.TrySetResult(result);
                });

            return taskCompletion.Task;
        }

        public GooglePurchase? GetPurchaseByToken(string purchaseToken, string skuType)
        {
            var taskCompletion = new TaskCompletionSource<GooglePurchase?>();
            m_BillingClient.QueryPurchasesAsync(skuType,
                (billingResult, purchases) =>
                {
                    var purchase = purchases.FirstOrDefault(purchase => purchase != null && purchase.Call<string>("getPurchaseToken") == purchaseToken);
                    var result = purchase != null && IsResultOk(billingResult) ? m_PurchaseBuilder.BuildPurchase(purchase) : null;
                    taskCompletion.TrySetResult(result);
                });

            return taskCompletion.Task.Result;
        }

        static bool IsResultOk(GoogleBillingResult result)
        {
            return result.responseCode == GoogleBillingResponseCode.Ok;
        }
    }
}
