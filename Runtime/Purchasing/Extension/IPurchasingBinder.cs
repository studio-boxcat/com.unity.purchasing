namespace UnityEngine.Purchasing.Extension
{
    /// <summary>
    /// Configures Unity Purchasing with one or more
    /// store implementations.
    /// </summary>
    public interface IPurchasingBinder
    {
        /// <summary>
        /// Informs Unity Purchasing that a store implementation exists,
        /// specifying its name.
        ///
        /// Modules can pass null IStore instances when running on platforms
        /// they do not support.
        /// </summary>
        /// <param name="name"> The name of the store </param>
        /// <param name="store"> The instance of the store </param>
        void RegisterStore(string name, IStore store);

        /// <summary>
        /// Informs Unity Purchasing that a store extension is available.
        /// </summary>
        /// <typeparam name="T"> Implementation of <c>IStoreExtension</c>. </typeparam>
        /// <param name="instance"> The instance of the store extension. </param>
        void RegisterExtension<T>(T instance) where T : IStoreExtension;

        /// <summary>
        /// Informs Unity Purchasing that extended Configuration is available.
        /// </summary>
        /// <typeparam name="T"> Implementation of <c>IStoreConfiguration</c>. </typeparam>
        /// <param name="instance"> The instance of the store configuration. </param>
        void RegisterConfiguration<T>(T instance) where T : IStoreConfiguration;
    }
}
