using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

using Microsoft.Extensions.Logging;

namespace ApiConsoleClient
{
    public class SimpleHttpClient
    {
        private readonly ILogger<SimpleHttpClient> _logger;
        private readonly HttpClient _client;

        public SimpleHttpClient(ILogger<SimpleHttpClient> logger, HttpClient client)
        {
            _logger = logger;
            _client = client;
        }

        public async Task MakeGet()
        {
            try
            {
                using (var response = await _client.GetAsync("http://www.google.com"))
                {
                    response.EnsureSuccessStatusCode();
                }
            }
            catch (Exception err)
            {
                var errorText = err.ToString();
                Debug.WriteLine(errorText);

                return;
            }
        }

        private async Task SendReuse(string textMessage)
        {
            try
            {
                var message = $"\"{textMessage}\"";

                using (var content = new StringContent(message, Encoding.UTF8, "application/json"))
                {
                    using (var response = await _client.PostAsync("http://localhost:5000/Logger", content))
                    {
                        response.EnsureSuccessStatusCode();
                    }
                }
            }
            catch (Exception err)
            {
                var errorText = err.ToString();
                Debug.WriteLine(errorText);

                await _client.PostAsync("Logger",
                    new StringContent(errorText, Encoding.UTF8, "application/json"));
            }
        }

        // This does not cause the exceptions
        private async Task SendNew(string textMessage)
        {
            try
            {
                var message = $"\"{textMessage}\"";

                using (var freshClient = new HttpClient())
                {
                    using (var content = new StringContent(message, Encoding.UTF8, "application/json"))
                    {
                        using (var response = await freshClient.PostAsync("http://localhost:5000/Logger", content))
                        {
                            response.EnsureSuccessStatusCode();
                        }
                    }
                }
            }
            catch (Exception err)
            {
                var errorText = err.ToString();
                Debug.WriteLine(errorText);
            }
        }

    }
}
