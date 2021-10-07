﻿using System;
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

        private string _jsonGetResponse;

        public SimpleHttpClient(ILogger<SimpleHttpClient> logger, HttpClient client)
        {
            _logger = logger;
            _client = client;
        }

        public async Task MakeGet()
        {
            //var mediaType = "application/json";
            //var mediaType = "application/sem.iot+json;domain=iamraf;version=1.0";

            try
            {
                using (var response = await _client.GetAsync("http://localhost:5000/Order"))
                {
                    response.EnsureSuccessStatusCode();
                    _jsonGetResponse = await response.Content.ReadAsStringAsync();
                }
            }
            catch (Exception err)
            {
                var errorText = err.ToString();
                Debug.WriteLine(errorText);

                return;
            }
        }

        public Task Resend()
        {
            return SendNew(_jsonGetResponse, "application/sdm.erpV1+json");
        }


        private async Task SendNew(string jsonMessage, string mediaType)
        {
            try
            {
                using (var content = new StringContent(jsonMessage, Encoding.UTF8, mediaType))
                {
                    using (var response = await _client.PostAsync("http://localhost:5000/Order", content))
                    {
                        response.EnsureSuccessStatusCode();
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
