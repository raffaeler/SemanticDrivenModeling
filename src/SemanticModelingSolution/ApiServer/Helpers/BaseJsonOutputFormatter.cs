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
    public class BaseJsonOutputFormatter : TextOutputFormatter
    {
        private readonly ILogger<SemanticJsonOutputFormatter> _logger;

        public BaseJsonOutputFormatter(ILogger<SemanticJsonOutputFormatter> logger,
            JsonSerializerOptions defaultOptions)
        {
            _logger = logger;
            SerializerOptions = defaultOptions;

            SupportedEncodings.Add(Encoding.UTF8);
            SupportedEncodings.Add(Encoding.Unicode);

            SupportedMediaTypes.Clear();
            SupportedMediaTypes.Add(StandardMediaTypes.ApplicationJson);
            SupportedMediaTypes.Add(StandardMediaTypes.TextJson);
            SupportedMediaTypes.Add(StandardMediaTypes.ApplicationAnyJsonSyntax);
        }

        public JsonSerializerOptions SerializerOptions { get; }

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
            var options = GetSerializerOptions(acceptValues);
            if (selectedEncoding.CodePage == Encoding.UTF8.CodePage)
            {
                await JsonSerializer.SerializeAsync(responseStream, context.Object, objectType, options);
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
                    await JsonSerializer.SerializeAsync(transcodingStream, context.Object, objectType, options);
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

        protected virtual JsonSerializerOptions GetSerializerOptions(Microsoft.Extensions.Primitives.StringValues acceptValues)
        {
            return SerializerOptions;
        }
    }
}
