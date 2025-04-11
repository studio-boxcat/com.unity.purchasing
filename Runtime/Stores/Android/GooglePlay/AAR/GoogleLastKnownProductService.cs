#nullable enable

namespace UnityEngine.Purchasing
{
    class GoogleLastKnownProductService
    {
        public string? LastKnownOldProductId { get; set; }
        public string? LastKnownProductId { get; set; }

        public GooglePlayProrationMode? LastKnownProrationMode { get; set; } =
            GooglePlayProrationMode.UnknownSubscriptionUpgradeDowngradePolicy;
    }
}
