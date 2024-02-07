using Microsoft.Extensions.Logging;
using DBHSBells.Services;

namespace DBHSBells
{
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
    		builder.Logging.AddDebug();
#endif
            // need to inject the HttpClient into the BellScheduleService
            
            // Register HttpClient
            builder.Services.AddSingleton<HttpClient>();
            
            // Inject the HttpClient into the BellScheduleService
            builder.Services.AddSingleton<BellScheduleService>(sp => 
                new BellScheduleService(sp.GetRequiredService<HttpClient>()));

            return builder.Build();
        }
    }
}
