using System;
using System.Windows;

namespace Netty.Services
{
    public class SimpleLogger : ILogger
    {
        private readonly bool _logToConsole;
        private readonly bool _logToPopup;

        public SimpleLogger(bool logToConsole, bool logToPopup)
        {
            _logToConsole = logToConsole;
            _logToPopup = logToPopup;
        }

        public void Info(string message)
        {
            if (_logToConsole)
            {
                System.Diagnostics.Debug.WriteLine(message);
            }

            if (_logToPopup)
            {
                MessageBox.Show(message, "Log FlowChatBubble", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        public void Warn(string message)
        {
            if (_logToConsole)
            {
                System.Diagnostics.Debug.WriteLine(message);
            }

            if (_logToPopup)
            {
                MessageBox.Show(message, "Log FlowChatBubble", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }


        public void Error(string message)
        {
            if (_logToConsole)
            {
                System.Diagnostics.Debug.WriteLine(message);
            }

            if (_logToPopup)
            {
                MessageBox.Show(message, "Log FlowChatBubble", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        public void Error(string message, Exception e)
        {
            string fullMessage = message + Environment.NewLine + "Exception: " + e.ToString();

            if (_logToConsole)
            {
                System.Diagnostics.Debug.WriteLine(fullMessage);
            }

            if (_logToPopup)
            {
                MessageBox.Show(message, "Log Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

    }
}
