using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using SemanticLibrary.Helpers;

namespace SemanticLibrary
{
    /// <summary>
    /// This class is used to preserve a specific "navigation" between a property and the type it points to
    /// We want to be able to navigate the graph back, not considering all the possible navigation but
    /// just the one we were drilling down into
    /// 
    /// A potential flaw is where a type (or its children) can navigate to a type passing by two different path
    /// In public models this is usually a remote possibility but we may want to address this hypthesis in future releases
    /// </summary>
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

        public int GetHashCode(ModelNavigationNode obj)
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
