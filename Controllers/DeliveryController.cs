
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using InfyPOS.Models;
using InfyPOS.Common;
using InfyPOS.Data;
using InfyPOS.Services;

namespace InfyPOS.Controllers
{
    [RoutePrefix("DeliveryService")]
    public class DeliveryServiceController : ApiController
    {

        [HttpGet]
        [Route("Entity")]
        public async Task<IHttpActionResult> Entity()
        {
            var result = new ActionRequest<Delivery>();
            result.Item = new Delivery();
            return this.Ok(result);
        }

        [HttpPost]
        [Route("Search")]
        public async Task<IHttpActionResult> Search(ActionRequest<DeliveryCreteria> request)
        {
            bool doDateCheck = false;
            DeliveryDataService DeliveryData = new DeliveryDataService();
            BillDataService BillData = new BillDataService();
            Common.ActionMulitipleResponse<Delivery> response = null;

            if (InfyPOS.Services.ConfigManager.Instance(DeliveryData.Context.OrganizationID).LocationConfig != null &&
                InfyPOS.Services.ConfigManager.Instance(DeliveryData.Context.OrganizationID).LocationConfig.ContainsKey(request.Item.locationid) &&
                InfyPOS.Services.ConfigManager.Instance(DeliveryData.Context.OrganizationID).LocationConfig[request.Item.locationid].system_giftsameday)
            {
                doDateCheck = true;
            }

            if (request.Item.isalteration)
            {
                var creteria = new BillCreteria()
                {
                    billno = request.Item.settlementcode,
                    locationid = request.Item.locationid,
                    isimport = true,
                    removecancelled = true
                };
                var bills = await Task<List<Bill>>.Run(() => BillData.Search(creteria));
                if (bills.Count > 0)
                {
                    var deliverylist = DeliveryData.Search(new DeliveryCreteria()
                    {
                        billno = request.Item.settlementcode,
                        locationid = request.Item.locationid,
                        isalteration = true
                    });
                    Delivery delivery = null;
                    if (deliverylist.Count > 0)
                    {
                        delivery = deliverylist[0];
                    }
                    else if (!bills[0].settled)
                    {
                        response = new ActionMulitipleResponse<Delivery>();
                        response.Items = null;
                        response.Success = false;
                        response.ErrorMessage = "UNSETTLED BILL";
                        return this.Ok(response);
                    }
                    else
                    {
                        Delivery.ReceivedItem receiveditem = new Delivery.ReceivedItem(bills[0],
                            Services.ConfigManager.Instance(BillData.Context.OrganizationID).Config.system_ismfgapplication);
                        delivery = new Delivery();

                        delivery.amount = receiveditem.amount;
                        delivery.billid = bills[0].id;
                        delivery.billno = bills[0].billno;
                        delivery.billdate = bills[0].billdate;
                        delivery.customerid = bills[0].customerid;
                        delivery.customername = bills[0].customer == null ? "" : bills[0].customer.name;
                        delivery.contactno = bills[0].customer == null ? "" : bills[0].customer.mobileno;
                        delivery.receivedon = DeliveryData.Context.DateTimeNow;
                        delivery.qty = (int)receiveditem.qty;
                        delivery.locationid = request.Item.locationid;
                        delivery.counterid = request.Item.counterid;
                        delivery.billinfo = receiveditem;
                        delivery.receivedby = DeliveryData.Context.UserID;
                        delivery.receivedon = DeliveryData.Context.DateTimeNow;
                        delivery.settled = bills[0].settled;
                        delivery.Deliverystatus = Delivery.DeliveryStatus.Received;
                    }

                    response = new ActionMulitipleResponse<Delivery>();
                    response.Items = new List<Delivery>();
                    response.Success = true;
                    response.Items.AddRange(new Delivery[] { delivery });
                    delivery.received = true;
                    delivery.receivedcount = await Task<int>.Run(() => DeliveryData.GetReceived(bills[0].billdate,
                        request.Item.locationid, request.Item.counterid, request.Item.isalteration));
                    response.ErrorMessage = "Recieved - " + delivery.billno + " @ " + delivery.amount.ToString("N0");
                    return this.Ok(response);
                }
                else
                {
                    var deliverylist = DeliveryData.Search(new DeliveryCreteria()
                    {
                        altationno = request.Item.settlementcode,
                        locationid = request.Item.locationid,
                        isalteration = true
                    });
                    if (deliverylist.Count > 0)
                    {
                        var delivery = deliverylist[0];
                        delivery.receivedcount = await Task<int>.Run(() => DeliveryData.GetReceived(DeliveryData.DateTimeNow.Date,
                        request.Item.locationid, request.Item.counterid, request.Item.isalteration));
                        response = new ActionMulitipleResponse<Delivery>();
                        response.Items = new List<Delivery>();
                        response.Success = true;
                        response.Items.AddRange(new Delivery[] { delivery });
                        delivery.received = true;
                        delivery.receivedcount = await Task<int>.Run(() => DeliveryData.GetReceived(DeliveryData.DateTimeNow.Date,
                            request.Item.locationid, request.Item.counterid, request.Item.isalteration));
                        response.ErrorMessage = "Recieved - " + delivery.billno + " @ " + delivery.amount.ToString("N0");
                        return this.Ok(response);
                    }
                    else
                    {
                        response = new ActionMulitipleResponse<Delivery>();
                        response.Items = new List<Delivery>();
                        response.Success = true;
                        var delivery = new Delivery();
                        response.Items.AddRange(new Delivery[] { delivery });
                        delivery.received = false;
                        delivery.receivedcount = await Task<int>.Run(() => DeliveryData.GetReceived(DeliveryData.DateTimeNow.Date,
                            request.Item.locationid, request.Item.counterid, request.Item.isalteration));
                        response.ErrorMessage = "No alteration found";
                        return this.Ok(response);
                    }
                }
            }
            else if (request.Item.isgiftcoupon)
            {
                var creteria = new BillCreteria()
                {
                    billno = request.Item.settlementcode,
                    locationid = request.Item.locationid,
                    isimport = true,
                    removecancelled = true
                };
                List<Bill> bills = null;

                BillsettlementmasterDataService billsettlementmasterDataService = new BillsettlementmasterDataService();
                var billsettlements = await Task<List<Billsettlementmaster>>.Run(() => billsettlementmasterDataService.Search(new BillsettlementmasterCreteria()
                {
                    code = request.Item.settlementcode
                }));
                if (billsettlements.Count == 1)
                {
                    creteria = new BillCreteria()
                    {
                        ids = billsettlements[0].Billsettlement.ConvertAll(x => x.billid)
                    };
                    bills = await Task<List<Bill>>.Run(() => BillData.Search(creteria));
                    if (bills.Exists(x => x.billattributes.giftcoupon))
                    {
                        response = new ActionMulitipleResponse<Delivery>();
                        response.Items = DeliveryData.SearchGift(request.Item.settlementcode);
                        response.Success = false;
                        response.ErrorMessage = "GIFTCOUPONREPRINT";
                        return this.Ok(response);
                    }

                    if (doDateCheck && billsettlements[0].settlementon.Date != DeliveryData.DateTimeNow.Date)
                    {
                        response = new ActionMulitipleResponse<Delivery>();
                        response.Items = null;
                        response.Success = false;
                        response.ErrorMessage = "GIFT VOUCHER CAN'T BE ISSUED ON OTHER DAY BILLS";
                        return this.Ok(response);
                    }

                    response = new ActionMulitipleResponse<Delivery>();
                    response.Items = new List<Delivery>();

                    foreach (var bill in bills)
                    {
                        Delivery.ReceivedItem receiveditem = DeliveryData.GetGiftCouponItem(bill, Services.ConfigManager.Instance(BillData.Context.OrganizationID));
                        receiveditem.settlementon = billsettlements[0].settlementon;
                        receiveditem.settlementcode = billsettlements[0].code;
                        Delivery delivery = new Delivery();
                        delivery.settlementcode = billsettlements[0].code;
                        delivery.settlementon = billsettlements[0].settlementon;
                        delivery.receivable = bill.receivable + bill.rounding;
                        delivery.amount = receiveditem.amount;
                        delivery.excluded = receiveditem.excluded;
                        delivery.billid = bill.id;
                        delivery.billno = receiveditem.billno;
                        delivery.billdate = bill.billdate;
                        delivery.receivedon = DeliveryData.Context.DateTimeNow;
                        delivery.qty = (int)receiveditem.qty;
                        delivery.locationid = request.Item.locationid;
                        delivery.counterid = request.Item.counterid;
                        delivery.billinfo = receiveditem;
                        delivery.receivedby = DeliveryData.Context.UserID;
                        delivery.receivedon = DeliveryData.Context.DateTimeNow;
                        delivery.settled = bill.settled;
                        delivery.customername = bill.customer != null ? bill.customer.name : "";
                        delivery.contactno = bill.customer != null ? bill.customer.mobileno : "";
                        if (!InfyPOS.Services.ConfigManager.Instance(new BaseData().Context.OrganizationID).Config.system_supportdeliverybarcode)
                        {
                            delivery.candeliver = bill.settled &&
                                (bill.billtype == Bill.BillTypes.Cash || bill.billtype == Bill.BillTypes.Credit);
                        }
                        delivery.Deliverystatus = Delivery.DeliveryStatus.Received;
                        delivery.received = true;
                        response.Items.AddRange(new Delivery[] { delivery });
                    }
                    response.Success = true;
                    //response.ErrorMessage = "Recieved - " + delivery.billno + " @ " + delivery.amount.ToString("N0");
                    return this.Ok(response);
                }
                else
                {
                    var deliverylist = DeliveryData.Search(new DeliveryCreteria()
                    {
                        altationno = request.Item.settlementcode,
                        locationid = request.Item.locationid,
                        isgift = true
                    });
                    if (deliverylist.Count > 0)
                    {
                        Delivery delivery = new Delivery() { id = deliverylist[0].id };
                        response = new ActionMulitipleResponse<Delivery>();
                        response.Items = new List<Delivery>();
                        response.Success = false;
                        response.Items.AddRange(new Delivery[] { delivery });
                        response.ErrorMessage = "GIFTREPRINT";
                        return this.Ok(response);
                    }
                }


                response = new ActionMulitipleResponse<Delivery>();
                response.Items = null;
                response.Success = false;
                response.ErrorMessage = "BILL NO / BARCODE NOT FOUND";
                return this.Ok(response);
            }
            else if (request.Item.isgift)
            {
                var creteria = new BillCreteria()
                {
                    billno = request.Item.settlementcode,
                    locationid = request.Item.locationid,
                    isimport = true,
                    removecancelled = true
                };
                List<Bill> bills = null;

                if (!Services.ConfigManager.Instance(DeliveryData.Context.OrganizationID).LocationConfig[request.Item.locationid].system_giftsettlement)
                    bills = await Task<List<Bill>>.Run(() => BillData.Search(creteria));

                if (bills != null && bills.Count == 1)
                {
                    if (bills[0].billattributes.gift)
                    {
                        response = new ActionMulitipleResponse<Delivery>();
                        response.Items = DeliveryData.SearchGift(request.Item.settlementcode);
                        response.Success = false;
                        response.ErrorMessage = "GIFT ISSUED";
                        return this.Ok(response);
                    }
                    else if (!bills[0].settled || bills[0].isactive == false)
                    {
                        response = new ActionMulitipleResponse<Delivery>();
                        response.Items = null;
                        response.Success = false;
                        response.ErrorMessage = "UNSETTLED BILL";
                        return this.Ok(response);
                    }
                    else if (doDateCheck && bills[0].billdate.Date != DeliveryData.DateTimeNow.Date)
                    {
                        response = new ActionMulitipleResponse<Delivery>();
                        response.Items = null;
                        response.Success = false;
                        response.ErrorMessage = "GIFT VOUCHER CAN'T BE ISSUED ON OTHER DAY BILLS";
                        return this.Ok(response);
                    }
                    else
                    {
                        Delivery delivery = new Delivery();
                        Delivery.ReceivedItem receiveditem = DeliveryData.GetGiftItem(bills[0], Services.ConfigManager.Instance(BillData.Context.OrganizationID));
                        delivery.receivable = bills[0].receivable + bills[0].additionalcharges + bills[0].rounding;
                        delivery.amount = receiveditem.amount;
                        delivery.excluded = receiveditem.excluded;
                        delivery.billid = bills[0].id;
                        delivery.billno = receiveditem.billno;
                        delivery.billdate = bills[0].billdate;
                        delivery.receivedon = DeliveryData.Context.DateTimeNow;
                        delivery.qty = (int)receiveditem.qty;
                        delivery.locationid = request.Item.locationid;
                        delivery.counterid = request.Item.counterid;
                        delivery.billinfo = receiveditem;
                        delivery.receivedby = DeliveryData.Context.UserID;
                        delivery.receivedon = DeliveryData.Context.DateTimeNow;
                        delivery.settled = bills[0].settled;
                        delivery.customername = bills[0].customer != null ? bills[0].customer.name : "";
                        delivery.contactno = bills[0].customer != null ? bills[0].customer.mobileno : "";
                        if (!InfyPOS.Services.ConfigManager.Instance(new BaseData().Context.OrganizationID).Config.system_supportdeliverybarcode)
                        {
                            delivery.candeliver = bills[0].settled &&
                                (bills[0].billtype == Bill.BillTypes.Cash || bills[0].billtype == Bill.BillTypes.Credit);
                        }
                        delivery.Deliverystatus = Delivery.DeliveryStatus.Received;
                        response = new ActionMulitipleResponse<Delivery>();
                        response.Items = new List<Delivery>();
                        response.Items.AddRange(new Delivery[] { delivery });
                        delivery.received = true;
                        response.Success = true;
                        response.ErrorMessage = "Recieved - " + delivery.billno + " @ " + delivery.amount.ToString("N0");
                        return this.Ok(response);
                    }
                }
                else
                {
                    BillsettlementmasterDataService billsettlementmasterDataService = new BillsettlementmasterDataService();
                    var billsettlements = await Task<List<Billsettlementmaster>>.Run(() => billsettlementmasterDataService.Search(new BillsettlementmasterCreteria()
                    {
                        code = request.Item.settlementcode
                    }));
                    if (billsettlements.Count == 1)
                    {
                        if (doDateCheck && billsettlements[0].settlementon.Date != DeliveryData.DateTimeNow.Date)
                        {
                            response = new ActionMulitipleResponse<Delivery>();
                            response.Items = null;
                            response.Success = false;
                            response.ErrorMessage = "GIFT VOUCHER CAN'T BE ISSUED ON OTHER DAY BILLS";
                            return this.Ok(response);
                        }

                        creteria = new BillCreteria()
                        {
                            ids = billsettlements[0].Billsettlement.ConvertAll(x => x.billid)
                        };
                        bills = await Task<List<Bill>>.Run(() => BillData.Search(creteria));
                        if (bills.Exists(x => x.billattributes.gift))
                        {
                            response = new ActionMulitipleResponse<Delivery>();
                            response.Items = DeliveryData.SearchGift(request.Item.settlementcode);
                            response.Success = false;
                            response.ErrorMessage = "GIFT ISSUED";
                            return this.Ok(response);
                        }
                        response = new ActionMulitipleResponse<Delivery>();
                        response.Items = new List<Delivery>();

                        foreach (var bill in bills)
                        {
                            Delivery.ReceivedItem receiveditem = DeliveryData.GetGiftItem(bill, Services.ConfigManager.Instance(BillData.Context.OrganizationID));
                            receiveditem.settlementon = billsettlements[0].settlementon;
                            receiveditem.settlementcode = billsettlements[0].code;
                            Delivery delivery = new Delivery();
                            delivery.settlementcode = billsettlements[0].code;
                            delivery.settlementon = billsettlements[0].settlementon;
                            delivery.receivable = bill.receivable + bill.rounding;
                            delivery.amount = receiveditem.amount;
                            delivery.excluded = receiveditem.excluded;
                            delivery.billid = bill.id;
                            delivery.billno = receiveditem.billno;
                            delivery.billdate = bill.billdate;
                            delivery.receivedon = DeliveryData.Context.DateTimeNow;
                            delivery.qty = (int)receiveditem.qty;
                            delivery.locationid = request.Item.locationid;
                            delivery.counterid = request.Item.counterid;
                            delivery.billinfo = receiveditem;
                            delivery.receivedby = DeliveryData.Context.UserID;
                            delivery.receivedon = DeliveryData.Context.DateTimeNow;
                            delivery.settled = bill.settled;
                            delivery.customername = bill.customer != null ? bill.customer.name : "";
                            delivery.contactno = bill.customer != null ? bill.customer.mobileno : "";
                            if (!InfyPOS.Services.ConfigManager.Instance(new BaseData().Context.OrganizationID).Config.system_supportdeliverybarcode)
                            {
                                delivery.candeliver = bill.settled &&
                                    (bill.billtype == Bill.BillTypes.Cash || bill.billtype == Bill.BillTypes.Credit);
                            }
                            delivery.Deliverystatus = Delivery.DeliveryStatus.Received;
                            delivery.received = true;
                            response.Items.AddRange(new Delivery[] { delivery });
                        }
                        response.Success = true;
                        //response.ErrorMessage = "Recieved - " + delivery.billno + " @ " + delivery.amount.ToString("N0");
                        return this.Ok(response);
                    }
                    else
                    {
                        var deliverylist = DeliveryData.Search(new DeliveryCreteria()
                        {
                            altationno = request.Item.settlementcode,
                            locationid = request.Item.locationid,
                            isgift = true
                        });
                        if (deliverylist.Count > 0)
                        {
                            Delivery delivery = new Delivery() { id = deliverylist[0].id };
                            response = new ActionMulitipleResponse<Delivery>();
                            response.Items = new List<Delivery>();
                            response.Success = false;
                            response.Items.AddRange(new Delivery[] { delivery });
                            response.ErrorMessage = "GIFTREPRINT";
                            return this.Ok(response);
                        }
                        else
                        {
                            StockmarkerDataService stockmarkerDataService = new StockmarkerDataService();
                            var stockmarkers = stockmarkerDataService.Search(new StockmarkerCreteria()
                            {
                                locationid = request.Item.locationid,
                                barcode = request.Item.settlementcode,
                                mode = 2,
                                markerids = request.Item.stockmarkersids
                            });
                            if (stockmarkers.Count == 1)
                            {
                                DeliveryGift deliveryGift = new DeliveryGift();
                                deliveryGift.Deliverystatus = Delivery.DeliveryStatus.StockMarker;
                                deliveryGift.barcode = stockmarkers[0].barcode;
                                deliveryGift.locationid = stockmarkers[0].locationid;
                                deliveryGift.name = stockmarkers[0].productname;
                                deliveryGift.salablegoodsid = stockmarkers[0].salablegoodsid;
                                deliveryGift.stockid = stockmarkers[0].stockid;
                                deliveryGift.stockmarkerid = stockmarkers[0].markerid;
                                deliveryGift.productid = stockmarkers[0].productid;
                                deliveryGift.salestaxid = stockmarkers[0].salestaxid;
                                deliveryGift.taxpercentage = stockmarkers[0].taxpercentage;
                                deliveryGift.Success = true;
                                return this.Ok(deliveryGift);
                            }
                        }
                    }
                }
                response = new ActionMulitipleResponse<Delivery>();
                response.Items = null;
                response.Success = false;
                response.ErrorMessage = "BILL NO / BARCODE NOT FOUND";
                return this.Ok(response);
            }
            else if (request.Item.isgiftissue)
            {
                var deliveritems = DeliveryData.Search(new DeliveryCreteria()
                {
                    altationno = request.Item.settlementcode,
                    isalteration = false,
                    isgift = true
                });
                if (deliveritems.Count == 0)
                {
                    StockmarkerDataService stockmarkerDataService = new StockmarkerDataService();
                    var stockmarkers = stockmarkerDataService.Search(new StockmarkerCreteria()
                    {
                        locationid = request.Item.locationid,
                        barcode = request.Item.settlementcode,
                        mode = 2,
                        markerids = request.Item.stockmarkersids
                    });
                    if (stockmarkers.Count == 1)
                    {
                        DeliveryGift deliveryGift = new DeliveryGift();
                        deliveryGift.Deliverystatus = Delivery.DeliveryStatus.StockMarker;
                        deliveryGift.barcode = stockmarkers[0].barcode;
                        deliveryGift.locationid = stockmarkers[0].locationid;
                        deliveryGift.name = stockmarkers[0].productname;
                        deliveryGift.salablegoodsid = stockmarkers[0].salablegoodsid;
                        deliveryGift.stockid = stockmarkers[0].stockid;
                        deliveryGift.stockmarkerid = stockmarkers[0].markerid;
                        deliveryGift.Success = true;
                        return this.Ok(deliveryGift);
                    }
                }
                else if (deliveritems[0].Deliverystatus == Delivery.DeliveryStatus.Delivered)
                {
                    response = new ActionMulitipleResponse<Delivery>();
                    response.Items = null;
                    response.Success = false;
                    response.ErrorMessage = "GIFT ALREADY DELIVERED";
                    return this.Ok(response);
                }
                else if (doDateCheck && deliveritems[0].receivedon.Date != DeliveryData.DateTimeNow.Date)
                {
                    response = new ActionMulitipleResponse<Delivery>();
                    response.Items = null;
                    response.Success = false;
                    response.ErrorMessage = "GIFT CAN'T BE ISSUED ON OTHER DAY VOUCHERS";
                    return this.Ok(response);
                }
                response = new ActionMulitipleResponse<Delivery>();
                response.Items = deliveritems;
                response.Success = true;
                response.ErrorMessage = "";
                return this.Ok(response);
            }
            else if (request.Item.searchbill)
            {
                Delivery delivery = new Delivery();
                delivery.ChangeStatus(BaseEntity.DataStatus.Create);
                if (!string.IsNullOrEmpty(request.Item.settlementcode))
                {
                    var deliverylist = DeliveryData.Search(new DeliveryCreteria()
                    {
                        billno = request.Item.settlementcode,
                        locationid = request.Item.locationid,
                        isdelivery = true
                    });

                    bool issettlementcode = false;
                    if (deliverylist.Count == 0 &&
                        InfyPOS.Services.ConfigManager.Instance(new BaseData().Context.OrganizationID).Config.system_supportdeliverybillno)
                    {
                        BillsettlementmasterDataService billsettlementmasterDataService = new BillsettlementmasterDataService();
                        var settlementmaster = billsettlementmasterDataService.Search(new BillsettlementmasterCreteria()
                        {
                            code = request.Item.settlementcode
                        });
                        if (settlementmaster.Count > 0)
                        {
                            issettlementcode = true;
                            deliverylist = DeliveryData.Search(new DeliveryCreteria()
                            {
                                billids = settlementmaster.FirstOrDefault().Billsettlement.ConvertAll(x => x.billid).ToList(),
                                locationid = request.Item.locationid,
                                isdelivery = true
                            });
                            var missingbills = settlementmaster.FirstOrDefault().Billsettlement.FindAll(x => !deliverylist.Exists(y => y.billid == x.billid));
                            if (missingbills.Count > 0)
                            {
                                var billcreteria = new BillCreteria()
                                {
                                    ids = missingbills.ConvertAll(x => x.billid)
                                };
                                var mbills = await Task<List<Bill>>.Run(() => BillData.Search(billcreteria));
                                foreach (var bill in mbills)
                                {
                                    Delivery.ReceivedItem receiveditem = DeliveryData.GetGiftItem(bill, Services.ConfigManager.Instance(BillData.Context.OrganizationID));
                                    receiveditem.settlementon = settlementmaster[0].settlementon;
                                    receiveditem.settlementcode = settlementmaster[0].code;
                                    delivery = new Delivery();
                                    delivery.settlementcode = settlementmaster[0].code;
                                    delivery.settlementon = settlementmaster[0].settlementon;
                                    delivery.receivable = bill.receivable + bill.rounding;
                                    delivery.amount = receiveditem.amount;
                                    delivery.excluded = receiveditem.excluded;
                                    delivery.billid = bill.id;
                                    delivery.billno = receiveditem.billno;
                                    delivery.billdate = bill.billdate;
                                    delivery.receivedon = DeliveryData.Context.DateTimeNow;
                                    delivery.qty = (int)receiveditem.qty;
                                    delivery.locationid = request.Item.locationid;
                                    delivery.counterid = request.Item.counterid;
                                    delivery.billinfo = receiveditem;
                                    delivery.receivedby = DeliveryData.Context.UserID;
                                    delivery.receivedon = DeliveryData.Context.DateTimeNow;
                                    delivery.settled = bill.settled;
                                    delivery.customername = bill.customer != null ? bill.customer.name : "";
                                    delivery.contactno = bill.customer != null ? bill.customer.mobileno : "";
                                    delivery.Deliverystatus = Delivery.DeliveryStatus.Received;
                                    delivery.received = true;
                                    deliverylist.Add(delivery);
                                }
                            }
                            deliverylist.ForEach(x =>
                            {
                                x.settlementcode = request.Item.settlementcode;
                                x.candeliver = x.status == 1;
                            });
                        }
                    }


                    var creteria = new BillCreteria()
                    {
                        billno = request.Item.settlementcode,
                        locationid = request.Item.locationid,
                        isimport = true,
                        removecancelled = true
                    };
                    var bills = await Task<List<Bill>>.Run(() => BillData.Search(creteria));

                    if (!issettlementcode && bills.Count == 0)
                    {
                        response = new ActionMulitipleResponse<Delivery>();
                        response.Items = new List<Delivery>();
                        response.Items.AddRange(deliverylist);
                        response.Success = true;
                        response.ErrorMessage = "BILL CANCELLED";
                        return this.Ok(response);
                    }

                    if (deliverylist.Count == 0 && bills.Count == 0)
                    {
                        response = new ActionMulitipleResponse<Delivery>();
                        response.Items = null;
                        response.Success = false;
                        response.ErrorMessage = "NOT FOUND";
                        return this.Ok(response);
                    }
                    if (deliverylist.Count == 0)
                    {
                        if (bills.Count == 1)
                        {
                            Delivery.ReceivedItem receiveditem = new Delivery.ReceivedItem(bills[0],
                        Services.ConfigManager.Instance(BillData.Context.OrganizationID).Config.system_ismfgapplication);
                            delivery.amount = receiveditem.amount;
                            delivery.billid = bills[0].id;
                            delivery.billno = receiveditem.billno;
                            delivery.billdate = bills[0].billdate;
                            delivery.receivedon = DeliveryData.Context.DateTimeNow;
                            delivery.qty = (int)receiveditem.qty;
                            delivery.locationid = request.Item.locationid;
                            delivery.counterid = request.Item.counterid;
                            delivery.billinfo = receiveditem;
                            delivery.receivedby = DeliveryData.Context.UserID;
                            delivery.receivedon = DeliveryData.Context.DateTimeNow;
                            delivery.settled = bills[0].settled;
                            delivery.customername = bills[0].customer != null ? bills[0].customer.name : "";
                            delivery.contactno = bills[0].customer != null ? bills[0].customer.mobileno : "";
                            if (!InfyPOS.Services.ConfigManager.Instance(new BaseData().Context.OrganizationID).Config.system_supportdeliverybarcode)
                            {
                                delivery.candeliver = bills[0].settled &&
                                    (bills[0].billtype == Bill.BillTypes.Cash || bills[0].billtype == Bill.BillTypes.Credit);
                            }
                            delivery.Deliverystatus = Delivery.DeliveryStatus.Received;
                            Delivery result = await Task<Delivery>.Run(() => DeliveryData.Insert(delivery));
                            response = new ActionMulitipleResponse<Delivery>();
                            response.Items = new List<Delivery>();
                            response.Items.AddRange(new Delivery[] { result });
                            result.received = true;
                            result.receivedcount = await Task<int>.Run(() => DeliveryData.GetReceived(bills[0].billdate,
                                request.Item.locationid, request.Item.counterid, request.Item.isalteration));
                            response.Success = true;
                            response.ErrorMessage = "Recieved - " + delivery.billno + " @ " + delivery.amount.ToString("N0");
                            return this.Ok(response);
                        }
                    }
                    else if (bills.Count > 0)
                    {
                        if (deliverylist.Any(e => e.Deliverystatus != Delivery.DeliveryStatus.Delivered))
                        {
                            if (deliverylist.Any(e => e.Deliverystatus == Delivery.DeliveryStatus.Received))
                            {
                                response = new ActionMulitipleResponse<Delivery>();
                                response.Items = new List<Delivery>();
                                response.Items.AddRange(deliverylist.FindAll(e => e.Deliverystatus == Delivery.DeliveryStatus.Received));
                                deliverylist.ForEach(x => x.received = true);
                                response.Items[0].receivedcount = await Task<int>.Run(() => DeliveryData.GetReceived(bills[0].billdate,
                                    request.Item.locationid, request.Item.counterid, request.Item.isalteration));
                                response.Success = true;
                                return this.Ok(response);
                            }
                            else if (bills.Exists(e => !e.settled))
                            {
                                deliverylist.ForEach(e => e.settled = bills.Exists(b => b.id == e.billid && e.settled));
                                response = new ActionMulitipleResponse<Delivery>();
                                response.Items = deliverylist;
                                response.Success = true;
                                response.ErrorMessage = "NOT PAID";
                                return this.Ok(response);
                            }
                            else
                            {
                                if (!InfyPOS.Services.ConfigManager.Instance(new BaseData().Context.OrganizationID).Config.system_supportdeliverybarcode)
                                {
                                    deliverylist.ForEach(x =>
                                    {
                                        x.Deliverystatus = Delivery.DeliveryStatus.Delivered;
                                        x.deliveredby = DeliveryData.Context.UserID;
                                        x.deliveredon = DeliveryData.Context.DateTimeNow;
                                        x.modifiedby = DeliveryData.Context.UserID;
                                        x.modifiedon = DeliveryData.Context.DateTimeNow;
                                        x.candeliver = true;
                                        DeliveryData.Update(x);
                                    });
                                    response = new ActionMulitipleResponse<Delivery>();
                                    response.Items = new List<Delivery>();
                                    response.Items.AddRange(deliverylist);
                                    response.Success = true;
                                    response.ErrorMessage = "Delivered - " +
                                        string.Join(",", deliverylist.ConvertAll(e => e.billno)) +
                                        " @ " + deliverylist.Sum(e => e.amount).ToString("N0");
                                    return this.Ok(response);
                                }
                                else if (issettlementcode)
                                {
                                    response = new ActionMulitipleResponse<Delivery>();
                                    response.Items = new List<Delivery>();
                                    response.Items.AddRange(deliverylist);
                                    deliverylist.ForEach(x => x.candeliver = true);
                                    deliverylist[0].receivedcount = await Task<int>.Run(() => DeliveryData.GetReceived(deliverylist[0].billdate,
                                        request.Item.locationid, request.Item.counterid, request.Item.isalteration));
                                    response.Success = true;
                                    return this.Ok(response);
                                }
                                else
                                {
                                    response = new ActionMulitipleResponse<Delivery>();
                                    response.Items = new List<Delivery>();
                                    response.Items.AddRange(deliverylist);
                                    deliverylist.ForEach(x => x.candeliver = true);
                                    deliverylist[0].receivedcount = await Task<int>.Run(() => DeliveryData.GetReceived(bills[0].billdate,
                                        request.Item.locationid, request.Item.counterid, request.Item.isalteration));
                                    response.Success = true;
                                    return this.Ok(response);
                                }
                            }
                        }
                        else
                        {
                            response = new ActionMulitipleResponse<Delivery>();
                            //response.Items = new List<Delivery>();
                            //response.Items.AddRange(deliverylist);
                            response.Items = null;
                            response.Success = false;
                            response.ErrorMessage = "Already Delivered - " + string.Join(",", deliverylist.ConvertAll(e => e.billno)) + " @ " +
                                deliverylist.Sum(e => e.amount).ToString("N0") + " AT " +
                                deliverylist.First().deliveredon.ToString("hh:mm tt");
                            return this.Ok(response);
                        }
                    }
                    else
                    {
                        response = new ActionMulitipleResponse<Delivery>();
                        response.Items = new List<Delivery>();
                        response.Items.AddRange(deliverylist);
                        response.Success = true;
                        return this.Ok(response);
                    }
                }

                if (!string.IsNullOrEmpty(request.Item.settlementcode))
                {
                    System.Text.StringBuilder message = new System.Text.StringBuilder();
                    Data.BillsettlementmasterDataService billsettlementdata = new BillsettlementmasterDataService();
                    var settlement = billsettlementdata.Search(new BillsettlementmasterCreteria()
                    {
                        code = request.Item.settlementcode
                    });
                    var ids = new List<long>();
                    if (settlement != null && settlement.Count > 0)
                    {
                        ids.AddRange(settlement[0].Billsettlement.ConvertAll(e => e.billid));
                    }
                    var creteria = new DeliveryCreteria()
                    {
                        billids = ids
                    };
                    var bills = await Task<List<Bill>>.Run(() => BillData.Search(new BillCreteria()
                    {
                        ids = ids,
                        isimport = true,
                        removecancelled = true
                    }));
                    List<Delivery> result = new List<Delivery>();
                    if (ids.Count > 0)
                    {

                        result = await Task<List<Delivery>>.Run(() => DeliveryData.Search(creteria));
                        foreach (var item in result)
                        {
                            if (item.Deliverystatus != Delivery.DeliveryStatus.Delivered)
                            {
                                if (!InfyPOS.Services.ConfigManager.Instance(new BaseData().Context.OrganizationID).Config.system_supportdeliverybarcode)
                                {
                                    item.Deliverystatus = Delivery.DeliveryStatus.Delivered;
                                    item.deliveredby = DeliveryData.Context.UserID;
                                    item.deliveredon = DeliveryData.Context.DateTimeNow;
                                    item.modifiedby = DeliveryData.Context.UserID;
                                    item.modifiedon = DeliveryData.Context.DateTimeNow;
                                    var newitem = await Task<Delivery>.Run(() => DeliveryData.Update(item));
                                    message.AppendLine("Delivered - " + item.billno + " @ " + item.amount.ToString("N0"));
                                }
                            }
                            else
                            {
                                message.AppendLine("Already Delivered - " + item.billno + " @ " + item.amount.ToString("N0"));
                            }
                        }
                        if (result.Count == 0 || !result.All(e => settlement[0].Billsettlement.Exists(x => x.billid == e.billid)))
                        {
                            foreach (var item in settlement[0].Billsettlement.Where(e => !result.Exists(x => x.billid == e.billid)))
                            {

                                message.AppendLine("Not RECEIVED - " + item.billno + " @ " + bills.Find(e => e.id == item.billid).total.ToString("N0"));
                                if (bills.Count == 1)
                                {
                                    Delivery.ReceivedItem receiveditem = new Delivery.ReceivedItem(bills[0],
                        Services.ConfigManager.Instance(BillData.Context.OrganizationID).Config.system_ismfgapplication);
                                    delivery.amount = receiveditem.amount;
                                    delivery.billid = bills[0].id;
                                    delivery.billno = receiveditem.billno;
                                    delivery.receivedon = DeliveryData.Context.DateTimeNow;
                                    delivery.qty = (int)receiveditem.qty;
                                    delivery.locationid = bills[0].locationid;
                                    delivery.billinfo = receiveditem;
                                    delivery.receivedby = DeliveryData.Context.UserID;
                                    delivery.receivedon = DeliveryData.Context.DateTimeNow;
                                    delivery.Deliverystatus = Delivery.DeliveryStatus.NotReceived;
                                    delivery.deliveredby = DeliveryData.Context.UserID;
                                    delivery.deliveredon = delivery.receivedon;
                                    result.Add(delivery);
                                }
                            }
                        }
                    }
                    result.ForEach(e => e.settled = true);
                    response = new ActionMulitipleResponse<Delivery>();
                    response.Items = new List<Delivery>();
                    response.Items.AddRange(result);
                    response.Success = true;
                    response.ErrorMessage = message.ToString();
                    foreach (var bill in bills)
                    {
                        if (!result.Exists(e => e.billid == bill.id))
                        {
                            result.Add(new Delivery()
                            {
                                billno = bill.billno,
                                billid = bill.id,
                                billinfo = new Delivery.ReceivedItem(bill,
                        Services.ConfigManager.Instance(BillData.Context.OrganizationID).Config.system_ismfgapplication),
                                notreceived = true,
                                settled = true,
                                candeliver = true
                            });
                        }
                        else
                        {
                            result.Find(e => e.billid == bill.id).billinfo = new Delivery.ReceivedItem(bill,
                        Services.ConfigManager.Instance(BillData.Context.OrganizationID).Config.system_ismfgapplication);
                            result.Find(e => e.billid == bill.id).settled = true;
                            result.Find(e => e.billid == bill.id).candeliver = true;
                        }
                    }
                    return this.Ok(response);
                }

                return this.NotFound();
            }
            else
            {

                List<Delivery> result = await Task<List<Delivery>>.Run(() => DeliveryData.Search(request.Item));
                return this.Ok(result);
            }
        }

