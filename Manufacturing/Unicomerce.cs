using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using PdfSharp.Pdf.Content.Objects;
using PdfSharp.Pdf.Content;
using PdfSharp.Pdf.IO;
using PdfSharp.Pdf;
using InfyPOS.Common;

namespace InfyPOS.Processors
{

    public class ECOMSalesOrderItem
    {
        public string InvoiceDisplayCode { get; set; }

        public string BillNo { get; set; }
        public string OrderNo { get; set; }
        public string Company { get; set; }
        public string Location { get; set; }
        public string Channel { get; set; }
        public string ExternOrderNo { get; set; }
        public DateTime OrderDate { get; set; }
        public string SKU { get; set; }
        public string CustomerName { get; set; }
        public string ShipAddress { get; set; }
        public string ShipCity { get; set; }
        public string ShipPinCode { get; set; }
        public string ShipPhone { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal OrderQty { get; set; }
        public decimal VoucherAmount { get; set; }
        public string ShipState { get; set; }
        public decimal LineAmount { get; set; }
        public string shippingPackageStatus { get; set; }
        public string statusCode { get; set; }
        public string cancellationReason { get; set; }
        public string color { get; set; }
        public string brand { get; set; }
        public string size { get; set; }
        public string itemName { get; set; }
        public decimal maxRetailPrice { get; set; }
        public string InvoiceCode { get; set; }
        public string ShipCode { get; set; }
        public DateTime DispatchOn { get; set; }
    }
    public class Unicomerce
    {
        public partial class UM_Authenticate
        {
            [JsonProperty("access_token")]
            public Guid access_token { get; set; }

            [JsonProperty("token_type")]
            public string token_type { get; set; }

            [JsonProperty("refresh_token")]
            public Guid refresh_token { get; set; }

            [JsonProperty("expires_in")]
            public long expires_in { get; set; }

            [JsonProperty("scope")]
            public string scope { get; set; }
        }


        public partial class UM_SearchItemTypeRequest
        {
            [JsonProperty("keyword")]
            public string keyword { get; set; }

            [JsonProperty("productCode")]
            public string productCode { get; set; }

            [JsonProperty("categoryCode")]
            public string categoryCode { get; set; }

            [JsonProperty("getInventorySnapshot")]
            public bool getInventorySnapshot { get; set; }

            [JsonProperty("searchOptions")]
            public SearchOptions searchOptions { get; set; }

            public partial class SearchOptions
            {
                [JsonProperty("searchKey")]
                public string searchKey { get; set; }

                [JsonProperty("displayLength")]
                public long displayLength { get; set; }

                [JsonProperty("displayStart")]
                public long displayStart { get; set; }

                [JsonProperty("columns")]
                public long columns { get; set; }

                [JsonProperty("sortingCols")]
                public long sortingCols { get; set; }

                [JsonProperty("sortColumnIndex")]
                public long sortColumnIndex { get; set; }

                [JsonProperty("sortDirection")]
                public string sortDirection { get; set; }

                [JsonProperty("columnNames")]
                public string columnNames { get; set; }

                [JsonProperty("getCount")]
                public bool getCount { get; set; }
            }

        }

        public partial class UM_SearchItemTypeResponse
        {
            [JsonProperty("successful")]
            public bool successful { get; set; }

            [JsonProperty("message")]
            public string message { get; set; }

            [JsonProperty("errors")]
            public Error[] errors { get; set; }

            [JsonProperty("warnings")]
            public Warning[] warnings { get; set; }

            [JsonProperty("totalRecords")]
            public long totalRecords { get; set; }

            [JsonProperty("elements")]
            public Element[] elements { get; set; }


            public partial class Element
            {
                [JsonProperty("skuCode")]
                public string skuCode { get; set; }

                [JsonProperty("name")]
                public string name { get; set; }

                [JsonProperty("categoryName")]
                public string categoryName { get; set; }

                [JsonProperty("itemTypeImageUrl")]
                public string itemTypeImageUrl { get; set; }

                [JsonProperty("itemTypePageUrl")]
                public string itemTypePageUrl { get; set; }

                [JsonProperty("openSale")]
                public long openSale { get; set; }

                [JsonProperty("openPurchase")]
                public long openPurchase { get; set; }

                [JsonProperty("inventory")]
                public long inventory { get; set; }

                [JsonProperty("putawayPending")]
                public long putawayPending { get; set; }

                [JsonProperty("inPicking")]
                public long inPicking { get; set; }

                [JsonProperty("pendingGRN")]
                public long pendingGRN { get; set; }

                [JsonProperty("enabled")]
                public bool enabled { get; set; }

                [JsonProperty("quantityNotFound")]
                public long quantityNotFound { get; set; }

                [JsonProperty("quantityDamaged")]
                public long quantityDamaged { get; set; }

                [JsonProperty("badInventoryCount")]
                public long badInventoryCount { get; set; }

                [JsonProperty("vendorItemTypes")]
                public VendorItemType[] vendorItemTypes { get; set; }

            }



            public partial class Error
            {
                [JsonProperty("code")]
                public long code { get; set; }

                [JsonProperty("fieldName")]
                public string fieldName { get; set; }

                [JsonProperty("description")]
                public string description { get; set; }

                [JsonProperty("message")]
                public string message { get; set; }

                [JsonProperty("errorParams")]
                public ErrorParams errorParams { get; set; }
            }

            public partial class ErrorParams
            {
                [JsonProperty("additionalProp1")]
                public AdditionalProp additionalProp1 { get; set; }

                [JsonProperty("additionalProp2")]
                public AdditionalProp additionalProp2 { get; set; }

                [JsonProperty("additionalProp3")]
                public AdditionalProp additionalProp3 { get; set; }
            }

            public partial class AdditionalProp
            {
            }

            public partial class Warning
            {
                [JsonProperty("code")]
                public long code { get; set; }

                [JsonProperty("message")]
                public string message { get; set; }

                [JsonProperty("description")]
                public string description { get; set; }
            }

            public partial class VendorItemType
            {
                [JsonProperty("vendorEnabled")]
                public bool vendorEnabled { get; set; }

                [JsonProperty("vendorCode")]
                public string vendorCode { get; set; }

                [JsonProperty("vendorName")]
                public string vendorName { get; set; }

                [JsonProperty("vendorSkuCode")]
                public string vendorSkuCode { get; set; }

                [JsonProperty("vendorId")]
                public long vendorId { get; set; }
            }

        }


        public partial class UniAdjustBulkResponse
        {
            [JsonProperty("successful")]
            public bool successful { get; set; }

            [JsonProperty("message")]
            public string message { get; set; }

            [JsonProperty("errors")]
            public Error[] errors { get; set; }

            [JsonProperty("warnings")]
            public Warning[] warnings { get; set; }

            [JsonProperty("inventoryAdjustmentResponses")]
            public InventoryAdjustmentResponse[] inventoryAdjustmentResponses { get; set; }

            public partial class Error
            {
                [JsonProperty("code")]
                public long code { get; set; }

                [JsonProperty("fieldName")]
                public string fieldName { get; set; }

                [JsonProperty("description")]
                public string description { get; set; }

                [JsonProperty("message")]
                public string message { get; set; }

                [JsonProperty("errorParams")]
                public ErrorParams errorParams { get; set; }
            }

            public partial class ErrorParams
            {
                [JsonProperty("additionalProp1")]
                public AdditionalProp additionalProp1 { get; set; }

                [JsonProperty("additionalProp2")]
                public AdditionalProp additionalProp2 { get; set; }

                [JsonProperty("additionalProp3")]
                public AdditionalProp additionalProp3 { get; set; }
            }

            public partial class AdditionalProp
            {
            }

