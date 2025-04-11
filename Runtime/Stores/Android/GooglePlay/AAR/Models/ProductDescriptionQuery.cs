using System;
using System.Collections.Generic;
using UnityEngine.Purchasing.Extension;

namespace UnityEngine.Purchasing.Models
{
    class ProductDescriptionQuery
    {
        internal ProductDefinition[] products;
        internal Action<List<ProductDescription>, GoogleBillingResult> onProductsReceived;
        internal Action<GoogleRetrieveProductsFailureReason, GoogleBillingResponseCode> onRetrieveProductsFailed;

        internal ProductDescriptionQuery(ProductDefinition[] products, Action<List<ProductDescription>, GoogleBillingResult> onProductsReceived, Action<GoogleRetrieveProductsFailureReason, GoogleBillingResponseCode> onRetrieveProductsFailed)
        {
            this.products = products;
            this.onProductsReceived = onProductsReceived;
            this.onRetrieveProductsFailed = onRetrieveProductsFailed;
        }
    }
}
