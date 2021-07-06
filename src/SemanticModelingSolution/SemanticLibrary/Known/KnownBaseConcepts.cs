﻿using System;
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
        public static Concept Any = new Concept("Undefined", "Used when some term or context is valid domain-wide");


        //        public static Concept Identity = new Concept("Identity", new[]
        //        {
        //            KnownConceptAttributes.Unique,
        //        });

        //        public static Concept Identifier = new Concept("Identifier");

        //        public static Concept Measure = new Concept("Measure");

        //        public static Concept Direction = new Concept("Direction");

        //        public static Concept Time = new Concept("Time");

        //        public static Concept Position = new Concept("Position");

        //        public static Concept Entity = new Concept("Entity");

        //        public static Concept State = new Concept("State");

        //        public static Concept Composition = new Concept("Composition");

        //        public static Concept Process = new Concept("Process");

        //        public static Concept Subject = new Concept("Subject");

        //        public static Concept Location = new Concept("Location");

        //        public static Concept Document = new Concept("Document");


    }
}
