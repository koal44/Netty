using System;
using System.Collections.Generic;
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
    public class WpfUtils
    {
        public static Style GetStyleFromType(Type controlType)
        {
            var controlStyle = Application.Current.FindResource(controlType);
            return (Style)controlStyle;
        }


        public static string GetXmlStyle(Style style)
        {
            var stringBuilder = new StringBuilder();
            var settings = new XmlWriterSettings
            {
                Indent = true,
                OmitXmlDeclaration = true
            };

            using (var xmlWriter = XmlWriter.Create(stringBuilder, settings))
            {
                XamlWriter.Save(style, xmlWriter);
            }

            return stringBuilder.ToString();
        }


        public static string GetCleanXml(string xmlString)
        {
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

                        if (nextLine2.Contains("<x:Null />")) value = "{x:Null}";

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
        /// <param name="sender">The TextBox controlStyle that raised the event.</param>
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
