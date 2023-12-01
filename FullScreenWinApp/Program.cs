using ImagesAdvanced;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace FullScreenWinApp
{
    internal static class Program
    {
        private static IHost? _host;

        public static IServiceProvider Provider => _host!.Services;

        /// <summary>
        ///  The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            _host = Host.CreateDefaultBuilder().ConfigureServices( (context, services) =>
            {
                services
                .AddSingleton<PreviewForm>()
                .AddSingleton<BrowserWithRotationCache>();

                services.Configure<BrowserOptions>(context.Configuration.GetSection("browser"));
                

            }).Build();


            // To customize application configuration such as set high DPI settings or default font,
            // see https://aka.ms/applicationconfiguration.
            ApplicationConfiguration.Initialize();
            Application.Run(new Form1());
        }
    }
}