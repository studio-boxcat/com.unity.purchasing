namespace UnityEngine.Purchasing
{
    /// <summary>
    /// Product definition used by Apps declaring products for sale.
    /// </summary>
    public readonly struct ProductDefinition
    {
        /// <summary>
        /// Parametrized constructor
        /// </summary>
        /// <param name="id"> The product id. </param>
        /// <param name="storeSpecificId"> The product's id for a specific store. </param>
        /// <param name="type"> The product type. </param>
        public ProductDefinition(string id, string storeSpecificId, ProductType type)
        {
            this.id = id;
            this.storeSpecificId = storeSpecificId;
            this.type = type;
        }

        /// <summary>
        /// Parametrized constructor, creating a ProductDefinition where the id is the same as the store specific ID.
        /// </summary>
        /// <param name="id"> The product id as well as its store-specific id. </param>
        /// <param name="type"> The product type. </param>
        public ProductDefinition(string id, ProductType type) : this(id, id, type)
        {
        }

        /// <summary>
        /// Store independent ID.
        /// </summary>
        public readonly string id;

        /// <summary>
        /// The ID this product has on a specific store.
        /// </summary>
        public readonly string storeSpecificId;

        /// <summary>
        /// The type of the product.
        /// </summary>
        public readonly ProductType type;

        /// <summary>
        /// Check if this product definition is equal to another.
        /// </summary>
        /// <param name="obj"> The product definition to compare with this object. </param>
        /// <returns> True if the definitions are equal </returns>
        public override bool Equals(object obj)
        {
            return obj is ProductDefinition p && id == p.id;
        }

        /// <summary>
        /// Get the unique Hash representing the product definition.
        /// </summary>
        /// <returns> The hash code as integer </returns>
        public override int GetHashCode()
        {
            return id.GetHashCode();
        }
    }
}
