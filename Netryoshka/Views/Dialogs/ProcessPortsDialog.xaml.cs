using System.Data.Common;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using Wpf.Ui.Controls;

namespace Netryoshka.Views
{
    /// <summary>
    /// Interaction logic for ProcessPortsDialog.xaml
    /// </summary>
    public partial class ProcessPortsDialog : ContentDialog
    {
        public string? SelectedPropertyName { get; private set; }
        public string? SelectedValue { get; private set; }

        public ProcessPortsDialog(ContentPresenter contentPresenter) 
            : base(contentPresenter)
        {
            InitializeComponent();
        }


        private void MouseUp_SelectFilter(object sender, MouseButtonEventArgs e)
        {
            if (sender is not System.Windows.Controls.DataGrid dataGrid) return;

            var currentCell = dataGrid.CurrentCell;
            var columnBinding = (currentCell.Column as DataGridBoundColumn)?.Binding as Binding;

            SelectedPropertyName = columnBinding?.Path.Path; // e.g. "LocalPort"

            var currentTcpRecord = dataGrid.CurrentItem as TcpProcessRecord;

            if (SelectedPropertyName != null && currentTcpRecord != null)
            {
                PropertyInfo? propertyInfo = currentTcpRecord.GetType().GetProperty(SelectedPropertyName);
                SelectedValue = propertyInfo?.GetValue(currentTcpRecord)?.ToString();
            }

            // Set the result of the task completion source
            Tcs?.SetResult(ContentDialogResult.Primary);
        }

        //protected override void OnButtonClick(ContentDialogButton button)
        //{
        //    base.OnButtonClick(button);
        //    return;
            
        //}

    }
}
