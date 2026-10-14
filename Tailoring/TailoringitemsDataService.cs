using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using InfyPOS.Models;
using InfyPOS.Common;
using Npgsql;
using WebAPI.Models;
using Newtonsoft.Json;
using System.Management.Instrumentation;
namespace InfyPOS.Data

{
    public class TailoringitemsDataService : BaseData
    {
        const string SQL_SearchAll = @"select id, tailoringorderid, sourceproductid, outcomeproductid, sourceqty,issuedqty,sourcemeter, expectedqty, receivedqty,additionalchargeid,
        additionalchargeamount, returnqty, deliveredqty, chargeid, charge, gross, tax, total, designname, designid, measurementname, measurementid,barcodes, measurement, createdon, 
        version, isactive, modifiedby, createdby, modifiedon,product,tailorid,delayperday,delaymaximum,chargeonpriority,delaycharge,prioritycharge,priorityqty from tailoringitems 
        where {0} isactive=true and tailoringorderid=@tailoringorderid";

        const string SQL_Insert = @"INSERT INTO public.tailoringitems(tailoringorderid, sourceproductid, outcomeproductid, sourceqty,issuedqty,sourcemeter, expectedqty, receivedqty,
        additionalchargeid,additionalchargeamount, returnqty, deliveredqty, chargeid, charge, gross, tax, total, designname, designid, measurementname, measurementid,barcodes, measurement, 
        createdon, version, isactive, modifiedby, createdby, modifiedon,product,tailorid,delayperday,delaymaximum,chargeonpriority,delaycharge,prioritycharge,priorityqty) 
        VALUES(@tailoringorderid, @sourceproductid, @outcomeproductid, @sourceqty,@issuedqty,@sourcemeter, @expectedqty, @receivedqty,@additionalchargeid,@additionalchargeamount, 
        @returnqty, @deliveredqty, @chargeid, @charge, @gross, @tax, @total, @designname, @designid, @measurementname, @measurementid,@barcodes, @measurement, @createdon, @version,
        @isactive, @modifiedby, @createdby, @modifiedon,@product,@tailorid,@delayperday,@delaymaximum,@chargeonpriority,@delaycharge,@prioritycharge,@priorityqty) RETURNING id";


        const string SQL_Update = @"UPDATE public.tailoringitems SET tailoringorderid=@tailoringorderid, sourceproductid=@sourceproductid, outcomeproductid=@outcomeproductid, sourceqty=@sourceqty,issuedqty=@issuedqty,sourcemeter=@sourcemeter, expectedqty=@expectedqty, 
        receivedqty=@receivedqty, returnqty=@returnqty, deliveredqty=@deliveredqty, chargeid=@chargeid, charge=@charge,additionalchargeid=@additionalchargeid,additionalchargeamount=@additionalchargeamount,
        gross=@gross, tax=@tax, total=@total, designname=@designname, designid=@designid, product=@product,tailorid=@tailorid, measurementname=@measurementname, measurementid=@measurementid,
        barcodes = @barcodes, measurement=@measurement, version=@version+1, modifiedby=@modifiedby, modifiedon=@modifiedon,delayperday=@delayperday,delaymaximum=@delaymaximum,chargeonpriority=@chargeonpriority,
        delaycharge=@delaycharge,prioritycharge=@prioritycharge,priorityqty=@priorityqty where id=@id and version=@version RETURNING version";

        const string SQL_Delete = @"update public.tailoringitems set isactive=false,version=@version+1,modifiedby=@modifiedby,modifiedon=@modifiedon where id=@id and version=@version";
        const string SQL_DeleteAll = @"update public.tailoringitems set isactive=false,modifiedby=@modifiedby,modifiedon=@modifiedon where tailoringorderid=@tailoringorderid";

