using Netryoshka.Utils;
using Netryoshka.ViewModels;
using System;
using System.Windows.Input;
using Wpf.Ui;
using Wpf.Ui.Controls;

namespace Netryoshka
{
    public partial class CapturePage //: INavigableView<ViewModels.CapturePageViewModel>
    {
        public CapturePageViewModel ViewModel { get; }

        public CapturePage(CapturePageViewModel viewModel, INavigationService navigationService)
        {
            ViewModel = viewModel;
            DataContext = this;
            InitializeComponent();

            ViewModel.TransmitCaptureDataToView += CapturedPacketReceivedHandler;
            ViewModel.SendInstructionsToViewToClearCaptureData += ClearCapturedDataHandler;
            var nav = navigationService.GetNavigationControl();
            nav.Navigated += OnNavigated;
        }

        private void OnNavigated(object sender, NavigatedEventArgs e)
        {
            if (e.Page == this)
            {
                ViewModel.TransmitCaptureDataToView += CapturedPacketReceivedHandler;
            }
            else
            {
                ViewModel.TransmitCaptureDataToView -= CapturedPacketReceivedHandler;
            }
        }

        private void ClearCapturedDataHandler(object? sender, EventArgs e)
        {
            CapturedTextBox.Clear();
        }


        private void CapturedPacketReceivedHandler(object? sender, CapturePageViewModel.TransmitCapturedDataEventArgs packet)
        {
            CapturedTextBox.AppendText(packet.PacketString + Environment.NewLine + Environment.NewLine);
            CapturedTextScroller.ScrollToEnd();
        }


        private void CapturedPacketReceivedHandler2(object? sender, CapturePageViewModel.TransmitCapturedDataEventArgs packet)
        {
            var hex = packet.PacketString;
            //var messageType = HexFun.Read2Bytes(hex);
            
            if (HexFun.TryRead2Bytes(hex, out var messageType) && messageType == 1044)
            {
                // var id = HexFun.Read4Bytes(hex)
                if (HexFun.TryRead4Bytes(hex, out var id))
                {
                    var accessKey = HexFun.GetAccessKeyFromHexMessage(hex);
                    string txt = $"{id}{Environment.NewLine}{Environment.NewLine}{accessKey}{Environment.NewLine}{Environment.NewLine}";
                    CapturedTextBox.AppendText(txt);
                    CapturedTextScroller.ScrollToEnd();
                }
            }

        }


        private void DoubleClickSelectsParagraphBlock(object sender, MouseButtonEventArgs e)
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
