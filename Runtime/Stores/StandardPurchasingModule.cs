using System;
using System.Collections.Generic;
using UnityEngine.Purchasing.Extension;
using UnityEngine.Purchasing.Models;
using UnityEngine.Purchasing.Utils;

#if UNITY_PURCHASING_GPBL
using UnityEngine.Purchasing.GooglePlayBilling;
#endif

namespace UnityEngine.Purchasing
{
    /// <summary>
    /// Module for the standard stores covered by Unity;
    /// Apple App store, Google Play and more.
    /// </summary>
    public class StandardPurchasingModule : AbstractPurchasingModule, IAndroidStoreSelection
    {
        internal readonly string k_Version = "4.13.0"; // NOTE: Changed using GenerateUnifiedIAP.sh before pack step.
        /// <summary>
        /// The version of com.unity.purchasing installed and the app was built using.
        /// </summary>
        public string Version => k_Version;

        private readonly NativeStoreProvider m_NativeStoreProvider;
        private readonly RuntimePlatform m_RuntimePlatform;
        private static StandardPurchasingModule ModuleInstance;

        internal UnityUtil util { get; private set; }
        internal ILogger logger { get; private set; }
        internal StoreInstance storeInstance { get; private set; }
        // Map Android store enums to their public names.
        // Necessary because store enum names and public names almost, but not quite, match.
        private static readonly Dictionary<AppStore, string> AndroidStoreNameMap = new Dictionary<AppStore, string>() {
            { AppStore.AmazonAppStore, AmazonApps.Name },
            { AppStore.GooglePlay, GooglePlay.Name },
        };

        internal class StoreInstance
        {
            internal string storeName { get; }
            internal IStore instance { get; }
            internal StoreInstance(string name, IStore instance)
            {
                storeName = name;
                this.instance = instance;
            }
        }

        internal StandardPurchasingModule(UnityUtil util, ILogger logger, NativeStoreProvider nativeStoreProvider,
            RuntimePlatform platform, AppStore android)
        {
            this.util = util;
            this.logger = logger;
            m_NativeStoreProvider = nativeStoreProvider;
            m_RuntimePlatform = platform;
            useFakeStoreUIMode = FakeStoreUIMode.Default;
            useFakeStoreAlways = false;
            appStore = android;
        }

        /// <summary>
        /// A property that retrieves the <c>AppStore</c> type.
        /// </summary>
        public AppStore appStore { get; private set; }

        // At some point we should remove this but to do so will cause a compile error
        // for App developers who used this property directly.
        private readonly bool usingMockMicrosoft;

        /// <summary>
        /// The UI mode for the Fake store, if it's in use.
        /// </summary>
        public FakeStoreUIMode useFakeStoreUIMode { get; set; }

        /// <summary>
        /// Whether or not to use the Fake store.
        /// </summary>
        public bool useFakeStoreAlways { get; set; }

        /// <summary>
        /// Creates an instance of StandardPurchasingModule or retrieves the existing one, specifying a type of App store.
        /// </summary>
        /// <param name="androidStore"> The type of Android Store with which to create the instance. </param>
        /// <returns> The existing instance or the one just created. </returns>
        public static StandardPurchasingModule Instance(AppStore androidStore)
        {
            if (null == ModuleInstance)
            {
                var logger = Debug.unityLogger;
                var gameObject = new GameObject("IAPUtil");
                Object.DontDestroyOnLoad(gameObject);
                gameObject.hideFlags = HideFlags.HideInHierarchy | HideFlags.HideInInspector;
                var util = gameObject.AddComponent<UnityUtil>();

                ModuleInstance = new StandardPurchasingModule(
                    util,
                    logger,
                    new NativeStoreProvider(),
                    Application.platform,
                    androidStore);
            }

            return ModuleInstance;
        }

