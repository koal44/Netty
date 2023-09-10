using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace Netryoshka
{
    public partial class MatryoshkaBox : UserControl
    {
        public  MatryoshkaBoxViewModel ViewModel { get; }

        public MatryoshkaBox()
        {
            ViewModel = new MatryoshkaBoxViewModel();
            DataContext = ViewModel;
            InitializeComponent();
        }

        public FlowsPageViewModel? ParentViewModel
        {
            get => (FlowsPageViewModel?)GetValue(ParentViewModelProperty);
            set => SetValue(ParentViewModelProperty, value);
        }

        public static readonly DependencyProperty ParentViewModelProperty =
            DependencyProperty.Register("ParentViewModel", typeof(FlowsPageViewModel), typeof(MatryoshkaBox), new PropertyMetadata(null, OnParentViewModelChanged));

        private static void OnParentViewModelChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var control = (MatryoshkaBox)d;
            control.SetupViewModel((FlowsPageViewModel)e.NewValue);
        }

        private void SetupViewModel(FlowsPageViewModel? parentViewModel)
        {
            ViewModel.ParentViewModel = parentViewModel;
            ViewModel.CurrentLayerOnDisplay = $"{parentViewModel?.SelectedNetworkLayer}";
        }



        //public static readonly DependencyProperty TextProperty =
        //    DependencyProperty.Register("Text", typeof(string), typeof(MyUserControl),
        //        new FrameworkPropertyMetadata(string.Empty, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));

        //public string Text
        //{
        //    get { return (string)GetValue(TextProperty); }
        //    set { SetValue(TextProperty, value); }
        //}

















        public void SetLayerOnMouseEnter(object sender, MouseEventArgs e)
        {
            if (sender is Button button && button.Tag is string tag)
            {
                ViewModel.CurrentLayerOnDisplay = tag;
            }
        }

        public void ClearLayerOnMouseExit(object sender, MouseEventArgs e)
        {
            ViewModel.CurrentLayerOnDisplay = $"{ParentViewModel?.SelectedNetworkLayer}";
        }

    }

}
