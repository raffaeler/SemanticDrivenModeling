using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SurrogateLibrary.Experimental
{
    public class NavigationPath : IEnumerable<NavigationNode>
    {
        private List<NavigationNode> _items;

        public NavigationNode CreateNode(SurrogateProperty property)
        {
            var node = new NavigationNode(this, property);
            _items.Add(node);
            return node;
        }


        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
        public IEnumerator<NavigationNode> GetEnumerator() => _items.GetEnumerator();
    }
}
