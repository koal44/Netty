using Newtonsoft.Json;
using System.Collections;
using System.Collections.Generic;

namespace Netryoshka
{
    public class TreeNode
    {
        // https://michaelscodingspot.com/displaying-any-net-object-in-a-wpf-treeview/

        public string? Name { get; set; }
        public string? Value { get; set; }
        public List<TreeNode> Children { get; set; } = new();

        public static TreeNode CreateTree(object obj)
        {
            //JavaScriptSerializer jss = new JavaScriptSerializer();
            //Dictionary<string, object> deserializedDict = jss.Deserialize<Dictionary<string, object>>(serialized);

            var settings = new JsonSerializerSettings
            {
                Error = (sender, args) =>
                {
                    args.ErrorContext.Handled = true;

                    // Replace the value causing error with the name of its type.
                    //args.CurrentObject = args.CurrentObject?.GetType().Name ?? "Unknown Object";
                }



            };

            var serialized = JsonConvert.SerializeObject(obj, settings);
            var deserializedDict = JsonConvert.DeserializeObject<Dictionary<string, object>>(serialized);
            var root = new TreeNode
            {
                Name = "Root"
            };
            if (deserializedDict != null)
            {
                BuildTree(deserializedDict, root);
            }
            return root;
        }

        private static void BuildTree(object item, TreeNode node)
        {
            if (item is KeyValuePair<string, object> kv)
            {
                var keyValueNode = new TreeNode()
                {
                    Name = kv.Key,
                    Value = GetValueAsString(kv.Value)
                };
                node.Children.Add(keyValueNode);
                BuildTree(kv.Value, keyValueNode);
            }
            else if (item is ArrayList list)
            {
                int index = 0;
                foreach (object value in list)
                {
                    var arrayItem = new TreeNode
                    {
                        Name = $"[{index}]",
                        Value = ""
                    };
                    node.Children.Add(arrayItem);
                    BuildTree(value, arrayItem);
                    index++;
                }
            }
            else if (item is Dictionary<string, object> dictionary)
            {
                foreach (KeyValuePair<string, object> d in dictionary)
                {
                    BuildTree(d, node);
                }
            }
        }

        private static string GetValueAsString(object value)
        {
            return value switch
            {
                null => "null",
                ArrayList arr => $"[{arr.Count}]",
                _ when value.GetType().IsArray => "[]",
                _ when value.GetType().IsGenericType => "{}",
                _ => $"{value}"
            };
        }
    }
}
