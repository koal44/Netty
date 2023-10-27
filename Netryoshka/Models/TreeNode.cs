using Netryoshka.Json;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Net;
using System.Net.NetworkInformation;

namespace Netryoshka
{
    public class TreeNode
    {
        public string? PropertyName { get; set; }
        public string? PropertyValue { get; set; }
        public ObservableCollection<TreeNode>? Children { get; set; }


        public TreeNode()
        {

        }


        public static TreeNode BuildFromJson(string json)
        {
            var jObject = JObject.Parse(json);
            return CreateTreeNodeFromJToken(jObject);
        }


        public static TreeNode BuildFromObject(object obj)
        {
            var jObject = SerializeToJObject(obj);
            return CreateTreeNodeFromJToken(jObject);
        }


        private static JObject SerializeToJObject(object obj)
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

            var jObject = JObject.FromObject(obj, JsonSerializer.CreateDefault(settings));
            return jObject;
        }


        private static TreeNode CreateTreeNodeFromJToken(JToken token)
        {
            var propName = string.Empty;

            if (token is JProperty prop)
            {
                propName = prop.Name;
                token = prop.Value;
            }

            var node = new TreeNode()
            {
                PropertyName = propName,
                PropertyValue = token is JValue value
                    ? value.Type == JTokenType.String
                        //? $"\"{(value.ToString().Replace("\r", "\\r").Replace("\n", "\\n"))}\""
                        ? $"\"{(value.ToString().Replace("\r\n", ""))}\""
                        : $"{value}"
                    : null,
                Children = token is JObject or JArray ? new ObservableCollection<TreeNode>() : null
            };

            foreach (var child in token.Children())
            {
                node.Children?.Add(CreateTreeNodeFromJToken(child));
            }

            return node;
        }

    }
}
