using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Markup;
using System.Windows.Media;
using System.Xml;

namespace Netryoshka.Utils
{
    public static class Util
    {

        private static readonly Random _random = new();




        /// <summary>
        /// Joins an array of strings with a given separator, ignoring any null or empty strings.
        /// </summary>
        /// <param name="separator">The string to use as a separator.</param>
        /// <param name="items">An array of strings to join.</param>
        /// <returns>A string containing all the array elements separated by the separator character.</returns>
        public static string StringJoin(string separator, params string[] items)
        {
            if (items == null)
            {
                return string.Empty;
            }

            var filteredItems = items.Where(item => !string.IsNullOrEmpty(item));
            return string.Join(separator, filteredItems);
        }


        /// <summary>
        /// Converts a hexadecimal string to a byte array using optimized bitwise operations.
        /// </summary>
        /// <param name="hex">The hexadecimal string to convert.</param>
        /// <returns>A byte array representing the hexadecimal string.</returns>
        /// <exception cref="ArgumentException">Thrown when the input hex string has an odd length.</exception>
        public static byte[] HexToBytesOptimized(string hex)
        {
            // Check if the hex string has an odd number of characters, which is invalid.
            if (hex.Length % 2 == 1)
            {
                throw new ArgumentException("The hex string cannot have an odd number of digits", nameof(hex));
            }

            // Initialize the byte array. 
            // Using bit-shifting to divide the length of the string by 2.
            byte[] arr = new byte[hex.Length >> 1];

            // Iterate through each pair of characters in the hex string.
            for (int i = 0; i < hex.Length >> 1; ++i)
            {
                // Convert each pair of hex characters to a byte using bitwise operations.
                // Shift the first hex character 4 bits to the left to make room for the second character,
                // and then use bitwise OR to combine them into a single byte.
                arr[i] = (byte)((GetHexValue(hex[i << 1]) << 4) + GetHexValue(hex[(i << 1) + 1]));
            }

            return arr;
        }

        /// <summary>
        /// Gets the integer value of a hex character.
        /// </summary>
        /// <param name="hexChar">The hex character to convert.</param>
        /// <returns>The integer value of the hex character.</returns>
        private static int GetHexValue(char hexChar)
        {
            int val = hexChar;
            // Convert the hex character to its integer value.
            // ASCII values for '0' to '9' are 48-57.
            // ASCII values for 'A' to 'F' are 65-70.
            // ASCII values for 'a' to 'f' are 97-102.
            return val - (val < 58 ? 48 : val < 97 ? 55 : 87);
        }



        /// <summary>
        /// Converts a hex string to a byte array.
        /// </summary>
        /// <param name="hex">The hex string to convert.</param>
        /// <returns>A byte array representing the hex string.</returns>
        /// <exception cref="ArgumentException">Thrown when the hex string has an odd number of digits.</exception>
        public static byte[] HexToBytes(string hex)
        {
            if (hex.Length % 2 != 0)
            {
                throw new ArgumentException($"The hex string cannot have an odd number of digits: {hex}");
            }

            byte[] data = new byte[hex.Length / 2];
            for (int byteIndex = 0; byteIndex < data.Length; byteIndex++)
            {
                string byteValue = hex.Substring(byteIndex * 2, 2);
                data[byteIndex] = byte.Parse(byteValue, NumberStyles.HexNumber, CultureInfo.InvariantCulture);
            }

            return data;
        }



        public static string GenerateRandomHexString(int length)
        {
            if (length < 0) throw new ArgumentException("Length should be non-negative");

            byte[] buffer = new byte[length / 2];  // 2 hex digits represent one byte
            _random.NextBytes(buffer);

            var sb = new StringBuilder();
            foreach (byte b in buffer)
            {
                sb.Append(b.ToString("X2"));  // Convert byte to 2-digit hex
            }

            // If odd, add one more _random digit
            if (length % 2 != 0)
            {
                sb.Append(_random.Next(0, 16).ToString("X"));
            }

            return sb.ToString();
        }

