using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Formatters;
using Microsoft.Extensions.Logging;

namespace ApiServer
{
    public class SemanticJsonInputFormatter : SystemTextJsonInputFormatter//TextInputFormatter
    {
        private readonly MetadataService _metadataService;

        public SemanticJsonInputFormatter(MetadataService metadataService,
            JsonOptions jsonOptions, ILogger<SemanticJsonInputFormatter> logger)
            : base(jsonOptions, logger)
        {
            jsonOptions.JsonSerializerOptions.Converters.Add(metadataService.JsonConverterFactory);

            SupportedMediaTypes.Clear();

            SupportedMediaTypes.Add(new Microsoft.Net.Http.Headers.MediaTypeHeaderValue(
                "application/sdm.erpV1+json"));
            
            SupportedMediaTypes.Add(new Microsoft.Net.Http.Headers.MediaTypeHeaderValue(
                "application/sdm.erpV2+json"));
            _metadataService = metadataService;
        }

        public override bool CanRead(InputFormatterContext context)
            => SupportedMediaTypes.Any(m => context.HttpContext.Request.ContentType.StartsWith(m));

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
