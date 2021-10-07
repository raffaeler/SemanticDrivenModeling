using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace ApiServer
{
    /// <summary>
    /// This configuration class is used to delay the configuration of
    /// the input formatter which requires the logger (not available
    /// in the Startup.ConfigureServices)
    /// </summary>
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

            options.InputFormatters.Insert(0, new SemanticJsonInputFormatter(_metadataService, inputLogger));
            options.OutputFormatters.Insert(0, new SemanticJsonOutputFormatter(_metadataService, outputLogger));
        }
    }

}
