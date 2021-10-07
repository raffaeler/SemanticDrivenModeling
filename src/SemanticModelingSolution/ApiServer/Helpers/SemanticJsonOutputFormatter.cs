using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;

using Microsoft.AspNetCore.Mvc.Formatters;
using Microsoft.Extensions.Logging;

namespace ApiServer
{
    public class SemanticJsonOutputFormatter : SystemTextJsonOutputFormatter
    {
        private readonly ILogger<SemanticJsonOutputFormatter> _logger;

        public SemanticJsonOutputFormatter(MetadataService metadataService, ILogger<SemanticJsonOutputFormatter> logger)
            : base(metadataService.JsonOptions)
        {
            SupportedMediaTypes.Clear();

            SupportedMediaTypes.Add(new Microsoft.Net.Http.Headers.MediaTypeHeaderValue(
                "application/sdm.erpV1+json"));
            
            SupportedMediaTypes.Add(new Microsoft.Net.Http.Headers.MediaTypeHeaderValue(
                "application/sdm.erpV2+json"));

            _logger = logger;
        }

        public override bool CanWriteResult(OutputFormatterCanWriteContext context)
            => SupportedMediaTypes.Any(m => m == context.ContentType);

        public override IReadOnlyList<string> GetSupportedContentTypes(string contentType, Type objectType)
        {
            return new[]
            {
                "application/sdm.erpV1+json",
                "application/sdm.erpV2+json"
            };
        }
    }
}
