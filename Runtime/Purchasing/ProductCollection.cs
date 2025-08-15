using System.Collections.Generic;
using System.Linq;

namespace UnityEngine.Purchasing
{
    /// <summary>
    /// Provides helper methods to retrieve products by
    /// store independent/store specific id.
    /// </summary>
    public class ProductCollection
    {
        private readonly Dictionary<string, Product> m_IdToProduct;
        private readonly Dictionary<string, Product> m_StoreSpecificIdToProduct;

        internal ProductCollection(Product[] products)
        {
            set = new HashSet<Product>(products);
            all = set.ToArray();
            m_IdToProduct = all.ToDictionary(x => x.definition.id);
            m_StoreSpecificIdToProduct = all.ToDictionary(x => x.definition.storeSpecificId);
        }

        /// <summary>
        /// The hash set of all products
        /// </summary>
        public HashSet<Product> set { get; }

        /// <summary>
        /// The array of all products
        /// </summary>
        public Product[] all { get; }

        /// <summary>
        /// Gets a product matching an id
        /// </summary>
        /// <param name="id"> The id of the desired product </param>
        /// <returns> The product matching the id, or null if not found </returns>
        public Product WithID(string id)
        {
            m_IdToProduct.TryGetValue(id, out var result);
            return result;
        }

        /// <summary>
        /// Gets a product matching a store-specific id
        /// </summary>
        /// <param name="id"> The store-specific id of the desired product </param>
        /// <returns> The product matching the id, or null if not found </returns>
        public Product WithStoreSpecificID(string id)
        {
            Product result = null;
            if (id != null)
            {
                m_StoreSpecificIdToProduct.TryGetValue(id, out result);
            }
            return result;
        }
    }
}
