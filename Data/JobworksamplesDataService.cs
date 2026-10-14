using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using InfyPOS.Models;
using InfyPOS.Common;
using Npgsql;
namespace InfyPOS.Data

{
    public class JobworksamplesDataService : BaseData
    {
        const string SQL_Search = @"select isactive, id, modifiedon, createdon, version, modifiedby, createdby, productid, designname, customerid, customername, sampleqty, orderqty, purchaseprice,sellingprice, sampleno, samplestatus, receivedby, receivedon, senton, sentby, receivedcouriercompanyid, sentcouriercompanyid, receivedcourierpod, sentcourierpod, sampleimageid, communication, expecteddelivery from jobworksamples where id=@id";


        const string SQL_Insert = @"INSERT INTO public.jobworksamples(isactive, modifiedon, createdon, version, modifiedby, createdby, productid, designname, customerid, customername, sampleqty, orderqty, purchaseprice,sellingprice, sampleno, samplestatus, receivedby, receivedon, senton, sentby, receivedcouriercompanyid, sentcouriercompanyid, receivedcourierpod, sentcourierpod, sampleimageid, communication, expecteddelivery) VALUES(@isactive, @modifiedon, @createdon, @version, @modifiedby, @createdby, @productid, @designname, @customerid, @customername, @sampleqty, @orderqty, @purchaseprice,@sellingprice, @sampleno, @samplestatus, @receivedby, @receivedon, @senton, @sentby, @receivedcouriercompanyid, @sentcouriercompanyid, @receivedcourierpod, @sentcourierpod, @sampleimageid, @communication, @expecteddelivery) RETURNING id";


        const string SQL_Update = @"UPDATE public.jobworksamples SET modifiedon=@modifiedon, version=@version+1, modifiedby=@modifiedby, productid=@productid, designname=@designname, customerid=@customerid, customername=@customername, sampleqty=@sampleqty, orderqty=@orderqty, purchaseprice=@purchaseprice,sellingprice=@sellingprice, sampleno=@sampleno, samplestatus=@samplestatus, receivedby=@receivedby, receivedon=@receivedon, senton=@senton, sentby=@sentby, receivedcouriercompanyid=@receivedcouriercompanyid, sentcouriercompanyid=@sentcouriercompanyid, receivedcourierpod=@receivedcourierpod, sentcourierpod=@sentcourierpod, sampleimageid=@sampleimageid, communication=@communication, expecteddelivery=@expecteddelivery where id=@id and version=@version RETURNING version";

        const string SQL_Delete = @"update public.jobworksamples set isactive=false,version=@version+1,modifiedby=@modifiedby,modifiedon=@modifiedon where id=@id and version=@version";

