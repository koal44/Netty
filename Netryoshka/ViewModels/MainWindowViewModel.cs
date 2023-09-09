using System.ComponentModel;
using System.Windows.Input;

namespace Netty
{
    public class MainWindowViewModel
    {

        private static void SwitchThemes()
        {
            var currentTheme = Wpf.Ui.Appearance.ApplicationThemeManager.GetAppTheme();
            var newTheme = currentTheme == Wpf.Ui.Appearance.ApplicationTheme.Light
                    ? Wpf.Ui.Appearance.ApplicationTheme.Dark
                    : Wpf.Ui.Appearance.ApplicationTheme.Light;
            var backgroundEffect = Wpf.Ui.Controls.WindowBackdropType.Mica;
            bool updateAccent = true;
            bool forceBackground = false;

            Wpf.Ui.Appearance.ApplicationThemeManager.Apply(newTheme, backgroundEffect, updateAccent, forceBackground);
        }

        public ICommand SwitchThemesCommand { get; }

        public MainWindowViewModel()
        {
            SwitchThemesCommand = new SimpleRelayCommand(
                execute: _ => SwitchThemes(),
                canExecute: _ => true
            );
        }


    }
}
