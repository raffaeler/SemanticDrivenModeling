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

namespace MaterializerLibrary
{
    public partial class SemanticConverter<T>
    {
        protected Dictionary<string, ScoredPropertyMapping<ModelNavigationNode>> _targetLookup = new();

        private static Dictionary<Type, Action<Utf8JsonWriter, T>> _writerCache = new();

        public override void Write(Utf8JsonWriter writer, T value, JsonSerializerOptions options)
        {
            if (!_writerCache.TryGetValue(typeof(T), out var transformDelegate))
            {
                var visitor = new SemanticSerializationVisitor<T>(_targetLookup, _conversionGenerator, _map);
                visitor.Visit(_map.TargetModelTypeNode);
                var expression = visitor.GetSerializationAction();
                transformDelegate = expression.Compile();
                _writerCache[typeof(T)] = transformDelegate;
            }

            transformDelegate(writer, value);
        }
    }
}
