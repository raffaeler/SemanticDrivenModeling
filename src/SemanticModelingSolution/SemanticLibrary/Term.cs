using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json.Serialization;

namespace SemanticLibrary
{
    public record Term(string Name, string Description = "", bool IsUnknown = false)
    {
        //[JsonConstructorAttribute]
        //internal Term() { }

        //public Term(string name, string description, params string[] terms) :
        //    this(name, description)
        //{
        //    foreach (var term in terms)
        //    {
        //        AltNames.Add(term);
        //    }
        //}

        public IList<string> AltNames { get; init; } = new List<string>();
    }
}
