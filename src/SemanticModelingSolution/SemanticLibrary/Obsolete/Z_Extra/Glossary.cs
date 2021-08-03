using System;
using System.Collections.Generic;
using System.Text;

namespace SemanticLibrary
{
    /// <summary>
    /// Glossary is the root for all the terms used
    /// in a specific domain
    /// </summary>
    public class Glossary
    {
        public IList<Term> Terms { get; set; }
    }
}