            public partial class InventoryAdjustmentResponse
            {
                [JsonProperty("facilityInventoryAdjustment")]
                public FacilityInventoryAdjustment facilityInventoryAdjustment { get; set; }

                [JsonProperty("successful")]
                public bool successful { get; set; }

                [JsonProperty("errors")]
                public Error[] errors { get; set; }
            }

            public partial class FacilityInventoryAdjustment
            {
                [JsonProperty("itemSKU")]
                public string itemSKU { get; set; }

                [JsonProperty("quantity")]
                public long quantity { get; set; }

                [JsonProperty("shelfCode")]
                public string shelfCode { get; set; }

                [JsonProperty("inventoryType")]
                public string inventoryType { get; set; }

                [JsonProperty("transferToShelfCode")]
                public string transferToShelfCode { get; set; }

                [JsonProperty("sla")]
                public long sla { get; set; }

                [JsonProperty("adjustmentType")]
                public string adjustmentType { get; set; }

                [JsonProperty("remarks")]
                public string remarks { get; set; }

                [JsonProperty("forceAllocate")]
                public bool forceAllocate { get; set; }

                [JsonProperty("batchDetails")]
                public object batchDetails { get; set; }

                [JsonProperty("calculationTime")]
                public object calculationTime { get; set; }

                [JsonProperty("facilityCode")]
                public string facilityCode { get; set; }
            }

            public partial class Warning
            {
                [JsonProperty("code")]
                public long code { get; set; }

                [JsonProperty("message")]
                public string message { get; set; }

                [JsonProperty("description")]
                public string description { get; set; }
            }

        }

        public partial class UniAdjustBulk
        {
            [JsonProperty("inventoryAdjustments")]
            public List<InventoryAdjustment> inventoryAdjustments { get; set; }

            [JsonProperty("forceAllocate")]
            public bool forceAllocate { get; set; }

            public partial class InventoryAdjustment
            {
                [JsonProperty("itemSKU")]
                public string itemSKU { get; set; }

                [JsonProperty("quantity")]
                public long quantity { get; set; }

                [JsonProperty("shelfCode")]
                public string shelfCode { get; set; }

                [JsonProperty("inventoryType")]
                public string inventoryType { get; set; }

                [JsonProperty("adjustmentType")]
                public string adjustmentType { get; set; }

                [JsonProperty("remarks")]
                public string remarks { get; set; }

                [JsonProperty("facilityCode")]
                public string facilityCode { get; set; }
            }
        }


        public partial class StockSnapshotRequet
        {
            [JsonProperty("itemTypeSKUs")]
            public string[] itemTypeSKUs { get; set; }

            [JsonProperty("updatedSinceInMinutes")]
            public long? updatedSinceInMinutes { get; set; }
        }

        public partial class StockSnapshotResponse
        {
            [JsonProperty("successful")]
            public bool successful { get; set; }

            [JsonProperty("message")]
            public string message { get; set; }

            [JsonProperty("errors")]
            public Error[] errors { get; set; }

            [JsonProperty("warnings")]
            public Warning[] warnings { get; set; }

            [JsonProperty("inventorySnapshots")]
            public InventorySnapshot[] inventorySnapshots { get; set; }

            public partial class Error
            {
                [JsonProperty("code")]
                public long code { get; set; }

                [JsonProperty("fieldName")]
                public string fieldName { get; set; }

                [JsonProperty("description")]
                public string description { get; set; }

                [JsonProperty("message")]
                public string message { get; set; }

                [JsonProperty("errorParams")]
                public ErrorParams errorParams { get; set; }
            }

            public partial class ErrorParams
            {
                [JsonProperty("additionalProp1")]
                public AdditionalProp additionalProp1 { get; set; }

                [JsonProperty("additionalProp2")]
                public AdditionalProp additionalProp2 { get; set; }

                [JsonProperty("additionalProp3")]
                public AdditionalProp additionalProp3 { get; set; }
            }

            public partial class AdditionalProp
            {
            }

            public partial class InventorySnapshot
            {
                [JsonProperty("itemTypeSKU")]
                public string itemTypeSKU { get; set; }

                [JsonProperty("inventory")]
                public long inventory { get; set; }

                [JsonProperty("openSale")]
                public long openSale { get; set; }

                [JsonProperty("openPurchase")]
                public long openPurchase { get; set; }

                [JsonProperty("putawayPending")]
                public long putawayPending { get; set; }

                [JsonProperty("inventoryBlocked")]
                public long inventoryBlocked { get; set; }

                [JsonProperty("pendingStockTransfer")]
                public long pendingStockTransfer { get; set; }

                [JsonProperty("vendorInventory")]
                public long vendorInventory { get; set; }

                [JsonProperty("virtualInventory")]
                public long virtualInventory { get; set; }

                [JsonProperty("pendingInventoryAssessment")]
                public long pendingInventoryAssessment { get; set; }
            }

            public partial class Warning
            {
                [JsonProperty("code")]
                public long code { get; set; }

                [JsonProperty("message")]
                public string message { get; set; }

                [JsonProperty("description")]
                public string description { get; set; }
            }
        }


        public partial class SearchSalesOrderRequest
        {

            [JsonProperty("displayOrderCode")]
            public string displayOrderCode { get; set; }

            [JsonProperty("status")]
            public string status { get; set; }

            [JsonProperty("channel")]
            public string channel { get; set; }

            [JsonProperty("customerEmailOrMobile")]
            public string customerEmailOrMobile { get; set; }

            [JsonProperty("customerName")]
            public string customerName { get; set; }

            [JsonProperty("cashOnDelivery")]
            public bool? cashOnDelivery { get; set; }

            [JsonProperty("fromDate")]
            public DateTime fromDate { get; set; }

            [JsonProperty("toDate")]
            public DateTime toDate { get; set; }

            [JsonProperty("dateType")]
            public string dateType { get; set; }

            [JsonProperty("facilityCodes")]
            public string[] facilityCodes { get; set; }

            [JsonProperty("returnStatuses")]
            public string[] returnStatuses { get; set; }

            [JsonProperty("searchOptions")]
            public SearchOptions searchOptions { get; set; }

            [JsonProperty("updatedSinceInMinutes")]
            public long? updatedSinceInMinutes { get; set; }

            [JsonProperty("onHold")]
            public bool? onHold { get; set; }
            public partial class SearchOptions
            {
                [JsonProperty("searchKey")]
                public string searchKey { get; set; }

                [JsonProperty("displayLength")]
                public long displayLength { get; set; }

                [JsonProperty("displayStart")]
                public long displayStart { get; set; }

                [JsonProperty("columns")]
                public long columns { get; set; }

                [JsonProperty("sortingCols")]
                public long sortingCols { get; set; }

                [JsonProperty("sortColumnIndex")]
                public long sortColumnIndex { get; set; }

                [JsonProperty("sortDirection")]
                public string sortDirection { get; set; }

                [JsonProperty("columnNames")]
                public string columnNames { get; set; }

                [JsonProperty("getCount")]
                public bool getCount { get; set; }
            }
        }

        public partial class SearchSalesOrderResponse
        {
            [JsonProperty("successful")]
            public bool successful { get; set; }

            [JsonProperty("message")]
            public string message { get; set; }

            [JsonProperty("errors")]
            public Error[] errors { get; set; }

            [JsonProperty("warnings")]
            public Warning[] warnings { get; set; }

            [JsonProperty("totalRecords")]
            public long? totalRecords { get; set; }

            [JsonProperty("elements")]
            public Element[] elements { get; set; }


            public partial class Element
            {
                [JsonProperty("code")]
                public string code { get; set; }

                [JsonProperty("displayOrderCode")]
                public string displayOrderCode { get; set; }

                [JsonProperty("channel")]
                public string channel { get; set; }

                [JsonProperty("source")]
                public string source { get; set; }

