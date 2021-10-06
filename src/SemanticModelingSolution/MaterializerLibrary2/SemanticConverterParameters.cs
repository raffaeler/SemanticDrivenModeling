using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using ConversionLibrary;

using SemanticLibrary;

using SurrogateLibrary;

namespace MaterializerLibrary
{
    public class SemanticConverterParameters
    {
        public SemanticConverterParameters(Type typeToConvert,
            IEnumerable<TypeSystem<Metadata>> typeSystems,
            IEnumerable<Mapping> maps,
            ConversionContext conversionContext)
        {
            var fullTypeName = SurrogateType.GetFullName(typeToConvert);
            TypeSystem = typeSystems.FirstOrDefault(t => t.Contains(typeToConvert));

            if (TypeSystem == null || !TypeSystem.TryGetSurrogateTypeByName(fullTypeName, out var internalType))
            {
                throw new ArgumentException($"One of the type systems must contain the type {typeToConvert.FullName}");
            }

            this.InternalType = internalType;

            Mapping map;
            map = maps.FirstOrDefault(m => m.Source.FullName == fullTypeName);
            if (map != null)
            {
                CanSerialize = true;
                SerializationLookup = map.CreateSerializationLookup();
                ExternalType = map.Target;
            }

            map = maps.FirstOrDefault(m => m.Target.FullName == fullTypeName);
            if (map != null)
            {
                CanDeserialize = true;
                DeserializationLookup = map.CreateDeserializationLookup();
                DeletableLookup = map.CreateDeletableLookup();
                ExternalType = map.Source;
            }

            this.ConversionContext = conversionContext;
        }

        /// <summary>
        /// A lookup created to optimize the serialization
        /// </summary>
        public Dictionary<string, NavigationPair> SerializationLookup { get; }

        /// <summary>
        /// A lookup created to optimize the deserialization
        /// </summary>
        public Dictionary<string, List<NavigationPair>> DeserializationLookup { get; }

        /// <summary>
        /// This is a map created to optimize the removal of certain objects during
        /// the deserialization process
        /// </summary>
        public IDictionary<string, HashSet<string>> DeletableLookup { get; }

        /// <summary>
        /// It is true when there is a map that is able to provide
        /// the mappings to serialize the InternalType to ExternalType
        /// </summary>
        public bool CanSerialize {  get; }

        /// <summary>
        /// It is true when there is a map that is able to provide
        /// the mappings to deserialize the ExternalType into InternalType
        /// </summary>
        public bool CanDeserialize {  get; }

        /// <summary>
        /// The abstraction over the type system. It is the one where
        /// the InternalType is defined.
        /// It is needed to retrieve reflection metadata when generating
        /// code
        /// </summary>
        public TypeSystem<Metadata> TypeSystem { get; }

        /// <summary>
        /// The context for data type conversions
        /// This is used to hook the conversions during the serialization
        /// and deserialization processes.
        /// </summary>
        public ConversionContext ConversionContext { get; }

        /// <summary>
        /// The type that is known to the CLR of the current process
        /// During serialization is the source type of the map or "T"
        /// During deserialization is the target type of the map
        /// </summary>
        public SurrogateType<Metadata> InternalType { get; }

        /// <summary>
        /// The type represented by the JSON document
        /// Usually it is not known as a System.Type by the current process
        /// therefore it is only available as a SurrogateType
        /// </summary>
        public SurrogateType<Metadata> ExternalType { get; }
    }
}
