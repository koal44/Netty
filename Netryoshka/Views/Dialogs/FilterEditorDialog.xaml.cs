using System.Windows;
using System.Windows.Controls;

namespace Netryoshka.Views.Dialogs
{
    /// <summary>
    /// Interaction logic for FilterEditorDialog.xaml
    /// </summary>
    public partial class FilterEditorDialog : UserControl
    {
        public static readonly DependencyProperty CustomFilterProperty =
        DependencyProperty.Register("CustomFilterProperty", typeof(string), typeof(FilterEditorDialog));

        public string CustomFilter
        {
            get { return (string)GetValue(CustomFilterProperty); }
            set { SetValue(CustomFilterProperty, value); }
        }

        public FilterEditorDialog()
        {
            InitializeComponent();
            CustomFilter = "tcp port 80";
        }
    }
}
