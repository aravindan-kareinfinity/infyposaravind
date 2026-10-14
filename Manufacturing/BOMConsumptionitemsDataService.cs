using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using InfyPOS.Models;
using InfyPOS.Common;
using Npgsql;
using InfyPOS.Controllers;
using System.Data;

namespace InfyPOS.Data

{
    public class BOMConsumptionitemsDataService : BaseData
    {
        const string SQL_Search = @"select BOMConsumptionitems.id, BOMConsumptionitems.bomcategoryid,bomcode,BOMConsumptionitems.bomid, width, qty, rate, amount,
BOMConsumptionitems.isactive, BOMConsumptionitems.imageid, BOMConsumption.preparedby,BOMConsumption.expectedate, BOMConsumption.supplierid, BOMConsumption.entryno,  BOMConsumption.entrydate,  BOMConsumption.companyid, BOMConsumption.approvedby,
BOMConsumptionitems.colourid, BOMConsumptionitems.createdby, BOMConsumptionitems.modifiedby, BOMConsumptionitems.version, bomconsumptionid, BOMConsumptionitems.colourcode,
BOMConsumptionitems.createdon,  BOMConsumptionitems.modifiedon from BOMConsumption inner join BOMConsumptionitems on BOMConsumptionitems.bomconsumptionid = BOMConsumption.id
                                    where BOMConsumptionitems.id=@id";

        const string SQL_SearchAll = @"select BOMConsumptionitems.bomcategoryid,bomcode,BOMConsumptionitems.bomid, BOMConsumptionitems.width, 
qty, rate, amount,BOMConsumptionitems.imageid, BOMConsumption.preparedby,BOMConsumption.expectedate,
BOMConsumption.supplierid, BOMConsumption.entryno,  BOMConsumption.entrydate,  BOMConsumption.companyid, 
BOMConsumption.approvedby,BOMConsumptionitems.colourcode,BOMConsumptionitems.id,
BOMConsumptionitems.colourid, BOMConsumptionitems.createdby, BOMConsumptionitems.createdon,
bomcategory.name bomcategoryname,boms.name bomname,boms.printingname bomtitle, colour.name colourname from BOMConsumption
inner join BOMConsumptionitems on BOMConsumptionitems.bomconsumptionid = BOMConsumption.id 
left outer join referencelist bomcategory on bomcategory.id = BOMConsumptionitems.bomcategoryid
left outer join bommaster boms on boms.id = BOMConsumptionitems.bomid
left outer join referencelist colour on colour.id = BOMConsumptionitems.colourid 
where BOMConsumptionid=@BOMConsumptionid and BOMConsumptionitems.isactive=true";


        const string SQL_Insert = @"INSERT INTO public.BOMConsumptionitems(bomcategoryid, bomid,width,colourid, qty, rate, amount, isactive, createdby, modifiedby, version,
bomconsumptionid, createdon, modifiedon,bomcode,imageid,colourcode) 
VALUES(@bomcategoryid, @bomid,@width,@colourid, @qty, @rate, @amount, @isactive, @createdby, @modifiedby, @version,
@bomconsumptionid, @createdon, @modifiedon,@bomcode,@imageid,@colourcode) RETURNING id";


        const string SQL_Update = @"UPDATE public.BOMConsumptionitems SET bomcategoryid=@bomcategoryid, bomid=@bomid, qty=@qty, rate=@rate, amount=@amount,
modifiedby=@modifiedby, version=@version+1, bomconsumptionid=@bomconsumptionid, modifiedon=@modifiedon,imageid=@imageid,colourid=@colourid,
width=@width,bomcode=@bomcode,colourcode=@colourcode
where id=@id and version=@version RETURNING version";

        const string SQL_Delete = @"update public.BOMConsumptionitems set isactive=false,version=@version+1,modifiedby=@modifiedby,modifiedon=@modifiedon where id=@id";
        const string SQL_DeleteAll = @"update public.BOMConsumptionitems set isactive=false,modifiedby=@modifiedby,modifiedon=@modifiedon where BOMConsumptionid=@BOMConsumptionid";

