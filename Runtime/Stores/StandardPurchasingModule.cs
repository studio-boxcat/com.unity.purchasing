// ReSharper disable InconsistentNaming
#nullable enable
using System;
using UnityEngine.Purchasing.Extension;
using UnityEngine.Purchasing.Models;
using UnityEngine.Purchasing.Utils;

namespace UnityEngine.Purchasing
{
    /// <summary>
    /// Module for the standard stores covered by Unity;
    /// Apple App store, Google Play and more.
    /// </summary>
    public static class StandardPurchasingModule
    {
        internal const string k_Version = "4.13.0"; // NOTE: Changed using GenerateUnifiedIAP.sh before pack step.

        public static IStore Create(AppStore appStore, out object? extension)
        {
            UnityUtil.Init();
            extension = null;
            return appStore switch
            {
                AppStore.GooglePlay => InstantiateGoogleStore(out extension),
                AppStore.AmazonAppStore => InstantiateAmazonStore(out extension),
                AppStore.AppleAppStore or AppStore.MacAppStore => InstantiateApple(appStore, out extension),
                _ => new FakeStore()
            };
        }

        private static IStore InstantiateGoogleStore(out object extension)
        {
            GooglePlayPurchaseCallback googlePurchaseCallback = new GooglePlayPurchaseCallback();
            GooglePlayProductCallback googleProductCallback = new GooglePlayProductCallback();
            var googlePurchaseStateEnumProvider = new GooglePurchaseStateEnumProvider();

            var googlePlayStoreService = BuildAndInitGooglePlayStoreServiceAar(googlePurchaseCallback, googleProductCallback, googlePurchaseStateEnumProvider);

            GooglePlayStoreFinishTransactionService googlePlayStoreFinishTransactionService = new GooglePlayStoreFinishTransactionService(googlePlayStoreService);
            GoogleFetchPurchases googleFetchPurchases = new GoogleFetchPurchases(googlePlayStoreService);
            var googlePlayConfiguration = BuildGooglePlayStoreConfiguration(googlePlayStoreService, googlePurchaseCallback, googleProductCallback);
            var googlePlayStoreExtensions = new GooglePlayStoreExtensions(
                googlePlayStoreService,
                googlePurchaseStateEnumProvider);
            var googlePlayStoreRetrieveProductsService = new GooglePlayStoreRetrieveProductsService(
                googlePlayStoreService,
                googleFetchPurchases,
                googlePlayConfiguration,
                googlePlayStoreExtensions);

            var googlePlayStore = new GooglePlayStore(
                googlePlayStoreRetrieveProductsService,
                googlePlayStoreService,
                googleFetchPurchases,
                googlePlayStoreFinishTransactionService,
                googlePurchaseCallback,
                googlePlayConfiguration,
                googlePlayStoreExtensions);
            UnityUtil.AddPauseListener(googlePlayStore.OnPause);
            extension = googlePlayStoreExtensions;
            return googlePlayStore;
        }

        static GooglePlayConfiguration BuildGooglePlayStoreConfiguration(GooglePlayStoreService googlePlayStoreService,
            GooglePlayPurchaseCallback googlePurchaseCallback, GooglePlayProductCallback googleProductCallback)
        {
            var googlePlayConfiguration = new GooglePlayConfiguration(googlePlayStoreService);
            googlePurchaseCallback.SetStoreConfiguration(googlePlayConfiguration);
            googleProductCallback.SetStoreConfiguration(googlePlayConfiguration);
            return googlePlayConfiguration;
        }

        static GooglePlayStoreService BuildAndInitGooglePlayStoreServiceAar(GooglePlayPurchaseCallback googlePurchaseCallback,
            GooglePlayProductCallback googleProductCallback, GooglePurchaseStateEnumProvider googlePurchaseStateEnumProvider)
        {
            var googleCachedQueryProductDetailsService = new GoogleCachedQueryProductDetailsService();
            var googleLastKnownProductService = new GoogleLastKnownProductService();
            var googlePurchaseBuilder = new GooglePurchaseBuilder(googleCachedQueryProductDetailsService);
            var googlePurchaseUpdatedListener = new GooglePurchaseUpdatedListener(googleLastKnownProductService,
                googlePurchaseCallback, googlePurchaseBuilder, googleCachedQueryProductDetailsService,
                googlePurchaseStateEnumProvider);
            var googleBillingClient = new GoogleBillingClient(googlePurchaseUpdatedListener);
            var productDetailsConverter = new ProductDetailsConverter();
            var retryPolicy = new ExponentialRetryPolicy();
            var googleRetryPolicy = new GoogleConnectionRetryPolicy();
            var googleQueryProductDetailsService = new QueryProductDetailsService(googleBillingClient, googleCachedQueryProductDetailsService, productDetailsConverter, retryPolicy, googleProductCallback);
            var purchaseService = new GooglePurchaseService(googleBillingClient, googlePurchaseCallback, googleQueryProductDetailsService);
            var queryPurchasesService = new GoogleQueryPurchasesService(googleBillingClient, googlePurchaseBuilder);
            var finishTransactionService = new GoogleFinishTransactionService(googleBillingClient, queryPurchasesService);
            var billingClientStateListener = new BillingClientStateListener();

            googlePurchaseUpdatedListener.SetGoogleQueryPurchaseService(queryPurchasesService);

            var googlePlayStoreService = new GooglePlayStoreService(
                googleBillingClient,
                googleQueryProductDetailsService,
                purchaseService,
                finishTransactionService,
                queryPurchasesService,
                billingClientStateListener,
                googleLastKnownProductService,
                googleRetryPolicy
            );

            googlePlayStoreService.InitConnectionWithGooglePlay();

            return googlePlayStoreService;
        }

        private static IStore InstantiateAmazonStore(out object extension)
        {
            var store = new JSONStore();

            // Switch Android callbacks to the scripting thread, via ScriptingUnityCallback.
            var proxy = new JavaBridge(new ScriptingUnityCallback(store));
            using var pluginClass = new AndroidJavaClass("com.unity.purchasing.amazon.AmazonPurchasing");
            var instance = pluginClass.CallStatic<AndroidJavaObject>("instance", proxy);
            INativeStore nativeStore = new AndroidJavaStore(instance);

            // Hook up our amazon specific functionality.
            extension = new AmazonAppStoreStoreExtensions(instance);

            store.SetNativeStore(nativeStore);

            return store;
        }

        private static IStore InstantiateApple(AppStore appStore, out object extension)
        {
            var store = new AppleStoreImpl();
            var nativeStore = appStore switch
            {
                AppStore.AppleAppStore => (INativeStore)new iOSStoreBindings(),
                AppStore.MacAppStore => new OSXStoreBindings(),
                _ => throw new ArgumentOutOfRangeException(nameof(appStore), appStore, null)
            };
            store.SetNativeStore(nativeStore);
            extension = store;
            return store;
        }
    }
}