        /// <summary>
        /// Configures the StandardPurchasingModule.
        /// </summary>
        public override void Configure()
        {
            BindConfiguration<IGooglePlayConfiguration>(new FakeGooglePlayStoreConfiguration());
            BindExtension<IGooglePlayStoreExtensions>(new FakeGooglePlayStoreExtensions());

            BindConfiguration<IAppleConfiguration>(new FakeAppleConfiguration());
            BindExtension<IAppleExtensions>(new FakeAppleExtensions());

            BindConfiguration<IAmazonConfiguration>(new FakeAmazonExtensions());
            BindExtension<IAmazonExtensions>(new FakeAmazonExtensions());

            BindConfiguration<IAndroidStoreSelection>(this);

            BindExtension<ITransactionHistoryExtensions>(new FakeTransactionHistoryExtensions());

            // Our store implementations are singletons, we must not attempt to instantiate
            // them more than once.
            if (null == storeInstance)
            {
                storeInstance = InstantiateStore();
            }

            RegisterStore(storeInstance.storeName, storeInstance.instance);

            // Moving SetModule from reflection to an interface
            var jsonStore = storeInstance.instance as JSONStore;
            if (jsonStore != null)
            {
                // NB: as currently implemented this is also doing Init work for ManagedStore
                jsonStore.SetModule(this);
            }

            // If we are using a JSONStore, bind to it to get transaction history.
            if ((util != null) && jsonStore != null)
            {
                BindExtension<ITransactionHistoryExtensions>(jsonStore);
            }
        }

        private StoreInstance InstantiateStore()
        {
            if (useFakeStoreAlways)
            {
                return new StoreInstance(FakeStore.Name, InstantiateFakeStore());
            }

            switch (m_RuntimePlatform)
            {
                case RuntimePlatform.OSXPlayer:
                    appStore = AppStore.MacAppStore;
                    return new StoreInstance(MacAppStore.Name, InstantiateApple());
                case RuntimePlatform.IPhonePlayer:
                case RuntimePlatform.tvOS:
#if UNITY_VISIONOS
                case RuntimePlatform.VisionOS:
#endif
                    appStore = AppStore.AppleAppStore;
                    return new StoreInstance(AppleAppStore.Name, InstantiateApple());
                case RuntimePlatform.Android:
                    return new StoreInstance(AndroidStoreNameMap[appStore], InstantiateAndroid());
            }
            appStore = AppStore.fake;
            return new StoreInstance(FakeStore.Name, InstantiateFakeStore());
        }

        private IStore InstantiateAndroid()
        {
            if (appStore == AppStore.GooglePlay)
            {
                return InstantiateGoogleStore();
            }
            else
            {
                return InstantiateAndroidHelper(new JSONStore());
            }
        }

        private IStore InstantiateGoogleStore()
        {
            GooglePlayPurchaseCallback googlePurchaseCallback = new GooglePlayPurchaseCallback(util);
            GooglePlayProductCallback googleProductCallback = new GooglePlayProductCallback();
            var googlePurchaseStateEnumProvider = new GooglePurchaseStateEnumProvider();

            var googlePlayStoreService = BuildAndInitGooglePlayStoreServiceAar(googlePurchaseCallback, googleProductCallback, googlePurchaseStateEnumProvider);

            GooglePlayStorePurchaseService googlePlayStorePurchaseService = new GooglePlayStorePurchaseService(googlePlayStoreService);
            GooglePlayStoreFinishTransactionService googlePlayStoreFinishTransactionService = new GooglePlayStoreFinishTransactionService(googlePlayStoreService);
            GoogleFetchPurchases googleFetchPurchases = new GoogleFetchPurchases(googlePlayStoreService, util);
            var googlePlayConfiguration = BuildGooglePlayStoreConfiguration(googlePlayStoreService, googlePurchaseCallback, googleProductCallback);
            var googlePlayStoreExtensions = new GooglePlayStoreExtensions(
                googlePlayStoreService,
                googlePurchaseStateEnumProvider,
                logger);
            var googlePlayStoreRetrieveProductsService = new GooglePlayStoreRetrieveProductsService(
                googlePlayStoreService,
                googleFetchPurchases,
                googlePlayConfiguration,
                googlePlayStoreExtensions);

            var googlePlayStore = new GooglePlayStore(
                googlePlayStoreRetrieveProductsService,
                googlePlayStorePurchaseService,
                googleFetchPurchases,
                googlePlayStoreFinishTransactionService,
                googlePurchaseCallback,
                googlePlayConfiguration,
                googlePlayStoreExtensions,
                util);
            util.AddPauseListener(googlePlayStore.OnPause);
            BindGoogleConfiguration(googlePlayConfiguration);
            BindGoogleExtension(googlePlayStoreExtensions);
            return googlePlayStore;
        }

