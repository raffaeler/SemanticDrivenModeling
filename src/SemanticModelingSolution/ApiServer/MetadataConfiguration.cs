using System.Collections.Generic;

namespace ApiServer
{
    public class MetadataConfiguration
    {
        /// <summary>
        /// The json filename containing the serialization of the domain
        /// </summary>
        public string DomainDefinitionsFile { get; set; }

        /// <summary>
        /// A dictionary containing:
        /// key => the friendly name of the domain
        /// value => the json filename containing the serialization of the list of ModelTypeNode
        /// </summary>
        public Dictionary<string, string> DomainTypes { get; set; }

        /// <summary>
        /// A dictionary containing:
        /// key => the full name (Namespace.Name) of the source type
        /// value => the json filename containing the serialization of a ScoredTypeMapping
        /// </summary>
        public Dictionary<string, string> TypeMappings { get; set; }
    }
}
