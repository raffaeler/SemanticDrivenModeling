using SemanticLibrary;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace SemanticStructuresTests.MappingUtilities
{
    public class MappingUtilities
    {
        public JsonSerializerOptions SettingsVanilla { get; } = new JsonSerializerOptions()
        {
            WriteIndented = true,
        };

        public Analyzer Analyzer { get; set; } = new Analyzer();

        public string GetJson<T>(IEnumerable<T> item) => JsonSerializer.Serialize(item, SettingsVanilla);

        public object FromJson(string json, Type type, JsonSerializerOptions options) => JsonSerializer.Deserialize(json, type, options);

        public JsonSerializerOptions CreateSettings(ScoredTypeMapping scoredTypeMapping)
            => new JsonSerializerOptions()
            {
                WriteIndented = true,
                Converters =
                {
                    new CodeGenerationLibrary.Serialization.TesterConverterFactory(scoredTypeMapping),
                },
            };


        public IEnumerable<SimpleDomain2.OnlineOrder> OrderToOnlineOrder(IList<ModelTypeNode> source, IList<ModelTypeNode> target,
            IEnumerable<SimpleDomain1.Order> sourceObjects)
        {
            var order = source.First(t => t.TypeName == "Order");
            var mapping = Analyzer.CreateMappingsFor(order, target);
            var settings = CreateSettings(mapping);

            var json = GetJson(sourceObjects);
            var clone = JsonSerializer.Deserialize(json, typeof(SimpleDomain1.Order[]));
            var targetObjects = (IEnumerable<SimpleDomain2.OnlineOrder>)FromJson(json, typeof(SimpleDomain2.OnlineOrder[]), settings);
            return targetObjects;
        }

        public IEnumerable<SimpleDomain1.Order> OnlineOrderToOrder(IList<ModelTypeNode> source, IList<ModelTypeNode> target,
            IEnumerable<SimpleDomain2.OnlineOrder> sourceObjects)
        {
            var onlineOrder = target.First(t => t.TypeName == "OnlineOrder");
            var mapping = Analyzer.CreateMappingsFor(onlineOrder, target);
            var settings = CreateSettings(mapping);

            var json = GetJson(sourceObjects);
            var clone = JsonSerializer.Deserialize(json, typeof(SimpleDomain2.OnlineOrder[]));
            var targetObjects = (IEnumerable<SimpleDomain1.Order>)FromJson(json, typeof(SimpleDomain1.Order[]), settings);
            return targetObjects;
        }


    }
}
