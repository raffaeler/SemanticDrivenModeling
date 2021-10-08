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
    /// <summary>
    /// This formatter deals with the Deserialization process
    /// A json document in a "foreign" model is deserialized into the
    /// model managed by the controller being served (POST)
    /// </summary>
    public class SemanticJsonInputFormatter : BaseJsonInputFormatter
    {
        private readonly MetadataService _metadataService;
        private readonly ILogger<SemanticJsonInputFormatter> _logger;
        private IDictionary<string, JsonSerializerOptions> _options;

        public SemanticJsonInputFormatter(MetadataService metadataService,
            ILogger<SemanticJsonInputFormatter> logger) : base(logger, metadataService.JsonDefaultOptions)
        {
            _metadataService = metadataService;
            _logger = logger;
            _options = metadataService.GetDeserializationSettings();

            SupportedEncodings.Add(UTF8EncodingWithoutBOM);
            SupportedEncodings.Add(UTF16EncodingLittleEndian);

            SupportedMediaTypes.Clear();
            foreach (var opt in _options)
            {
                var mediaTypeString = $"application/{opt.Key}+json";
                SupportedMediaTypes.Add(new Microsoft.Net.Http.Headers.MediaTypeHeaderValue(mediaTypeString));
            }
        }

        /// <summary>
        /// The type matches the one specified in the controller (action parameter which is an input)
        /// </summary>
        protected override bool CanReadType(Type type)
        {
            return base.CanReadType(type);
        }

        public override IReadOnlyList<string> GetSupportedContentTypes(string contentType, Type objectType)
        {
            return base.GetSupportedContentTypes(contentType, objectType);
        }

        protected override JsonSerializerOptions GetSerializerOptions(MediaType requestMediaType)
        {
            if (_options.TryGetValue(requestMediaType.SubTypeWithoutSuffix.Value, out var serializationOptions))
                return serializationOptions;

            return SerializerOptions;
        }

    }
}
