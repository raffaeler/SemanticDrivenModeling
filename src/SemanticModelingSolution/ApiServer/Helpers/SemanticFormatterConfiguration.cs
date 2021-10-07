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

        public SemanticFormatterConfiguration(MetadataService metadataService, ILoggerFactory loggerFactory)
        {
            _metadataService = metadataService;
            _factory = loggerFactory;
            _jsonOptions = new JsonOptions();
        }

        public void Configure(MvcOptions options)
        {
            var inputLogger = _factory.CreateLogger<SemanticJsonInputFormatter>();
            var outputLogger = _factory.CreateLogger<SemanticJsonOutputFormatter>();

            options.InputFormatters.Insert(0, new SemanticJsonInputFormatter(_metadataService, _jsonOptions, inputLogger));
            options.OutputFormatters.Insert(0, new SemanticJsonOutputFormatter(_metadataService, outputLogger));
        }
    }

}
