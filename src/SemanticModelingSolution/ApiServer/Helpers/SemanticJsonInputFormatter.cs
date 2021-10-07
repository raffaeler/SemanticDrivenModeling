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
    public class SemanticJsonInputFormatter : TextInputFormatter
    {
        private readonly MetadataService _metadataService;
        private readonly ILogger<SemanticJsonInputFormatter> _logger;
        private readonly JsonSerializerOptions _noTransformOptions;

        public SemanticJsonInputFormatter(MetadataService metadataService,
            ILogger<SemanticJsonInputFormatter> logger)
        {
            SupportedMediaTypes.Clear();

            SupportedMediaTypes.Add(new Microsoft.Net.Http.Headers.MediaTypeHeaderValue(
                "application/sdm.erpV1+json"));
            
            SupportedMediaTypes.Add(new Microsoft.Net.Http.Headers.MediaTypeHeaderValue(
                "application/sdm.erpV2+json"));

            SupportedEncodings.Add(UTF8EncodingWithoutBOM);
            SupportedEncodings.Add(UTF16EncodingLittleEndian);

            //SupportedMediaTypes.Add(MediaTypeHeaderValues.ApplicationJson);
            //SupportedMediaTypes.Add(MediaTypeHeaderValues.TextJson);
            //SupportedMediaTypes.Add(MediaTypeHeaderValues.ApplicationAnyJsonSyntax);

            _noTransformOptions = metadataService.JsonDefaultOptions;
            SerializerOptions = metadataService.JsonOptions;
            _metadataService = metadataService;
            _logger = logger;           
        }

        public JsonSerializerOptions SerializerOptions { get; }

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

        public async override Task<InputFormatterResult> ReadRequestBodyAsync(InputFormatterContext context, Encoding encoding)
        {
            if (context == null) throw new ArgumentNullException(nameof(context));

            var httpContext = context.HttpContext;
            //var encoding = SelectCharacterEncoding(context);
            var (inputStream, usesTranscodingStream) = GetInputStream(httpContext, encoding);

            var requestContentType = context.HttpContext.Request.ContentType;
            var requestMediaType = string.IsNullOrEmpty(requestContentType) ? default : new MediaType(requestContentType);

            object model;
            try
            {
                var serializerOptions = GetSerializerOptions(requestMediaType);
                model = await JsonSerializer.DeserializeAsync(inputStream, context.ModelType, SerializerOptions);
            }
            catch (JsonException jsonException)
            {
                var path = jsonException.Path;

                var formatterException = new InputFormatterException(jsonException.Message, jsonException);

                context.ModelState.TryAddModelError(path, formatterException, context.Metadata);

                _logger.LogError(jsonException, "Semantic input formatter failed");

                return InputFormatterResult.Failure();
            }
            catch (Exception exception) when (exception is FormatException || exception is OverflowException)
            {
                // The code in System.Text.Json never throws these exceptions.
                // However a custom converter could produce these errors for instance when
                // parsing a value. These error messages are considered safe to report to users using ModelState.

                context.ModelState.TryAddModelError(string.Empty, exception, context.Metadata);
                _logger.LogError(exception, "Semantic input formatter failed");

                return InputFormatterResult.Failure();
            }
            finally
            {
                if (usesTranscodingStream)
                {
                    await inputStream.DisposeAsync();
                }
            }

            if (model == null && !context.TreatEmptyInputAsDefaultValue)
            {
                // Some nonempty inputs might deserialize as null, for example whitespace,
                // or the JSON-encoded value "null". The upstream BodyModelBinder needs to
                // be notified that we don't regard this as a real input so it can register
                // a model binding error.
                return InputFormatterResult.NoValue();
            }
            else
            {
                _logger.LogInformation("Deserialization success on {0}", context.ModelType);
                return InputFormatterResult.Success(model);
            }
        }

        private (Stream inputStream, bool usesTranscodingStream) GetInputStream(HttpContext httpContext, Encoding encoding)
        {
            if (encoding.CodePage == Encoding.UTF8.CodePage)
            {
                return (httpContext.Request.Body, false);
            }

            var inputStream = Encoding.CreateTranscodingStream(httpContext.Request.Body, encoding, Encoding.UTF8, leaveOpen: true);
            return (inputStream, true);
        }

        private JsonSerializerOptions GetSerializerOptions(MediaType requestMediaType)
        {
            switch(requestMediaType.SubTypeWithoutSuffix.ToString())
            {
                case "sdm.erpV1":
                    return SerializerOptions;

                case "sdm.erpV2":
                    return SerializerOptions;

                default:
                    return _noTransformOptions;
            }
        }

    }
}
