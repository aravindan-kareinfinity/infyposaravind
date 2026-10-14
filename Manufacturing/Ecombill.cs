using InfyPOS.Common;
using InfyPOS.Processors;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
namespace InfyPOS.Models
{
    public class Ecombill : BaseEntity
    {
        public Ecombill()
        {

        }
        public Ecombill(ECOMSalesOrderItem source)
        {
            orderdate = source.OrderDate;
            unitprice = source.UnitPrice;
            orderqty = source.OrderQty;
            voucheramount = source.VoucherAmount;
            lineamount = source.LineAmount;
            maxretailprice = source.maxRetailPrice;
            customername = source.CustomerName;
            shipaddress = source.ShipAddress;
            shipcity = source.ShipCity;
            shipphone = source.ShipPhone;
            shippincode = source.ShipPinCode;
            dispatchon = source.DispatchOn;
            shipcode = source.ShipCode;
            invoicecode = source.InvoiceCode;
            invoicedisplaycode = source.InvoiceCode;
            color = source.color;
            brand = source.brand;
            size = source.size;
            itemname = source.itemName;
            address = source.ShipAddress;
            shipstate = source.ShipState;
            shippingpackagestatus = source.shippingPackageStatus;
            statuscode = source.statusCode;
            orderno = source.OrderNo;
            company = source.Company;
            location = source.Location;
            channel = source.Channel;
            externorderno = source.ExternOrderNo;
            cancellationreason = source.cancellationReason;
            billno = source.BillNo;
            sku = source.SKU;
        }

        public string billno { get; set; }
        public long id { get; set; }
        public bool isactive { get; set; }
        public DateTime modifiedon { get; set; }
        public DateTime createdon { get; set; }
        public long createdby { get; set; }
        public long modifiedby { get; set; }
        public DateTime orderdate { get; set; }
        public decimal unitprice { get; set; }
        public decimal orderqty { get; set; }
        public decimal voucheramount { get; set; }
        public decimal lineamount { get; set; }
        public decimal maxretailprice { get; set; }
        public int version { get; set; }
        public string customername { get; set; }
        public string shipaddress { get; set; }
        public string shipcity { get; set; }
        public string shippincode { get; set; }
        public string shipphone { get; set; }
        public string color { get; set; }
        public string brand { get; set; }
        public string size { get; set; }
        public string itemname { get; set; }
        public string address { get; set; }
        public string shipstate { get; set; }
        public string shippingpackagestatus { get; set; }
        public string statuscode { get; set; }
        public string orderno { get; set; }
        public string company { get; set; }
        public string location { get; set; }
        public string channel { get; set; }
        public string externorderno { get; set; }
        public string cancellationreason { get; set; }
        public string sku { get; set; }
        public DateTime importedon { get; set; }
        public long importedby { get; set; }
        public string imported { get; set; }
        public DateTime dispatchon { get; set; }
        public string invoicecode { get; set; }
        public string shipcode { get; set; }
        public string invoicedisplaycode { get; set; }

        internal ExcelImportManager.ECommerceOrderItem ToEcommerceOrderItem()
        {
            return new ExcelImportManager.ECommerceOrderItem()
            {
                Company = this.company, 
                Location = this.location, 
                BillDate = this.dispatchon,
                Qty = this.orderqty,
                UAN = "",
                SKU = this.sku,
                CONTACTNO = shipphone,
                CITY = shipcity,
                PRICE = unitprice,
                DISCOUNT = voucheramount,
                AMOUNT = lineamount,
                CUSTOMER = customername,
                ADDRESS = address,
                PINCODE = shippincode,
                Channel = channel,
                STATE = shipstate,
                MOrderNo = orderno,
                dispatchon = dispatchon,
                OrderNo = externorderno,
                invoicecode = invoicecode,
                invoicedisplaycode = invoicedisplaycode
            };
        }
    }
    public class EcombillCreteria : Ecombill
    {
        public string bundlecode { get; set; }
        public bool Online { get; set; }
        public string datetype { get; set; }
        public int minutes { get; set; }
        public long locationid { get; set; }
        public string status { get; set; }
        public DateTime addedfrom { get; set; }
        public DateTime addedto { get; set; }
        public List<long> ids { get; set; }
    }
}
