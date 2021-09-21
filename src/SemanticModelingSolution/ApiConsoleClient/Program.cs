using System;
using System.Text;

using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace ApiConsoleClient
{
    internal class Program
    {
        static void Main(string[] args)
        {
            Console.OutputEncoding = Encoding.UTF8;
            CreateHostBuilder(args).Build().Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)

        .UseConsoleLifetime()   // Ctrl-C

        .ConfigureLogging(options =>
        {
            // Microsoft.Extensions.Logging
            options.ClearProviders();
            options.AddDebug();
        })

        .ConfigureAppConfiguration(config =>
        {
        })

        .ConfigureServices((hostContext, services) =>
        {
            //services.Configure<MyConfig>(hostContext.Configuration.GetSection("MyConfig"));

            var generalSection = hostContext.Configuration.GetSection("General");
            services.Configure<GeneralConfig>(generalSection);
            var baseUrl = generalSection["BaseUrl"];

            services.AddHttpClient<SimpleHttpClient>("Simple", client =>
            {
                client.BaseAddress = new Uri(baseUrl);
                client.DefaultRequestHeaders.Add("Accept", "application/json");
                client.DefaultRequestHeaders.Add("User-Agent", "ApiConsoleClient 1.0");
            })
            .AddHttpMessageHandler(() =>
            {
                return new HttpLogger();
            });

            services.AddHostedService<Worker>();
        });

    }
}
