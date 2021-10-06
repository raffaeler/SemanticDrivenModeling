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
        /// The "other" type of the conversion (T is the one owned by the running code)
        /// </summary>
        protected readonly SurrogateType<Metadata> _externalType;

        /// <summary>
        /// Important note: the instance of the JsonConverter is recycled during the same deserialization
        /// therefore anything that should not be recycled must be re-initialized in the Read/Write method
        /// </summary>
        public SemanticConverter(IEnumerable<TypeSystem<Metadata>> typeSystems, IEnumerable<Mapping> maps)
        {
            var fullTypeName = SurrogateType.GetFullName(typeof(T));
            var destinationTypeSystem = typeSystems.FirstOrDefault(t => t.Contains(typeof(T)));

            if (destinationTypeSystem == null)
            {
                throw new ArgumentException($"One of the type systems must contain the type {typeof(T).FullName}");
            }

            _serializationTypeSystem = destinationTypeSystem;
            _deserializationTypeSystem = destinationTypeSystem;

            Mapping map;
            map = maps.FirstOrDefault(m => m.Source.FullName == fullTypeName);
            if (map != null)
            {
                CanSerialize = true;
                _serializationLookup = map.CreateSerializationLookup();
                _externalType = map.Target;
            }

            map = maps.FirstOrDefault(m => m.Target.FullName == fullTypeName);
            if (map != null)
            {
                CanDeserialize = true;
                _deserializationLookup = map.CreateDeserializationLookup();
                _targetDeletablePaths = map.CreateDeletableLookup();
                _externalType = map.Source;
            }

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

        public bool CanSerialize { get; }
        public bool CanDeserialize { get; }
    }
}
