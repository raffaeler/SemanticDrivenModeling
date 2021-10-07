using System;
using Microsoft.OpenApi.Models;

using Swashbuckle.AspNetCore.SwaggerGen;

namespace ApiServer
{
    public class SdmOpenApiFilter : ISchemaFilter
    {
        private readonly MetadataService _metadataService;

        public SdmOpenApiFilter(MetadataService metadataService)
        {
            _metadataService = metadataService;
        }

        public void Apply(OpenApiSchema schema, SchemaFilterContext context)
        {
        }
    }
}
