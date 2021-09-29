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
            Action<SurrogateType<T>, string> onBeginType = null,
            Action<SurrogateType<T>, string> onEndType = null,
            Action<SurrogateProperty<T>, string> onProperty = null,
            Action<SurrogateProperty<T>, string> onBeginCollection = null,
            Action<SurrogateProperty<T>, string> onEndCollection = null)
        {
            _onBeginType = onBeginType;
            _onEndType = onEndType;
            _onProperty = onProperty;
            _onBeginCollection = onBeginCollection;
            _onEndCollection = onEndCollection;

            VisitType(type, type.Name);
        }

        public virtual void OnBeginVisitType(SurrogateType<T> type, string path)
        {
            _onBeginType?.Invoke(type, path);
        }

        public virtual void OnEndVisitType(SurrogateType<T> type, string path)
        {
            _onEndType?.Invoke(type, path);
        }

        public virtual void OnVisitProperty(SurrogateProperty<T> property, string path)
        {
            _onProperty?.Invoke(property, path);
        }

        public virtual void OnBeginVisitCollectionProperty(SurrogateProperty<T> property, string path)
        {
            _onBeginCollection?.Invoke(property, path);
        }

        public virtual void OnEndVisitCollectionProperty(SurrogateProperty<T> property, string path)
        {
            _onEndCollection?.Invoke(property, path);
        }


        private void VisitType(SurrogateType<T> type, string path)
        {
            OnBeginVisitType(type, path);
            foreach (var prop in type.Properties.Values)
            {
                VisitPropertyTypeNode(prop, path);
            }

            OnEndVisitType(type, path);
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
                OnBeginVisitCollectionProperty(property, collectionPath);
                path += ".$";
            }

            OnVisitProperty(property, path);
            if (isOneToMany || kind == PropertyKind.OneToOne)
            {
                var navType = property.PropertyType;
                if(navType.InnerType1 != null) navType = navType.InnerType1;
                VisitType(navType, path);
            }

            if (isOneToMany)
            {
                OnEndVisitCollectionProperty(property, collectionPath);
            }

        }
    }
}
