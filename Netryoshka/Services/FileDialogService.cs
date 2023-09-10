using Microsoft.Win32;
using Netryoshka.Services;
using System;
using System.IO;
using System.Threading.Tasks;

namespace Netryoshka
{
    public class FileDialogService : IFileDialogService
    {
        private readonly ILogger _logger;

        public FileDialogService(ILogger logger)
        {
            _logger = logger;
        }

        public async Task ShowOpenDialogAndExecuteAction(string filter, string defaultExt, string title, Func<string, Task> loadAction)
        {
            OpenFileDialog openFileDialog = new()
            {
                Filter = filter,
                DefaultExt = defaultExt,
                Title = title,
            };

            if (openFileDialog.ShowDialog() == true)
            {
                try
                {
                    await loadAction(openFileDialog.FileName);
                    await ShowMessageAsync("", "File loaded successfully!");
                }
                catch (UnauthorizedAccessException ex)
                {
                    await ShowMessageAsync("Permission Denied", "Permission denied. Please select a different file or run the application as an administrator.");
                    _logger.Error(ex.Message);
                }
                catch (FileNotFoundException ex)
                {
                    await ShowMessageAsync("File Not Found", "The selected file was not found. It may have been moved or deleted.");
                    _logger.Error(ex.Message);
                }
                catch (FileFormatException ex)
                {
                    await ShowMessageAsync("Invalid File", "The selected file is not in a supported format or may be corrupted.");
                    _logger.Error(ex.Message);
                }
                catch (IOException ex)
                {
                    await ShowMessageAsync("IO Error", $"An error occurred while reading the file: {ex.Message}");
                    _logger.Error(ex.Message);
                }
                catch (Exception ex)
                {
                    await ShowMessageAsync("Error", $"An unexpected error occurred: {ex.Message}");
                    _logger.Error(ex.Message);
                }

            }
        }


        public async Task ShowSaveDialogAndExecuteAction(string filter, string defaultExt, string title, string fileName, Func<string, Task> saveAction)
        {
            SaveFileDialog saveFileDialog = new()
            {
                Filter = filter,
                DefaultExt = defaultExt,
                Title = title,
                FileName = fileName
            };

            if (saveFileDialog.ShowDialog() == true)
            {
                try
                {
                    await saveAction(saveFileDialog.FileName);
                    await ShowMessageAsync("", "File saved successfully!");
                    //return saveFileDialog.FileName;
                }
                catch (UnauthorizedAccessException)
                {
                    await ShowMessageAsync("Error", "Permission denied. Please select a different location or run the application as an administrator.");
                }
                catch (IOException ex)
                {
                    await ShowMessageAsync("Error", $"An error occurred while saving the file: {ex.Message}");
                    _logger.Error(ex.Message);
                }
                catch (Exception ex)
                {
                    await ShowMessageAsync("Error", $"An unexpected error occurred: {ex.Message}");
                    _logger.Error(ex.Message);
                }
            }
            //return null;
        }


        private static async Task ShowMessageAsync(string title, string content)
        {
            _ = await new Wpf.Ui.Controls.MessageBox
            {
                Title = title,
                Content = content
            }.ShowDialogAsync();
        }
    }
}
