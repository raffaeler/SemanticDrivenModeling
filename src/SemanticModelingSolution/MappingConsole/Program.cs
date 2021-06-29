using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;

using LegacyModels;

using ManualMapping;
using ManualMapping.MatchingRules;

using SemanticLibrary;

namespace MappingConsole
{
    class Program
    {
        static Type[] _domain1 = new Type[]
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

        static Type[] _domain2 = new Type[]
        {
            typeof(coderush.Models.Bill               ),
            typeof(coderush.Models.BillType           ),
            typeof(coderush.Models.Branch             ),
            typeof(coderush.Models.CashBank           ),
            typeof(coderush.Models.Currency           ),
            typeof(coderush.Models.Customer           ),
            typeof(coderush.Models.CustomerType       ),
            typeof(coderush.Models.GoodsReceivedNote  ),
            typeof(coderush.Models.Invoice            ),
            typeof(coderush.Models.InvoiceType        ),
            typeof(coderush.Models.NumberSequence     ),
            typeof(coderush.Models.PaymentReceive     ),
            typeof(coderush.Models.PaymentType        ),
            typeof(coderush.Models.PaymentVoucher     ),
            typeof(coderush.Models.Product            ),
            typeof(coderush.Models.ProductType        ),
            typeof(coderush.Models.PurchaseOrder      ),
            typeof(coderush.Models.PurchaseOrderLine  ),
            typeof(coderush.Models.PurchaseType       ),
            typeof(coderush.Models.SalesOrder         ),
            typeof(coderush.Models.SalesOrderLine     ),
            typeof(coderush.Models.SalesType          ),
            typeof(coderush.Models.Shipment           ),
            typeof(coderush.Models.ShipmentType       ),
            typeof(coderush.Models.UnitOfMeasure      ),
            typeof(coderush.Models.Vendor             ),
            typeof(coderush.Models.VendorType         ),
            typeof(coderush.Models.Warehouse          ),
        };


        static void Main(string[] args)
        {
            //Visit(new Type[] { typeof(Lot), typeof(Article) });
            //Visit(_domain1);
            //Visit(_domain2);

            //var models1 = Visit(_domain1, new[] { typeof(ERP_Model.Models.Supplier) });
            
            VisitCompare();
        }

        static void VisitCompare()
        {
            var modelsDomain1 = Visit(_domain1);

            var models2 = Visit(_domain2, new[] { typeof(coderush.Models.Vendor) });
            Debug.Assert(models2.Count == 1);
            var model2 = models2.Single();

            var matcher = new StandardConceptMatchingRule();

            var winner = matcher.FindMatch(model2, modelsDomain1);
            
            var list = matcher.FindOrderedMatches(model2, modelsDomain1);

        }

        static IList<ModelTypeNode> Visit(Type[] domainTypes, Type[] visitOnlyTheseTypes = null)
        {
            var visitor = new ModelGraphVisitor(domainTypes);
            return visitor.Visit(VisitType, VisitProperty, visitOnlyTheseTypes);

            static void VisitType(ModelTypeNode modelTypeNode)
            {
                Console.WriteLine();
                Console.WriteLine($"Type {modelTypeNode.TypeName} => [{string.Join(", ", modelTypeNode.CandidateConceptNames)}]");
            }

            static void VisitProperty(ModelPropertyNode modelPropertyNode)
            {
                Console.Write($"    - {{P}} {modelPropertyNode.Name.PadRight(30)} => [{string.Join(", ", modelPropertyNode.CandidateConceptNames).PadRight(60)}]");
                Console.WriteLine($" <== {modelPropertyNode.Property.PropertyType.Name.PadRight(20)} - {modelPropertyNode.PropertyKind.ToString().PadRight(20)} - {modelPropertyNode.CoreType?.Name}");
            }
        }




    }
}
