using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using InfyPOS.Models;
using InfyPOS.Common;
using Npgsql;
using InfyPOS.Services;
using Newtonsoft.Json.Converters;


namespace InfyPOS.Data
{
    public class DeliveryDataService : SAASData
    {
        const string SQL_Search = @"select id, isactive, createdby, modifiedby, version, createdon, modifiedon, billid, locationid,counterid,validatedby, amount, billno, receivedby, deliveredby, returnby, cancelledby, qty, 
                                    receivedon, deliveredon, status,returnto,isalteration,billinfo,contactno,customername,altationno,isgift,isgiftcoupon,voucherinfo,giftlist,remarks from delivery where id=@id";

        const string SQL_Insert = @"INSERT INTO public.delivery(isactive, createdby, modifiedby, version, createdon, modifiedon, billid, locationid,counterid,validatedby, amount, billno, receivedby, deliveredby, returnby, cancelledby,
                                    qty, receivedon, deliveredon, status,returnto,isalteration,billinfo,contactno,customername,altationno,isgift,isgiftcoupon,voucherinfo,giftlist,remarks) 
                                    VALUES(@isactive, @createdby, @modifiedby, @version, @createdon, @modifiedon, @billid, @locationid,@counterid,@validatedby, @amount, @billno, @receivedby, @deliveredby, @returnby, @cancelledby,
                                    @qty, @receivedon, @deliveredon, @status,@returnto,@isalteration,@billinfo,@contactno,@customername,@altationno,@isgift,@isgiftcoupon,@voucherinfo,@giftlist,@remarks) RETURNING id";


        const string SQL_Update = @"UPDATE public.delivery SET modifiedby=@modifiedby, version=@version+1, modifiedon=@modifiedon, billid=@billid,counterid=@counterid,validatedby=@validatedby, locationid=@locationid, amount=@amount, 
                                    billno=@billno, receivedby=@receivedby, deliveredby=@deliveredby,billinfo=@billinfo,contactno=@contactno,customername=@customername,giftlist=@giftlist, remarks=@remarks,
                                    returnby=@returnby, returnto=@returnto , cancelledby=@cancelledby, qty=@qty, receivedon=@receivedon, deliveredon=@deliveredon, status=@status where id=@id and version=@version RETURNING version";

        const string SQL_Delete = @"update public.delivery set isactive=false,version=@version+1,modifiedby=@modifiedby,modifiedon=@modifiedon where id=@id and version=@version";