        public string PrintBarcode(Delivery odelivery, string key)
        {
            InfyPOS.Data.ConfigurationDataService config = new ConfigurationDataService();
            var reportformats = config.Search(new ConfigurationCreteria
            {
                key = key,
                locationid = odelivery.locationid,
                organizationid = 0
            });
            if (reportformats.Count == 0) return "";

            if (odelivery.customerid > 0)
            {
                CustomerDataService customerDataService = new CustomerDataService();
                odelivery.customer = customerDataService.Search(new CustomerCreteria()
                {
                    id = odelivery.customerid
                }).First();
            }
            else
            {
                odelivery.customer = new Customer();
            }

            if (odelivery.companyid > 0)
            {
                CompanyDataService customerDataService = new CompanyDataService();
                odelivery.company = customerDataService.Search(new CompanyCreteria()
                {
                    id = odelivery.companyid
                }).First();
            }
            else
            {
                odelivery.company = new Company();
            }

            DelivaryGiftBarcode delivaryGiftBarcode = new DelivaryGiftBarcode(odelivery);
            var reportformat = reportformats.First();
            InfyPOS.Report.CitizenBarcode barcodeprinter = new Report.CitizenBarcode(
            odelivery.company == null ? "" :  odelivery.company.name,
            reportformat.reportformat.format, reportformat.reportformat.mode, reportformat.nooflines);
            var response = barcodeprinter.Render(new List<Report.IBarcodeFormat>(new Report.IBarcodeFormat[] { delivaryGiftBarcode }));
            if (response.Count == 1)
                return Convert.ToBase64String(System.Text.ASCIIEncoding.ASCII.GetBytes(response[0]));
            return "";
        }