        public List<Tailoringitems> SearchAll(long _tailoringorderid)
        {
            return SearchAll(_tailoringorderid, null, false);
        }
        public List<Tailoringitems> SearchAll(long _tailoringorderid, List<long> _id, bool isoutward)
        {
            List<Tailoringitems> lst = new List<Tailoringitems>();
            NpgsqlConnection conn = CreateConnection();
            try
            {
                conn.Open();
                var query = string.Format(SQL_SearchAll, (isoutward ? " deliveredqty<expectedqty and " : ""));
                if (_id != null && _id.Count > 0)
                    query = query.Replace("tailoringorderid=@tailoringorderid", "id in (" + string.Join(",", _id)+")");

                NpgsqlCommand command = new NpgsqlCommand(query, conn);
                command.Parameters.Add("tailoringorderid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = _tailoringorderid;
                using (System.Data.IDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        Tailoringitems otailoringitems = new Tailoringitems();
                        otailoringitems.id = reader["id"] == DBNull.Value ? 0 : Convert.ToInt64(reader["id"]);
                        otailoringitems.tailoringorderid = reader["tailoringorderid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["tailoringorderid"]);
                        otailoringitems.sourceproductid = reader["sourceproductid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["sourceproductid"]);
                        otailoringitems.tailorid = reader["tailorid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["tailorid"]);
                        otailoringitems.outcomeproductid = reader["outcomeproductid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["outcomeproductid"]);
                        otailoringitems.sourceqty = reader["sourceqty"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["sourceqty"]);
                        otailoringitems.issuedqty = reader["issuedqty"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["issuedqty"]);
                        otailoringitems.sourcemeter = reader["sourcemeter"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["sourcemeter"]);
                        otailoringitems.expectedqty = reader["expectedqty"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["expectedqty"]);
                        otailoringitems.receivedqty = reader["receivedqty"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["receivedqty"]);
                        otailoringitems.additionalchargeamount = reader["additionalchargeamount"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["additionalchargeamount"]);
                        otailoringitems.additionalchargeid = reader["additionalchargeid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["additionalchargeid"]);

                        otailoringitems.sourceproductid = reader["sourceproductid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["sourceproductid"]);

                        otailoringitems.deliveredqty = reader["deliveredqty"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["deliveredqty"]);
                        otailoringitems.charge = reader["charge"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["charge"]);

                        otailoringitems.delayperday = reader["delayperday"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["delayperday"]);
                        otailoringitems.delaymaximum = reader["delaymaximum"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["delaymaximum"]);
                        otailoringitems.chargeonpriority = reader["chargeonpriority"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["chargeonpriority"]);
                        otailoringitems.delaycharge = reader["delaycharge"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["delaycharge"]);
                        otailoringitems.prioritycharge = reader["prioritycharge"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["prioritycharge"]);
                        otailoringitems.priorityqty = reader["priorityqty"] == DBNull.Value ? 0 : Convert.ToInt32(reader["priorityqty"]);
                        
                        otailoringitems.gross = reader["gross"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["gross"]);
                        otailoringitems.tax = reader["tax"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["tax"]);
                        otailoringitems.total = reader["total"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["total"]);

                        otailoringitems.chargeid = reader["chargeid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["chargeid"]);
                        otailoringitems.designname = reader["designname"] == DBNull.Value ? "" : reader["designname"].ToString();
                        otailoringitems.product = reader["product"] == DBNull.Value ? "" : reader["product"].ToString();
                        otailoringitems.designid = reader["designid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["designid"]);
                        otailoringitems.measurementname = reader["measurementname"] == DBNull.Value ? "" : reader["measurementname"].ToString();
                        otailoringitems.measurementid = reader["measurementid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["measurementid"]);
                        otailoringitems.measurement_json = reader["measurement"] == DBNull.Value ? null : reader["measurement"].ToString();

                        otailoringitems.barcodes_json = reader["barcodes"] == DBNull.Value ? "" : reader["barcodes"].ToString();

                        otailoringitems.createdon = reader["createdon"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(reader["createdon"]);
                        otailoringitems.version = reader["version"] == DBNull.Value ? 0 : Convert.ToInt32(reader["version"]);
                        otailoringitems.isactive = reader["isactive"] == DBNull.Value ? false : Convert.ToBoolean(reader["isactive"]);
                        otailoringitems.modifiedby = reader["modifiedby"] == DBNull.Value ? 0 : Convert.ToInt64(reader["modifiedby"]);
                        otailoringitems.createdby = reader["createdby"] == DBNull.Value ? 0 : Convert.ToInt64(reader["createdby"]);
                        otailoringitems.modifiedon = reader["modifiedon"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(reader["modifiedon"]);
                        lst.Add(otailoringitems);
                    }
                }
                conn.Close();
            }
            catch (Exception exp)
            {
                if (conn != null) conn.Close();
                WebAPI.Common.Logger.Instance(Context.OrganizationID).Log(exp, WebAPI.Common.Logger.LogType.Error, _tailoringorderid);
            }
            return lst;
        }

        public Tailoringitems Insert(Tailoringitems oTailoringitems)
        {

            NpgsqlTransaction transaction = null;
            try
            {
                using (NpgsqlConnection conn = CreateConnection())
                {
                    conn.Open();
                    transaction = conn.BeginTransaction();
                    NpgsqlCommand command = new NpgsqlCommand(SQL_Insert, conn, transaction);

                    Insert(transaction, conn, oTailoringitems);
                    transaction.Commit();
                    conn.Close();
                }
            }
            catch (Exception exp)
            {
                WebAPI.Common.Logger.Instance(Context.OrganizationID).Log(exp, WebAPI.Common.Logger.LogType.Error, oTailoringitems);
            }

            return oTailoringitems;
        }

        public Tailoringitems Insert(NpgsqlTransaction transaction, NpgsqlConnection conn, Tailoringitems oTailoringitems)
        {
            NpgsqlCommand command = new NpgsqlCommand(SQL_Insert, conn, transaction);
            command.Parameters.Add("tailoringorderid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oTailoringitems.tailoringorderid;
            command.Parameters.Add("sourceproductid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oTailoringitems.sourceproductid;
            command.Parameters.Add("outcomeproductid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oTailoringitems.outcomeproductid;
            command.Parameters.Add("tailorid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oTailoringitems.tailorid;
            command.Parameters.Add("sourceqty", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oTailoringitems.sourceqty;
            command.Parameters.Add("issuedqty", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oTailoringitems.issuedqty;
            command.Parameters.Add("sourcemeter", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oTailoringitems.sourcemeter;
            command.Parameters.Add("expectedqty", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oTailoringitems.expectedqty;
            command.Parameters.Add("receivedqty", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oTailoringitems.receivedqty;
            command.Parameters.Add("additionalchargeid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oTailoringitems.additionalchargeid;
            command.Parameters.Add("additionalchargeamount", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oTailoringitems.additionalchargeamount;
            command.Parameters.Add("returnqty", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oTailoringitems.returnqty;
            command.Parameters.Add("deliveredqty", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oTailoringitems.deliveredqty;
            command.Parameters.Add("chargeid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oTailoringitems.chargeid;
            command.Parameters.Add("charge", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oTailoringitems.charge;

            command.Parameters.Add("delayperday", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oTailoringitems.delayperday;
            command.Parameters.Add("delaymaximum", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oTailoringitems.delaymaximum;
            command.Parameters.Add("chargeonpriority", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oTailoringitems.chargeonpriority;
            command.Parameters.Add("delaycharge", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oTailoringitems.delaycharge;
            command.Parameters.Add("prioritycharge", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oTailoringitems.prioritycharge;
            command.Parameters.Add("priorityqty", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oTailoringitems.priorityqty;
            
            command.Parameters.Add("gross", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oTailoringitems.gross;
            command.Parameters.Add("tax", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oTailoringitems.tax;
            command.Parameters.Add("total", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oTailoringitems.total;
            command.Parameters.Add("designname", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oTailoringitems.designname == null ? "" : oTailoringitems.designname;
            command.Parameters.Add("designid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oTailoringitems.designid;
            command.Parameters.Add("measurementname", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oTailoringitems.measurementname == null ? "" : oTailoringitems.measurementname;
            command.Parameters.Add("product", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oTailoringitems.product == null ? "" : oTailoringitems.product;
            command.Parameters.Add("measurementid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oTailoringitems.measurementid;
            command.Parameters.Add("measurement", NpgsqlTypes.NpgsqlDbType.Json).Value = oTailoringitems.measurement_json == null ? "null" : oTailoringitems.measurement_json;
            command.Parameters.Add("barcodes", NpgsqlTypes.NpgsqlDbType.Json).Value = oTailoringitems.barcodes_json == null ? "null" : oTailoringitems.barcodes_json;
            command.Parameters.Add("createdon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = DateTimeNow;
            command.Parameters.Add("version", NpgsqlTypes.NpgsqlDbType.Integer).Value = oTailoringitems.version;
            command.Parameters.Add("isactive", NpgsqlTypes.NpgsqlDbType.Boolean).Value = oTailoringitems.isactive;
            command.Parameters.Add("modifiedby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oTailoringitems.modifiedby;
            command.Parameters.Add("createdby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oTailoringitems.createdby;
            command.Parameters.Add("modifiedon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = DateTimeNow;
            using (System.Data.IDataReader reader = command.ExecuteReader())
            {
                if (reader.Read())
                {
                    oTailoringitems.id = Convert.ToInt64(reader[0].ToString());
                }
            }
            return oTailoringitems;
        }

        public Tailoringitems Update(Tailoringitems oTailoringitems)
        {

            NpgsqlTransaction transaction = null;
            try
            {
                using (NpgsqlConnection conn = CreateConnection())
                {
                    conn.Open();
                    transaction = conn.BeginTransaction();

                    Update(transaction, conn, oTailoringitems);
                    transaction.Commit();
                    conn.Close();
                }
            }
            catch (Exception exp)
            {
                WebAPI.Common.Logger.Instance(Context.OrganizationID).Log(exp, WebAPI.Common.Logger.LogType.Error, oTailoringitems);

            }

            return oTailoringitems;
        }

        public Tailoringitems Update(NpgsqlTransaction transaction, NpgsqlConnection conn, Tailoringitems oTailoringitems)
        {

            NpgsqlCommand command = new NpgsqlCommand(SQL_Update, conn, transaction);
            command.Parameters.Add("id", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oTailoringitems.id;
            command.Parameters.Add("tailoringorderid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oTailoringitems.tailoringorderid;
            command.Parameters.Add("sourceproductid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oTailoringitems.sourceproductid;
            command.Parameters.Add("outcomeproductid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oTailoringitems.outcomeproductid;
            command.Parameters.Add("additionalchargeid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oTailoringitems.additionalchargeid;
            command.Parameters.Add("additionalchargeamount", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oTailoringitems.additionalchargeamount;
            command.Parameters.Add("tailorid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oTailoringitems.tailorid;
            command.Parameters.Add("sourceqty", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oTailoringitems.sourceqty;
            command.Parameters.Add("issuedqty", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oTailoringitems.issuedqty;
            command.Parameters.Add("sourcemeter", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oTailoringitems.sourcemeter;
            command.Parameters.Add("expectedqty", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oTailoringitems.expectedqty;
            command.Parameters.Add("receivedqty", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oTailoringitems.receivedqty;
            command.Parameters.Add("returnqty", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oTailoringitems.returnqty;
            command.Parameters.Add("deliveredqty", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oTailoringitems.deliveredqty;
            command.Parameters.Add("chargeid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oTailoringitems.chargeid;
            command.Parameters.Add("charge", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oTailoringitems.charge;
            command.Parameters.Add("delayperday", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oTailoringitems.delayperday;
            command.Parameters.Add("delaymaximum", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oTailoringitems.delaymaximum;
            command.Parameters.Add("chargeonpriority", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oTailoringitems.chargeonpriority;
            command.Parameters.Add("delaycharge", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oTailoringitems.delaycharge;
            command.Parameters.Add("prioritycharge", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oTailoringitems.prioritycharge;
            command.Parameters.Add("priorityqty", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oTailoringitems.priorityqty;
            command.Parameters.Add("gross", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oTailoringitems.gross;
            command.Parameters.Add("tax", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oTailoringitems.tax;
            command.Parameters.Add("total", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oTailoringitems.total;
            command.Parameters.Add("product", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oTailoringitems.product == null ? "" : oTailoringitems.product;
            command.Parameters.Add("designname", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oTailoringitems.designname == null ? "" : oTailoringitems.designname;
            command.Parameters.Add("designid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oTailoringitems.designid;
            command.Parameters.Add("measurementname", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oTailoringitems.measurementname == null ? "" : oTailoringitems.measurementname;
            command.Parameters.Add("measurementid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oTailoringitems.measurementid;
            command.Parameters.Add("measurement", NpgsqlTypes.NpgsqlDbType.Json).Value = oTailoringitems.measurement_json == null ? "null" : oTailoringitems.measurement_json;
            command.Parameters.Add("barcodes", NpgsqlTypes.NpgsqlDbType.Json).Value = oTailoringitems.barcodes_json == null ? "null" : oTailoringitems.barcodes_json;
            command.Parameters.Add("createdon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = DateTimeNow;
            command.Parameters.Add("version", NpgsqlTypes.NpgsqlDbType.Integer).Value = oTailoringitems.version;
            command.Parameters.Add("isactive", NpgsqlTypes.NpgsqlDbType.Boolean).Value = oTailoringitems.isactive;
            command.Parameters.Add("modifiedby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oTailoringitems.modifiedby;
            command.Parameters.Add("createdby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oTailoringitems.createdby;
            command.Parameters.Add("modifiedon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = DateTimeNow;
            using (System.Data.IDataReader reader = command.ExecuteReader())
            {
                if (reader.Read())
                {
                    oTailoringitems.version = Convert.ToInt32(reader[0].ToString());
                }
            }

            return oTailoringitems;
        }

        public bool Update(NpgsqlTransaction transaction, NpgsqlConnection conn, List<Tailoringitems> list, long id)
        {
            var existingItems = SearchAll(id);
            if (existingItems != null && existingItems.Count > 0)
            {
                var deletedItems = existingItems.FindAll(e => !list.Exists(f => f.id == e.id));
                foreach (var item in deletedItems)
                {
                    if (item.receivedqty > 0 || item.returnqty > 0 || item.deliveredqty > 0)
                        throw new Exception("Order item can't be removed.");
                    Delete(transaction, conn, item);
                }
            }

            foreach (var item in list)
            {
                if (item.id > 0)
                {
                    if (item.expectedqty < item.receivedqty || item.expectedqty < item.returnqty
                        || item.expectedqty < item.deliveredqty)
                        throw new Exception("Delivery qty can't be less then the received qty or return qty");
                    if (item.deliveredqty > 0)
                        throw new Exception("Order item can't be updated after the delivery");
                    Update(transaction, conn, item);
                }
                else
                {
                    item.tailoringorderid = id;
                    Insert(transaction, conn, item);
                }
            }
            return true;
        }

        public bool UpdateTailor(List<Tailoringitems> oTailoringitems)
        {
            NpgsqlTransaction transaction = null;
            bool hasSuccessCount = false;
            try
            {
                using (NpgsqlConnection conn = CreateConnection())
                {
                    conn.Open();
                    transaction = conn.BeginTransaction();
                    foreach (var itm in oTailoringitems)
                    {
                        NpgsqlCommand command = new NpgsqlCommand("Update tailoringitems set tailorid=@tailorid where id=@id", conn, transaction);
                        command.Parameters.Add("tailorid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = itm.tailorid;
                        command.Parameters.Add("id", NpgsqlTypes.NpgsqlDbType.Bigint).Value = itm.id;
                        int successCount = command.ExecuteNonQuery();
                    }
                    transaction.Commit();
                    conn.Close();
                }
            }
            catch (Exception exp)
            {
                WebAPI.Common.Logger.Instance(Context.OrganizationID).Log(exp, WebAPI.Common.Logger.LogType.Error, oTailoringitems);
            }
            return hasSuccessCount;

        }

        public bool Delete(Tailoringitems oTailoringitems)
        {
            NpgsqlTransaction transaction = null;
            bool hasSuccessCount = false;
            try
            {
                using (NpgsqlConnection conn = CreateConnection())
                {
                    conn.Open();
                    transaction = conn.BeginTransaction();
                    hasSuccessCount = Delete(transaction, conn, oTailoringitems);
                    transaction.Commit();
                    conn.Close();
                }
            }
            catch (Exception exp)
            {
                WebAPI.Common.Logger.Instance(Context.OrganizationID).Log(exp, WebAPI.Common.Logger.LogType.Error, oTailoringitems);
            }
            return hasSuccessCount;

        }

        public bool Delete(NpgsqlTransaction transaction, NpgsqlConnection conn, Tailoringitems oTailoringitems)
        {

            NpgsqlCommand command = new NpgsqlCommand(SQL_Delete, conn, transaction);
            command.Parameters.Add("isactive", NpgsqlTypes.NpgsqlDbType.Boolean).Value = oTailoringitems.isactive;
            command.Parameters.Add("modifiedby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oTailoringitems.modifiedby;
            command.Parameters.Add("modifiedon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = oTailoringitems.modifiedon;
            command.Parameters.Add("version", NpgsqlTypes.NpgsqlDbType.Integer).Value = oTailoringitems.version;
            command.Parameters.Add("id", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oTailoringitems.id;
            int successCount = command.ExecuteNonQuery();

            return successCount > 0;
        }

        public bool DeleteAll(long tailoringorderid, long userid, NpgsqlConnection conn, NpgsqlTransaction transaction)
        {


            int successCount = 0;
            NpgsqlCommand command = new NpgsqlCommand(SQL_DeleteAll, conn, transaction);
            command.Parameters.Add("isactive", NpgsqlTypes.NpgsqlDbType.Boolean).Value = false;
            command.Parameters.Add("modifiedby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = userid;
            command.Parameters.Add("modifiedon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = DateTimeNow;
            command.Parameters.Add("tailoringorderid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = tailoringorderid;
            successCount = command.ExecuteNonQuery();

            return successCount > 0;
        }

        internal bool SplitQty(TailoringitemsTransaction item)
        {
            NpgsqlTransaction transaction = null;
            var response = new List<Tailoringitems>();

            try
            {
                using (NpgsqlConnection conn = CreateConnection())
                {

                    var existitem = SearchAll(item.tailoringorderid, new List<long>() { item.id }, false).First();
                    if (item.splitqty <= (existitem.expectedqty - existitem.receivedqty))
                    {
                        conn.Open();
                        transaction = conn.BeginTransaction();
                        
                        var newitem = JsonConvert.DeserializeObject<Tailoringitems>(JsonConvert.SerializeObject(existitem));
                        newitem.id = 0;
                        newitem.expectedqty = item.splitqty;
                        newitem.receivedqty = 0;
                        newitem.deliveredqty = 0;
                        newitem.returnqty = 0;
                        newitem.issuedqty = 0;
                        newitem.total = existitem.total / existitem.expectedqty * item.splitqty;
                        if(existitem.tax>0)
                            newitem.tax = existitem.tax / existitem.expectedqty * item.splitqty;
                        newitem.gross = existitem.gross / existitem.expectedqty * item.splitqty;
                        Insert(transaction, conn, newitem);

                        existitem.expectedqty -= item.splitqty;
                        existitem.gross -= item.gross;
                        existitem.tax -= item.tax;
                        existitem.total -= item.total;
                        Update(transaction, conn, existitem);
                        transaction.Commit();
                        conn.Close();
                        return true;
                    }
                    else
                    {
                        throw new Exception("Split qty can't be greater than the pending qty");
                    }
                }
            }
            catch (Exception exp)
            {
                WebAPI.Common.Logger.Instance(Context.OrganizationID).Log(exp, WebAPI.Common.Logger.LogType.Error, item);
            }
            return false;
        }

        internal bool DeliverQty(TailoringitemsTransaction item)
        {
            NpgsqlTransaction transaction = null;
            var response = new List<Tailoringitems>();

            try
            {
                using (NpgsqlConnection conn = CreateConnection())
                {
                    conn.Open();
                    transaction = conn.BeginTransaction();

                    var existitem = SearchAll(item.tailoringorderid,
                        (item.allids != null && item.allids.Count > 0) ? item.allids : (new List<long>() { item.id }), false);

                    TailoringorderDataService tailoringorderDataService = new TailoringorderDataService() { Context = this.Context };
                    var tailoringorders = tailoringorderDataService.Search(new TailoringorderCreteria()
                    {
                        ids = existitem.ConvertAll(x => x.tailoringorderid).Distinct().ToList()
                    });

                    TailoringitemsdeliveredDataService subdelivery = new TailoringitemsdeliveredDataService();
                    foreach (var eitem in existitem)
                    {
                        Tailoringitemsdelivered pitem = new Tailoringitemsdelivered();

                        pitem.tailoringorderid = eitem.tailoringorderid;
                        pitem.tailoringitemsid = eitem.id;
                        if (item.allids != null && item.allids.Count > 0)
                            pitem.deliveredqty = eitem.receivedqty-eitem.deliveredqty;
                        else
                            pitem.deliveredqty += item.deliveredqty;
                        
                        pitem.deliveredon = DateTimeNow;
                        pitem.deliveredby = item.deliveredby;
                        pitem.createdon = DateTimeNow;
                        pitem.modifiedon = DateTimeNow;
                        pitem.createdby = Context.UserID;
                        pitem.modifiedby = Context.UserID;

                        pitem.receivedby = item.receivedby;
                        pitem.receivedbycustomer = item.receivedbycustomer;

                        subdelivery.Insert(transaction, conn, pitem);

                        if (tailoringorders.Exists(x => x.id == eitem.tailoringorderid))
                        {
                            decimal delaycharge = eitem.delaycharge;
                            decimal prioritycharge = 0;
                            var tailoringorder = tailoringorders.Find(x => x.id == eitem.tailoringorderid);
                            if (tailoringorder.expectedon.Date < DateTimeNow.Date)
                            {
                                if (eitem.delayperday > 0)
                                {
                                    delaycharge += pitem.deliveredqty * eitem.delayperday;
                                }
                                if (eitem.delaymaximum > 0)
                                {
                                    delaycharge += pitem.deliveredqty * eitem.delaymaximum;
                                }
                            }

                            if(eitem.priorityqty>0 &&
                                eitem.chargeonpriority>0 &&
                                eitem.priorityqty>=(eitem.delayperday+ pitem.deliveredqty))
                            {
                                prioritycharge = eitem.priorityqty*eitem.chargeonpriority;
                            }

                            if (delaycharge > 0 || prioritycharge > 0)
                            {
                                string query = "Update Tailoringitems set prioritycharge=@prioritycharge,delaycharge=@delaycharge where id=@id";
                                var param = new SortedList<string, object>();
                                param.Add("id", eitem.id);
                                param.Add("prioritycharge", prioritycharge);
                                param.Add("delaycharge", delaycharge);
                                ExecuteQuery(query, param,conn,transaction);
                            }
                        }
                    }
                    transaction.Commit();
                    conn.Close();
                    return true;
                }
            }
            catch (Exception exp)
            {
                WebAPI.Common.Logger.Instance(Context.OrganizationID).Log(exp, WebAPI.Common.Logger.LogType.Error, item);
            }
            return false;
        }

        internal bool ReceivedQty(TailoringitemsTransaction item)
        {
            NpgsqlTransaction transaction = null;
            var response = new List<Tailoringitems>();

            try
            {
                using (NpgsqlConnection conn = CreateConnection())
                {
                    conn.Open();
                    transaction = conn.BeginTransaction();

                    var existitem = SearchAll(item.tailoringorderid,
                        (item.allids != null && item.allids.Count > 0) ? item.allids : (new List<long>() { item.id }), false);

                    existitem.RemoveAll(x => x.expectedqty == x.receivedqty);

                    foreach (var eitem in existitem)
                    {
                        if (item.allids != null && item.allids.Count > 0)
                            eitem.receivedqty = eitem.expectedqty;
                        else
                            eitem.receivedqty += item.receivedqty;
                        Update(transaction, conn, eitem);
                    }
                    transaction.Commit();
                    conn.Close();

                    if (existitem.Count > 0)
                    {
                        var allorderid = existitem.GroupBy(x => x.tailoringorderid).ToList().ConvertAll(X => X.Key);

                        string query = @"with completedorders as
                        (
                        select tailoringorderid,sum(receivedqty) readyqty from Tailoringitems
                        where tailoringorderid in (" + string.Join(",", allorderid) + @") 
                        group by tailoringorderid
                        having sum(expectedqty) = sum(receivedqty)
                        )
                        select Tailoringorder.receivedon,customer.mobileno,customer.name,readyqty,
                        Tailoringorder.orderno,Tailoringorder.entryno,Tailoringorder.expectedon
                        from completedorders
                        inner join Tailoringorder on Tailoringorder.id = tailoringorderid
                        inner join customer on customer.id = Tailoringorder.customerid";
                        List<Delivery> deliverymsgs = new List<Delivery>();
                        using (var reader = ExecuteReader(query, null))
                        {
                            while (reader.Read())
                            {
                                Delivery delivery = new Delivery();
                                delivery.billdate = reader["receivedon"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(reader["receivedon"]);
                                delivery.contactno = reader["mobileno"] == DBNull.Value ? "" : reader["mobileno"].ToString();
                                delivery.customername = reader["name"] == DBNull.Value ? "" : reader["name"].ToString();
                                delivery.qty = reader["readyqty"] == DBNull.Value ? 0 : Convert.ToInt32(reader["readyqty"].ToString());
                                delivery.orderno = reader["orderno"] == DBNull.Value ? "" : reader["orderno"].ToString();
                                delivery.entryno = reader["entryno"] == DBNull.Value ? "" : reader["entryno"].ToString();
                                delivery.expectedon = reader["expectedon"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(reader["expectedon"]);
                                deliverymsgs.Add(delivery);
                            }
                        }
                        foreach (var delivery in deliverymsgs)
                            Processors.Schedule_Actions.Instance.OnChange(Context.OrganizationID, delivery, Processors.Schedule_Actions.Actions.AlterationReady);
                    }
                    return true;
                }
            }
            catch (Exception exp)
            {
                WebAPI.Common.Logger.Instance(Context.OrganizationID).Log(exp, WebAPI.Common.Logger.LogType.Error, item);
            }
            return false;
        }

        internal bool TailoringPayment(TailoringPayment item)
        {
            NpgsqlTransaction transaction = null;
            var response = new List<Tailoringitems>();

            try
            {
                using (NpgsqlConnection conn = CreateConnection())
                {
                    conn.Open();
                    transaction = conn.BeginTransaction();
                    var items = item.GetAllItems();
                    var existitem = SearchAll(0, items.ConvertAll(x => x.id), false);
                    TailoringpaymentDataService tailoringpaymentDataService = new TailoringpaymentDataService() { Context = this.Context };
                    var sequence = SequenceManager.Instance(Context.OrganizationID).Get("tailoringpayment.paymentcode", item.locationid);
                    foreach (var tailorgrp in existitem.GroupBy(x => x.tailorid))
                    {
                        var paymentcode = sequence.GetNextNumber(this.Context, conn, transaction);
                        foreach (var eitem in tailorgrp)
                        {
                            var amtitm = items.Find(x => x.id == eitem.id);
                            Tailoringpayment pitem = new Tailoringpayment();
                            pitem.paymentcode = paymentcode;
                            pitem.tailoringorderid = eitem.tailoringorderid;
                            pitem.Tailoringitemid = eitem.id;
                            pitem.companyid = item.companyid;
                            pitem.locationid = item.locationid;
                            pitem.tailorid = amtitm.tailorid;
                            if (item.paymode > 0)
                            {
                                pitem.paymode = 1;
                                pitem.bankid = item.paymode;
                            }
                            pitem.isactive = true;
                            pitem.createdon = DateTimeNow;
                            pitem.modifiedon = DateTimeNow;
                            pitem.createdby = Context.UserID;
                            pitem.modifiedby = Context.UserID;
                            pitem.amount = amtitm.newpaid;
                            pitem.collectedon = item.date;
                            tailoringpaymentDataService.Insert(transaction, conn, pitem);
                            var param = new SortedList<string, object>();
                            param.Add("id", eitem.id);
                            param.Add("paidon", item.date);
                            param.Add("paidamount", amtitm.newpaid);
                            ExecuteQuery("update Tailoringitems set paidon=@paidon,paidamount=paidamount+@paidamount where id=@id",
                                param, conn, transaction);
                        }
                    }
                    transaction.Commit();
                    conn.Close();
                    return true;
                }
            }
            catch (Exception exp)
            {
                WebAPI.Common.Logger.Instance(Context.OrganizationID).Log(exp, WebAPI.Common.Logger.LogType.Error, item);
            }
            return false;
        }
    }

}