                [JsonProperty("displayOrderDateTime")]
                public long displayOrderDateTime { get; set; }

                [JsonProperty("status")]
                public string status { get; set; }

                [JsonProperty("created")]
                public long created { get; set; }

                [JsonProperty("updated")]
                public long updated { get; set; }

                [JsonProperty("fulfillmentTat")]
                public long fulfillmentTat { get; set; }

                [JsonProperty("notificationEmail")]
                public string notificationEmail { get; set; }

                [JsonProperty("notificationMobile")]
                public string notificationMobile { get; set; }

                [JsonProperty("customerGSTIN")]
                public string customerGSTIN { get; set; }

                [JsonProperty("channelProcessingTime")]
                public long channelProcessingTime { get; set; }
            }

            public partial class Error
            {
                [JsonProperty("code")]
                public long code { get; set; }

                [JsonProperty("fieldName")]
                public string fieldName { get; set; }

                [JsonProperty("description")]
                public string description { get; set; }

                [JsonProperty("message")]
                public string message { get; set; }

                [JsonProperty("errorParams")]
                public ErrorParams errorParams { get; set; }
            }

            public partial class ErrorParams
            {
                [JsonProperty("additionalProp1")]
                public AdditionalProp additionalProp1 { get; set; }

                [JsonProperty("additionalProp2")]
                public AdditionalProp additionalProp2 { get; set; }

                [JsonProperty("additionalProp3")]
                public AdditionalProp additionalProp3 { get; set; }
            }

            public partial class AdditionalProp
            {
            }

            public partial class Warning
            {
                [JsonProperty("code")]
                public long code { get; set; }

                [JsonProperty("message")]
                public string message { get; set; }

                [JsonProperty("description")]
                public string description { get; set; }
            }
        }


        public partial class GetSalesOrderRequest
        {
            [JsonProperty("code")]
            public string code { get; set; }

            [JsonProperty("facilityCodes")]
            public string[] facilityCodes { get; set; }

            [JsonProperty("paymentDetailRequired")]
            public bool paymentDetailRequired { get; set; }

        }
        public partial class GetSalesOrderResponse
        {
            [JsonProperty("successful")]
            public bool successful { get; set; }

            [JsonProperty("message")]
            public string message { get; set; }

            [JsonProperty("errors")]
            public Error[] errors { get; set; }

            [JsonProperty("warnings")]
            public Warning[] warnings { get; set; }

            [JsonProperty("saleOrderDTO")]
            public SaleOrderDto saleOrderDTO { get; set; }

            [JsonProperty("failedOrderFetch")]
            public bool failedOrderFetch { get; set; }

            [JsonProperty("refreshEnabled")]
            public bool refreshEnabled { get; set; }


            public partial class Error
            {
                [JsonProperty("code")]
                public long code { get; set; }

                [JsonProperty("fieldName")]
                public string fieldName { get; set; }

                [JsonProperty("description")]
                public string description { get; set; }

                [JsonProperty("message")]
                public string message { get; set; }

                [JsonProperty("errorParams")]
                public ErrorParams errorParams { get; set; }
            }

            public partial class ErrorParams
            {
                [JsonProperty("additionalProp1")]
                public AdditionalProp additionalProp1 { get; set; }

                [JsonProperty("additionalProp2")]
                public AdditionalProp additionalProp2 { get; set; }

                [JsonProperty("additionalProp3")]
                public AdditionalProp additionalProp3 { get; set; }
            }

            public partial class AdditionalProp
            {
            }

            public partial class SaleOrderDto
            {
                [JsonProperty("code")]
                public string code { get; set; }

                [JsonProperty("displayOrderCode")]
                public string displayOrderCode { get; set; }

                [JsonProperty("channel")]
                public string channel { get; set; }

                [JsonProperty("source")]
                public string source { get; set; }

                [JsonProperty("displayOrderDateTime")]
                public long displayOrderDateTime { get; set; }

                [JsonProperty("channelProcessingTime")]
                public long channelProcessingTime { get; set; }

                [JsonProperty("status")]
                public string status { get; set; }

                [JsonProperty("created")]
                public long created { get; set; }

                [JsonProperty("updated")]
                public long updated { get; set; }

                [JsonProperty("fulfillmentTat")]
                public long fulfillmentTat { get; set; }

                [JsonProperty("notificationEmail")]
                public string notificationEmail { get; set; }

                [JsonProperty("notificationMobile")]
                public string notificationMobile { get; set; }

                [JsonProperty("customerGSTIN")]
                public string customerGSTIN { get; set; }

                [JsonProperty("cod")]
                public bool cod { get; set; }

                [JsonProperty("thirdPartyShipping")]
                public bool thirdPartyShipping { get; set; }

                [JsonProperty("priority")]
                public long priority { get; set; }

                [JsonProperty("currencyCode")]
                public string currencyCode { get; set; }

                [JsonProperty("customerCode")]
                public string customerCode { get; set; }

                [JsonProperty("billingAddress")]
                public Address billingAddress { get; set; }

                [JsonProperty("addresses")]
                public Address[] addresses { get; set; }

                [JsonProperty("shippingPackages")]
                public ShippingPackage[] shippingPackages { get; set; }

                [JsonProperty("saleOrderItems")]
                public SaleOrderItem[] saleOrderItems { get; set; }

                [JsonProperty("returns")]
                public Return[] returns { get; set; }

                [JsonProperty("customFieldValues")]
                public string[] customFieldValues { get; set; }

                [JsonProperty("cancellable")]
                public bool cancellable { get; set; }

                [JsonProperty("reversePickable")]
                public bool reversePickable { get; set; }

                [JsonProperty("packetConfigurable")]
                public bool packetConfigurable { get; set; }

                [JsonProperty("cFormProvided")]
                public bool cFormProvided { get; set; }

                [JsonProperty("totalDiscount")]
                public string totalDiscount { get; set; }

                [JsonProperty("totalShippingCharges")]
                public string totalShippingCharges { get; set; }

                [JsonProperty("additionalInfo")]
                public string additionalInfo { get; set; }

                [JsonProperty("paymentDetail")]
                public PaymentDetail paymentDetail { get; set; }
            }

            public partial class Address
            {
                [JsonProperty("id")]
                public long id { get; set; }

                [JsonProperty("name")]
                public string name { get; set; }

                [JsonProperty("addressLine1")]
                public string addressLine1 { get; set; }

                [JsonProperty("addressLine2")]
                public string addressLine2 { get; set; }

                [JsonProperty("city")]
                public string city { get; set; }

                [JsonProperty("state")]
                public string state { get; set; }

                [JsonProperty("country")]
                public string country { get; set; }

                [JsonProperty("pincode")]
                [JsonConverter(typeof(StringOrNumberConverter))]
                public string pincode { get; set; }

                [JsonProperty("phone")]
                public string phone { get; set; }

                [JsonProperty("email")]
                public string email { get; set; }

                [JsonProperty("type")]
                public string type { get; set; }
            }

            public partial class PaymentDetail
            {
                [JsonProperty("paymentMode")]
                public string paymentMode { get; set; }

                [JsonProperty("transactionId")]
                public string transactionId { get; set; }

                [JsonProperty("amountPaid")]
                public long amountPaid { get; set; }
            }

            public partial class Return
            {
                [JsonProperty("code")]
                public string code { get; set; }

                [JsonProperty("statusCode")]
                public string statusCode { get; set; }

                [JsonProperty("shippingProvider")]
                public string shippingProvider { get; set; }

                [JsonProperty("trackingNumber")]
                public string trackingNumber { get; set; }

                [JsonProperty("trackingStatus")]
                public string trackingStatus { get; set; }

                [JsonProperty("providerStatus")]
                public string providerStatus { get; set; }

                [JsonProperty("putawayCode")]
                public string putawayCode { get; set; }