        public string PrintDocument(Delivery odelivery, string key)
        {
            InfyPOS.Data.ConfigurationDataService config = new ConfigurationDataService();
            var reportformats = config.Search(new ConfigurationCreteria
            {
                key = key,
                locationid = odelivery.locationid,
                organizationid = 0
            });
            if (reportformats.Count == 0) return "";
            var additional = new SortedDictionary<string, object>();
            var imageData = new Common.ImageData() { Context = new UserClaim() { OrganizationID = config.Context.OrganizationID } };
            ReportLibrary.DefaultPageSettings setting = new ReportLibrary.DefaultPageSettings();
            ReportLibrary.DotMatrixReporter labreport = new ReportLibrary.DotMatrixReporter(imageData.Context.OrganizationID, new ReportLibrary.DotMatrixReporter.PrinterConfig());
            labreport.format = ReportLibrary.DosFormatParser.ParseXML(imageData.Context.OrganizationID, reportformats[0].reportformat.format);

            List<string> imageids = labreport.format.GetImages();
            labreport.Additional = new SortedDictionary<string, object>();
            foreach (var imgid in imageids.FindAll(x => x.Length > 6))
            {
                var file = imageData.GetData(imgid.LongFromBase62());
                if (file != null && file.data != null)
                {
                    additional.Add("IMG-" + imgid, file.data);
                }
            }

            if (odelivery.validatedby > 0)
            {
                EmployeeDataService employeeDataService = new EmployeeDataService();
                odelivery.validatedbyname = employeeDataService.SearchHeader(new EmployeeCreteria()
                {
                    id = odelivery.validatedby
                }).First().name;
            }
            if (odelivery.createdby > 0)
            {
                EmployeeDataService employeeDataService = new EmployeeDataService();
                var empnames = employeeDataService.GetUserName(new List<long>(new long[] { odelivery.createdby }));
                odelivery.cashiername = empnames[0].name;
            }
            ReferencelistDataService referencelistDataService = new ReferencelistDataService();
            var location = referencelistDataService.Search(new ReferencelistCreteria()
            {
                id = odelivery.locationid
            }).First();

            odelivery.location = new CompanyLocation(location);
            odelivery.location.name = location.name;
            odelivery.location.phonenumber = location.additionalinfo == null ? "" : location.additionalinfo.formula;
            odelivery.location.address = location.additionalinfo == null ? "" : location.additionalinfo.address;
            odelivery.location.addresslines = new List<CompanyLocation.Address>();
            if (!string.IsNullOrEmpty(odelivery.location.address))
            {
                var addresslist = odelivery.location.address.Split(new string[] { "\r\n", "\r", "\n" }, StringSplitOptions.None);
                addresslist.ToList().ForEach(e =>
                {
                    odelivery.location.addresslines.Add(new CompanyLocation.Address()
                    {
                        value = e
                    });
                });
            }
            odelivery.countername = referencelistDataService.Search(new ReferencelistCreteria() { id = odelivery.counterid }).First().name;

            if (odelivery.giftlist != null && odelivery.giftlist.Count > 0)
            {
                var giftmarkernames = referencelistDataService.Search(new ReferencelistCreteria()
                {
                    ids = odelivery.giftlist.ConvertAll(x => x.stockmarkerid)
                });
                odelivery.giftlist.ForEach(x =>
                {
                    x.giftmarkername = giftmarkernames.Exists(y => y.id == x.stockmarkerid) ? giftmarkernames.Find(y => y.id == x.stockmarkerid).name : "UNKNOW";
                });
            }

            bool issetlist = reportformats[0].reportformat.format.IndexOf("setlist") > 0;
            if (reportformats[0].reportformat.format.IndexOf("setlist") > 0)
            {
                odelivery.setlist = new List<Delivery.SettlementItem>();
                int sno = 1;
                foreach (var setitem in odelivery.billinfo.items.GroupBy(x => x.settlementcode))
                {
                    odelivery.setlist.Add(new Delivery.SettlementItem()
                    {
                        amount = setitem.Sum(x => x.amount),
                        code = setitem.Key,
                        date = setitem.First().settlementon,
                        sno = sno++
                    });
                }
            }
            byte[] document = null;
            if (odelivery.isgift && issetlist)
            {
                document = labreport.format.RenderBytes(new ImageData(), odelivery, odelivery.setlist, additional, true);
            }
            else
            {
                document = labreport.format.RenderBytes(new ImageData(), odelivery,
                    (odelivery.isgift) ? odelivery.billinfo.Bills : odelivery.billinfo.items, additional, true);
            }

            using (System.IO.MemoryStream ms = new System.IO.MemoryStream())
            {
                System.IO.BinaryWriter bw = new System.IO.BinaryWriter(ms);
                bw.Write(document);
                int c = 1;
                while (c < reportformats[0].noofcopy)
                {
                    bw.Write(document);
                    c++;
                }
                return ms.Length > 0 ? Convert.ToBase64String(ms.ToArray()) : "";
            }
            //return Convert.ToBase64String(document);
        }

