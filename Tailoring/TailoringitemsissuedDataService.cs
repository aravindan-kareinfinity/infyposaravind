using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using InfyPOS.Models;
using InfyPOS.Common;
using Npgsql;
namespace InfyPOS.Data
{
    public class TailoringitemsissuedDataService : BaseData
    {
        const string SQL_Search = @"select tailoringitemsissued.id, tailoringitemsissued.tailoringitemsid, tailoringitemsissued.issuedqty, 
tailoringitemsissued.issuedby,tailoringitemsissued.issuedto,tailoringitemsissued.outwardno, tailoringitemsissued.issuedon, 
tailoringitemsissued.createdon, tailoringitemsissued.version, tailoringitemsissued.isactive, tailoringitemsissued.modifiedby, 
tailoringitemsissued.createdby, tailoringitemsissued.modifiedon,
customername, product,orderno,entryno,locationid,Tailoringitems.total charge,sourcemeter,sourceqty,
Tailoringitems.expectedqty,Tailoringitems.tailoringorderid from tailoringitemsissued
inner join Tailoringitems on Tailoringitems.id = Tailoringitemsissued.tailoringitemsid
inner join Tailoringorder on Tailoringorder.id = Tailoringitems.tailoringorderid 
where tailoringitemsissued.id=@id";

        const string SQL_SearchAll = @"select id, tailoringitemsid, issuedqty, issuedby,issuedto,outwardno, issuedon, 
createdon, version, isactive, modifiedby, createdby, modifiedon from tailoringitemsissued 
where tailoringitemsid=@tailoringitemsid";

        const string SQL_Insert = @"
UPDATE Tailoringitems SET issuedqty=issuedqty+@issuedqty,status=2 WHERE id = @tailoringitemsid;
INSERT INTO public.tailoringitemsissued(tailoringitemsid, issuedqty, issuedby,issuedto,outwardno, issuedon, createdon, version, isactive, modifiedby, createdby, modifiedon)
VALUES(@tailoringitemsid, @issuedqty, @issuedby,@issuedto,@outwardno, @issuedon, @createdon, @version, @isactive, @modifiedby, @createdby, @modifiedon) RETURNING id";


        const string SQL_Update = @"UPDATE public.tailoringitemsissued SET tailoringitemsid=@tailoringitemsid, issuedqty=@issuedqty, issuedby=@issuedby, issuedon=@issuedon, version=@version+1, modifiedby=@modifiedby, modifiedon=@modifiedon where id=@id and version=@version RETURNING version";

        const string SQL_Delete = @"
UPDATE Tailoringitems SET Tailoringitems.issuedqty=Tailoringitems.issuedqty-tailoringitemsissued.issuedqty from tailoringitemsissued
WHERE tailoringitemsissued.id = @id and tailoringitemsissued.tailoringitemsid = Tailoringitems.id;
update public.tailoringitemsissued set isactive=false,version=@version+1,modifiedby=@modifiedby,modifiedon=@modifiedon where id=@id and version=@version";