                [JsonProperty("returnInvoiceCode")]
                public string returnInvoiceCode { get; set; }

                [JsonProperty("returnInvoiceDisplayCode")]
                public string returnInvoiceDisplayCode { get; set; }

                [JsonProperty("actionCode")]
                public string actionCode { get; set; }

                [JsonProperty("type")]
                public string type { get; set; }

                [JsonProperty("returnItems")]
                public ReturnItem[] returnItems { get; set; }

                [JsonProperty("returnCompletedDate")]
                public long returnCompletedDate { get; set; }

                [JsonProperty("inventoryReceivedDate")]
                public long inventoryReceivedDate { get; set; }

                [JsonProperty("returnFacilityCode")]
                public string returnFacilityCode { get; set; }
            }

            public partial class ReturnItem
            {
                [JsonProperty("saleOrderItemCode")]
                public string saleOrderItemCode { get; set; }

                [JsonProperty("itemName")]
                public string itemName { get; set; }

                [JsonProperty("itemSku")]
                public string itemSku { get; set; }

                [JsonProperty("inventoryType")]
                public string inventoryType { get; set; }
            }

            public partial class SaleOrderItem
            {
                [JsonProperty("id")]
                public long id { get; set; }

                [JsonProperty("shippingPackageCode")]
                public string shippingPackageCode { get; set; }

                [JsonProperty("shippingPackageStatus")]
                public string shippingPackageStatus { get; set; }

                [JsonProperty("facilityCode")]
                public string facilityCode { get; set; }

                [JsonProperty("facilityName")]
                public string facilityName { get; set; }

                [JsonProperty("alternateFacilityCode")]
                public string alternateFacilityCode { get; set; }

                [JsonProperty("reversePickupCode")]
                public string reversePickupCode { get; set; }

                [JsonProperty("shippingAddressId")]
                public long shippingAddressId { get; set; }

                [JsonProperty("packetNumber")]
                public long packetNumber { get; set; }

                [JsonProperty("combinationIdentifier")]
                public string combinationIdentifier { get; set; }

                [JsonProperty("combinationDescription")]
                public string combinationDescription { get; set; }

                [JsonProperty("type")]
                public string type { get; set; }

                [JsonProperty("item")]
                public string item { get; set; }

                [JsonProperty("shippingMethodCode")]
                public string shippingMethodCode { get; set; }

                [JsonProperty("itemName")]
                public string itemName { get; set; }

                [JsonProperty("itemSku")]
                public string itemSku { get; set; }

                [JsonProperty("sellerSkuCode")]
                public string sellerSkuCode { get; set; }

                [JsonProperty("channelProductId")]
                public string channelProductId { get; set; }

                [JsonProperty("imageUrl")]
                public string imageUrl { get; set; }

                [JsonProperty("statusCode")]
                public string statusCode { get; set; }

                [JsonProperty("code")]
                public string code { get; set; }

                [JsonProperty("shelfCode")]
                public string shelfCode { get; set; }

                [JsonProperty("totalPrice")]
                public decimal totalPrice { get; set; }

                [JsonProperty("sellingPrice")]
                public decimal sellingPrice { get; set; }

                [JsonProperty("shippingCharges")]
                public long shippingCharges { get; set; }

                [JsonProperty("shippingMethodCharges")]
                public long shippingMethodCharges { get; set; }

                [JsonProperty("cashOnDeliveryCharges")]
                public long cashOnDeliveryCharges { get; set; }

                [JsonProperty("prepaidAmount")]
                public long prepaidAmount { get; set; }

                [JsonProperty("voucherCode")]
                public string voucherCode { get; set; }

                [JsonProperty("voucherValue")]
                public long voucherValue { get; set; }

                [JsonProperty("storeCredit")]
                public long storeCredit { get; set; }

                [JsonProperty("discount")]
                public long discount { get; set; }

                [JsonProperty("giftWrap")]
                public string giftWrap { get; set; }

                [JsonProperty("giftWrapCharges")]
                public long giftWrapCharges { get; set; }

                [JsonProperty("taxPercentage")]
                public string taxPercentage { get; set; }

                [JsonProperty("giftMessage")]
                public string giftMessage { get; set; }

                [JsonProperty("cancellable")]
                public bool cancellable { get; set; }

                [JsonProperty("editAddress")]
                public bool editAddress { get; set; }

                [JsonProperty("reversePickable")]
                public bool reversePickable { get; set; }

                [JsonProperty("packetConfigurable")]
                public bool packetConfigurable { get; set; }

                [JsonProperty("created")]
                public long created { get; set; }

                [JsonProperty("updated")]
                public long updated { get; set; }

                [JsonProperty("onHold")]
                public bool onHold { get; set; }

                [JsonProperty("saleOrderItemAlternateId")]
                public string saleOrderItemAlternateId { get; set; }

                [JsonProperty("cancellationReason")]
                public string cancellationReason { get; set; }

                [JsonProperty("pageUrl")]
                public string pageUrl { get; set; }

                [JsonProperty("color")]
                public string color { get; set; }

                [JsonProperty("brand")]
                public string brand { get; set; }

                [JsonProperty("size")]
                public string size { get; set; }

                [JsonProperty("replacementSaleOrderCode")]
                public string replacementSaleOrderCode { get; set; }

                [JsonProperty("bundleSkuCode")]
                public string bundleSkuCode { get; set; }

                [JsonProperty("customFieldValues")]
                public string[] customFieldValues { get; set; }

                [JsonProperty("itemDetailFieldDTOList")]
                public string[] itemDetailFieldDTOList { get; set; }

                [JsonProperty("hsnCode")]

                public long hsnCode { get; set; }

                [JsonProperty("totalIntegratedGst")]
                public long totalIntegratedGst { get; set; }

                [JsonProperty("integratedGstPercentage")]
                public long integratedGstPercentage { get; set; }

                [JsonProperty("totalUnionTerritoryGst")]
                public long totalUnionTerritoryGst { get; set; }

                [JsonProperty("unionTerritoryGstPercentage")]
                public long unionTerritoryGstPercentage { get; set; }

                [JsonProperty("totalStateGst")]
                public long totalStateGst { get; set; }

                [JsonProperty("stateGstPercentage")]
                public long stateGstPercentage { get; set; }

                [JsonProperty("totalCentralGst")]
                public long totalCentralGst { get; set; }

                [JsonProperty("centralGstPercentage")]
                public long centralGstPercentage { get; set; }

                [JsonProperty("maxRetailPrice")]
                public long maxRetailPrice { get; set; }

                [JsonProperty("itemDetailFields")]
                public ItemDetailFields itemDetailFields { get; set; }

                [JsonProperty("itemDetailsKey", NullValueHandling = NullValueHandling.Ignore)]
                public string itemDetailsKey { get; set; }
            }

            public partial class ItemDetailFields
            {
                [JsonProperty("itemSealId")]

                public string itemSealId { get; set; }

                [JsonProperty("SerialNumber", NullValueHandling = NullValueHandling.Ignore)]
                public string SerialNumber { get; set; }

                [JsonProperty("Imei", NullValueHandling = NullValueHandling.Ignore)]
                public string Imei { get; set; }
            }

            public partial class ShippingPackage
            {
                [JsonProperty("code")]
                public string code { get; set; }

                [JsonProperty("saleOrderCode")]
                public string saleOrderCode { get; set; }

                [JsonProperty("channel")]
                public string channel { get; set; }

                [JsonProperty("status")]
                public string status { get; set; }

                [JsonProperty("shippingPackageType")]

                public string shippingPackageType { get; set; }

                [JsonProperty("shippingProvider")]
                public string shippingProvider { get; set; }

                [JsonProperty("shippingMethod")]
                public string shippingMethod { get; set; }

