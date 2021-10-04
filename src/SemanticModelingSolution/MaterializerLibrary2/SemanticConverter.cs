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
        protected Mapping _map;
        private ConversionGenerator _conversionGenerator;

        /// <summary>
        /// Important note: the instance of the JsonConverter is recycled during the same deserialization
        /// therefore anything that should not be recycled must be re-initialized in the Read/Write method
        /// </summary>
        public SemanticConverter(TypeSystem<Metadata> destinationTypeSystem, Mapping map)
        {
            if (!destinationTypeSystem.Contains(typeof(T)))
            {
                throw new ArgumentException($"The type system must contain the type {typeof(T).FullName}");
            }
            
            _serializationTypeSystem = destinationTypeSystem;
            _deserializationTypeSystem = destinationTypeSystem;
            _map = map;
            _sourceTypeName = _map.Source.Name;

            (_sourceDeserializationLookup, _targetSerializationLookup) = _map.CreateLookups();
            _targetDeletablePaths = _map.CreateDeletableLookup();


            var context = new ConversionLibrary.ConversionContext()
            {
                OnNotSupported = (converter, value) =>
                {
#if DEBUG
                    Console.WriteLine($"Conversion of a value from {value} To {converter.TargetType.Name} is not supported");
#endif
                    return converter.TargetType.GetDefaultValue();
                },
            };

            _conversionGenerator = new(context);   // the new is here in order to recycle the generator cache
        }
    }
}
