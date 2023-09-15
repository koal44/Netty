using Netryoshka.Extensions;
using Netryoshka.Utils;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;

namespace Netryoshka
{
    public partial class DevPage
    {
        public DevPage()
        {
            DataContext = this;
            InitializeComponent();
        }

        private void ButtonClickHandler(object sender, RoutedEventArgs e)
        {
            Print(Util.GetCurrentWindowsTheme().PrettyPrint());
            //Print(Util.GetCurrentWpfTheme());
            //Print(Util.GetXmlTemplateString(typeof(Slider)));
            //Print(Util.GetCleanXmlTemplateString(typeof(Slider)));
        }

        public void Print(string? text)
        {
            TextBox.Text = text;
        }

    }
}
