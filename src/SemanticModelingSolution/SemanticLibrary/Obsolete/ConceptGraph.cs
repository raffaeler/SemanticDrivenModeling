//using System;
//using System.Collections.Generic;
//using System.Text;

//namespace SemanticLibrary
//{
//    public class ConceptGraph
//    {
//        private HashSet<Concept> _concepts = new HashSet<Concept>();
//        private HashSet<ConceptLink> _arcs = new HashSet<ConceptLink>();

//        //public IReadOnlySet<Concept> Concepts => _concepts;
//        //public IReadOnlySet<ConceptLink> Arcs => _arcs;

//        public HashSet<Concept> Concepts => _concepts;
//        public HashSet<ConceptLink> Arcs => _arcs;

//        public void Connect(Concept from, Concept to, int weight)
//        {
//            _concepts.Add(from);
//            _concepts.Add(to);
//            _arcs.Add(new ConceptLink(from, to, weight));
//        }
//    }
//}