        [AllowAnonymous]
        [HttpGet]
        [Route("GetReceived/{organizationid}/{isdelivery}/{locationid}/{counterid}")]

        public async Task<IHttpActionResult> GetReceived(string organizationid, string isdelivery,
            string locationid, string counterid)
        {
            DeliveryDataService DeliveryData = new DeliveryDataService();
            DeliveryData.Context = new UserClaim()
            {
                OrganizationID = long.Parse(organizationid)
            };
            int result = await Task<int>.Run(() => DeliveryData.GetReceived(DeliveryData.Context.DateTimeNow.Date,
                long.Parse(locationid), long.Parse(counterid), isdelivery == "false"));
            return this.Ok(result);
        }

        [HttpPost]
        [Route("Report")]
        public async Task<IHttpActionResult> Report(ActionRequest<DeliverySearch> request)
        {
            DeliveryDataService DeliveryData = new DeliveryDataService();
            if (request.Item.mode > 2)
            {
                List<DeliveryGiftSummary> result = await Task<List<DeliveryGiftSummary>>.Run(() => DeliveryData.GiftIssueSummary(request.Item));
                return this.Ok(result);
            }
            else
            {
                List<DelieryGiftItem> result = await Task<List<DelieryGiftItem>>.Run(() => DeliveryData.GiftIssueReport(request.Item));
                return this.Ok(result);
            }
        }

