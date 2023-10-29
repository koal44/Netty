using Netryoshka.Extensions;
using Netryoshka.Services;
using Netryoshka.Utils;
using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Automation;
using System.Windows.Controls;
using System.Windows.Documents;

namespace Netryoshka
{
    public partial class DevPage
    {
        private readonly TSharkService _sharkService;
        public DevPage(TSharkService sharkService)
        {
            DataContext = this;
            InitializeComponent();
            _sharkService = sharkService;
        }

        private async void ButtonClickHandler(object sender, RoutedEventArgs e)
        {
            await _sharkService.WritePcapngWithKeys(new System.Collections.Generic.List<string>(), new System.Threading.CancellationToken());
            //Print(WpfUtils.GetCurrentWindowsTheme().PrettyPrint());
            //PrintList(WpfUtils.GetCurrentWpfTheme());
            //Print(WpfUtils.GetXmlStyle(WpfUtils.GetStyleFromType(typeof(Slider))));
            //Print(WpfUtils.GetCleanXml(WpfUtils.GetXmlStyle(WpfUtils.GetStyleFromType(typeof(TreeView)))));
            //Print($"{ReflectionUtils.GetDeclaringBaseTypeOfProperty(typeof(TreeView), "ThemeStyle")}");
            //var style = GetThemeStyle(typeof(TreeView));
            //var style = WpfUtils.GetStyleFromType(typeof(TreeView));
            //var xmlStyle = WpfUtils.GetCleanXml(WpfUtils.GetXmlStyle(style));
            //Print(xmlStyle);
        }

        public void Print(string? text)
        {
            TextBox.Text = text;
        }

        public void PrintList(List<string> list)
        {
            foreach (var item in list)
            {
                TextBox.Text += item + Environment.NewLine;
            }
        }

        public Style GetThemeStyle(Type controlType)
        {
            var control = (FrameworkElement?)Activator.CreateInstance(controlType)
                ?? throw new InvalidOperationException("could not create instance of control");
            control.Style = null;
            MainPanel.Children.Add(control);
            var style = ReflectionUtils.GetNonPublicProperty<Style>(control, "ThemeStyle")
                ?? throw new InvalidOperationException("could not get style");
            return style;
        }

    }
}
