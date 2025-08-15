namespace UnityEngine.Purchasing
{
    /// <summary>
    /// The type of Native App store being used.
    /// </summary>
    public enum AppStore
    {
        /// <summary>
        /// GooglePlay Store.
        /// </summary>
        GooglePlay, //<= Map to AndroidStore. First Android store. In AppStoreMeta.

        /// <summary>
        /// Amazon App Store.
        /// </summary>
        AmazonAppStore, //

        /// <summary>
        /// MacOS App Store.
        /// </summary>
        MacAppStore,

        /// <summary>
        /// iOS or tvOS App Stores.
        /// </summary>
        AppleAppStore,

        /// <summary>
        /// Universal Windows Platform's store.
        /// </summary>
        WinRT,

        /// <summary>
        /// A fake store used for testing and Play-In-Editor.
        /// </summary>
        fake
    }
}