        [HttpPost]
        [Route("UpateAlteration")]
        public async Task<IHttpActionResult> UpateAlteration(ActionRequest<Delivery> request)
        {
            DeliveryDataService DeliveryData = new DeliveryDataService();
            request.Item.modifiedby = DeliveryData.Context.UserID;
            request.Item.modifiedon = DeliveryData.DateTimeNow;
            request.Item.isalteration = true;
            if (request.Item.status == 1)
                request.Item.validatedby = DeliveryData.Context.UserID;
            else if (request.Item.status == 2)
            {
                request.Item.deliveredby = DeliveryData.Context.UserID;
                request.Item.deliveredon = DeliveryData.DateTimeNow;
            }

            Delivery result = await Task<Delivery>.Run(() => DeliveryData.Update(request.Item));
            result.printdocument = PrintDocument(request.Item, "ALTERATION");

            if (request.Item.status == 1)
                Processors.Schedule_Actions.Instance.OnChange(DeliveryData.Context.OrganizationID, request.Item, Processors.Schedule_Actions.Actions.AlterationReady);
            else if (request.Item.status == 2)
                Processors.Schedule_Actions.Instance.OnChange(DeliveryData.Context.OrganizationID, request.Item, Processors.Schedule_Actions.Actions.AlterationDelivered);

            return this.Ok(result);
        }

