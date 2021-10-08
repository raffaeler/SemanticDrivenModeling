using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Formatters;
using Microsoft.Extensions.Logging;

namespace ApiServer
{
    public class SemanticJsonInputFormatter : BaseJsonInputFormatter
    {
        private readonly MetadataService _metadataService;
        private readonly ILogger<SemanticJsonInputFormatter> _logger;

        public SemanticJsonInputFormatter(MetadataService metadataService,
            ILogger<SemanticJsonInputFormatter> logger) : base(logger, metadataService.JsonDefaultOptions)
        {
            _metadataService = metadataService;
            _logger = logger;

            SupportedEncodings.Add(UTF8EncodingWithoutBOM);
            SupportedEncodings.Add(UTF16EncodingLittleEndian);

            SupportedMediaTypes.Clear();

            SupportedMediaTypes.Add(new Microsoft.Net.Http.Headers.MediaTypeHeaderValue(
                "application/sdm.erpV1+json"));
            
            SupportedMediaTypes.Add(new Microsoft.Net.Http.Headers.MediaTypeHeaderValue(
                "application/sdm.erpV2+json"));
        }

        protected override bool CanReadType(Type type)
        {
            // is this type available in the metadata service?
            return base.CanReadType(type);
        }

        public override IReadOnlyList<string> GetSupportedContentTypes(string contentType, Type objectType)
        {
            return new[]
            {
                "application/sdm.erpV1+json",
                "application/sdm.erpV2+json"
            };
        }

        protected override JsonSerializerOptions GetSerializerOptions(MediaType requestMediaType)
        {
            switch(requestMediaType.SubTypeWithoutSuffix.ToString())
            {
                case "sdm.erpV1":
                    return _metadataService.JsonOptions;

                case "sdm.erpV2":
                    return _metadataService.JsonOptions;

                default:
                    return SerializerOptions;
            }
        }

    }
}
