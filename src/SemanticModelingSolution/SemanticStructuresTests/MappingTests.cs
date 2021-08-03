using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;

using Humanizer;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using SemanticLibrary;

namespace SemanticStructuresTests
{
    [TestClass]
    public class MappingTests
    {
        static Type[] _domainTypes1 = new Type[]
        {
            typeof(SimpleDomain1.Order),
            typeof(SimpleDomain1.OrderItem),
            typeof(SimpleDomain1.Article),
            typeof(SimpleDomain1.Company),
            typeof(SimpleDomain1.Address),
        };

        static Type[] _domainTypes2 = new Type[]
        {
            typeof(SimpleDomain2.OnlineOrder),
            typeof(SimpleDomain2.OrderLine),
        };


        [TestMethod]
        public void MappingOrderToOnlineOrder()
        {
            var utilities = new MappingUtilities.MappingUtilities();
            var domain = new GeneratedCode.Domain();
            var analyzer = new MappingUtilities.Analyzer();
            var m1 = analyzer.Prepare("SimpleDomain1", _domainTypes1);
            var m2 = analyzer.Prepare("SimpleDomain2", _domainTypes2);

            var sourceObjects = SimpleDomain1.Samples.GetOrders();
            var targetObjects = utilities.OrderToOnlineOrder(m1, m2, sourceObjects);
            
        }


        [TestMethod]
        public void MappingOnlineOrderToOrder()
        {
            var utilities = new MappingUtilities.MappingUtilities();
            var domain = new GeneratedCode.Domain();
            var analyzer = new MappingUtilities.Analyzer();
            var m1 = analyzer.Prepare("SimpleDomain1", _domainTypes1);
            var m2 = analyzer.Prepare("SimpleDomain2", _domainTypes2);

            var sourceObjects = SimpleDomain2.Samples.GetOrders();
            var targetObjects = utilities.OnlineOrderToOrder(m2, m1, sourceObjects);

        }

        [TestMethod]
        public void ModelNodeVisitorTests2()
        {
            var domain = new GeneratedCode.Domain();
            var modelsDomain1 = new DomainTypesGraphVisitor(domain, _domainTypes1).Visit(null, null, null);
            var order = modelsDomain1.Single(m => m.Type.Name == "Order");
            var onlineOrder = modelsDomain1.Single(m => m.Type.Name == "OnlineOrder");

            var propertiesSupplier = order.FlatHierarchyProperties().ToList();

            var propertiesdelivery = onlineOrder.FlatHierarchyProperties().ToList();


            //Assert.AreEqual(, );
        }
    }
}