        public List<Delivery> Search(DeliveryCreteria _delivery)
        {
            List<Delivery> lst = new List<Delivery>();
            NpgsqlConnection conn = CreateConnection();
            try
            {
                conn.Open();

                NpgsqlCommand command = null;
                if (_delivery.id != 0)
                {
                    command = GetCommand(SQL_Search, conn);
                    command.Parameters.Add("id", NpgsqlTypes.NpgsqlDbType.Bigint).Value = _delivery.id;
                }
                else
                {
                    QueryBuilder qb = new QueryBuilder();
                    qb.CurrentQuery = SQL_Search.Substring(0, SQL_Search.IndexOf("where"));
                    qb.AddParameter("isactive", true, "=");

                    if (_delivery.billids != null)
                    {
                        qb.AddInParamer("delivery.billid", string.Join<long>(",", _delivery.billids));
                    }

                    if (_delivery.isalteration)
                    {
                        qb.AddInParamer("delivery.id", @"select ID from delivery
                                        LEFT JOIN LATERAL json_array_elements (delivery.billinfo->'items') schemelist ON true
                                        where schemelist->>'billno' = @billno");
                    }
                    else
                    {
                        if (!string.IsNullOrEmpty(_delivery.billno))
                            qb.AddParameter("billno", _delivery.billno, "=");
                    }

                    if (_delivery.locationid > 0)
                        qb.AddParameter("locationid", _delivery.locationid, "=");

                    if (_delivery.counterid > 0)
                        qb.AddParameter("counterid", _delivery.counterid, "=");

                    qb.AddParameter("isalteration", _delivery.isalteration, "=");
                    if (!string.IsNullOrEmpty(_delivery.altationno))
                        qb.AddParameter("altationno", _delivery.altationno, "=");
                    qb.AddParameter("isgift", _delivery.isgift, "=");
                    qb.AddParameter("isgiftcoupon", _delivery.isgiftcoupon, "=");

                    command = qb.GetCommand(conn);
                    command.Parameters.AddWithValue("billno", string.IsNullOrEmpty(_delivery.billno) ? "" : _delivery.billno);
                }

                using (System.Data.IDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        Delivery odelivery = new Delivery();
                        odelivery.id = reader["id"] == DBNull.Value ? 0 : Convert.ToInt64(reader["id"]);
                        odelivery.isactive = reader["isactive"] == DBNull.Value ? false : Convert.ToBoolean(reader["isactive"]);
                        odelivery.createdby = reader["createdby"] == DBNull.Value ? 0 : Convert.ToInt64(reader["createdby"]);
                        odelivery.modifiedby = reader["modifiedby"] == DBNull.Value ? 0 : Convert.ToInt64(reader["modifiedby"]);
                        odelivery.version = reader["version"] == DBNull.Value ? 0 : Convert.ToInt32(reader["version"]);
                        odelivery.createdon = reader["createdon"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(reader["createdon"]);
                        odelivery.modifiedon = reader["modifiedon"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(reader["modifiedon"]);
                        odelivery.billid = reader["billid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["billid"]);
                        odelivery.locationid = reader["locationid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["locationid"]);
                        odelivery.counterid = reader["counterid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["counterid"]);
                        odelivery.validatedby = reader["validatedby"] == DBNull.Value ? 0 : Convert.ToInt64(reader["validatedby"]);
                        odelivery.amount = reader["amount"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["amount"]);
                        odelivery.billno = reader["billno"] == DBNull.Value ? "" : reader["billno"].ToString();
                        odelivery.remarks = reader["remarks"] == DBNull.Value ? "" : reader["remarks"].ToString();

                        odelivery.altationno = reader["altationno"] == DBNull.Value ? "" : reader["altationno"].ToString();
                        odelivery.customername = reader["customername"] == DBNull.Value ? "" : reader["customername"].ToString();
                        odelivery.contactno = reader["contactno"] == DBNull.Value ? "" : reader["contactno"].ToString();
                        odelivery.billinfo_json = reader["billinfo"] == DBNull.Value ? "null" : reader["billinfo"].ToString();
                        odelivery.voucherinfo_json = reader["voucherinfo"] == DBNull.Value ? "null" : reader["voucherinfo"].ToString();
                        odelivery.giftlist_json = reader["giftlist"] == DBNull.Value ? "null" : reader["giftlist"].ToString();

                        odelivery.receivedby = reader["receivedby"] == DBNull.Value ? 0 : Convert.ToInt64(reader["receivedby"]);
                        odelivery.deliveredby = reader["deliveredby"] == DBNull.Value ? 0 : Convert.ToInt64(reader["deliveredby"]);
                        odelivery.returnby = reader["returnby"] == DBNull.Value ? 0 : Convert.ToInt64(reader["returnby"]);
                        odelivery.returnto = reader["returnto"] == DBNull.Value ? 0 : Convert.ToInt64(reader["returnto"]);
                        odelivery.cancelledby = reader["cancelledby"] == DBNull.Value ? 0 : Convert.ToInt64(reader["cancelledby"]);
                        odelivery.qty = reader["qty"] == DBNull.Value ? 0 : Convert.ToInt32(reader["qty"]);
                        odelivery.isalteration = reader["isalteration"] == DBNull.Value ? false : Convert.ToBoolean(reader["isalteration"]);
                        odelivery.isgift = reader["isgift"] == DBNull.Value ? false : Convert.ToBoolean(reader["isgift"]);
                        odelivery.isgiftcoupon = reader["isgiftcoupon"] == DBNull.Value ? false : Convert.ToBoolean(reader["isgiftcoupon"]);
                        odelivery.receivedon = reader["receivedon"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(reader["receivedon"]);
                        odelivery.deliveredon = reader["deliveredon"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(reader["deliveredon"]);
                        odelivery.status = reader["status"] == DBNull.Value ? 0 : Convert.ToInt32(reader["status"]);

                        if (!lst.Exists(e => e.billno == _delivery.billno))
                            lst.Add(odelivery);
                    }
                }

                if (!string.IsNullOrEmpty(_delivery.billno) && lst.Count == 2)
                {
                    if (lst.FindAll(e => e.billno == _delivery.billno).Count == 2)
                    {
                        lst.Remove(lst.Find(e => e.billno == _delivery.billno));
                    }
                }

                if (!_delivery.isalteration)
                {
                    if (!string.IsNullOrEmpty(_delivery.billno) || (_delivery.billids != null && _delivery.billids.Count > 0))
                    {
                        BillDataService billitemsDataService = new BillDataService() { Context = this.Context };
                        if (lst.Count == 0 && _delivery.billids != null && _delivery.billids.Count > 0)
                        {
                            lst.AddRange(_delivery.billids.ConvertAll(x => new Delivery()
                            {
                                billid = x
                            }));
                        }

                        if (lst.Count > 0)
                        {
                            var bills = billitemsDataService.Search(new BillCreteria()
                            {
                                id = lst.Count > 1 ? 0 : lst[0].billid,
                                ids = lst.Count > 1 ? lst.ConvertAll(x => x.billid) : null,
                                isimport = true
                            });
                            foreach (var bitem in lst)
                            {
                                if (bills.Exists(x => x.id == bitem.billid))
                                {
                                    var setbill = bills.Find(x => x.id == bitem.billid);
                                    if (setbill.customer != null)
                                    {
                                        bitem.customername = setbill.customer.name;
                                        bitem.contactno = setbill.customer.mobileno;
                                    }
                                    if (_delivery.isdelivery)
                                        bitem.billinfo = new Delivery.ReceivedItem(setbill, Services.ConfigManager.Instance(billitemsDataService.Context.OrganizationID).Config.system_ismfgapplication);
                                    else
                                        bitem.billinfo = GetGiftItem(setbill,
                                        Services.ConfigManager.Instance(billitemsDataService.Context.OrganizationID));
                                }
                            }
                        }

                        //foreach (var bitem in lst)
                        //{
                        //    var bills = billitemsDataService.Search(new BillCreteria()
                        //    {
                        //        id = bitem.billid
                        //    });
                        //    if (bills.Count > 0)
                        //    {
                        //        if (bills[0].customer != null)
                        //        {
                        //            bitem.customername = bills[0].customer.name;
                        //            bitem.contactno = bills[0].customer.mobileno;
                        //        }
                        //        if (_delivery.isdelivery)
                        //            bitem.billinfo = new Delivery.ReceivedItem(bills.First(), Services.ConfigManager.Instance(billitemsDataService.Context.OrganizationID).Config.system_ismfgapplication);
                        //        else
                        //            bitem.billinfo = GetGiftItem(bills.First(),
                        //            Services.ConfigManager.Instance(billitemsDataService.Context.OrganizationID));
                        //    }
                        //}
                    }
                }
                conn.Close();
            }
            catch (Exception exp)
            {
                if (conn != null) conn.Close();
                WebAPI.Common.Logger.Instance(Context.OrganizationID).Log(exp, WebAPI.Common.Logger.LogType.Error, _delivery);
            }
            return lst;
        }

        internal List<Delivery> SearchGift(string billno)
        {

            const string _SQL = @"Select id,cast(itemlist->> 'settlementcode' as character varying(20)) from delivery 
INNER JOIN LATERAL json_array_elements (delivery.billinfo->'items') itemlist ON true
where cast(itemlist->> 'settlementcode' as character varying(20)) = @settlementcode limit 1 ";

            long deliveryid = 0;
            List<RetailReport> lst = new List<RetailReport>();
            NpgsqlConnection conn = CreateConnection();
            try
            {
                conn.Open();
                NpgsqlCommand command = null;
                command = GetCommand(_SQL, conn);
                command.Parameters.Add("settlementcode", NpgsqlTypes.NpgsqlDbType.Varchar).Value = billno;

                using (System.Data.IDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        deliveryid = reader["id"] == DBNull.Value ? 0 : Convert.ToInt64(reader["id"]);
                    }
                }
                conn.Close();
            }
            catch (Exception exp)
            {
                if (conn != null) conn.Close();
                WebAPI.Common.Logger.Instance(Context.OrganizationID).Log(exp, WebAPI.Common.Logger.LogType.Error, billno);
            }

            if (deliveryid > 0)
                return Search(new DeliveryCreteria() { id = deliveryid });
            return null;
        }

        public Delivery.ReceivedItem GetGiftCouponItem(Bill bill, ConfigManager configManager)
        {
            var billitemcount = bill.Billitems.Count;




            if (configManager.LocationConfig[bill.locationid].gft_enable)
            {
                var allfreeitems = bill.Billitems.FindAll(x => x.schemediscount > 0 || x.schemeid > 0);
                allfreeitems = allfreeitems.FindAll(x => x.attributes != null && x.attributes.freeitems != null && x.attributes.freeitems.Count > 0);
                if (configManager.LocationConfig[bill.locationid].gft_excludediscount)
                {
                    bill.Billitems.RemoveAll(x => x.discountpercentage >= configManager.LocationConfig[bill.locationid].gft_excludediscountabove);
                }
                if (configManager.LocationConfig[bill.locationid].gft_excludeaddiscount)
                {
                    bill.Billitems.RemoveAll(x => x.additionaldiscountpercentage >= configManager.LocationConfig[bill.locationid].gft_excludeaddiscountabove);
                }

                if (configManager.LocationConfig[bill.locationid].gft_excludepromotion)
                {
                    var schemediscounts = bill.Billitems.FindAll(x => x.schemediscount > 0 || x.schemeid > 0);
                    if (schemediscounts.Count > 0)
                    {
                        var filterrows = schemediscounts.GroupBy(x => new { x.schemeid }).Select(y => new
                        {
                            schemeid = y.Key.schemeid,
                            receivable = y.Sum(s => s.receivable),
                            schemediscount = y.Sum(s => s.schemediscount),
                            items = y.ToList()
                        });
                        foreach (var item in filterrows)
                        {
                            if (ConfigManager.Instance(Context.OrganizationID).Config.gft_excludepromotionlist != null &&
                                ConfigManager.Instance(Context.OrganizationID).Config.gft_excludepromotionlist.Count > 0)
                            {
                                bill.Billitems.RemoveAll(x => ConfigManager.Instance(Context.OrganizationID).Config.gft_excludepromotionlist.Contains(x.schemeid));
                            }

                            if (configManager.LocationConfig[bill.locationid].gft_excludepromotionabove > 0 &&
                               configManager.LocationConfig[bill.locationid].gft_excludepromotionabove < (item.schemediscount / item.receivable) * 100)
                            {
                                bill.Billitems.RemoveAll(x => item.items.Exists(y => y.id == x.id));
                            }
                        }
                    }
                }

                if (!string.IsNullOrEmpty(configManager.LocationConfig[bill.locationid].gft_excludepromotioncode))
                {
                    var promotionedbill = InfyPOS.Services.PromotionManager.Instance(Context.OrganizationID).FindAvailableMatchingSchmes(this.Context,
                        new PromotionSearch()
                        {
                            locationid = bill.locationid,
                            barcodes = bill.Billitems.ConvertAll(x => x.barcode),
                            schemecodes = new List<string>(configManager.LocationConfig[bill.locationid].gft_excludepromotioncode.Split(','))
                        });

                    if (promotionedbill.Exists(x => x.eligible))
                    {
                        foreach (var eligible in promotionedbill.FindAll(x => x.eligible))
                        {
                            if (eligible.billitems != null &&
                                eligible.billitems.Count > 0)
                            {
                                bill.Billitems.RemoveAll(x => eligible.billitems.Exists(y => y.barcode == x.barcode));
                            }
                        }
                    }
                }

                decimal excluded = 0;
                if (billitemcount != bill.Billitems.Count)
                {
                    excluded = (bill.receivable + bill.rounding) - bill.Billitems.Sum(x => x.receivable);
                    bill.receivable = bill.Billitems.Sum(x => x.receivable);
                }


                allfreeitems.ForEach(x =>
                {
                    if (!bill.Billitems.Exists(y => y.id == x.id))
                    {
                        bill.Billitems.Add(x);
                    }
                });

                var response = new Delivery.ReceivedItem(bill, configManager.Config.system_ismfgapplication);
                response.excluded = excluded;
                return response;
            }
            else
            {
                return new Delivery.ReceivedItem(bill, configManager.Config.system_ismfgapplication);
            }
        }
        public Delivery.ReceivedItem GetGiftItem(Bill bill, ConfigManager configManager)
        {
            var billitemcount = bill.Billitems.Count;
            if (configManager.LocationConfig[bill.locationid].gft_enable)
            {
                var allfreeitems = bill.Billitems.FindAll(x => x.schemediscount > 0 || x.schemeid > 0);
                allfreeitems = allfreeitems.FindAll(x => x.attributes != null && x.attributes.freeitems != null && x.attributes.freeitems.Count > 0);
                if (configManager.LocationConfig[bill.locationid].gft_excludediscount)
                {
                    bill.Billitems.RemoveAll(x => x.discountpercentage >= configManager.LocationConfig[bill.locationid].gft_excludediscountabove);
                }
                if (configManager.LocationConfig[bill.locationid].gft_excludeaddiscount)
                {
                    bill.Billitems.RemoveAll(x => x.additionaldiscountpercentage >= configManager.LocationConfig[bill.locationid].gft_excludeaddiscountabove);
                }
                if (configManager.LocationConfig[bill.locationid].gft_excludepromotion)
                {
                    var schemediscounts = bill.Billitems.FindAll(x => x.schemediscount > 0 || x.schemeid > 0);
                    if (schemediscounts.Count > 0)
                    {
                        var filterrows = schemediscounts.GroupBy(x => new { x.schemeid }).Select(y => new
                        {
                            schemeid = y.Key.schemeid,
                            receivable = y.Sum(s => s.receivable),
                            schemediscount = y.Sum(s => s.schemediscount),
                            items = y.ToList()
                        });
                        foreach (var item in filterrows)
                        {
                            if (configManager.LocationConfig[bill.locationid].gft_excludepromotionabove == 0)
                            {
                                if (ConfigManager.Instance(Context.OrganizationID).Config.gft_excludepromotionlist != null &&
                                    ConfigManager.Instance(Context.OrganizationID).Config.gft_excludepromotionlist.Count > 0)
                                {
                                    bill.Billitems.RemoveAll(x => ConfigManager.Instance(Context.OrganizationID).Config.gft_excludepromotionlist.Contains(x.schemeid));
                                }
                                else
                                {
                                    bill.Billitems.RemoveAll(x => item.items.Exists(y => y.id == x.id));
                                }
                            }
                            else if (configManager.LocationConfig[bill.locationid].gft_excludepromotionabove < (item.schemediscount / item.receivable) * 100)
                            {
                                bill.Billitems.RemoveAll(x => item.items.Exists(y => y.id == x.id));
                            }
                        }
                    }
                }

                if (!string.IsNullOrEmpty(configManager.LocationConfig[bill.locationid].gft_excludepromotioncode))
                {
                    var promotionedbill = InfyPOS.Services.PromotionManager.Instance(Context.OrganizationID).FindAvailableMatchingSchmes(this.Context,
                        new PromotionSearch()
                        {
                            locationid = bill.locationid,
                            barcodes = bill.Billitems.ConvertAll(x => x.barcode),
                            schemecodes = new List<string>(configManager.LocationConfig[bill.locationid].gft_excludepromotioncode.Split(','))
                        });

                    if (promotionedbill.Exists(x => x.eligible))
                    {
                        foreach (var eligible in promotionedbill.FindAll(x => x.eligible))
                        {
                            if (eligible.billitems != null &&
                                eligible.billitems.Count > 0)
                            {
                                bill.Billitems.RemoveAll(x => eligible.billitems.Exists(y => y.barcode == x.barcode));
                            }
                        }
                    }
                }

                decimal excluded = 0;
                if (billitemcount != bill.Billitems.Count)
                {
                    excluded = (bill.receivable + bill.rounding) - bill.Billitems.Sum(x => x.receivable);
                    bill.receivable = bill.Billitems.Sum(x => x.receivable);
                }


                allfreeitems.ForEach(x =>
                {
                    if (!bill.Billitems.Exists(y => y.id == x.id))
                    {
                        bill.Billitems.Add(x);
                    }
                });

                var response = new Delivery.ReceivedItem(bill, configManager.Config.system_ismfgapplication);
                response.excluded = excluded;
                return response;
            }
            else
            {
                return new Delivery.ReceivedItem(bill, configManager.Config.system_ismfgapplication);
            }
        }
        public Delivery Insert(Delivery oDelivery)
        {

            NpgsqlTransaction transaction = null;
            try
            {
                using (NpgsqlConnection conn = CreateConnection())
                {
                    conn.Open();
                    transaction = conn.BeginTransaction();
                    NpgsqlCommand command = GetCommand(SQL_Insert, conn, transaction);

                    Insert(transaction, conn, oDelivery);
                    transaction.Commit();
                    conn.Close();
                }
            }
            catch (Exception exp)
            {
                WebAPI.Common.Logger.Instance(Context.OrganizationID).Log(exp, WebAPI.Common.Logger.LogType.Error, oDelivery);
            }

            return oDelivery;
        }

        public int GetReceived(DateTime date, long locationid, long counterid, string type)
        {
            var param = new SortedList<string, object>();
            param.Add("fromdate", date.Date);
            param.Add("locationid", locationid);
            param.Add("counterid", counterid);
            param.Add("isalteration", false);
            param.Add("todate", date.AddDays(1).Date);
            using (var reader = ExecuteReader(@"select count(*) from delivery where receivedon>=@fromdate
                and receivedon<@todate and status=0 and counterid=@counterid and locationid=@locationid and isalteration=@isalteration",
                param))
            {

                if (reader.Read())
                    return reader.GetInt32(0);
            }
            return 0;
        }

        public int GetReceived(DateTime date, long locationid, long counterid, bool isalteration)
        {
            var param = new SortedList<string, object>();
            param.Add("fromdate", date.Date);
            param.Add("locationid", locationid);
            param.Add("counterid", counterid);
            param.Add("isalteration", isalteration);
            param.Add("todate", date.AddDays(1).Date);
            using (var reader = ExecuteReader(@"select count(*) from delivery where receivedon>=@fromdate
                and receivedon<@todate and status=0 and counterid=@counterid and locationid=@locationid and isalteration=@isalteration",
                param))
            {

                if (reader.Read())
                    return reader.GetInt32(0);
            }
            return 0;
        }
        public Delivery Insert(NpgsqlTransaction transaction, NpgsqlConnection conn, Delivery oDelivery)
        {
            if (oDelivery.isalteration)
            {
                var sequence = SequenceManager.Instance(Context.OrganizationID).Get("delivery.altationno", oDelivery.locationid);
                oDelivery.altationno = sequence.GetNextNumber(Context, conn, transaction);
            }
            if (oDelivery.isgift)
            {
                var sequence = SequenceManager.Instance(Context.OrganizationID).Get("delivery.giftno", oDelivery.locationid);
                oDelivery.altationno = sequence.GetNextNumber(Context, conn, transaction);
                InfyPOS.Processors.RandomManager randomManager = new Processors.RandomManager(8, Processors.RandomManager.Type.AlphaNumeric, "altationno", "delivery");
                oDelivery.altationno += "-" + randomManager.Generate();
            }
            if (oDelivery.isgiftcoupon)
            {
                var sequence = SequenceManager.Instance(Context.OrganizationID).Get("delivery.giftcouponno", oDelivery.locationid);
                oDelivery.altationno = sequence.GetNextNumber(Context, conn, transaction);
                InfyPOS.Processors.RandomManager randomManager = new Processors.RandomManager(8, Processors.RandomManager.Type.AlphaNumeric, "altationno", "delivery");
                oDelivery.altationno += "-" + randomManager.Generate();
            }
            NpgsqlCommand command = GetCommand(SQL_Insert, conn, transaction);
            command.Parameters.Add("isactive", NpgsqlTypes.NpgsqlDbType.Boolean).Value = oDelivery.isactive;
            command.Parameters.Add("createdby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = Context.UserID;
            command.Parameters.Add("modifiedby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = Context.UserID;
            command.Parameters.Add("isalteration", NpgsqlTypes.NpgsqlDbType.Boolean).Value = oDelivery.isalteration;
            command.Parameters.Add("isgift", NpgsqlTypes.NpgsqlDbType.Boolean).Value = oDelivery.isgift;
            command.Parameters.Add("isgiftcoupon", NpgsqlTypes.NpgsqlDbType.Boolean).Value = oDelivery.isgiftcoupon;
            command.Parameters.Add("version", NpgsqlTypes.NpgsqlDbType.Integer).Value = oDelivery.version;
            command.Parameters.Add("createdon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = DateTimeNow;
            command.Parameters.Add("modifiedon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = DateTimeNow;
            command.Parameters.Add("billid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oDelivery.billid;
            command.Parameters.Add("locationid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oDelivery.locationid;
            command.Parameters.Add("counterid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oDelivery.counterid;
            command.Parameters.Add("validatedby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oDelivery.validatedby;
            command.Parameters.Add("amount", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oDelivery.amount;
            command.Parameters.Add("altationno", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oDelivery.altationno == null ? "" : oDelivery.altationno;
            command.Parameters.Add("billno", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oDelivery.billno == null ? "" : oDelivery.billno;
            command.Parameters.Add("customername", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oDelivery.customername == null ? "" : oDelivery.customername;
            command.Parameters.Add("contactno", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oDelivery.contactno == null ? "" : oDelivery.contactno;
            command.Parameters.Add("billinfo", NpgsqlTypes.NpgsqlDbType.Json).Value = oDelivery.billinfo == null ? "null" : oDelivery.billinfo_json;
            command.Parameters.Add("voucherinfo", NpgsqlTypes.NpgsqlDbType.Json).Value = oDelivery.voucherinfo == null ? "null" : oDelivery.voucherinfo_json;
            command.Parameters.Add("giftlist", NpgsqlTypes.NpgsqlDbType.Json).Value = oDelivery.giftlist == null ? "null" : oDelivery.giftlist_json;
            command.Parameters.Add("remarks", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oDelivery.remarks == null ? "" : oDelivery.remarks;

            command.Parameters.Add("receivedby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oDelivery.receivedby;
            command.Parameters.Add("deliveredby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oDelivery.deliveredby;
            command.Parameters.Add("returnto", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oDelivery.returnto;
            command.Parameters.Add("returnby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oDelivery.returnby;
            command.Parameters.Add("cancelledby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oDelivery.cancelledby;
            command.Parameters.Add("qty", NpgsqlTypes.NpgsqlDbType.Integer).Value = oDelivery.qty;
            command.Parameters.Add("receivedon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = oDelivery.receivedon;
            command.Parameters.Add("deliveredon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = oDelivery.deliveredon;
            command.Parameters.Add("status", NpgsqlTypes.NpgsqlDbType.Integer).Value = oDelivery.status;
            using (System.Data.IDataReader reader = command.ExecuteReader())
            {
                if (reader.Read())
                {
                    oDelivery.id = Convert.ToInt64(reader[0].ToString());
                }
            }
            return oDelivery;
        }

        internal Delivery Cancel(Delivery item)
        {
            BillDataService billdataservice = new BillDataService() { Context = this.Context };
            billdataservice.Context = this.Context;
            var ids = new List<long>();
            ids.Add(item.billid);
            billdataservice.Cancellation(new ActionRequest<BillCancel>()
            {
                Item = new BillCancel()
                {
                    locationid = item.locationid,
                    ids = ids
                }
            });
            item.cancelledby = Context.UserID;
            item.deliveredon = DateTimeNow;
            item.Deliverystatus = Delivery.DeliveryStatus.Cancelled;
            item.modifiedby = Context.UserID;
            item.modifiedon = DateTimeNow;
            return Update(item);
        }

        public Delivery Update(Delivery oDelivery)
        {

            NpgsqlTransaction transaction = null;
            try
            {
                using (NpgsqlConnection conn = CreateConnection())
                {
                    conn.Open();
                    transaction = conn.BeginTransaction();
                    if (oDelivery.billinfo != null &&
                        oDelivery.billinfo.items != null)
                    {
                        BillDataService billDataService = new BillDataService()
                        {
                            Context = this.Context
                        };
                        var bills = billDataService.Search(new BillCreteria()
                        {
                            billno = oDelivery.billinfo.items[0].billno,
                            isimport = true
                        });
                        if (bills.Count == 1)
                        {
                            if (oDelivery.giftlist != null)
                            {
                                var missinggf = new List<Delivery.GiftItem>();
                                foreach (var gf in oDelivery.giftlist)
                                {
                                    if (!bills[0].Billitems.Exists(x => x.salablegoodsid == gf.salablegoodsid))
                                    {
                                        missinggf.Add(gf);
                                    }
                                }
                                if (missinggf.Count > 0)
                                {
                                    SalablegoodsDataService salablegoodsDataService = new SalablegoodsDataService()
                                    {
                                        Context = this.Context
                                    };
                                    var sbarcodes = salablegoodsDataService.Search(new SalablegoodsCreteria()
                                    {
                                        ids = missinggf.ConvertAll(x => x.salablegoodsid)
                                    });
                                    var billitems = missinggf.ConvertAll(x => new Billitems()
                                    {
                                        barcode = x.barcode,
                                        salablegoodsid = x.salablegoodsid,
                                        qty = x.qty,
                                        isactive = true,
                                        stockid = sbarcodes.Find(y => y.id == x.salablegoodsid).stockid,
                                        taxid = sbarcodes.Find(y => y.id == x.salablegoodsid).saletaxid,
                                        taxpercentage = 0,
                                        billid = bills[0].id,
                                        locationid = bills[0].locationid,
                                        createdon = DateTimeNow,
                                        createdby = Context.UserID,
                                        modifiedby = Context.UserID,
                                        modifiedon = DateTimeNow,
                                        version = 1
                                    });
                                    BillitemsDataService billitemsDataService = new BillitemsDataService()
                                    {
                                        Context = this.Context
                                    };
                                    foreach (var billitem in billitems)
                                    {
                                        billitemsDataService.Insert(transaction,
                                            conn, bills[0], billitem, bills[0].locationid);
                                    }
                                }
                            }
                        }
                    }
                    Update(transaction, conn, oDelivery);
                    transaction.Commit();
                    conn.Close();
                }
            }
            catch (Exception exp)
            {
                WebAPI.Common.Logger.Instance(Context.OrganizationID).Log(exp, WebAPI.Common.Logger.LogType.Error, oDelivery);
            }

            return oDelivery;
        }

        public Delivery Update(NpgsqlTransaction transaction, NpgsqlConnection conn, Delivery oDelivery)
        {
            NpgsqlCommand command = GetCommand(SQL_Update, conn, transaction);
            command.Parameters.Add("id", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oDelivery.id;
            command.Parameters.Add("isactive", NpgsqlTypes.NpgsqlDbType.Boolean).Value = oDelivery.isactive;
            command.Parameters.Add("createdby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oDelivery.createdby;
            command.Parameters.Add("modifiedby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = Context.UserID;
            command.Parameters.Add("version", NpgsqlTypes.NpgsqlDbType.Integer).Value = oDelivery.version;
            command.Parameters.Add("createdon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = oDelivery.createdon;
            command.Parameters.Add("modifiedon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = DateTimeNow;
            command.Parameters.Add("billid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oDelivery.billid;
            command.Parameters.Add("locationid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oDelivery.locationid;
            command.Parameters.Add("counterid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oDelivery.counterid;
            command.Parameters.Add("validatedby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oDelivery.validatedby;
            command.Parameters.Add("amount", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oDelivery.amount;
            command.Parameters.Add("billno", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oDelivery.billno == null ? "" : oDelivery.billno;
            command.Parameters.Add("receivedby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oDelivery.receivedby;
            command.Parameters.Add("deliveredby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oDelivery.deliveredby;
            command.Parameters.Add("returnby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oDelivery.returnby;
            command.Parameters.Add("cancelledby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oDelivery.cancelledby;
            command.Parameters.Add("qty", NpgsqlTypes.NpgsqlDbType.Integer).Value = oDelivery.qty;
            command.Parameters.Add("returnto", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oDelivery.returnto;
            command.Parameters.Add("receivedon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = oDelivery.receivedon;
            command.Parameters.Add("deliveredon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = oDelivery.deliveredon;
            command.Parameters.Add("status", NpgsqlTypes.NpgsqlDbType.Integer).Value = oDelivery.status;
            command.Parameters.Add("giftlist", NpgsqlTypes.NpgsqlDbType.Json).Value = oDelivery.giftlist == null ? "null" : oDelivery.giftlist_json;
            command.Parameters.Add("customername", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oDelivery.customername == null ? "" : oDelivery.customername;
            command.Parameters.Add("contactno", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oDelivery.contactno == null ? "" : oDelivery.contactno;
            command.Parameters.Add("billinfo", NpgsqlTypes.NpgsqlDbType.Json).Value = oDelivery.billinfo == null ? "null" : oDelivery.billinfo_json;
            command.Parameters.Add("remarks", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oDelivery.remarks == null ? "" : oDelivery.remarks;

            using (System.Data.IDataReader reader = command.ExecuteReader())
            {
                if (reader.Read())
                {
                    oDelivery.version = Convert.ToInt32(reader[0].ToString());
                }
            }

            return oDelivery;
        }

        internal List<BillsettlementGift> GetSettlementGiftList(Billsettlementmaster source)
        {
            List<BillsettlementGift> response = new List<BillsettlementGift>();
            var billvalues = Search(new DeliveryCreteria()
            {
                billids = source.Billsettlement.ConvertAll(x => x.billid).ToList()
            });

            ReferencelistDataService referencelistDataService = new ReferencelistDataService() { Context = this.Context };
            var markerlist = referencelistDataService.Search(new ReferencelistCreteria()
            {
                type = "MARKER"
            }).FindAll(x => x.additionalinfo != null && x.additionalinfo.issales).OrderByDescending(x => x.additionalinfo.billdiscount).ToList();

            var value = billvalues.Sum(x => x.billinfo == null ? 0 : x.billinfo.amount);
            for (var i = 0; i < markerlist.Count; i++)
            {
                if (markerlist[i].additionalinfo.billdiscount < value &&
                    markerlist[i].additionalinfo.billdiscount != 0)
                {
                    var nextvalue = value % markerlist[i].additionalinfo.billdiscount;
                    if ((value - nextvalue) != 0)
                    {
                        var noof = (value - nextvalue) / markerlist[i].additionalinfo.billdiscount;

                        response.Add(new BillsettlementGift()
                        {
                            amount = markerlist[i].additionalinfo.billdiscount,
                            code = markerlist[i].code,
                            name = markerlist[i].name,
                            qty = noof
                        });
                        value = nextvalue;
                    }
                }
            }
            return response;
        }

        internal List<Delivery> UpdateAll(List<Delivery> items)
        {
            NpgsqlTransaction transaction = null;
            try
            {
                using (NpgsqlConnection conn = CreateConnection())
                {
                    conn.Open();
                    transaction = conn.BeginTransaction();
                    foreach (var item in items)
                    {
                        if (item.Deliverystatus == Delivery.DeliveryStatus.Cancelled)
                        {
                            List<long> ids = new List<long>();
                            ids.Add(item.billid);
                            BillDataService billdata = new BillDataService() { Context = this.Context };
                            billdata.Context = this.Context;
                            billdata.Cancellation(new ActionRequest<BillCancel>()
                            {
                                Item = new BillCancel()
                                {
                                    locationid = item.locationid,
                                    ids = ids
                                }
                            });
                            item.deliveredby = 0;
                            item.cancelledby = Context.UserID;
                            item.deliveredon = DateTimeNow;
                            item.isactive = false;
                        }
                        else if (item.Deliverystatus == Delivery.DeliveryStatus.Returned)
                        {
                            item.deliveredby = 0;
                            item.returnby = Context.UserID;
                            item.deliveredon = DateTimeNow;
                            item.isactive = true;
                        }
                        else if (item.Deliverystatus == Delivery.DeliveryStatus.Delivered)
                        {
                            item.deliveredby = Context.UserID;
                            item.deliveredon = DateTimeNow;
                        }
                        else if (item.Deliverystatus == Delivery.DeliveryStatus.Validated)
                        {
                            item.validatedby = Context.UserID;
                        }
                        else
                        {
                            item.receivedby = Context.UserID;
                            item.receivedon = DateTimeNow;
                            item.deliveredby = 0;
                            item.deliveredon = DateTime.MinValue;
                        }
                        item.modifiedby = Context.UserID;
                        item.modifiedon = DateTimeNow;
                        Update(transaction, conn, item);
                    }
                    transaction.Commit();
                    conn.Close();
                }
            }
            catch (Exception exp)
            {
                WebAPI.Common.Logger.Instance(Context.OrganizationID).Log(exp, WebAPI.Common.Logger.LogType.Error, items);
            }
            return items;
        }

        public bool Delete(Delivery oDelivery)
        {

            NpgsqlTransaction transaction = null;
            bool hasSuccessCount = false;
            try
            {
                using (NpgsqlConnection conn = CreateConnection())
                {
                    conn.Open();
                    transaction = conn.BeginTransaction();
                    hasSuccessCount = Delete(transaction, conn, oDelivery);
                    transaction.Commit();
                    conn.Close();
                }
            }
            catch (Exception exp)
            {
                WebAPI.Common.Logger.Instance(Context.OrganizationID).Log(exp, WebAPI.Common.Logger.LogType.Error, oDelivery);
            }
            return hasSuccessCount;

        }

        public bool Delete(NpgsqlTransaction transaction, NpgsqlConnection conn, Delivery oDelivery)
        {

            NpgsqlCommand command = GetCommand(SQL_Delete, conn, transaction);
            command.Parameters.Add("isactive", NpgsqlTypes.NpgsqlDbType.Boolean).Value = oDelivery.isactive;
            command.Parameters.Add("modifiedby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = Context.UserID;
            command.Parameters.Add("modifiedon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = DateTimeNow;
            command.Parameters.Add("version", NpgsqlTypes.NpgsqlDbType.Integer).Value = oDelivery.version;
            command.Parameters.Add("id", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oDelivery.id;
            int successCount = command.ExecuteNonQuery();

            return successCount > 0;
        }

        public List<DeliveryGiftSummary> GiftIssueSummary(DeliverySearch _delivery)
        {


            List<DeliveryGiftSummary> lst = new List<DeliveryGiftSummary>();
            NpgsqlConnection conn = CreateConnection();
            try
            {
                conn.Open();

                NpgsqlCommand command = null;

                QueryBuilder qb = new QueryBuilder();

                if (_delivery.mode == 3)
                {
                    qb.CurrentQuery = @"select referencelist.name categoryname,
sum(cast(glist->>'qty' as numeric)) voucherqty,receivedon::date receivedon,
sum(case when cast(glist->>'issued' as boolean) = true then cast(glist->>'qty' as numeric) else 0 end) issueqty
from delivery
LEFT JOIN LATERAL
json_array_elements (cast(delivery.giftlist as json)) glist ON true
LEFT OUTER JOIN referencelist on referencelist.id = cast(glist->>'stockmarkerid' as bigint)
 where delivery.isactive and delivery.isgift=true
";
                }
                else if (_delivery.mode == 4)
                {
                    qb.CurrentQuery = @"
 select sum(cast(glist->>'qty' as numeric)) voucherqty,
referencelist.name categoryname,glist->>'barcode' barcode,products.name giftproduct,
receivedon::date receivedon
from delivery
LEFT JOIN LATERAL
json_array_elements (cast(delivery.giftlist as json)) glist ON true
LEFT OUTER JOIN referencelist on referencelist.id = cast(glist->>'stockmarkerid' as bigint)
LEFT OUTER JOIN stock on stock.barcode = glist->>'barcode'
LEFT OUTER JOIN products on products.id = stock.productid
 where delivery.isactive and delivery.isgift=true and cast(glist->>'issued' as boolean)=true
";
                }

                if (_delivery.fromdate.Year > 2000)
                {
                    qb.AddParameter("receivedon", "fromreceivedon", _delivery.fromdate.Date, ">=");
                    qb.AddParameter("receivedon", "toreceivedon", _delivery.todate.Date.AddDays(1), "<");
                }

                if (_delivery.locationid > 0)
                    qb.AddParameter("delivery.locationid", _delivery.locationid, "=");

                if (_delivery.counterid > 0)
                    qb.AddParameter("delivery.counterid", _delivery.counterid, "=");

                qb.AddParameter("isalteration", false, "=");

                if (_delivery.mode == 3)
                {
                    qb.CurrentQuery += @" group by referencelist.name,receivedon::date";
                }
                else if (_delivery.mode == 4)
                {
                    qb.CurrentQuery += @" group by referencelist.name,glist->>'barcode',products.name,receivedon::date";
                }

                command = qb.GetCommand(conn);
                using (System.Data.IDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        DeliveryGiftSummary odelivery = new DeliveryGiftSummary();
                        odelivery.categoryname = reader["categoryname"] == DBNull.Value ? "" : reader["categoryname"].ToString();
                        odelivery.receivedon = reader["receivedon"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(reader["receivedon"]);

                        if (_delivery.mode == 3)
                        {
                            odelivery.voucherqty = reader["voucherqty"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["voucherqty"]);
                            odelivery.issueqty = reader["issueqty"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["issueqty"]);
                        }
                        else
                        {
                            odelivery.voucherqty = reader["voucherqty"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["voucherqty"]);
                            odelivery.barcode = reader["barcode"] == DBNull.Value ? "" : reader["barcode"].ToString();
                            odelivery.giftproduct = reader["giftproduct"] == DBNull.Value ? "" : reader["giftproduct"].ToString();
                        }
                        lst.Add(odelivery);
                    }
                }
                conn.Close();
            }
            catch (Exception exp)
            {
                if (conn != null) conn.Close();
                WebAPI.Common.Logger.Instance(Context.OrganizationID).Log(exp, WebAPI.Common.Logger.LogType.Error, _delivery);
            }
            return lst;
        }
        public List<DelieryGiftItem> GiftIssueReport(DeliverySearch _delivery)
        {


            List<DelieryGiftItem> lst = new List<DelieryGiftItem>();
            NpgsqlConnection conn = CreateConnection();
            try
            {
                conn.Open();

                NpgsqlCommand command = null;

                QueryBuilder qb = new QueryBuilder();
                qb.CurrentQuery = @"
select altationno voucherno,billlist->>'amount' billamount,
billlist->>'billno' billno,cast(glist->>'qty' as numeric) giftqty,
billlist->>'settlementcode' settlementcode,billlist->>'settlementon' settlementon,
referencelist.name categoryname,glist->>'barcode' barcode,products.name giftproduct,
cast(glist->>'value' as numeric) giftvalue,cast(glist->>'issued' as boolean) hasissued,counter.name counter,
deliveredon,receivedon,employee.name voucheredby,employee.name issuedby from delivery
LEFT JOIN LATERAL
json_array_elements (cast(delivery.giftlist as json)) glist ON true
LEFT JOIN LATERAL
json_array_elements (cast(delivery.billinfo->>'items' as json)) billlist ON true
LEFT OUTER JOIN referencelist on referencelist.id = cast(glist->>'stockmarkerid' as bigint)
LEFT OUTER JOIN referencelist counter on counter.id = delivery.counterid
LEFT OUTER JOIN users on users.id = delivery.createdby
LEFT OUTER JOIN employee on employee.id = users.employeeid
LEFT OUTER JOIN users dusers on dusers.id = delivery.deliveredby
LEFT OUTER JOIN employee demployee on demployee.id = dusers.employeeid
LEFT OUTER JOIN stock on stock.barcode = glist->>'barcode'
LEFT OUTER JOIN products on products.id = stock.productid
 where delivery.isactive and delivery.isgift=true
";

                if (_delivery.mode != 0)
                {
                    qb.CurrentQuery = @"
with settlements
as
(
    select delivery.id,string_agg(distinct billlist->>'settlementcode',',') settlementcode,
    min(billlist->>'settlementon') settlementon
    from delivery
    LEFT JOIN LATERAL
    json_array_elements (cast(delivery.billinfo->>'items' as json)) billlist ON true
    where delivery.isactive and delivery.isgift=true and receivedon>=@fromdate and receivedon<@todate
    group by delivery.id
)

select altationno voucherno,cast(glist->>'qty' as numeric) giftqty,settlements.settlementcode,
referencelist.name categoryname,glist->>'barcode' barcode,products.name giftproduct,settlements.settlementon,
cast(glist->>'value' as numeric) giftvalue,cast(glist->>'issued' as boolean) hasissued,counter.name counter,
deliveredon,receivedon,employee.name voucheredby,employee.name issuedby from delivery
LEFT JOIN LATERAL
json_array_elements (cast(delivery.giftlist as json)) glist ON true
LEFT OUTER JOIN settlements ON settlements.ID = delivery.id
LEFT OUTER JOIN referencelist on referencelist.id = cast(glist->>'stockmarkerid' as bigint)
LEFT OUTER JOIN referencelist counter on counter.id = delivery.counterid
LEFT OUTER JOIN users on users.id = delivery.createdby
LEFT OUTER JOIN employee on employee.id = users.employeeid
LEFT OUTER JOIN users dusers on dusers.id = delivery.deliveredby
LEFT OUTER JOIN employee demployee on demployee.id = dusers.employeeid
LEFT OUTER JOIN stock on stock.barcode = glist->>'barcode'
LEFT OUTER JOIN products on products.id = stock.productid
 where delivery.isactive and delivery.isgift=true and receivedon>=@fromdate and receivedon<@todate
";
                }



                if (_delivery.mode == 1)
                {
                    qb.AddParameter("cast(glist->> 'issued' as boolean)", "issued", true, "=");
                }
                else if (_delivery.mode == 2)
                {
                    qb.AddParameter("cast(glist->> 'issued' as boolean)", "issued", false, "=");
                }

                qb.AddParameter("receivedon", "fromreceivedon", _delivery.fromdate.Date, ">=");
                qb.AddParameter("receivedon", "toreceivedon", _delivery.todate.Date.AddDays(1), "<");

                if (!string.IsNullOrEmpty(_delivery.voucherno))
                    qb.AddParameter("altationno", _delivery.voucherno, "=");

                if (!string.IsNullOrEmpty(_delivery.billno))
                    qb.AddParameter("billlist->>'billno'", _delivery.billno, "=");

                if (_delivery.locationid > 0)
                    qb.AddParameter("delivery.locationid", _delivery.locationid, "=");

                if (_delivery.counterid > 0)
                    qb.AddParameter("delivery.counterid", _delivery.counterid, "=");

                qb.AddParameter("isalteration", false, "=");
                command = qb.GetCommand(conn);
                command.Parameters.Add("fromdate", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = _delivery.fromdate.Date;
                command.Parameters.Add("todate", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = _delivery.todate.AddDays(1).Date;
                using (System.Data.IDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        DelieryGiftItem odelivery = new DelieryGiftItem();
                        odelivery.giftvalue = reader["giftvalue"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["giftvalue"]);
                        odelivery.giftqty = reader["giftqty"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["giftqty"]);
                        odelivery.issued = reader["hasissued"] == DBNull.Value ? false : Convert.ToBoolean(reader["hasissued"]);
                        if (_delivery.mode == 0)
                        {
                            odelivery.billamount = reader["billamount"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["billamount"]);
                            odelivery.billno = reader["billno"] == DBNull.Value ? "" : reader["billno"].ToString();
                        }
                        odelivery.barcode = reader["barcode"] == DBNull.Value ? "" : reader["barcode"].ToString();
                        odelivery.settlementcode = reader["settlementcode"] == DBNull.Value ? "" : reader["settlementcode"].ToString();
                        odelivery.voucherno = reader["voucherno"] == DBNull.Value ? "" : reader["voucherno"].ToString();
                        odelivery.counter = reader["counter"] == DBNull.Value ? "" : reader["counter"].ToString();
                        odelivery.categoryname = reader["categoryname"] == DBNull.Value ? "" : reader["categoryname"].ToString();
                        odelivery.giftproduct = reader["giftproduct"] == DBNull.Value ? "" : reader["giftproduct"].ToString();
                        odelivery.issuedby = reader["issuedby"] == DBNull.Value ? "" : reader["issuedby"].ToString();
                        odelivery.voucheredby = reader["voucheredby"] == DBNull.Value ? "" : reader["voucheredby"].ToString();
                        odelivery.settlementon = reader["settlementon"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(reader["settlementon"]);
                        odelivery.receivedon = reader["receivedon"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(reader["receivedon"]);
                        odelivery.deliveredon = reader["deliveredon"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(reader["deliveredon"]);
                        lst.Add(odelivery);
                    }
                }
                conn.Close();
            }
            catch (Exception exp)
            {
                if (conn != null) conn.Close();
                WebAPI.Common.Logger.Instance(Context.OrganizationID).Log(exp, WebAPI.Common.Logger.LogType.Error, _delivery);
            }
            return lst;
        }
        internal Delivery UpdateGiftDelivery(Delivery oDelivery)
        {
            oDelivery.deliveredby = Context.UserID;
            oDelivery.deliveredon = DateTimeNow;
            return Update(oDelivery);
        }

        internal Delivery SaveGiftCoupon(Delivery oDelivery)
        {
            NpgsqlTransaction transaction = null;
            try
            {
                using (NpgsqlConnection conn = CreateConnection())
                {
                    conn.Open();
                    transaction = conn.BeginTransaction();
                    long locationid = 0;
                    long companyid = 0;
                    long customerid = 0;
                    oDelivery.billinfo = new Delivery.ReceivedItem();
                    oDelivery.billinfo.items = new List<Delivery.ReceivedItem.Item>();
                    oDelivery.createdby = Context.UserID;
                    oDelivery.createdon = DateTimeNow;

                    foreach (var item in oDelivery.bills)
                    {
                        BillAttribute billAttribute = null;
                        var param = new SortedList<string, object>();
                        param.Add("id", item.billid);
                        using (var reader = ExecuteReader("select billattributes,locationid,companyid,customerid from bill where id=@id", param, conn, transaction))
                        {
                            if (reader.Read() && !reader.IsDBNull(0))
                            {
                                locationid = reader.GetInt64(1);
                                companyid = reader.GetInt64(2);
                                customerid = reader.GetInt64(3);
                                if (reader[0].ToString() != "null")
                                    billAttribute = Newtonsoft.Json.JsonConvert.DeserializeObject<BillAttribute>(reader[0].ToString());
                            }
                        }
                        if (billAttribute != null && billAttribute.giftcoupon)
                        {
                            transaction.Rollback();
                            throw new Exception(item.billno + " Already gift coupon issued");
                        }

                        if (billAttribute != null && billAttribute.gift)
                        {
                            transaction.Rollback();
                            throw new Exception(item.billno + " Already gift issued");
                        }

                        var command = GetCommand("update bill set billattributes=@billattributes where id=@id and isactive=true and billattributes->>'giftcoupon' = 'false'", conn, transaction);
                        if (billAttribute == null) billAttribute = new BillAttribute();
                        billAttribute.giftcoupon = true;
                        command.Parameters.Add("id", NpgsqlTypes.NpgsqlDbType.Bigint).Value = item.billid;
                        command.Parameters.Add("billattributes", NpgsqlTypes.NpgsqlDbType.Json).Value = billAttribute == null ? "null" : Newtonsoft.Json.JsonConvert.SerializeObject(billAttribute);
                        if(command.ExecuteNonQuery() == 0)
                        {
                            transaction.Rollback();
                            throw new Exception(item.billno + " Already gift coupon issued");
                        }
                        oDelivery.billinfo.items.Add(new Delivery.ReceivedItem.Item()
                        {
                            billno = item.billno,
                            amount = item.amount,
                            settlementcode = item.billinfo.settlementcode,
                            settlementon = item.billinfo.settlementon,
                            excluded = item.excluded,
                            receivable = item.amount + item.excluded
                        });
                    }

                    oDelivery.billno = oDelivery.bills.First().billno;
                    oDelivery.billid = oDelivery.bills.First().billid;
                    oDelivery.amount = oDelivery.bills.Sum(x => x.amount);
                    oDelivery.isgiftcoupon = true;

                    if (oDelivery.voucherinfo != null && oDelivery.voucherinfo.amount > 0)
                    {
                        oDelivery.customerid = customerid;
                        oDelivery.companyid = companyid;
                        if (oDelivery.voucherinfo.fromdays > 0)
                        {
                            oDelivery.voucherinfo.validfrom = DateTimeNow.Date.AddDays(oDelivery.voucherinfo.fromdays);
                        }
                        if (oDelivery.voucherinfo.expirydays > 0)
                        {
                            oDelivery.voucherinfo.expiry = DateTimeNow.Date.AddDays(oDelivery.voucherinfo.expirydays);
                        }
                        CustomerCouponsDataService voucherdata = new CustomerCouponsDataService() { Context = this.Context };
                        var voucher = new CustomerCoupons()
                        {
                            issuedby = Context.UserID,
                            issueto = customerid,
                            amount = oDelivery.voucherinfo.amount,
                            isactive = true,
                            vouchertype = CustomerCoupons.CouponType.Coupon,
                            issuedon = DateTimeNow,
                            validfrom = oDelivery.voucherinfo.validfrom,
                            validupto = oDelivery.voucherinfo.expiry,
                        };
                        voucherdata.Insert(transaction, conn, voucher);
                        oDelivery.voucherinfo.couponcode = voucher.code;
                    }
                    Insert(transaction, conn, oDelivery);
                    transaction.Commit();
                    conn.Close();
                }
            }
            catch (Exception exp)
            {
                WebAPI.Common.Logger.Instance(Context.OrganizationID).Log(exp, WebAPI.Common.Logger.LogType.Error, oDelivery);
            }
            return oDelivery;
        }
        internal Delivery SaveGiftDelivery(Delivery oDelivery)
        {
            if (oDelivery.id != 0)
            {
                return UpdateGiftDelivery(oDelivery);
            }

            NpgsqlTransaction transaction = null;
            try
            {
                using (NpgsqlConnection conn = CreateConnection())
                {
                    conn.Open();
                    transaction = conn.BeginTransaction();
                    long locationid = 0;
                    long companyid = 0;
                    oDelivery.billinfo = new Delivery.ReceivedItem();
                    oDelivery.billinfo.items = new List<Delivery.ReceivedItem.Item>();
                    oDelivery.createdby = Context.UserID;
                    oDelivery.createdon = DateTimeNow;
                    foreach (var item in oDelivery.bills)
                    {
                        BillAttribute billAttribute = null;
                        var param = new SortedList<string, object>();
                        param.Add("id", item.billid);
                        using (var reader = ExecuteReader("select billattributes,locationid,companyid from bill where id=@id", param, conn, transaction))
                        {
                            if (reader.Read() && !reader.IsDBNull(0))
                            {
                                locationid = reader.GetInt64(1);
                                companyid = reader.GetInt64(2);
                                if (reader[0].ToString() != "null")
                                    billAttribute = Newtonsoft.Json.JsonConvert.DeserializeObject<BillAttribute>(reader[0].ToString());
                            }
                        }
                        if (billAttribute != null && billAttribute.gift)
                        {
                            transaction.Rollback();
                            throw new Exception(item.billno + " Already gift issued");
                        }
                        if (billAttribute != null && billAttribute.giftcoupon)
                        {
                            transaction.Rollback();
                            throw new Exception(item.billno + " Already gift coupon issued");
                        }
                        var command = GetCommand("update bill set billattributes=@billattributes where id=@id and isactive=true and billattributes->>'gift'='false'", conn, transaction);
                        if (billAttribute == null) billAttribute = new BillAttribute();
                        billAttribute.gift = true;
                        command.Parameters.Add("id", NpgsqlTypes.NpgsqlDbType.Bigint).Value = item.billid;
                        command.Parameters.Add("billattributes", NpgsqlTypes.NpgsqlDbType.Json).Value = billAttribute == null ? "null" : Newtonsoft.Json.JsonConvert.SerializeObject(billAttribute);
                        if(command.ExecuteNonQuery() == 0)
                        {
                            transaction.Rollback();
                            throw new Exception(item.billno + " Already gift issued");
                        }
                        oDelivery.billinfo.items.Add(new Delivery.ReceivedItem.Item()
                        {
                            billno = item.billno,
                            amount = item.amount,
                            settlementcode = item.billinfo.settlementcode,
                            settlementon = item.billinfo.settlementon,
                            excluded = item.excluded,
                            receivable = item.amount + item.excluded
                        });
                    }

                    
                    oDelivery.billno = oDelivery.bills.First().billno;
                    oDelivery.billid = oDelivery.bills.First().billid;
                    oDelivery.amount = oDelivery.bills.Sum(x => x.amount);
                    oDelivery.isgift = true;
                    oDelivery.isalteration = false;
                    if (oDelivery.giftitems != null)
                    {
                        foreach (var gitem in oDelivery.giftitems)
                        {
                            oDelivery.billinfo.items.Add(new Delivery.ReceivedItem.Item()
                            {
                                barcode = gitem.barcode,
                                giftmarkerid = gitem.stockmarkerid,
                                printingname = gitem.name,
                                qty = gitem.qty,
                                price = gitem.value,
                                billno = oDelivery.bills.OrderBy(e => e.amount).Last().billno,
                                billid = oDelivery.bills.OrderBy(e => e.amount).Last().billid,
                                salablegoodsid = gitem.salablegoodsid,
                                receivable = gitem.qty * gitem.value
                            });
                        }
                    }

                    if (oDelivery.voucherinfo != null && oDelivery.voucherinfo.amount > 0)
                    {
                        CustomerCouponsDataService voucherdata = new CustomerCouponsDataService() { Context = this.Context };
                        var voucher = new CustomerCoupons()
                        {
                            issuedby = Context.UserID,
                            issueto = 0,
                            amount = oDelivery.voucherinfo.amount,
                            isactive = true,
                            vouchertype = CustomerCoupons.CouponType.Coupon,
                            issuedon = DateTimeNow,
                            validupto = DateTime.MaxValue,
                            validfrom = oDelivery.voucherinfo.validfrom
                        };
                        voucherdata.Insert(transaction, conn, voucher);
                        oDelivery.voucherinfo.couponcode = voucher.code;
                    }

                    if (oDelivery.giftlist != null && oDelivery.giftlist.Count > 0)
                    {
                        oDelivery.qty = oDelivery.giftlist.Sum(x => x.qty);
                    }

                    Insert(transaction, conn, oDelivery);
                    if (oDelivery.giftitems != null && oDelivery.giftitems.Count > 0)
                    {

                        if (!Services.ConfigManager.Instance(Context.OrganizationID).LocationConfig[oDelivery.locationid].gft_disablebillitem)
                        {
                            BillitemsDataService billitemsDataService = new BillitemsDataService() { Context = this.Context };
                            var giftitembillid = oDelivery.bills.OrderBy(e => e.amount).Last().billid;
                            foreach (var gitem in oDelivery.giftitems)
                            {
                                Bill bill = new Bill() { locationid = locationid, updatestock = true, companyid = companyid };
                                billitemsDataService.Insert(transaction, conn, bill, new Billitems()
                                {
                                    isactive = true,
                                    salablegoodsid = gitem.salablegoodsid,
                                    qty = gitem.qty,
                                    barcode = gitem.barcode,
                                    printingname = gitem.name,
                                    billid = giftitembillid,
                                    createdby = Context.UserID,
                                    createdon = DateTimeNow,
                                    modifiedby = Context.UserID,
                                    modifiedon = DateTimeNow,
                                    companyid = companyid,
                                    locationid = locationid,
                                    stockid = gitem.stockid,
                                    productid = gitem.productid,
                                    taxid = gitem.salestaxid,
                                    taxpercentage = gitem.taxpercentage,
                                    version = 1
                                }, locationid);
                            }
                        }else if(Services.ConfigManager.Instance(Context.OrganizationID).LocationConfig[oDelivery.locationid].gft_stockreduce)
                        {
                            const string SQL_UpdateStock = @"update salablegoods set oqty=oqty+@qty where id=@id and stockid=@stockid RETURNING iqty,oqty,dqty,jqty,olqty";
                            var command = GetCommand(SQL_UpdateStock, conn, transaction);
                            var giftitembillid = oDelivery.bills.OrderBy(e => e.amount).Last().billid;
                            foreach (var gitem in oDelivery.giftitems)
                            {
                                command.Parameters.Add("stockid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = gitem.stockid;
                                command.Parameters.Add("id", NpgsqlTypes.NpgsqlDbType.Bigint).Value = gitem.salablegoodsid;
                                command.Parameters.Add("qty", NpgsqlTypes.NpgsqlDbType.Numeric).Value = gitem.qty;
                                decimal shortagestock = 0;
                                using (var reader = command.ExecuteReader())
                                {
                                    if (reader.Read())
                                    {
                                        var iqty = reader["iqty"] == DBNull.Value ? 0 : (decimal)reader["iqty"];
                                        var oqty = reader["oqty"] == DBNull.Value ? 0 : (decimal)reader["oqty"];
                                        var dqty = reader["dqty"] == DBNull.Value ? 0 : (decimal)reader["dqty"];
                                        var jqty = reader["jqty"] == DBNull.Value ? 0 : (decimal)reader["jqty"];
                                        var olqty = reader["olqty"] == DBNull.Value ? 0 : (decimal)reader["olqty"];
                                        shortagestock = oqty + dqty + jqty - iqty;
                                    }
                                }
                            }
                        }
                    }

                    transaction.Commit();
                    conn.Close();
                }
            }
            catch (Exception exp)
            {
                WebAPI.Common.Logger.Instance(Context.OrganizationID).Log(exp, WebAPI.Common.Logger.LogType.Error, oDelivery);
            }
            return oDelivery;
        }
    }

}
