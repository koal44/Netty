﻿using Netryoshka.Utils;
using Netryoshka.ViewModels;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using Wpf.Ui;
using Wpf.Ui.Controls;

namespace Netryoshka
{
    public partial class CapturePage
    {
        public CapturePageViewModel ViewModel { get; }
        private ScrollViewer? _capturedScrollViewer;

        public CapturePage(CapturePageViewModel viewModel, INavigationService navigationService)
        {
            ViewModel = viewModel;
            DataContext = ViewModel;
            InitializeComponent();

            //var nav = navigationService.GetNavigationControl();
            //nav.Navigated += OnNavigated;
            viewModel.PropertyChanged += ViewModel_CaptureTextChanged;
            this.Loaded += CapturePage_Loaded;
        }

        private void CapturePage_Loaded(object sender, RoutedEventArgs e)
        {
            _capturedScrollViewer = Util.FindVisualChild<ScrollViewer>(CapturedTextListBox);
        }

        private void ViewModel_CaptureTextChanged(object? sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(ViewModel.CapturedTextCollection))
            {
                _capturedScrollViewer?.ScrollToBottom();
            }
        }

        private void OnNavigated(object sender, NavigatedEventArgs e)
        {

        }

        private void CapturedTextDoubleClickHandler(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            Util.DoubleClickSelectsParagraphBlock(sender, e);
        }
    }
}
