using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using SemanticLibrary.Helpers;

namespace ManualMapping
{
    public class ModelNavigationNode : IEqualityComparer<ModelNavigationNode>, IName
    {
        public ModelNavigationNode(ModelPropertyNode modelPropertyNode, ModelNavigationNode previous)
        {
            ModelPropertyNode = modelPropertyNode;
            Previous = previous;
        }

        public ModelPropertyNode ModelPropertyNode { get; }
        public ModelNavigationNode Previous { get; }
        public string Name => ModelPropertyNode.Name;

        public bool Equals(ModelNavigationNode x, ModelNavigationNode y)
        {
            return x.ModelPropertyNode.Equals(y.ModelPropertyNode);
        }

        public int GetHashCode([DisallowNull] ModelNavigationNode obj)
        {
            return obj.ModelPropertyNode.GetHashCode();
        }

        public override int GetHashCode()
        {
            return ModelPropertyNode.GetHashCode();
        }

        public override string ToString()
        {
            if(Previous == null)
                return $"{ModelPropertyNode.OwnerTypeName}.{ModelPropertyNode.Name}";

            return $"{ModelPropertyNode.OwnerTypeName}.{ModelPropertyNode.Name} (from: {Previous?.ToString()})";
        }
    }
}
