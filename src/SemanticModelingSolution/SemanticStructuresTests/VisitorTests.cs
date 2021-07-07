using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;

using Humanizer;

using ManualMapping;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using SemanticGlossaryGenerator;

using SemanticLibrary;
using SemanticLibrary.Helpers;

namespace SemanticStructuresTests
{
    [TestClass]
    public class VisitorTests
    {
        static Type[] _domainTypes1 = new Type[]
        {
            typeof(ERP_Model.Models.Supply           ),
            typeof(ERP_Model.Models.SupplyItem       ),
            typeof(ERP_Model.Models.Address          ),
            typeof(ERP_Model.Models.Customer         ),
            typeof(ERP_Model.Models.Delivery         ),
            typeof(ERP_Model.Models.DeliveryItem     ),
            typeof(ERP_Model.Models.GoodsReceipt     ),
            typeof(ERP_Model.Models.GoodsReceiptItem ),
            typeof(ERP_Model.Models.Order            ),
            typeof(ERP_Model.Models.OrderItem        ),
            typeof(ERP_Model.Models.Product          ),
            typeof(ERP_Model.Models.Stock            ),
            typeof(ERP_Model.Models.StockItem        ),
            typeof(ERP_Model.Models.Supplier         ),
            typeof(ERP_Model.Models.Supply           ),
            typeof(ERP_Model.Models.SupplyItem       ),
        };

        [TestMethod]
        public void ModelNodeVisitorTests()
        {
            var domain = new GeneratedCode.Domain();
            var modelsDomain1 = new DomainTypesGraphVisitor(domain, _domainTypes1).Visit(null, null, null);
            var supplier = modelsDomain1.Single(m => m.Type.Name == "Supplier");
            var delivery = modelsDomain1.Single(m => m.Type.Name == "Delivery");

            var propertiesSupplier = supplier.FlatHierarchyProperties().ToList();

            var propertiesdelivery = delivery.FlatHierarchyProperties().ToList();
            

            //Assert.AreEqual(, );
        }
    }
}
