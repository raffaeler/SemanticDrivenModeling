# Textual file format to define a Domain

The DomainGenerator looks for any file starting with "Domain" and has the following structure:


* `#` is used for comments that will be totally ignored by the parser
* Lines starting with `//` are parsed as comments for the `Concepts`
* Lines starting with `$` are parsed as description for the `Concepts`
* Concepts must start with normal text and must not contain any space
  * They eventually may contain other words that are separated by `:` and optional spaces before or after the `:`
* Terms referring to a `Concept` must be indented with one or more spaces. The Terms line is parsed as follow:
  * The string is split in parts using `:` as separator
  * The first word defines a `Term` that is linked to the `Concept`
  * The second word defines a number representing the `weight` of the link going from `Term` to `Concept` 
  * The third word/phrase defines the description and the comment for the `Term`


For example, let's take the following text:
```
# This file contains the common concepts and related terms

// The non unique identity of an entity
$ Not necessarily an item identifier. Name, Tag, etc.
Identity
    Name : 100: The name of the entity
    Title: 80

Product
    Material : 100
```

The generator translates it to three different classes:

```
using System.Collections.Generic;
using SemanticLibrary;

namespace GeneratedCode
{
    public class KnownConcepts
    {
        
        /// <summary>
        /// The non unique identity of an entity
        /// </summary>
        public static Concept Identity = new Concept("Identity", "Not necessarily an item identifier. Name, Tag, etc.");
        
        /// <summary>
        /// </summary>
        public static Concept Product = new Concept("Product", "");
    }
}
```

```
using System.Collections.Generic;
using SemanticLibrary;

namespace GeneratedCode
{
    public class KnownTerms
    {
        
        /// <summary>
        /// The name of the entity
        /// </summary>
        public static Term Name = new Term("Name", "The name of the entity");
        
        /// <summary>
        /// </summary>
        public static Term Title = new Term("Title", "");
        
        /// <summary>
        /// </summary>
        public static Term Material = new Term("Material", "");
    }
}
```


```
using System.Collections.Generic;
using SemanticLibrary;

namespace GeneratedCode
{
    public class Domain
    {
        
        /// <summary>
        /// The relationships between terms and a concept
        /// </summary>
        public List<TermToConcept> Links { get; } = new()
        {};
        public Domain()
        {
            Links.Add(new TermToConcept(KnownConcepts.Identity, (KnownTerms.Name, 100)));
            Links.Add(new TermToConcept(KnownConcepts.Identity, (KnownTerms.Title, 80)));
            Links.Add(new TermToConcept(KnownConcepts.Product, (KnownTerms.Material, 100)));
        }
    }
}
```

