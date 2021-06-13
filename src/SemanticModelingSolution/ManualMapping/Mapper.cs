using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

using LegacyModels;

using SemanticLibrary;
using SemanticLibrary.Known;
using GeneratedCode;

namespace ManualMapping
{
    public class Mapper
    {
        //Scope global = new Scope(
        //    new Imply(KnownTerms.Id, KnownConcepts.UniqueIdentity),
        //    new Imply(KnownTerms.Name, KnownConcepts.Identity)
        //    //new Imply(KnownTerms.Title, KnownConcepts.Identity)
        //    );

        //Scope scopeProduct = new Scope(KnownConcepts.Product,
        //    new Imply(KnownTerms.Name, KnownConcepts.Identity),
        //    new Imply(KnownTerms.Quantity, KnownConcepts.QuantitativeMeasure)
        //    );

        

        public void BuildMap()
        {
            //var result = MapBuilder<Article>.Build()
            //    .Map(a => a.Id, KnownConcepts.Identity)
            //    //.Map(a => a.ArticleClass, KnownConcepts.
            //    ;


        }

        //public static Concept Excipient = new Concept("Excipient", "");

        public List<TermsToConcept> Links { get; } = new()
        {
            //new TermsToConcept(KnownConcepts.Identity, (KnownTerms.Name, 80), (KnownTerms.Name, 80)),
        };

        public void BuildConceptGraph()
        {
            //var builder = GraphBuilder.Build()
            //    .Connect(KnownConcepts.Product, KnownConcepts.Product, 100)
            //    ;
        }

    }

    public class GraphBuilder
    {
        private GraphBuilder()
        {
        }

        public ConceptGraph ConceptGraph { get; } = new ConceptGraph();

        public static GraphBuilder Build()
        {
            var instance = new GraphBuilder();
            return instance.BuildInternal();
        }

        private GraphBuilder BuildInternal()
        {
            return this;
        }

        public GraphBuilder Connect(Concept from, Concept to, int weight)
        {
            ConceptGraph.Connect(from, to, weight);
            return this;
        }
    }


    public class MapBuilder<T> where T : class
    {
        private BindingFlags _flags = BindingFlags.FlattenHierarchy | BindingFlags.Public;
        private Dictionary<string, PropertyMap> _properties;

        private MapBuilder()
        {
        }

        public static MapBuilder<T> Build()
        {
            var mapBuilder = new MapBuilder<T>();
            return mapBuilder.BuildInternal();
        }

        private MapBuilder<T> BuildInternal()
        {
            var properties = typeof(T).GetProperties(_flags);

            _properties = properties
                .ToDictionary(p => p.Name, p => new PropertyMap(p));

            return this;
        }

        public MapBuilder<T> Map(Expression<Func<T, object>> selector, Concept concept)
        {
            var property = selector.ReturnType;
            var propertyName = property.Name;
            if (!_properties.TryGetValue(propertyName, out PropertyMap propertyMap))
            {
                throw new Exception("Can't find any property in the map");
            }

            propertyMap.Concept = concept;
            return this;
        }

    }

    public class PropertyMap
    {
        public PropertyMap(PropertyInfo propertyInfo, Concept concept = null)
        {
            this.PropertyInfo = propertyInfo;
            this.Concept = concept;
        }

        public PropertyInfo PropertyInfo { get; init; }
        public Concept Concept { get; set; }
        public bool IsMapped => Concept != null;
    }

}
