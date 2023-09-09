using System;
using System.Windows;
using System.Windows.Markup;
using Wpf.Ui.Appearance;

namespace Netty
{
    /// <summary>
    /// Provides a dictionary implementation that contains <c>WPF UI</c> theme resources used by components and other elements of a WPF application.
    /// </summary>
    [Localizability(LocalizationCategory.Ignore)]
    [Ambient]
    [UsableDuringInitialization(true)]
    public class MyThemesDictionary : ResourceDictionary
    {
        /// <summary>
        /// Sets the default application theme.
        /// </summary>
        public ApplicationTheme ApplicationTheme
        {
            set
            {
                var themeName = value switch
                {
                    ApplicationTheme.Dark => "Dark",
                    ApplicationTheme.HighContrast => "HighContrast",
                    _ => "Light"
                };

                var libraryThemeDictionariesUri = "pack://application:,,,/Netryoshka;component/Skins/";

                Source = new Uri(
                    libraryThemeDictionariesUri + themeName + ".xaml",
                    UriKind.Absolute
                );
            }
        }
    }
}
