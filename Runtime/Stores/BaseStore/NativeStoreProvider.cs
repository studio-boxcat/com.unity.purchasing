using System;
using UnityEngine.Purchasing.Extension;

namespace UnityEngine.Purchasing
{
    internal class NativeStoreProvider
    {
        public INativeStore GetAndroidStore(IUnityCallback callback, AppStore store, IPurchasingBinder binder, UnityUtil util)
        {
            INativeStore nativeStore;
            try
            {
                nativeStore = GetAndroidStoreHelper(callback, store, binder, util);
            }
            catch (Exception e)
            {
                throw new NotSupportedException("Failed to bind to native store: " + e.ToString());
            }

            if (nativeStore != null)
            {
                return nativeStore;
            }

            throw new NotImplementedException();
        }

        private INativeStore GetAndroidStoreHelper(IUnityCallback callback, AppStore store, IPurchasingBinder binder,
            UnityUtil util)
        {
            switch (store)
            {
                case AppStore.AmazonAppStore:
                    using (var pluginClass = new AndroidJavaClass("com.unity.purchasing.amazon.AmazonPurchasing"))
                    {
                        // Switch Android callbacks to the scripting thread, via ScriptingUnityCallback.
                        var proxy = new JavaBridge(new ScriptingUnityCallback(callback, util));
                        var instance = pluginClass.CallStatic<AndroidJavaObject>("instance", proxy);
                        // Hook up our amazon specific functionality.
                        var extensions = new AmazonAppStoreStoreExtensions(instance);
                        binder.RegisterExtension<IAmazonExtensions>(extensions);
                        binder.RegisterConfiguration<IAmazonConfiguration>(extensions);
                        return new AndroidJavaStore(instance);
                    }
            }

            throw new NotImplementedException();
        }

        public INativeAppleStore GetStorekit(IUnityCallback callback)
        {
            // Both tvOS, iOS and visionOS use the same Objective-C linked to the XCode project.
            if (Application.platform == RuntimePlatform.IPhonePlayer ||
                Application.platform == RuntimePlatform.tvOS
#if UNITY_VISIONOS
                || Application.platform == RuntimePlatform.VisionOS
#endif
               )
            {
                return new iOSStoreBindings();
            }
            return new OSXStoreBindings();
        }
    }
}
