using System;
using System.Collections.Generic;
using System.Text;

namespace SemanticLibrary
{
    public class ModelTypeNodeVisitor
    {
        private Action<ModelTypeNode, string> _onBeginModelTypeNode;
        private Action<ModelTypeNode, string> _onEndModelTypeNode;
        private Action<ModelPropertyNode, string> _onModelPropertyNode;

        public void Visit(ModelTypeNode modelTypeNode,
            Action<ModelTypeNode, string> onBeginModelTypeNode = null,
            Action<ModelTypeNode, string> onEndModelTypeNode = null,
            Action<ModelPropertyNode, string> onModelPropertyNode = null)
        {
            _onBeginModelTypeNode = onBeginModelTypeNode;
            _onEndModelTypeNode = onEndModelTypeNode;
            _onModelPropertyNode = onModelPropertyNode;

            VisitModelTypeNode(modelTypeNode, modelTypeNode.Type.Name);
        }

        public virtual void OnBeginVisitModelTypeNode(ModelTypeNode modelTypeNode, string path)
        {
            _onBeginModelTypeNode?.Invoke(modelTypeNode, path);
        }

        public virtual void OnEndVisitModelTypeNode(ModelTypeNode modelTypeNode, string path)
        {
            _onEndModelTypeNode?.Invoke(modelTypeNode, path);
        }

        public virtual void OnVisitModelPropertyNode(ModelPropertyNode modelPropertyNode, string path)
        {
            _onModelPropertyNode?.Invoke(modelPropertyNode, path);
        }


        private void VisitModelTypeNode(ModelTypeNode modelTypeNode, string path)
        {
            OnBeginVisitModelTypeNode(modelTypeNode, path);
            foreach (var prop in modelTypeNode.PropertyNodes)
            {
                VisitPropertyTypeNode(prop, path);
            }

            OnEndVisitModelTypeNode(modelTypeNode, path);
        }

        private void VisitPropertyTypeNode(ModelPropertyNode modelPropertyNode, string path)
        {
            path += "." + modelPropertyNode.Name;
            if (modelPropertyNode.PropertyKind == PropertyKind.OneToManyToDomain ||
                modelPropertyNode.PropertyKind == PropertyKind.OneToManyToUnknown)
            {
                path += ".$";
            }

            OnVisitModelPropertyNode(modelPropertyNode, path);
            if (modelPropertyNode.NavigationNode != null)
            {
                VisitModelTypeNode(modelPropertyNode.NavigationNode, path);
            }
        }
    }
}
