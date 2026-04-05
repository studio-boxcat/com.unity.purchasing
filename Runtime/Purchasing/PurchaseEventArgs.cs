#nullable enable

namespace UnityEngine.Purchasing
{
    /// <summary>
    /// A purchase that succeeded, including the purchased product
    /// along with its purchase receipt.
    /// </summary>
    public struct PurchaseEventArgs
    {
        internal PurchaseEventArgs(Product purchasedProduct)
        {
            this.purchasedProduct = purchasedProduct;
        }

        /// <summary>
        /// The product which was purchased successfully.
        /// </summary>
        public readonly Product purchasedProduct;
    }
}
