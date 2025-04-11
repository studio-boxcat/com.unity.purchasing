using System.Linq;
using UnityEngine.Assertions;
using UnityEngine.Purchasing.Extension;

namespace UnityEngine.Purchasing
{
    /// <summary>
    /// Builds configuration for Unity Purchasing,
    /// consisting of products and store specific configuration details.
    /// </summary>
    public class ConfigurationBuilder
    {
        internal ConfigurationBuilder(PurchasingFactory factory)
        {
            this.factory = factory;
        }

        /// <summary>
        /// The set of products in the catalog.
        /// </summary>
        public ProductDefinition[] products { get; private set; }

        internal PurchasingFactory factory { get; }

        /// <summary>
        /// Configure the store as specified by the template parameter.
        /// </summary>
        /// <typeparam name="T"> Implementation of <c>IStoreConfiguration</c> </typeparam>
        /// <returns> The store configuration as an object. </returns>
        public T Configure<T>() where T : IStoreConfiguration
        {
            return factory.GetConfig<T>();
        }

        /// <summary>
        /// Create an instance of the configuration builder.
        /// </summary>
        /// <param name="first"> The first purchasing module. </param>
        /// <param name="rest"> The remaining purchasing modules, excluding the one passes as first. </param>
        /// <returns> The instance of the configuration builder as specified. </returns>
        public static ConfigurationBuilder Instance(IPurchasingModule first, params IPurchasingModule[] rest)
        {
            var factory = new PurchasingFactory(first, rest);
            return new ConfigurationBuilder(factory);
        }

        /// <summary>
        /// Add multiple products to the configuration builder.
        /// </summary>
        /// <param name="products"> The enumerator of the product definitions to be added. </param>
        /// <returns> The instance of the configuration builder with the new product added. </returns>
        public ConfigurationBuilder AddProducts(ProductDefinition[] products)
        {
            Assert.IsNull(this.products, "Products have already been set");
            Assert.AreEqual(products.Length, products.Select(p => p.id).Distinct().Count(), "Product ids must be unique");

            this.products = products;
            return this;
        }
    }
}