                [JsonProperty("trackingNumber")]
                public string trackingNumber { get; set; }

                [JsonProperty("trackingStatus")]
                public string trackingStatus { get; set; }

                [JsonProperty("trackingLink")]
                public string trackingLink { get; set; }

                [JsonProperty("courierStatus")]
                public string courierStatus { get; set; }

                [JsonProperty("estimatedWeight")]
                public decimal? estimatedWeight { get; set; }

                [JsonProperty("actualWeight")]
                public decimal? actualWeight { get; set; }

                [JsonProperty("customer")]
                public string customer { get; set; }

                [JsonProperty("created")]
                public long created { get; set; }

                [JsonProperty("updated")]
                public long updated { get; set; }

                [JsonProperty("dispatched")]
                public long dispatched { get; set; }

                [JsonProperty("delivered")]
                public long delivered { get; set; }

                [JsonProperty("invoice")]
                public long invoice { get; set; }

                [JsonProperty("invoiceCode")]
                public string invoiceCode { get; set; }

                [JsonProperty("invoiceDisplayCode")]
                public string invoiceDisplayCode { get; set; }

                [JsonProperty("noOfItems")]
                public long noOfItems { get; set; }

                [JsonProperty("city")]
                public string city { get; set; }

                [JsonProperty("collectableAmount")]
                public long collectableAmount { get; set; }

                [JsonProperty("collectedAmount")]
                public decimal collectedAmount { get; set; }

                [JsonProperty("paymentReconciled")]
                public bool paymentReconciled { get; set; }

                [JsonProperty("podCode")]
                public string podCode { get; set; }

                [JsonProperty("shippingManifestCode")]
                public string shippingManifestCode { get; set; }

                [JsonProperty("items")]
                public Items items { get; set; }

                //[JsonProperty("customFieldValues")]
                //public string [] customFieldValues { get; set; }

                [JsonProperty("shippingLabelLink")]
                public string shippingLabelLink { get; set; }
            }

            public partial class Items
            {
                [JsonProperty("itemCode")]
                public ItemCode itemCode { get; set; }
            }

            public partial class ItemCode
            {
                [JsonProperty("itemSku")]
                public string itemSku { get; set; }

                [JsonProperty("itemName")]
                public string itemName { get; set; }

                [JsonProperty("itemTypeImageUrl")]
                public string itemTypeImageUrl { get; set; }

                [JsonProperty("itemTypePageUrl")]
                public string itemTypePageUrl { get; set; }

                [JsonProperty("quantity")]
                public long quantity { get; set; }
            }

            public partial class Warning
            {
                [JsonProperty("code")]
                public long code { get; set; }

                [JsonProperty("message")]
                public string message { get; set; }

                [JsonProperty("description")]
                public string description { get; set; }
            }

        }

        //static string serverurl = @"https://demozastores.unicommerce.co.in";
        static string method_authenticate = @"/oauth/token?grant_type=password&client_id=my-trusted-client&username={0}&password={1}";
        static string method_searchitem = @"/services/rest/v1/product/itemType/search";
        static string method_snapshot = @"/services/rest/v1/inventory/inventorySnapshot/get";
        static string method_searchsalesorder = @"/services/rest/v1/oms/saleOrder/search";
        static string method_getsalesorder = @"/services/rest/v1/oms/saleorder/get";
        static string method_adjustbulk = @"/services/rest/v1/inventory/adjust/bulk";
        public static UM_Authenticate Authenticate(string serverurl, string username, string password)
        {
            System.Net.WebClient wc = new System.Net.WebClient();
            var response = wc.DownloadString(serverurl +
                string.Format(method_authenticate, username, password));
            return Newtonsoft.Json.JsonConvert.DeserializeObject<UM_Authenticate>(response, defaultJSONSettings);
        }

        public static UniAdjustBulkResponse AdjustBulk(string serverurl, Guid access_token, 
            List<Unicomerce.QuantoSKUAdjust> sku)
        {

            UniAdjustBulk stockSnapshotRequet = new UniAdjustBulk() {
             forceAllocate= false,
             inventoryAdjustments = new List<UniAdjustBulk.InventoryAdjustment>()
            };

            sku.ForEach(x =>
            {
                stockSnapshotRequet.inventoryAdjustments.Add(new UniAdjustBulk.InventoryAdjustment()
                {
                    adjustmentType = x.add >0 ? "ADD" : (x.remove > 0 ? "REMOVE" : "REPLACE"),
                    facilityCode = "demozastores",
                    inventoryType = "GOOD_INVENTORY",
                    itemSKU = x.prdsku,
                    quantity = x.add > 0 ? x.add : (x.remove > 0 ? x.remove : x.replace),
                    shelfCode = "default"
                });
            });

            var httpWebRequest = (HttpWebRequest)WebRequest.Create(serverurl + method_adjustbulk);
            httpWebRequest.ContentType = "application/json";
            httpWebRequest.Accept = "application/json";
            httpWebRequest.Method = "POST";
            httpWebRequest.KeepAlive = false;
            httpWebRequest.Headers.Add("Authorization", "Bearer " + access_token);
            httpWebRequest.Headers.Add("Facility", "demozastores");
            using (var streamWriter = new StreamWriter(httpWebRequest.GetRequestStream()))
            {
                streamWriter.Write(JsonConvert.SerializeObject(stockSnapshotRequet, defaultJSONSettings));
                streamWriter.Flush();
            }

            string _resCode = string.Empty;
            string _decrypt = string.Empty;
            string _output = string.Empty;
            HttpWebResponse httpResponse = null;
            httpResponse = (HttpWebResponse)httpWebRequest.GetResponse();
            _resCode = Convert.ToString(httpResponse.StatusCode);
            using (var streamReader = new StreamReader(httpResponse.GetResponseStream()))
            {
                _output = streamReader.ReadToEnd();
            }

            return JsonConvert.DeserializeObject<UniAdjustBulkResponse>(_output, defaultJSONSettings);
        }

        public static UM_SearchItemTypeResponse SearchItms(string serverurl, UM_Authenticate token, UM_SearchItemTypeRequest uM_SearchItemType)
        {
            //uM_SearchItemType.categoryCode = "Dupatta";
            uM_SearchItemType.getInventorySnapshot = true;
            uM_SearchItemType.productCode = "Pack of 3 Black & Offwhite & Skin Thighs";
            uM_SearchItemType.searchOptions = new UM_SearchItemTypeRequest.SearchOptions()
            {
                getCount = true
            };
            var httpWebRequest = (HttpWebRequest)WebRequest.Create(serverurl + method_searchitem);
            httpWebRequest.ContentType = "application/json";
            httpWebRequest.Accept = "application/json";
            httpWebRequest.Method = "POST";
            httpWebRequest.KeepAlive = false;
            httpWebRequest.Headers.Add("Authorization", "Bearer " + token.access_token);
            using (var streamWriter = new StreamWriter(httpWebRequest.GetRequestStream()))
            {
                streamWriter.Write(JsonConvert.SerializeObject(uM_SearchItemType));
                streamWriter.Flush();
            }

            string _resCode = string.Empty;
            string _decrypt = string.Empty;
            string _output = string.Empty;
            HttpWebResponse httpResponse = null;
            httpResponse = (HttpWebResponse)httpWebRequest.GetResponse();
            _resCode = Convert.ToString(httpResponse.StatusCode);
            using (var streamReader = new StreamReader(httpResponse.GetResponseStream()))
            {
                _output = streamReader.ReadToEnd();
            }

            return JsonConvert.DeserializeObject<UM_SearchItemTypeResponse>(_output, defaultJSONSettings);

        }

