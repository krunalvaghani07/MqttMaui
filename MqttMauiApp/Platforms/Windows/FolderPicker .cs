using MqttMauiApp.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage.Pickers;
using WindowsFolderPicker = Windows.Storage.Pickers.FolderPicker;

namespace MqttMauiApp.Platforms.Windows
{
    public class FolderPicker : IFolderPicker
    {
        public async Task<string> PickFolder()
        {
            var folderPicker = new WindowsFolderPicker();
            // Might be needed to make it work on Windows 10
            folderPicker.FileTypeFilter.Add("*");

            // Get the current window's HWND by passing in the Window object
            var hwnd = ((MauiWinUIWindow)App.Current.Windows[0].Handler.PlatformView).WindowHandle;

            // Associate the HWND with the file picker
            WinRT.Interop.InitializeWithWindow.Initialize(folderPicker, hwnd);

            var result = await folderPicker.PickSingleFolderAsync();

            return result?.Path;
        }
        public async ValueTask SaveFileAsync(string filename, Stream stream)
        {
            var extension = Path.GetExtension(filename);

            var fileSavePicker = new FileSavePicker();
            fileSavePicker.SuggestedFileName = filename;
            fileSavePicker.FileTypeChoices.Add(extension, new List<string> { extension });

            if (MauiWinUIApplication.Current.Application.Windows[0].Handler.PlatformView is MauiWinUIWindow window)
            {
                WinRT.Interop.InitializeWithWindow.Initialize(fileSavePicker, window.WindowHandle);
            }

            var result = await fileSavePicker.PickSaveFileAsync();
            if (result != null)
            {
                using (var fileStream = await result.OpenStreamForWriteAsync())
                {
                    fileStream.SetLength(0); // override
                    await stream.CopyToAsync(fileStream);
                }
            }
        }
    }
}
