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

        JsonSerializerOptions _settingsVanilla = new JsonSerializerOptions()
        {
            WriteIndented = true,
        };

        static void Main(string[] args)
        {
            var p = new Program();
            p.Analyzer = new Analyzer();
            var erp = p.Analyzer.Prepare("ERP", _domainTypes1);
            var coderush = p.Analyzer.Prepare("coderush", _domainTypes2);
            var northwind = p.Analyzer.Prepare("Northwind", _domainTypesNW);

            p.VendorToSupplier(erp, coderush, northwind);
            //p.SupplierToVendor(erp, coderush, northwind);
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

        public void VendorToSupplier(IList<ModelTypeNode> erp, IList<ModelTypeNode> coderush, IList<ModelTypeNode> northwind)
        {
            var vendor = coderush.First(t => t.TypeName == "Vendor");
            var mapping = Analyzer.CreateMappingsFor(vendor, erp);
            var settings = CreateSettings(mapping);

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

            var sourceObjects = Samples.GetSupplier1();
            var json = GetJson(sourceObjects);
            var targetObjects = FromJson(json, typeof(coderush.Models.Vendor[]), settings);
        }

    }
}
