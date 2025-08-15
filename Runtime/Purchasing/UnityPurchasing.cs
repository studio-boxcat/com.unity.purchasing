using UnityEngine.Assertions;

namespace UnityEngine.Purchasing
{
    /// <summary>
    /// The core abstract implementation for Unity Purchasing.
    /// </summary>
    public abstract class UnityPurchasing
    {
        /// <summary>
        /// The main initialization call for Unity Purchasing.
        /// </summary>
        /// <param name="listener"> The <c>IDetailedStoreListener</c> to receive callbacks for future transactions </param>
        /// <param name="builder"> The <c>ConfigurationBuilder</c> containing the product definitions mapped to stores </param>
        public static void Initialize(IStoreListener listener, ConfigurationBuilder builder)
        {
            var logger = Debug.unityLogger;

            Initialize(listener, builder, logger, Application.persistentDataPath);
        }

        /// <summary>
        /// This is useful in certain test scenarios, such as repeatedly testing
        /// an App's behaviour when purchases are restored.
        ///
        /// This is a static method since developers may wish to clear the log before
        /// initialising IAP.
        /// </summary>
        public static void ClearTransactionLog()
        {
            var log = new TransactionLog(Debug.unityLogger, Application.persistentDataPath);
            log.Clear();
        }

        /// <summary>
        /// Created for integration testing.
        /// </summary>
        internal static void Initialize(IStoreListener listener, ConfigurationBuilder builder,
            ILogger logger, string persistentDatapath)
        {
            Assert.IsNotNull(builder.products, "Products must be set before initialization");

            var transactionLog = new TransactionLog(logger, persistentDatapath);
            var manager = new PurchasingManager(transactionLog, logger, builder.factory.service,
                builder.factory.storeName);

            // Proxy the PurchasingManager's callback interface to forward Transactions to Analytics.
            var proxy = new StoreListenerProxy(listener, builder.factory);
            manager.Initialize(proxy, builder.products);
        }
    }
}
