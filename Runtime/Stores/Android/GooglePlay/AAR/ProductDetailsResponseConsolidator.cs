using System;
using System.Collections.Generic;
using UnityEngine.Purchasing.Models;

namespace UnityEngine.Purchasing
{
    class ProductDetailsResponseConsolidator
    {
        const int k_RequiredNumberOfCallbacks = 2;
        int m_NumberReceivedCallbacks;
        readonly Action<ProductDetailsQueryResponse> m_OnProductDetailsResponseConsolidated;
        readonly ProductDetailsQueryResponse m_Responses = new ProductDetailsQueryResponse();

        internal ProductDetailsResponseConsolidator(Action<ProductDetailsQueryResponse> onProductDetailsResponseConsolidated)
        {
            m_OnProductDetailsResponseConsolidated = onProductDetailsResponseConsolidated;
        }

        public void Consolidate(GoogleBillingResult billingResult, IEnumerable<AndroidJavaObject> productDetails)
        {
            try
            {
                m_NumberReceivedCallbacks++;

                m_Responses.AddResponse(billingResult, productDetails);

                if (m_NumberReceivedCallbacks >= k_RequiredNumberOfCallbacks)
                {
                    m_OnProductDetailsResponseConsolidated(m_Responses);
                }
            }
            catch (Exception ex)
            {
            }
        }
    }
}