        public static StockSnapshotResponse GetSnapshot(string serverurl, Guid access_token, int minutes, List<string> sku)
        {

            StockSnapshotRequet stockSnapshotRequet = new StockSnapshotRequet() { };
            if (sku != null && sku.Count > 0)
            {
                stockSnapshotRequet = new StockSnapshotRequet() { itemTypeSKUs = sku.ToArray() };
            }
            if(minutes>0)
            {
                stockSnapshotRequet.updatedSinceInMinutes = minutes;
            }

            var httpWebRequest = (HttpWebRequest)WebRequest.Create(serverurl + method_snapshot);
            httpWebRequest.ContentType = "application/json";
            httpWebRequest.Accept = "application/json";
            httpWebRequest.Method = "POST";
            httpWebRequest.KeepAlive = false;
            httpWebRequest.Headers.Add("Authorization", "Bearer " + access_token);
            httpWebRequest.Headers.Add("Facility", "demozastores");
            using (var streamWriter = new StreamWriter(httpWebRequest.GetRequestStream()))
            {
                streamWriter.Write(JsonConvert.SerializeObject(stockSnapshotRequet, defaultJSONSettings));
                streamWriter.Flush();
            }

            string _resCode = string.Empty;
            string _decrypt = string.Empty;
            string _output = string.Empty;
            HttpWebResponse httpResponse = null;
            httpResponse = (HttpWebResponse)httpWebRequest.GetResponse();
            _resCode = Convert.ToString(httpResponse.StatusCode);
            using (var streamReader = new StreamReader(httpResponse.GetResponseStream()))
            {
                _output = streamReader.ReadToEnd();
            }

            return JsonConvert.DeserializeObject<StockSnapshotResponse>(_output, defaultJSONSettings);
        }

        public static JsonSerializerSettings defaultJSONSettings = new JsonSerializerSettings
        {
            Formatting = Formatting.Indented,
            DefaultValueHandling = DefaultValueHandling.Populate,
            NullValueHandling = NullValueHandling.Ignore, // Ignore null values
            Converters = { new IsoDateTimeConverter { DateTimeFormat = "yyyy-MM-ddTHH:mm:ss.fffZ" } } // Custom date format
        };

        public static SearchSalesOrderResponse SearchSalesOrder(string serverurl, Guid access_token, DateTime from, DateTime to,
            string status, bool cashdelivery, string[] facilities)
        {
            SearchSalesOrderRequest stockSnapshotRequet = new SearchSalesOrderRequest()
            {
                fromDate = from.Date,
                toDate = to.Date.AddDays(1),
                dateType = "UPDATED",
                status = "COMPLETE",
                facilityCodes = facilities
            };
            if(stockSnapshotRequet.status != "COMPLETE" &&
                stockSnapshotRequet.status != "RETURNS")
            {
                stockSnapshotRequet.status = status;
            }
            //if (string.IsNullOrEmpty(status) || status == "ALL")
            //{
            //    stockSnapshotRequet = new SearchSalesOrderRequest()
            //    {
            //        fromDate = from,
            //        cashOnDelivery = cashdelivery,
            //        toDate = to.AddDays(1)
            //    };
            //}
            //else
            //{
            //    stockSnapshotRequet = new SearchSalesOrderRequest()
            //    {
            //        fromDate = from,
            //        toDate = to.AddDays(1),
            //        cashOnDelivery = cashdelivery,
            //        status = status
            //    };
            //}

            var httpWebRequest = (HttpWebRequest)WebRequest.Create(serverurl + method_searchsalesorder);
            httpWebRequest.ContentType = "application/json";
            httpWebRequest.Accept = "application/json";
            httpWebRequest.Method = "POST";
            httpWebRequest.KeepAlive = false;
            httpWebRequest.Headers.Add("Authorization", "Bearer " + access_token);

            using (var streamWriter = new StreamWriter(httpWebRequest.GetRequestStream()))
            {
                //streamWriter.Write(t);
                streamWriter.Write(JsonConvert.SerializeObject(stockSnapshotRequet, defaultJSONSettings));
                streamWriter.Flush();
            }

            string _resCode = string.Empty;
            string _decrypt = string.Empty;
            string _output = string.Empty;
            HttpWebResponse httpResponse = null;
            httpResponse = (HttpWebResponse)httpWebRequest.GetResponse();
            _resCode = Convert.ToString(httpResponse.StatusCode);
            using (var streamReader = new StreamReader(httpResponse.GetResponseStream()))
            {
                _output = streamReader.ReadToEnd();
            }
            return JsonConvert.DeserializeObject<SearchSalesOrderResponse>(_output, defaultJSONSettings);
        }

        public class PDFText
        {
            public string position { get; set; }
            public string text { get; set; }
            public string type { get; set; }
        }
        public static List<PDFText> GetText(byte[] pdfFileName)
        {
            List<PDFText> response = new List<PDFText>();
            using (System.IO.MemoryStream memoryStream = new MemoryStream(pdfFileName))
            {
                using (var _document = PdfReader.Open(memoryStream, PdfDocumentOpenMode.ReadOnly))
                {
                    int position = 0;
                    foreach (var page in _document.Pages.OfType<PdfPage>())
                    {
                        position++;
                        ExtractText(ContentReader.ReadContent(page), position.ToString(), response);
                    }
                    return response;
                }
            }
        }

        #region CObject Visitor
        private static void ExtractText(CObject obj, string position, List<PDFText> target)
        {
            if (obj is CArray)
                ExtractText((CArray)obj, position, target);
            else if (obj is CComment)
                ExtractText((CComment)obj, position, target);
            else if (obj is CInteger)
                ExtractText((CInteger)obj, position, target);
            else if (obj is CName)
                ExtractText((CName)obj, position, target);
            else if (obj is CNumber)
                ExtractText((CNumber)obj, position, target);
            else if (obj is COperator)
                ExtractText((COperator)obj, position, target);
            else if (obj is CReal)
                ExtractText((CReal)obj, position, target);
            else if (obj is CSequence)
                ExtractText((CSequence)obj, position, target);
            else if (obj is CString)
                ExtractText((CString)obj, position, target);
            else
            {
                string value = "";
                string type = "";
                if (obj is COperator)
                {
                    value = (obj as COperator).Name;
                    type = "Operator";
                }
                target.Add(new PDFText()
                {
                    position = position,
                    text = value,
                    type = "Name"
                });
            }
            //else
            //    throw new NotImplementedException(obj.GetType().AssemblyQualifiedName);
        }

