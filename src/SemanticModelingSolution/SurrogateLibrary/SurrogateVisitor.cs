using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SurrogateLibrary
{
    public class SurrogateVisitor<T>
    {
        private Action<SurrogateType<T>, string> _onBeginType;
        private Action<SurrogateType<T>, string> _onEndType;
        private Action<SurrogateProperty<T>, string> _onProperty;
        private Action<SurrogateProperty<T>, string> _onBeginCollection;
        private Action<SurrogateProperty<T>, string> _onEndCollection;

        public void Visit(SurrogateType<T> type,
            Action<SurrogateType<T>, string> onBeginType,
            Action<SurrogateType<T>, string> onEndType,
            Action<SurrogateProperty<T>, string> onProperty,
            Action<SurrogateProperty<T>, string> onBeginCollection,
            Action<SurrogateProperty<T>, string> onEndCollection)
        {
            _onBeginType = onBeginType;
            _onEndType = onEndType;
            _onProperty = onProperty;
            _onBeginCollection = onBeginCollection;
            _onEndCollection = onEndCollection;

            VisitType(type, type.Name);
        }

        public virtual void OnBeginVisitModelTypeNode(SurrogateType<T> type, string path)
        {
            _onBeginType?.Invoke(type, path);
        }

        public virtual void OnEndVisitModelTypeNode(SurrogateType<T> type, string path)
        {
            _onEndType?.Invoke(type, path);
        }

        public virtual void OnVisitModelPropertyNode(SurrogateProperty<T> property, string path)
        {
            _onProperty?.Invoke(property, path);
        }

        public virtual void OnBeginVisitCollectionPropertyNode(SurrogateProperty<T> property, string path)
        {
            _onBeginCollection?.Invoke(property, path);
        }

        public virtual void OnEndVisitCollectionPropertyNode(SurrogateProperty<T> property, string path)
        {
            _onEndCollection?.Invoke(property, path);
        }


        private void VisitType(SurrogateType<T> type, string path)
        {
            OnBeginVisitModelTypeNode(type, path);
            foreach (var prop in type.Properties.Values)
            {
                VisitPropertyTypeNode(prop, path);
            }

            OnEndVisitModelTypeNode(type, path);
        }

        private void VisitPropertyTypeNode(SurrogateProperty<T> property, string path)
        {
            string collectionPath = string.Empty;
            path += "." + property.Name;

            var kind = property.GetKind();
            var isOneToMany = kind == PropertyKind.OneToMany ||
                kind == PropertyKind.OneToManyBasicType ||
                kind == PropertyKind.OneToManyEnum;

            if (isOneToMany)
            {
                collectionPath = path;
                OnBeginVisitCollectionPropertyNode(property, collectionPath);
                path += ".$";
            }

            OnVisitModelPropertyNode(property, path);
            if (isOneToMany || kind == PropertyKind.OneToOne)
            {
                var navType = property.PropertyType;
                if(navType.InnerType1 != null) navType = navType.InnerType1;
                VisitType(navType, path);
            }

            if (isOneToMany)
            {
                OnEndVisitCollectionPropertyNode(property, collectionPath);
            }

        }
    }
}
