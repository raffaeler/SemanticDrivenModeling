using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ManualMapping
{
    /// <summary>
    /// A visitor which avoids the risk of circular references
    /// when walking through a graph of IModelNode elements (types and properties)
    /// </summary>
    public static class ModelNodeHelpers
    {
        public static IEnumerable<ModelPropertyNode> FlatHierarchyProperties(this ModelTypeNode node)
        {
            Dictionary<string, ModelTypeNode> visited = new();
            return FlatHierarchyPropertiesInternal(node, visited);
        }

        private static IEnumerable<ModelPropertyNode> FlatHierarchyPropertiesInternal(ModelTypeNode node, Dictionary<string, ModelTypeNode> visited)
        {
            if (!visited.TryGetValue(node.Type.AssemblyQualifiedName, out _))
            {
                visited[node.Type.AssemblyQualifiedName] = node;
                foreach (var propertyNode in node.PropertyNodes)
                {
                    if (propertyNode.NavigationNode == null)
                    {
                        yield return propertyNode;
                        continue;
                    }

                    foreach(var sub in FlatHierarchyPropertiesInternal(propertyNode.NavigationNode, visited))
                    {
                        yield return sub;
                    }
                }
            }
        }

    }
}