        private static void ExtractText(CArray obj, string position, List<PDFText> target)
        {
            int ipos = 0;
            foreach (var element in obj)
            {
                ipos++;
                ExtractText(element, position + "." + ipos, target);
            }
        }
        private static void ExtractText(CComment obj, string position, List<PDFText> target)
        {
            if (obj != null & !string.IsNullOrEmpty(obj.Text))
                target.Add(new PDFText()
                {
                    position = position,
                    text = obj.Text,
                    type = "Text"
                });
        }
        private static void ExtractText(CInteger obj, string position, List<PDFText> target)
        {
            if (obj != null & obj.Value > 0)
                target.Add(new PDFText()
                {
                    position = position,
                    text = obj.Value.ToString(),
                    type = "Int"
                });
        }
        private static void ExtractText(CName obj, string position, List<PDFText> target)
        {
            if (obj != null & !string.IsNullOrEmpty(obj.Name))
                target.Add(new PDFText()
                {
                    position = position,
                    text = obj.Name,
                    type = "Name"
                });
        }
        private static void ExtractText(CNumber obj, string position, List<PDFText> target)
        {
            if (obj != null)
                target.Add(new PDFText()
                {
                    position = position,
                    text = obj.ToString(),
                    type = "Number"
                });
        }
        private static void ExtractText(COperator obj, string position, List<PDFText> target)
        {
            if (obj.OpCode.OpCodeName == OpCodeName.Tj || obj.OpCode.OpCodeName == OpCodeName.TJ)
            {
                int ipos = 0;
                foreach (var element in obj.Operands)
                {
                    ipos++;
                    ExtractText(element, position + "." + ipos, target);
                }
            }
        }
        private static void ExtractText(CReal obj, string position, List<PDFText> target) { /* nothing */ }
        private static void ExtractText(CSequence obj, string position, List<PDFText> target)
        {
            int ipos = 0;
            foreach (var element in obj)
            {
                ipos++;
                ExtractText(element, position + "." + ipos, target);
            }
        }
        private static void ExtractText(CString obj, string position, List<PDFText> target)
        {
            target.Add(new PDFText()
            {
                position = position,
                text = obj.Value,
                type = "Text"
            });
        }
        #endregion
        public static List<ECOMSalesOrderItem> GetECOMSalesOrderItems(string serverurl, Guid accesstoken,
            InfyPOS.Models.Configuration.UnicommerceConfig uniconfig, SearchSalesOrderResponse.Element element)
        {
            var response = GetSalesOrder(serverurl, accesstoken, element.code);
            List<ECOMSalesOrderItem> resultitems = new List<ECOMSalesOrderItem>();
            foreach (var itm in response.saleOrderDTO.saleOrderItems)
            {
                ECOMSalesOrderItem eCOMSalesOrderItem = new ECOMSalesOrderItem();
                eCOMSalesOrderItem.OrderNo = itm.code;
                eCOMSalesOrderItem.Channel = element.channel;
                eCOMSalesOrderItem.ExternOrderNo = element.code;
                eCOMSalesOrderItem.OrderDate = new DateTimeOffset(1970, 1, 1, 0, 0, 0, TimeSpan.Zero).AddMilliseconds(element.displayOrderDateTime).DateTime.ToLocalTime();
                if (response.saleOrderDTO.shippingPackages != null && response.saleOrderDTO.shippingPackages.Length > 0)
                {
                    var package = response.saleOrderDTO.shippingPackages.ToList().Find(x => x.code == itm.shippingPackageCode);
                    if (package == null) package = response.saleOrderDTO.shippingPackages[0];
                    eCOMSalesOrderItem.DispatchOn = new DateTimeOffset(1970, 1, 1, 0, 0, 0, TimeSpan.Zero).AddMilliseconds(package.dispatched).DateTime.ToLocalTime();
                    eCOMSalesOrderItem.ShipCode = package.code;
                    eCOMSalesOrderItem.InvoiceCode = package.invoiceCode;
                    eCOMSalesOrderItem.InvoiceDisplayCode = package.invoiceDisplayCode;
                }
                eCOMSalesOrderItem.ShipPhone = response.saleOrderDTO.billingAddress.phone;
                eCOMSalesOrderItem.ShipPinCode = response.saleOrderDTO.billingAddress.pincode.ToString();
                if (uniconfig.channels != null && uniconfig.channels.Count > 0)
                {
                    if (uniconfig.channels.Exists(x => x.name == element.channel))
                    {
                        var channel = uniconfig.channels.Find(x => x.name == element.channel);
                        eCOMSalesOrderItem.CustomerName = channel.customercode;
                        eCOMSalesOrderItem.Channel = channel.customercode;
                        eCOMSalesOrderItem.Company = channel.company;
                        eCOMSalesOrderItem.Location = channel.location;
                    }
                }
                eCOMSalesOrderItem.SKU = itm.itemSku;
                eCOMSalesOrderItem.ShipAddress = response.saleOrderDTO.billingAddress.city;
                eCOMSalesOrderItem.ShipCity = response.saleOrderDTO.billingAddress.city;
                eCOMSalesOrderItem.UnitPrice = itm.sellingPrice;
                eCOMSalesOrderItem.OrderQty = 1;// itm.totalPrice / itm.sellingPrice;
                eCOMSalesOrderItem.VoucherAmount = itm.voucherValue;
                eCOMSalesOrderItem.LineAmount = eCOMSalesOrderItem.UnitPrice * eCOMSalesOrderItem.OrderQty;
                eCOMSalesOrderItem.ShipState = response.saleOrderDTO.billingAddress.state;
                if (StateCodes.Exists(x => x.ECODE == eCOMSalesOrderItem.ShipState))
                {
                    eCOMSalesOrderItem.ShipState = StateCodes.Find(x => x.ECODE == eCOMSalesOrderItem.ShipState).ToString();
                }
                if (response.saleOrderDTO.shippingPackages != null &&
                    response.saleOrderDTO.shippingPackages.Length > 0)
                    eCOMSalesOrderItem.shippingPackageStatus = response.saleOrderDTO.shippingPackages.FirstOrDefault().status;

                eCOMSalesOrderItem.statusCode = response.saleOrderDTO.status;
                eCOMSalesOrderItem.cancellationReason = itm.cancellationReason;
                eCOMSalesOrderItem.color = itm.color;
                eCOMSalesOrderItem.brand = itm.brand;
                eCOMSalesOrderItem.size = itm.size;
                eCOMSalesOrderItem.itemName = itm.itemName;
                eCOMSalesOrderItem.maxRetailPrice = itm.maxRetailPrice;
                resultitems.Add(eCOMSalesOrderItem);
            }

            if (response.saleOrderDTO.returns != null && response.saleOrderDTO.returns.Length > 0 &&
                response.saleOrderDTO.returns.ToList().Exists(x => x.statusCode == "RETURNED" && !string.IsNullOrEmpty(x.putawayCode)))
            {

                foreach (var ritm in response.saleOrderDTO.returns)
                {
                    if (!string.IsNullOrEmpty(ritm.putawayCode) &&
                        ritm.statusCode == "RETURNED")
                    {
                        foreach (var rsitem in ritm.returnItems)
                        {
                            var itm = response.saleOrderDTO.saleOrderItems.ToList().Find(x => x.code == rsitem.saleOrderItemCode);
                            if (itm == null) continue;
                            ECOMSalesOrderItem eCOMSalesOrderItem = new ECOMSalesOrderItem();
                            eCOMSalesOrderItem.OrderNo = itm.code;
                            eCOMSalesOrderItem.Channel = element.channel;
                            eCOMSalesOrderItem.ExternOrderNo = element.code + "-R";
                            eCOMSalesOrderItem.OrderDate = new DateTimeOffset(1970, 1, 1, 0, 0, 0, TimeSpan.Zero).AddMilliseconds(ritm.inventoryReceivedDate).DateTime.ToLocalTime();
                            if (response.saleOrderDTO.shippingPackages != null && response.saleOrderDTO.shippingPackages.Length > 0)
                            {
                                var package = response.saleOrderDTO.shippingPackages.ToList().Find(x => x.code == itm.shippingPackageCode);
                                if (package == null) package = response.saleOrderDTO.shippingPackages[0];
                                eCOMSalesOrderItem.DispatchOn = new DateTimeOffset(1970, 1, 1, 0, 0, 0, TimeSpan.Zero).AddMilliseconds(ritm.inventoryReceivedDate == 0? ritm.returnCompletedDate: ritm.inventoryReceivedDate).DateTime.ToLocalTime();
                                eCOMSalesOrderItem.ShipCode = ritm.trackingNumber;
                                eCOMSalesOrderItem.InvoiceCode = ritm.returnInvoiceCode;
                                eCOMSalesOrderItem.InvoiceDisplayCode = ritm.returnInvoiceDisplayCode;
                            }
                            eCOMSalesOrderItem.ShipPhone = response.saleOrderDTO.billingAddress.phone;
                            eCOMSalesOrderItem.ShipPinCode = response.saleOrderDTO.billingAddress.pincode.ToString();
                            if (uniconfig.channels != null && uniconfig.channels.Count > 0)
                            {
                                if (uniconfig.channels.Exists(x => x.name == element.channel))
                                {
                                    var channel = uniconfig.channels.Find(x => x.name == element.channel);
                                    eCOMSalesOrderItem.CustomerName = channel.customercode;
                                    eCOMSalesOrderItem.Channel = channel.customercode;
                                    eCOMSalesOrderItem.Company = channel.company;
                                    eCOMSalesOrderItem.Location = channel.location;
                                }
                            }
                            eCOMSalesOrderItem.SKU = itm.itemSku;
                            eCOMSalesOrderItem.ShipAddress = response.saleOrderDTO.billingAddress.city;
                            eCOMSalesOrderItem.ShipCity = response.saleOrderDTO.billingAddress.city;
                            eCOMSalesOrderItem.UnitPrice = itm.sellingPrice;
                            eCOMSalesOrderItem.OrderQty = -1;// itm.totalPrice / itm.sellingPrice;
                            eCOMSalesOrderItem.VoucherAmount = itm.voucherValue;
                            eCOMSalesOrderItem.LineAmount = eCOMSalesOrderItem.UnitPrice * eCOMSalesOrderItem.OrderQty;
                            eCOMSalesOrderItem.ShipState = response.saleOrderDTO.billingAddress.state;
                            if (StateCodes.Exists(x => x.ECODE == eCOMSalesOrderItem.ShipState))
                            {
                                eCOMSalesOrderItem.ShipState = StateCodes.Find(x => x.ECODE == eCOMSalesOrderItem.ShipState).ToString();
                            }
                            if (response.saleOrderDTO.shippingPackages != null &&
                                response.saleOrderDTO.shippingPackages.Length > 0)
                                eCOMSalesOrderItem.shippingPackageStatus = response.saleOrderDTO.shippingPackages.FirstOrDefault().status;

                            eCOMSalesOrderItem.statusCode = response.saleOrderDTO.status;
                            eCOMSalesOrderItem.cancellationReason = itm.cancellationReason;
                            eCOMSalesOrderItem.color = itm.color;
                            eCOMSalesOrderItem.brand = itm.brand;
                            eCOMSalesOrderItem.size = itm.size;
                            eCOMSalesOrderItem.itemName = itm.itemName;
                            eCOMSalesOrderItem.maxRetailPrice = itm.maxRetailPrice;
                            if(!string.IsNullOrEmpty(eCOMSalesOrderItem.Location))
                                resultitems.Add(eCOMSalesOrderItem);
                        }
                    }
                }
            }
            return resultitems;
        }

