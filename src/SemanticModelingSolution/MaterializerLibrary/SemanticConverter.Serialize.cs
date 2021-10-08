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
using SemanticLibrary.Helpers;

using SurrogateLibrary;

namespace MaterializerLibrary
{
    public partial class SemanticConverter<T>
    {
        /// <summary>
        /// The type system containing the type definition to serialize
        /// </summary>
        protected TypeSystem<Metadata> _serializationTypeSystem;

        /// <summary>
        /// The serialization lookup map (taken from the target side of the map)
        /// </summary>
        protected IDictionary<string, NavigationPair> _serializationLookup;

        private static Dictionary<Type, Action<Utf8JsonWriter, T>> _writerCache = new();


        // serialization:
        // original object (and therefore 'T') is part of the _sourceTypeSystem
        // json will have the format of a type described in _targetTypeSystem
        public override void Write(Utf8JsonWriter writer, T value, JsonSerializerOptions options)
        {
            if (!CanSerialize)
            {
                JsonSerializer.Serialize(writer, value, CloneWithoutSelf(options));
                return;
            }

            if (!_writerCache.TryGetValue(typeof(T), out var transformDelegate))
            {
                var visitor = new SemanticSerializationVisitor<T>(_serializationTypeSystem, _serializationLookup,
                    _conversionGenerator);
                visitor.Visit(ExternalType);
                var expression = visitor.GetSerializationAction();
                transformDelegate = expression.Compile();
                _writerCache[typeof(T)] = transformDelegate;
            }

            transformDelegate(writer, value);
        }
    }
}
