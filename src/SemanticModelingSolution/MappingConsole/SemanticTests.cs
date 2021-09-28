using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

using SemanticLibrary;

using SurrogateLibrary;

namespace MappingConsole
{
    internal class SemanticTests
    {
        public void Run()
        {
            AssignSemantic();
        }

        private void AssignSemantic()
        {
            var jsonDomainDefinitions = File.ReadAllText("Metadata\\domainDefinitions.json");
            var domain = JsonSerializer.Deserialize<DomainBase>(jsonDomainDefinitions);

            var ts = new TypeSystem<Metadata>();
            var order = ts.GetOrCreate(typeof(SimpleDomain1.Order));
            ts.UpdateCache();

            var analysis = new SemanticAnalysis2(domain);
            analysis.AssignSemantics(order);

        }


    }
}