        void BindGoogleExtension(GooglePlayStoreExtensions googlePlayStoreExtensions)
        {
            BindExtension<IGooglePlayStoreExtensions>(googlePlayStoreExtensions);
        }

        static GooglePlayConfiguration BuildGooglePlayStoreConfiguration(GooglePlayStoreService googlePlayStoreService,
            GooglePlayPurchaseCallback googlePurchaseCallback, GooglePlayProductCallback googleProductCallback)
        {
            var googlePlayConfiguration = new GooglePlayConfiguration(googlePlayStoreService);
            googlePurchaseCallback.SetStoreConfiguration(googlePlayConfiguration);
            googleProductCallback.SetStoreConfiguration(googlePlayConfiguration);
            return googlePlayConfiguration;
        }

        void BindGoogleConfiguration(GooglePlayConfiguration googlePlayConfiguration)
        {
            BindConfiguration<IGooglePlayConfiguration>(googlePlayConfiguration);
        }

        GooglePlayStoreService BuildAndInitGooglePlayStoreServiceAar(GooglePlayPurchaseCallback googlePurchaseCallback,
            GooglePlayProductCallback googleProductCallback, GooglePurchaseStateEnumProvider googlePurchaseStateEnumProvider)
        {
            var googleCachedQueryProductDetailsService = new GoogleCachedQueryProductDetailsService();
            var googleLastKnownProductService = new GoogleLastKnownProductService();
            var googlePurchaseBuilder = new GooglePurchaseBuilder(googleCachedQueryProductDetailsService, logger);
            var googlePurchaseUpdatedListener = new GooglePurchaseUpdatedListener(googleLastKnownProductService,
                googlePurchaseCallback, googlePurchaseBuilder, googleCachedQueryProductDetailsService,
                googlePurchaseStateEnumProvider);
            var googleBillingClient = new GoogleBillingClient(googlePurchaseUpdatedListener, util);
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
                logger,
                googleRetryPolicy,
                util
            );

            googlePlayStoreService.InitConnectionWithGooglePlay();

            return googlePlayStoreService;
        }

        private IStore InstantiateAndroidHelper(JSONStore store)
        {
            store.SetNativeStore(GetAndroidNativeStore(store));
            return store;
        }

        private INativeStore GetAndroidNativeStore(JSONStore store)
        {
            return m_NativeStoreProvider.GetAndroidStore(store, appStore, m_Binder, util);
        }

#if UNITY_PURCHASING_GPBL
        private IStore InstantiateGooglePlayBilling()
        {
            var gameObject = new GameObject("GooglePlayBillingUtil");
            Object.DontDestroyOnLoad (gameObject);
            gameObject.hideFlags = HideFlags.HideInHierarchy | HideFlags.HideInInspector;

            var _util = gameObject.AddComponent<GooglePlayBillingUtil>();

            var store = new GooglePlayStoreImpl(_util);
            BindExtension((IGooglePlayStoreExtensions) store);
            BindConfiguration((IGooglePlayConfiguration) store);
            return store;
        }
#endif

        private IStore InstantiateApple()
        {
            var store = new AppleStoreImpl(util);
            var appleBindings = m_NativeStoreProvider.GetStorekit(store);
            store.SetNativeStore(appleBindings);
            BindExtension<IAppleExtensions>(store);
            return store;
        }

        private IStore InstantiateFakeStore()
        {
            FakeStore fakeStore = null;
            if (useFakeStoreUIMode != FakeStoreUIMode.Default)
            {
                // To access class not available due to UnityEngine.UI conflicts with
                // unit-testing framework, instantiate via reflection
                fakeStore = new UIFakeStore
                {
                    UIMode = useFakeStoreUIMode
                };
            }

            if (fakeStore == null)
            {
                fakeStore = new FakeStore();
            }
            return fakeStore;
        }
    }
}
