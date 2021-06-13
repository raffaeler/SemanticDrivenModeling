using System;
using System.Collections.Generic;
using System.Text;

namespace SemanticLibrary
{
    /// <summary>
    /// Domain is the application field.
    /// Every service works in the context of a specific domain
    /// where the concepts has a specifical meaning
    /// </summary>
    public class Domain
    {
        public IList<Concept> Concepts { get; set; }
    }
}
