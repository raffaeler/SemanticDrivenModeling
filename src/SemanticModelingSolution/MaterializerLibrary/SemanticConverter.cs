using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

using SemanticLibrary;
using SurrogateLibrary;

namespace MaterializerLibrary
{
    public partial class SemanticConverter<T> : JsonConverter<T>
    {
        private ConversionGenerator _conversionGenerator;

        /// <summary>
        /// Important note: the instance of the JsonConverter is recycled during the same deserialization
        /// therefore anything that should not be recycled must be re-initialized in the Read/Write method
        /// </summary>
        public SemanticConverter(SemanticConverterParameters parameters)
        {
            InternalType = parameters.InternalType;
            ExternalType = parameters.ExternalType;

            _serializationTypeSystem = parameters.TypeSystem;
            _deserializationTypeSystem = parameters.TypeSystem;

            CanSerialize = parameters.CanSerialize;
            CanDeserialize = parameters.CanDeserialize;

            _serializationLookup = parameters.SerializationLookup;
            _deserializationLookup = parameters.DeserializationLookup;
            _targetDeletablePaths = parameters.DeletableLookup;

            _conversionGenerator = new(parameters.ConversionContext);
        }

        /// <summary>
        /// The "other" type of the conversion (T is the one owned by the running code)
        /// </summary>
        public SurrogateType<Metadata> ExternalType { get; }

        /// <summary>
        /// The internal type (the 'T' / typeToConvert type)
        /// which is known from the CLR
        /// </summary>
        public SurrogateType<Metadata> InternalType { get; }

        /// <summary>
        /// The loaded map allows to serialize the type
        /// </summary>
        public bool CanSerialize { get; }

        /// <summary>
        /// The loaded map allows to deserialize the type
        /// </summary>
        public bool CanDeserialize { get; }

        private JsonSerializerOptions CloneWithoutSelf(JsonSerializerOptions options)
        {
            var emptyOptions = new JsonSerializerOptions(options);
            var todelete = emptyOptions.Converters.Where(c => c is SemanticConverterFactory).ToArray();
            foreach (var del in todelete)
            {
                emptyOptions.Converters.Remove(del);
            }

            return emptyOptions;
        }
    }
}
