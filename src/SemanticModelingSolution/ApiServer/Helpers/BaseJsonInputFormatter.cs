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
    public class BaseJsonInputFormatter : TextInputFormatter
    {
        private readonly ILogger<SemanticJsonInputFormatter> _logger;

        public BaseJsonInputFormatter(ILogger<SemanticJsonInputFormatter> logger,
            JsonSerializerOptions defaultOptions)
        {
            _logger = logger;
            SerializerOptions = defaultOptions;

            SupportedEncodings.Add(UTF8EncodingWithoutBOM);
            SupportedEncodings.Add(UTF16EncodingLittleEndian);

            SupportedMediaTypes.Clear();
            SupportedMediaTypes.Add(StandardMediaTypes.ApplicationJson);
            SupportedMediaTypes.Add(StandardMediaTypes.TextJson);
            SupportedMediaTypes.Add(StandardMediaTypes.ApplicationAnyJsonSyntax);
        }

        public virtual JsonSerializerOptions SerializerOptions { get; }

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
                var options = GetSerializerOptions(requestMediaType);
                model = await JsonSerializer.DeserializeAsync(inputStream, context.ModelType, options);
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

        protected virtual JsonSerializerOptions GetSerializerOptions(MediaType requestMediaType)
        {
            return SerializerOptions;
        }

    }
}
