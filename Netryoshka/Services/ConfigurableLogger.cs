using System;
using System.Diagnostics;
using System.Windows;

namespace Netryoshka.Services
{
    public class ConfigurableLogger : ILogger
    {
        [Flags]
        public enum LogTarget
        {
            None = 0,
            Console = 1,
            Popup = 2
        }

        public class LoggerConfiguration
        {
            public LogTarget InfoTarget { get; set; }
            public LogTarget ErrorTarget { get; set; }
        }

        private readonly LoggerConfiguration _config;

        public ConfigurableLogger(LoggerConfiguration config)
        {
            _config = config;
        }

        public void Info(string message)
        {
            Log(message, "Log Info", _config.InfoTarget, MessageBoxImage.Information);
        }
        public void Warn(string message)
        {
            Log(message, "Log Info", _config.InfoTarget, MessageBoxImage.Warning);
        }

        public void Error(string message)
        {
            Log(message, "Log Error", _config.ErrorTarget, MessageBoxImage.Error);
        }

        public void Error(string message, Exception e)
        {
            var fullMessage = message + Environment.NewLine + "Exception: " + e.ToString();
            Log(fullMessage, "Log Info", _config.InfoTarget, MessageBoxImage.Warning);
        }

        private void Log(string message, string title, LogTarget target, MessageBoxImage image)
        {
            if (target.HasFlag(LogTarget.Console))
            {
                Debug.WriteLine(message);
            }

            if (target.HasFlag(LogTarget.Popup))
            {
                MessageBox.Show(message, title, MessageBoxButton.OK, image);
            }
        }
    }
}
