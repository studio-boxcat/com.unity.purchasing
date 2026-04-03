// ReSharper disable InconsistentNaming
#nullable enable
using System.Collections.Generic;

namespace UnityEngine.Purchasing
{
    /// <summary>
    /// Provides helper methods to retrieve products by
    /// store independent/store specific id.
    /// </summary>
    public class ProductCollection
    {
        public readonly ProductDefinition[] definitions;
        private readonly Dictionary<string, Product> m_IdToProduct;
        private readonly Dictionary<string, Product> m_StoreSpecificIdToProduct;

        internal ProductCollection(ProductDefinition[] products)
        {
            definitions = products;

            var len = products.Length;
            all = new Product[len];
            m_IdToProduct = new Dictionary<string, Product>(len);
            m_StoreSpecificIdToProduct = new Dictionary<string, Product>(len);

            for (var i = 0; i < products.Length; i++)
            {
                var d = products[i];
                var p = new Product(d, new ProductMetadata());
                all[i] = p;
                m_IdToProduct[d.id] = p;
                m_StoreSpecificIdToProduct[d.storeSpecificId] = p;
            }
        }

        /// <summary>
        /// The array of all products
        /// </summary>
        public readonly Product[] all;

        /// <summary>
        /// Gets a product matching an id
        /// </summary>
        /// <param name="id"> The id of the desired product </param>
        /// <returns> The product matching the id, or null if not found </returns>
        public Product? WithID(string id)
        {
            m_IdToProduct.TryGetValue(id, out var result);
            return result;
        }

        /// <summary>
        /// Gets a product matching a store-specific id
        /// </summary>
        /// <param name="id"> The store-specific id of the desired product </param>
        /// <returns> The product matching the id, or null if not found </returns>
        public Product? WithStoreSpecificID(string? id)
        {
            Product? result = null;
            if (id != null)
            {
                m_StoreSpecificIdToProduct.TryGetValue(id, out result);
            }
            return result;
        }
    }
}
