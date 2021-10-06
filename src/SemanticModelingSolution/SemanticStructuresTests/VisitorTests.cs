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
    public class VisitorTests
    {
        //[TestMethod]
        //public void ModelNodeVisitorTests()
        //{
        //    var domain = new GeneratedCode.Domain();
        //    var modelsDomain1 = new DomainTypesGraphVisitor(domain, ERP_Model.Types.All).Visit(null, null, null);
        //    var supplier = modelsDomain1.Single(m => m.Type.Name == "Supplier");
        //    var delivery = modelsDomain1.Single(m => m.Type.Name == "Delivery");

        //    var propertiesSupplier = supplier.FlatHierarchyProperties().ToList();

        //    var propertiesdelivery = delivery.FlatHierarchyProperties().ToList();
            

        //    //Assert.AreEqual(, );
        //}
    }
}
