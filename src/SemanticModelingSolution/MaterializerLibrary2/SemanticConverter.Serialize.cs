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
        // ScoredPropertyMapping<ModelNavigationNode> => NavigationPair
        protected Dictionary<string, NavigationPair> _targetLookup = new();

        private static Dictionary<Type, Action<Utf8JsonWriter, T>> _writerCache = new();

        // serialization:
        // original object (and therefore 'T') is part of the _sourceTypeSystem
        // json will have the format of a type described in _targetTypeSystem
        public override void Write(Utf8JsonWriter writer, T value, JsonSerializerOptions options)
        {
            if (!_writerCache.TryGetValue(typeof(T), out var transformDelegate))
            {
                var visitor = new SemanticSerializationVisitor<T>(_sourceTypeSystem, _targetLookup, _conversionGenerator, _map);
                visitor.Visit(_map.Target);
                var expression = visitor.GetSerializationAction();
                transformDelegate = expression.Compile();
                _writerCache[typeof(T)] = transformDelegate;
            }

            transformDelegate(writer, value);
        }
    }
}
