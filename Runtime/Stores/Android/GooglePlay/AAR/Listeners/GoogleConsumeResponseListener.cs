using System;
using UnityEngine.Purchasing.Models;
using UnityEngine.Scripting;

namespace UnityEngine.Purchasing
{
    /// <summary>
    /// This is C# representation of the Java Class ConsumeResponseListener
    /// <a href="https://developer.android.com/reference/com/android/billingclient/api/ConsumeResponseListener">See more</a>
    /// </summary>
    class GoogleConsumeResponseListener : AndroidJavaProxy
    {
        const string k_AndroidConsumeResponseListenerClassName = "com.android.billingclient.api.ConsumeResponseListener";
        readonly Action<GoogleBillingResult> m_OnConsumeResponse;

        internal GoogleConsumeResponseListener(Action<GoogleBillingResult> onConsumeResponseAction)
            : base(k_AndroidConsumeResponseListenerClassName)
        {
            m_OnConsumeResponse = onConsumeResponseAction;
        }

        [Preserve]
        void onConsumeResponse(AndroidJavaObject billingResult, string purchaseToken)
        {
            m_OnConsumeResponse(new GoogleBillingResult(billingResult));

            billingResult.Dispose();
        }
    }
}
