using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine.Assertions;
using UnityEngine.Purchasing.Extension;

namespace UnityEngine.Purchasing
{
    static class SerializationExtensions
    {
        public static string TryGetString(this Dictionary<string, object> dic, string key)
        {
            if (dic.ContainsKey(key))
            {
                if (dic[key] != null)
                {
                    return dic[key].ToString();
                }
            }

            return null;
        }
    }

    internal class JSONSerializer
    {
        public static string SerializeProductDef(ProductDefinition product)
        {
            var sb = new StringBuilder();
            EncodeProductDef(product, sb);
            return sb.ToString();
        }

        public static string SerializeProductDefs(ProductDefinition[] products)
        {
            if (products.Length is 0)
                return "[]";

            var sb = new StringBuilder();
            sb.Append('[');
            for (var index = 0; index < products.Length; index++)
            {
                var product = products[index];
                if (index > 0) sb.Append(',');
                EncodeProductDef(product, sb);
            }
            sb.Append(']');
            return sb.ToString();
        }

        public static Dictionary<string, string> DeserializeSubscriptionDescriptions(string json)
        {
            var objects = (List<object>)MiniJson.JsonDecode(json);
            var result = new Dictionary<string, string>();
            foreach (Dictionary<string, object> obj in objects)
            {
                var subscription = new Dictionary<string, string>();
                if (obj.TryGetValue("metadata", out var metadata))
                {
                    var metadataDict = (Dictionary<string, object>)metadata;
                    subscription["introductoryPrice"] = metadataDict.TryGetString("introductoryPrice");
                    subscription["introductoryPriceLocale"] = metadataDict.TryGetString("introductoryPriceLocale");
                    subscription["introductoryPriceNumberOfPeriods"] = metadataDict.TryGetString("introductoryPriceNumberOfPeriods");
                    subscription["numberOfUnits"] = metadataDict.TryGetString("numberOfUnits");
                    subscription["unit"] = metadataDict.TryGetString("unit");

                    // this is a double check for Apple side's bug
                    if (!string.IsNullOrEmpty(subscription["numberOfUnits"]) && string.IsNullOrEmpty(subscription["unit"]))
                    {
                        subscription["unit"] = "0";
                    }
                }
                else
                {
                    Debug.LogWarning("metadata key not found in subscription description json");
                }

                if (obj.TryGetValue("storeSpecificId", out var id))
                {
                    var idStr = (string)id;
                    result.Add(idStr, MiniJson.JsonEncode(subscription));
                }
                else
                {
                    Debug.LogWarning("storeSpecificId key not found in subscription description json");
                }
            }

            return result;
        }

        public static PurchaseFailureDescription DeserializeFailureReason(string json)
        {
            var dic = (Dictionary<string, object>)MiniJson.JsonDecode(json);
            var reason = PurchaseFailureReason.Unknown;

            if (dic.TryGetValue("reason", out var reasonStr))
            {
                if (Enum.IsDefined(typeof(PurchaseFailureReason), (string)reasonStr))
                {
                    reason = (PurchaseFailureReason)Enum.Parse(typeof(PurchaseFailureReason), (string)reasonStr);
                }

                if (dic.TryGetValue("productId", out var productId))
                {
                    return new PurchaseFailureDescription((string)productId, reason, BuildPurchaseFailureDescriptionMessage(dic));
                }
            }
            else
            {
                Debug.LogWarning("Reason key not found in purchase failure json: " + json);
            }

            return new PurchaseFailureDescription("Unknown ProductID", reason, BuildPurchaseFailureDescriptionMessage(dic));
        }

        static string BuildPurchaseFailureDescriptionMessage(Dictionary<string, object> dic)
        {
            var message = dic.TryGetString("message");
            var storeSpecificErrorCode = dic.TryGetString("storeSpecificErrorCode");

            if (message == null && storeSpecificErrorCode == null)
            {
                return null;
            }

            if (storeSpecificErrorCode != null)
            {
                storeSpecificErrorCode = " storeSpecificErrorCode: " + storeSpecificErrorCode;
            }

            return message + storeSpecificErrorCode;
        }

        private static void EncodeProductDef(ProductDefinition product, StringBuilder sb)
        {
            // do the manual JSON encoding here to avoid the overhead.
            AssertJsonString(product.id);
            AssertJsonString(product.storeSpecificId);
            sb.Append("{\"id\":\"").Append(product.id)
                .Append("\",\"storeSpecificId\":\"").Append(product.storeSpecificId)
                .Append("\",\"type\":\"").Append(ProductTypeToString(product.type))
                .Append("\"}");
            return;

            static void AssertJsonString(string str)
            {
                Assert.IsFalse(str.Contains('\"'), $"String {str} contains a double quote, which may cause JSON encoding issues.");
            }

            static string ProductTypeToString(ProductType type)
            {
                return type switch
                {
                    ProductType.Consumable => nameof(ProductType.Consumable),
                    ProductType.NonConsumable => nameof(ProductType.NonConsumable),
                    ProductType.Subscription => nameof(ProductType.Subscription),
                    _ => throw new ArgumentOutOfRangeException(nameof(type), $"Unexpected product type: {type}")
                };
            }
        }
    }
}
