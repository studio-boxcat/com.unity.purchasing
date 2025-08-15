using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.Purchasing.Models;
namespace UnityEngine.Purchasing
{
    class ProductDetailsQueryResponse
    {
        readonly ConcurrentBag<(GoogleBillingResult, IEnumerable<AndroidJavaObject>)> m_Responses = new ConcurrentBag<(GoogleBillingResult, IEnumerable<AndroidJavaObject>)>();

        ~ProductDetailsQueryResponse()
        {
#if UNITY_2021_2_OR_NEWER
            foreach (var response in m_Responses)
            {
                var objList = response.Item2;
                if (objList == null)
                {
                    continue;
                }

                foreach (var obj in objList)
                {
                    obj?.Dispose();
                }
            }
#endif
        }

        public void AddResponse(GoogleBillingResult billingResult, IEnumerable<AndroidJavaObject> productDetails)
        {
#if UNITY_2021_2_OR_NEWER
            m_Responses.Add((billingResult, productDetails.Select(product => product.CloneReference()).ToList()));
#else
            m_Responses.Add((billingResult, productDetails.Select(product => product).ToList()));
#endif
        }

        public List<AndroidJavaObject> ProductDetails()
        {
            return m_Responses.Where(response => response.Item1.responseCode == GoogleBillingResponseCode.Ok)
                .SelectMany(response => response.Item2).ToList();
        }

        public bool IsRecoverable()
        {
            return m_Responses.Select(response => response.Item1).Any(IsRecoverable);
        }

        public GoogleBillingResult GetGoogleBillingResult()
        {
            return m_Responses.Select(response => response.Item1).FirstOrDefault(response => response.responseCode != GoogleBillingResponseCode.Ok);
        }

        static bool IsRecoverable(GoogleBillingResult billingResult)
        {
            return billingResult.responseCode == GoogleBillingResponseCode.ServiceUnavailable || billingResult.responseCode == GoogleBillingResponseCode.DeveloperError;
        }
    }
}
