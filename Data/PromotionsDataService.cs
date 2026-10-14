using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using InfyPOS.Models;
using InfyPOS.Common;
using Npgsql;
namespace InfyPOS.Data

{
    public class PromotionsDataService : SAASData
    {
        const string SQL_Search = @"select id, isactive, createdby, modifiedby, version, createdon, modifiedon, locationid,schemelocations, validfrom, validto, name, mode, rules,benefit,enabled,applicablemode,companyid from promotions where id=@id";
        const string SQL_SearchAll = @"select id, isactive, createdby, modifiedby, version, createdon, modifiedon, locationid,schemelocations, validfrom, validto, name, mode,enabled,applicablemode,companyid from promotions where id=@id";
        const string SQL_Insert = @"INSERT INTO public.promotions(isactive, createdby, modifiedby, version, createdon, modifiedon, locationid,schemelocations, validfrom, validto, name, mode, rules,benefit,enabled,applicablemode,companyid) 
            VALUES(@isactive, @createdby, @modifiedby, @version, @createdon, @modifiedon, @locationid,@schemelocations, @validfrom, @validto, @name, @mode, @rules,@benefit,@enabled,@applicablemode,@companyid) RETURNING id";

        internal bool RemoveBarcode(PromotionBarcode item)
        {
            var param = new SortedList<string, object>();
            param.Add("schemeid", item.id);
            if (item.include)
            {
                return ExecuteQuery("delete from Schemebarcodes where schemeid=@schemeid;", param) > 0;
            }
            else
            {
                return ExecuteQuery("delete from SchemeExcludedbarcodes where schemeid=@schemeid;", param) > 0;
            }
        }

        const string SQL_Update = @"UPDATE public.promotions SET modifiedby=@modifiedby, version=@version+1, modifiedon=@modifiedon, locationid=@locationid,schemelocations=@schemelocations, validfrom=@validfrom, validto=@validto,
                                    benefit=@benefit,enabled=@enabled,name=@name, mode=@mode, rules=@rules, applicablemode=@applicablemode,companyid=@companyid where id=@id and version=@version RETURNING version";
        const string SQL_Delete = @"update public.promotions set isactive=false,enabled=false,version=@version+1,modifiedby=@modifiedby,modifiedon=@modifiedon where id=@id and version=@version";

        internal bool UpdateRule(PromotionUpdate item)
        {

            if (item.option == PromotionSearch.SearchMode.barcodes)
            {
                using (NpgsqlConnection conn = CreateConnection())
                {
                    conn.Open();
                    var transaction = conn.BeginTransaction();
                    SchemebarcodesDataService barcodedata = new SchemebarcodesDataService() { Context = this.Context };
                    SchemeExcludedbarcodesDataService barcodeexcludedata = new SchemeExcludedbarcodesDataService() { Context = this.Context };
                    if (item.items != null && item.items.Count > 0)
                    {
                        foreach (var itm in item.items)
                        {
                            if (item.isadd)
                            {
                                barcodeexcludedata.Delete(new Schemebarcodes()
                                {
                                    barcode = itm.barcode,
                                    locationid = item.promotion.locationid,
                                    schemeid = item.promotion.id
                                });

                                barcodedata.Insert(transaction, conn, new Schemebarcodes()
                                {
                                    schemeid = item.promotion.id,
                                    locationid = item.locationid,
                                    barcode = itm.barcode
                                });
                            }
                            else
                            {
                                if (!barcodedata.Delete(transaction, conn, new Schemebarcodes()
                                {
                                    barcode = itm.barcode,
                                    schemeid = item.promotion.id
                                }))
                                {
                                    barcodeexcludedata.Insert(new Schemebarcodes()
                                    {
                                        barcode = itm.barcode,
                                        locationid = item.promotion.locationid,
                                        schemeid = item.promotion.id
                                    });
                                }
                            }
                        }
                    }
                    else
                    {
                        if (item.isadd)
                        {
                            barcodeexcludedata.Delete(new Schemebarcodes()
                            {
                                barcode = item.barcode,
                                locationid = item.promotion.locationid,
                                schemeid = item.promotion.id
                            });

                            barcodedata.Insert(transaction, conn, new Schemebarcodes()
                            {
                                schemeid = item.promotion.id,
                                locationid = item.promotion.locationid,
                                barcode = item.barcode
                            });
                        }
                        else
                        {
                            if (!barcodedata.Delete(transaction, conn, new Schemebarcodes()
                            {
                                schemeid = item.promotion.id,
                                locationid = item.promotion.locationid,
                                barcode = item.barcode
                            }))
                            {
                                barcodeexcludedata.Insert(new Schemebarcodes()
                                {
                                    barcode = item.barcode,
                                    locationid = item.promotion.locationid,
                                    schemeid = item.promotion.id
                                });
                            }
                        }
                    }
                    transaction.Commit();
                }
            }
            else
            {
                var promotions = Search(new PromotionsCreteria()
                {
                    id = item.promotion.id
                });
                if (promotions.Count == 1)
                {
                    if (!promotions[0].rules.Exists(e => e.mode == Promotions.Rules.RuleMode.invoiceitem))
                    {
                        promotions[0].rules.Add(new Promotions.Rules()
                        {
                            mode = Promotions.Rules.RuleMode.invoiceitem
                        });
                    }
                    var rule = promotions[0].rules.Find(e => e.mode == Promotions.Rules.RuleMode.invoiceitem);
                    if (rule.invoiceitemid == null)
                        rule.invoiceitemid = new List<long>();
                    if (item.isadd)
                    {
                        rule.invoiceitemid.AddRange(item.items.ConvertAll(e => e.id));
                    }
                    else
                    {
                        rule.invoiceitemid.RemoveAll(e => item.items.ConvertAll(ex => ex.id).Contains(e));
                    }
                    Update(promotions[0]);
                }
            }
            return true;
        }

