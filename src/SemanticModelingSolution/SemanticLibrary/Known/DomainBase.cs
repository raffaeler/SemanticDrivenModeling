using System;
using System.Collections.Generic;
using System.Text;

namespace SemanticLibrary
{
    /// <summary>
    /// Domain is the application field.
    /// Every service works in the context of a specific domain
    /// where the concepts has a specifical meaning
    /// This class is used as the base class for every automatically generated Domain
    /// </summary>
    public class DomainBase
    {
        public virtual List<TermToConcept> Links { get; init; } = new()
        {
            new TermToConcept(KnownBaseConcepts.UniqueIdentity, KnownBaseConcepts.Any, KnownBaseConceptSpecifiers.None, KnownBaseTerms.Id, 100),
            new TermToConcept(KnownBaseConcepts.UniqueIdentity, KnownBaseConcepts.Any, KnownBaseConceptSpecifiers.None, KnownBaseTerms.Guid, 100),
            new TermToConcept(KnownBaseConcepts.UniqueIdentity, KnownBaseConcepts.Any, KnownBaseConceptSpecifiers.None, KnownBaseTerms.NaturalId, 100),
            new TermToConcept(KnownBaseConcepts.UniqueIdentity, KnownBaseConcepts.Any, KnownBaseConceptSpecifiers.None, KnownBaseTerms.UniqueIdentity, 100),
            new TermToConcept(KnownBaseConcepts.Identity, KnownBaseConcepts.Any, KnownBaseConceptSpecifiers.None, KnownBaseTerms.Name, 100),
            new TermToConcept(KnownBaseConcepts.Identity, KnownBaseConcepts.Person, KnownBaseConceptSpecifiers.None, KnownBaseTerms.First, 100),
            new TermToConcept(KnownBaseConcepts.Identity, KnownBaseConcepts.Person, KnownBaseConceptSpecifiers.None, KnownBaseTerms.FirstName, 100),
            new TermToConcept(KnownBaseConcepts.Identity, KnownBaseConcepts.Person, KnownBaseConceptSpecifiers.None, KnownBaseTerms.Last, 100),
            new TermToConcept(KnownBaseConcepts.Identity, KnownBaseConcepts.Person, KnownBaseConceptSpecifiers.None, KnownBaseTerms.LastName, 100),
            new TermToConcept(KnownBaseConcepts.Identity, KnownBaseConcepts.Person, KnownBaseConceptSpecifiers.None, KnownBaseTerms.Middle, 100),
            new TermToConcept(KnownBaseConcepts.Identity, KnownBaseConcepts.Person, KnownBaseConceptSpecifiers.None, KnownBaseTerms.MiddleName, 100),
            new TermToConcept(KnownBaseConcepts.Identity, KnownBaseConcepts.Person, KnownBaseConceptSpecifiers.None, KnownBaseTerms.Mid, 100),
            new TermToConcept(KnownBaseConcepts.Identity, KnownBaseConcepts.Person, KnownBaseConceptSpecifiers.None, KnownBaseTerms.MidName, 100),
            new TermToConcept(KnownBaseConcepts.Identity, KnownBaseConcepts.Person, KnownBaseConceptSpecifiers.None, KnownBaseTerms.Nick, 100),
            new TermToConcept(KnownBaseConcepts.Identity, KnownBaseConcepts.Person, KnownBaseConceptSpecifiers.None, KnownBaseTerms.NickName, 100),
            new TermToConcept(KnownBaseConcepts.Identity, KnownBaseConcepts.Person, KnownBaseConceptSpecifiers.None, KnownBaseTerms.Title, 80),
            new TermToConcept(KnownBaseConcepts.Identity, KnownBaseConcepts.Any, KnownBaseConceptSpecifiers.None, KnownBaseTerms.Code, 100),
            new TermToConcept(KnownBaseConcepts.Identity, KnownBaseConcepts.Any, KnownBaseConceptSpecifiers.None, KnownBaseTerms.Image, 80),
            new TermToConcept(KnownBaseConcepts.Identity, KnownBaseConcepts.Any, KnownBaseConceptSpecifiers.None, KnownBaseTerms.Picture, 80),
            new TermToConcept(KnownBaseConcepts.Identity, KnownBaseConcepts.Any, KnownBaseConceptSpecifiers.None, KnownBaseTerms.Barcode, 100),
            new TermToConcept(KnownBaseConcepts.Identity, KnownBaseConcepts.Any, KnownBaseConceptSpecifiers.None, KnownBaseTerms.Tag, 50),
            new TermToConcept(KnownBaseConcepts.Identity, KnownBaseConcepts.Any, KnownBaseConceptSpecifiers.None, KnownBaseTerms.Identity, 100),

            new TermToConcept(KnownBaseConcepts.Person, KnownBaseConcepts.Any, KnownBaseConceptSpecifiers.None, KnownBaseTerms.Human, 80),
            new TermToConcept(KnownBaseConcepts.Person, KnownBaseConcepts.Any, KnownBaseConceptSpecifiers.None, KnownBaseTerms.Employee, 100),
            new TermToConcept(KnownBaseConcepts.Person, KnownBaseConcepts.Any, KnownBaseConceptSpecifiers.None, KnownBaseTerms.Contact, 100),
            new TermToConcept(KnownBaseConcepts.Person, KnownBaseConcepts.Any, KnownBaseConceptSpecifiers.None, KnownBaseTerms.Owner, 50),
            new TermToConcept(KnownBaseConcepts.Person, KnownBaseConcepts.Any, KnownBaseConceptSpecifiers.None, KnownBaseTerms.Operator, 90),
            new TermToConcept(KnownBaseConcepts.Person, KnownBaseConcepts.Any, KnownBaseConceptSpecifiers.None, KnownBaseTerms.Person, 100),

        };
    }
}
