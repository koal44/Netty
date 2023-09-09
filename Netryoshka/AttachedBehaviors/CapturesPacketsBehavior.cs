using System;
using System.Windows.Controls;

namespace Proton
{
    public class CapturesPacketsBehavior : AttachedBehaviorsBase<TextBox, CapturesPacketsBehavior>
    {
        protected override void OnBehaviorAttached(TextBox element)
        {

            if (element.DataContext is CapturePage view && view.ViewModel != null)
            {
                //view.ViewModel.TransmitCaptureDataToView += CapturedPacketReceivedHandler;
            }
        }

        protected override void OnBehaviorDetached(TextBox element)
        {
            throw new NotImplementedException();
        }

        private void CapturedPacketReceivedHandler(object? sender, string packet)
        {
            var textbox = sender as TextBox;
            if (textbox != null)
            {
                textbox.AppendText(packet + Environment.NewLine);
                var scrollviewer = textbox.Parent as ScrollViewer;
                if (scrollviewer != null)
                {
                    scrollviewer.ScrollToEnd();
                }
            }
        }
    }
}
