using System;
using System.Collections.Generic;
using System.Text;

namespace SemanticLibrary
{
    public class ModelTypeNodeVisitor
    {
        private Action<ModelTypeNode> _onModelTypeNode;
        private Action<ModelPropertyNode> _onModelPropertyNode;

        public void Visit(ModelTypeNode modelTypeNode,
            Action<ModelTypeNode> onModelTypeNode = null,
            Action<ModelPropertyNode> onModelPropertyNode = null)
        {
            _onModelTypeNode = onModelTypeNode;
            _onModelPropertyNode = onModelPropertyNode;

            VisitModelTypeNode(modelTypeNode);
        }

        public virtual void OnVisitModelTypeNode(ModelTypeNode modelTypeNode)
        {
            _onModelTypeNode?.Invoke(modelTypeNode);
        }

        public virtual void OnVisitModelPropertyNode(ModelPropertyNode modelPropertyNode)
        {
            _onModelPropertyNode?.Invoke(modelPropertyNode);
        }


        private void VisitModelTypeNode(ModelTypeNode modelTypeNode)
        {
            OnVisitModelTypeNode(modelTypeNode);
            foreach (var prop in modelTypeNode.PropertyNodes)
            {
                VisitPropertyTypeNode(prop);
            }
        }

        private void VisitPropertyTypeNode(ModelPropertyNode modelPropertyNode)
        {
            OnVisitModelPropertyNode(modelPropertyNode);
            if (modelPropertyNode.NavigationNode != null)
            {
                VisitModelTypeNode(modelPropertyNode.NavigationNode);
            }
        }
    }
}
