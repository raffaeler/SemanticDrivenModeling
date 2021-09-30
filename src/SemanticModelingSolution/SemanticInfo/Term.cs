using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json.Serialization;

using SurrogateLibrary.Helpers;

namespace SemanticLibrary
{
    public record Term(string Name, string Description = "", bool IsUnknown = false)
    {
        private ListEx<string> _altNames = new();
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

        public ListEx<string> AltNames
        {
            get => _altNames;
            init { _altNames = value; }
        }
    }
}
