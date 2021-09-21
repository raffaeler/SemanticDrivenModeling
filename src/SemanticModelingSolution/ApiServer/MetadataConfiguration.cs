using System.Collections.Generic;

namespace ApiServer
{
    public class MetadataConfiguration
    {
        public string DomainDefinitionsFile { get; set; }
        public Dictionary<string, string> DomainTypesFileMap { get; set; }
    }
}
