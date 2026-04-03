/*
* Copyright (c) 2013 Calvin Rien
*
* Based on the JSON parser by Patrick van Bergen
* http://techblog.procurios.nl/k/618/news/view/14605/14863/How-do-I-write-my-own-parser-for-JSON.html
*
* Simplified it so that it doesn't throw exceptions
* and can be used in Unity iPhone with maximum code stripping.
*
* Permission is hereby granted, free of charge, to any person obtaining
* a copy of this software and associated documentation files (the
* "Software"), to deal in the Software without restriction, including
* without limitation the rights to use, copy, modify, merge, publish,
* distribute, sublicense, and/or sell copies of the Software, and to
* permit persons to whom the Software is furnished to do so, subject to
* the following conditions:
*
* The above copyright notice and this permission notice shall be
* included in all copies or substantial portions of the Software.
*
* THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
* EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
* MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT.
* IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY
* CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT,
* TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE
* SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
*/
using System.Collections.Generic;

namespace UnityEngine.Purchasing
{
    namespace MiniJSON
    {
        // Example usage:
        //
        //  using UnityEngine;
        //  using System.Collections;
        //  using System.Collections.Generic;
        //  using MiniJSON;
        //
        //  public class MiniJSONTest : MonoBehaviour {
        //      void Start () {
        //          var jsonString = "{ \"array\": [1.44,2,3], " +
        //                          "\"object\": {\"key1\":\"value1\", \"key2\":256}, " +
        //                          "\"string\": \"The quick brown fox \\\"jumps\\\" over the lazy dog \", " +
        //                          "\"unicode\": \"\\u3041 Men\u00fa sesi\u00f3n\", " +
        //                          "\"int\": 65536, " +
        //                          "\"float\": 3.1415926, " +
        //                          "\"bool\": true, " +
        //                          "\"null\": null }";
        //
        //          var dict = Json.Deserialize(jsonString) as Dictionary<string,object>;
        //
        //          Debug.Log("deserialized: " + dict.GetType());
        //          Debug.Log("dict['array'][0]: " + ((List<object>) dict["array"])[0]);
        //          Debug.Log("dict['string']: " + (string) dict["string"]);
        //          Debug.Log("dict['float']: " + (double) dict["float"]); // floats come out as doubles
        //          Debug.Log("dict['int']: " + (long) dict["int"]); // ints come out as longs
        //          Debug.Log("dict['unicode']: " + (string) dict["unicode"]);
        //
        //          var str = Json.Serialize(dict);
        //
        //          Debug.Log("serialized: " + str);
        //      }
        //  }

        // By Unity
        #region Extension methods

        /// <summary>
        /// Extension class for MiniJson to access values in JSON format.
        /// </summary>
        public static class MiniJsonExtensions
        {
            /// <summary>
            /// Convert a Dictionary to JSON.
            /// </summary>
            /// <param name="obj">The dictionary to convert to JSON.</param>
            /// <returns>The converted dictionary in JSON string format.</returns>
            public static string toJson(this Dictionary<string, object> obj)
            {
                return MiniJson.JsonEncode(obj);
            }

            /// <summary>
            /// Convert string JSON into List of Objects.
            /// </summary>
            /// <param name="json">String JSON to convert.</param>
            /// <returns>List of Objects converted from string json.</returns>
            public static List<object> ArrayListFromJson(this string json)
            {
                return MiniJson.JsonDecode(json) as List<object>;
            }
        }

        #endregion
    }

    /// <summary>
    /// Extension class for MiniJson to Encode and Decode JSON.
    /// </summary>
    public class MiniJson
    {
        /// <summary>
        /// Converts an object into a JSON string
        /// </summary>
        /// <param name="json">Object to convert to JSON string</param>
        /// <returns>JSON string</returns>
        public static string JsonEncode(object json)
        {
            return Google.MiniJSON.Json.Serialize(json);
        }

        /// <summary>
        /// Converts an string into a JSON object
        /// </summary>
        /// <param name="json">String to convert to JSON object</param>
        /// <returns>JSON object</returns>
        public static object JsonDecode(string json)
        {
            return Google.MiniJSON.Json.Deserialize(json);
        }
    }
}
