using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using InfyPOS.Models;
using InfyPOS.Common;
using Npgsql;
namespace InfyPOS.Data

{
    public class DailypriceDataService : BaseData
    {
        const string SQL_Search = @"select modifiedby, isactive, version, id, modifiedon, createdon, createdby, priceon, employeeid, items, companyid, locationid,
        pricegroupid,appliedon,appliedby,appliedrows from dailyprice where id=@id";

        const string SQL_SearchProducts = @"select modifiedby, isactive, version, id, modifiedon, createdon, createdby, priceon, employeeid, companyid, locationid,
        pricegroupid,appliedon,appliedby,appliedrows,
		cast(schemelist->>'lastprice' as numeric) lastprice,
		cast(schemelist->>'newprice' as numeric) lastprice,
		schemelist->>'barcode' barcode,
		schemelist->>'itemcode' itemcode,
		schemelist->>'description' description
		from dailyprice
		LEFT JOIN LATERAL
        json_array_elements (dailyprice.items :: json) schemelist ON true
        where id=@id";

        const string SQL_Insert = @"INSERT INTO public.dailyprice(modifiedby, isactive, version, modifiedon, createdon, createdby, priceon, employeeid, items, companyid, locationid, pricegroupid) VALUES(@modifiedby, @isactive, @version, @modifiedon, @createdon, @createdby, @priceon, @employeeid, @items, @companyid, @locationid, @pricegroupid) RETURNING id";

        const string SQL_Update = @"UPDATE public.dailyprice SET modifiedby=@modifiedby, version=@version+1, modifiedon=@modifiedon, priceon=@priceon, employeeid=@employeeid, items=@items, companyid=@companyid, locationid=@locationid, pricegroupid=@pricegroupid where id=@id and version=@version RETURNING version";

        const string SQL_Delete = @"update public.dailyprice set isactive=false,version=@version+1,modifiedby=@modifiedby,modifiedon=@modifiedon where id=@id and version=@version";

