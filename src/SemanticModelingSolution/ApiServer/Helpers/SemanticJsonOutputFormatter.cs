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
    public class SemanticJsonOutputFormatter : TextOutputFormatter//SystemTextJsonOutputFormatter
    {
        private readonly ILogger<SemanticJsonOutputFormatter> _logger;
        private readonly JsonSerializerOptions _noTransformOptions;

        public SemanticJsonOutputFormatter(MetadataService metadataService, ILogger<SemanticJsonOutputFormatter> logger)
        //: base(metadataService.JsonOptions)
        {
            SupportedMediaTypes.Clear();

            SupportedMediaTypes.Add(new Microsoft.Net.Http.Headers.MediaTypeHeaderValue(
                "application/sdm.erpV1+json"));

            SupportedMediaTypes.Add(new Microsoft.Net.Http.Headers.MediaTypeHeaderValue(
                "application/sdm.erpV2+json"));

            SupportedEncodings.Add(Encoding.UTF8);
            SupportedEncodings.Add(Encoding.Unicode);
            //SupportedMediaTypes.Add(MediaTypeHeaderValues.ApplicationJson);
            //SupportedMediaTypes.Add(MediaTypeHeaderValues.TextJson);
            //SupportedMediaTypes.Add(MediaTypeHeaderValues.ApplicationAnyJsonSyntax);

            _logger = logger;

            _noTransformOptions = metadataService.JsonDefaultOptions;
            SerializerOptions = metadataService.JsonOptions;
        }

        public JsonSerializerOptions SerializerOptions { get; }

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

        public override async Task WriteResponseBodyAsync(OutputFormatterWriteContext context, Encoding selectedEncoding)
        {
            if (context == null)
            {
                throw new ArgumentNullException(nameof(context));
            }

            if (selectedEncoding == null)
            {
                throw new ArgumentNullException(nameof(selectedEncoding));
            }

            var httpContext = context.HttpContext;

            // context.ObjectType reflects the declared model type when specified.
            // For polymorphic scenarios where the user declares a return type, but returns a derived type,
            // we want to serialize all the properties on the derived type. This keeps parity with
            // the behavior you get when the user does not declare the return type and with Json.Net at least at the top level.
            var objectType = context.Object?.GetType() ?? context.ObjectType ?? typeof(object);


            if (!context.HttpContext.Request.Headers.TryGetValue("Accept", out var acceptValues))
            {
                acceptValues = new Microsoft.Extensions.Primitives.StringValues("application/json");
            }

            var responseStream = httpContext.Response.Body;
            if (selectedEncoding.CodePage == Encoding.UTF8.CodePage)
            {
                var serializerOptions = GetSerializerOptions(acceptValues);
                await JsonSerializer.SerializeAsync(responseStream, context.Object, objectType, serializerOptions);
                await responseStream.FlushAsync();
            }
            else
            {
                // JsonSerializer only emits UTF8 encoded output, but we need to write the response in the encoding specified by
                // selectedEncoding
                var transcodingStream = Encoding.CreateTranscodingStream(httpContext.Response.Body, selectedEncoding, Encoding.UTF8, leaveOpen: true);

                System.Runtime.ExceptionServices.ExceptionDispatchInfo exceptionDispatchInfo = null;
                try
                {
                    var serializerOptions = GetSerializerOptions(acceptValues);
                    await JsonSerializer.SerializeAsync(transcodingStream, context.Object, objectType, serializerOptions);
                    await transcodingStream.FlushAsync();
                }
                catch (Exception ex)
                {
                    // TranscodingStream may write to the inner stream as part of it's disposal.
                    // We do not want this exception "ex" to be eclipsed by any exception encountered during the write. We will stash it and
                    // explicitly rethrow it during the finally block.
                    exceptionDispatchInfo = System.Runtime.ExceptionServices.ExceptionDispatchInfo.Capture(ex);
                }
                finally
                {
                    try
                    {
                        await transcodingStream.DisposeAsync();
                    }
                    catch when (exceptionDispatchInfo != null)
                    {
                    }

                    exceptionDispatchInfo?.Throw();
                }
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

        private JsonSerializerOptions GetSerializerOptions(Microsoft.Extensions.Primitives.StringValues acceptValues)
        {
            if (acceptValues.Any(a => a.Contains("sdm.erpV2"))) return SerializerOptions;
            if (acceptValues.Any(a => a.Contains("sdm.erpV1"))) return SerializerOptions;

            return _noTransformOptions;
        }


    }
}
