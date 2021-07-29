using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;

using SemanticLibrary;

namespace DeserializationConsole
{
    public class Program
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

        static Type[] _domain1 = new Type[]
        {
            typeof(SimpleDomain1.Order),
            typeof(SimpleDomain1.OrderItem),
            typeof(SimpleDomain1.Product),
            typeof(SimpleDomain1.Company),
            typeof(SimpleDomain1.Address),
        };

        static Type[] _domain2 = new Type[]
        {
            typeof(SimpleDomain2.OnlineOrder),
            typeof(SimpleDomain2.OrderLine),
        };



        JsonSerializerOptions _settingsVanilla = new JsonSerializerOptions()
        {
            WriteIndented = true,
        };

        static void Main0(string[] args)
        {
            var p = new Program();
            p.Analyzer = new Analyzer();
            var erp = p.Analyzer.Prepare("ERP", _domainTypes1);
            var coderush = p.Analyzer.Prepare("coderush", _domainTypes2);
            var northwind = p.Analyzer.Prepare("Northwind", _domainTypesNW);

            //p.VendorToSupplier(erp, coderush, northwind);
            //p.SupplierToVendor(erp, coderush, northwind);
        }

        static void Main(string[] args)
        {
            var p = new Program();

            //p.VendorToSupplier();
            //p.SupplierToVendor();
            p.MappingOrderToOnlineOrder();
            //p.MappingOnlineOrderToOrder();
        }

        public Analyzer Analyzer { get; set; }

        public string GetJson<T>(T[] item) => JsonSerializer.Serialize(item, _settingsVanilla);

        public object FromJson(string json, Type type, JsonSerializerOptions options) => JsonSerializer.Deserialize(json, type, options);

        public JsonSerializerOptions CreateSettings(ScoredTypeMapping scoredTypeMapping)
            => new JsonSerializerOptions()
            {
                WriteIndented = true,
                Converters =
                {
                    new CodeGenerationLibrary.Serialization.TesterConverterFactory(scoredTypeMapping),
                },
            };

        public void VendorToSupplier()
        {
            var analyzer = new Analyzer();
            var erp = analyzer.Prepare("ERP", _domainTypes1);
            var coderush = analyzer.Prepare("coderush", _domainTypes2);
            var northwind = analyzer.Prepare("Northwind", _domainTypesNW);

            TestMaterializer("Vendor", analyzer, coderush, erp);

            var sourceObjects = Samples.GetVendors1();
            var utilities = new MappingUtilities();
            var targetObjects = utilities.Transform<coderush.Models.Vendor, ERP_Model.Models.Supplier>("Vendor", coderush, erp, sourceObjects);
        }

        public void SupplierToVendor()
        {
            var analyzer = new Analyzer();
            var erp = analyzer.Prepare("ERP", _domainTypes1);
            var coderush = analyzer.Prepare("coderush", _domainTypes2);
            var northwind = analyzer.Prepare("Northwind", _domainTypesNW);

            TestMaterializer("Supplier", analyzer, erp, coderush);

            var sourceObjects = Samples.GetSupplier1();
            var utilities = new MappingUtilities();
            var targetObjects = utilities.Transform<ERP_Model.Models.Supplier, coderush.Models.Vendor>("Supplier", erp, coderush, sourceObjects);
        }

        public void VendorToSupplier(IList<ModelTypeNode> erp, IList<ModelTypeNode> coderush, IList<ModelTypeNode> northwind)
        {
            var vendor = coderush.First(t => t.TypeName == "Vendor");
            var mapping = Analyzer.CreateMappingsFor(vendor, erp);
            var settings = CreateSettings(mapping);

            TestMaterializer("Vendor", Analyzer, coderush, erp);


            var sourceObjects = Samples.GetVendors1();
            var json = GetJson(sourceObjects);
            var clone = JsonSerializer.Deserialize(json, typeof(coderush.Models.Vendor[]));
            var targetObjects = FromJson(json, typeof(ERP_Model.Models.Supplier[]), settings);
        }

        public void SupplierToVendor(IList<ModelTypeNode> erp, IList<ModelTypeNode> coderush, IList<ModelTypeNode> northwind)
        {
            var supplier = erp.First(t => t.TypeName == "Supplier");
            var mapping = Analyzer.CreateMappingsFor(supplier, coderush);
            var settings = CreateSettings(mapping);

            TestMaterializer("Supplier", Analyzer, erp, coderush);

            var sourceObjects = Samples.GetSupplier1();
            var json = GetJson(sourceObjects);
            var targetObjects = FromJson(json, typeof(coderush.Models.Vendor[]), settings);
        }


        public void MappingOrderToOnlineOrder()
        {
            var analyzer = new Analyzer();
            var m1 = analyzer.Prepare("SimpleDomain1", _domain1);
            var m2 = analyzer.Prepare("SimpleDomain2", _domain2);

            TestMaterializer("Order", analyzer, m1, m2);

            var sourceObjects = SimpleDomain1.Samples.GetOrders();
            var utilities = new MappingUtilities();
            var targetObjects = utilities.OrderToOnlineOrder(m1, m2, sourceObjects);
        }


        public void MappingOnlineOrderToOrder()
        {
            var analyzer = new Analyzer();
            var m1 = analyzer.Prepare("SimpleDomain1", _domain1);
            var m2 = analyzer.Prepare("SimpleDomain2", _domain2);

            TestMaterializer("OnlineOrder", analyzer, m2, m1);

            var sourceObjects = SimpleDomain2.Samples.GetOrders();
            var utilities = new MappingUtilities();
            var targetObjects = utilities.OnlineOrderToOrder(m2, m1, sourceObjects);

        }


        public void TestMaterializer(string sourceItem, Analyzer analyzer,IList<ModelTypeNode> source, IList<ModelTypeNode> target)
        {
            var sourceModel = source.First(t => t.TypeName == sourceItem);
            var mapping = analyzer.CreateMappingsFor(sourceModel, target);

            var materializer = new Materializer();
            materializer.Materialize(mapping);

        }
    }
}




