using System;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Markup;
using System.Xml;

namespace Netryoshka
{
    public partial class DevPage
    {
        public DevPage()
        {
            DataContext = this;
            InitializeComponent();
        }

        public static string GetTemplateString()
        {
            var control = Application.Current.FindResource(typeof(Slider));

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

        public static string CleanXmlString(string xmlString)
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


        private void PrintTemplateClickHandler(object sender, RoutedEventArgs e)
        {
            var xamlString = GetTemplateString();
            xamlString = CleanXmlString(xamlString);
            OutputRichTextBox.Document.Blocks.Clear();
            OutputRichTextBox.Document.Blocks.Add(new Paragraph(new Run(xamlString)));
        }

    }
}
