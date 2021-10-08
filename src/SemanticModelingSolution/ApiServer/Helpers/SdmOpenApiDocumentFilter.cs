using System;
using System.Collections.Generic;

using Microsoft.OpenApi.Models;

using Swashbuckle.AspNetCore.SwaggerGen;

namespace ApiServer
{
    public class SdmOpenApiDocumentFilter : IDocumentFilter
    {
        private readonly string _version;
        private readonly Dictionary<string, OpenApiSchema> _types;

        public SdmOpenApiDocumentFilter(string version, Dictionary<string, OpenApiSchema> types)
        {
            _version = version;
            _types = types;
        }

        public void Apply(OpenApiDocument swaggerDoc, DocumentFilterContext context)
        {
            foreach (var type in _types)
            {
                swaggerDoc.Components.Schemas[type.Key] = type.Value;
            }
        }
    }

}