        const string SQL_UpdatePRF = @"update public.productrequestformitems set rqty=rqty+@rqty where id=@id";

        public List<BOMConsumptionitemsDetail> Search(BOMConsumptionitemsCreteria _BOMConsumptionitems)
        {
            List<BOMConsumptionitemsDetail> lst = new List<BOMConsumptionitemsDetail>();
            NpgsqlConnection conn = CreateConnection();
            try
            {
                conn.Open();

                NpgsqlCommand command = null;
                if (_BOMConsumptionitems.id != 0)
                {
                    command = GetCommand(SQL_Search, conn);
                    command.Parameters.Add("id", NpgsqlTypes.NpgsqlDbType.Bigint).Value = _BOMConsumptionitems.id;
                }
                else
                {
                    QueryBuilder qb = new QueryBuilder();
                    qb.CurrentQuery = SQL_Search.Substring(0, SQL_Search.IndexOf("where"));
                    qb.AddParameter("BOMConsumptionitems.isactive", true, "=");
                    qb.AddParameter("BOMConsumption.isactive", true, "=");

                    if (_BOMConsumptionitems.bomconsumptionid > 0)
                        qb.AddParameter("bomconsumptionid", _BOMConsumptionitems.bomconsumptionid, "=");

                    //if (!string.IsNullOrEmpty(_BOMConsumptionitems.Code))
                    //    qb.AddParameter("code", _BOMConsumptionitems.Code, "=");

                    command = qb.GetCommand(conn);
                }

                using (System.Data.IDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        BOMConsumptionitemsDetail oBOMConsumptionitems = new BOMConsumptionitemsDetail();
                        oBOMConsumptionitems.id = reader["id"] == DBNull.Value ? 0 : Convert.ToInt64(reader["id"]);
                        oBOMConsumptionitems.entryno = reader["entryno"] == DBNull.Value ? "" : reader["entryno"].ToString();
                        oBOMConsumptionitems.width = reader["width"] == DBNull.Value ? "" : reader["width"].ToString();
                        oBOMConsumptionitems.bomcode = reader["bomcode"] == DBNull.Value ? "" : reader["bomcode"].ToString();
                        oBOMConsumptionitems.bomcategoryid = reader["bomcategoryid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["bomcategoryid"]);
                        oBOMConsumptionitems.bomid = reader["bomid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["bomid"]);

                        oBOMConsumptionitems.entrydate = reader["entrydate"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(reader["entrydate"]);
                        oBOMConsumptionitems.imageid = reader["imageid"] == DBNull.Value ? "" : reader["imageid"].ToString();
                        oBOMConsumptionitems.colourcode = reader["colourcode"] == DBNull.Value ? "" : reader["colourcode"].ToString();
                        oBOMConsumptionitems.approvedby = reader["approvedby"] == DBNull.Value ? 0 : Convert.ToInt64(reader["approvedby"]);
                        oBOMConsumptionitems.preparedby = reader["preparedby"] == DBNull.Value ? 0 : Convert.ToInt64(reader["preparedby"]);
                        oBOMConsumptionitems.supplierid = reader["supplierid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["supplierid"]);
                        oBOMConsumptionitems.expectedate = reader["expectedate"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(reader["expectedate"]);
                        oBOMConsumptionitems.companyid = reader["companyid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["companyid"]);
                        oBOMConsumptionitems.colourid = reader["colourid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["colourid"]);
                        oBOMConsumptionitems.qty = reader["qty"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["qty"]);
                        oBOMConsumptionitems.rate = reader["rate"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["rate"]);
                        oBOMConsumptionitems.amount = reader["amount"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["amount"]);
                        oBOMConsumptionitems.isactive = reader["isactive"] == DBNull.Value ? false : Convert.ToBoolean(reader["isactive"]);
                        oBOMConsumptionitems.createdby = reader["createdby"] == DBNull.Value ? 0 : Convert.ToInt64(reader["createdby"]);
                        oBOMConsumptionitems.modifiedby = reader["modifiedby"] == DBNull.Value ? 0 : Convert.ToInt64(reader["modifiedby"]);
                        oBOMConsumptionitems.version = reader["version"] == DBNull.Value ? 0 : Convert.ToInt32(reader["version"]);
                        oBOMConsumptionitems.bomconsumptionid = reader["bomconsumptionid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["bomconsumptionid"]);
                        oBOMConsumptionitems.createdon = reader["createdon"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(reader["createdon"]);
                        oBOMConsumptionitems.modifiedon = reader["modifiedon"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(reader["modifiedon"]);
                        lst.Add(oBOMConsumptionitems);
                    }
                }
                conn.Close();
            }
            catch (Exception exp)
            {
                if (conn != null) conn.Close();
                //WebAPI.Common.Logger.Instance(Context.OrganizationID).Log(exp, WebAPI.Common.Logger.LogType.Error, _BOMConsumptionitems);
            }
            return lst;
        }

        public List<BOMConsumptionitemsDetail> SearchAll(long _BOMConsumptionid)
        {
            List<BOMConsumptionitemsDetail> lst = new List<BOMConsumptionitemsDetail>();
            NpgsqlConnection conn = CreateConnection();
            try
            {
                conn.Open();

                NpgsqlCommand command = GetCommand(SQL_SearchAll, conn);
                command.Parameters.Add("BOMConsumptionid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = _BOMConsumptionid;
                using (System.Data.IDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        BOMConsumptionitemsDetail oBOMConsumptionitems = new BOMConsumptionitemsDetail();
                        oBOMConsumptionitems.entryno = reader["entryno"] == DBNull.Value ? "" : reader["entryno"].ToString();
                        oBOMConsumptionitems.width = reader["width"] == DBNull.Value ? "" : reader["width"].ToString();
                        oBOMConsumptionitems.bomcode = reader["bomcode"] == DBNull.Value ? "" : reader["bomcode"].ToString();
                        oBOMConsumptionitems.id = reader["id"] == DBNull.Value ? 0 : Convert.ToInt64(reader["id"]);
                        oBOMConsumptionitems.bomcategoryid = reader["bomcategoryid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["bomcategoryid"]);
                        oBOMConsumptionitems.bomid = reader["bomid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["bomid"]);
                        oBOMConsumptionitems.colourcode = reader["colourcode"] == DBNull.Value ? "" : reader["colourcode"].ToString();
                        oBOMConsumptionitems.entrydate = reader["entrydate"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(reader["entrydate"]);
                        oBOMConsumptionitems.imageid = reader["imageid"] == DBNull.Value ? "" : reader["imageid"].ToString();
                        oBOMConsumptionitems.approvedby = reader["approvedby"] == DBNull.Value ? 0 : Convert.ToInt64(reader["approvedby"]);
                        oBOMConsumptionitems.preparedby = reader["preparedby"] == DBNull.Value ? 0 : Convert.ToInt64(reader["preparedby"]);
                        oBOMConsumptionitems.supplierid = reader["supplierid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["supplierid"]);
                        oBOMConsumptionitems.expectedate = reader["expectedate"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(reader["expectedate"]);
                        oBOMConsumptionitems.companyid = reader["companyid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["companyid"]);
                        oBOMConsumptionitems.colourid = reader["colourid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["colourid"]);
                        oBOMConsumptionitems.qty = reader["qty"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["qty"]);
                        oBOMConsumptionitems.rate = reader["rate"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["rate"]);
                        oBOMConsumptionitems.amount = reader["amount"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["amount"]);
                        oBOMConsumptionitems.createdby = reader["createdby"] == DBNull.Value ? 0 : Convert.ToInt64(reader["createdby"]);
                        oBOMConsumptionitems.createdon = reader["createdon"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(reader["createdon"]);
                        oBOMConsumptionitems.bomcategoryname = reader["bomcategoryname"] == DBNull.Value ? "" : reader["bomcategoryname"].ToString();
                        oBOMConsumptionitems.bomname = reader["bomname"] == DBNull.Value ? "" : reader["bomname"].ToString();
                        oBOMConsumptionitems.bomtitle = reader["bomtitle"] == DBNull.Value ? "" : reader["bomtitle"].ToString();
                        oBOMConsumptionitems.colourname = reader["colourname"] == DBNull.Value ? "" : reader["colourname"].ToString(); 
                        lst.Add(oBOMConsumptionitems);
                    }
                }
                conn.Close();
            }
            catch (Exception exp)
            {
                if (conn != null) conn.Close();
                //WebAPI.Common.Logger.Instance(Context.OrganizationID).Log(exp, WebAPI.Common.Logger.LogType.Error, _BOMConsumptionid);
            }
            return lst;
        }

        public BOMConsumptionitems Insert(BOMConsumptionitemsDetail oBOMConsumptionitems)
        {

            NpgsqlTransaction transaction = null;
            try
            {
                using (NpgsqlConnection conn = CreateConnection())
                {
                    conn.Open();
                    transaction = conn.BeginTransaction();
                    
                    Insert(transaction, conn, oBOMConsumptionitems);
                    transaction.Commit();
                    conn.Close();
                }
            }
            catch (Exception exp)
            {
                //WebAPI.Common.Logger.Instance(Context.OrganizationID).Log(exp, WebAPI.Common.Logger.LogType.Error, oBOMConsumptionitems);


            }

            return oBOMConsumptionitems;
        }

        public BOMConsumptionitems Insert(NpgsqlTransaction transaction, NpgsqlConnection conn, BOMConsumptionitemsDetail oBOMConsumptionitems)
        {
            NpgsqlCommand command = GetCommand(SQL_Insert, conn, transaction);
            command.Parameters.Add("bomcategoryid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oBOMConsumptionitems.bomcategoryid;
            command.Parameters.Add("bomid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oBOMConsumptionitems.bomid;
            command.Parameters.Add("colourid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oBOMConsumptionitems.colourid;

            command.Parameters.Add("qty", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oBOMConsumptionitems.qty;
            command.Parameters.Add("rate", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oBOMConsumptionitems.rate;
            command.Parameters.Add("amount", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oBOMConsumptionitems.amount;

            command.Parameters.Add("width", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oBOMConsumptionitems.width == null ? "" : oBOMConsumptionitems.width;
            command.Parameters.Add("imageid", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oBOMConsumptionitems.imageid == null ? "" : oBOMConsumptionitems.imageid;
            command.Parameters.Add("bomcode", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oBOMConsumptionitems.bomcode == null ? "" : oBOMConsumptionitems.bomcode;
            command.Parameters.Add("colourcode", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oBOMConsumptionitems.colourcode == null ? "" : oBOMConsumptionitems.colourcode;

            command.Parameters.Add("isactive", NpgsqlTypes.NpgsqlDbType.Boolean).Value = oBOMConsumptionitems.isactive;
            command.Parameters.Add("createdby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = Context.UserID;
            command.Parameters.Add("modifiedby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = Context.UserID;
            command.Parameters.Add("version", NpgsqlTypes.NpgsqlDbType.Integer).Value = oBOMConsumptionitems.version;
            command.Parameters.Add("bomconsumptionid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oBOMConsumptionitems.bomconsumptionid;
            command.Parameters.Add("createdon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = DateTimeNow;
            command.Parameters.Add("modifiedon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = DateTimeNow;

            using (System.Data.IDataReader reader = command.ExecuteReader())
            {
                if (reader.Read())
                {
                    oBOMConsumptionitems.id = Convert.ToInt64(reader[0].ToString());
                }
            }

            return oBOMConsumptionitems;
        }

        public BOMConsumptionitems Update(BOMConsumptionitemsDetail oBOMConsumptionitems)
        {

            NpgsqlTransaction transaction = null;
            try
            {
                using (NpgsqlConnection conn = CreateConnection())
                {
                    conn.Open();
                    transaction = conn.BeginTransaction();
                    var items = Search(new BOMConsumptionitemsCreteria { id = oBOMConsumptionitems.id });
                    if (items.Count == 1)
                        Update(transaction, conn, oBOMConsumptionitems, items[0]);
                    else
                        Update(transaction, conn, oBOMConsumptionitems, null);
                    transaction.Commit();
                    conn.Close();
                }
            }
            catch (Exception exp)
            {
                //WebAPI.Common.Logger.Instance(Context.OrganizationID).Log(exp, WebAPI.Common.Logger.LogType.Error, oBOMConsumptionitems);
            }

            return oBOMConsumptionitems;
        }

        public BOMConsumptionitems Update(NpgsqlTransaction transaction, NpgsqlConnection conn,
            BOMConsumptionitems oBOMConsumptionitems, BOMConsumptionitems oldItem)
        {

            NpgsqlCommand command = GetCommand(SQL_Update, conn, transaction);
            command.Parameters.Add("id", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oBOMConsumptionitems.id;
            command.Parameters.Add("bomcategoryid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oBOMConsumptionitems.bomcategoryid;
            command.Parameters.Add("bomid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oBOMConsumptionitems.bomid;
            command.Parameters.Add("qty", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oBOMConsumptionitems.qty;
            command.Parameters.Add("rate", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oBOMConsumptionitems.rate;
            command.Parameters.Add("amount", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oBOMConsumptionitems.amount;
            command.Parameters.Add("bomcode", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oBOMConsumptionitems.bomcode == null ? "" : oBOMConsumptionitems.bomcode;
            command.Parameters.Add("width", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oBOMConsumptionitems.width == null ? "" : oBOMConsumptionitems.width;
            command.Parameters.Add("colourcode", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oBOMConsumptionitems.colourcode == null ? "" : oBOMConsumptionitems.colourcode;
            command.Parameters.Add("isactive", NpgsqlTypes.NpgsqlDbType.Boolean).Value = oBOMConsumptionitems.isactive;
            command.Parameters.Add("createdby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oBOMConsumptionitems.createdby;
            command.Parameters.Add("modifiedby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = Context.UserID;
            command.Parameters.Add("version", NpgsqlTypes.NpgsqlDbType.Integer).Value = oBOMConsumptionitems.version;
            command.Parameters.Add("bomconsumptionid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oBOMConsumptionitems.bomconsumptionid;
            command.Parameters.Add("createdon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = oBOMConsumptionitems.createdon;
            command.Parameters.Add("modifiedon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = DateTimeNow;
            command.Parameters.Add("imageid", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oBOMConsumptionitems.imageid == null ? "" : oBOMConsumptionitems.imageid;
            command.Parameters.Add("colourid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oBOMConsumptionitems.colourid;
            using (System.Data.IDataReader reader = command.ExecuteReader())
            {
                if (reader.Read())
                {
                    oBOMConsumptionitems.version = Convert.ToInt32(reader[0].ToString());
                }
            }

            return oBOMConsumptionitems;
        }

        public bool Update(NpgsqlTransaction transaction, NpgsqlConnection conn,
            BOMConsumption oBOMConsumption, List<BOMConsumptionitemsDetail> list, long id)
        {
            BomstocktransactionDataService bomstocktransactionDataService = new BomstocktransactionDataService() { Context = this.Context };


            var existingItems = SearchAll(id);
            if (existingItems != null && existingItems.Count > 0)
            {
                var deletedItems = existingItems.FindAll(e => !list.Exists(f => f.id == e.id));
                foreach (var item in deletedItems)
                    Delete(transaction, conn, item);
            }

            //Bomstocktransaction.TransactionType transactionType = Bomstocktransaction.TransactionType.Purchase;
            //if (oBOMConsumption.isoutward)
            //{
            //    if (oBOMConsumption.mfgjoborderid > 0)
            //        transactionType = Bomstocktransaction.TransactionType.JobOrderOutward;
            //    else
            //        transactionType = Bomstocktransaction.TransactionType.WorkOrderOutward;
            //}
            //else
            //{
            //    if (oBOMConsumption.mfgjoborderid > 0)
            //        transactionType = Bomstocktransaction.TransactionType.JobOrderConsumption;
            //    else
            //        transactionType = Bomstocktransaction.TransactionType.WorkOrderConsumption;
            //}

            //bomstocktransactionDataService.Delete(transactionType,
            //    transaction, conn, oBOMConsumption.id);

            foreach (var item in list)
            {
                if (item.id > 0)
                {
                    Update(transaction, conn, item, existingItems.Find(e => e.id == item.id));
                }
                else
                {
                    item.bomconsumptionid = id;
                    Insert(transaction, conn, item);
                }
            }
            //bomstocktransactionDataService.Insert(transactionType,
            //   conn, transaction, oBOMConsumption, list);
            return true;
        }

        public bool Delete(BOMConsumptionitems oBOMConsumptionitems)
        {

            NpgsqlTransaction transaction = null;
            bool hasSuccessCount = false;
            try
            {
                using (NpgsqlConnection conn = CreateConnection())
                {
                    conn.Open();
                    transaction = conn.BeginTransaction();
                    hasSuccessCount = Delete(transaction, conn, oBOMConsumptionitems);
                    transaction.Commit();
                    conn.Close();
                }
            }
            catch (Exception exp)
            {
                //WebAPI.Common.Logger.Instance(Context.OrganizationID).Log(exp, WebAPI.Common.Logger.LogType.Error, oBOMConsumptionitems);



            }
            return hasSuccessCount;

        }

        public bool Delete(NpgsqlTransaction transaction, NpgsqlConnection conn, BOMConsumptionitems oBOMConsumptionitems)
        {
            var items = Search(new BOMConsumptionitemsCreteria
            {
                id = oBOMConsumptionitems.id
            });


            NpgsqlCommand command = GetCommand(SQL_Delete, conn, transaction);
            command.Parameters.Add("isactive", NpgsqlTypes.NpgsqlDbType.Boolean).Value = oBOMConsumptionitems.isactive;
            command.Parameters.Add("modifiedby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = Context.UserID;
            command.Parameters.Add("modifiedon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = DateTimeNow;
            command.Parameters.Add("version", NpgsqlTypes.NpgsqlDbType.Integer).Value = oBOMConsumptionitems.version;
            command.Parameters.Add("id", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oBOMConsumptionitems.id;
            int successCount = command.ExecuteNonQuery();

            return successCount > 0;
        }

        public bool DeleteAll(long BOMConsumptionid, long userid, NpgsqlConnection conn, NpgsqlTransaction transaction)
        {
            int successCount = 0;
            NpgsqlCommand command = GetCommand(SQL_DeleteAll, conn, transaction);
            command.Parameters.Add("isactive", NpgsqlTypes.NpgsqlDbType.Boolean).Value = false;
            command.Parameters.Add("modifiedby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = Context.UserID;
            command.Parameters.Add("modifiedon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = DateTimeNow;
            command.Parameters.Add("BOMConsumptionid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = BOMConsumptionid;
            successCount = command.ExecuteNonQuery();
            return successCount > 0;
        }
        /*

with allcolourcodes
as
(
select schemelist->>'name' shade,bomconsumptionitems.id from bommaster
inner join bomconsumptionitems on bomconsumptionitems.bomid= bommaster.id
LEFT JOIN LATERAL
json_array_elements (cast(bommaster.colourlist as json)) schemelist ON true

)
update bomconsumptionitems set colourcode = shade from allcolourcodes
where allcolourcodes.id =bomconsumptionitems.id
        
with allcolourcodes
as
(
select schemelist->>'name' shade, cast(schemelist->>'id' as bigint) colourid,bommaster.id from bommaster
LEFT JOIN LATERAL
json_array_elements (cast(bommaster.colourlist as json)) schemelist ON true
)
update bomstocktransaction set isactive=false,version=9999 where id in (
select bomstocktransaction.id from bomstocktransaction
left outer join allcolourcodes on bomstocktransaction.bomid = allcolourcodes.id
and bomstocktransaction.colourid = allcolourcodes.colourid
where allcolourcodes.id is null)*/

        internal List<BOMConsumptionitemsDetail> ConverttoItems(string locationid, ImageController.ExcelDocument exceldocument)
        {
            var designindex = exceldocument.columns.FindIndex(x => x.ToLower().Contains("design"));
            var shadeindex = exceldocument.columns.FindIndex(x => x.ToLower().Contains("shade"));
            //var colourindex = exceldocument.columns.FindIndex(x => x.ToLower().Contains("colour"));
            var qtyindex = exceldocument.columns.FindIndex(x => x.ToLower().Contains("qty") || x.ToLower().Contains("mtr"));

            if (designindex < 0)
                throw new Exception("Design column missing");

            if (shadeindex < 0)
                throw new Exception("Shade column missing");

            if (qtyindex < 0)
                throw new Exception("Qty/Mtr column missing");

            for (var i = 1; i < exceldocument.rows.Count; i++)
            {
                if (string.IsNullOrEmpty(exceldocument.rows[i].values[designindex]))
                {
                    exceldocument.rows[i].values[designindex] = exceldocument.rows[i - 1].values[designindex];
                }
            }

            string query = @"select id,categoryid,width,taxid,colourmode,name,cast(schemelist->>'id' as bigint) lcolourid,schemelist->>'name' shade from bommaster
                            LEFT JOIN LATERAL
                            json_array_elements (cast(bommaster.colourlist as json)) schemelist ON true
                            where bommaster.isactive=true and bommaster.name in (" + string.Join(",", exceldocument.rows.ConvertAll(x => x.values[designindex]).Distinct().ToList().ConvertAll(x => "'" + x + "'")) + ")";

            var dset = this.ExecuteDataSet(query, new SortedList<string, object>());
            var result = from d in dset.Tables[0].AsEnumerable()
                         select new
                         {
                             id = d.Field<long>("id"),
                             categoryid = d.Field<long>("categoryid"),
                             width = d.Field<string>("width"),
                             shade = d.Field<string>("shade"),
                             name = d.Field<string>("name"),
                             taxid = d.IsNull("taxid") ? 0 : d.Field<long>("taxid"),
                             colourmode = d.IsNull("colourmode") ? 0 : d.Field<int>("colourmode"),
                             lcolourid = d.IsNull("lcolourid") ? 0 : d.Field<long>("lcolourid")
                         };
            var resultlist = result.ToList();
            BomstocktransactionDataService bomstocktransactionDataService = new BomstocktransactionDataService() { Context = this.Context };
            var stocksummary = bomstocktransactionDataService.SummaryReport(new BomstocktransactionCreteria()
            {
                fromdate = DateTime.MinValue,
                todate = DateTime.MaxValue.AddDays(-10),
                locationid = long.Parse(locationid)
            });
            List<BOMConsumptionitemsDetail> response = new List<BOMConsumptionitemsDetail>();
            foreach (var item in exceldocument.rows.GroupBy(x => new { design = x.values[designindex], shade = x.values[shadeindex] }))
            {
                string design = item.Key.design;
                string shade = item.Key.shade;
                decimal qty = item.Sum(x =>
                {
                    decimal d;
                    decimal.TryParse(x.values[qtyindex], out d);
                    return d;
                }
                );
                var bomlist = resultlist.FindAll(x => x.name == design && x.shade == shade);
                if (bomlist.Count > 0)
                {
                    var stocklist = stocksummary.FindAll(x => x.bomid == bomlist.First().id && x.colourid == bomlist.First().lcolourid);
                    var currentstock = stocklist.Sum(x => x.inward - x.outward);
                    response.Add(new BOMConsumptionitemsDetail()
                    {
                        bomcategoryid = bomlist.First().categoryid,
                        bomid = bomlist.First().id,
                        width = bomlist.First().width,
                        colourid = bomlist.First().lcolourid,
                        rate = 0,
                        amount = 0,
                        colourcode = bomlist.First().shade,
                        qty = currentstock-qty,
                        isactive = true
                    });
                }
            }

            return response;
        }
    }

}
