#nullable enable

using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.Purchasing.Extension;
using UnityEngine.Purchasing.Models;
using UnityEngine.Scripting;

namespace UnityEngine.Purchasing
{
    /// <summary>
    /// This is C# representation of the Java Class ProductDetailsResponseListener
    /// <a href="https://developer.android.com/reference/com/android/billingclient/api/ProductDetailsResponseListener">See more</a>
    /// </summary>
    class ProductDetailsResponseListener : AndroidJavaProxy
    {
        const string k_AndroidProductDetailsResponseListenerClassName = "com.android.billingclient.api.ProductDetailsResponseListener";
        readonly Action<GoogleBillingResult, List<AndroidJavaObject>> m_OnProductDetailsResponse;

        internal ProductDetailsResponseListener(
            Action<GoogleBillingResult, List<AndroidJavaObject>> onProductDetailsResponseAction)
            : base(k_AndroidProductDetailsResponseListenerClassName)
        {
            m_OnProductDetailsResponse = onProductDetailsResponseAction;
        }

        // Billing 8 changed the second argument from List<ProductDetails> to QueryProductDetailsResult.
        // AndroidJavaProxy dispatches on name then arity then post-unboxing compatibility, and both
        // parameters are AndroidJavaObject, so a mismatch here still fires the proxy — it surfaces
        // only as a failure to enumerate. Hence no empty catch: swallowing it strands IAP
        // initialization forever with no callback, no log and no exception.
        [Preserve]
        public void onProductDetailsResponse(AndroidJavaObject billingResult, AndroidJavaObject? queryProductDetailsResult)
        {
            UnityUtil.RunOnMainThread(() =>
            {
                List<AndroidJavaObject>? productDetailsList = null;

                try
                {
                    using var javaProductDetailsList = queryProductDetailsResult?.Call<AndroidJavaObject>("getProductDetailsList");
                    productDetailsList = javaProductDetailsList.Enumerate<AndroidJavaObject>().ToList();
                    m_OnProductDetailsResponse(new GoogleBillingResult(billingResult), productDetailsList);
                }
                catch (Exception ex)
                {
                    // Logged, not rethrown: this runs inside UnityUtil's main-thread pump, which
                    // drops the rest of the batch — including the sibling inapp/subs callback — if
                    // an action throws.
                    UnityUtil.LogException(ex);
                }

#if UNITY_2021_2_OR_NEWER
                if (productDetailsList != null)
                {
                    foreach (var obj in productDetailsList)
                    {
                        obj?.Dispose();
                    }
                }
#endif

                billingResult.Dispose();
                queryProductDetailsResult?.Dispose();
            });
        }
    }
}