        public List<DailyPriceDetail> Search(DailypriceCreteria _dailyprice)
        {
            List<DailyPriceDetail> lst = new List<DailyPriceDetail>();
            NpgsqlConnection conn = CreateConnection();
            try
            {
                conn.Open();

                NpgsqlCommand command = null;
                if (_dailyprice.id != 0)
                {
                    command = new NpgsqlCommand(SQL_Search, conn);
                    command.Parameters.Add("id", NpgsqlTypes.NpgsqlDbType.Bigint).Value = _dailyprice.id;
                }
                else if (_dailyprice.productid > 0)
                {
                    QueryBuilder qb = new QueryBuilder();
                    qb.CurrentQuery = SQL_SearchProducts.Substring(0, SQL_Search.IndexOf("where"));
                    qb.AddParameter("isactive", true, "=");

                    if (_dailyprice.locationid > 0)
                        qb.AddParameter("locationid", _dailyprice.locationid, "=");

                    if (_dailyprice.companyid > 0)
                        qb.AddParameter("companyid", _dailyprice.companyid, "=");

                    if (_dailyprice.pricegroupid > 0)
                        qb.AddParameter("pricegroupid", _dailyprice.pricegroupid, "=");

                    if (_dailyprice.priceon.Year > 2000)
                        qb.AddParameter("priceon", _dailyprice.priceon.Date, "=");

                    if (_dailyprice.fromdate.Year > 2000)
                        qb.AddParameter("priceon", _dailyprice.fromdate.Date, ">=");

                    if (_dailyprice.todate.Year > 2000)
                        qb.AddParameter("priceon", _dailyprice.todate.AddDays(1).Date, "<");

                    qb.AddParameter("schemelist->>'productid'", "productid", _dailyprice.productid.ToString(), "=");
                    command = qb.GetCommand(conn);
                }
                else
                {
                    QueryBuilder qb = new QueryBuilder();
                    qb.CurrentQuery = SQL_Search.Substring(0, SQL_Search.IndexOf("where"));
                    qb.AddParameter("isactive", true, "=");

                    if (_dailyprice.locationid > 0)
                        qb.AddParameter("locationid", _dailyprice.locationid, "=");

                    if (_dailyprice.companyid > 0)
                        qb.AddParameter("companyid", _dailyprice.companyid, "=");

                    if (_dailyprice.pricegroupid > 0)
                        qb.AddParameter("pricegroupid", _dailyprice.pricegroupid, "=");

                    if (_dailyprice.priceon.Year > 2000)
                        qb.AddParameter("priceon", _dailyprice.priceon.Date, "=");

                    if (_dailyprice.fromdate.Year > 2000)
                        qb.AddParameter("priceon", _dailyprice.fromdate.Date, ">=");

                    if (_dailyprice.todate.Year > 2000)
                        qb.AddParameter("priceon", _dailyprice.todate.AddDays(1).Date, "<");

                    command = qb.GetCommand(conn);
                }

                using (System.Data.IDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        DailyPriceDetail odailyprice = new DailyPriceDetail();
                        odailyprice.modifiedby = reader["modifiedby"] == DBNull.Value ? 0 : Convert.ToInt64(reader["modifiedby"]);
                        odailyprice.isactive = reader["isactive"] == DBNull.Value ? false : Convert.ToBoolean(reader["isactive"]);
                        odailyprice.version = reader["version"] == DBNull.Value ? 0 : Convert.ToInt32(reader["version"]);
                        odailyprice.id = reader["id"] == DBNull.Value ? 0 : Convert.ToInt64(reader["id"]);
                        odailyprice.modifiedon = reader["modifiedon"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(reader["modifiedon"]);
                        odailyprice.createdon = reader["createdon"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(reader["createdon"]);
                        odailyprice.createdby = reader["createdby"] == DBNull.Value ? 0 : Convert.ToInt64(reader["createdby"]);
                        odailyprice.priceon = reader["priceon"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(reader["priceon"]);
                        odailyprice.employeeid = reader["employeeid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["employeeid"]);
                        if (!_dailyprice.searchonly)
                        {
                            odailyprice.items_json = reader["items"] == DBNull.Value ? null : reader["items"].ToString();
                        }
                        odailyprice.companyid = reader["companyid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["companyid"]);
                        odailyprice.locationid = reader["locationid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["locationid"]);
                        odailyprice.pricegroupid = reader["pricegroupid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["pricegroupid"]);
                        odailyprice.appliedby = reader["appliedby"] == DBNull.Value ? 0 : Convert.ToInt64(reader["appliedby"]);
                        odailyprice.appliedon = reader["appliedon"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(reader["appliedon"]);
                        odailyprice.appliedrows = reader["appliedrows"] == DBNull.Value ? 0 : Convert.ToInt32(reader["appliedrows"]);
                        if(_dailyprice.productid>0)
                        {
                            odailyprice.lastprice = reader["lastprice"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["lastprice"]);
                            odailyprice.newprice = reader["newprice"] == DBNull.Value ? 0 : Convert.ToInt64(reader["newprice"]);
                            odailyprice.barcode = reader["barcode"] == DBNull.Value ? "" : Convert.ToString(reader["barcode"]);
                            odailyprice.itemcode = reader["itemcode"] == DBNull.Value ? "" : Convert.ToString(reader["itemcode"]);
                            odailyprice.description = reader["description"] == DBNull.Value ? "" : Convert.ToString(reader["description"]);
                        }
                        lst.Add(odailyprice);
                    }
                }
                conn.Close();
            }
            catch (Exception exp)
            {
                if (conn != null) conn.Close();
                WebAPI.Common.Logger.Instance(Context.OrganizationID).Log(exp, WebAPI.Common.Logger.LogType.Error, _dailyprice);
            }
            return lst;
        }

        public Dailyprice Insert(Dailyprice oDailyprice)
        {

            NpgsqlTransaction transaction = null;
            try
            {
                using (NpgsqlConnection conn = CreateConnection())
                {
                    conn.Open();
                    transaction = conn.BeginTransaction();
                    NpgsqlCommand command = new NpgsqlCommand(SQL_Insert, conn, transaction);

                    Insert(transaction, conn, oDailyprice);
                    transaction.Commit();
                    conn.Close();
                }
            }
            catch (Exception exp)
            {
                WebAPI.Common.Logger.Instance(Context.OrganizationID).Log(exp, WebAPI.Common.Logger.LogType.Error, oDailyprice);
            }

            return oDailyprice;
        }

        public Dailyprice Insert(NpgsqlTransaction transaction, NpgsqlConnection conn, Dailyprice oDailyprice)
        {

            NpgsqlCommand command = new NpgsqlCommand(SQL_Insert, conn, transaction);
            command.Parameters.Add("modifiedby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oDailyprice.modifiedby;
            command.Parameters.Add("isactive", NpgsqlTypes.NpgsqlDbType.Boolean).Value = oDailyprice.isactive;
            command.Parameters.Add("version", NpgsqlTypes.NpgsqlDbType.Integer).Value = oDailyprice.version;
            command.Parameters.Add("modifiedon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = oDailyprice.modifiedon;
            command.Parameters.Add("createdon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = oDailyprice.createdon;
            command.Parameters.Add("createdby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oDailyprice.createdby;
            command.Parameters.Add("priceon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = oDailyprice.priceon.Date;
            command.Parameters.Add("employeeid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oDailyprice.employeeid;
            command.Parameters.Add("items", NpgsqlTypes.NpgsqlDbType.Json).Value = oDailyprice.items_json == null ? "null" : oDailyprice.items_json;
            command.Parameters.Add("companyid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oDailyprice.companyid;
            command.Parameters.Add("locationid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oDailyprice.locationid;
            command.Parameters.Add("pricegroupid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oDailyprice.pricegroupid;
            using (System.Data.IDataReader reader = command.ExecuteReader())
            {
                if (reader.Read())
                {
                    oDailyprice.id = Convert.ToInt64(reader[0].ToString());
                }
            }
            return oDailyprice;
        }

        public Dailyprice Update(Dailyprice oDailyprice)
        {

            NpgsqlTransaction transaction = null;
            try
            {
                using (NpgsqlConnection conn = CreateConnection())
                {
                    conn.Open();
                    transaction = conn.BeginTransaction();

                    Update(transaction, conn, oDailyprice);
                    transaction.Commit();
                    conn.Close();
                }
            }
            catch (Exception exp)
            {
                WebAPI.Common.Logger.Instance(Context.OrganizationID).Log(exp, WebAPI.Common.Logger.LogType.Error, oDailyprice);
            }

            return oDailyprice;
        }

        public Dailyprice Update(NpgsqlTransaction transaction, NpgsqlConnection conn, Dailyprice oDailyprice)
        {

            NpgsqlCommand command = new NpgsqlCommand(SQL_Update, conn, transaction);
            command.Parameters.Add("modifiedby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oDailyprice.modifiedby;
            command.Parameters.Add("isactive", NpgsqlTypes.NpgsqlDbType.Boolean).Value = oDailyprice.isactive;
            command.Parameters.Add("version", NpgsqlTypes.NpgsqlDbType.Integer).Value = oDailyprice.version;
            command.Parameters.Add("id", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oDailyprice.id;
            command.Parameters.Add("modifiedon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = oDailyprice.modifiedon;
            command.Parameters.Add("createdon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = oDailyprice.createdon;
            command.Parameters.Add("createdby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oDailyprice.createdby;
            command.Parameters.Add("priceon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = oDailyprice.priceon.Date;
            command.Parameters.Add("employeeid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oDailyprice.employeeid;
            command.Parameters.Add("items", NpgsqlTypes.NpgsqlDbType.Json).Value = oDailyprice.items_json == null ? "null" : oDailyprice.items_json;
            command.Parameters.Add("companyid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oDailyprice.companyid;
            command.Parameters.Add("locationid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oDailyprice.locationid;
            command.Parameters.Add("pricegroupid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oDailyprice.pricegroupid;
            using (System.Data.IDataReader reader = command.ExecuteReader())
            {
                if (reader.Read())
                {
                    oDailyprice.version = Convert.ToInt32(reader[0].ToString());
                }
            }

            return oDailyprice;
        }


        public bool Delete(Dailyprice oDailyprice)
        {

            NpgsqlTransaction transaction = null;
            bool hasSuccessCount = false;
            try
            {
                using (NpgsqlConnection conn = CreateConnection())
                {
                    conn.Open();
                    transaction = conn.BeginTransaction();
                    hasSuccessCount = Delete(transaction, conn, oDailyprice);
                    transaction.Commit();
                    conn.Close();
                }
            }
            catch (Exception exp)
            {
                WebAPI.Common.Logger.Instance(Context.OrganizationID).Log(exp, WebAPI.Common.Logger.LogType.Error, oDailyprice);
            }
            return hasSuccessCount;

        }

        public bool Delete(NpgsqlTransaction transaction, NpgsqlConnection conn, Dailyprice oDailyprice)
        {

            NpgsqlCommand command = new NpgsqlCommand(SQL_Delete, conn, transaction);
            command.Parameters.Add("isactive", NpgsqlTypes.NpgsqlDbType.Boolean).Value = oDailyprice.isactive;
            command.Parameters.Add("modifiedby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oDailyprice.modifiedby;
            command.Parameters.Add("modifiedon", NpgsqlTypes.NpgsqlDbType.TimestampTZ).Value = oDailyprice.modifiedon;
            command.Parameters.Add("version", NpgsqlTypes.NpgsqlDbType.Integer).Value = oDailyprice.version;
            command.Parameters.Add("id", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oDailyprice.id;
            int successCount = command.ExecuteNonQuery();

            return successCount > 0;
        }

        internal Dailyprice NewPriceList(DailypriceCreteria item)
        {
            List<Dailyprice.Items> response = new List<Dailyprice.Items>();
            try
            {
                using (NpgsqlConnection conn = CreateConnection())
                {
                    conn.Open();
                    string query = @"select schemelist->>'gln' barcode,
                            schemelist->>'printingname' description,
                            suppliertradeagreement.id agreementid,agreementname,
                            suppliertradeagreement.itemcode,products.id productid from supplierproductgroups
                            inner join products on products.id = supplierproductgroups.productgroupid and products.isdailyprice = true
                            inner join suppliertradeagreement on suppliertradeagreement.Supplierproductgroupsid = supplierproductgroups.id
                            LEFT JOIN LATERAL json_array_elements (suppliertradeagreement.items :: json) schemelist ON true
                            where products.isdailyprice = true 
                            and supplierproductgroups.isactive=true and products.isactive=true
                            and suppliertradeagreement.disable=false
							and suppliertradeagreement.items is not null
							and suppliertradeagreement.items->0 is not null
                            and products.basepriceid=@basepriceid";

                    NpgsqlCommand command = new NpgsqlCommand(query, conn);
                    command.Parameters.Add("basepriceid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = item.pricegroupid;

                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            Dailyprice.Items items = new Dailyprice.Items();
                            items.barcode = reader["barcode"] == DBNull.Value ? "" : reader["barcode"].ToString();
                            items.description = reader["description"] == DBNull.Value ? "" : reader["description"].ToString();
                            items.itemcode = reader["itemcode"] == DBNull.Value ? "" : reader["itemcode"].ToString();
                            items.agreementname = reader["agreementname"] == DBNull.Value ? "" : reader["agreementname"].ToString();
                            items.agreementid = reader["agreementid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["agreementid"]);
                            items.productid = reader["productid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["productid"]);
                            items.stockexist = false;
                            if(!response.Exists(x=>x.barcode == items.barcode))
                                response.Add(items);
                        }
                    }

                    if (response.Count > 0)
                    {
                        string currentpricequery = string.Format(@";with currentprice as
                    (
                    select stock.productid,stock.serialno barcode,salablegoods.locationid,salablegoods.id salablegoodsid,
	                salablegoods.companyid,salablegoods.price,salablegoods.iqty - (salablegoods.oqty+salablegoods.jqty+salablegoods.dqty) stockqty,
                    row_number() OVER (PARTITION BY stock.serialno ORDER BY salablegoods.id desc) rowid
                    from salablegoods
                    inner join stock on stock.id = salablegoods.stockid
                    where stock.serialno in ({0}) and salablegoods.locationid=@locationid and salablegoods.companyid=@companyid
                    and salablegoods.iqty - (salablegoods.oqty+salablegoods.jqty+salablegoods.dqty)>0
                    order by salablegoods.id desc
                    )
                    select * from currentprice where rowid=1",
                         string.Join(",", response.ConvertAll(x => "'" + x.barcode + "'"))
                        );
                        command = new NpgsqlCommand(currentpricequery, conn);
                        command.Parameters.Add("locationid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = item.locationid;
                        command.Parameters.Add("companyid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = item.companyid;
                        using (var reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                var barcode = reader["barcode"] == DBNull.Value ? "" : reader["barcode"].ToString();
                                Dailyprice.Items items = response.Find(x => x.barcode == barcode);
                                if (items != null)
                                {
                                    if (!items.stockexist)
                                    {
                                        items.stockexist = true;
                                        items.lastprice = reader["price"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["price"]);
                                        items.stock = new List<Dailyprice.Items.StockInfo>();
                                        items.stock.Add(new Dailyprice.Items.StockInfo()
                                        {
                                            id = Convert.ToInt64(reader["salablegoodsid"]),
                                            qty = Convert.ToInt64(reader["stockqty"]),
                                        });
                                    }
                                    //items.locationid = reader["locationid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["locationid"]);
                                }
                            }
                        }
                    }
                    conn.Close();
                }

                var existprice = Search(new DailypriceCreteria()
                {
                    locationid = item.locationid,
                    companyid = item.companyid,
                    priceon = item.priceon.Date,
                    pricegroupid = item.pricegroupid
                });
                if (existprice != null &&
                    existprice.Count > 0)
                {
                    var param = new SortedList<string, object>();
                    param.Add("priceon", item.priceon.Date);
                    param.Add("pricegroupid", item.pricegroupid);

                    using (var connection = CreateConnection())
                    {
                        connection.Open();
                        NpgsqlCommand command = new NpgsqlCommand("select locationid,applied,appliedrows,id from Dailyprice where priceon=@priceon and pricegroupid=@pricegroupid", connection);
                        command.Parameters.Add("pricegroupid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = item.pricegroupid;
                        command.Parameters.Add("priceon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = item.priceon.Date;

                        using (var reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                if (existprice[0].priceavailablelocations == null)
                                    existprice[0].priceavailablelocations = new List<Dailyprice.AppliedInfo>();

                                Dailyprice.AppliedInfo appliedInfo = new Dailyprice.AppliedInfo();
                                appliedInfo.locationid = reader.GetInt64(0);
                                appliedInfo.applied = reader.IsDBNull(1) ? false : reader.GetBoolean(1);
                                appliedInfo.rows = reader.IsDBNull(2) ? 0 : reader.GetInt64(2);
                                appliedInfo.existpriceid = reader.GetInt64(3);
                                existprice[0].priceavailablelocations.Add(appliedInfo);
                            }
                        }
                        connection.Close();
                    }

                    if (existprice[0].items == null) existprice[0].items = new List<Dailyprice.Items>();


                    foreach(var grp in existprice[0].items.GroupBy(x=>x.barcode).ToList())
                    {
                        if(grp.Count()>1)
                        {
                            var grps = grp.ToList();
                            for (var i = 1; i < grp.Count(); i++)
                            {
                                existprice[0].items.Remove(grps[i]);
                            }
                        }
                    }

                    foreach (var exitem in response)
                    {
                        if(existprice[0].items.Exists(x=>x.barcode == exitem.barcode))
                        {
                            var existitem = existprice[0].items.Find(x => x.barcode == exitem.barcode);
                            existitem.description = exitem.description;
                            existitem.agreementname = exitem.agreementname;
                            existitem.agreementid = exitem.agreementid;
                            existitem.productid = exitem.productid;
                            existitem.itemcode = exitem.itemcode;
                            existitem.lastprice = exitem.lastprice;
                            existitem.stockexist = exitem.stockexist;
                            existitem.stock = exitem.stock;
                        }
                        else
                        {
                            existprice[0].items.Add(exitem);
                        }
                    }
                    //var newitems = response.FindAll(x => !existprice[0].items.Exists(y => y.barcode == x.barcode));
                    //existprice[0].items.AddRange(newitems);
                    return existprice[0];
                }
                else
                {
                    item.items = response;
                    item.isactive = true;
                    item.createdby = Context.UserID;
                    item.modifiedby = Context.UserID;
                    item.createdon = DateTime.Now;
                    item.modifiedon = DateTime.Now;
                    return item;
                }
            }
            catch (Exception exp)
            {
                WebAPI.Common.Logger.Instance(Context.OrganizationID).Log(exp, WebAPI.Common.Logger.LogType.Error, item);
            }
            return new Dailyprice();
        }

        internal bool Apply(DailypriceCreteria item)
        {
            //List<Dailyprice> applylist = new List<Dailyprice>();
            var param = new SortedList<string, object>();
            param.Add("priceon", item.priceon.Date);
            //using (var reader = ExecuteReader("select locationid,id,companyid from Dailyprice where priceon=@priceon", param))
            //{
            //    while (reader.Read())
            //    {
            //        Dailyprice dailyprice = new Dailyprice();
            //        dailyprice.locationid = reader.GetInt64(0);
            //        dailyprice.id = reader.GetInt64(1);
            //        dailyprice.companyid = reader.GetInt64(2);
            //        applylist.Add(dailyprice);
            //    }
            //}

            string updatepricequery = @";with allnewprices
                as
                (
                    select cast(schemelist->>'newprice' as numeric) newprice,
	                cast(schemelist->>'barcode' as varchar) barcode,
	                locationid,
	                id from dailyprice
                    LEFT JOIN LATERAL json_array_elements (dailyprice.items :: json) schemelist ON true
                    where id=@id and cast(schemelist->>'newprice' as numeric)>0
                )
                update salablegoods set price=allnewprices.newprice from allnewprices
                inner join stock on stock.serialno = allnewprices.barcode
                where salablegoods.stockid = stock.id 
                and salablegoods.locationid = allnewprices.locationid";

            if (InfyPOS.Services.ConfigManager.Instance(Context.OrganizationID).Config.system_hyper_uan)
            {
                updatepricequery = @";with allnewprices
                as
                (
                    select cast(schemelist->>'newprice' as numeric) newprice,
	                cast(schemelist->>'barcode' as varchar) barcode,
	                locationid,
	                id from dailyprice
                    LEFT JOIN LATERAL json_array_elements (dailyprice.items :: json) schemelist ON true
                    where id=@id and cast(schemelist->>'newprice' as numeric)>0
                )
                update salablegoods set price=allnewprices.newprice from allnewprices
                where salablegoods.barcode = allnewprices.barcode 
                and salablegoods.locationid = allnewprices.locationid";
            }

            string updatedailyquery = "Update dailyprice set appliedrows=@appliedrows,appliedon=@appliedon,appliedby=@appliedby,applied=true,version=version+1,modifiedon=@modifiedon,modifiedby=@modifiedby where id=@id";

            NpgsqlTransaction transaction = null;
            try
            {
                using (NpgsqlConnection conn = CreateConnection())
                {
                    conn.Open();
                    transaction = conn.BeginTransaction();
                    foreach (var applylocation in item.applylocations)
                    {

                        NpgsqlCommand command = new NpgsqlCommand(updatepricequery, conn, transaction);
                        command.Parameters.Add("id", NpgsqlTypes.NpgsqlDbType.Bigint).Value = applylocation.existpriceid;
                        var updateedrows = command.ExecuteNonQuery();

                        command = new NpgsqlCommand(updatedailyquery, conn, transaction);
                        command.Parameters.Add("modifiedby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = Context.UserID;
                        command.Parameters.Add("modifiedon", NpgsqlTypes.NpgsqlDbType.TimestampTZ).Value = DateTime.Now;
                        command.Parameters.Add("id", NpgsqlTypes.NpgsqlDbType.Bigint).Value = applylocation.existpriceid;
                        command.Parameters.Add("appliedrows", NpgsqlTypes.NpgsqlDbType.Bigint).Value = updateedrows;
                        command.Parameters.Add("appliedon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = DateTime.Now;
                        command.Parameters.Add("appliedby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = Context.UserID;
                        command.ExecuteNonQuery();

                    }
                    transaction.Commit();
                    conn.Close();
                }
            }
            catch (Exception exp)
            {
                WebAPI.Common.Logger.Instance(Context.OrganizationID).Log(exp, WebAPI.Common.Logger.LogType.Error, item);
                return false;
            }

            return true;
        }

        internal bool Copy(DailypriceCreteria item)
        {
            NpgsqlTransaction transaction = null;
            try
            {
                using (NpgsqlConnection conn = CreateConnection())
                {
                    conn.Open();
                    transaction = conn.BeginTransaction();
                    NpgsqlCommand command = new NpgsqlCommand(SQL_Insert, conn, transaction);
                    foreach (var copylocation in item.copylocations)
                    {
                        item.locationid = copylocation.locationid;
                        item.createdby = Context.UserID;
                        item.createdon = DateTime.Now;
                        item.modifiedby = Context.UserID;
                        item.modifiedon = DateTime.Now;
                        item.id = 0;
                        item.version = 1;
                        if (copylocation.existpriceid > 0)
                        {
                            item.id = copylocation.existpriceid;
                            Update(transaction, conn, item);
                        }
                        else
                        {
                            Insert(transaction, conn, item);
                        }
                    }
                    transaction.Commit();
                    conn.Close();
                }
            }
            catch (Exception exp)
            {
                WebAPI.Common.Logger.Instance(Context.OrganizationID).Log(exp, WebAPI.Common.Logger.LogType.Error, item);
            }
            return true;
        }
    }

}
