using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Netryoshka.Extensions
{
    public static class DictionaryExtensions
    {
        public static string PrettyPrint<TKey, TValue>(this IDictionary<TKey, TValue> dict)
        {
            return dict.Aggregate("{\n", (x, kvp) => $"{x}\n  {kvp.Key}: {kvp.Value},", x => $"{x}\n}}");
        }
    }
}
