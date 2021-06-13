using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SemanticLibrary.Known
{
    public static class KnownConceptAttributes
    {
        /// <summary>
        /// Can be used as an unique identifier
        /// </summary>
        public static ConceptAttribute Unique = new ConceptAttribute("Unique");

        /// <summary>
        /// The data is friendly to a human being
        /// </summary>
        public static ConceptAttribute HumanReadable = new ConceptAttribute("HumanReadable");

        /// <summary>
        /// The data consists in a description that is also human readable
        /// </summary>
        public static ConceptAttribute Descriptive = new ConceptAttribute("Descriptive");

        /// <summary>
        /// The data can be used to separate items in different sets
        /// </summary>
        public static ConceptAttribute Classification = new ConceptAttribute("Classification");

        //public static ConceptAttribute Classification = new ConceptAttribute("Classification");

    }
}
