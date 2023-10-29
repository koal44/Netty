using Netryoshka.Json;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net;
using System.Net.NetworkInformation;

namespace Netryoshka
{
    public class TreeNode
    {
        public string? PropertyName { get; set; }
        public string? PropertyValue { get; set; }
        public List<TreeNode>? Children { get; set; }


        public TreeNode()
        {

        }


        public static List<TreeNode> BuildNodesFromJson(string json, string rootName = "")
        {
            var jToken = JToken.Parse(json);
            return CreateTreeNodesFromJToken(jToken, rootName);
        }


        public static List<TreeNode> BuildNodesFromObject(object obj, string rootName = "")
        {
            var jToken = SerializeToJToken(obj);
            return CreateTreeNodesFromJToken(jToken, rootName);
        }


        private static JToken SerializeToJToken(object obj)
        {
            var settings = new JsonSerializerSettings
            {
                Converters = new List<JsonConverter>
                {
                    new ToStringConverter(new List<Type>
                    {
                        typeof(PhysicalAddress),
                        typeof(IPAddress),
                        typeof(byte[]),
                    })
                },
                NullValueHandling = NullValueHandling.Ignore,
                Error = (sender, args) =>
                {
                    Debug.Print(args.ErrorContext.Error.Message);
                    args.ErrorContext.Handled = true;
                }
            };

            var jToken = JToken.FromObject(obj, JsonSerializer.CreateDefault(settings));
            return jToken;
        }


        /// <summary>
        /// Creates a list of tree nodes from a given JToken.
        /// </summary>
        /// <param name="token">The JToken to convert.</param>
        /// <param name="rootName">The root name for the tree.</param>
        /// <param name="parentPropName">The property name of the parent array node.For recursive calls.</param>
        /// <param name="arrayIndex">The index in the parent array. For recursive calls.</param>
        /// <returns>A list of <see cref="TreeNode"/>s objects representing the JToken.</returns>
        /// <remarks>
        /// For JToken types that are not JArray, the returned list will contain a single TreeNode.
        /// </remarks>
        private static List<TreeNode> CreateTreeNodesFromJToken(JToken token, string rootName = "", string parentPropName = "", int arrayIndex = 0)
        {
            var propName = string.Empty;

            if (token.Parent is JArray)
            {
                // JArrays will be named like "foo.bar[0]".
                propName = token.Parent.Parent == null ? $"{rootName}[{arrayIndex}]" : $"{parentPropName}[{arrayIndex}]";
            }
            else if (token is JProperty prop)
            {
                propName = prop.Name;
                token = prop.Value;
            }
            else if (token is JObject && token.Parent == null)
            {
                propName = rootName;
            }

            // Create the TreeNode object with PropertyName, PropertyValue, and Children initialized.
            var node = new TreeNode()
            {
                PropertyName = propName,
                PropertyValue = token is JValue value
                    // Handle strings separately to include quotes.
                    ? value.Type == JTokenType.String
                        ? $"\"{(value.ToString().Replace("\r\n", ""))}\""
                        : $"{value}"
                    : null,
                // Initialize Children list if the token is an object or array.
                Children = token is JObject or JArray ? new List<TreeNode>() : null
            };

            int i = 0;
            // Recursively populate the Children list for JObject or JArray types.
            foreach (var child in token.Children())
            {
                string nextParentPropName = token is JArray ? propName : "";
                node.Children?.AddRange(CreateTreeNodesFromJToken(child, rootName, nextParentPropName, i++));
            }

            // If the token is a JArray, the current node serves as a container and is not returned.
            // Instead, its children are returned.
            if (token is JArray)
            {
                return node.Children ?? new List<TreeNode>();
            }

            // If it's not a JArray, return a single node list.
            return new List<TreeNode> { node };
        }

    }
}
