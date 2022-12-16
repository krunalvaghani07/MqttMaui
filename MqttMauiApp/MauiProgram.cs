using Microsoft.AspNetCore.Components.WebView.Maui;
using MqttMauiApp.Data;
using MqttMauiApp.Interfaces;

namespace MqttMauiApp;

public static class MauiProgram
{
	public static MauiApp CreateMauiApp()
	{
		var builder = MauiApp.CreateBuilder();
		builder
			.UseMauiApp<App>()
			.ConfigureFonts(fonts =>
			{
				fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
			});

		builder.Services.AddMauiBlazorWebView();
		#if DEBUG
		builder.Services.AddBlazorWebViewDeveloperTools();
#endif

#if WINDOWS
		builder.Services.AddTransient<IFolderPicker, Platforms.Windows.FolderPicker>();
#elif MACCATALYST
		builder.Services.AddTransient<IFolderPicker, Platforms.MacCatalyst.FolderPicker>();
#endif

        builder.Services.AddTransient<MainPage>();
        builder.Services.AddTransient<App>();
        // Note: end added part

        builder.Services.AddSingleton<WeatherForecastService>();

		return builder.Build();
	}
}