        public List<Jobworksamples> Search(JobworksamplesCreteria _jobworksamples)
        {
            List<Jobworksamples> lst = new List<Jobworksamples>();
            NpgsqlConnection conn = CreateConnection();
            try
            {
                conn.Open();

                NpgsqlCommand command = null;
                if (_jobworksamples.id != 0)
                {
                    command = new NpgsqlCommand(SQL_Search, conn);
                    command.Parameters.Add("id", NpgsqlTypes.NpgsqlDbType.Bigint).Value = _jobworksamples.id;
                }
                else
                {
                    QueryBuilder qb = new QueryBuilder();
                    qb.CurrentQuery = SQL_Search.Substring(0, SQL_Search.IndexOf("where"));
                    qb.AddParameter("isactive", true, "=");
                    if (_jobworksamples.samplestatus > 0)
                        qb.AddParameter("samplestatus", _jobworksamples.samplestatus, "=");
                    if (_jobworksamples.customerid > 0)
                        qb.AddParameter("customerid", _jobworksamples.customerid, "=");
                    if (_jobworksamples.productid > 0)
                        qb.AddParameter("productid", _jobworksamples.productid, "=");
                    if (_jobworksamples.receivedby > 0)
                        qb.AddParameter("receivedby", _jobworksamples.receivedby, "=");
                    if (_jobworksamples.receivedcouriercompanyid > 0)
                        qb.AddParameter("receivedcouriercompanyid", _jobworksamples.receivedcouriercompanyid, "=");
                    if (_jobworksamples.sentcouriercompanyid > 0)
                        qb.AddParameter("sentcouriercompanyid", _jobworksamples.sentcouriercompanyid, "=");
                    if (_jobworksamples.sentby > 0)
                        qb.AddParameter("sentby", _jobworksamples.sentby, "=");
                    if (!string.IsNullOrEmpty(_jobworksamples.sampleno))
                        qb.AddParameter("sampleno", _jobworksamples.sampleno, "=");
                    if (_jobworksamples.receivedtodate.Year > 2000)
                        qb.AddParameter("receivedtodate", "receivedtodate", _jobworksamples.receivedtodate.Date.AddDays(1), "<");
                    if (_jobworksamples.receivedfromdate.Year > 2000)
                        qb.AddParameter("receivedtodate", "receivedfromdate", _jobworksamples.receivedfromdate.Date, ">=");

                    if (_jobworksamples.senttodate.Year > 2000)
                        qb.AddParameter("senton", "senttodate", _jobworksamples.senttodate.Date.AddDays(1), "<");
                    if (_jobworksamples.receivedfromdate.Year > 2000)
                        qb.AddParameter("senton", "sentfromdate", _jobworksamples.sentfromdate.Date, ">=");

                    if (_jobworksamples.expectedtodate.Year > 2000)
                        qb.AddParameter("expecteddelivery", "expectedtodate", _jobworksamples.expectedtodate.Date.AddDays(1), "<");
                    if (_jobworksamples.expectedfromdate.Year > 2000)
                        qb.AddParameter("expecteddelivery", "expectedfromdate", _jobworksamples.expectedfromdate.Date, ">=");

                    if (!string.IsNullOrEmpty(_jobworksamples.sentcourierpod))
                        qb.AddParameter("sentcourierpod", _jobworksamples.sentcourierpod, "=");

                    if (!string.IsNullOrEmpty(_jobworksamples.receivedcourierpod))
                        qb.AddParameter("receivedcourierpod", _jobworksamples.receivedcourierpod, "=");

                    if (!string.IsNullOrEmpty(_jobworksamples.designname))
                        qb.AddParameter("designname", _jobworksamples.designname, "=");
                    command = qb.GetCommand(conn);
                }

                using (System.Data.IDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        Jobworksamples ojobworksamples = new Jobworksamples();
                        ojobworksamples.isactive = reader["isactive"] == DBNull.Value ? false : Convert.ToBoolean(reader["isactive"]);
                        ojobworksamples.id = reader["id"] == DBNull.Value ? 0 : Convert.ToInt64(reader["id"]);
                        ojobworksamples.modifiedon = reader["modifiedon"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(reader["modifiedon"]);
                        ojobworksamples.createdon = reader["createdon"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(reader["createdon"]);
                        ojobworksamples.version = reader["version"] == DBNull.Value ? 0 : Convert.ToInt32(reader["version"]);
                        ojobworksamples.modifiedby = reader["modifiedby"] == DBNull.Value ? 0 : Convert.ToInt64(reader["modifiedby"]);
                        ojobworksamples.createdby = reader["createdby"] == DBNull.Value ? 0 : Convert.ToInt64(reader["createdby"]);
                        ojobworksamples.productid = reader["productid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["productid"]);
                        ojobworksamples.designname = reader["designname"] == DBNull.Value ? "" : reader["designname"].ToString();
                        ojobworksamples.customerid = reader["customerid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["customerid"]);
                        ojobworksamples.customername = reader["customername"] == DBNull.Value ? "" : reader["customername"].ToString();
                        ojobworksamples.sampleqty = reader["sampleqty"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["sampleqty"]);
                        ojobworksamples.orderqty = reader["orderqty"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["orderqty"]);
                        ojobworksamples.purchaseprice = reader["purchaseprice"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["purchaseprice"]);
                        ojobworksamples.sellingprice = reader["sellingprice"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["sellingprice"]);
                        ojobworksamples.sampleno = reader["sampleno"] == DBNull.Value ? "" : reader["sampleno"].ToString();
                        ojobworksamples.samplestatus = reader["samplestatus"] == DBNull.Value ? 0 : Convert.ToInt32(reader["samplestatus"]);
                        ojobworksamples.receivedby = reader["receivedby"] == DBNull.Value ? 0 : Convert.ToInt64(reader["receivedby"]);
                        ojobworksamples.receivedon = reader["receivedon"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(reader["receivedon"]);
                        ojobworksamples.senton = reader["senton"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(reader["senton"]);
                        ojobworksamples.sentby = reader["sentby"] == DBNull.Value ? 0 : Convert.ToInt64(reader["sentby"]);
                        ojobworksamples.receivedcouriercompanyid = reader["receivedcouriercompanyid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["receivedcouriercompanyid"]);
                        ojobworksamples.sentcouriercompanyid = reader["sentcouriercompanyid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["sentcouriercompanyid"]);
                        ojobworksamples.receivedcourierpod = reader["receivedcourierpod"] == DBNull.Value ? "" : reader["receivedcourierpod"].ToString();
                        ojobworksamples.sentcourierpod = reader["sentcourierpod"] == DBNull.Value ? "" : reader["sentcourierpod"].ToString();
                        ojobworksamples.sampleimageid = reader["sampleimageid"] == DBNull.Value ? "" : reader["sampleimageid"].ToString();
                        ojobworksamples.communication_json = reader["communication"] == DBNull.Value ? null : reader["communication"].ToString();
                        ojobworksamples.expecteddelivery = reader["expecteddelivery"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(reader["expecteddelivery"]);
                        lst.Add(ojobworksamples);
                    }
                }
                conn.Close();
            }
            catch (Exception exp)
            {
                if (conn != null) conn.Close();
                WebAPI.Common.Logger.Instance(Context.OrganizationID).Log(exp, WebAPI.Common.Logger.LogType.Error, _jobworksamples);
            }
            return lst;
        }

        public Jobworksamples Insert(Jobworksamples oJobworksamples)
        {

            NpgsqlTransaction transaction = null;
            try
            {
                var sequence = SequenceManager.Instance(Context.OrganizationID).Get("jobworksamples.sampleno", 0);
                using (NpgsqlConnection conn = CreateConnection())
                {
                    conn.Open();
                    transaction = conn.BeginTransaction();
                    NpgsqlCommand command = new NpgsqlCommand(SQL_Insert, conn, transaction);
                    oJobworksamples.sampleno = sequence.GetNextNumber(Context,conn, transaction);

                    Insert(transaction, conn, oJobworksamples);
                    transaction.Commit();
                    conn.Close();
                }
            }
            catch (Exception exp)
            {
                WebAPI.Common.Logger.Instance(Context.OrganizationID).Log(exp, WebAPI.Common.Logger.LogType.Error, oJobworksamples);
            }

            return oJobworksamples;
        }

        public Jobworksamples Insert(NpgsqlTransaction transaction, NpgsqlConnection conn, Jobworksamples oJobworksamples)
        {

            NpgsqlCommand command = new NpgsqlCommand(SQL_Insert, conn, transaction);
            command.Parameters.Add("isactive", NpgsqlTypes.NpgsqlDbType.Boolean).Value = oJobworksamples.isactive;
            command.Parameters.Add("modifiedon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = oJobworksamples.modifiedon;
            command.Parameters.Add("createdon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = oJobworksamples.createdon;
            command.Parameters.Add("version", NpgsqlTypes.NpgsqlDbType.Integer).Value = oJobworksamples.version;
            command.Parameters.Add("modifiedby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oJobworksamples.modifiedby;
            command.Parameters.Add("createdby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oJobworksamples.createdby;
            command.Parameters.Add("productid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oJobworksamples.productid;
            command.Parameters.Add("designname", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oJobworksamples.designname == null ? "" : oJobworksamples.designname;
            command.Parameters.Add("customerid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oJobworksamples.customerid;
            command.Parameters.Add("customername", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oJobworksamples.customername == null ? "" : oJobworksamples.customername;
            command.Parameters.Add("sampleqty", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oJobworksamples.sampleqty;
            command.Parameters.Add("orderqty", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oJobworksamples.orderqty;
            command.Parameters.Add("purchaseprice", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oJobworksamples.purchaseprice;
            command.Parameters.Add("sellingprice", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oJobworksamples.sellingprice;
            command.Parameters.Add("sampleno", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oJobworksamples.sampleno == null ? "" : oJobworksamples.sampleno;
            command.Parameters.Add("samplestatus", NpgsqlTypes.NpgsqlDbType.Integer).Value = oJobworksamples.samplestatus;
            command.Parameters.Add("receivedby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oJobworksamples.receivedby;
            command.Parameters.Add("receivedon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = oJobworksamples.receivedon;
            command.Parameters.Add("senton", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = oJobworksamples.senton;
            command.Parameters.Add("sentby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oJobworksamples.sentby;
            command.Parameters.Add("receivedcouriercompanyid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oJobworksamples.receivedcouriercompanyid;
            command.Parameters.Add("sentcouriercompanyid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oJobworksamples.sentcouriercompanyid;
            command.Parameters.Add("receivedcourierpod", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oJobworksamples.receivedcourierpod == null ? "" : oJobworksamples.receivedcourierpod;
            command.Parameters.Add("sentcourierpod", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oJobworksamples.sentcourierpod == null ? "" : oJobworksamples.sentcourierpod;
            command.Parameters.Add("sampleimageid", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oJobworksamples.sampleimageid == null ? "" : oJobworksamples.sampleimageid;
            command.Parameters.Add("communication", NpgsqlTypes.NpgsqlDbType.Json).Value = oJobworksamples.communication_json == null ? "null" : oJobworksamples.communication_json;
            command.Parameters.Add("expecteddelivery", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = oJobworksamples.expecteddelivery;
            using (System.Data.IDataReader reader = command.ExecuteReader())
            {
                if (reader.Read())
                {
                    oJobworksamples.id = Convert.ToInt64(reader[0].ToString());
                }
            }
            return oJobworksamples;
        }

        public Jobworksamples Update(Jobworksamples oJobworksamples)
        {

            NpgsqlTransaction transaction = null;
            try
            {
                using (NpgsqlConnection conn = CreateConnection())
                {
                    conn.Open();
                    transaction = conn.BeginTransaction();

                    Update(transaction, conn, oJobworksamples);
                    transaction.Commit();
                    conn.Close();
                }
            }
            catch (Exception exp)
            {
                WebAPI.Common.Logger.Instance(Context.OrganizationID).Log(exp, WebAPI.Common.Logger.LogType.Error, oJobworksamples);
            }

            return oJobworksamples;
        }

        public Jobworksamples Update(NpgsqlTransaction transaction, NpgsqlConnection conn, Jobworksamples oJobworksamples)
        {

            NpgsqlCommand command = new NpgsqlCommand(SQL_Update, conn, transaction);
            command.Parameters.Add("isactive", NpgsqlTypes.NpgsqlDbType.Boolean).Value = oJobworksamples.isactive;
            command.Parameters.Add("id", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oJobworksamples.id;
            command.Parameters.Add("modifiedon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = oJobworksamples.modifiedon;
            command.Parameters.Add("createdon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = oJobworksamples.createdon;
            command.Parameters.Add("version", NpgsqlTypes.NpgsqlDbType.Integer).Value = oJobworksamples.version;
            command.Parameters.Add("modifiedby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oJobworksamples.modifiedby;
            command.Parameters.Add("createdby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oJobworksamples.createdby;
            command.Parameters.Add("productid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oJobworksamples.productid;
            command.Parameters.Add("designname", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oJobworksamples.designname == null ? "" : oJobworksamples.designname;
            command.Parameters.Add("customerid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oJobworksamples.customerid;
            command.Parameters.Add("customername", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oJobworksamples.customername == null ? "" : oJobworksamples.customername;
            command.Parameters.Add("sampleqty", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oJobworksamples.sampleqty;
            command.Parameters.Add("orderqty", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oJobworksamples.orderqty;
            command.Parameters.Add("purchaseprice", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oJobworksamples.purchaseprice;
            command.Parameters.Add("sellingprice", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oJobworksamples.sellingprice;
            command.Parameters.Add("sampleno", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oJobworksamples.sampleno == null ? "" : oJobworksamples.sampleno;
            command.Parameters.Add("samplestatus", NpgsqlTypes.NpgsqlDbType.Integer).Value = oJobworksamples.samplestatus;
            command.Parameters.Add("receivedby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oJobworksamples.receivedby;
            command.Parameters.Add("receivedon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = oJobworksamples.receivedon;
            command.Parameters.Add("senton", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = oJobworksamples.senton;
            command.Parameters.Add("sentby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oJobworksamples.sentby;
            command.Parameters.Add("receivedcouriercompanyid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oJobworksamples.receivedcouriercompanyid;
            command.Parameters.Add("sentcouriercompanyid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oJobworksamples.sentcouriercompanyid;
            command.Parameters.Add("receivedcourierpod", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oJobworksamples.receivedcourierpod == null ? "" : oJobworksamples.receivedcourierpod;
            command.Parameters.Add("sentcourierpod", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oJobworksamples.sentcourierpod == null ? "" : oJobworksamples.sentcourierpod;
            command.Parameters.Add("sampleimageid", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oJobworksamples.sampleimageid == null ? "" : oJobworksamples.sampleimageid;
            command.Parameters.Add("communication", NpgsqlTypes.NpgsqlDbType.Json).Value = oJobworksamples.communication_json == null ? "null" : oJobworksamples.communication_json;
            command.Parameters.Add("expecteddelivery", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = oJobworksamples.expecteddelivery;
            using (System.Data.IDataReader reader = command.ExecuteReader())
            {
                if (reader.Read())
                {
                    oJobworksamples.version = Convert.ToInt32(reader[0].ToString());
                }
            }

            return oJobworksamples;
        }


        public bool Delete(Jobworksamples oJobworksamples)
        {

            NpgsqlTransaction transaction = null;
            bool hasSuccessCount = false;
            try
            {
                using (NpgsqlConnection conn = CreateConnection())
                {
                    conn.Open();
                    transaction = conn.BeginTransaction();
                    hasSuccessCount = Delete(transaction, conn, oJobworksamples);
                    transaction.Commit();
                    conn.Close();
                }
            }
            catch (Exception exp)
            {
                WebAPI.Common.Logger.Instance(Context.OrganizationID).Log(exp, WebAPI.Common.Logger.LogType.Error, oJobworksamples);

            }
            return hasSuccessCount;

        }

        public bool Delete(NpgsqlTransaction transaction, NpgsqlConnection conn, Jobworksamples oJobworksamples)
        {

            NpgsqlCommand command = new NpgsqlCommand(SQL_Delete, conn, transaction);
            command.Parameters.Add("isactive", NpgsqlTypes.NpgsqlDbType.Boolean).Value = oJobworksamples.isactive;
            command.Parameters.Add("modifiedby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oJobworksamples.modifiedby;
            command.Parameters.Add("modifiedon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = oJobworksamples.modifiedon;
            command.Parameters.Add("version", NpgsqlTypes.NpgsqlDbType.Integer).Value = oJobworksamples.version;
            command.Parameters.Add("id", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oJobworksamples.id;
            int successCount = command.ExecuteNonQuery();

            return successCount > 0;
        }

    }

}