        internal List<PromotionItem> SearchItems(PromotionSearch item)
        {
            if (item.option == PromotionSearch.SearchMode.barcodes)
            {
                string query = "Select invoiceitemid from stock where stock.barcode=@barcode";
                var param = new SortedList<string, object>();
                param.Add("barcode", item.barcode);
                long invoiceitemid = 0;
                using (var reader = ExecuteReader(query, param))
                {
                    if (reader.Read())
                        invoiceitemid = reader.GetInt64(0);
                }
                if (invoiceitemid > 0)
                {
                    query = @"select salablegoods.barcode,salablegoods.iqty,salablegoods.iqty - (salablegoods.oqty+salablegoods.jqty+salablegoods.dqty) stockqty,referencelist.name sizename,products.name productname,invoiceitemid from salablegoods
                            inner join stock on stock.id = salablegoods.stockid
                            inner join products on products.id = stock.productid
                            left outer join items on items.id = stock.itemid
                            left outer join referencelist on referencelist.id = items.sizeid
                            where salablegoods.locationid = @locationid and  invoiceitemid=@invoiceitemid";
                    param.Add("invoiceitemid", invoiceitemid);
                    param.Add("locationid", item.locationid);
                    using (var reader = ExecuteReader(query, param))
                    {
                        List<PromotionItem> retValue = new List<PromotionItem>();
                        while (reader.Read())
                        {
                            PromotionItem ret = new PromotionItem();
                            ret.barcode = reader["barcode"] != DBNull.Value ? reader["barcode"].ToString() : "";
                            ret.qty = reader["stockqty"] != DBNull.Value ? Convert.ToDecimal(reader["stockqty"]) : 0;
                            ret.tqty = reader["iqty"] != DBNull.Value ? Convert.ToDecimal(reader["iqty"]) : 0;
                            ret.product = reader["productname"] != DBNull.Value ? reader["productname"].ToString() : "";
                            ret.size = reader["sizename"] != DBNull.Value ? reader["sizename"].ToString() : "";
                            retValue.Add(ret);
                        }
                        return retValue;
                    }
                }
            }
            else
            {
                string query = @"Select lrinvoiceitems.lrinvoiceid from stock 
                                 inner join lrinvoiceitems on lrinvoiceitems.id = invoiceitemid and lrinvoiceitems.isactive=true
                                 where stock.barcode=@barcode";
                var param = new SortedList<string, object>();
                param.Add("barcode", item.barcode);
                long lrinvoiceid = 0;
                using (var reader = ExecuteReader(query, param))
                {
                    if (reader.Read())
                        lrinvoiceid = reader.GetInt64(0);
                }
                if (lrinvoiceid > 0)
                {
                    query = @"select sum(salablegoods.iqty - (salablegoods.oqty+salablegoods.jqty+salablegoods.dqty)) stockqty,buyingqty,referencelist.name sizename,products.name productname,invoiceitemid,sno,mrp from lrinvoiceitems
                            inner join stock on stock.invoiceitemid = lrinvoiceitems.id
			                inner join salablegoods on salablegoods.stockid = stock.id
                            inner join products on products.id = stock.productid
                            left outer join items on items.id = stock.itemid
                            left outer join referencelist on referencelist.id = items.sizeid
                            where salablegoods.locationid = @locationid and lrinvoiceitems.lrinvoiceid = @lrinvoiceid
                            group by referencelist.name ,buyingqty,products.name ,invoiceitemid,sno,mrp";
                    param.Add("lrinvoiceid", lrinvoiceid);
                    param.Add("locationid", item.locationid);
                    using (var reader = ExecuteReader(query, param))
                    {
                        List<PromotionItem> retValue = new List<PromotionItem>();
                        while (reader.Read())
                        {
                            PromotionItem ret = new PromotionItem();
                            ret.qty = reader["stockqty"] != DBNull.Value ? Convert.ToDecimal(reader["stockqty"]) : 0;
                            ret.tqty = reader["buyingqty"] != DBNull.Value ? Convert.ToDecimal(reader["buyingqty"]) : 0;
                            ret.product = reader["productname"] != DBNull.Value ? reader["productname"].ToString() : "";
                            ret.size = reader["sizename"] != DBNull.Value ? reader["sizename"].ToString() : "";
                            ret.id = reader["invoiceitemid"] != DBNull.Value ? Convert.ToInt64(reader["invoiceitemid"]) : 0;
                            ret.sno = reader["sno"] != DBNull.Value ? reader["sno"].ToString() : "";
                            ret.mrp = reader["mrp"] != DBNull.Value ? Convert.ToDecimal(reader["mrp"]) : 0;
                            retValue.Add(ret);
                        }
                        retValue.Sort((a, b) =>
                        {
                            int aint;
                            int bint;
                            int.TryParse(a.sno, out aint);
                            int.TryParse(b.sno, out bint);
                            return aint.CompareTo(bint);
                        });
                        return retValue;
                    }
                }
            }
            return new List<PromotionItem>();
        }