        [HttpPost]
        [Route("SaveGiftCoupon")]
        public async Task<IHttpActionResult> SaveGiftCoupon(ActionRequest<Delivery> request)
        {
            DeliveryDataService DeliveryData = new DeliveryDataService();
            if (request.Item.id != 0)
            {
                Delivery result = await Task<Delivery>.Run(() => DeliveryData.UpdateGiftDelivery(request.Item));
                return this.Ok(result);
            }
            else
            {
                request.Item.isalteration = false;
                request.Item.isgift = false;
                request.Item.isactive = true;
                request.Item.version = 1;
                request.Item.receivedon = DeliveryData.Context.DateTimeNow;
                request.Item.receivedby = DeliveryData.Context.UserID;
                request.Item.receivedon = DeliveryData.Context.DateTimeNow;
                request.Item.Deliverystatus = Delivery.DeliveryStatus.StockMarker;
                Delivery result = await Task<Delivery>.Run(() => DeliveryData.SaveGiftCoupon(request.Item));
                result.printdocument = PrintDocument(result, "GIFTCOUPON");
                result.printbarcode = PrintBarcode(result, "GIFTBARCODE"); 
                return this.Ok(result);
            }
        }

        [HttpPost]
        [Route("SaveGiftDelivery")]
        public async Task<IHttpActionResult> SaveGiftDelivery(ActionRequest<Delivery> request)
        {

            DeliveryDataService DeliveryData = new DeliveryDataService();
            if (request.Item.id != 0)
            {
                Delivery result = await Task<Delivery>.Run(() => DeliveryData.UpdateGiftDelivery(request.Item));
                return this.Ok(result);
            }
            else
            {
                request.Item.isalteration = false;
                request.Item.isgift = false;
                request.Item.isactive = true;
                request.Item.version = 1;
                request.Item.receivedon = DeliveryData.Context.DateTimeNow;
                request.Item.receivedby = DeliveryData.Context.UserID;
                request.Item.receivedon = DeliveryData.Context.DateTimeNow;
                request.Item.Deliverystatus = Delivery.DeliveryStatus.StockMarker;
                Delivery result = await Task<Delivery>.Run(() => DeliveryData.SaveGiftDelivery(request.Item));
                result.printdocument = PrintDocument(result, "GIFTDELIVERY");
                return this.Ok(result);
            }
        }