        public static string BytesToAscii(byte[] buffer, bool keepSpacing = true)
        {
            var sb = new StringBuilder();

            foreach (byte b in buffer)
            {
                char c = (char)b; // Directly cast byte to char

                if (keepSpacing)
                {
                    switch (c)
                    {
                        case '\n': sb.Append("\\n"); continue;
                        case '\r': sb.Append("\\r"); continue;
                        case '\t': sb.Append("\\t"); continue;
                    }
                }

                // ASCII control characters are from 0x00 to 0x1F
                //sb.Append(c >= 0x00 && c <= 0x1F ? '.' : c);

                // Only append printable ASCII characters, replace anything else with '.'
                sb.Append((c >= 0x20 && c <= 0x7E) ? c : '.');
            }

            return sb.ToString();
        }



        //public static string BytesToEscapedString(byte[] buffer, bool keepSpacing = true, Encoding? encoding = null)
        //{
        //    encoding ??= Encoding.ASCII;
        //    string str = encoding.GetString(buffer);
        //    StringBuilder sb = new StringBuilder();

        //    foreach (char c in str)
        //    {
        //        if (keepSpacing)
        //        {
        //            switch (c)
        //            {
        //                case '\n': sb.Append("\\n"); continue;
        //                case '\r': sb.Append("\\r"); continue;
        //                case '\t': sb.Append("\\t"); continue;
        //            }
        //        }

        //        // ASCII control characters are from 0x00 to 0x1F
        //        sb.Append(c >= 0x00 && c <= 0x1F
        //            ? '.'
        //            : c);
        //    }

        //    return sb.ToString();
        //}

        /* if ((b >= 32 && b <= 126) || b == 9 || b == 10)  // ASCII printable characters + tab and newline
            {
                sb.Append((char)b);
            }
            else
            {
                sb.Append('.');
            }*/


        public static string GetXmlTemplateString(Type controlType)
        {
            var control = Application.Current.FindResource(controlType);

            var stringBuilder = new StringBuilder();
            var settings = new XmlWriterSettings
            {
                Indent = true,
                OmitXmlDeclaration = true

            };

            using (var xmlWriter = XmlWriter.Create(stringBuilder, settings))
            {
                XamlWriter.Save(control, xmlWriter);
            }

            return stringBuilder.ToString();
        }

        public static string GetCleanXmlTemplateString(Type controlType)
        {
            var xmlString = GetXmlTemplateString(controlType);

            string[] lines = xmlString.Split(new[] { Environment.NewLine }, StringSplitOptions.None);
            var stringBuilder = new StringBuilder();

#pragma warning disable SYSLIB1045 // Convert to 'GeneratedRegexAttribute'.
            for (int i = 0; i < lines.Length; i++)
            {
                string line = lines[i];

                line = Regex.Replace(line, @"FrameworkElement\.|UIElement\.", "");
                line = Regex.Replace(line, @"Panel\.Background", "Background");
                if (line.Contains("Property="))
                {
                    line = Regex.Replace(line, @"FrameworkElement\.|UIElement\.", "");
                }

                if (line.Contains("<Setter Property="))
                {
                    string nextLine1 = lines[i + 1];
                    string nextLine2 = lines[i + 2];
                    string nextLine3 = lines[i + 3];
                    string nextLine4 = lines[i + 4];

                    // Check if the next lines are formatted as expected
                    if (nextLine1.Contains("<Setter.Value>")
                        && nextLine3.Contains("</Setter.Value>")
                        && nextLine4.Contains("</Setter>"))
                    {

                        string value = string.Empty;
                        Match valueMatch;

                        // `<s:Boolean>True</s:Boolean>`
                        // `True`
                        valueMatch = Regex.Match(nextLine2, ">.*?<");
                        if (valueMatch.Success) value = valueMatch.Value[1..^1];

                        // `<SolidColorBrush Color="{DynamicResource ControlFillColorSecondary}" />`
                        // `{DynamicResource ControlFillColorSecondary}`
                        valueMatch = Regex.Match(nextLine2, "\"(\\{DynamicResource [^\"]+\\})\"");
                        if (valueMatch.Success)
                        {
                            //value = valueMatch.Groups[1].Value;

                            // skip. bug was casting color to brush
                            stringBuilder.AppendLine(line);
                            continue;
                        }

                        // `<SolidColorBrush Color="{StaticResource ControlFillColorSecondary}" />`
                        // `{StaticResource ControlFillColorSecondary}`
                        valueMatch = Regex.Match(nextLine2, "\"(\\{StaticResource [^\"]+\\})\"");
                        if (valueMatch.Success)
                        {
                            //value = valueMatch.Groups[1].Value;

                            // skip. bug was casting color to brush
                            stringBuilder.AppendLine(line);
                            continue;
                        }

                        // `<x:Static Member="Visibility.Visible" />`
                        // `Visible`
                        valueMatch = Regex.Match(nextLine2, @"<x:Static[^>]*=""[^.]*\.([^""]*)""\s*\/>");
                        if (valueMatch.Success) value = valueMatch.Groups[1].Value;

                        // `<DynamicResource ResourceKey="DefaultControlFocusVisualStyle" />`
                        // `{DynamicResource DefaultControlFocusVisualStyle}`
                        valueMatch = Regex.Match(nextLine2, @"DynamicResource ResourceKey=""([^""]+)""");
                        if (valueMatch.Success) value = "{DynamicResource " + $"{valueMatch.Groups[1].Value}" + "}";

                        if (string.IsNullOrEmpty(value))
                        {
                            throw new Exception("no value match!");
                        }

                        line = $"{line.TrimEnd('>')} Value=\"{value}\" />"; // Modify the current line
                        i += 4; // Skip the next 4 lines, as they've been condensed into this one
                    }
                    // Append the modified line to stringBuilder
                    stringBuilder.AppendLine(line);
                }
                else
                {
                    stringBuilder.AppendLine(line);
                }
            }
#pragma warning restore SYSLIB1045 // Convert to 'GeneratedRegexAttribute'.
            string modifiedXmlString = stringBuilder.ToString();
            return modifiedXmlString;
        }


