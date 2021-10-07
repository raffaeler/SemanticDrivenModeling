using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;

using Microsoft.AspNetCore.Mvc.Formatters;

namespace ApiServer
{
    public class SemanticJsonOutputFormatter : SystemTextJsonOutputFormatter
    {
        public SemanticJsonOutputFormatter(MetadataService metadataService) : base(metadataService.JsonOptions)
        {
            SupportedMediaTypes.Clear();

            SupportedMediaTypes.Add(new Microsoft.Net.Http.Headers.MediaTypeHeaderValue(
                "application/sdm.erpV1+json"));
            
            SupportedMediaTypes.Add(new Microsoft.Net.Http.Headers.MediaTypeHeaderValue(
                "application/sdm.erpV2+json"));
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
