namespace UnityEngine.Purchasing.Models
{
    /// <summary>
    /// This is C# representation of the Java Class ProductType
    /// <a href="https://developer.android.com/reference/com/android/billingclient/api/BillingClient.ProductType">See more</a>
    /// </summary>
    static class GoogleProductTypeEnum
    {
        internal static string InApp()
        {
            return "inapp";
        }

        internal static string Sub()
        {
            return "subs";
        }
    }
}
