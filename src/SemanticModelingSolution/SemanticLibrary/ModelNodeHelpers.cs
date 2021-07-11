using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SemanticLibrary
{
    /// <summary>
    /// A visitor which avoids the risk of circular references
    /// when walking through a graph of IModelNode elements (types and properties)
    /// </summary>
    public static class ModelNodeHelpers
    {
        public static IEnumerable<ModelNavigationNode> FlatHierarchyProperties(this ModelTypeNode node)
        {
            Dictionary<string, ModelTypeNode> visited = new();
            return FlatHierarchyPropertiesInternal(node, null, visited);
        }

        private static IEnumerable<ModelNavigationNode> FlatHierarchyPropertiesInternal(ModelTypeNode node, ModelNavigationNode previous,
            Dictionary<string, ModelTypeNode> visited)
        {
            if (!visited.TryGetValue(node.Type.AssemblyQualifiedName, out _))
            {
                visited[node.Type.AssemblyQualifiedName] = node;
                foreach (var propertyNode in node.PropertyNodes)
                {
                    var navigation = new ModelNavigationNode(propertyNode, previous);
                    if (propertyNode.NavigationNode == null)
                    {
                        yield return navigation;
                        continue;
                    }

                    foreach(var sub in FlatHierarchyPropertiesInternal(propertyNode.NavigationNode, navigation, visited))
                    {
                        yield return sub;
                    }
                }
            }
        }

    }
}