        internal List<Services.PromotionManager.ElibleReason> SearchPromotion(PromotionSearch item)
        {
            List<Services.PromotionManager.ElibleReason> schemeids = 
                Services.PromotionManager.Instance(Context.OrganizationID).FindAvailableMatchingSchmes(Context, item);
            return schemeids;
        }

        const string SQL_Deactivate = @"update public.promotions set enabled=@enabled,version=@version+1,modifiedby=@modifiedby,modifiedon=@modifiedon where id=@id and version=@version";
        const string SQL_HasChanged = "Select count(*) from public.promotions where modifiedon>@modifedon";
        public bool HasChaged(DateTime modifiedon)
        {
            List<Promotions> lst = new List<Promotions>();
            NpgsqlConnection conn = CreateConnection();
            conn.Open();
            var command = GetCommand(SQL_HasChanged, conn);
            command.Parameters.Add("modifiedon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = modifiedon;
            int count = Convert.ToInt32(command.ExecuteScalar());
            return count > 0;
        }

        public List<Promotions> Search(PromotionsCreteria _promotions)
        {
            List<Promotions> lst = new List<Promotions>();
            NpgsqlConnection conn = CreateConnection();
            try
            {
                conn.Open();

                NpgsqlCommand command = null;
                if (_promotions.id != 0)
                {
                    command = GetCommand(SQL_Search, conn);
                    command.Parameters.Add("id", NpgsqlTypes.NpgsqlDbType.Bigint).Value = _promotions.id;
                }
                else
                {
                    QueryBuilder qb = new QueryBuilder();
                    qb.CurrentQuery = SQL_SearchAll.Substring(0, SQL_SearchAll.IndexOf("where"));

                    if(_promotions.includedetail)
                        qb.CurrentQuery = SQL_Search.Substring(0, SQL_Search.IndexOf("where"));

                    qb.AddParameter("isactive", true, "=");

                    if (_promotions.locationid > 0)
                        qb.AddORParameter("locationid", _promotions.locationid, "=", "locationid = -1");


                    if (_promotions.applicablemode > 0)
                        qb.AddParameter("applicablemode", _promotions.applicablemode, "=");

                    if (_promotions.companyid > 0)
                        qb.AddParameter("companyid", _promotions.companyid, "=");

                    if (!string.IsNullOrEmpty(_promotions.name))
                        qb.AddParameter("lower(name)", "name", "%" + _promotions.name.ToLower() + "%", " ilike ");
                    if (_promotions.mode > 0)
                        qb.AddParameter("mode", _promotions.mode, "=");

                    if (_promotions.validfrom != DateTime.MinValue)
                    {
                        qb.AddParameter("validfrom", "validfrom", _promotions.validfrom.Date, ">=");
                    }
                    if (_promotions.validto != DateTime.MinValue)
                    {
                        qb.AddParameter("validto", "validto", _promotions.validto.AddDays(1), "<");
                    }

                    qb.SortField = "validfrom desc";
                    command = qb.GetCommand(conn);
                }

                using (System.Data.IDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        Promotions opromotions = new Promotions();
                        opromotions.id = reader["id"] == DBNull.Value ? 0 : Convert.ToInt64(reader["id"]);
                        opromotions.isactive = reader["isactive"] == DBNull.Value ? false : Convert.ToBoolean(reader["isactive"]);
                        opromotions.createdby = reader["createdby"] == DBNull.Value ? 0 : Convert.ToInt64(reader["createdby"]);
                        opromotions.modifiedby = reader["modifiedby"] == DBNull.Value ? 0 : Convert.ToInt64(reader["modifiedby"]);
                        opromotions.version = reader["version"] == DBNull.Value ? 0 : Convert.ToInt32(reader["version"]);
                        opromotions.createdon = reader["createdon"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(reader["createdon"]);
                        opromotions.modifiedon = reader["modifiedon"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(reader["modifiedon"]);
                        opromotions.locationid = reader["locationid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["locationid"]);
                        opromotions.schemelocations_json = reader["schemelocations"] == DBNull.Value ? "" : reader["schemelocations"].ToString();
                        opromotions.companyid = reader["companyid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["companyid"]);
                        opromotions.applicablemode = reader["applicablemode"] == DBNull.Value ? 0 : Convert.ToInt32(reader["applicablemode"]);
                        opromotions.validfrom = reader["validfrom"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(reader["validfrom"]);
                        opromotions.validto = reader["validto"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(reader["validto"]);
                        opromotions.name = reader["name"] == DBNull.Value ? "" : reader["name"].ToString();
                        opromotions.mode = reader["mode"] == DBNull.Value ? 0 : Convert.ToInt32(reader["mode"]);
                        if (_promotions.id != 0 || _promotions.includedetail)
                        {
                            opromotions.rules_json = reader["rules"] == DBNull.Value ? "" : reader["rules"].ToString();
                            opromotions.benefit_json = reader["benefit"] == DBNull.Value ? "" : reader["benefit"].ToString();
                        }
                        opromotions.enabled = reader["enabled"] == DBNull.Value ? false : Convert.ToBoolean(reader["enabled"]);
                        lst.Add(opromotions);
                    }
                }
                conn.Close();

                if (_promotions.id != 0)
                {
                    var param = new SortedList<string, object>();
                    param.Add("@schemeid", _promotions.id);
                    using (var reader = ExecuteReader(@"select count(*) from Schemebarcodes where schemeid=@schemeid;
                                    select count(*) from SchemeExcludedbarcodes where schemeid=@schemeid;", param))
                    {
                        if (reader.Read())
                            lst[0].barcode_count = Convert.ToInt32(reader[0]);
                        if (reader.NextResult() && reader.Read())
                            lst[0].excludebarcode_count = Convert.ToInt32(reader[0]);
                    }
                }
            }
            catch (Exception exp)
            {
                if (conn != null) conn.Close();
                WebAPI.Common.Logger.Instance(Context.OrganizationID).Log(exp, WebAPI.Common.Logger.LogType.Error, _promotions);
            }
            return lst;
        }

        public Promotions Insert(Promotions oPromotions)
        {

            NpgsqlTransaction transaction = null;
            try
            {
                using (NpgsqlConnection conn = CreateConnection())
                {
                    conn.Open();
                    transaction = conn.BeginTransaction();
                    NpgsqlCommand command = GetCommand(SQL_Insert, conn, transaction);

                    Insert(transaction, conn, oPromotions);
                    if (oPromotions.barcode_add != null && oPromotions.barcode_add.Count > 0)
                    {
                        if (oPromotions.barcode_remove != null && oPromotions.barcode_remove.Count > 0)
                            oPromotions.barcode_add.RemoveAll(e => oPromotions.barcode_remove.Contains(e));
                        SchemebarcodesDataService barcodedata = new SchemebarcodesDataService() { Context = this.Context };
                        foreach (string item in oPromotions.barcode_add)
                        {
                            barcodedata.Insert(transaction, conn, new Schemebarcodes()
                            {
                                schemeid = oPromotions.id,
                                locationid = oPromotions.locationid,
                                barcode = item
                            });
                        }
                    }

                    if (oPromotions.barcode_exclude_add != null && oPromotions.barcode_exclude_add.Count > 0)
                    {
                        if (oPromotions.barcode_exclude_remove != null && oPromotions.barcode_exclude_remove.Count > 0)
                            oPromotions.barcode_exclude_add.RemoveAll(e => oPromotions.barcode_exclude_remove.Contains(e));
                        SchemeExcludedbarcodesDataService barcodedata = new SchemeExcludedbarcodesDataService() { Context = this.Context };
                        foreach (string item in oPromotions.barcode_exclude_add)
                        {
                            barcodedata.Insert(transaction, conn, new Schemebarcodes()
                            {
                                schemeid = oPromotions.id,
                                locationid = oPromotions.locationid,
                                barcode = item
                            });
                        }
                    }

                    if (oPromotions.coupon_add != null && oPromotions.coupon_add.Count > 0 &&
                        oPromotions.applicablemode == 4 && oPromotions.benefit.Exists(e => e.mode == Promotions.Benefit.BenefitMode.Coupen))
                    {
                        var couponitem = oPromotions.benefit.First(e => e.mode == Promotions.Benefit.BenefitMode.Coupen);
                        CustomerCouponsDataService voucherdata = new CustomerCouponsDataService() { Context = this.Context };
                        var voucher = new CustomerCoupons()
                        {
                            issuedby = Context.UserID,
                            issueto = 0,
                            amount = couponitem.unit > 1 ? couponitem.unit : couponitem.value,
                            isactive = true,
                            vouchertype = CustomerCoupons.CouponType.Coupon,
                            issuedon = DateTimeNow,
                            validupto = couponitem.validity,
                            minbillvalue = couponitem.pricefrom,
                            acceptablelocationid = couponitem.locationid,
                            maxbilldiscountpercentage = couponitem.priceto,
                            unlimited = couponitem.unlimited
                        };
                        voucherdata.Insert(transaction, conn, voucher);
                    }

                    transaction.Commit();

                    InfyPOS.Processors.CoreDBListener.SendNotification(Context.OrganizationID, new InfyPOS.Processors.Config()
                    {
                        id = oPromotions.id,
                        type = "PROMOTION"
                    }, conn);

                    conn.Close();
                }
            }
            catch (Exception exp)
            {
                WebAPI.Common.Logger.Instance(Context.OrganizationID).Log(exp, WebAPI.Common.Logger.LogType.Error, oPromotions);

            }
            return oPromotions;
        }

        public Promotions Insert(NpgsqlTransaction transaction, NpgsqlConnection conn, Promotions oPromotions)
        {

            NpgsqlCommand command = GetCommand(SQL_Insert, conn, transaction);
            command.Parameters.Add("isactive", NpgsqlTypes.NpgsqlDbType.Boolean).Value = oPromotions.isactive;
            command.Parameters.Add("createdby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = Context.UserID;
            command.Parameters.Add("modifiedby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = Context.UserID;
            command.Parameters.Add("version", NpgsqlTypes.NpgsqlDbType.Integer).Value = oPromotions.version;
            command.Parameters.Add("createdon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = DateTimeNow;
            command.Parameters.Add("modifiedon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = DateTimeNow;
            command.Parameters.Add("locationid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oPromotions.locationid;
            command.Parameters.Add("companyid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oPromotions.companyid;
            command.Parameters.Add("applicablemode", NpgsqlTypes.NpgsqlDbType.Integer).Value = oPromotions.applicablemode;
            command.Parameters.Add("validfrom", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = oPromotions.validfrom;
            command.Parameters.Add("validto", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = oPromotions.validto;
            command.Parameters.Add("schemelocations", NpgsqlTypes.NpgsqlDbType.Json).Value = oPromotions.schemelocations_json == null ? "null" : oPromotions.schemelocations_json;
            command.Parameters.Add("name", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oPromotions.name == null ? "" : oPromotions.name;
            command.Parameters.Add("mode", NpgsqlTypes.NpgsqlDbType.Integer).Value = oPromotions.mode;
            command.Parameters.Add("rules", NpgsqlTypes.NpgsqlDbType.Json).Value = oPromotions.rules_json == null ? "null" : oPromotions.rules_json;
            command.Parameters.Add("benefit", NpgsqlTypes.NpgsqlDbType.Json).Value = oPromotions.benefit_json == null ? "null" : oPromotions.benefit_json;
            command.Parameters.Add("enabled", NpgsqlTypes.NpgsqlDbType.Boolean).Value = oPromotions.enabled;
            using (System.Data.IDataReader reader = command.ExecuteReader())
            {
                if (reader.Read())
                {
                    oPromotions.id = Convert.ToInt64(reader[0].ToString());
                }
            }
            return oPromotions;
        }
        public bool UpdateSchemeLocation(PromotionsCreteria oPromotions)
        {
            NpgsqlTransaction transaction = null;
            try
            {
                using (NpgsqlConnection conn = CreateConnection())
                {
                    conn.Open();
                    transaction = conn.BeginTransaction();
                    foreach (long id in oPromotions.ids)
                    {
                        var results = Search(new PromotionsCreteria() { id = id });
                        if (results.Count == 1)
                        {
                            results[0].schemelocations = oPromotions.schemelocations;
                            Update(transaction, conn, results[0]);
                        }
                    }
                    transaction.Commit();
                    InfyPOS.Processors.CoreDBListener.SendNotification(Context.OrganizationID, new InfyPOS.Processors.Config()
                    {
                        id = oPromotions.id,
                        type = "PROMOTION"
                    }, conn);

                    conn.Close();
                }
            }
            catch (Exception exp)
            {
                WebAPI.Common.Logger.Instance(Context.OrganizationID).Log(exp, WebAPI.Common.Logger.LogType.Error, oPromotions);
                throw exp;
            }
            return true;
        }

        public Promotions Update(Promotions oPromotions)
        {

            NpgsqlTransaction transaction = null;
            try
            {
                using (NpgsqlConnection conn = CreateConnection())
                {
                    conn.Open();
                    transaction = conn.BeginTransaction();

                    Update(transaction, conn, oPromotions);

                    oPromotions.barcode_add.RemoveAll(e => oPromotions.barcode_exclude_add.Contains(e));
                    oPromotions.barcode_exclude_add.RemoveAll(e => oPromotions.barcode_add.Contains(e));

                    SchemeExcludedbarcodesDataService barcodeexcludeddata = new SchemeExcludedbarcodesDataService() { Context = this.Context };
                    SchemebarcodesDataService barcodedata = new SchemebarcodesDataService() { Context = this.Context };

                    if (oPromotions.barcode_add != null && oPromotions.barcode_add.Count > 0)
                    {
                        foreach (string item in oPromotions.barcode_add)
                        {
                            barcodedata.Insert(transaction, conn, new Schemebarcodes()
                            {
                                schemeid = oPromotions.id,
                                locationid = oPromotions.locationid,
                                barcode = item
                            });

                            barcodeexcludeddata.Delete(transaction, conn, new Schemebarcodes()
                            {
                                schemeid = oPromotions.id,
                                locationid = oPromotions.locationid,
                                barcode = item
                            });
                        }
                    }

                    //if (oPromotions.barcode_remove != null && oPromotions.barcode_remove.Count > 0)
                    //{
                    //    foreach (string item in oPromotions.barcode_remove)
                    //    {
                    //        barcodedata.Delete(transaction, conn, new Schemebarcodes()
                    //        {
                    //            schemeid = oPromotions.id,
                    //            locationid = oPromotions.locationid,
                    //            barcode = item
                    //        });
                    //    }
                    //}


                    if (oPromotions.barcode_exclude_add != null && oPromotions.barcode_exclude_add.Count > 0)
                    {

                        foreach (string item in oPromotions.barcode_exclude_add)
                        {
                            barcodeexcludeddata.Insert(transaction, conn, new Schemebarcodes()
                            {
                                schemeid = oPromotions.id,
                                locationid = oPromotions.locationid,
                                barcode = item
                            });

                            barcodedata.Delete(transaction, conn, new Schemebarcodes()
                            {
                                schemeid = oPromotions.id,
                                locationid = oPromotions.locationid,
                                barcode = item
                            });
                        }
                    }

                    //if (oPromotions.barcode_exclude_remove != null && oPromotions.barcode_exclude_remove.Count > 0)
                    //{
                    //    foreach (string item in oPromotions.barcode_exclude_remove)
                    //    {
                    //        barcodeexcludeddata.Delete(transaction, conn, new Schemebarcodes()
                    //        {
                    //            schemeid = oPromotions.id,
                    //            locationid = oPromotions.locationid,
                    //            barcode = item
                    //        });
                    //    }
                    //}

                    transaction.Commit();


                    InfyPOS.Processors.CoreDBListener.SendNotification(Context.OrganizationID, new InfyPOS.Processors.Config()
                    {
                        id = oPromotions.id,
                        type = "PROMOTION"
                    }, conn);

                    conn.Close();
                }
            }
            catch (Exception exp)
            {
                WebAPI.Common.Logger.Instance(Context.OrganizationID).Log(exp, WebAPI.Common.Logger.LogType.Error, oPromotions);
                throw exp;
            }

            return oPromotions;
        }

        public Promotions Update(NpgsqlTransaction transaction, NpgsqlConnection conn, Promotions oPromotions)
        {

            NpgsqlCommand command = GetCommand(SQL_Update, conn, transaction);
            command.Parameters.Add("id", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oPromotions.id;
            command.Parameters.Add("isactive", NpgsqlTypes.NpgsqlDbType.Boolean).Value = oPromotions.isactive;
            command.Parameters.Add("createdby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oPromotions.createdby;
            command.Parameters.Add("modifiedby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = Context.UserID;
            command.Parameters.Add("version", NpgsqlTypes.NpgsqlDbType.Integer).Value = oPromotions.version;
            command.Parameters.Add("createdon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = oPromotions.createdon;
            command.Parameters.Add("modifiedon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = DateTimeNow;
            command.Parameters.Add("locationid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oPromotions.locationid;
            command.Parameters.Add("companyid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oPromotions.companyid;
            command.Parameters.Add("applicablemode", NpgsqlTypes.NpgsqlDbType.Integer).Value = oPromotions.applicablemode;
            command.Parameters.Add("validfrom", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = oPromotions.validfrom;
            command.Parameters.Add("validto", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = oPromotions.validto;
            command.Parameters.Add("name", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oPromotions.name == null ? "" : oPromotions.name;
            command.Parameters.Add("mode", NpgsqlTypes.NpgsqlDbType.Integer).Value = oPromotions.mode;
            command.Parameters.Add("schemelocations", NpgsqlTypes.NpgsqlDbType.Json).Value = oPromotions.schemelocations_json == null ? "null" : oPromotions.schemelocations_json;
            command.Parameters.Add("rules", NpgsqlTypes.NpgsqlDbType.Json).Value = oPromotions.rules_json == null ? "null" : oPromotions.rules_json;
            command.Parameters.Add("benefit", NpgsqlTypes.NpgsqlDbType.Json).Value = oPromotions.benefit_json == null ? "null" : oPromotions.benefit_json;
            command.Parameters.Add("enabled", NpgsqlTypes.NpgsqlDbType.Boolean).Value = oPromotions.enabled;
            using (System.Data.IDataReader reader = command.ExecuteReader())
            {
                if (reader.Read())
                {
                    oPromotions.version = Convert.ToInt32(reader[0].ToString());
                }
            }

            return oPromotions;
        }


        public bool Delete(Promotions oPromotions)
        {

            NpgsqlTransaction transaction = null;
            bool hasSuccessCount = false;
            try
            {
                using (NpgsqlConnection conn = CreateConnection())
                {
                    conn.Open();
                    transaction = conn.BeginTransaction();
                    hasSuccessCount = Delete(transaction, conn, oPromotions);
                    transaction.Commit();

                    InfyPOS.Processors.CoreDBListener.SendNotification(Context.OrganizationID, new InfyPOS.Processors.Config()
                    {
                        id = oPromotions.id,
                        type = "PROMOTION"
                    }, conn);

                    conn.Close();
                }
            }
            catch (Exception exp)
            {
                WebAPI.Common.Logger.Instance(Context.OrganizationID).Log(exp, WebAPI.Common.Logger.LogType.Error, oPromotions);

                throw exp;
            }
            return hasSuccessCount;

        }

        public bool Delete(NpgsqlTransaction transaction, NpgsqlConnection conn, Promotions oPromotions)
        {

            NpgsqlCommand command = GetCommand(SQL_Delete, conn, transaction);
            command.Parameters.Add("isactive", NpgsqlTypes.NpgsqlDbType.Boolean).Value = oPromotions.isactive;
            command.Parameters.Add("modifiedby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = Context.UserID;
            command.Parameters.Add("modifiedon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = DateTimeNow;
            command.Parameters.Add("version", NpgsqlTypes.NpgsqlDbType.Integer).Value = oPromotions.version;
            command.Parameters.Add("id", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oPromotions.id;
            int successCount = command.ExecuteNonQuery();

            return successCount > 0;
        }

        public bool Deactivate(Promotions oPromotions)
        {

            NpgsqlTransaction transaction = null;
            bool hasSuccessCount = false;
            try
            {
                using (NpgsqlConnection conn = CreateConnection())
                {
                    conn.Open();
                    transaction = conn.BeginTransaction();

                    NpgsqlCommand command = GetCommand(SQL_Deactivate, conn, transaction);
                    command.Parameters.Add("enabled", NpgsqlTypes.NpgsqlDbType.Boolean).Value = oPromotions.enabled;
                    command.Parameters.Add("modifiedby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = Context.UserID;
                    command.Parameters.Add("modifiedon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = DateTimeNow;
                    command.Parameters.Add("version", NpgsqlTypes.NpgsqlDbType.Integer).Value = oPromotions.version;
                    command.Parameters.Add("id", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oPromotions.id;
                    int successCount = command.ExecuteNonQuery();
                    hasSuccessCount = successCount > 0;

                    transaction.Commit();

                    InfyPOS.Processors.CoreDBListener.SendNotification(Context.OrganizationID, new InfyPOS.Processors.Config()
                    {
                        id = oPromotions.id,
                        type = "PROMOTION"
                    }, conn);

                    conn.Close();
                }
            }
            catch (Exception exp)
            {
                WebAPI.Common.Logger.Instance(Context.OrganizationID).Log(exp, WebAPI.Common.Logger.LogType.Error, oPromotions);

                throw exp;
            }
            return hasSuccessCount;

        }

        internal List<Promotions> InsertAll(PromotionUpload item)
        {
            List<Promotions> created = new List<Promotions>();
            foreach (var rule in item.items.GroupBy(x=>new {x.discountpercentage,x.discountvalue,x.fromdate,x.todate}))
            {
                Promotions promotions = new Promotions()
                {
                    isactive = true,
                    createdby = Context.UserID,
                    modifiedby = Context.UserID,
                    version = 1,
                    createdon = DateTime.Now,
                    modifiedon = DateTime.Now,
                    mode = 2,
                    applicablemode = 1,
                    validfrom = BaseEntity.GetDataFromString(rule.Key.fromdate,"yyyy-MM-dd"),
                    validto = BaseEntity.GetDataFromString(rule.Key.todate, "yyyy-MM-dd"),
                    companyid = item.companyid,
                    locationid = item.locationid,
                    rules = new List<Promotions.Rules>(),
                    benefit = new List<Promotions.Benefit>()
                };

                promotions.rules.Add(new Promotions.Rules()
                {
                    mode = Promotions.Rules.RuleMode.agmtcode,
                    item = new Promotions.ItemDefinition()
                    {
                        agmtcodes = rule.ToList().ConvertAll(x=>x.itemcode)
                    }
                });

                if (rule.Key.discountvalue > 0)
                    promotions.benefit.Add(new Promotions.Benefit()
                    {
                        mode = Promotions.Benefit.BenefitMode.DiscountInFixedValue,
                        qtyfrom = 1,
                        value = rule.Key.discountvalue
                    });

                if (rule.Key.discountpercentage > 0)
                    promotions.benefit.Add(new Promotions.Benefit()
                    {
                        mode = Promotions.Benefit.BenefitMode.DiscountInPercentage,
                        qtyfrom = 1,
                        value = rule.Key.discountpercentage
                    });

                if (rule.Key.discountpercentage > 0)
                    promotions.name = item.name + rule.Key.discountpercentage.ToString("F0") + "%";
                else
                    promotions.name = item.name + rule.Key.discountvalue.ToString("F0");

                Insert(promotions);
                created.Add(promotions);
            }
            return created;
        }

        internal Promotions Clone(Promotions item)
        {
            var sourceid = item.id;

            SchemebarcodesDataService barcodedata = new SchemebarcodesDataService() { Context = this.Context };
            SchemeExcludedbarcodesDataService barcodeexcludedata = new SchemeExcludedbarcodesDataService() { Context = this.Context };
            NpgsqlTransaction transaction = null;

            try
            {
                using (NpgsqlConnection conn = CreateConnection())
                {
                    conn.Open();
                    transaction = conn.BeginTransaction();
                    Insert(transaction, conn, item);
                    var barcodes = barcodedata.Search(new SchemebarcodesCreteria()
                    {
                        schemeid = sourceid
                    });

                    foreach(var bcode in barcodes)
                    {
                        bcode.schemeid = item.id;
                        bcode.id = 0;
                        barcodedata.Insert(transaction, conn, bcode);
                    }

                    var ebarcodes = barcodeexcludedata.Search(new SchemebarcodesCreteria()
                    {
                        schemeid = sourceid
                    });
                    foreach (var bcode in ebarcodes)
                    {
                        bcode.schemeid = item.id;
                        bcode.id = 0;
                        barcodeexcludedata.Insert(transaction, conn, bcode);
                    }

                    transaction.Commit();

                    InfyPOS.Processors.CoreDBListener.SendNotification(Context.OrganizationID, new InfyPOS.Processors.Config()
                    {
                        id = item.id,
                        type = "PROMOTION"
                    }, conn);

                    conn.Close();
                }
            }
            catch (Exception exp)
            {
                WebAPI.Common.Logger.Instance(Context.OrganizationID).Log(exp, WebAPI.Common.Logger.LogType.Error, item);

                throw exp;
            }
            return item;
        }
    }

}
