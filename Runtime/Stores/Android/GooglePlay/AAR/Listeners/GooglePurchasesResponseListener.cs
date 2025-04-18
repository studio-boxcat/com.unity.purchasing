using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.Purchasing.Models;
using UnityEngine.Scripting;

namespace UnityEngine.Purchasing
{
    /// <summary>
    /// This is C# representation of the Java Class PurchasesResponseListener
    /// <a href="https://developer.android.com/reference/com/android/billingclient/api/PurchasesResponseListener">See more</a>
    /// </summary>
    class GooglePurchasesResponseListener : AndroidJavaProxy
    {
        const string k_AndroidPurchasesResponseListenerClassName =
            "com.android.billingclient.api.PurchasesResponseListener";
        readonly Action<GoogleBillingResult, IEnumerable<AndroidJavaObject>> m_OnQueryPurchasesResponse;

        internal GooglePurchasesResponseListener(
            Action<GoogleBillingResult, IEnumerable<AndroidJavaObject>> onQueryPurchasesResponse)
            : base(k_AndroidPurchasesResponseListenerClassName)
        {
            m_OnQueryPurchasesResponse = onQueryPurchasesResponse;
        }

        [Preserve]
        public void onQueryPurchasesResponse(AndroidJavaObject billingResult, AndroidJavaObject purchases)
        {
            var purchasesList = purchases.Enumerate().ToList();
            m_OnQueryPurchasesResponse(new GoogleBillingResult(billingResult), purchasesList);

            foreach (var obj in purchasesList)
            {
                obj?.Dispose();
            }

            billingResult.Dispose();
            purchases?.Dispose();
        }
    }
}
