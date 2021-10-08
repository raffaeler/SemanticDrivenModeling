using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Formatters;
using Microsoft.Extensions.Logging;

namespace ApiServer
{
    public class SemanticJsonOutputFormatter : BaseJsonOutputFormatter
    {
        private readonly MetadataService _metadataService;
        private readonly ILogger<SemanticJsonOutputFormatter> _logger;

        public SemanticJsonOutputFormatter(MetadataService metadataService,
            ILogger<SemanticJsonOutputFormatter> logger) : base(logger, metadataService.JsonDefaultOptions)
        {
            _metadataService = metadataService;
            _logger = logger;

            SupportedMediaTypes.Clear();

            SupportedMediaTypes.Add(new Microsoft.Net.Http.Headers.MediaTypeHeaderValue(
                "application/sdm.erpV1+json"));

            SupportedMediaTypes.Add(new Microsoft.Net.Http.Headers.MediaTypeHeaderValue(
                "application/sdm.erpV2+json"));
        }

        protected override bool CanWriteType(Type type)
        {
            return base.CanWriteType(type);
        }

        public override IReadOnlyList<string> GetSupportedContentTypes(string contentType, Type objectType)
        {
            return new[]
            {
                "application/sdm.erpV1+json",
                "application/sdm.erpV2+json"
            };
        }

        protected override JsonSerializerOptions GetSerializerOptions(Microsoft.Extensions.Primitives.StringValues acceptValues)
        {
            if (acceptValues.Any(a => a.Contains("sdm.erpV2"))) return _metadataService.JsonOptions;
            if (acceptValues.Any(a => a.Contains("sdm.erpV1"))) return _metadataService.JsonOptions;

            return SerializerOptions;
        }


    }
}
