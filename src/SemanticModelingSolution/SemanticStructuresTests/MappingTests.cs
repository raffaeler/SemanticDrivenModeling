﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text.Json;

using Humanizer;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using SemanticLibrary;

namespace SemanticStructuresTests
{
    [TestClass]
    public class MappingTests
    {
        [TestMethod]
        public void MappingOrderToOnlineOrder()
        {
            var utilities = new MappingUtilities.MappingUtilities();
            var domain = new GeneratedCode.Domain();
            var analyzer = new MappingUtilities.Analyzer();
            var m1 = analyzer.Prepare("SimpleDomain1", SimpleDomain1.Types.All);
            var m2 = analyzer.Prepare("SimpleDomain2", SimpleDomain2.Types.All);

            var sourceObjects = SimpleDomain1.Samples.GetOrders();
            var targetObjects = utilities.OrderToOnlineOrder(m1, m2, sourceObjects);

        }


        [TestMethod]
        public void MappingOnlineOrderToOrder()
        {
            var utilities = new MappingUtilities.MappingUtilities();
            var domain = new GeneratedCode.Domain();
            var analyzer = new MappingUtilities.Analyzer();
            var m1 = analyzer.Prepare("SimpleDomain1", SimpleDomain1.Types.All);
            var m2 = analyzer.Prepare("SimpleDomain2", SimpleDomain2.Types.All);

            var sourceObjects = SimpleDomain2.Samples.GetOrders();
            var targetObjects = utilities.OnlineOrderToOrder(m2, m1, sourceObjects);
        }

        [TestMethod]
        public void MappingOnlineOrderToOrderFromSerialization()
        {
            var jsonDomainDefinitions = File.ReadAllText("Serializations\\domainDefinitions.json");
            var jsonDomain1 = File.ReadAllText("Serializations\\domain1types.json");
            var jsonDomain2 = File.ReadAllText("Serializations\\domain2types.json");


            var utilities = new MappingUtilities.MappingUtilities();
            //var domain = new GeneratedCode.Domain();
            var domain = JsonSerializer.Deserialize<GeneratedCode.Domain>(jsonDomainDefinitions);
            var analyzer = new MappingUtilities.Analyzer();
            //var m1 = analyzer.Prepare("SimpleDomain1", SimpleDomain1.Types.All);
            //var m2 = analyzer.Prepare("SimpleDomain2", SimpleDomain2.Types.All);
            var m1 = ModelTypeNodeExtensions.DeserializeMany(jsonDomain1, domain);
            var m2 = ModelTypeNodeExtensions.DeserializeMany(jsonDomain2, domain);

            var sourceObjects = SimpleDomain2.Samples.GetOrders();
            var targetObjects = utilities.OnlineOrderToOrder(m2, m1, sourceObjects);
        }




        [TestMethod]
        public void ModelNodeVisitorTests2()
        {
            var domain = new GeneratedCode.Domain();
            var modelsDomain1 = new DomainTypesGraphVisitor(domain, SimpleDomain1.Types.All).Visit(null, null, null);

            var order = modelsDomain1.Single(m => m.Type.Name == "Order");
            var propertiesSupplier = order.FlatHierarchyProperties().ToList();


            var modelsDomain2 = new DomainTypesGraphVisitor(domain, SimpleDomain2.Types.All).Visit(null, null, null);
            var onlineOrder = modelsDomain2.Single(m => m.Type.Name == "OnlineOrder");
            var propertiesdelivery = onlineOrder.FlatHierarchyProperties().ToList();


            //Assert.AreEqual(, );
        }

        //[TestMethod]
        //public void SerializeAll()
        //{
        //    var domain = new GeneratedCode.Domain();
        //    var jsonDomainDefinitions = JsonSerializer.Serialize(domain);
        //    File.WriteAllText("domainDefinitions.json", jsonDomainDefinitions);

        //    var modelsDomain1 = new DomainTypesGraphVisitor(domain, SimpleDomain1.Types.All).Visit(null, null, null);
        //    var jsonDomain1 = modelsDomain1.Serialize(domain);
        //    File.WriteAllText("domain1types.json", jsonDomain1);

        //    var modelsDomain2 = new DomainTypesGraphVisitor(domain, SimpleDomain2.Types.All).Visit(null, null, null);
        //    var jsonDomain2 = modelsDomain2.Serialize(domain);
        //    File.WriteAllText("domain2types.json", jsonDomain2);
        //}


        [TestMethod]
        public void MetadataSerialization()
        {
            var domain = new GeneratedCode.Domain();
            var modelsDomain1 = new DomainTypesGraphVisitor(domain, SimpleDomain1.Types.All).Visit(null, null, null);
            //var json2 = Newtonsoft.Json.JsonConvert.SerializeObject(modelsDomain1);
            var json = modelsDomain1.Serialize(domain);

            var cloneDomain = ModelTypeNodeExtensions.DeserializeMany(json, domain);
            Assert.AreEqual(modelsDomain1.Count, cloneDomain.Count);

            ModelTypeNodeVisitor visitor = new();
            
            var types1 = 0;
            var properties1 = 0;
            foreach (var modelTypeNode in modelsDomain1)
            {
                visitor.Visit(modelTypeNode,
                    tn => types1++,
                    pn => properties1++);
            }

            var types2 = 0;
            var properties2 = 0;
            foreach (var modelTypeNode in cloneDomain)
            {
                visitor.Visit(modelTypeNode,
                    tn => types2++,
                    pn => properties2++);
            }

            Assert.AreEqual(types1, types2);
            Assert.AreEqual(properties1, properties2);
        }
    }
}
