namespace UnityEngine.Purchasing
{
    /// <summary>
    /// The Google Play replacement mode used when upgrading and downgrading subscription.
    /// <a href="https://developer.android.com/reference/com/android/billingclient/api/BillingFlowParams.SubscriptionUpdateParams.ReplacementMode"> See more </a>
    /// </summary>
    public enum GooglePlayReplacementMode
    {
        /// <summary>
        /// Unknown replacement mode.
        /// </summary>
        /// <a href="https://developer.android.com/reference/com/android/billingclient/api/BillingFlowParams.SubscriptionUpdateParams.ReplacementMode#UNKNOWN_REPLACEMENT_MODE()"> See more </a>
        UnknownReplacementMode = 0,

        /// <summary>
        /// Replacement takes effect immediately, and the remaining time will be prorated and credited to the user. This is the current default behavior.
        /// <a href="https://developer.android.com/reference/com/android/billingclient/api/BillingFlowParams.SubscriptionUpdateParams.ReplacementMode#WITH_TIME_PRORATION()"> See more </a>
        /// </summary>
        WithTimeProration = 1,

        /// <summary>
        /// Replacement takes effect immediately, and the billing cycle remains the same. The price for the remaining period will be charged. This option is only available for subscription upgrade.
        /// <a href="https://developer.android.com/reference/com/android/billingclient/api/BillingFlowParams.SubscriptionUpdateParams.ReplacementMode#CHARGE_PRORATED_PRICE()"> See more </a>
        /// </summary>
        ChargeProratedPrice = 2,

        /// <summary>
        /// Replacement takes effect immediately, and the new price will be charged on next recurrence time. The billing cycle stays the same.
        /// <a href="https://developer.android.com/reference/com/android/billingclient/api/BillingFlowParams.SubscriptionUpdateParams.ReplacementMode#WITHOUT_PRORATION()"> See more </a>
        /// </summary>
        WithoutProration = 3,

        /// <summary>
        /// Replacement takes effect immediately, and the user is charged full price of new plan and is given a full billing cycle of subscription, plus remaining prorated time from the old plan.
        /// <a href="https://developer.android.com/reference/com/android/billingclient/api/BillingFlowParams.SubscriptionUpdateParams.ReplacementMode#CHARGE_FULL_PRICE()"> See more </a>
        /// </summary>
        ChargeFullPrice = 5,

        /// <summary>
        /// Replacement takes effect when the old plan expires, and the new price will be charged at the same time.
        /// <a href="https://developer.android.com/reference/com/android/billingclient/api/BillingFlowParams.SubscriptionUpdateParams.ReplacementMode#DEFERRED()"> See more </a>
        /// </summary>
        Deferred = 4,
    }
}
