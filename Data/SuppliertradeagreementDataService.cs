using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using InfyPOS.Models;
using InfyPOS.Common;
using Npgsql;
using System.Data;

namespace InfyPOS.Data
{
    public class SuppliertradeagreementDataService : BaseData
    {
        const string SQL_Search = @"select id, isactive, createdby, modifiedby, version, createdon, modifiedon, productgroupid,Supplierproductgroupsid, cur_seq, mrp, disable, disabledon, items, matrixlist, supplierid, approvedby, brandid, agreementon, cost, price, itemcode, marginmode, code, agreementname, barcodemode,namingmode from suppliertradeagreement where id=@id";
        const string SQL_Insert = @"INSERT INTO public.suppliertradeagreement(isactive, createdby, modifiedby, version, createdon, modifiedon, productgroupid,Supplierproductgroupsid, cur_seq, mrp, disable, disabledon, items, matrixlist, supplierid, approvedby, brandid, agreementon, cost, price, itemcode, marginmode, code, agreementname, barcodemode,namingmode) VALUES(@isactive, @createdby, @modifiedby, @version, @createdon, @modifiedon, @productgroupid,@Supplierproductgroupsid, @cur_seq, @mrp, @disable, @disabledon, @items, @matrixlist, @supplierid, @approvedby, @brandid, @agreementon, @cost, @price, @itemcode, @marginmode, @code, @agreementname, @barcodemode,@namingmode) RETURNING id";
        const string SQL_Update = @"UPDATE public.suppliertradeagreement SET modifiedby=@modifiedby, version=@version+1, modifiedon=@modifiedon, productgroupid=@productgroupid,Supplierproductgroupsid=@Supplierproductgroupsid, cur_seq=@cur_seq, mrp=@mrp, disable=@disable, disabledon=@disabledon, items=@items, matrixlist=@matrixlist, supplierid=@supplierid, approvedby=@approvedby, brandid=@brandid, agreementon=@agreementon, cost=@cost, price=@price, itemcode=@itemcode, marginmode=@marginmode, code=@code, agreementname=@agreementname, barcodemode=@barcodemode,namingmode=@namingmode where id=@id and version=@version RETURNING version";
        const string SQL_Delete = @"update public.suppliertradeagreement set isactive=false,version=@version+1,modifiedby=@modifiedby,modifiedon=@modifiedon where id=@id and version=@version";
        public bool IsMigration { get; set; }
        public List<Suppliertradeagreement> Search(SuppliertradeagreementCreteria _suppliertradeagreement)
        {
            List<Suppliertradeagreement> lst = new List<Suppliertradeagreement>();
            NpgsqlConnection conn = CreateConnection();
            try
            {
                conn.Open();

                NpgsqlCommand command = null;
                if (_suppliertradeagreement.id != 0)
                {
                    command = new NpgsqlCommand(SQL_Search, conn);
                    command.Parameters.Add("id", NpgsqlTypes.NpgsqlDbType.Bigint).Value = _suppliertradeagreement.id;
                }
                else
                {
                    QueryBuilder qb = new QueryBuilder();
                    qb.CurrentQuery = SQL_Search.Substring(0, SQL_Search.IndexOf("where"));
                    qb.AddParameter("isactive", true, "=");

                    if (!string.IsNullOrEmpty(_suppliertradeagreement.agreementname))
                    {
                        if (_suppliertradeagreement.agreementname.IndexOf('%') >= 0)
                            qb.AddParameter("agreementname", _suppliertradeagreement.agreementname, " ilike ");
                        else
                            qb.AddParameter("agreementname", _suppliertradeagreement.agreementname, "=");
                    }


                    if (!string.IsNullOrEmpty(_suppliertradeagreement.uan))
                    {
                        qb.Add2Query("id in (select tradeagreeentid from items where gln='" + WebAPI.Data.Utility.SafeSQL( _suppliertradeagreement.uan) + "')");
                    }

                    if (!string.IsNullOrEmpty(_suppliertradeagreement.itemcode))
                    {
                        if (_suppliertradeagreement.itemcode.IndexOf('%') >= 0)
                            qb.AddParameter("itemcode", _suppliertradeagreement.itemcode, " ilike ");
                        else
                            qb.AddParameter("itemcode", _suppliertradeagreement.itemcode, "=");

                        //qb.Add2Query("supplierproductgroupsid in (select id from supplierproductgroups where itemcode='" + WebAPI.Data.Utility.SafeSQL(_suppliertradeagreement.itemcode) + "')");
                    }

                    if (_suppliertradeagreement.productgroupid > 0)
                        qb.AddParameter("productgroupid", _suppliertradeagreement.productgroupid, "=");

                    if (_suppliertradeagreement.supplierid > 0)
                        qb.AddParameter("supplierid", _suppliertradeagreement.supplierid, "=");

                    if (_suppliertradeagreement.approvedby > 0)
                        qb.AddParameter("approvedby", _suppliertradeagreement.approvedby, "=");

                    if (_suppliertradeagreement.fromdisabled.Year > 2000)
                    {
                        qb.AddParameter("disabledon", _suppliertradeagreement.fromdisabled.Date, ">=");
                        qb.AddParameter("disabledon", _suppliertradeagreement.todisabled.Date.AddDays(1), "<");
                    }

                    if (_suppliertradeagreement.fromodified.Year > 2000)
                    {
                        qb.AddParameter("modifiedon", _suppliertradeagreement.fromodified.Date, ">=");
                        qb.AddParameter("modifiedon", _suppliertradeagreement.tomodified.Date.AddDays(1), "<");
                    }

                    if (_suppliertradeagreement.fromcreated.Year > 2000)
                    {
                        qb.AddParameter("createdon", _suppliertradeagreement.fromcreated.Date, ">=");
                        qb.AddParameter("createdon", _suppliertradeagreement.tocreated.Date.AddDays(1), "<");
                    }

                    if (_suppliertradeagreement.Supplierproductgroupsid > 0)
                        qb.AddParameter("Supplierproductgroupsid", _suppliertradeagreement.Supplierproductgroupsid, "=");

                    if (_suppliertradeagreement.Supplierproductgroupsids != null &&
                        _suppliertradeagreement.Supplierproductgroupsids.Count > 0)
                    {
                        qb.AddInParamer("Supplierproductgroupsid", String.Join(",", _suppliertradeagreement.Supplierproductgroupsids));
                    }

                    command = qb.GetCommand(conn);
                }

                using (System.Data.IDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        Suppliertradeagreement osuppliertradeagreement = new Suppliertradeagreement();
                        osuppliertradeagreement.id = reader["id"] == DBNull.Value ? 0 : Convert.ToInt64(reader["id"]);
                        osuppliertradeagreement.isactive = reader["isactive"] == DBNull.Value ? false : Convert.ToBoolean(reader["isactive"]);
                        osuppliertradeagreement.createdby = reader["createdby"] == DBNull.Value ? 0 : Convert.ToInt64(reader["createdby"]);
                        osuppliertradeagreement.modifiedby = reader["modifiedby"] == DBNull.Value ? 0 : Convert.ToInt64(reader["modifiedby"]);
                        osuppliertradeagreement.version = reader["version"] == DBNull.Value ? 0 : Convert.ToInt32(reader["version"]);
                        osuppliertradeagreement.createdon = reader["createdon"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(reader["createdon"]);
                        osuppliertradeagreement.modifiedon = reader["modifiedon"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(reader["modifiedon"]);
                        osuppliertradeagreement.productgroupid = reader["productgroupid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["productgroupid"]);
                        osuppliertradeagreement.Supplierproductgroupsid = reader["Supplierproductgroupsid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["Supplierproductgroupsid"]);
                        osuppliertradeagreement.mrp = reader["mrp"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["mrp"]);
                        osuppliertradeagreement.disable = reader["disable"] == DBNull.Value ? false : Convert.ToBoolean(reader["disable"]);
                        osuppliertradeagreement.disabledon = reader["disabledon"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(reader["disabledon"]);

                        osuppliertradeagreement.supplierid = reader["supplierid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["supplierid"]);
                        osuppliertradeagreement.approvedby = reader["approvedby"] == DBNull.Value ? 0 : Convert.ToInt64(reader["approvedby"]);
                        osuppliertradeagreement.brandid = reader["brandid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["brandid"]);
                        osuppliertradeagreement.agreementon = reader["agreementon"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(reader["agreementon"]);
                        osuppliertradeagreement.cost = reader["cost"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["cost"]);
                        osuppliertradeagreement.price = reader["price"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["price"]);
                        osuppliertradeagreement.itemcode = reader["itemcode"] == DBNull.Value ? "" : reader["itemcode"].ToString();
                        osuppliertradeagreement.code = reader["code"] == DBNull.Value ? "" : reader["code"].ToString();
                        osuppliertradeagreement.agreementname = reader["agreementname"] == DBNull.Value ? "" : reader["agreementname"].ToString();
                        if (!_suppliertradeagreement.summaryview)
                        {
                            osuppliertradeagreement.items_json = reader["items"] == DBNull.Value ? null : reader["items"].ToString();
                            osuppliertradeagreement.matrixlist_json = reader["matrixlist"] == DBNull.Value ? null : reader["matrixlist"].ToString();
                        }
                        else
                        {
                            osuppliertradeagreement.cur_seq = reader["cur_seq"] == DBNull.Value ? 0 : Convert.ToInt32(reader["cur_seq"]);
                            osuppliertradeagreement.barcodemode = reader["barcodemode"] == DBNull.Value ? "" : reader["barcodemode"].ToString();
                            osuppliertradeagreement.namingmode = reader["namingmode"] == DBNull.Value ? 0 : Convert.ToInt64(reader["namingmode"]);
                            osuppliertradeagreement.marginmode = reader["marginmode"] == DBNull.Value ? "" : reader["marginmode"].ToString();

                        }
                        lst.Add(osuppliertradeagreement);
                    }
                }
                conn.Close();
            }
            catch (Exception exp)
            {
                if (conn != null) conn.Close();
                WebAPI.Common.Logger.Instance(Context.OrganizationID).Log(exp, WebAPI.Common.Logger.LogType.Error, _suppliertradeagreement);
            }
            return lst;
        }

        public List<Suppliertradeagreementdetail> SearchDetail(SuppliertradeagreementCreteria _suppliertradeagreement)
        {
            List<Suppliertradeagreementdetail> lst = new List<Suppliertradeagreementdetail>();
            NpgsqlConnection conn = CreateConnection();
            try
            {
                conn.Open();

                NpgsqlCommand command = null;


//                select distinct schemelist->> 'printingname' itemname,
//schemelist->> 'gln' itemgln,products.hsncode,
// itemcode,  agreementname, products.name productname
// from suppliertradeagreement
// inner join products on products.id = productgroupid
//LEFT JOIN LATERAL
//json_array_elements(suppliertradeagreement.items :: json) schemelist ON true
// where suppliertradeagreement.isactive and suppliertradeagreement.items is not null and
//suppliertradeagreement.items->0 is not null

                QueryBuilder qb = new QueryBuilder();
                qb.CurrentQuery = @"select schemelist->>'printingname' itemname,
cast(schemelist->>'cost' as numeric) itemcost,
cast(schemelist->>'price' as numeric) itemprice,
cast(schemelist->>'mrp' as numeric) itemmrp,
schemelist->>'gln' itemgln,
id, isactive, createdby, modifiedby, version, createdon, modifiedon, 
productgroupid,Supplierproductgroupsid, cur_seq, mrp, disable, disabledon, matrixlist, 
supplierid, approvedby, brandid, agreementon, cost, price, itemcode, marginmode, code, agreementname, 
barcodemode,namingmode from suppliertradeagreement 
LEFT JOIN LATERAL
json_array_elements (suppliertradeagreement.items :: json) schemelist ON true 
 where suppliertradeagreement.isactive and suppliertradeagreement.items is not null and 
suppliertradeagreement.items->0 is not null";

                qb.AddParameter("suppliertradeagreement.isactive", true, "=");

                if (!string.IsNullOrEmpty(_suppliertradeagreement.agreementname))
                {
                    if (_suppliertradeagreement.agreementname.IndexOf('%') >= 0)
                        qb.AddParameter("agreementname", _suppliertradeagreement.agreementname, " ilike ");
                    else
                        qb.AddParameter("agreementname", _suppliertradeagreement.agreementname, "=");
                }


                if (!string.IsNullOrEmpty(_suppliertradeagreement.itemname))
                {
                    if (_suppliertradeagreement.itemname.IndexOf('%') >= 0)
                        qb.CurrentQuery += " and cast(schemelist->>'printingname' as varchar) ilike '" + WebAPI.Data.Utility.SafeSQL(_suppliertradeagreement.itemname) + "'";
                    else
                        qb.CurrentQuery += " and cast(schemelist->>'printingname' as varchar) = '" + WebAPI.Data.Utility.SafeSQL(_suppliertradeagreement.itemname) + "'";
                }

                if (!string.IsNullOrEmpty(_suppliertradeagreement.uan))
                {
                    qb.Add2Query("id in (select tradeagreeentid from items where gln='" + WebAPI.Data.Utility.SafeSQL(_suppliertradeagreement.uan) + "')");
                }

                if (_suppliertradeagreement.productgroupid > 0)
                    qb.AddParameter("productgroupid", _suppliertradeagreement.productgroupid, "=");

                if (_suppliertradeagreement.supplierid > 0)
                    qb.AddParameter("supplierid", _suppliertradeagreement.supplierid, "=");

                if (_suppliertradeagreement.approvedby > 0)
                    qb.AddParameter("approvedby", _suppliertradeagreement.approvedby, "=");

                if (_suppliertradeagreement.fromdisabled.Year > 2000)
                {
                    qb.AddParameter("disabledon", _suppliertradeagreement.fromdisabled.Date, ">=");
                    qb.AddParameter("disabledon", _suppliertradeagreement.todisabled.Date.AddDays(1), "<");
                }

                if (_suppliertradeagreement.fromodified.Year > 2000)
                {
                    qb.AddParameter("modifiedon", _suppliertradeagreement.fromodified.Date, ">=");
                    qb.AddParameter("modifiedon", _suppliertradeagreement.tomodified.Date.AddDays(1), "<");
                }

                if (_suppliertradeagreement.fromcreated.Year > 2000)
                {
                    qb.AddParameter("createdon", _suppliertradeagreement.fromcreated.Date, ">=");
                    qb.AddParameter("createdon", _suppliertradeagreement.tocreated.Date.AddDays(1), "<");
                }

                if (_suppliertradeagreement.Supplierproductgroupsid > 0)
                    qb.AddParameter("Supplierproductgroupsid", _suppliertradeagreement.Supplierproductgroupsid, "=");

                if (_suppliertradeagreement.Supplierproductgroupsids != null &&
                    _suppliertradeagreement.Supplierproductgroupsids.Count > 0)
                {
                    qb.AddInParamer("Supplierproductgroupsid", String.Join(",", _suppliertradeagreement.Supplierproductgroupsids));
                }

                command = qb.GetCommand(conn);

                using (System.Data.IDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        Suppliertradeagreementdetail osuppliertradeagreement = new Suppliertradeagreementdetail();
                        osuppliertradeagreement.id = reader["id"] == DBNull.Value ? 0 : Convert.ToInt64(reader["id"]);
                        osuppliertradeagreement.isactive = reader["isactive"] == DBNull.Value ? false : Convert.ToBoolean(reader["isactive"]);
                        osuppliertradeagreement.createdby = reader["createdby"] == DBNull.Value ? 0 : Convert.ToInt64(reader["createdby"]);
                        osuppliertradeagreement.modifiedby = reader["modifiedby"] == DBNull.Value ? 0 : Convert.ToInt64(reader["modifiedby"]);
                        osuppliertradeagreement.version = reader["version"] == DBNull.Value ? 0 : Convert.ToInt32(reader["version"]);
                        osuppliertradeagreement.createdon = reader["createdon"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(reader["createdon"]);
                        osuppliertradeagreement.modifiedon = reader["modifiedon"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(reader["modifiedon"]);
                        osuppliertradeagreement.productgroupid = reader["productgroupid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["productgroupid"]);
                        osuppliertradeagreement.Supplierproductgroupsid = reader["Supplierproductgroupsid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["Supplierproductgroupsid"]);
                        osuppliertradeagreement.mrp = reader["mrp"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["mrp"]);
                        osuppliertradeagreement.disable = reader["disable"] == DBNull.Value ? false : Convert.ToBoolean(reader["disable"]);
                        osuppliertradeagreement.disabledon = reader["disabledon"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(reader["disabledon"]);

                        osuppliertradeagreement.supplierid = reader["supplierid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["supplierid"]);
                        osuppliertradeagreement.approvedby = reader["approvedby"] == DBNull.Value ? 0 : Convert.ToInt64(reader["approvedby"]);
                        osuppliertradeagreement.brandid = reader["brandid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["brandid"]);
                        osuppliertradeagreement.agreementon = reader["agreementon"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(reader["agreementon"]);
                        osuppliertradeagreement.cost = reader["cost"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["cost"]);
                        osuppliertradeagreement.price = reader["price"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["price"]);
                        osuppliertradeagreement.itemcode = reader["itemcode"] == DBNull.Value ? "" : reader["itemcode"].ToString();
                        osuppliertradeagreement.code = reader["code"] == DBNull.Value ? "" : reader["code"].ToString();
                        osuppliertradeagreement.agreementname = reader["agreementname"] == DBNull.Value ? "" : reader["agreementname"].ToString();
                        osuppliertradeagreement.cur_seq = reader["cur_seq"] == DBNull.Value ? 0 : Convert.ToInt32(reader["cur_seq"]);
                        osuppliertradeagreement.barcodemode = reader["barcodemode"] == DBNull.Value ? "" : reader["barcodemode"].ToString();
                        osuppliertradeagreement.namingmode = reader["namingmode"] == DBNull.Value ? 0 : Convert.ToInt64(reader["namingmode"]);
                        osuppliertradeagreement.marginmode = reader["marginmode"] == DBNull.Value ? "" : reader["marginmode"].ToString();

                        osuppliertradeagreement.itemcost = reader["itemcost"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["itemcost"]);
                        osuppliertradeagreement.itemprice = reader["itemprice"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["itemprice"]);
                        osuppliertradeagreement.itemmrp = reader["itemmrp"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["itemmrp"]);
                        osuppliertradeagreement.itemname = reader["itemname"] == DBNull.Value ? "" : reader["itemname"].ToString();
                        osuppliertradeagreement.itemgln = reader["itemgln"] == DBNull.Value ? "" : reader["itemgln"].ToString();

                        lst.Add(osuppliertradeagreement);
                    }
                }
                conn.Close();
            }
            catch (Exception exp)
            {
                if (conn != null) conn.Close();
                WebAPI.Common.Logger.Instance(Context.OrganizationID).Log(exp, WebAPI.Common.Logger.LogType.Error, _suppliertradeagreement);
            }
            return lst;
        }

        internal Tuple<int, string> GetNextItemCode(long tradeagreementid, int noofbarcode, NpgsqlConnection conn, NpgsqlTransaction trans)
        {
            var command = new Npgsql.NpgsqlCommand(@"UPDATE suppliertradeagreement a SET cur_seq=cur_seq+@noofbarcode
                                                where id=@id
                                                RETURNING a.cur_seq,a.itemcode", conn, trans);
            command.Parameters.Add("id", NpgsqlTypes.NpgsqlDbType.Bigint).Value = tradeagreementid;
            command.Parameters.Add("noofbarcode", NpgsqlTypes.NpgsqlDbType.Integer).Value = noofbarcode;
            using (var dr = command.ExecuteReader(CommandBehavior.SingleRow))
            {
                if (dr.Read())
                {
                    var cur_seq = dr.GetInt32(0);
                    var itemcode = dr.GetString(1);
                    return new Tuple<int, string>(cur_seq, itemcode);
                }
            }
            return new Tuple<int, string>(0, "");
        }

        public Suppliertradeagreement Insert(Suppliertradeagreement oSuppliertradeagreement)
        {

            NpgsqlTransaction transaction = null;
            try
            {
                //var itemcodeformat = "[SUPPLIERCODE][PRODUCTCODE]";
                var sequence = SequenceManager.Instance(Context.OrganizationID).Get("suppliertradeagreement.code", 0);
                oSuppliertradeagreement.ChangeStatus(BaseEntity.DataStatus.Create);
                //AutonumberDataService autonumberDataService = new AutonumberDataService() { Context = this.Context };
                //var itemodeconfig = autonumberDataService.Search(new AutonumberCreteria()
                //{
                //    type = "suppliertradeagreement.itemcode"
                //});
                //if(itemodeconfig.Count>0)
                //    itemodeconfig
                using (NpgsqlConnection conn = CreateConnection())
                {
                    conn.Open();
                    transaction = conn.BeginTransaction();
                    oSuppliertradeagreement.code = sequence.GetNextNumber(Context, conn, transaction);
                    SupplierproductgroupsDataService supplierproductgroupsDataService = new SupplierproductgroupsDataService() { Context = Context }; ;
                    if (!this.IsMigration && string.IsNullOrEmpty(oSuppliertradeagreement.itemcode))
                    {
                        var itemcodedata = supplierproductgroupsDataService.GetNextItemCode(oSuppliertradeagreement.Supplierproductgroupsid, conn, transaction);
                        oSuppliertradeagreement.itemcode = itemcodedata.Item2 + itemcodedata.Item1.ToString("F0").PadLeft(4, '0');
                    }
                    Insert(transaction, conn, oSuppliertradeagreement);
                    transaction.Commit();
                    conn.Close();
                }
            }
            catch (Exception exp)
            {
                WebAPI.Common.Logger.Instance(Context.OrganizationID).Log(exp, WebAPI.Common.Logger.LogType.Error, oSuppliertradeagreement);
                if (transaction != null)
                    transaction.Rollback();
            }

            return oSuppliertradeagreement;
        }

        public Suppliertradeagreement Insert(NpgsqlTransaction transaction, NpgsqlConnection conn, Suppliertradeagreement oSuppliertradeagreement)
        {

            NpgsqlCommand command = new NpgsqlCommand(SQL_Insert, conn, transaction);
            command.Parameters.Add("isactive", NpgsqlTypes.NpgsqlDbType.Boolean).Value = oSuppliertradeagreement.isactive;
            command.Parameters.Add("createdby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oSuppliertradeagreement.createdby;
            command.Parameters.Add("modifiedby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oSuppliertradeagreement.modifiedby;
            command.Parameters.Add("version", NpgsqlTypes.NpgsqlDbType.Integer).Value = oSuppliertradeagreement.version;
            command.Parameters.Add("createdon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = oSuppliertradeagreement.createdon;
            command.Parameters.Add("modifiedon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = oSuppliertradeagreement.modifiedon;
            command.Parameters.Add("productgroupid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oSuppliertradeagreement.productgroupid;
            command.Parameters.Add("Supplierproductgroupsid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oSuppliertradeagreement.Supplierproductgroupsid;
            command.Parameters.Add("cur_seq", NpgsqlTypes.NpgsqlDbType.Integer).Value = oSuppliertradeagreement.cur_seq;
            command.Parameters.Add("mrp", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oSuppliertradeagreement.mrp;
            command.Parameters.Add("disable", NpgsqlTypes.NpgsqlDbType.Boolean).Value = oSuppliertradeagreement.disable;
            command.Parameters.Add("disabledon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = oSuppliertradeagreement.disabledon;
            command.Parameters.Add("items", NpgsqlTypes.NpgsqlDbType.Json).Value = oSuppliertradeagreement.items_json == null ? "null" : oSuppliertradeagreement.items_json;
            command.Parameters.Add("matrixlist", NpgsqlTypes.NpgsqlDbType.Json).Value = oSuppliertradeagreement.matrixlist_json == null ? "null" : oSuppliertradeagreement.matrixlist_json;
            command.Parameters.Add("supplierid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oSuppliertradeagreement.supplierid;
            command.Parameters.Add("approvedby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oSuppliertradeagreement.approvedby;
            command.Parameters.Add("brandid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oSuppliertradeagreement.brandid;
            command.Parameters.Add("agreementon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = oSuppliertradeagreement.agreementon;
            command.Parameters.Add("cost", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oSuppliertradeagreement.cost;
            command.Parameters.Add("price", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oSuppliertradeagreement.price;
            command.Parameters.Add("itemcode", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oSuppliertradeagreement.itemcode == null ? "" : oSuppliertradeagreement.itemcode;
            command.Parameters.Add("marginmode", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oSuppliertradeagreement.marginmode == null ? "" : oSuppliertradeagreement.marginmode;
            command.Parameters.Add("code", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oSuppliertradeagreement.code == null ? "" : oSuppliertradeagreement.code;
            command.Parameters.Add("agreementname", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oSuppliertradeagreement.agreementname == null ? "" : oSuppliertradeagreement.agreementname;
            command.Parameters.Add("barcodemode", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oSuppliertradeagreement.barcodemode == null ? "" : oSuppliertradeagreement.barcodemode;
            command.Parameters.Add("namingmode", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oSuppliertradeagreement.namingmode;
            using (System.Data.IDataReader reader = command.ExecuteReader())
            {
                if (reader.Read())
                {
                    oSuppliertradeagreement.id = Convert.ToInt64(reader[0].ToString());
                }
            }
            return oSuppliertradeagreement;
        }

        public Suppliertradeagreement Update(Suppliertradeagreement oSuppliertradeagreement)
        {
            return Update(oSuppliertradeagreement, false);
        }
        public Suppliertradeagreement Update(Suppliertradeagreement oSuppliertradeagreement, bool updateitems)
        {

            NpgsqlTransaction transaction = null;
            try
            {
                using (NpgsqlConnection conn = CreateConnection())
                {
                    conn.Open();
                    transaction = conn.BeginTransaction();

                    Update(transaction, conn, oSuppliertradeagreement);


                    if (updateitems)
                    {
                        var allglns = oSuppliertradeagreement.items.FindAll(x => !string.IsNullOrEmpty(x.gln));
                        var allnonglns = oSuppliertradeagreement.items.FindAll(x => string.IsNullOrEmpty(x.gln));
                        ItemsDataService itemdatasource = new ItemsDataService() { Context = this.Context };
                        var existitems = itemdatasource.Search(new ItemsCreteria()
                        {
                            tradeagreeentid = oSuppliertradeagreement.id,
                            agmtcode = oSuppliertradeagreement.itemcode
                        });

                        if (allglns.Count > 0)
                        {
                            allglns.FindAll(x => x.itemid == 0).ForEach(y =>
                            {
                                if (existitems.Exists(z => z.gln == y.gln))
                                {
                                    y.itemid = existitems.Find(z => z.gln == y.gln).id;
                                }
                            });
                        }

                        if (allnonglns.Count > 0)
                        {
                            allnonglns.FindAll(x => x.itemid == 0).ForEach(y =>
                            {
                                var existindex = existitems.FindIndex(z => z.sizeid == y.sizeid &&
                                z.colourid == y.colourid && z.materialid == y.materialid &&
                                z.typeid == y.typeid && z.patternid == y.patternid &&
                                z.sleeveid == y.sleeveid && z.fitid == y.fitid &&
                                z.styleid == y.styleid);
                                if (existindex >= 0)
                                    y.itemid = existitems[existindex].id;
                            });
                        }

                        var newitems = oSuppliertradeagreement.items.FindAll(x => x.itemid == 0);
                        var glnupdates = oSuppliertradeagreement.items.FindAll(x => x.itemid != 0 && !string.IsNullOrEmpty(x.gln));

                        foreach (var newitem in newitems)
                        {
                            newitem.itemid = itemdatasource.Insert(transaction, conn, new Models.Items()
                            {
                                productid = oSuppliertradeagreement.productgroupid,
                                brandid = oSuppliertradeagreement.brandid,
                                agmtcode = oSuppliertradeagreement.itemcode,
                                sizeid = newitem.sizeid,
                                colourid = newitem.colourid,
                                patternid = newitem.patternid,
                                styleid = newitem.styleid,
                                materialid = newitem.materialid,
                                sleeveid = newitem.sleeveid,
                                fitid = newitem.fitid,
                                typeid = newitem.typeid,
                                support_gln = true,
                                gln = newitem.gln,
                                tradeagreeentid = oSuppliertradeagreement.id
                            }).id;
                        }

                        foreach (var glnitem in glnupdates)
                        {
                            NpgsqlCommand glncommand = new NpgsqlCommand("update items set gln=@gln,printingname=@printingname,purchaserate=@purchaserate,salerate=@salerate,mrp=@mrp,version=version+1,nonuan=@nonuan where id=@id", conn, transaction);
                            glncommand.Parameters.Add("gln", NpgsqlTypes.NpgsqlDbType.Varchar).Value = string.IsNullOrEmpty(glnitem.gln) ? "" : glnitem.gln;
                            glncommand.Parameters.Add("printingname", NpgsqlTypes.NpgsqlDbType.Varchar).Value = string.IsNullOrEmpty(glnitem.printingname) ? "" : glnitem.printingname;
                            glncommand.Parameters.Add("id", NpgsqlTypes.NpgsqlDbType.Bigint).Value = glnitem.itemid;
                            glncommand.Parameters.Add("purchaserate", NpgsqlTypes.NpgsqlDbType.Numeric).Value = glnitem.cost;
                            glncommand.Parameters.Add("salerate", NpgsqlTypes.NpgsqlDbType.Numeric).Value = glnitem.price;
                            glncommand.Parameters.Add("mrp", NpgsqlTypes.NpgsqlDbType.Numeric).Value = glnitem.mrp;
                            glncommand.Parameters.Add("nonuan", NpgsqlTypes.NpgsqlDbType.Boolean).Value = glnitem.nonuan;
                            glncommand.ExecuteNonQuery();
                        }
                    }

                    transaction.Commit();
                    conn.Close();
                }
            }
            catch (Exception exp)
            {
                WebAPI.Common.Logger.Instance(Context.OrganizationID).Log(exp, WebAPI.Common.Logger.LogType.Error, oSuppliertradeagreement);
                if (transaction != null)
                    transaction.Rollback();
            }

            return oSuppliertradeagreement;
        }

        public Suppliertradeagreement Update(NpgsqlTransaction transaction, NpgsqlConnection conn, Suppliertradeagreement oSuppliertradeagreement)
        {

            NpgsqlCommand command = new NpgsqlCommand(SQL_Update, conn, transaction);
            command.Parameters.Add("id", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oSuppliertradeagreement.id;
            command.Parameters.Add("isactive", NpgsqlTypes.NpgsqlDbType.Boolean).Value = oSuppliertradeagreement.isactive;
            command.Parameters.Add("createdby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oSuppliertradeagreement.createdby;
            command.Parameters.Add("modifiedby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oSuppliertradeagreement.modifiedby;
            command.Parameters.Add("version", NpgsqlTypes.NpgsqlDbType.Integer).Value = oSuppliertradeagreement.version;
            command.Parameters.Add("createdon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = oSuppliertradeagreement.createdon;
            command.Parameters.Add("modifiedon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = oSuppliertradeagreement.modifiedon;
            command.Parameters.Add("productgroupid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oSuppliertradeagreement.productgroupid;
            command.Parameters.Add("Supplierproductgroupsid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oSuppliertradeagreement.Supplierproductgroupsid;
            command.Parameters.Add("cur_seq", NpgsqlTypes.NpgsqlDbType.Integer).Value = oSuppliertradeagreement.cur_seq;
            command.Parameters.Add("mrp", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oSuppliertradeagreement.mrp;
            command.Parameters.Add("disable", NpgsqlTypes.NpgsqlDbType.Boolean).Value = oSuppliertradeagreement.disable;
            command.Parameters.Add("disabledon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = oSuppliertradeagreement.disabledon;
            command.Parameters.Add("items", NpgsqlTypes.NpgsqlDbType.Json).Value = oSuppliertradeagreement.items_json == null ? "null" : oSuppliertradeagreement.items_json;
            command.Parameters.Add("matrixlist", NpgsqlTypes.NpgsqlDbType.Json).Value = oSuppliertradeagreement.matrixlist_json == null ? "null" : oSuppliertradeagreement.matrixlist_json;
            command.Parameters.Add("supplierid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oSuppliertradeagreement.supplierid;
            command.Parameters.Add("approvedby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oSuppliertradeagreement.approvedby;
            command.Parameters.Add("brandid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oSuppliertradeagreement.brandid;
            command.Parameters.Add("agreementon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = oSuppliertradeagreement.agreementon;
            command.Parameters.Add("cost", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oSuppliertradeagreement.cost;
            command.Parameters.Add("price", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oSuppliertradeagreement.price;
            command.Parameters.Add("itemcode", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oSuppliertradeagreement.itemcode == null ? "" : oSuppliertradeagreement.itemcode;
            command.Parameters.Add("marginmode", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oSuppliertradeagreement.marginmode == null ? "" : oSuppliertradeagreement.marginmode;
            command.Parameters.Add("code", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oSuppliertradeagreement.code == null ? "" : oSuppliertradeagreement.code;
            command.Parameters.Add("agreementname", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oSuppliertradeagreement.agreementname == null ? "" : oSuppliertradeagreement.agreementname;
            command.Parameters.Add("barcodemode", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oSuppliertradeagreement.barcodemode == null ? "" : oSuppliertradeagreement.barcodemode;
            command.Parameters.Add("namingmode", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oSuppliertradeagreement.namingmode;
            using (System.Data.IDataReader reader = command.ExecuteReader())
            {
                if (reader.Read())
                {
                    oSuppliertradeagreement.version = Convert.ToInt32(reader[0].ToString());
                }
            }

            return oSuppliertradeagreement;
        }


        public bool Delete(Suppliertradeagreement oSuppliertradeagreement)
        {

            NpgsqlTransaction transaction = null;
            bool hasSuccessCount = false;
            try
            {
                using (NpgsqlConnection conn = CreateConnection())
                {
                    conn.Open();
                    transaction = conn.BeginTransaction();
                    hasSuccessCount = Delete(transaction, conn, oSuppliertradeagreement);
                    transaction.Commit();
                    conn.Close();
                }
            }
            catch (Exception exp)
            {
                WebAPI.Common.Logger.Instance(Context.OrganizationID).Log(exp, WebAPI.Common.Logger.LogType.Error, oSuppliertradeagreement);
            }
            return hasSuccessCount;

        }

        public bool Delete(NpgsqlTransaction transaction, NpgsqlConnection conn, Suppliertradeagreement oSuppliertradeagreement)
        {

            NpgsqlCommand command = new NpgsqlCommand(SQL_Delete, conn, transaction);
            command.Parameters.Add("isactive", NpgsqlTypes.NpgsqlDbType.Boolean).Value = oSuppliertradeagreement.isactive;
            command.Parameters.Add("modifiedby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oSuppliertradeagreement.modifiedby;
            command.Parameters.Add("modifiedon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = oSuppliertradeagreement.modifiedon;
            command.Parameters.Add("version", NpgsqlTypes.NpgsqlDbType.Integer).Value = oSuppliertradeagreement.version;
            command.Parameters.Add("id", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oSuppliertradeagreement.id;
            int successCount = command.ExecuteNonQuery();

            return successCount > 0;
        }

        internal Suppliertradeagreement UpdateMatrixItems(SupplierTransactionWithMatrix item)
        {
            NpgsqlTransaction transaction = null;
            try
            {
                using (NpgsqlConnection conn = CreateConnection())
                {
                    conn.Open();
                    transaction = conn.BeginTransaction();
                    NpgsqlCommand command = null;
                    if (item.updateitemonly)
                    {
                        command = new NpgsqlCommand("update suppliertradeagreement set items=@items,version=version+1,modifiedby=@modifiedby,modifiedon=@modifiedon where id=@id", conn, transaction);
                        command.Parameters.Add("items", NpgsqlTypes.NpgsqlDbType.Json).Value = item.items_json;
                        command.Parameters.Add("id", NpgsqlTypes.NpgsqlDbType.Bigint).Value = item.id;
                        command.Parameters.Add("modifiedby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = Context.UserID;
                        command.Parameters.Add("modifiedon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = DateTimeNow;
                        command.ExecuteNonQuery();
                    }
                    else
                    {
                        if (!item.updateglnonly)
                        {
                            var allglns = item.items.FindAll(x => !string.IsNullOrEmpty(x.gln));
                            var allnonglns = item.items.FindAll(x => string.IsNullOrEmpty(x.gln));
                            ItemsDataService itemdatasource = new ItemsDataService() { Context = this.Context };
                            var existitems = itemdatasource.Search(new ItemsCreteria()
                            {
                                tradeagreeentid = item.id
                            });

                            if (allglns.Count > 0)
                            {
                                allglns.FindAll(x => x.itemid == 0).ForEach(y =>
                                {
                                    if (existitems.Exists(z => z.gln == y.gln))
                                    {
                                        y.itemid = existitems.Find(z => z.gln == y.gln).id;
                                    }
                                });
                            }

                            if (allnonglns.Count > 0)
                            {
                                allnonglns.FindAll(x => x.itemid == 0).ForEach(y =>
                                {
                                    var existindex = existitems.FindIndex(z => z.sizeid == y.sizeid &&
                                    z.colourid == y.colourid && z.materialid == y.materialid &&
                                    z.typeid == y.typeid && z.patternid == y.patternid &&
                                    z.sleeveid == y.sleeveid && z.fitid == y.fitid &&
                                    z.styleid == y.styleid);
                                    if (existindex >= 0)
                                        y.itemid = existitems[existindex].id;
                                });
                            }

                            var newitems = item.items.FindAll(x => x.itemid == 0);
                            var glnupdates = item.items.FindAll(x => x.itemid != 0 && !string.IsNullOrEmpty(x.gln));

                            foreach (var newitem in newitems)
                            {
                                newitem.itemid = itemdatasource.Insert(transaction, conn, new Models.Items()
                                {
                                    productid = item.productgroupid,
                                    brandid = item.brandid,
                                    agmtcode = item.itemcode,
                                    sizeid = newitem.sizeid,
                                    colourid = newitem.colourid,
                                    patternid = newitem.patternid,
                                    styleid = newitem.styleid,
                                    materialid = newitem.materialid,
                                    sleeveid = newitem.sleeveid,
                                    fitid = newitem.fitid,
                                    typeid = newitem.typeid,
                                    support_gln = true,
                                    gln = newitem.gln,
                                    tradeagreeentid = item.id
                                }).id;
                            }

                            if (item.barcodemode == "3" &&
                                item.items.Exists(x => string.IsNullOrEmpty(x.gln)))
                            {
                                glnupdates = item.items.FindAll(x => string.IsNullOrEmpty(x.gln));
                                var barcodeconfig = GetNextItemCode(item.id, glnupdates.Count, conn, transaction);
                                var lastbarcodeindex = barcodeconfig.Item1;
                                for (var i = 0; i < glnupdates.Count; i++)
                                {
                                    glnupdates[i].gln = barcodeconfig.Item2 + lastbarcodeindex.ToString("F0").PadLeft(4, '0');
                                    lastbarcodeindex--;
                                }
                            }

                            foreach (var glnitem in glnupdates)
                            {
                                NpgsqlCommand glncommand = new NpgsqlCommand("update items set gln=@gln,printingname=@printingname,purchaserate=@purchaserate,salerate=@salerate,mrp=@mrp,version=version+1,nonuan=@nonuan where id=@id", conn, transaction);
                                glncommand.Parameters.Add("gln", NpgsqlTypes.NpgsqlDbType.Varchar).Value = string.IsNullOrEmpty(glnitem.gln) ? "" : glnitem.gln;
                                glncommand.Parameters.Add("printingname", NpgsqlTypes.NpgsqlDbType.Varchar).Value = string.IsNullOrEmpty(glnitem.printingname) ? "" : glnitem.printingname;
                                glncommand.Parameters.Add("id", NpgsqlTypes.NpgsqlDbType.Bigint).Value = glnitem.itemid;
                                glncommand.Parameters.Add("purchaserate", NpgsqlTypes.NpgsqlDbType.Numeric).Value = glnitem.cost;
                                glncommand.Parameters.Add("salerate", NpgsqlTypes.NpgsqlDbType.Numeric).Value = glnitem.price;
                                glncommand.Parameters.Add("mrp", NpgsqlTypes.NpgsqlDbType.Numeric).Value = glnitem.mrp;
                                glncommand.Parameters.Add("nonuan", NpgsqlTypes.NpgsqlDbType.Boolean).Value = glnitem.nonuan;
                                glncommand.ExecuteNonQuery();
                            }

                            command = new NpgsqlCommand("update suppliertradeagreement set items=@items,barcodemode=@barcodemode,namingmode=@namingmode,matrixlist=@matrixlist,version=version+1,modifiedby=@modifiedby,modifiedon=@modifiedon where id=@id", conn, transaction);
                            command.Parameters.Add("items", NpgsqlTypes.NpgsqlDbType.Json).Value = item.items_json;
                            command.Parameters.Add("matrixlist", NpgsqlTypes.NpgsqlDbType.Json).Value = item.matrixlist_json;
                            command.Parameters.Add("namingmode", NpgsqlTypes.NpgsqlDbType.Bigint).Value = item.namingmode;

                            command.Parameters.Add("modifiedby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = Context.UserID;
                            command.Parameters.Add("modifiedon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = DateTimeNow;

                            command.Parameters.Add("barcodemode", NpgsqlTypes.NpgsqlDbType.Varchar).Value = string.IsNullOrEmpty(item.barcodemode) ? "" : item.barcodemode;
                            command.Parameters.Add("id", NpgsqlTypes.NpgsqlDbType.Bigint).Value = item.id;
                            command.ExecuteNonQuery();
                        }
                        else
                        {
                            command = new NpgsqlCommand("update suppliertradeagreement set items=@items,version=version+1,modifiedby=@modifiedby,modifiedon=@modifiedon where id=@id", conn, transaction);
                            command.Parameters.Add("items", NpgsqlTypes.NpgsqlDbType.Json).Value = item.items_json;
                            command.Parameters.Add("id", NpgsqlTypes.NpgsqlDbType.Bigint).Value = item.id;
                            command.Parameters.Add("modifiedby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = Context.UserID;
                            command.Parameters.Add("modifiedon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = DateTimeNow;

                            command.ExecuteNonQuery();

                            var newitems = item.items.FindAll(x => x.itemid == 0);
                            ItemsDataService itemdatasource = new ItemsDataService() { Context = this.Context };
                            foreach (var newitem in newitems)
                            {
                                newitem.itemid = itemdatasource.Insert(transaction, conn, new Models.Items()
                                {
                                    productid = item.productgroupid,
                                    brandid = item.brandid,
                                    agmtcode = item.itemcode,
                                    sizeid = newitem.sizeid,
                                    colourid = newitem.colourid,
                                    patternid = newitem.patternid,
                                    styleid = newitem.styleid,
                                    materialid = newitem.materialid,
                                    sleeveid = newitem.sleeveid,
                                    fitid = newitem.fitid,
                                    typeid = newitem.typeid,
                                    support_gln = true,
                                    gln = newitem.gln,
                                    tradeagreeentid = item.id
                                }).id;
                            }

                            foreach (var glnitem in item.items)
                            {
                                NpgsqlCommand glncommand = new NpgsqlCommand("update items set gln=@gln,printingname=@printingname,purchaserate=@purchaserate,salerate=@salerate,mrp=@mrp,version=version+1,modifiedby=@modifiedby,modifiedon=@modifiedon,nonuan=@nonuan where id=@id", conn, transaction);
                                glncommand.Parameters.Add("gln", NpgsqlTypes.NpgsqlDbType.Varchar).Value = string.IsNullOrEmpty(glnitem.gln) ? "" : glnitem.gln;
                                glncommand.Parameters.Add("printingname", NpgsqlTypes.NpgsqlDbType.Varchar).Value = string.IsNullOrEmpty(glnitem.printingname) ? "" : glnitem.printingname;
                                glncommand.Parameters.Add("id", NpgsqlTypes.NpgsqlDbType.Bigint).Value = glnitem.itemid;
                                glncommand.Parameters.Add("purchaserate", NpgsqlTypes.NpgsqlDbType.Numeric).Value = glnitem.cost;
                                glncommand.Parameters.Add("salerate", NpgsqlTypes.NpgsqlDbType.Numeric).Value = glnitem.price;
                                glncommand.Parameters.Add("mrp", NpgsqlTypes.NpgsqlDbType.Numeric).Value = glnitem.mrp;
                                glncommand.Parameters.Add("nonuan", NpgsqlTypes.NpgsqlDbType.Boolean).Value = glnitem.nonuan;
                                glncommand.Parameters.Add("modifiedby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = Context.UserID;
                                glncommand.Parameters.Add("modifiedon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = DateTimeNow;
                                glncommand.ExecuteNonQuery();
                            }
                        }
                        if (item.attributelist != null &&
                            item.attributelist.Count > 0)
                        {
                            command = new NpgsqlCommand("update Supplierproductgroups set attributelist=@attributelist,version=version+1,modifiedby=@modifiedby,modifiedon=@modifiedon where id=@id", conn, transaction);
                            command.Parameters.Add("attributelist", NpgsqlTypes.NpgsqlDbType.Json).Value = item.attributelist_json;
                            command.Parameters.Add("id", NpgsqlTypes.NpgsqlDbType.Bigint).Value = item.Supplierproductgroupsid;
                            command.Parameters.Add("modifiedby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = Context.UserID;
                            command.Parameters.Add("modifiedon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = DateTimeNow;
                            command.ExecuteNonQuery();
                        }
                    }
                    transaction.Commit();
                    conn.Close();
                }
            }
            catch (Exception exp)
            {
                WebAPI.Common.Logger.Instance(Context.OrganizationID).Log(exp, WebAPI.Common.Logger.LogType.Error, item);
                if (transaction != null)
                    transaction.Rollback();
            }
            return item;
        }
        internal bool UpdateDisable(Suppliertradeagreement oSuppliertradeagreement)
        {
            NpgsqlTransaction transaction = null;
            int successCount = 0;
            try
            {
                using (NpgsqlConnection conn = CreateConnection())
                {
                    conn.Open();
                    transaction = conn.BeginTransaction();
                    NpgsqlCommand command = new NpgsqlCommand("update suppliertradeagreement set disable=@disable,disabledon=@disabledon,revokedon=@revokedon,revokedby=@revokedby,disabledby=@disabledby,modifiedby=@modifiedby,modifiedon=@modifiedon,version=version+1 where id=@id", conn, transaction);
                    command.Parameters.Add("disable", NpgsqlTypes.NpgsqlDbType.Boolean).Value = oSuppliertradeagreement.disable;
                    command.Parameters.Add("modifiedby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = Context.UserID;
                    command.Parameters.Add("modifiedon", NpgsqlTypes.NpgsqlDbType.TimestampTZ).Value = DateTime.Now;
                    if (oSuppliertradeagreement.disable)
                    {
                        command.Parameters.Add("disabledon", NpgsqlTypes.NpgsqlDbType.TimestampTZ).Value = DateTime.Now;
                        command.Parameters.Add("disabledby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = Context.UserID;
                        command.Parameters.Add("revokedby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = 0;
                        command.Parameters.Add("revokedon", NpgsqlTypes.NpgsqlDbType.TimestampTZ).Value = DateTime.MinValue;
                    }
                    else
                    {
                        command.Parameters.Add("disabledon", NpgsqlTypes.NpgsqlDbType.TimestampTZ).Value = DateTime.MinValue;
                        command.Parameters.Add("disabledby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = 0;
                        command.Parameters.Add("revokedby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = Context.UserID;
                        command.Parameters.Add("revokedon", NpgsqlTypes.NpgsqlDbType.TimestampTZ).Value = DateTime.Now;
                    }
                    command.Parameters.Add("id", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oSuppliertradeagreement.id;
                    successCount = command.ExecuteNonQuery();
                    transaction.Commit();
                    conn.Close();
                }
            }
            catch (Exception exp)
            {
                WebAPI.Common.Logger.Instance(Context.OrganizationID).Log(exp, WebAPI.Common.Logger.LogType.Error, oSuppliertradeagreement);
            }
            return successCount > 0;
        }

        internal Suppliertradeagreement.MatrixItem CloneMatrixItems(Suppliertradeagreement.MatrixItem item)
        {
            ItemsDataService itemdatasource = new ItemsDataService() { Context = this.Context };
            using (NpgsqlConnection conn = CreateConnection())
            {
                conn.Open();
                var transaction = conn.BeginTransaction();
                var newitem = itemdatasource.Search(new ItemsCreteria()
                {
                    id = item.itemid
                }).First();
                newitem.gln = "";
                item.itemid = itemdatasource.Insert(transaction, conn, newitem).id;
                transaction.Commit();
                conn.Close();
            }
            return item;
        }
    }

}
