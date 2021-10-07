using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace ApiServer
{
    public class SemanticFormatterConfiguration : IConfigureOptions<MvcOptions>
    {
        private readonly MetadataService _metadataService;
        private readonly ILoggerFactory _factory;
        private readonly JsonOptions _jsonOptions;
        public SemanticFormatterConfiguration(MetadataService metadataService, /*JsonOptions options, */ILoggerFactory loggerFactory)
        {
            _metadataService = metadataService;
            _factory = loggerFactory;
            //_jsonOptions = options;
            _jsonOptions = new JsonOptions();
        }

        public void Configure(MvcOptions options)
        {
            var logger = _factory.CreateLogger<SemanticJsonInputFormatter>();
            var formatter = new SemanticJsonInputFormatter(_metadataService, _jsonOptions, logger);
            options.InputFormatters.Insert(0, formatter);
        }
    }

}
