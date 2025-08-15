using System.Collections.Generic;
using UnityEngine.Purchasing;

namespace UnityEditor.Purchasing
{
    /// <summary>
    /// Editor tools to set build-time configurations for app stores.
    /// </summary>
    public static class UnityPurchasingEditor
    {
        const string BinPath = "Packages/com.unity.purchasing/Plugins/UnityPurchasing/Android";

        // Notice: Multiple files per target supported. While Key must be unique, Value can be duplicated!
        static readonly Dictionary<string, AppStore> StoreSpecificFiles = new Dictionary<string, AppStore>()
        {
            {"billing-7.1.1.aar", AppStore.GooglePlay},
            {"AmazonAppStore.aar", AppStore.AmazonAppStore}
        };

        /// <summary>
        /// Target a specified Android store.
        /// This sets the correct plugin importer settings for the store
        /// and writes the choice to BillingMode.json so the player
        /// can choose the correct store API at runtime.
        /// Note: This can fail if preconditions are not met for the AppStore.UDP target.
        /// </summary>
        /// <param name="target">App store to enable for next build</param>
        public static void TargetAndroidStore(AppStore target)
        {
            ConfigureProject(target);
        }

        // Unfortunately the UnityEditor API updates only the in-memory list of
        // files available to the build when what we want is a persistent modification
        // to the .meta files. So we must also rely upon the PostProcessScene attribute
        // below to process the
        private static void ConfigureProject(AppStore target)
        {
            foreach (var mapping in StoreSpecificFiles)
            {
                var enabled = mapping.Value == target;

                var path = string.Format("{0}/{1}", BinPath, mapping.Key);
                UnityEngine.Debug.Log($"[UnityPurchasing] Setting {mapping.Key} enabled: {enabled}");
                var importer = (PluginImporter)AssetImporter.GetAtPath(path);
                if (importer != null)
                    importer.SetCompatibleWithPlatform(BuildTarget.Android, enabled);
            }
        }
    }
}
