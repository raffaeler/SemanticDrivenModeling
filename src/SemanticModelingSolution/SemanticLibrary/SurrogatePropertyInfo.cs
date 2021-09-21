using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace SemanticLibrary
{
    public class SurrogatePropertyInfo
    {
        private PropertyInfo _propertyInfo;

        // default constructor is intended only for the serializer
        public SurrogatePropertyInfo()
        {
        }

        public SurrogatePropertyInfo(PropertyInfo propertyInfo, SurrogateType parentType)
        {
            Name = propertyInfo.Name;
            PropertyType = new(propertyInfo.PropertyType);
            ParentType = parentType;
        }

        public string Name { get; set; }
        public SurrogateType PropertyType { get; set; }
        public SurrogateType ParentType { get; set; }

        public PropertyInfo GetOriginalPropertyInfo()
        {
            if (_propertyInfo == null)
            {
                var parentType = ParentType.GetOriginalType();
                _propertyInfo = parentType.GetProperty(Name);
                if (_propertyInfo == null) throw new InvalidOperationException($"Can't find the property '{Name}' in the type '{ParentType.Name}'");
            }

            return _propertyInfo;
        }

        public void SetValue(object instance, object value)
        {
            var propertyInfo = GetOriginalPropertyInfo();
            propertyInfo.SetValue(instance, value);
        }

        public override string ToString()
        {
            return $"{PropertyType}.{Name}";
        }
    }
}
