using System;
using System.Collections.Generic;
using System.Linq;
using InfyPOS.Models;
using InfyPOS.Common;
using Npgsql;

namespace InfyPOS.Data

{
    public class DeliveryrackDataService : BaseData
    {
        const string SQL_Search = @"select disabled, deliveryrack.modifiedby, deliveryrack.id, deliveryrack.customerid,
                                    deliveryrack.locationid,deliveryrack.addinfo, deliveryrack.version, deliveryrack.isactive, 
                                    deliveryrack.createdon, deliveryrack.modifiedon, customer.name,billno,grp,row,col,
                                    deliveryrack.createdby, deliveryrack.code, deliveryrack.mobileno,pieces,deliveryrack.loyeltycardno
                                    from deliveryrack 
                                    left join customer on deliveryrack.customerid=customer.id                         
                                    where deliveryrack.id=@id";

        const string SQL_Insert = @"INSERT INTO public.deliveryrack(disabled, modifiedby, customerid,locationid,addinfo, version, isactive, createdon, modifiedon, createdby, code, mobileno,grp,row,col) 
                                    VALUES(@disabled, @modifiedby, @customerid,@locationid,@addinfo, @version, @isactive, @createdon, @modifiedon, @createdby, @code, @mobileno,@grp,@row,@col) RETURNING id";

        const string SQL_Update = @"UPDATE public.deliveryrack SET modifiedby=@modifiedby, locationid=@locationid,
                                    version=@version+1, modifiedon=@modifiedon, code=@code, grp=@grp,row=@row,col=@col
                                    where id=@id and version=@version RETURNING version";

        const string SQL_Delete = @"update public.deliveryrack set isactive=false,version=@version+1,modifiedby=@modifiedby,modifiedon=@modifiedon where id=@id and version=@version";

        /*

CREATE TRIGGER deliveryrack_update_trg 
BEFORE UPDATE ON deliveryrack
	FOR EACH ROW EXECUTE FUNCTION deliveryrack_update()


CREATE OR REPLACE FUNCTION public.deliveryrack_update()
 RETURNS trigger
 LANGUAGE plpgsql
AS $function$
BEGIN
    IF NEW.isactive=false and New.customerid>0 THEN
        RAISE EXCEPTION 'Non-Active rack ';
    END IF;
	    IF NEW.disabled=true and New.customerid>0 THEN
        RAISE EXCEPTION 'Disabled rack ';
    END IF;
    RETURN NEW;
END;
$function$
*/

