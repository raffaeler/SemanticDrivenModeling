using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using SemanticLibrary.Helpers;

namespace SemanticLibrary
{
    public class ModelTypeNode : IModelNode
    {
        //private Type _type;

        // default constructor is intended only for the serializer
        public ModelTypeNode()
        {
        }

        public ModelTypeNode(Type type, IList<TermToConcept> termToConcepts)
        {
            Type = new(type);
            //AssemblyName = type.Assembly.GetName().Name;
            //TypeName = type.Name;
            //TypeFullName = type.FullName;
            //UniqueTypeName = type.GetUniqueTypeName();
            TermToConcepts = termToConcepts;
        }

        public SurrogateType Type { get; set; }

        //public string AssemblyName { get; set; }
        //public string TypeName { get; set; }
        //public string TypeFullName { get; set; }

        ///// <summary>
        ///// This only works when the Type is already loaded in memory
        ///// otherwise an Exception will be thrown;
        ///// </summary>
        //public Type GetOriginalType() => _type ??= TypeHelper.GetEntityType(TypeFullName, AssemblyName);

        //public object CreateInstanceOfOriginalType() => Activator.CreateInstance(GetOriginalType());

        ///// <summary>
        ///// This is used in dictionaries to ensure uniqueness
        ///// It was "AssemblyQualifiedName" and replaced with "FullName" to avoid dependencies on the assembly name
        ///// By using "FullName", if this is used to create a type, it will succeed only if the assembly is already in memory
        ///// We now use the GetUniqueTypeName() extension method in order to rollback, if needed, to "AssemblyQualifiedName"
        ///// </summary>
        //public string UniqueTypeName { get; set; }

        public IList<TermToConcept> TermToConcepts { get; set; }
        public IEnumerable<Concept> CandidateConcepts => TermToConcepts.Select(c => c.Concept);
        public IEnumerable<string> CandidateConceptNames => TermToConcepts.Select(c => c.Concept.Name);

        public IList<ModelPropertyNode> PropertyNodes { get; set; } = new List<ModelPropertyNode>();

        public override string ToString()
        {
            var concepts = string.Join(", ", CandidateConceptNames);
            return $"{Type.Name} [{concepts}]";
        }
    }
}
