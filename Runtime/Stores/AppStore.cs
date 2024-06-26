using System;

namespace UnityEngine.Purchasing
{
    /// <summary>
    /// The type of Native App store being used.
    /// </summary>
    public enum AppStore
    {
        /// <summary>
        /// No store specified.
        /// </summary>
        NotSpecified,

        /// <summary>
        /// GooglePlay Store.
        /// </summary>
        GooglePlay, //<= Map to AndroidStore. First Android store. In AppStoreMeta.

        /// <summary>
        /// Amazon App Store.
        /// </summary>
        AmazonAppStore, //

        [Obsolete("AppStore to be removed with UDP deprecation.")]
        /// <summary>
        /// Unity Distribution Portal, which supports a set of stores internally.
        /// Will become deprecated with UDP eventually.
        /// </summary>
        UDP, // Last Android store. Also in AppStoreMeta.

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