        [DllImport("uxtheme.dll", CharSet = CharSet.Unicode)]
        private static extern int GetCurrentThemeName(StringBuilder pszThemeFileName, int cchMaxNameChars, StringBuilder pszColorBuff, int cchMaxColorChars, StringBuilder pszSizeBuff, int cchMaxSizeChars);


        public static Dictionary<string, string> GetCurrentWindowsTheme()
        {
            const int bufferSize = 260; // MAX_PATH
            var themeNameBuffer = new StringBuilder(bufferSize);
            var colorBuffer = new StringBuilder(bufferSize);
            var sizeBuffer = new StringBuilder(bufferSize);

            int error = GetCurrentThemeName(themeNameBuffer, themeNameBuffer.Capacity, colorBuffer, colorBuffer.Capacity, sizeBuffer, sizeBuffer.Capacity);

            if (error != 0)
            {
                Marshal.ThrowExceptionForHR(error);
            }

            return new Dictionary<string, string>
            {
                { "ThemeName", $"{themeNameBuffer}" },
                { "Color", $"{colorBuffer}" },
                { "Size", $"{sizeBuffer}" }
            };
        }


        public static List<string> GetCurrentWpfTheme()
        {
            var themeNames = new List<string>();

            foreach (Assembly assembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                if (assembly.GetName().Name is { } name && name.StartsWith("PresentationFramework"))
                {
                    themeNames.Add(assembly.Location);
                }
            }
            return themeNames;
        }

        /// <summary>
        /// Splits a JSON array string into a list of top-level JSON object strings.
        /// E.g., given input "[{}, {}, {}]", it returns a list ["{}", "{}", "{}"].
        /// </summary>
        /// <param name="json">The JSON array string to be split. Should be a well-formed JSON array string.</param>
        /// <returns>A list of individual JSON object strings extracted from the top-level array.</returns>
        public static List<string> SplitJsonObjects2(string json)
        {
            var result = new List<string>();
            int openBraces = 0;
            int startIndex = 0;

            for (int i = 0; i < json.Length; i++)
            {
                char c = json[i];

                switch (c)
                {
                    case '{':
                        if (openBraces == 0)
                        {
                            startIndex = i;
                        }
                        openBraces++;
                        break;
                    case '}':
                        openBraces--;
                        break;
                }

                if (openBraces == 0 && c == '}')
                {
                    result.Add(json.Substring(startIndex, i - startIndex + 1));
                }
            }

            return result;
        }

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

