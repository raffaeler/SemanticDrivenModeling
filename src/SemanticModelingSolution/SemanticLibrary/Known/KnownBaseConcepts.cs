using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SemanticLibrary
{
    public class KnownBaseConcepts
    {
        /// <summary>
        /// Unique identifier
        /// </summary>
        public static Concept Undefined = new Concept("Undefined", "Used to mark unmappable concepts in the graph");

        /// <summary>
        /// Domain wide context
        /// </summary>
        public static Concept Any = new Concept("Any", "Used when some term or context is valid domain-wide");

        /// <summary>
        /// Unique identifier
        /// </summary>
        public static Concept UniqueIdentity = new Concept("UniqueIdentity", "Unique identifier also known as key");

        /// <summary>
        /// The non unique identity of an entity
        /// </summary>
        public static Concept Identity = new Concept("Identity", "Not necessarily an item identifier. Name, Tag, etc.");

        /// <summary>
        /// </summary>
        public static Concept Person = new Concept("Person", "");
    }
}
