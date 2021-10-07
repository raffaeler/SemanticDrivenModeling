using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;

using SurrogateLibrary.Helpers;

// It was necessary to override the Equals/GetHashCode because of a limitation of the System.Text.JsonConverter
// https://github.com/dotnet/runtime/issues/59804

#nullable enable

namespace SemanticLibrary
{
    public record Concept
    {
        [JsonConstructor]
        public Concept(string name, string description, params ConceptSpecifier[] conceptSpecifiers)
            => (Name, Description, ConceptSpecifiers) =
            (name, description, conceptSpecifiers);


        public string Name { get; init; }
        public string Description { get; init; }
        public ConceptSpecifier[] ConceptSpecifiers { get; init; }

        //public ListEx<ConceptSpecifier> ConceptSpecifiers { get; init; }
        //{
        //    get => _conceptSpecifiers;
        //    init => _conceptSpecifiers = value == null
        //        ? new ListEx<ConceptSpecifier>()
        //        : new ListEx<ConceptSpecifier>(value);
        //}

        protected virtual Type EqualityContract => typeof(Concept);

        public override string ToString()
        {
            StringBuilder stringBuilder = new StringBuilder();
            stringBuilder.Append("Concept");
            stringBuilder.Append(" { ");
            if (PrintMembers(stringBuilder))
            {
                stringBuilder.Append(' ');
            }
            stringBuilder.Append('}');
            return stringBuilder.ToString();
        }

        protected virtual bool PrintMembers(StringBuilder builder)
        {
            builder.Append("Name = ");
            builder.Append((object?)Name);
            builder.Append(", Description = ");
            builder.Append((object?)Description);
            builder.Append(", ConceptSpecifiers = ");
            builder.Append(ConceptSpecifiers);
            return true;
        }

        public virtual bool Equals(Concept? other)
        {
            return (object)this == other || (
                (object?)other != null
                && EqualityContract == other!.EqualityContract
                && EqualityComparer<string>.Default.Equals(Name, other!.Name)
                && EqualityComparer<string>.Default.Equals(Description, other!.Description)
                && EnumerableEquals(ConceptSpecifiers, other!.ConceptSpecifiers));
        }

        public override int GetHashCode()
        {
            return (
                (EqualityComparer<Type>.Default.GetHashCode(EqualityContract) * -1521134295
                + EqualityComparer<string>.Default.GetHashCode(Name)) * -1521134295
                + EqualityComparer<string>.Default.GetHashCode(Description)) * -1521134295
                + GetHashCodeEnumerable(ConceptSpecifiers);
        }

        private static bool EnumerableEquals<T>(IEnumerable<T> left, IEnumerable<T> right)
            => left.SequenceEqual(right);

        // https://stackoverflow.com/a/48192420/492913
        private static int GetHashCodeEnumerable<T>(IEnumerable<T> list)
        {
            if (list == null) return 0;
            const int seedValue = 0x2D2816FE;
            const int primeNumber = 397;
            return list.Aggregate(seedValue,
                (current, item) => (current * primeNumber) + (Equals(item, default(T)) ? 0 : item!.GetHashCode()));
        }

        //public static bool operator !=(Concept? left, Concept? right)
        //{
        //    return !(left == right);
        //}

        //public static bool operator ==(Concept? left, Concept? right)
        //{
        //    return (object)left == right || (left?.Equals(right) ?? false);
        //}

        //public override bool Equals(object? obj)
        //{
        //    return Equals(obj as Concept);
        //}
    }
}

#nullable restore