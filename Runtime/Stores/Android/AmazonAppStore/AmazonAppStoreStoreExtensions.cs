namespace UnityEngine.Purchasing
{
    /// <summary>
    /// Access Amazon store specific functionality.
    /// </summary>
    public class AmazonAppStoreStoreExtensions
    {
        private readonly AndroidJavaObject android;
        /// <summary>
        /// Build the AmazonAppStoreExtensions with the instance of the AmazonAppStore java object
        /// </summary>
        /// <param name="a">AmazonAppStore java object</param>
        public AmazonAppStoreStoreExtensions(AndroidJavaObject a)
        {
            android = a;
        }

        /// <summary>
        /// Amazon makes it possible to notify them of a product that cannot be fulfilled.
        ///
        /// This method calls Amazon's notifyFulfillment(transactionID, FulfillmentResult.UNAVAILABLE);
        /// https://developer.amazon.com/public/apis/earn/in-app-purchasing/docs-v2/implementing-iap-2.0
        /// </summary>
        /// <param name="transactionID">Products transaction id</param>
        public void NotifyUnableToFulfillUnavailableProduct(string transactionID)
        {
            android.Call("notifyUnableToFulfillUnavailableProduct", transactionID);
        }

        /// <summary>
        /// Gets the current Amazon user ID (for other Amazon services).
        /// </summary>
        public string amazonUserId => android.Call<string>("getAmazonUserId");
    }
}
