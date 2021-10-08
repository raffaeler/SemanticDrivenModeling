using System;
using System.Collections.Generic;
using System.Linq;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Microsoft.OpenApi.Models;

using Swashbuckle.AspNetCore.SwaggerGen;
using SurrogateLibrary;
using SemanticLibrary;

namespace ApiServer
{
    /// <summary>
    /// This class configures OpenApi / swagger by adding the "projected" types
    /// to the UI created by Swashbuckle.
    /// The controller emits orders of type SimpleModel1.Order
    /// The client requests a media type "application/sdm.Erp.OnlineOrder.onlineorder_ts_v2+json"
    /// The server output formatter instruct the serialization to output SimpleModel2.OnlineOrder
    /// Since the OnlineOrder is just a different representation of the same data "dressed" as Order,
    /// the media type is appropriate to request a different type
    /// For this reason the Swagger configuration shows both the entities in the same document.
    /// </summary>
    public class SemanticSwaggerConfiguration : IConfigureOptions<SwaggerGenOptions>
    {
        private readonly MetadataService _metadataService;

        public SemanticSwaggerConfiguration(MetadataService metadataService)
        {
            _metadataService = metadataService;
        }

        public void Configure(SwaggerGenOptions options)
        {
            var typeSystemIdentifier = "onlineorder_ts_v2";
            var ts = _metadataService.TypeSystems
                .FirstOrDefault(ts => ts.Identifier == typeSystemIdentifier);

            var types = ts.Types.Where(ts => !ts.Value.IsBasicType).ToList();

            Dictionary<string, OpenApiSchema> typeDefinitions = new();
            foreach (var type in types)
            {
                if (type.Value.IsCollection()) continue;
                var schema = CreateTypeOpenApiSchema(ts, type.Value);
                typeDefinitions[type.Value.Name] = schema;
            }

            options.DocumentFilter<SdmOpenApiDocumentFilter>(ts.Identifier, typeDefinitions);
        }

        private OpenApiSchema CreateTypeOpenApiSchema(TypeSystem<Metadata> typeSystem, SurrogateType<Metadata> type)
        {
            var props = new Dictionary<string, OpenApiSchema>();
            foreach (var prop in type.Properties.Values)
            {
                var propName = prop.Name;
                var propType = prop.PropertyType;
                OpenApiSchema schema = null;

                if (propType.IsBasicType)
                {
                    schema = new OpenApiSchema { Type = propType.Name.ToLower() };
                }
                else if (propType.IsCollection())
                {
                    var collectedItem = prop.PropertyType.GetInnerType1(typeSystem);
                    schema = new OpenApiSchema()
                    {
                        Type = "array",
                        Items = new OpenApiSchema()
                        {
                            Reference = new OpenApiReference()
                            {
                                Id = collectedItem.Name,
                                Type = ReferenceType.Schema,
                            }
                        }
                    };

                }

                if (schema != null) props[propName] = schema;
            }

            OpenApiSchema typeSchema = new();
            typeSchema.Type = "object";
            typeSchema.Properties = props;
            return typeSchema;
        }
    }
}

