#nullable enable

using System;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine.Purchasing.Models;

namespace UnityEngine.Purchasing
{
    class GoogleFinishTransactionService
    {
        readonly GoogleBillingClient m_BillingClient;
        readonly GoogleQueryPurchasesService m_GoogleQueryPurchasesService;

        internal GoogleFinishTransactionService(GoogleBillingClient billingClient,
            GoogleQueryPurchasesService googleQueryPurchasesService)
        {
            m_BillingClient = billingClient;
            m_GoogleQueryPurchasesService = googleQueryPurchasesService;
        }

        public async void FinishTransaction(ProductDefinition? product, string purchaseToken,
            Action<GoogleBillingResult, GooglePurchase> onTransactionFinished)
        {
            try
            {
                var purchase = await FindPurchase(purchaseToken);
                if (purchase.IsPurchased())
                {
                    FinishTransactionForPurchase(purchase, product, purchaseToken, onTransactionFinished);
                }
            }
            catch (InvalidOperationException) { }
        }

        async Task<GooglePurchase> FindPurchase(string purchaseToken)
        {
            var purchases = await m_GoogleQueryPurchasesService.QueryPurchases();
            var purchaseToFinish =
                purchases.NonNull().First(purchase => purchase.purchaseToken == purchaseToken);

            return purchaseToFinish;
        }

        private void FinishTransactionForPurchase(GooglePurchase purchase, ProductDefinition? product,
            string purchaseToken,
            Action<GoogleBillingResult, GooglePurchase> onTransactionFinished)
        {
            if (product!.Value.type == ProductType.Consumable)
            {
                m_BillingClient.ConsumeAsync(purchaseToken, result => onTransactionFinished(result, purchase));
            }
            else if (!purchase.IsAcknowledged())
            {
                m_BillingClient.AcknowledgePurchase(purchaseToken, result => onTransactionFinished(result, purchase));
            }
        }
    }
}
