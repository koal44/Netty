using System.Collections.Generic;
using System.Linq;

namespace Netryoshka.Utils
{
    public class StringUtils
    {
        /// <summary>
        /// Joins a sequence of strings with a given separator, ignoring any null or empty strings.
        /// </summary>
        /// <param name="separator">The string to use as a separator.</param>
        /// <param name="items">A sequence of strings to join.</param>
        /// <returns>A string containing all the sequence elements separated by the separator string.</returns>
        public static string StringJoin(string separator, IEnumerable<string?>? items)
        {
            if (items == null)
            {
                return string.Empty;
            }

            var filteredItems = items.Where(item => !string.IsNullOrEmpty(item));
            return string.Join(separator, filteredItems);
        }
    }
}
