using Microsoft.AspNetCore.Components.WebView.Maui;
using Microsoft.Maui.LifecycleEvents;
using MqttMauiApp.Data;
using MqttMauiApp.Interfaces;
using MqttMauiApp.Model;

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
        MqttClientModel.MainDirPath = AppDomain.CurrentDomain.BaseDirectory;
#elif MACCATALYST
		builder.Services.AddTransient<IFolderPicker, Platforms.MacCatalyst.FolderPicker>();
         MqttClientModel.MainDirPath = AppDomain.CurrentDomain.BaseDirectory.Replace("Monobundle", "Resources");;
#endif

        builder.Services.AddTransient<MainPage>();
        builder.Services.AddTransient<App>();
        // Note: end added part

        builder.Services.AddSingleton<WeatherForecastService>();

        builder.ConfigureLifecycleEvents(AppLifecycle => {
#if WINDOWS
        AppLifecycle
         .AddWindows(windows =>
           windows.OnClosed((app, args) => {
             Serializer.SerializeList();
           }));
#elif ANDROID
            AppLifecycle.AddAndroid(android => android
               .OnStop((activity) => Serializer.SerializeList()));
#elif IOS || MACCATALYST
 AppLifecycle.AddiOS(ios => ios
          .WillTerminate((app) =>  Serializer.SerializeList())
       );
#endif
        });

        return builder.Build();
	}
}
