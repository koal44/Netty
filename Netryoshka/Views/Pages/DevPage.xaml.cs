using Netryoshka.Extensions;
using Netryoshka.Services;
using Netryoshka.Utils;
using System;
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
            //Print(Util.GetCurrentWindowsTheme().PrettyPrint());
            //Print(Util.GetCurrentWpfTheme());
            //Print(Util.GetXmlStyle(typeof(Slider)));
            //Print(Util.GetCleanXmlTemplateString(typeof(TreeView)));
            //Print($"{Util.GetDeclaringBaseTypeOfProperty(typeof(TreeView), "ThemeStyle")}");
            //var style = GetThemeStyle(typeof(TreeView));
            //var style = Util.GetStyleFromType(typeof(TreeView));
            //var xmlStyle = Util.GetCleanXml(Util.GetXmlStyle(style));
            //Print(xmlStyle);
        }

        public void Print(string? text)
        {
            TextBox.Text = text;
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
