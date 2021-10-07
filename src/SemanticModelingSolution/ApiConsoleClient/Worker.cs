using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace ApiConsoleClient
{
    internal class Worker : BackgroundService
    {
        private readonly ILogger<Worker> _logger;
        private readonly IHostApplicationLifetime _hostApplicationLifetime;
        private SimpleHttpClient _client;

        public Worker(ILogger<Worker> logger,
            IHostApplicationLifetime hostApplicationLifetime,
            SimpleHttpClient client)
        {
            _logger = logger;
            _hostApplicationLifetime = hostApplicationLifetime;
            _hostApplicationLifetime.ApplicationStarted.Register(OnApplicationStarted);
            _hostApplicationLifetime.ApplicationStopping.Register(OnApplicationStopping);

            _client = client;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                if (Console.KeyAvailable)
                {
                    var keyInfo = Console.ReadKey(true);
                    if (keyInfo.Key == ConsoleKey.Q)
                    {
                        await StopAsync(stoppingToken);
                        return;
                    }

                    if (keyInfo.Key == ConsoleKey.G)
                    {
                        await _client.MakeGet();
                    }

                    if (keyInfo.Key == ConsoleKey.P)
                    {
                        await _client.Resend();
                    }

                    if (keyInfo.Key == ConsoleKey.R)
                    {
                        //await foreach (var tweet in _client.GetTweetsStreamAsync("raffaeler"))
                        //{
                        //    Console.WriteLine($"{tweet.user.name}: {tweet.text}");
                        //    if (Console.KeyAvailable && Console.ReadKey(true).Key == ConsoleKey.Escape)
                        //        break;
                        //}
                    }

                    Dispatch(keyInfo);
                }

                _logger.LogInformation("Worker running at: {time}", DateTimeOffset.Now);
                await Task.Delay(1000, stoppingToken);
            }
        }

        public override Task StopAsync(CancellationToken cancellationToken)
        {
            var baseres = base.StopAsync(cancellationToken);
            _hostApplicationLifetime.StopApplication();
            return baseres;
        }

        protected void OnApplicationStarted()
        {
            Console.WriteLine("Application Started");
        }

        protected void OnApplicationStopping()
        {
            Console.WriteLine("Application is stopping");
        }

        protected void Dispatch(ConsoleKeyInfo keyInfo)
        {
        }

    }
}