        [HttpPost]
        [Route("SaveAlteration")]
        public async Task<IHttpActionResult> SaveAlteration(ActionRequest<Delivery> request)
        {
            DeliveryDataService DeliveryData = new DeliveryDataService();
            request.Item.isalteration = true;
            request.Item.isactive = true;
            request.Item.version = 1;
            request.Item.billno = request.Item.billinfo.billno;
            request.Item.receivedon = DeliveryData.Context.DateTimeNow;
            request.Item.qty = (int)request.Item.billinfo.qty;
            request.Item.amount = request.Item.billinfo.amount;
            request.Item.receivedby = DeliveryData.Context.UserID;
            request.Item.receivedon = DeliveryData.Context.DateTimeNow;
            request.Item.Deliverystatus = Delivery.DeliveryStatus.Received;
            Delivery result = await Task<Delivery>.Run(() => DeliveryData.Insert(request.Item));

            Processors.Schedule_Actions.Instance.OnChange(DeliveryData.Context.OrganizationID, request.Item, Processors.Schedule_Actions.Actions.AlterationReceived);

            result.printdocument = PrintDocument(request.Item, "ALTERATION");
            return this.Ok(result);
        }


        [HttpPost]
        [Route("Print")]
        public async Task<IHttpActionResult> Print(ActionRequest<Delivery> request)
        {
            DeliveryDataService DeliveryData = new DeliveryDataService();
            List<Delivery> result = await Task<List<Delivery>>.Run(() => DeliveryData.Search(new DeliveryCreteria() { id = request.Item.id }));
            string key = result[0].isalteration ? "ALTERATION" : "GIFTDELIVERY";
            if (result[0].isgiftcoupon)
                key = "GIFTCOUPON";
            result[0].printdocument = PrintDocument(result[0], key);
            return this.Ok(result[0]);
        }

