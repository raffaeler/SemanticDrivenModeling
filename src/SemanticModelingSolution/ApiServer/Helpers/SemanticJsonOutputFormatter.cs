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
    /// <summary>
    /// This formatter deals with the Serialization process
    /// The entity provided by the controller is serialized into the
    /// json document with the foreign format (GET)
    /// </summary>
    public class SemanticJsonOutputFormatter : BaseJsonOutputFormatter
    {
        private readonly MetadataService _metadataService;
        private readonly ILogger<SemanticJsonOutputFormatter> _logger;
        private IDictionary<string, JsonSerializerOptions> _options;

        public SemanticJsonOutputFormatter(MetadataService metadataService,
            ILogger<SemanticJsonOutputFormatter> logger) : base(logger, metadataService.JsonDefaultOptions)
        {
            _metadataService = metadataService;
            _logger = logger;
            _options = metadataService.GetSerializationSettings();

            SupportedMediaTypes.Clear();
            foreach (var opt in _options)
            {
                var mediaTypeString = $"application/{opt.Key}+json";
                SupportedMediaTypes.Add(new Microsoft.Net.Http.Headers.MediaTypeHeaderValue(mediaTypeString));
            }
        }

        /// <summary>
        /// The type matches the one specified in the controller (action return type which is an output)
        /// It can be a list, an IEnumerable of something or whatelse
        /// </summary>
        protected override bool CanWriteType(Type type)
        {
            return base.CanWriteType(type);
        }

        public override IReadOnlyList<string> GetSupportedContentTypes(string contentType, Type objectType)
        {
            return base.GetSupportedContentTypes(contentType, objectType);
        }

        protected override JsonSerializerOptions GetSerializerOptions(IList<Microsoft.Net.Http.Headers.MediaTypeHeaderValue> accepts)
        {
            foreach (var accept in accepts)
            {
                if (string.Compare(accept.Type.Value, "application", true) != 0 ||
                    string.Compare(accept.Suffix.Value, "json", true) != 0) continue;

                if (_options.TryGetValue(accept.SubTypeWithoutSuffix.Value, out var serializationOptions))
                    return serializationOptions;
            }

            return SerializerOptions;
        }
    }
}
