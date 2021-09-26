using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace SurrogateLibrary
{
    public record NavigationProperty
    {
        public NavigationProperty(NavigationProperty previous, SurrogateProperty property)
        {
            this.Previous = previous;
            this.PropertyIndex = property.Index;
            this.Property = property;
        }

        public NavigationProperty(NavigationProperty previous, SurrogateType type)
        {
            this.Previous = previous;
            this.TypeIndex = type.Index;
            this.Type = type;
        }

        [JsonConstructor]
        public NavigationProperty(UInt64 propertyIndex, UInt64 typeIndex, NavigationProperty next) =>
            (PropertyIndex, TypeIndex, Next) =
            (propertyIndex, typeIndex, next);

        [JsonIgnore]
        public NavigationProperty Previous { get; private set; }

        public UInt64 PropertyIndex { get; private set; }
        [JsonIgnore]
        public SurrogateProperty Property { get; private set; }
        public UInt64 TypeIndex { get; private set; }
        [JsonIgnore]
        public SurrogateType Type { get; private set; }

        public NavigationProperty Next { get; private set; }
        public string Path { get; private set; }
        public string Name => Property != null ? Property.Name : Type?.Name;

        public void SetNext(NavigationProperty next) => this.Next = next;

        private void OnEach(Func<NavigationProperty, bool> func)
        {
            var temp = GetRoot();
            while (temp != null)
            {
                if (!func(temp)) return;
                temp = temp.Next;
            }
        }

        private void OnAllBefore(NavigationProperty start, Func<NavigationProperty, bool> func)
        {
            var temp = start;
            while (temp != null)
            {
                if (!func(temp)) return;
                temp = temp.Previous;
            }
        }

        private void OnAllAfter(NavigationProperty start, Func<NavigationProperty, bool> func)
        {
            var temp = start;
            while (temp != null)
            {
                if (!func(temp)) return;
                temp = temp.Next;
            }
        }

        public void ClearCache()
        {
            OnEach(nav =>
            {
                Previous = null;
                Property = null;
                Type = null;
                nav.Path = null;
                return true;
            });
        }

        public void UpdateCache(TypeSystem typeSystem)
        {
            var sb = new StringBuilder();

            NavigationProperty previous = null;
            OnEach(nav =>
            {
                // update Property and Type
                nav.Property = typeSystem.GetSurrogateProperty(nav.PropertyIndex);
                nav.Type = typeSystem.GetSurrogateType(nav.TypeIndex);

                // update path
                sb.Append(nav.Name);
                nav.Path = sb.ToString();
                if (nav.Next != null) sb.Append(".");

                // update Previous
                nav.Previous = previous;
                previous = nav;

                return true;
            });
        }

        public NavigationProperty GetRoot()
        {
            var temp = this;
            while (true)
            {
                if (temp.Previous == null) return temp;
                temp = temp.Previous;
            }
        }

        public NavigationProperty GetLast()
        {
            var temp = this;
            while (true)
            {
                if (temp.Next == null) return temp;
                temp = temp.Next;
            }
        }

        public NavigationProperty CloneRoot()
        {
            var temp = GetRoot();
            NavigationProperty cloned = null;
            while (temp != null)
            {
                var currentClone = temp with { };
                if (cloned != null)
                {
                    cloned.Next = currentClone;
                    currentClone.Previous = cloned;
                }

                cloned = currentClone;

                temp = temp.Next;
            }

            return cloned;
        }

        public bool PointsBack(SurrogateProperty property)
        {
            var temp = this;
            while (temp != null)
            {
                if (temp.Property != null)
                {
                    if (temp.Property.PropertyType.Index == property.PropertyType.Index) return true;
                }

                if (temp.Type != null)
                {
                    if (temp.Type.Index == property.PropertyType.Index) return true;
                }

                temp = temp.Previous;
            }

            return false;
        }

        public bool ContainsBack(SurrogateProperty property)
        {
            var temp = this;
            while (temp != null)
            {
                if (temp.Property != null && temp.Property.Index == property.Index) return true;
                temp = temp.Previous;
            }

            return false;
        }

        public bool ContainsForward(SurrogateProperty property)
        {
            var temp = this;
            while (temp != null)
            {
                if (temp.Property.Index == property.Index) return true;
                temp = temp.Next;
            }

            return false;
        }

        public string ToStringCurrent()
        {
            if (Property != null)
                return Property.ToString();

            return Type.ToString();
        }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            if (Previous != null)
            {
                sb.Append(Previous.ToStringCurrent());
                sb.Append(" <= ");
            }

            sb.Append(ToStringCurrent());

            if (Next != null)
            {
                sb.Append(" => ");
                sb.Append(Next.ToStringCurrent());
            }

            return sb.ToString();
        }
    }
}