        static List<StateCode> StateCodes = new List<StateCode>(new StateCode[]
        {
        new StateCode("AN","35","ANDAMAN AND NICOBAR ISLANDS"),
        new StateCode("AP","28","ANDHRA PRADESH"),
        new StateCode("AD","37","ANDHRA PRADESH (NEW)"),
        new StateCode("AR","12","ARUNACHAL PRADESH"),
        new StateCode("AS","18","ASSAM"),
        new StateCode("BH","10","BIHAR"),
        new StateCode("BR","10","BIHAR"),
        new StateCode("CH","4","CHANDIGARH"),
        new StateCode("CT","22","CHATTISGARH"),
        new StateCode("DN","26","DADRA AND NAGAR HAVELI"),
        new StateCode("DD","25","DAMAN AND DIU"),
        new StateCode("DL","7","DELHI"),
        new StateCode("GA","30","GOA"),
        new StateCode("GJ","24","GUJARAT"),
        new StateCode("HR","6","HARYANA"),
        new StateCode("HP","2","HIMACHAL PRADESH"),
        new StateCode("JK","1","JAMMU AND KASHMIR"),
        new StateCode("JH","20","JHARKHAND"),
        new StateCode("KA","29","KARNATAKA"),
        new StateCode("KL","32","KERALA"),
        new StateCode("LD","31","LAKSHADWEEP ISLANDS"),
        new StateCode("MP","23","MADHYA PRADESH"),
        new StateCode("MH","27","MAHARASHTRA"),
        new StateCode("MN","14","MANIPUR"),
        new StateCode("ME","17","MEGHALAYA"),
        new StateCode("ML","17","MEGHALAYA"),
        new StateCode("MI","15","MIZORAM"),
        new StateCode("NL","13","NAGALAND"),
        new StateCode("OR","21","ODISHA"),
        new StateCode("PY","34","PONDICHERRY"),
        new StateCode("PB","3","PUNJAB"),
        new StateCode("RJ","8","RAJASTHAN"),
        new StateCode("SK","11","SIKKIM"),
        new StateCode("TN","33","TAMIL NADU"),
        new StateCode("TS","36","TELANGANA"),
        new StateCode("TL","36","TELANGANA"),
        new StateCode("TR","16","TRIPURA"),
        new StateCode("UP","9","UTTAR PRADESH"),
        new StateCode("UT","5","UTTARAKHAND"),
        new StateCode("WB","19","WEST BENGAL")
        });

        public class StateCode
        {
            public override string ToString()
            {
                return NCODE + "-" + NAME;
            }
            public string ECODE { get; set; }
            public string NCODE { get; set; }
            public string NAME { get; set; }

            public StateCode(string ECODE, string NCODE, string NAME)
            {
                this.ECODE = ECODE;
                this.NCODE = NCODE;
                this.NAME = NAME;
            }
        }

        public static GetSalesOrderResponse GetSalesOrder(string serverurl, Guid access_token, string code)
        {
            GetSalesOrderRequest stockSnapshotRequet = new GetSalesOrderRequest() { code = code };

            var httpWebRequest = (HttpWebRequest)WebRequest.Create(serverurl + method_getsalesorder);
            httpWebRequest.ContentType = "application/json";
            httpWebRequest.Accept = "application/json";
            httpWebRequest.Method = "POST";
            httpWebRequest.KeepAlive = false;
            httpWebRequest.Headers.Add("Authorization", "Bearer " + access_token);
            using (var streamWriter = new StreamWriter(httpWebRequest.GetRequestStream()))
            {
                streamWriter.Write(JsonConvert.SerializeObject(stockSnapshotRequet, defaultJSONSettings));
                streamWriter.Flush();
            }

            string _resCode = string.Empty;
            string _decrypt = string.Empty;
            string _output = string.Empty;
            HttpWebResponse httpResponse = null;
            httpResponse = (HttpWebResponse)httpWebRequest.GetResponse();
            _resCode = Convert.ToString(httpResponse.StatusCode);
            using (var streamReader = new StreamReader(httpResponse.GetResponseStream()))
            {
                _output = streamReader.ReadToEnd();
            }

            return JsonConvert.DeserializeObject<GetSalesOrderResponse>(_output, defaultJSONSettings);
        }

        public class QuantoSKUStcok : StockSnapshotResponse.InventorySnapshot
        {
            public string prdsku { get; set; }
            public string uan { get; set; }
            public decimal stk { get; set; }
            public string status { get; set; }
        }

        public class QuantoBundleUAN : BaseEntity
        {
            public string uan { get; set; }
            public decimal add { get; set; }
            public decimal remove { get; set; }
            public decimal qty { get; set; }
        }

        public class QuantoSKUAdjust : BaseEntity
        {
            public string prdsku { get; set; }
            public long add { get; set; }
            public long remove { get; set; }
            public long replace { get; set; }
            public bool updated { get; set; }
        }

    }

    public class StringOrNumberConverter : JsonConverter
    {
        public override bool CanConvert(Type objectType)
        {
            return objectType == typeof(string);
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            if (reader.TokenType == JsonToken.String)
                return reader.Value?.ToString();
            if (reader.TokenType == JsonToken.Integer || reader.TokenType == JsonToken.Float)
                return Convert.ToString(reader.Value);
            return null;
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            writer.WriteValue((string)value);
        }
    }
}


