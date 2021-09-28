﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace SurrogateLibrary
{
    /// <summary>
    /// This class represents the complete path from a Type down
    /// to a leaf property in a graph
    /// </summary>
    public class NavigationSegment<T> : IEquatable<NavigationSegment<T>>
    {
        private NavigationSegment<T> _root;
        private NavigationSegment<T> _leaf;

        public NavigationSegment(NavigationSegment<T> previous, SurrogateProperty<T> property)
        {
            this.Previous = previous;
            this.PropertyIndex = property.Index;
            this.Property = property;
        }

        public NavigationSegment(NavigationSegment<T> previous, SurrogateType<T> type)
        {
            this.Previous = previous;
            this.TypeIndex = type.Index;
            this.Type = type;
        }

        [JsonConstructor]
        public NavigationSegment(UInt64 propertyIndex, UInt64 typeIndex, NavigationSegment<T> next) =>
            (PropertyIndex, TypeIndex, Next) =
            (propertyIndex, typeIndex, next);

        public UInt64 PropertyIndex { get; private set; }

        [JsonIgnore]
        public SurrogateProperty<T> Property { get; private set; }


        public UInt64 TypeIndex { get; private set; }

        [JsonIgnore]
        public SurrogateType<T> Type { get; private set; }


        [JsonIgnore]
        public NavigationSegment<T> Previous { get; private set; }

        public NavigationSegment<T> Next { get; private set; }


        [JsonIgnore]
        public string Path { get; private set; }

        [JsonIgnore]
        public string Name => Property != null ? Property.Name : Type?.Name;

        public void SetNext(NavigationSegment<T> next) => this.Next = next;

        private void OnEach(Func<NavigationSegment<T>, bool> func)
        {
            var temp = GetRoot();
            while (temp != null)
            {
                if (!func(temp)) return;
                temp = temp.Next;
            }
        }

        private void OnAllBefore(NavigationSegment<T> start, Func<NavigationSegment<T>, bool> func)
        {
            var temp = start;
            while (temp != null)
            {
                if (!func(temp)) return;
                temp = temp.Previous;
            }
        }

        private void OnAllAfter(NavigationSegment<T> start, Func<NavigationSegment<T>, bool> func)
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

            _root = null;
            _leaf = null;
        }

        public void UpdateCache(ITypeSystem<T> typeSystem)
        {
            var sb = new StringBuilder();

            NavigationSegment<T> previous = null;
            OnEach(nav =>
            {
                if (typeSystem != null)
                {
                    // update Property and Type
                    nav.Property = typeSystem.GetSurrogateProperty(nav.PropertyIndex);
                    nav.Type = typeSystem.GetSurrogateType(nav.TypeIndex);
                }
                else
                {
                    if (nav.Property == null && nav.Type == null)
                    {
                        throw new InvalidOperationException($"UpdateCache was called with a null TypeSystem. This is legal only when the Property or Type properties are already set");
                    }
                }

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

        public NavigationSegment<T> GetRoot()
        {
            if (_root != null && _root.Previous == null) return _root;

            _root = this;
            while (true)
            {
                if (_root.Previous == null) return _root;
                _root = _root.Previous;
            }
        }

        public NavigationSegment<T> GetLeaf()
        {
            if(_leaf != null && _leaf.Next == null) return _leaf;

            _leaf = this;
            while (true)
            {
                if (_leaf.Next == null) return _leaf;
                _leaf = _leaf.Next;
            }
        }

        public NavigationSegment<T> CloneAndReturnLeaf()
        {
            var temp = GetRoot();
            NavigationSegment<T> cloned = null;
            while (temp != null)
            {
                var currentClone = temp.Clone();//temp with { };
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

        public bool PointsBack(SurrogateProperty<T> property)
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

        public bool ContainsBack(SurrogateProperty<T> property)
        {
            var temp = this;
            while (temp != null)
            {
                if (temp.Property != null && temp.Property.Index == property.Index) return true;
                temp = temp.Previous;
            }

            return false;
        }

        public bool ContainsForward(SurrogateProperty<T> property)
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

        /*
        
        Methods stolen from the record automatic generation
        IEquatable<NavigationPath>

        */

        protected virtual Type EqualityContract => typeof(NavigationSegment<T>);

        public static bool operator ==(NavigationSegment<T> left, NavigationSegment<T> right)
            => (object)left == right || (left?.Equals(right) ?? false);

        public static bool operator !=(NavigationSegment<T> left, NavigationSegment<T> right)
            => !(left == right);

        public override bool Equals(object obj) => Equals(obj as NavigationSegment<T>);


        /// <summary>
        /// Fence to avoid re-entrancy
        /// </summary>
        private bool _isInEquals;

        /// <summary>
        /// Added fence-testing to avoid re-entrancy
        /// </summary>
        /// <param name="other"></param>
        /// <returns></returns>
        public virtual bool Equals(NavigationSegment<T> other)
        {
            if (_isInEquals) return true;
            _isInEquals = true;

            var result = (object)this == other ||
                ((object)other != null &&
                EqualityContract == other!.EqualityContract &&
                EqualityComparer<NavigationSegment<T>>.Default.Equals(Previous, other!.Previous) &&
                EqualityComparer<ulong>.Default.Equals(PropertyIndex, other!.PropertyIndex) &&
                EqualityComparer<SurrogateProperty<T>>.Default.Equals(Property, other!.Property) &&
                EqualityComparer<ulong>.Default.Equals(TypeIndex, other!.TypeIndex) &&
                EqualityComparer<SurrogateType<T>>.Default.Equals(Type, other!.Type) &&
                EqualityComparer<NavigationSegment<T>>.Default.Equals(Next, other!.Next) &&
                EqualityComparer<string>.Default.Equals(Path, other!.Path));

            _isInEquals = false;
            return result;
        }

        /// <summary>
        /// This is obtained from the record code generation
        /// but removing the following properties from the computation:
        /// - Previous, Next: they are just convenient ways to navigate the path but do not change the
        ///   uniqueness of the path itself
        /// - Property and Type are already computed with their index counterpart
        /// In theory we could just rely on the Path property but this is computed by UpdateCache
        /// therefore it is better to rely on the others as well.
        /// </summary>
        public override int GetHashCode()
        {
            return ((((((
                EqualityComparer<System.Type>.Default.GetHashCode(EqualityContract) * -1521134295
                /*+ EqualityComparer<NavigationPath>.Default.GetHashCode(Previous)*/) * -1521134295
                + EqualityComparer<ulong>.Default.GetHashCode(PropertyIndex)) * -1521134295
                /*+ EqualityComparer<SurrogateProperty>.Default.GetHashCode(Property)*/) * -1521134295
                + EqualityComparer<ulong>.Default.GetHashCode(TypeIndex)) * -1521134295
                /*+ EqualityComparer<SurrogateType>.Default.GetHashCode(Type)*/) * -1521134295
                /*+ EqualityComparer<NavigationPath>.Default.GetHashCode(Next)*/) * -1521134295
                + EqualityComparer<string>.Default.GetHashCode(Path);

        }

        public virtual NavigationSegment<T> Clone() => new NavigationSegment<T>(this);

        protected NavigationSegment(NavigationSegment<T> original)
        {
            Previous = original.Previous;
            PropertyIndex = original.PropertyIndex;
            Property = original.Property;
            TypeIndex = original.TypeIndex;
            Type = original.Type;
            Next = original.Next;
            Path = original.Path;
        }

    }
}
