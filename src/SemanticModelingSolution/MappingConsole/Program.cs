using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;

using SemanticLibrary;

// Rules to enforce:
// UniqueIdentity must match only on 1st level and equal concepts (cannot be a child)
// UniqueIdentity on the child targets must be at the same level or created as new
// The target type must be >= the source one. If the destination is a boolean, it can't fit

namespace MappingConsole
{
    class Program
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

        static Type[] _domainTypes2 = new Type[]
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

        static Type[] _domainTypesNW = new Type[]
        {
            typeof(NorthwindDataLayer.Models.Alphabetical_list_of_product        ),
            typeof(NorthwindDataLayer.Models.Category                            ),
            typeof(NorthwindDataLayer.Models.Category_Sales_for_1997             ),
            typeof(NorthwindDataLayer.Models.Current_Product_List                ),
            typeof(NorthwindDataLayer.Models.Customer                            ),
            typeof(NorthwindDataLayer.Models.Customer_and_Suppliers_by_City      ),
            typeof(NorthwindDataLayer.Models.CustomerDemographic                 ),
            typeof(NorthwindDataLayer.Models.Employee                            ),
            typeof(NorthwindDataLayer.Models.Invoice                             ),
            typeof(NorthwindDataLayer.Models.Order                               ),
            typeof(NorthwindDataLayer.Models.Order_Detail                        ),
            typeof(NorthwindDataLayer.Models.Order_Details_Extended              ),
            typeof(NorthwindDataLayer.Models.Order_Subtotal                      ),
            typeof(NorthwindDataLayer.Models.Orders_Qry                          ),
            typeof(NorthwindDataLayer.Models.Product                             ),
            typeof(NorthwindDataLayer.Models.Product_Sales_for_1997              ),
            typeof(NorthwindDataLayer.Models.Products_Above_Average_Price        ),
            typeof(NorthwindDataLayer.Models.Products_by_Category                ),
            typeof(NorthwindDataLayer.Models.Region                              ),
            typeof(NorthwindDataLayer.Models.Sales_by_Category                   ),
            typeof(NorthwindDataLayer.Models.Sales_Totals_by_Amount              ),
            typeof(NorthwindDataLayer.Models.Shipper                             ),
            typeof(NorthwindDataLayer.Models.Summary_of_Sales_by_Quarter         ),
            typeof(NorthwindDataLayer.Models.Summary_of_Sales_by_Year            ),
            typeof(NorthwindDataLayer.Models.Supplier                            ),
            typeof(NorthwindDataLayer.Models.Territory                           ),
        };

        static void Main(string[] args)
        {
            //Visit(new Type[] { typeof(Lot), typeof(Article) });
            //Visit(_domainTypes1);
            //Visit(_domainTypes2);

            //var models1 = Visit(_domainTypes1, new[] { typeof(ERP_Model.Models.Supplier) });

            VisitCompare1();
            //VisitCompare2a();
            //VisitCompare2b();
            //VisitCompare2c();
        }

        static void VisitCompare2a()
        {
            var domain = new GeneratedCode.Domain();
            var modelsDomain1 = Visit(domain, _domainTypes1);

            var modelsNW = Visit(domain, _domainTypesNW, new[] { typeof(NorthwindDataLayer.Models.Supplier) });
            var modelNW = modelsNW.Single();
            DumpType(modelNW, "source");

            var matcher = new ConceptMatchingRule(true);
            matcher.ComputeMappings(modelNW, modelsDomain1);
            
        }

        static void VisitCompare2b()
        {
            var domain = new GeneratedCode.Domain();
            var modelsDomain1 = Visit(domain, _domainTypesNW);

            var modelsNW = Visit(domain, _domainTypes1, new[] { typeof(ERP_Model.Models.OrderItem) });
            var modelNW = modelsNW.Single();
            DumpType(modelNW, "source");

            var matcher = new ConceptMatchingRule(true);
            //matcher.ComputeMappings(model2, modelsDomain1);
            matcher.ComputeMappings(modelNW, modelsDomain1);
            
        }

        static void VisitCompare2c()
        {
            var domain = new GeneratedCode.Domain();
            var modelsDomain1 = Visit(domain, _domainTypes1);

            var modelsNW = Visit(domain, _domainTypesNW, new[] { typeof(NorthwindDataLayer.Models.Order) });
            var modelNW = modelsNW.Single();
            DumpType(modelNW, "source");

            var matcher = new ConceptMatchingRule(true);
            //matcher.ComputeMappings(model2, modelsDomain1);
            matcher.ComputeMappings(modelNW, modelsDomain1);

        }

        static void VisitCompare1()
        {
            var domain = new GeneratedCode.Domain();
            var modelsDomain1 = Visit(domain, _domainTypes1);

            var models2 = Visit(domain, _domainTypes2, new[] { typeof(coderush.Models.Vendor) });
            Debug.Assert(models2.Count == 1);
            var model2 = models2.Single();
            DumpType(model2, "source");

            var matcher = new ConceptMatchingRule(true);
            matcher.ComputeMappings(model2, modelsDomain1);

        }

        // DumpType(..., "source");
        // DumpType(..., "target");
        private static void DumpType(ModelTypeNode type, string typeCategory, string scoring = null)
        {
            if(string.IsNullOrEmpty(scoring))
                Console.WriteLine($"The {typeCategory} type is {type.Type.Name} whose properties are:");
            else
                Console.WriteLine($"The {typeCategory} type is {type.Type.Name} [{scoring}] whose properties are:");
            foreach (var p in type.FlatHierarchyProperties())
                Console.WriteLine($"\t{p.ToString()}");
            Console.WriteLine();
        }

        static IList<ModelTypeNode> Visit(DomainBase domain, Type[] domainTypes, Type[] visitOnlyTheseTypes = null)
        {
            var visitor = new DomainTypesGraphVisitor(domain, domainTypes);
            return visitor.Visit(VisitType, VisitProperty, visitOnlyTheseTypes);

            static void VisitType(ModelTypeNode modelTypeNode)
            {
                Console.WriteLine();
                Console.WriteLine($"Type {modelTypeNode.Type.Name} => [{string.Join(", ", modelTypeNode.CandidateConceptNames)}]");
            }

            static void VisitProperty(ModelPropertyNode modelPropertyNode)
            {
                var conceptSpecifiers = modelPropertyNode.CandidateConceptSpecifierNames
                    .Select(n => (n == KnownBaseConceptSpecifiers.None.Name) ? "N" : n);

                Console.Write($"    - {{P}} {modelPropertyNode.Name.PadRight(30)} => [{string.Join(", ", modelPropertyNode.CandidateConceptNames).PadRight(60)}] ");
                Console.Write($"[{string.Join(", ", conceptSpecifiers).PadRight(25)}]");
                Console.WriteLine($" <== {modelPropertyNode.Property.PropertyType.Name.PadRight(20)} - {modelPropertyNode.PropertyKind.ToString().PadRight(20)} - {modelPropertyNode.CoreType?.Name}");
            }
        }




    }
}
