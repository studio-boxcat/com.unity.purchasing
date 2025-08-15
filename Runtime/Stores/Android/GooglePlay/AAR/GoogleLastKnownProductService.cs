#nullable enable

namespace UnityEngine.Purchasing
{
    class GoogleLastKnownProductService
    {
        public string? LastKnownOldProductId { get; set; }
        public string? LastKnownProductId { get; set; }

        public GooglePlayReplacementMode? LastKnownReplacementMode { get; set; } =
            GooglePlayReplacementMode.UnknownReplacementMode;
    }
}
