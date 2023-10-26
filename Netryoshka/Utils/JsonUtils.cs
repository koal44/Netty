using System.Collections.Generic;

namespace Netryoshka.Utils
{
    public class JsonUtils
    {
        /// <summary>
        /// Splits a JSON array string into a list of top-level JSON object strings.
        /// E.g., given input "[{}, {}, {}]", it returns a list ["{}", "{}", "{}"].
        /// </summary>
        /// <param name="json">The JSON array string to be split. Should be a well-formed JSON array string.</param>
        /// <returns>A list of individual JSON object strings extracted from the top-level array.</returns>
        public static List<string> SplitJsonObjects(string json)
        {
            var result = new List<string>();
            int index = 0;

            while (index < json.Length)
            {
                var (startIndex, endIndex) = FindJsonObjectBoundaries(json, index);

                if (startIndex == -1)
                {
                    break;
                }

                result.Add(json.Substring(startIndex, endIndex - startIndex + 1));
                index = endIndex + 1;
            }

            return result;
        }


        private static (int Start, int End) FindJsonObjectBoundaries(string json, int startingIndex)
        {
            int openBraces = 0;
            int startIndex = 0;
            int endIndex = 0;
            bool foundObject = false;
            bool insideString = false;

            for (int i = startingIndex; i < json.Length; i++)
            {
                char c = json[i];

                if (c == '"' && (i == 0 || json[i - 1] != '\\')) // Toggle string mode, but ignore escaped quotes
                {
                    insideString = !insideString;
                }

                if (insideString)
                {
                    continue; // Skip this iteration if we're inside a string
                }

                switch (c)
                {
                    case '{':
                        if (!foundObject)
                        {
                            startIndex = i;
                            foundObject = true;
                        }
                        openBraces++;
                        break;
                    case '}':
                        openBraces--;
                        break;
                }

                if (foundObject && openBraces == 0)
                {
                    endIndex = i;
                    break;
                }
            }

            if (foundObject && endIndex > startIndex)
            {
                return (startIndex, endIndex);
            }

            return (-1, -1); // Couldn't find object
        }


        public static List<string> ExtractJsonObjectsFromKey(string json, string key)
        {
            var result = new List<string>();
            int keyIndex = 0;
            string keySearch = $"\"{key}\": {{";

            while ((keyIndex = json.IndexOf(keySearch, keyIndex)) != -1)
            {
                var (startIndex, endIndex) = FindJsonObjectBoundaries(json, keyIndex);

                if (startIndex != -1)
                {
                    result.Add(json.Substring(startIndex, endIndex - startIndex + 1));
                }

                keyIndex = endIndex + 1;  // Start the next search after the last found object
            }

            return result;
        }

    }
}