        const string SQL_DeleteAll = @"update public.tailoringitemsissued set isactive=false,modifiedby=@modifiedby,modifiedon=@modifiedon where tailoringitemsid=@tailoringitemsid";
        public List<TailoringitemsissuedDetail> Search(TailoringitemsissuedCreteria _tailoringitemsissued)
        {
            List<TailoringitemsissuedDetail> lst = new List<TailoringitemsissuedDetail>();
            NpgsqlConnection conn = CreateConnection();
            try
            {
                conn.Open();

                NpgsqlCommand command = null;
                if (_tailoringitemsissued.id != 0)
                {
                    command = new NpgsqlCommand(SQL_Search, conn);
                    command.Parameters.Add("id", NpgsqlTypes.NpgsqlDbType.Bigint).Value = _tailoringitemsissued.id;
                }
                else
                {
                    QueryBuilder qb = new QueryBuilder();
                    qb.CurrentQuery = SQL_Search.Substring(0, SQL_Search.IndexOf("where"));
                    qb.AddParameter("tailoringitemsissued.isactive", true, "=");

                    if (_tailoringitemsissued.issuedto > 0)
                        qb.AddParameter("tailoringitemsissued.issuedto", _tailoringitemsissued.issuedto, "=");
                    if (_tailoringitemsissued.issuedby > 0)
                        qb.AddParameter("tailoringitemsissued.issuedby", _tailoringitemsissued.issuedby, "=");
                    if (_tailoringitemsissued.companyid > 0)
                        qb.AddParameter("tailoringorder.companyid", _tailoringitemsissued.companyid, "=");
                    if (_tailoringitemsissued.locationid > 0)
                        qb.AddParameter("tailoringorder.locationid", _tailoringitemsissued.locationid, "=");
                    if (_tailoringitemsissued.fromdate.Year > 1900)
                        qb.AddParameter("Tailoringitemsissued.issuedon", "fromdate", _tailoringitemsissued.fromdate.Date, ">=");
                    if (_tailoringitemsissued.todate.Year > 1900)
                        qb.AddParameter("Tailoringitemsissued.issuedon", "todate", _tailoringitemsissued.todate.Date.AddDays(1), "<");

                    if (!string.IsNullOrEmpty(_tailoringitemsissued.outwardno))
                        qb.AddParameter("tailoringitemsissued.outwardno", _tailoringitemsissued.outwardno, "=");

                    qb.SortField = "tailoringitemsissued.id desc";
                    command = qb.GetCommand(conn);
                }

                using (System.Data.IDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        TailoringitemsissuedDetail otailoringitemsissued = new TailoringitemsissuedDetail();
                        otailoringitemsissued.id = reader["id"] == DBNull.Value ? 0 : Convert.ToInt64(reader["id"]);
                        otailoringitemsissued.outwardno = reader["outwardno"] == DBNull.Value ? "" : Convert.ToString(reader["outwardno"]);
                        otailoringitemsissued.tailoringitemsid = reader["tailoringitemsid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["tailoringitemsid"]);
                        otailoringitemsissued.issuedqty = reader["issuedqty"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["issuedqty"]);
                        otailoringitemsissued.issuedby = reader["issuedby"] == DBNull.Value ? 0 : Convert.ToInt64(reader["issuedby"]);
                        otailoringitemsissued.issuedto = reader["issuedto"] == DBNull.Value ? 0 : Convert.ToInt64(reader["issuedto"]);
                        otailoringitemsissued.issuedon = reader["issuedon"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(reader["issuedon"]);
                        otailoringitemsissued.createdon = reader["createdon"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(reader["createdon"]);
                        otailoringitemsissued.version = reader["version"] == DBNull.Value ? 0 : Convert.ToInt32(reader["version"]);
                        otailoringitemsissued.isactive = reader["isactive"] == DBNull.Value ? false : Convert.ToBoolean(reader["isactive"]);
                        otailoringitemsissued.modifiedby = reader["modifiedby"] == DBNull.Value ? 0 : Convert.ToInt64(reader["modifiedby"]);
                        otailoringitemsissued.createdby = reader["createdby"] == DBNull.Value ? 0 : Convert.ToInt64(reader["createdby"]);
                        otailoringitemsissued.modifiedon = reader["modifiedon"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(reader["modifiedon"]);

                        otailoringitemsissued.customername = reader["customername"] == DBNull.Value ? "" : Convert.ToString(reader["customername"]);
                        otailoringitemsissued.product = reader["product"] == DBNull.Value ? "" : Convert.ToString(reader["product"]);
                        otailoringitemsissued.orderno = reader["orderno"] == DBNull.Value ? "" : Convert.ToString(reader["orderno"]);
                        otailoringitemsissued.entryno = reader["entryno"] == DBNull.Value ? "" : Convert.ToString(reader["entryno"]);
                        otailoringitemsissued.locationid = reader["locationid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["locationid"]);
                        otailoringitemsissued.tailoringorderid = reader["tailoringorderid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["tailoringorderid"]);
                        otailoringitemsissued.charge = reader["charge"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["charge"]);
                        otailoringitemsissued.sourcemeter = reader["sourcemeter"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["sourcemeter"]);
                        otailoringitemsissued.sourceqty = reader["sourceqty"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["sourceqty"]);
                        otailoringitemsissued.expectedqty = reader["expectedqty"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["expectedqty"]);

                        otailoringitemsissued.sno = lst.Count + 1;
                        lst.Add(otailoringitemsissued);
                    }
                }
                conn.Close();
            }
            catch (Exception exp)
            {
                if (conn != null) conn.Close();
                WebAPI.Common.Logger.Instance(Context.OrganizationID).Log(exp, WebAPI.Common.Logger.LogType.Error, _tailoringitemsissued);
            }
            return lst;
        }

        internal List<TailoringOutwardBundle> SearchBundle(TailoringitemsissuedCreteria item)
        {
            List<TailoringOutwardBundle> lst = new List<TailoringOutwardBundle>();
            NpgsqlConnection conn = CreateConnection();
            try
            {
                conn.Open();

                NpgsqlCommand command = null;

                QueryBuilder qb = new QueryBuilder();
                qb.CurrentQuery = @"select outwardno,issuedon::date,sum(Tailoringitemsissued.issuedqty) issuedqty from Tailoringitemsissued
inner join Tailoringitems on Tailoringitems.id = Tailoringitemsissued.tailoringitemsid
inner join Tailoringorder on Tailoringorder.id = Tailoringitems.tailoringorderid ";
                //group by outwardno,issuedon";
                qb.AddParameter("Tailoringitemsissued.isactive", true, "=");
                if (item.companyid > 0)
                    qb.AddParameter("tailoringorder.companyid", item.companyid, "=");
                if (item.locationid > 0)
                    qb.AddParameter("tailoringorder.locationid", item.locationid, "=");
                if (item.issuedto > 0)
                    qb.AddParameter("Tailoringitemsissued.issuedto", item.issuedto, "=");
                if (item.fromdate.Year > 1900)
                    qb.AddParameter("Tailoringitemsissued.issuedon", "fromdate", item.fromdate.Date, ">=");
                if (item.todate.Year > 1900)
                    qb.AddParameter("Tailoringitemsissued.issuedon", "todate", item.todate.Date.AddDays(1), "<");

                qb.CurrentQuery = qb.CurrentQuery + " group by outwardno,issuedon::date";
                command = qb.GetCommand(conn);


                using (System.Data.IDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        TailoringOutwardBundle otailoringitemsissued = new TailoringOutwardBundle();
                        otailoringitemsissued.outwardno = reader["outwardno"] == DBNull.Value ? "" : Convert.ToString(reader["outwardno"]);
                        otailoringitemsissued.issuedqty = reader["issuedqty"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["issuedqty"]);
                        otailoringitemsissued.issuedon = reader["issuedon"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(reader["issuedon"]);
                        lst.Add(otailoringitemsissued);
                    }
                }
                conn.Close();
            }
            catch (Exception exp)
            {
                if (conn != null) conn.Close();
                WebAPI.Common.Logger.Instance(Context.OrganizationID).Log(exp, WebAPI.Common.Logger.LogType.Error, item);
            }
            return lst;
        }


        public List<Tailoringitemsissued> SearchAll(long _tailoringitemsid)
        {
            List<Tailoringitemsissued> lst = new List<Tailoringitemsissued>();
            NpgsqlConnection conn = CreateConnection();
            try
            {
                conn.Open();

                NpgsqlCommand command = new NpgsqlCommand(SQL_SearchAll, conn);
                command.Parameters.Add("tailoringitemsid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = _tailoringitemsid;
                using (System.Data.IDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        Tailoringitemsissued otailoringitemsissued = new Tailoringitemsissued();
                        otailoringitemsissued.id = reader["id"] == DBNull.Value ? 0 : Convert.ToInt64(reader["id"]);
                        otailoringitemsissued.outwardno = reader["outwardno"] == DBNull.Value ? "" : Convert.ToString(reader["outwardno"]);
                        otailoringitemsissued.tailoringitemsid = reader["tailoringitemsid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["tailoringitemsid"]);
                        otailoringitemsissued.issuedqty = reader["issuedqty"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["issuedqty"]);
                        otailoringitemsissued.issuedby = reader["issuedby"] == DBNull.Value ? 0 : Convert.ToInt64(reader["issuedby"]);
                        otailoringitemsissued.issuedto = reader["issuedto"] == DBNull.Value ? 0 : Convert.ToInt64(reader["issuedto"]);
                        otailoringitemsissued.issuedon = reader["issuedon"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(reader["issuedon"]);
                        otailoringitemsissued.createdon = reader["createdon"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(reader["createdon"]);
                        otailoringitemsissued.version = reader["version"] == DBNull.Value ? 0 : Convert.ToInt32(reader["version"]);
                        otailoringitemsissued.isactive = reader["isactive"] == DBNull.Value ? false : Convert.ToBoolean(reader["isactive"]);
                        otailoringitemsissued.modifiedby = reader["modifiedby"] == DBNull.Value ? 0 : Convert.ToInt64(reader["modifiedby"]);
                        otailoringitemsissued.createdby = reader["createdby"] == DBNull.Value ? 0 : Convert.ToInt64(reader["createdby"]);
                        otailoringitemsissued.modifiedon = reader["modifiedon"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(reader["modifiedon"]);
                        lst.Add(otailoringitemsissued);
                    }
                }
                conn.Close();
            }
            catch (Exception exp)
            {
                if (conn != null) conn.Close();
                WebAPI.Common.Logger.Instance(Context.OrganizationID).Log(exp, WebAPI.Common.Logger.LogType.Error, _tailoringitemsid);
            }
            return lst;
        }

        public List<TailoringitemsissuedDetail> Insert(List<TailoringitemsissuedDetail> oTailoringitemsissued)
        {
            if (oTailoringitemsissued.Exists(e => e.issuedto == 0))
                throw new Exception("Please select Issue to tailoring unit");
            if (oTailoringitemsissued.Exists(e => e.issuedby == 0))
                throw new Exception("Please select issued by employee name");
            if (oTailoringitemsissued.Exists(e => e.issuedon == DateTime.MinValue))
                throw new Exception("Please select date of issue");
            
            NpgsqlTransaction transaction = null;
            try
            {
                var sequence = SequenceManager.Instance(Context.OrganizationID).Get("tailoringorder.outwardno", oTailoringitemsissued.First().locationid);

                using (NpgsqlConnection conn = CreateConnection())
                {
                    conn.Open();
                    transaction = conn.BeginTransaction();
                    NpgsqlCommand command = new NpgsqlCommand(SQL_Insert, conn, transaction);
                    if (oTailoringitemsissued.Exists(e => !string.IsNullOrEmpty(e.outwardno)))
                    {
                        var existingoutward = oTailoringitemsissued.FindAll(e => !string.IsNullOrEmpty(e.outwardno));
                        var no = existingoutward.First().outwardno;
                        if (existingoutward.All(e => e.outwardno == no))
                        {
                            oTailoringitemsissued.ForEach(e => e.outwardno = no);
                        }
                        else
                        {
                            var bundleno = sequence.GetNextNumber(Context,conn, transaction);
                            oTailoringitemsissued.FindAll(e => string.IsNullOrEmpty(e.outwardno)).ForEach(e => e.outwardno = bundleno);
                        }
                    }
                    else
                    {
                        var bundleno = sequence.GetNextNumber(Context,conn, transaction);
                        oTailoringitemsissued.ForEach(e => e.outwardno = bundleno);
                    }

                    // To update the Order details
                    foreach (var item in oTailoringitemsissued.GroupBy(e => new { e.tailoringorderid }))
                    {
                        UpdateOrder(transaction, conn, item.First().tailoringorderid, 2, item.Sum(e => e.outwardqty));
                    }

                    foreach (var item in oTailoringitemsissued)
                    {
                        item.issuedqty = item.outwardqty;
                        Insert(transaction, conn, item);
                    }
                    transaction.Commit();
                    conn.Close();
                }
            }
            catch (Exception exp)
            {
                WebAPI.Common.Logger.Instance(Context.OrganizationID).Log(exp, WebAPI.Common.Logger.LogType.Error, oTailoringitemsissued);

            }

            return oTailoringitemsissued;
        }

        public Tailoringitemsissued Insert(Tailoringitemsissued oTailoringitemsissued)
        {

            NpgsqlTransaction transaction = null;
            try
            {
                using (NpgsqlConnection conn = CreateConnection())
                {
                    conn.Open();
                    transaction = conn.BeginTransaction();
                    NpgsqlCommand command = new NpgsqlCommand(SQL_Insert, conn, transaction);

                    Insert(transaction, conn, oTailoringitemsissued);
                    transaction.Commit();
                    conn.Close();
                }
            }
            catch (Exception exp)
            {
                WebAPI.Common.Logger.Instance(Context.OrganizationID).Log(exp, WebAPI.Common.Logger.LogType.Error, oTailoringitemsissued);

            }

            return oTailoringitemsissued;
        }

        public Tailoringitemsissued Insert(NpgsqlTransaction transaction, NpgsqlConnection conn, Tailoringitemsissued oTailoringitemsissued)
        {

            NpgsqlCommand command = new NpgsqlCommand(SQL_Insert, conn, transaction);
            command.Parameters.Add("outwardno", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oTailoringitemsissued.outwardno == null ? "" : oTailoringitemsissued.outwardno;
            command.Parameters.Add("tailoringitemsid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oTailoringitemsissued.tailoringitemsid;
            command.Parameters.Add("issuedqty", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oTailoringitemsissued.issuedqty;
            command.Parameters.Add("issuedby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oTailoringitemsissued.issuedby;
            command.Parameters.Add("issuedto", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oTailoringitemsissued.issuedto;
            command.Parameters.Add("issuedon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = oTailoringitemsissued.issuedon;
            command.Parameters.Add("createdon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = oTailoringitemsissued.createdon;
            command.Parameters.Add("version", NpgsqlTypes.NpgsqlDbType.Integer).Value = oTailoringitemsissued.version;
            command.Parameters.Add("isactive", NpgsqlTypes.NpgsqlDbType.Boolean).Value = oTailoringitemsissued.isactive;
            command.Parameters.Add("modifiedby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oTailoringitemsissued.modifiedby;
            command.Parameters.Add("createdby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oTailoringitemsissued.createdby;
            command.Parameters.Add("modifiedon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = oTailoringitemsissued.modifiedon;
            using (System.Data.IDataReader reader = command.ExecuteReader())
            {
                if (reader.Read())
                {
                    oTailoringitemsissued.id = Convert.ToInt64(reader[0].ToString());
                }
            }
            return oTailoringitemsissued;
        }

        public Tailoringitemsissued Update(Tailoringitemsissued oTailoringitemsissued)
        {

            NpgsqlTransaction transaction = null;
            try
            {
                using (NpgsqlConnection conn = CreateConnection())
                {
                    conn.Open();
                    transaction = conn.BeginTransaction();

                    Update(transaction, conn, oTailoringitemsissued);
                    transaction.Commit();
                    conn.Close();
                }
            }
            catch (Exception exp)
            {
                WebAPI.Common.Logger.Instance(Context.OrganizationID).Log(exp, WebAPI.Common.Logger.LogType.Error, oTailoringitemsissued);

            }

            return oTailoringitemsissued;
        }

        public Tailoringitemsissued Update(NpgsqlTransaction transaction, NpgsqlConnection conn, Tailoringitemsissued oTailoringitemsissued)
        {
            NpgsqlCommand command = new NpgsqlCommand(SQL_Update, conn, transaction);
            command.Parameters.Add("id", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oTailoringitemsissued.id;
            command.Parameters.Add("tailoringitemsid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oTailoringitemsissued.tailoringitemsid;
            command.Parameters.Add("issuedqty", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oTailoringitemsissued.issuedqty;
            command.Parameters.Add("issuedby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oTailoringitemsissued.issuedby;
            command.Parameters.Add("issuedon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = oTailoringitemsissued.issuedon;
            command.Parameters.Add("createdon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = oTailoringitemsissued.createdon;
            command.Parameters.Add("version", NpgsqlTypes.NpgsqlDbType.Integer).Value = oTailoringitemsissued.version;
            command.Parameters.Add("isactive", NpgsqlTypes.NpgsqlDbType.Boolean).Value = oTailoringitemsissued.isactive;
            command.Parameters.Add("modifiedby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oTailoringitemsissued.modifiedby;
            command.Parameters.Add("createdby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oTailoringitemsissued.createdby;
            command.Parameters.Add("modifiedon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = oTailoringitemsissued.modifiedon;
            using (System.Data.IDataReader reader = command.ExecuteReader())
            {
                if (reader.Read())
                {
                    oTailoringitemsissued.version = Convert.ToInt32(reader[0].ToString());
                }
            }

            return oTailoringitemsissued;
        }
        public bool UpdateOrder(NpgsqlTransaction transaction, NpgsqlConnection conn, long orderid, int status, decimal qty)
        {
            const string SQL_UpdateOrder = @"UPDATE public.tailoringorder SET status=@status,issuedqty=issuedqty+@qty, version=version+1, modifiedby=@modifiedby, modifiedon=@modifiedon where id=@id RETURNING version";

            NpgsqlCommand command = new NpgsqlCommand(SQL_UpdateOrder, conn, transaction);
            command.Parameters.Add("status", NpgsqlTypes.NpgsqlDbType.Integer).Value = status;
            command.Parameters.Add("qty", NpgsqlTypes.NpgsqlDbType.Numeric).Value = qty;
            command.Parameters.Add("modifiedby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = Context.UserID;
            command.Parameters.Add("modifiedon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = DateTimeNow;
            command.Parameters.Add("id", NpgsqlTypes.NpgsqlDbType.Bigint).Value = orderid;
            int successCount = command.ExecuteNonQuery();

            return successCount > 0;
        }

        public bool Update(NpgsqlTransaction transaction, NpgsqlConnection conn, List<Tailoringitemsissued> list, long id)
        {
            var existingItems = SearchAll(id);
            if (existingItems != null && existingItems.Count > 0)
            {
                var deletedItems = existingItems.FindAll(e => !list.Exists(f => f.id == e.id));
                foreach (var item in deletedItems)
                    Delete(transaction, conn, item);
            }

            foreach (var item in list)
            {
                if (item.id > 0)
                {
                    Update(transaction, conn, item);
                }
                else
                {
                    item.tailoringitemsid = id;
                    Insert(transaction, conn, item);
                }
            }
            return true;
        }

        public bool Delete(Tailoringitemsissued oTailoringitemsissued)
        {

            NpgsqlTransaction transaction = null;
            bool hasSuccessCount = false;
            try
            {
                using (NpgsqlConnection conn = CreateConnection())
                {
                    conn.Open();
                    transaction = conn.BeginTransaction();
                    hasSuccessCount = Delete(transaction, conn, oTailoringitemsissued);
                    transaction.Commit();
                    conn.Close();
                }
            }
            catch (Exception exp)
            {
                WebAPI.Common.Logger.Instance(Context.OrganizationID).Log(exp, WebAPI.Common.Logger.LogType.Error, oTailoringitemsissued);

            }
            return hasSuccessCount;

        }

        public bool Delete(NpgsqlTransaction transaction, NpgsqlConnection conn, Tailoringitemsissued oTailoringitemsissued)
        {

            NpgsqlCommand command = new NpgsqlCommand(SQL_Delete, conn, transaction);
            command.Parameters.Add("isactive", NpgsqlTypes.NpgsqlDbType.Boolean).Value = oTailoringitemsissued.isactive;
            command.Parameters.Add("modifiedby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oTailoringitemsissued.modifiedby;
            command.Parameters.Add("modifiedon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = oTailoringitemsissued.modifiedon;
            command.Parameters.Add("version", NpgsqlTypes.NpgsqlDbType.Integer).Value = oTailoringitemsissued.version;
            command.Parameters.Add("id", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oTailoringitemsissued.id;
            int successCount = command.ExecuteNonQuery();

            return successCount > 0;
        }

        public bool DeleteAll(long tailoringitemsid, long userid, NpgsqlConnection conn, NpgsqlTransaction transaction)
        {


            int successCount = 0;
            NpgsqlCommand command = new NpgsqlCommand(SQL_DeleteAll, conn, transaction);
            command.Parameters.Add("isactive", NpgsqlTypes.NpgsqlDbType.Boolean).Value = false;
            command.Parameters.Add("modifiedby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = userid;
            command.Parameters.Add("modifiedon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = DateTimeNow;
            command.Parameters.Add("tailoringitemsid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = tailoringitemsid;
            successCount = command.ExecuteNonQuery();

            return successCount > 0;
        }

    }

}
