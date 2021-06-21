using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

using ManualMapping;

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
            SemanticAnalysis analysis = new SemanticAnalysis();
            var step1 = analysis.Analyze("Lot");
            var step2 = analysis.Analyze(step1, "Article", typeof(string));

            //var visitor = new ModelGraphVisitor(typeof(Lot), typeof(Article));
            //var visitor = new ModelGraphVisitor(_domain1);
            var visitor = new ModelGraphVisitor(_domain2);
            IList<TermsToConcept> classTermsToConcepts = null;
            visitor.Visit(
                (string className) =>
                {
                    classTermsToConcepts = analysis.Analyze(className);
                    var concepts = classTermsToConcepts.Select(t => t.Concept);
                    Console.WriteLine();
                    Console.WriteLine($"Type {className} => [{string.Join(", ", concepts.Select(c => c.Name))}]");
                },
                (string className, PropertyInfo pi, ModelGraphVisitor.PropertyKind kind, Type coreType) =>
                {
                    var propertytermsToConcepts = analysis.Analyze(classTermsToConcepts, pi.Name, coreType);
                    var concepts = propertytermsToConcepts.Select(t => t.Concept);
                    Console.Write($"    - {{P}} {pi.Name.PadRight(30)} => [{string.Join(", ", concepts.Select(c => c.Name)).PadRight(60)}]");
                    Console.WriteLine($" <== {pi.PropertyType.Name.PadRight(20)} - {kind.ToString().PadRight(20)} - {coreType?.Name}");
                });

        }


    }
}