        public DeliveryrackDetail Find(long id)
        {
            DeliveryrackDetail odeliveryrack = new DeliveryrackDetail();
            NpgsqlConnection conn = CreateConnection();
            try
            {
                conn.Open();

                NpgsqlCommand command = null;
                command = new NpgsqlCommand(SQL_Search, conn);
                command.Parameters.Add("id", NpgsqlTypes.NpgsqlDbType.Bigint).Value = id;

                using (System.Data.IDataReader reader = command.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        odeliveryrack.disabled = reader["disabled"] == DBNull.Value ? false : Convert.ToBoolean(reader["disabled"]);
                        odeliveryrack.modifiedby = reader["modifiedby"] == DBNull.Value ? 0 : Convert.ToInt64(reader["modifiedby"]);
                        odeliveryrack.locationid = reader["locationid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["locationid"]);
                        odeliveryrack.id = reader["id"] == DBNull.Value ? 0 : Convert.ToInt64(reader["id"]);
                        odeliveryrack.pieces = reader["pieces"] == DBNull.Value ? 0 : Convert.ToInt32(reader["pieces"]);
                        odeliveryrack.customerid = reader["customerid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["customerid"]);
                        odeliveryrack.version = reader["version"] == DBNull.Value ? 0 : Convert.ToInt32(reader["version"]);
                        odeliveryrack.isactive = reader["isactive"] == DBNull.Value ? false : Convert.ToBoolean(reader["isactive"]);
                        odeliveryrack.createdon = reader["createdon"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(reader["createdon"]);
                        odeliveryrack.modifiedon = reader["modifiedon"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(reader["modifiedon"]);
                        odeliveryrack.createdby = reader["createdby"] == DBNull.Value ? 0 : Convert.ToInt64(reader["createdby"]);
                        odeliveryrack.code = reader["code"] == DBNull.Value ? "" : reader["code"].ToString();
                        odeliveryrack.billno = reader["billno"] == DBNull.Value ? "" : reader["billno"].ToString();
                        odeliveryrack.customer = reader["name"] == DBNull.Value ? "" : reader["name"].ToString();
                        odeliveryrack.addinfo_json = reader["addinfo"] == DBNull.Value ? null : reader["addinfo"].ToString();
                        odeliveryrack.mobileno = reader["mobileno"] == DBNull.Value ? "" : reader["mobileno"].ToString();
                        odeliveryrack.grp = reader["grp"] == DBNull.Value ? "" : reader["grp"].ToString();
                        odeliveryrack.row = reader["row"] == DBNull.Value ? "" : reader["row"].ToString();
                        odeliveryrack.col = reader["col"] == DBNull.Value ? "" : reader["col"].ToString();
                        odeliveryrack.card = reader["loyeltycardno"] == DBNull.Value ? "" : reader["loyeltycardno"].ToString();
                    }
                }
                conn.Close();
            }
            catch (Exception exp)
            {
                if (conn != null) conn.Close();
                WebAPI.Common.Logger.Instance(Context.OrganizationID).Log(exp, WebAPI.Common.Logger.LogType.Error, "finddelivery rack - " + id);
            }
            return odeliveryrack;
        }

        public List<Deliveryrack> Search(DeliveryrackCreteria _deliveryrack)
        {
            List<Deliveryrack> lst = new List<Deliveryrack>();
            NpgsqlConnection conn = CreateConnection();
            try
            {
                conn.Open();
                NpgsqlCommand command = null;
                if (_deliveryrack.id != 0)
                {
                    command = new NpgsqlCommand(SQL_Search, conn);
                    command.Parameters.Add("id", NpgsqlTypes.NpgsqlDbType.Bigint).Value = _deliveryrack.id;
                }
                else
                {
                    QueryBuilder qb = new QueryBuilder();
                    qb.CurrentQuery = SQL_Search.Substring(0, SQL_Search.IndexOf("where"));
                    qb.AddParameter("deliveryrack.isactive", true, "=");
                    if (!string.IsNullOrEmpty(_deliveryrack.billno))
                    {
                        qb.Add2Query("(deliveryrack.billno = @loyeltycardno OR deliveryrack.billno ILIKE @loyeltycardno || '(%' or deliveryrack.loyeltycardno = @loyeltycardno)");
                    }
                    else
                    {
                        qb.AddParameter("deliveryrack.disabled", _deliveryrack.ShowInactive, "=");
                        qb.AddParameter("deliveryrack.locationid", _deliveryrack.locationid, "=");
                        if (_deliveryrack.customerid > 0)
                            qb.AddParameter("deliveryrack.customerid", _deliveryrack.customerid, "=");
                        if (_deliveryrack.ids != null && _deliveryrack.ids.Count > 0)
                            qb.AddInParamer("deliveryrack.id", string.Join(",", _deliveryrack.ids));
                    }
                    qb.SortField = "grp,row,col";
                    command = qb.GetCommand(conn);
                    command.Parameters.Add("loyeltycardno", NpgsqlTypes.NpgsqlDbType.Varchar).Value = _deliveryrack.billno == null ? "" : _deliveryrack.billno;
                }

                using (System.Data.IDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        Deliveryrack odeliveryrack = new Deliveryrack();
                        odeliveryrack.disabled = reader["disabled"] == DBNull.Value ? false : Convert.ToBoolean(reader["disabled"]);
                        odeliveryrack.modifiedby = reader["modifiedby"] == DBNull.Value ? 0 : Convert.ToInt64(reader["modifiedby"]);
                        odeliveryrack.locationid = reader["locationid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["locationid"]);
                        odeliveryrack.id = reader["id"] == DBNull.Value ? 0 : Convert.ToInt64(reader["id"]);
                        odeliveryrack.pieces = reader["pieces"] == DBNull.Value ? 0 : Convert.ToInt32(reader["pieces"]);
                        odeliveryrack.customerid = reader["customerid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["customerid"]);
                        odeliveryrack.version = reader["version"] == DBNull.Value ? 0 : Convert.ToInt32(reader["version"]);
                        odeliveryrack.isactive = reader["isactive"] == DBNull.Value ? false : Convert.ToBoolean(reader["isactive"]);
                        odeliveryrack.createdon = reader["createdon"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(reader["createdon"]);
                        odeliveryrack.modifiedon = reader["modifiedon"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(reader["modifiedon"]);
                        odeliveryrack.createdby = reader["createdby"] == DBNull.Value ? 0 : Convert.ToInt64(reader["createdby"]);
                        odeliveryrack.code = reader["code"] == DBNull.Value ? "" : reader["code"].ToString();
                        odeliveryrack.billno = reader["billno"] == DBNull.Value ? "" : reader["billno"].ToString();
                        odeliveryrack.customer = reader["name"] == DBNull.Value ? "" : reader["name"].ToString();
                        odeliveryrack.addinfo_json = reader["addinfo"] == DBNull.Value ? null : reader["addinfo"].ToString();
                        odeliveryrack.mobileno = reader["mobileno"] == DBNull.Value ? "" : reader["mobileno"].ToString();
                        odeliveryrack.card = reader["loyeltycardno"] == DBNull.Value ? "" : reader["loyeltycardno"].ToString();
                        lst.Add(odeliveryrack);
                    }
                }
                conn.Close();
            }
            catch (Exception exp)
            {
                if (conn != null) conn.Close();
                WebAPI.Common.Logger.Instance(Context.OrganizationID).Log(exp, WebAPI.Common.Logger.LogType.Error, _deliveryrack);
            }
            return lst;
        }

        public Deliveryrack Insert(DeliveryrackDetail oDeliveryrack)
        {

            NpgsqlTransaction transaction = null;
            try
            {
                using (NpgsqlConnection conn = CreateConnection())
                {
                    conn.Open();
                    transaction = conn.BeginTransaction();
                    if (oDeliveryrack.code.IndexOf(",") > 0)
                    {
                        foreach (var code in oDeliveryrack.code.Split(',').ToList())
                        {
                            oDeliveryrack.code = code;
                            oDeliveryrack.isactive = true;
                            oDeliveryrack.ChangeStatus(BaseEntity.DataStatus.Create);
                            Insert(transaction, conn, oDeliveryrack);
                        }
                    }
                    else
                    {
                        Insert(transaction, conn, oDeliveryrack);
                    }
                    transaction.Commit();
                    conn.Close();
                }
            }
            catch (Exception exp)
            {
                WebAPI.Common.Logger.Instance(Context.OrganizationID).Log(exp, WebAPI.Common.Logger.LogType.Error, oDeliveryrack);
                if (transaction != null)
                    transaction.Rollback();
            }

            return oDeliveryrack;
        }

        public Deliveryrack Insert(NpgsqlTransaction transaction, NpgsqlConnection conn, DeliveryrackDetail oDeliveryrack)
        {
            if (string.IsNullOrEmpty(oDeliveryrack.code)) throw new Exception("Empty code can't accept");
            if (oDeliveryrack.locationid <= 0) throw new Exception("Please select the location");
            NpgsqlCommand command = new NpgsqlCommand(SQL_Insert, conn, transaction);
            command.Parameters.Add("disabled", NpgsqlTypes.NpgsqlDbType.Boolean).Value = oDeliveryrack.disabled;
            command.Parameters.Add("modifiedby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oDeliveryrack.modifiedby;
            command.Parameters.Add("customerid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oDeliveryrack.customerid;
            command.Parameters.Add("locationid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oDeliveryrack.locationid;
            command.Parameters.Add("row", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oDeliveryrack.row;
            command.Parameters.Add("grp", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oDeliveryrack.grp;
            command.Parameters.Add("col", NpgsqlTypes.NpgsqlDbType.Integer).Value = oDeliveryrack.col;
            command.Parameters.Add("version", NpgsqlTypes.NpgsqlDbType.Integer).Value = oDeliveryrack.version;
            command.Parameters.Add("isactive", NpgsqlTypes.NpgsqlDbType.Boolean).Value = oDeliveryrack.isactive;
            command.Parameters.Add("createdon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = oDeliveryrack.createdon;
            command.Parameters.Add("modifiedon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = oDeliveryrack.modifiedon;
            command.Parameters.Add("createdby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oDeliveryrack.createdby;
            command.Parameters.Add("code", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oDeliveryrack.code == null ? "" : oDeliveryrack.code;
            command.Parameters.Add("mobileno", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oDeliveryrack.mobileno == null ? "" : oDeliveryrack.mobileno;
            command.Parameters.Add("addinfo", NpgsqlTypes.NpgsqlDbType.Json).Value = oDeliveryrack.addinfo == null ? "null" : oDeliveryrack.addinfo_json;

            using (System.Data.IDataReader reader = command.ExecuteReader())
            {
                if (reader.Read())
                {
                    oDeliveryrack.id = Convert.ToInt64(reader[0].ToString());
                }
            }
            return oDeliveryrack;
        }

        public DeliveryrackDetail Update(DeliveryrackDetail oDeliveryrack)
        {

            NpgsqlTransaction transaction = null;
            try
            {
                using (NpgsqlConnection conn = CreateConnection())
                {
                    conn.Open();
                    transaction = conn.BeginTransaction();

                    Update(transaction, conn, oDeliveryrack);
                    transaction.Commit();
                    conn.Close();
                }
            }
            catch (Exception exp)
            {
                WebAPI.Common.Logger.Instance(Context.OrganizationID).Log(exp, WebAPI.Common.Logger.LogType.Error, oDeliveryrack);
                if (transaction != null)
                    transaction.Rollback();
            }

            return oDeliveryrack;
        }

        public DeliveryrackDetail Update(NpgsqlTransaction transaction, NpgsqlConnection conn, DeliveryrackDetail oDeliveryrack)
        {
            NpgsqlCommand command = new NpgsqlCommand(SQL_Update, conn, transaction);
            command.Parameters.Add("disabled", NpgsqlTypes.NpgsqlDbType.Boolean).Value = oDeliveryrack.disabled;
            command.Parameters.Add("modifiedby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oDeliveryrack.modifiedby;
            command.Parameters.Add("id", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oDeliveryrack.id;
            command.Parameters.Add("customerid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oDeliveryrack.customerid;
            command.Parameters.Add("locationid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oDeliveryrack.locationid;
            command.Parameters.Add("row", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oDeliveryrack.row;
            command.Parameters.Add("grp", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oDeliveryrack.grp;
            command.Parameters.Add("col", NpgsqlTypes.NpgsqlDbType.Integer).Value = oDeliveryrack.col;
            command.Parameters.Add("version", NpgsqlTypes.NpgsqlDbType.Integer).Value = oDeliveryrack.version;
            command.Parameters.Add("isactive", NpgsqlTypes.NpgsqlDbType.Boolean).Value = oDeliveryrack.isactive;
            command.Parameters.Add("createdon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = oDeliveryrack.createdon;
            command.Parameters.Add("modifiedon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = oDeliveryrack.modifiedon;
            command.Parameters.Add("createdby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oDeliveryrack.createdby;
            command.Parameters.Add("code", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oDeliveryrack.code == null ? "" : oDeliveryrack.code;
            command.Parameters.Add("mobileno", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oDeliveryrack.mobileno == null ? "" : oDeliveryrack.mobileno;
            command.Parameters.Add("addinfo", NpgsqlTypes.NpgsqlDbType.Json).Value = oDeliveryrack.addinfo == null ? "null" : oDeliveryrack.addinfo_json;
            using (System.Data.IDataReader reader = command.ExecuteReader())
            {
                if (reader.Read())
                {
                    oDeliveryrack.version = Convert.ToInt32(reader[0].ToString());
                }
            }
            return oDeliveryrack;
        }


        public bool Delete(Deliveryrack oDeliveryrack)
        {

            NpgsqlTransaction transaction = null;
            bool hasSuccessCount = false;
            try
            {
                using (NpgsqlConnection conn = CreateConnection())
                {
                    conn.Open();
                    transaction = conn.BeginTransaction();
                    hasSuccessCount = Delete(transaction, conn, oDeliveryrack);
                    transaction.Commit();
                    conn.Close();
                }
            }
            catch (Exception exp)
            {
                WebAPI.Common.Logger.Instance(Context.OrganizationID).Log(exp, WebAPI.Common.Logger.LogType.Error, oDeliveryrack);

                if (transaction != null)
                    transaction.Rollback();
            }
            return hasSuccessCount;

        }

        public bool Delete(NpgsqlTransaction transaction, NpgsqlConnection conn, Deliveryrack oDeliveryrack)
        {
            NpgsqlCommand command = new NpgsqlCommand(SQL_Delete, conn, transaction);
            command.Parameters.Add("isactive", NpgsqlTypes.NpgsqlDbType.Boolean).Value = oDeliveryrack.isactive;
            command.Parameters.Add("modifiedby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oDeliveryrack.modifiedby;
            command.Parameters.Add("modifiedon", NpgsqlTypes.NpgsqlDbType.TimestampTZ).Value = oDeliveryrack.modifiedon;
            command.Parameters.Add("version", NpgsqlTypes.NpgsqlDbType.Integer).Value = oDeliveryrack.version;
            command.Parameters.Add("id", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oDeliveryrack.id;
            int successCount = command.ExecuteNonQuery();

            return successCount > 0;
        }


        internal List<Deliveryrack> SetRackFree(DeliveryrackCreteria item)
        {
            if (string.IsNullOrEmpty(item.billno)) return null;
            var response = Search(new DeliveryrackCreteria()
            {
                billno = item.billno,
                card = item.billno
            });
            if (response.Count == 0) return null;
            try
            {
                using (NpgsqlConnection conn = CreateConnection())
                {
                    conn.Open();
                    var transaction = conn.BeginTransaction();
                    var query = @"Update deliveryrack set blockedon=null,mobileno='',billno='',loyeltycardno='',pieces=0,hasblocked=false,customerid=0,addinfo=null,releasedon=current_date+current_time where id=@id returning id,code";
                    NpgsqlCommand command = new NpgsqlCommand(query, conn, transaction);
                    var param = new SortedList<string, object>();
                    param.Add("id", response[0].id);
                    using (var reader = ExecuteReader(query, param, conn))
                    {
                        if (reader.Read())
                        {
                            item.id = reader.GetInt64(0);
                            item.code = reader.GetString(1);
                        }
                    }
                    UpdateDelivery(conn, transaction, response[0]);
                    transaction.Commit();
                    conn.Close();
                }
            }
            catch (Exception exp)
            {
                WebAPI.Common.Logger.Instance(Context.OrganizationID).Log(exp, WebAPI.Common.Logger.LogType.Error, item);
            }
            if (item.id > 0)
                return response;
            return new List<Deliveryrack>();
        }

        internal void UpdateDelivery(NpgsqlConnection connection, NpgsqlTransaction transaction, Deliveryrack deliveryrack)
        {
            string query = @"insert into delivery(receivedon,deliveredby,deliveredon,billno,amount,locationid,deliveryrackid,qty,customername,contactno,isactive)
                              values(@receivedon,@deliveredby,@deliveredon,@billno,@amount,@locationid,@deliveryrackid,@qty,@customername,@contactno,true)";

            NpgsqlCommand command = new NpgsqlCommand(query, connection, transaction);
            command.Parameters.Add("receivedon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = deliveryrack.addinfo.starton;
            command.Parameters.Add("deliveredby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = Context.UserID;
            command.Parameters.Add("deliveredon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = DateTimeNow;
            command.Parameters.Add("billno", NpgsqlTypes.NpgsqlDbType.Varchar).Value = deliveryrack.billno == null ? "" : deliveryrack.billno;
            command.Parameters.Add("amount", NpgsqlTypes.NpgsqlDbType.Numeric).Value = deliveryrack.addinfo.amount;
            command.Parameters.Add("locationid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = deliveryrack.locationid;
            command.Parameters.Add("deliveryrackid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = deliveryrack.id;
            command.Parameters.Add("qty", NpgsqlTypes.NpgsqlDbType.Numeric).Value = deliveryrack.addinfo.noofpieces;
            command.Parameters.Add("customername", NpgsqlTypes.NpgsqlDbType.Varchar).Value = deliveryrack.customer == null ? "" : deliveryrack.customer;
            command.Parameters.Add("contactno", NpgsqlTypes.NpgsqlDbType.Varchar).Value = deliveryrack.mobileno == null ? "" : deliveryrack.mobileno;
            command.ExecuteNonQuery();
        }

        public List<DeliveryRackHistory> DeliveryRackHistory(DeliveryRackHistory deliveryrack)
        {
            string query = @"select receivedon, deliveredby, deliveredon, billno, amount, 
                            delivery.locationid, deliveryrackid, qty, customername, 
							delivery.contactno,employee.name employee
                     from delivery
                     inner join users on delivery.deliveredby=users.id
                     left outer join employee on employee.id = users.employeeid
                     where deliveryrackid=@deliveryrackid 
                       and deliveredon >= @fromdate 
                       and deliveredon < @todate";

            var param = new SortedList<string, object>
    {
        { "deliveryrackid", deliveryrack.deliveryrackid },
        { "fromdate", deliveryrack.deliveredon.Date },
        { "todate", deliveryrack.deliveredon.Date.AddDays(1) }
    };

            var deliveryRackHistoryList = new List<DeliveryRackHistory>();

            using (var reader = ExecuteReader(query, param))
            {
                while (reader.Read())
                {
                    var deliveryRackHistory = new DeliveryRackHistory
                    {
                        deliveryrackid = reader["deliveryrackid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["deliveryrackid"]),
                        receivedon = reader["receivedon"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(reader["receivedon"]),
                        deliveredon = reader["deliveredon"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(reader["deliveredon"]),
                        deliveredby = reader["employee"] == DBNull.Value ? string.Empty : reader["employee"].ToString(),
                        billno = reader["billno"] == DBNull.Value ? string.Empty : reader["billno"].ToString(),
                        customername = reader["customername"] == DBNull.Value ? string.Empty : reader["customername"].ToString(),
                        contactno = reader["contactno"] == DBNull.Value ? string.Empty : reader["contactno"].ToString(),
                        amount = reader["amount"] == DBNull.Value ? 0m : Convert.ToDecimal(reader["amount"]),
                        qty = reader["qty"] == DBNull.Value ? 0m : Convert.ToDecimal(reader["qty"])
                    };
                    deliveryRackHistoryList.Add(deliveryRackHistory);
                }
            }

            return deliveryRackHistoryList;
        }

        internal List<Deliveryrack> UpdateStatus(DeliveryrackCreteria item)
        {
            if (item == null || item.ids == null || item.ids.Count == 0) return null;

            try
            {
                using (NpgsqlConnection conn = CreateConnection())
                {
                    conn.Open();
                    string query = "Update deliveryrack set disabled=@disabled where id in (" + string.Join(",", item.ids) + ") and customerid=0";
                    if (item.hasdelete)
                        query = "Update deliveryrack set isactive=false where id in (" + string.Join(",", item.ids) + ") and customerid=0";
                    if (item.hasclear)
                        query = "Update deliveryrack set blockedon=null,mobileno='',billno='',loyeltycardno='',pieces=0,hasblocked=false,customerid=0,addinfo=null,releasedon=current_date+current_time where id in (" + string.Join(",", item.ids) + ")";
                    NpgsqlCommand command = new NpgsqlCommand(query, conn);
                    command.Parameters.Add("disabled", NpgsqlTypes.NpgsqlDbType.Boolean).Value = item.disabled;
                    command.ExecuteNonQuery();
                    conn.Close();
                }
            }
            catch (Exception exp)
            {
                WebAPI.Common.Logger.Instance(Context.OrganizationID).Log(exp, WebAPI.Common.Logger.LogType.Error, item);
            }
            if (item.hasdelete)
                return null;
            if (item.hasclear)
                return Search(new DeliveryrackCreteria()
                {
                    ids = item.ids
                });

            return Search(new DeliveryrackCreteria()
            {
                ShowInactive = !item.disabled
            });
        }

        public bool UpdateFreeRack(Billmaster oBillmaster, NpgsqlConnection conn, NpgsqlTransaction transaction)
        {
            var layeltycardno = (oBillmaster.billattributes != null && !string.IsNullOrEmpty(oBillmaster.billattributes.deliveryrackcard)) ? oBillmaster.billattributes.deliveryrackcard : "";
            if (string.IsNullOrEmpty(layeltycardno))
                layeltycardno = (oBillmaster.Bills[0].billattributes != null && !string.IsNullOrEmpty(oBillmaster.Bills[0].billattributes.deliveryrackcard)) ? oBillmaster.Bills[0].billattributes.deliveryrackcard : "";

            if (oBillmaster.customerid == 0 && string.IsNullOrWhiteSpace(layeltycardno))
                throw new Exception("Customer is mandatory for Delivery Rack Support");

            bool isnewallocation = false;
            Deliveryrack deliveryrack = new Deliveryrack();
            var param = new SortedList<string, object>();
            param.Add("blockedon", DateTime.Now);
            param.Add("customerid", oBillmaster.customerid);
            param.Add("locationid", oBillmaster.locationid);
            param.Add("loyeltycardno", layeltycardno);
            string lockexistingquery = "UPDATE deliveryrack SET blockedon=@blockedon,hasblocked=true where id=(select id from deliveryrack where hasblocked=true and isactive=true and disabled=false and loyeltycardno=@loyeltycardno and locationid=@locationid limit 1) returning id,code,loyeltycardno";
            if (!string.IsNullOrEmpty(layeltycardno))
            {
                using (var reader = ExecuteReader(lockexistingquery, param, conn, transaction))
                {
                    if (reader.Read())
                    {
                        deliveryrack.id = reader.GetInt64(0);
                        deliveryrack.code = reader.GetString(1);
                        deliveryrack.card = reader["loyeltycardno"] == DBNull.Value ? "" : reader.GetString(2);
                    }
                }
            }

            if (string.IsNullOrEmpty(layeltycardno) && deliveryrack.id == 0 && oBillmaster.customerid > 0)
            {
                lockexistingquery = "UPDATE deliveryrack SET blockedon=@blockedon,hasblocked=true where id=(select id from deliveryrack where hasblocked=true and isactive=true and disabled=false and customerid=@customerid and locationid=@locationid limit 1) returning id,code,loyeltycardno";
                using (var reader = ExecuteReader(lockexistingquery, param, conn, transaction))
                {
                    if (reader.Read())
                    {
                        deliveryrack.id = reader.GetInt64(0);
                        deliveryrack.code = reader.GetString(1);
                        deliveryrack.card = reader["loyeltycardno"] == DBNull.Value ? "" : reader.GetString(2);
                    }
                }
            }

            if (deliveryrack.id == 0)
            {
                if (Services.ConfigManager.Instance(Context.OrganizationID).Config.system_deliveryrackauto)
                {
                    lockexistingquery = "UPDATE deliveryrack SET blockedon=@blockedon,hasblocked=true,loyeltycardno=@loyeltycardno where id=(select id from deliveryrack where hasblocked=false and disabled=false and isactive=true and locationid=@locationid order by releasedon limit 1) returning id,code,loyeltycardno";
                }
                else
                {
                    lockexistingquery = "UPDATE deliveryrack SET blockedon=@blockedon,hasblocked=true,loyeltycardno=@loyeltycardno where id=(select id from deliveryrack where hasblocked=false and isactive=true and disabled=false and locationid=@locationid order by grp,row,col asc limit 1) returning id,code,loyeltycardno";
                }
                using (var reader = ExecuteReader(lockexistingquery, param, conn, transaction))
                {
                    if (reader.Read())
                    {
                        deliveryrack.id = reader.GetInt64(0);
                        deliveryrack.code = reader.GetString(1);
                        deliveryrack.card = reader["loyeltycardno"] == DBNull.Value ? "" : reader.GetString(2);
                    }
                }
                if (deliveryrack.id == 0)
                    throw new Exception("No free rack avilable, please create new racks");
                isnewallocation = true;
            }

            if (oBillmaster.billattributes == null)
                oBillmaster.billattributes = new BillAttribute();
            oBillmaster.billattributes.deliveryrack = deliveryrack.code;
            oBillmaster.billattributes.deliveryrackid = deliveryrack.id;
            oBillmaster.billattributes.deliveryrackft = isnewallocation;
            oBillmaster.billattributes.deliveryrackcard = deliveryrack.card;
            oBillmaster.Bills.ForEach(x =>
            {
                if (x.billattributes == null)
                    x.billattributes = new BillAttribute();
                x.billattributes.deliveryrack = deliveryrack.code;
                x.billattributes.deliveryrackid = deliveryrack.id;
                x.billattributes.deliveryrackft = isnewallocation;
                x.billattributes.deliveryrackcard = deliveryrack.card;
            });
            return true;
        }
        internal bool AssignRack(Billmaster oBillmaster, NpgsqlConnection conn, NpgsqlTransaction transaction, List<Bill> Cancelledestimates)
        {
            long deliveryrackid = oBillmaster.billattributes.deliveryrackid;
            if (deliveryrackid == 0 && oBillmaster.Bills.Any(x => x.billattributes != null && x.billattributes.deliveryrackid > 0))
                deliveryrackid = oBillmaster.Bills.First(x => x.billattributes != null && x.billattributes.deliveryrackid > 0).billattributes.deliveryrackid;

            var command = new NpgsqlCommand(SQL_Search, conn, transaction);
            command.Parameters.Add("id", NpgsqlTypes.NpgsqlDbType.Bigint).Value = deliveryrackid;
            Deliveryrack odeliveryrack = new Deliveryrack();
            using (System.Data.IDataReader reader = command.ExecuteReader())
            {
                if (reader.Read())
                {
                    odeliveryrack.id = reader["id"] == DBNull.Value ? 0 : Convert.ToInt64(reader["id"]);
                    odeliveryrack.customerid = reader["customerid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["customerid"]);
                    odeliveryrack.addinfo_json = reader["addinfo"] == DBNull.Value ? null : reader["addinfo"].ToString();
                    odeliveryrack.mobileno = reader["mobileno"] == DBNull.Value ? "" : reader["mobileno"].ToString();
                }
            }

            if (odeliveryrack.addinfo == null)
            {
                odeliveryrack.addinfo = new Deliveryrack.AddInfo();
                odeliveryrack.addinfo.starton = DateTimeNow;
            }

            if (Cancelledestimates != null && Cancelledestimates.Count > 0)
            {
                odeliveryrack.addinfo.billnos.Remove(Cancelledestimates.First().billno);
                odeliveryrack.addinfo.billids.Remove(Cancelledestimates.First().id);
                odeliveryrack.addinfo.noofbills -= 1;
                odeliveryrack.addinfo.noofpieces -= Cancelledestimates.Sum(e => e.totalpiece);
                odeliveryrack.addinfo.amount -= Cancelledestimates.Sum(e => e.total);
                odeliveryrack.addinfo.gross -= Cancelledestimates.Sum(e => e.salevaluewithdiscount);
            }

            odeliveryrack.addinfo.updateon = DateTimeNow;
            odeliveryrack.customerid = oBillmaster.customerid;
            odeliveryrack.mobileno = oBillmaster.customermobileno;
            odeliveryrack.addinfo.billnos.AddRange(oBillmaster.Bills.ConvertAll(x => x.billno));
            odeliveryrack.addinfo.billids.AddRange(oBillmaster.Bills.ConvertAll(x => x.id));
            odeliveryrack.addinfo.noofbills += oBillmaster.Bills.Count;
            odeliveryrack.addinfo.noofpieces += oBillmaster.Bills.Sum(x => x.totalpiece);
            odeliveryrack.addinfo.amount += oBillmaster.Bills.Sum(x => x.total);
            odeliveryrack.addinfo.gross += oBillmaster.Bills.Sum(x => x.salevaluewithdiscount);

            string SQL_Block = @"UPDATE public.deliveryrack SET customerid=@customerid, addinfo=@addinfo, mobileno=@mobileno 
                                 where id=@id";

            oBillmaster.billattributes.deliveryrackinfo = odeliveryrack.addinfo;
            oBillmaster.Bills.ForEach(x =>
            {
                if (x.billattributes == null)
                    x.billattributes = new BillAttribute();
                x.billattributes.deliveryrackinfo = odeliveryrack.addinfo;
            });

            command = new NpgsqlCommand(SQL_Block, conn, transaction);
            command.Parameters.Add("id", NpgsqlTypes.NpgsqlDbType.Bigint).Value = odeliveryrack.id;
            command.Parameters.Add("customerid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = odeliveryrack.customerid;
            command.Parameters.Add("mobileno", NpgsqlTypes.NpgsqlDbType.Varchar).Value = odeliveryrack.mobileno == null ? "" : odeliveryrack.mobileno;
            command.Parameters.Add("addinfo", NpgsqlTypes.NpgsqlDbType.Json).Value = odeliveryrack.addinfo == null ? "null" : odeliveryrack.addinfo_json;
            return command.ExecuteNonQuery() > 0;
        }

        internal Deliveryrack UpdateSettlement(Billmaster oBillmaster, NpgsqlConnection conn, NpgsqlTransaction transaction)
        {
            Deliveryrack deliveryrack = new Deliveryrack();
            var param = new SortedList<string, object>();
            param.Add("id", oBillmaster.billattributes.deliveryrackid);

            string billno = oBillmaster.Bills.First().billno;
            if (oBillmaster.Bills.First().billattributes != null &&
               oBillmaster.Bills.First().billattributes.ebillno != null)
                billno += "(" + string.Join(",", oBillmaster.Bills.First().billattributes.ebillno.ConvertAll(x => x.billno)) + ")";
            param.Add("billno", billno);
            param.Add("pieces", oBillmaster.Bills.Sum(x => x.totalpiece));
            param.Add("settlementcode", oBillmaster.code);
            string lockexistingquery = "UPDATE deliveryrack SET billno=@billno,pieces=@pieces where id=@id  returning id,code";
            using (var reader = ExecuteReader(lockexistingquery, param, conn, transaction))
            {
                if (reader.Read())
                {
                    deliveryrack.id = reader.GetInt64(0);
                    deliveryrack.code = reader.GetString(1);
                }
            }

            if (oBillmaster.billattributes == null)
                oBillmaster.billattributes = new BillAttribute();

            oBillmaster.billattributes.deliveryrack = deliveryrack.code;
            oBillmaster.billattributes.deliveryrackid = deliveryrack.id;
            oBillmaster.Bills.ForEach(x =>
            {
                if (x.billattributes == null)
                    x.billattributes = new BillAttribute();
                x.billattributes.deliveryrack = deliveryrack.code;
                x.billattributes.deliveryrackid = deliveryrack.id;
            });

            return deliveryrack;
        }

        internal Deliveryrack GetRack(Billmaster oBillmaster, NpgsqlConnection conn, NpgsqlTransaction transaction)
        {
            var layeltycardno = (oBillmaster.billattributes != null && !string.IsNullOrEmpty(oBillmaster.billattributes.deliveryrackcard)) ? oBillmaster.billattributes.deliveryrackcard : "";
            if (string.IsNullOrEmpty(layeltycardno))
                layeltycardno = (oBillmaster.Bills[0].billattributes != null && !string.IsNullOrEmpty(oBillmaster.Bills[0].billattributes.deliveryrackcard)) ? oBillmaster.Bills[0].billattributes.deliveryrackcard : "";

            Deliveryrack deliveryrack = new Deliveryrack();
            var param = new SortedList<string, object>();
            param.Add("blockedon", DateTime.Now);
            param.Add("customerid", oBillmaster.customerid);
            param.Add("customer", string.IsNullOrEmpty(oBillmaster.customername) ? "" : oBillmaster.customername);
            param.Add("mobileno", string.IsNullOrEmpty(oBillmaster.customermobileno) ? "" : oBillmaster.customermobileno);
            param.Add("locationid", oBillmaster.locationid);
            param.Add("loyeltycardno", string.IsNullOrEmpty(layeltycardno) ? "" : layeltycardno);
            string lockexistingquery = "UPDATE deliveryrack SET blockedon=@blockedon,hasblocked=true where id=(select id from deliveryrack where hasblocked=true and isactive=true and disabled=false and loyeltycardno=@loyeltycardno and locationid=@locationid limit 1) returning id,code,loyeltycardno,mobileno,customerid";
            if (!string.IsNullOrEmpty(layeltycardno))
            {
                using (var reader = ExecuteReader(lockexistingquery, param, conn, transaction))
                {
                    if (reader.Read())
                    {
                        deliveryrack.id = reader.GetInt64(0);
                        deliveryrack.code = reader.IsDBNull(1) ? "" : reader.GetString(1);
                        deliveryrack.card = reader["loyeltycardno"] == DBNull.Value ? "" : reader.GetString(2);
                        deliveryrack.mobileno = reader["mobileno"] == DBNull.Value ? "" : reader.GetString(3);
                        deliveryrack.customerid = reader.IsDBNull(4) ? 0 : reader.GetInt64(4);
                    }
                }
            }

            if (deliveryrack.id == 0 && oBillmaster.customerid > 0)
            {
                lockexistingquery = "UPDATE deliveryrack SET blockedon=@blockedon,hasblocked=true where id=(select id from deliveryrack where hasblocked=true and isactive=true and disabled=false and customerid=@customerid and locationid=@locationid limit 1) returning id,code,loyeltycardno,mobileno,customerid";
                using (var reader = ExecuteReader(lockexistingquery, param, conn, transaction))
                {
                    if (reader.Read())
                    {
                        deliveryrack.id = reader.GetInt64(0);
                        deliveryrack.code = reader.IsDBNull(1) ? "" : reader.GetString(1);
                        deliveryrack.card = reader["loyeltycardno"] == DBNull.Value ? "" : reader.GetString(2);
                        deliveryrack.mobileno = reader["mobileno"] == DBNull.Value ? "" : reader.GetString(3);
                        deliveryrack.customerid = reader.IsDBNull(4) ? 0 : reader.GetInt64(4);
                    }
                }
            }

            if (deliveryrack.id > 0 &&
                oBillmaster.customerid > 0 && oBillmaster.customerid != deliveryrack.customerid)
            {
                param.Add("id", deliveryrack.id);
                ExecuteQuery("UPDATE deliveryrack SET customerid=@customerid,mobileno=@mobileno where id=@id", param, conn, transaction);
            }
            return deliveryrack;
        }

        internal Deliveryrack ReleaseRack(Billmaster oBillmaster, NpgsqlConnection conn, NpgsqlTransaction transaction)
        {
            var layeltycardno = (oBillmaster.billattributes != null && !string.IsNullOrEmpty(oBillmaster.billattributes.deliveryrackcard)) ? oBillmaster.billattributes.deliveryrackcard : "";
            if (string.IsNullOrEmpty(layeltycardno))
                layeltycardno = (oBillmaster.Bills[0].billattributes != null && !string.IsNullOrEmpty(oBillmaster.Bills[0].billattributes.deliveryrackcard)) ? oBillmaster.Bills[0].billattributes.deliveryrackcard : "";

            Deliveryrack deliveryrack = new Deliveryrack();
            var param = new SortedList<string, object>();
            param.Add("blockedon", DateTime.Now);
            param.Add("customerid", oBillmaster.customerid);
            param.Add("locationid", oBillmaster.locationid);
            param.Add("loyeltycardno", layeltycardno);
            string lockexistingquery = "UPDATE deliveryrack SET blockedon=null,mobileno='',billno='',loyeltycardno='',pieces=0,hasblocked=false,customerid=0,addinfo=null,releasedon=current_date+current_time where id=(select id from deliveryrack where hasblocked=true and disabled=false and loyeltycardno=@loyeltycardno and locationid=@locationid limit 1) returning id,code";
            using (var reader = ExecuteReader(lockexistingquery, param, conn, transaction))
            {
                if (reader.Read())
                {
                    deliveryrack.id = reader.GetInt64(0);
                    deliveryrack.code = reader.GetString(1);
                }
            }

            if (oBillmaster.Bills.Exists(x => x.billattributes.tailoringorderid > 0))
            {
                foreach(Bill obill in oBillmaster.Bills)
                {
                    param = new SortedList<string, object>();
                    param.Add("billid", obill.id);
                    param.Add("tailoringorderid", obill.billattributes.tailoringorderid);
                    var query = "UPDATE tailoringorder SET hasbilled=true, billid=@billid where id=@tailoringorderid";
                    if (obill.billattributes.tailoringorderid > 0)
                    {
                        ExecuteQuery(query, param, conn, transaction);
                    }
                }
            }
            return deliveryrack;
        }

        internal List<Bill> SearchBill(DeliveryrackCreteria item)
        {
            var racks = Search(item);
            if (racks.Count == 1 && racks[0].addinfo != null)
            {
                BulkBillDataService BulkBillData = new BulkBillDataService();
                var bills = BulkBillData.Search(new BillCreteria()
                {
                    ids = racks[0].addinfo.billids
                });
                bills.ForEach(x => x.Billitems.ForEach(y => y.discount -= y.schemediscount));
                TailoringorderDataService tailoringorderDataService = new TailoringorderDataService() { Context = this.Context };
                if (bills != null && bills.Count > 0)
                    tailoringorderDataService.UpdateAdditionalCharges(bills[0]);
                return bills;
            }
            return new List<Bill>();
        }

        internal Deliveryrack RemoveEstimate(DeliveryrackCreteria item)
        {
            var secondary = Search(new DeliveryrackCreteria()
            {
                id = item.id
            });
            if (secondary.Count == 1)
            {
                secondary[0].addinfo.billids.Remove(item.billid);
                secondary[0].addinfo.billnos.Remove(item.billno);

                BulkBillDataService bulkBillDataService = new BulkBillDataService() { Context = this.Context };
                var bill = bulkBillDataService.Search(new BillCreteria()
                {
                    id = item.billid
                }).First();
                secondary[0].addinfo.noofbills--;
                secondary[0].addinfo.noofpieces -= bill.totalpiece;
                secondary[0].addinfo.amount -= bill.total;
                secondary[0].addinfo.gross -= bill.salevaluewithdiscount;


                NpgsqlTransaction transaction = null;
                try
                {
                    using (NpgsqlConnection conn = CreateConnection())
                    {
                        conn.Open();
                        transaction = conn.BeginTransaction();
                        string SQL_Block = @"UPDATE public.deliveryrack SET addinfo=@addinfo where id=@id";
                        var command = new NpgsqlCommand(SQL_Block, conn, transaction);
                        command.Parameters.Add("id", NpgsqlTypes.NpgsqlDbType.Bigint).Value = item.id;
                        command.Parameters.Add("addinfo", NpgsqlTypes.NpgsqlDbType.Json).Value = secondary[0].addinfo == null ? "null" : secondary[0].addinfo_json;
                        command.ExecuteNonQuery();
                        transaction.Commit();
                        conn.Close();
                        return secondary[0];
                    }
                }
                catch (Exception exp)
                {
                    WebAPI.Common.Logger.Instance(Context.OrganizationID).Log(exp, WebAPI.Common.Logger.LogType.Error, item);
                }
            }
            return new Deliveryrack();
        }

        internal List<Deliveryrack> Merge(DeliveryrackCreteria item)
        {
            item.secondary.RemoveAll(x => item.primaryid == x);
            var secondary = Search(new DeliveryrackCreteria()
            {
                ids = item.secondary,
                locationid = item.locationid
            });
            var primary = Search(new DeliveryrackCreteria()
            {
                id = item.primaryid
            }).First();

            List<long> billids = new List<long>();
            secondary.ForEach(x =>
            {
                primary.addinfo.noofpieces += x.addinfo.noofpieces;
                primary.addinfo.noofbills += x.addinfo.noofpieces;
                primary.addinfo.billnos.AddRange(x.addinfo.billnos);
                primary.addinfo.billids.AddRange(x.addinfo.billids);
                primary.addinfo.amount += x.addinfo.amount;
                primary.addinfo.gross += x.addinfo.gross;
                billids.AddRange(x.addinfo.billids);
            });

            NpgsqlTransaction transaction = null;
            try
            {
                using (NpgsqlConnection conn = CreateConnection())
                {
                    conn.Open();
                    transaction = conn.BeginTransaction();
                    string SQL_Block = @"UPDATE public.deliveryrack SET addinfo=@addinfo where id=@id";
                    var command = new NpgsqlCommand(SQL_Block, conn, transaction);
                    command.Parameters.Add("id", NpgsqlTypes.NpgsqlDbType.Bigint).Value = primary.id;
                    command.Parameters.Add("addinfo", NpgsqlTypes.NpgsqlDbType.Json).Value = primary.addinfo == null ? "null" : primary.addinfo_json;
                    command.ExecuteNonQuery();
                    secondary.ForEach(x =>
                    {
                        var query = @"Update deliveryrack set blockedon=null,mobileno='',billno='',loyeltycardno='',pieces=0,hasblocked=false,customerid=0,addinfo=null,releasedon=current_date+current_time where id=@id";
                        command = new NpgsqlCommand(query, conn, transaction);
                        command.Parameters.Add("id", NpgsqlTypes.NpgsqlDbType.Bigint).Value = x.id;
                        command.ExecuteNonQuery();
                    });
                    transaction.Commit();
                    conn.Close();
                }
            }
            catch (Exception exp)
            {
                WebAPI.Common.Logger.Instance(Context.OrganizationID).Log(exp, WebAPI.Common.Logger.LogType.Error, item);
            }
            return Search(new DeliveryrackCreteria()
            {
                locationid = item.locationid,
                ShowInactive = false
            });
        }

        internal DeliveryRackWakin CheckWalkin(DeliveryrackCreteria item)
        {
            if (string.IsNullOrEmpty(item.billno)) return null;
            var response = Search(new DeliveryrackCreteria()
            {
                billno = item.billno,
                card = item.billno
            });
            if (response.Count > 0)
            {
                if (response[0].addinfo != null &&
                    response[0].addinfo.amount != 0)
                {
                    var racks = SetRackFree(item);
                    if (racks != null && racks.Count == 1)
                    {
                        return new DeliveryRackWakin()
                        {
                            Rack = racks[0]
                        };
                    }
                    throw new Exception("Can'r find the rack to release");
                }
                else
                {
                    return new DeliveryRackWakin()
                    {
                        nobills = true
                    };
                }
            }

            var layeltycardno = item.billno;

            NpgsqlTransaction transaction = null;
            using (NpgsqlConnection conn = CreateConnection())
            {
                Deliveryrack deliveryrack = new Deliveryrack();
                var param = new SortedList<string, object>();
                param.Add("blockedon", DateTime.Now);
                param.Add("loyeltycardno", layeltycardno);
                param.Add("locationid", item.locationid);
                string lockexistingquery = "UPDATE deliveryrack SET blockedon=@blockedon,hasblocked=true where id=(select id from deliveryrack where hasblocked=true and isactive=true and disabled=false and loyeltycardno=@loyeltycardno and locationid=@locationid limit 1) returning id,code,loyeltycardno";
                if (!string.IsNullOrEmpty(layeltycardno))
                {
                    using (var reader = ExecuteReader(lockexistingquery, param, conn, transaction))
                    {
                        if (reader.Read())
                        {
                            deliveryrack.id = reader.GetInt64(0);
                            deliveryrack.code = reader.GetString(1);
                            deliveryrack.card = reader["loyeltycardno"] == DBNull.Value ? "" : reader.GetString(2);
                        }
                    }
                }

                if (deliveryrack.id == 0)
                {
                    if (Services.ConfigManager.Instance(Context.OrganizationID).Config.system_deliveryrackauto)
                    {
                        lockexistingquery = "UPDATE deliveryrack SET blockedon=@blockedon,hasblocked=true,loyeltycardno=@loyeltycardno where id=(select id from deliveryrack where hasblocked=false and disabled=false and isactive=true and locationid=@locationid order by releasedon limit 1) returning id,code,loyeltycardno";
                    }
                    else
                    {
                        lockexistingquery = "UPDATE deliveryrack SET blockedon=@blockedon,hasblocked=true,loyeltycardno=@loyeltycardno where id=(select id from deliveryrack where hasblocked=false and isactive=true and disabled=false and locationid=@locationid order by grp,row,col asc limit 1) returning id,code,loyeltycardno";
                    }
                    using (var reader = ExecuteReader(lockexistingquery, param, conn, transaction))
                    {
                        if (reader.Read())
                        {
                            deliveryrack.id = reader.GetInt64(0);
                            deliveryrack.code = reader.GetString(1);
                            deliveryrack.card = reader["loyeltycardno"] == DBNull.Value ? "" : reader.GetString(2);
                        }
                    }
                    if (deliveryrack.id == 0)
                        throw new Exception("No free rack avilable, please create new racks");
                }
                transaction.Commit();
                conn.Close();
                return new DeliveryRackWakin()
                {
                    newcard = true,
                    Rack = deliveryrack
                };
            }
        }
    }

}