        [HttpPost]
        [Route("Save")]
        public async Task<IHttpActionResult> Save(ActionRequest<Delivery> request)
        {
            DeliveryDataService DeliveryData = new DeliveryDataService();
            if (request.Item.id <= 0)
            {
                Delivery result = await Task<Delivery>.Run(() => DeliveryData.Insert(request.Item));
                return this.Ok(result);
            }
            else
            {
                Delivery result = await Task<Delivery>.Run(() => DeliveryData.Update(request.Item));
                return this.Ok(result);
            }
        }

        [HttpPost]
        [Route("Insert")]
        public async Task<IHttpActionResult> Insert(ActionRequest<Delivery> request)
        {
            DeliveryDataService DeliveryData = new DeliveryDataService();
            Delivery result = await Task<Delivery>.Run(() => DeliveryData.Insert(request.Item));
            return this.Ok(result);
        }

        [HttpPost]
        [Route("Cancel")]
        public async Task<IHttpActionResult> Cancel(ActionRequest<Delivery> request)
        {
            DeliveryDataService DeliveryData = new DeliveryDataService();
            Delivery result = await Task<Delivery>.Run(() => DeliveryData.Cancel(request.Item));
            return this.Ok(result);
        }

        [HttpPost]
        [Route("Update")]
        public async Task<IHttpActionResult> Update(ActionRequest<Delivery> request)
        {
            DeliveryDataService DeliveryData = new DeliveryDataService();
            Delivery result = await Task<Delivery>.Run(() => DeliveryData.Update(request.Item));
            return this.Ok(result);
        }


        [HttpPost]
        [Route("UpdateAll")]
        public async Task<IHttpActionResult> Update(ActionMulitpleRequest<Delivery> request)
        {
            DeliveryDataService DeliveryData = new DeliveryDataService();
            List<Delivery> result = await Task<List<Delivery>>.Run(() => DeliveryData.UpdateAll(request.Items));
            return this.Ok(result);
        }

        [HttpPost]
        [Route("Delete")]
        public async Task<IHttpActionResult> Delete(ActionRequest<Delivery> request)
        {
            DeliveryDataService DeliveryData = new DeliveryDataService();
            bool result = await Task<bool>.Run(() => DeliveryData.Delete(request.Item));
            return this.Ok(result);
        }
    }
}