        public static string? ExtractHttpJsonObject(string json)
        {
            int httpIndex = json.IndexOf("\"http\": {");
            if (httpIndex == -1)
            {
                return null;
            }

            var (startIndex, endIndex) = FindJsonObjectBoundaries(json, httpIndex);
            return startIndex != -1 ? json.Substring(startIndex, endIndex - startIndex + 1) : null;
        }

        private static (int Start, int End) FindJsonObjectBoundaries(string json, int startingIndex)
        {
            int openBraces = 0;
            int startIndex = 0;
            int endIndex = 0;
            bool foundObject = false;

            for (int i = startingIndex; i < json.Length; i++)
            {
                char c = json[i];

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


        public static void ClearAllBindings(DependencyObject obj)
        {
            if (obj == null)
                return;

            foreach (var child in LogicalTreeHelper.GetChildren(obj))
            {
                if (child is not DependencyObject dp)
                    continue;

                BindingOperations.ClearAllBindings(dp);
                ClearAllBindings(dp);
            }
        }


        /// <summary>
        /// Finds the first child of a given type in the Visual Tree.
        /// </summary>
        /// <typeparam name="T">The type of the child to find.</typeparam>
        /// <param name="depObj">The root dependency object where the search starts.</param>
        /// <returns>The first child of the specified type, or null if no children of that type are found.</returns>
        public static T? FindVisualChild<T>(DependencyObject depObj) where T : DependencyObject
        {
            if (depObj == null) return null;

            for (int i = 0; i < VisualTreeHelper.GetChildrenCount(depObj); i++)
            {
                DependencyObject child = VisualTreeHelper.GetChild(depObj, i);
                if (child is T t)
                {
                    return t;
                }

                T? childOfChild = FindVisualChild<T>(child);
                if (childOfChild != null)
                {
                    return childOfChild;
                }
            }
            return null;
        }

        /// <summary>
        /// Handles the MouseDoubleClick event for a WPF TextBox. Selects the entire paragraph where the double-click occurs.
        /// </summary>
        /// <param name="sender">The TextBox control that raised the event.</param>
        /// <param name="e">The MouseButtonEventArgs containing the event data.</param>
        /// <remarks>
        /// A paragraph is defined as a block of text separated by blank lines. 
        /// The method marks the event as handled by setting e.Handled to true.
        /// </remarks>
        public static void DoubleClickSelectsParagraphBlock(object sender, MouseButtonEventArgs e)
        {
            if (sender is not System.Windows.Controls.TextBox tb) return;

            int caretLineIndex = tb.GetLineIndexFromCharacterIndex(tb.CaretIndex);
            int paragraphLineStartIndex = caretLineIndex;
            int paragraphLineEndIndex = caretLineIndex;

            // Find the start of the paragraph
            while (paragraphLineStartIndex >= 0 && !string.IsNullOrWhiteSpace(tb.GetLineText(paragraphLineStartIndex)))
            {
                paragraphLineStartIndex--;
            }

            // If we moved up, adjust the start index to the first line of the paragraph
            if (paragraphLineStartIndex != caretLineIndex)
                paragraphLineStartIndex++;

            // Find the end of the paragraph
            while (paragraphLineEndIndex < tb.LineCount && !string.IsNullOrWhiteSpace(tb.GetLineText(paragraphLineEndIndex)))
            {
                paragraphLineEndIndex++;
            }

            // If we moved down, adjust the end index to the last line of the paragraph
            if (paragraphLineEndIndex != caretLineIndex)
                paragraphLineEndIndex--;

            int paragraphStart = tb.GetCharacterIndexFromLineIndex(paragraphLineStartIndex);
            int paragraphEnd = tb.GetCharacterIndexFromLineIndex(paragraphLineEndIndex) + tb.GetLineText(paragraphLineEndIndex).TrimEnd('\r', '\n').Length;

            tb.Select(paragraphStart, paragraphEnd - paragraphStart);
            e.Handled = true;
        }


    }

}
