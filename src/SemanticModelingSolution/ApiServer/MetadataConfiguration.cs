using System.Collections.Generic;

using SemanticLibrary;

using SurrogateLibrary;

namespace ApiServer
{
    public class MetadataConfiguration
    {
        /// <summary>
        /// The json filename containing the serialization of the domain
        /// </summary>
        public string DomainDefinitionsFile { get; set; }

        /// <summary>
        /// </summary>
        public List<string> TypeSystemFilenames { get; set; }

        /// <summary>
        /// </summary>
        public List<string> MappingFilenames { get; set; }
    }
}
