using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using InfyPOS.Models;
using InfyPOS.Common;
using Npgsql;
namespace InfyPOS.Data

{
    public class ComponentPOitemsDataService : SAASData
    {
        const string SQL_Search = @"select ComponentPOitems.id, bomcategoryid, bomid, colourid, ComponentPOitems.taxid, ComponentPOitems.uomid, ComponentPOitems.coretypeid, 
ComponentPOitems.qty, ComponentPOitems.width, ComponentPOitems.rate, ComponentPOitems.amount, ComponentPOitems.imageid, colourcode,colourshade,
ComponentPOitems.componentorderid, ComponentPOitems.isactive, ComponentPOitems.createdby, ComponentPOitems.modifiedby,ComponentPOitems.designid,
ComponentPOitems.version, ComponentPOitems.createdon, ComponentPOitems.modifiedon, bommaster.name bomcode,ComponentPOitems.brandname
from ComponentPOitems left outer join bommaster on bommaster.id = ComponentPOitems.bomid  
where ComponentPOitems.id=@id";

        const string SQL_SearchAll = @"select ComponentPOitems.id, bomcategoryid, category.name categoryname, colourcode,colourshade,tax.taxpercentage,
bomid, colourid, colour.name colourname, bommaster.supplierbrand,bommaster.designno,bommaster.hsncode,ComponentPOitems.brandname,
ComponentPOitems.taxid, ComponentPOitems.uomid, bomuom.name uomname, ComponentPOitems.coretypeid, ComponentPOitems.designid,
ComponentPOitems.qty, ComponentPOitems.width, ComponentPOitems.rate, ComponentPOitems.amount, ComponentPOitems.imageid, 
ComponentPOitems.componentorderid, ComponentPOitems.isactive, ComponentPOitems.createdby, ComponentPOitems.modifiedby, 
ComponentPOitems.version, ComponentPOitems.createdon, ComponentPOitems.modifiedon, bommaster.name bomcode
from ComponentPOitems 
left outer join bommaster on bommaster.id = ComponentPOitems.bomid  
left outer join tax on tax.id = bommaster.taxid
left outer join referencelist category on category.id = ComponentPOitems.bomcategoryid
left outer join referencelist colour on colour.id = ComponentPOitems.colourid
left outer join referencelist bomuom on bomuom.id = bommaster.uomid  
where componentorderid=@componentorderid and ComponentPOitems.isactive=true";


        const string SQL_Insert = @"INSERT INTO public.ComponentPOitems(bomcategoryid, bomid, colourid, taxid, uomid, coretypeid, qty, width, rate, amount, imageid, componentorderid, isactive, createdby, modifiedby, version, createdon, modifiedon,brandname,designid,colourcode,colourshade) 
        VALUES(@bomcategoryid, @bomid, @colourid, @taxid, @uomid, @coretypeid, @qty, @width, @rate, @amount, @imageid, @componentorderid, @isactive, @createdby, @modifiedby, @version, @createdon, @modifiedon,@brandname,@designid,@colourcode,@colourshade) RETURNING id";


        const string SQL_Update = @"UPDATE public.ComponentPOitems SET bomcategoryid=@bomcategoryid, bomid=@bomid, colourid=@colourid, taxid=@taxid, uomid=@uomid, coretypeid=@coretypeid, qty=@qty, width=@width, rate=@rate, amount=@amount, imageid=@imageid, 
        componentorderid=@componentorderid, modifiedby=@modifiedby, version=@version+1, colourshade=@colourshade,
        modifiedon=@modifiedon,brandname=@brandname,designid=@designid,colourcode=@colourcode where id=@id and version=@version RETURNING version";

        const string SQL_Delete = @"update public.ComponentPOitems set isactive=false,version=@version+1,modifiedby=@modifiedby,modifiedon=@modifiedon where id=@id and version=@version";
        const string SQL_DeleteAll = @"update public.ComponentPOitems set isactive=false,modifiedby=@modifiedby,modifiedon=@modifiedon where componentorderid=@componentorderid";
        const string SQL_CancelAll = @"update public.ComponentPOitems set cqty=qty-rqty,modifiedby=@modifiedby,modifiedon=@modifiedon where componentorderid=@componentorderid";

        public List<ComponentPOitems> Search(ComponentPOitemsCreteria _componentpoitems)
        {
            List<ComponentPOitems> lst = new List<ComponentPOitems>();
            NpgsqlConnection conn = CreateConnection();
            try
            {
                conn.Open();

                NpgsqlCommand command = null;
                if (_componentpoitems.id != 0)
                {
                    command = new NpgsqlCommand(SQL_Search, conn);
                    command.Parameters.Add("id", NpgsqlTypes.NpgsqlDbType.Bigint).Value = _componentpoitems.id;
                }
                else
                {
                    QueryBuilder qb = new QueryBuilder();
                    qb.CurrentQuery = SQL_Search.Substring(0, SQL_Search.IndexOf("where"));
                    qb.AddParameter("ComponentPOitems.isactive", true, "=");

                    if (_componentpoitems.componentorderid > 0)
                        qb.AddParameter("ComponentPOitems.componentorderid", _componentpoitems.componentorderid, "=");

                    command = qb.GetCommand(conn);
                }

                using (System.Data.IDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        ComponentPOitems ocomponentpoitems = new ComponentPOitems();
                        ocomponentpoitems.id = reader["id"] == DBNull.Value ? 0 : Convert.ToInt64(reader["id"]);
                        ocomponentpoitems.bomcategoryid = reader["bomcategoryid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["bomcategoryid"]);
                        ocomponentpoitems.bomid = reader["bomid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["bomid"]);
                        ocomponentpoitems.bomcode = reader["bomcode"] == DBNull.Value ? "" : reader["bomcode"].ToString();
                        ocomponentpoitems.colourid = reader["colourid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["colourid"]);
                        ocomponentpoitems.taxid = reader["taxid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["taxid"]);
                        ocomponentpoitems.uomid = reader["uomid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["uomid"]);
                        ocomponentpoitems.coretypeid = reader["coretypeid"] == DBNull.Value ? 0 : Convert.ToInt32(reader["coretypeid"]);
                        ocomponentpoitems.qty = reader["qty"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["qty"]);
                        ocomponentpoitems.width = reader["width"] == DBNull.Value ? "" : reader["width"].ToString();

                        ocomponentpoitems.brandname = reader["brandname"] == DBNull.Value ? "" : reader["brandname"].ToString();
                        ocomponentpoitems.colourcode = reader["colourcode"] == DBNull.Value ? "" : reader["colourcode"].ToString();
                        ocomponentpoitems.colourshade = reader["colourshade"] == DBNull.Value ? "" : reader["colourshade"].ToString();
                        ocomponentpoitems.designid = reader["designid"] == DBNull.Value ? "" : reader["designid"].ToString();

                        ocomponentpoitems.rate = reader["rate"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["rate"]);
                        ocomponentpoitems.amount = reader["amount"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["amount"]);
                        ocomponentpoitems.imageid = reader["imageid"] == DBNull.Value ? "" : reader["imageid"].ToString();
                        ocomponentpoitems.componentorderid = reader["componentorderid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["componentorderid"]);
                        ocomponentpoitems.isactive = reader["isactive"] == DBNull.Value ? false : Convert.ToBoolean(reader["isactive"]);
                        ocomponentpoitems.createdby = reader["createdby"] == DBNull.Value ? 0 : Convert.ToInt64(reader["createdby"]);
                        ocomponentpoitems.modifiedby = reader["modifiedby"] == DBNull.Value ? 0 : Convert.ToInt64(reader["modifiedby"]);
                        ocomponentpoitems.version = reader["version"] == DBNull.Value ? 0 : Convert.ToInt32(reader["version"]);
                        ocomponentpoitems.createdon = reader["createdon"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(reader["createdon"]);
                        ocomponentpoitems.modifiedon = reader["modifiedon"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(reader["modifiedon"]);
                        lst.Add(ocomponentpoitems);
                    }
                }
                conn.Close();
            }
            catch (Exception exp)
            {
                if (conn != null) conn.Close();
                WebAPI.Common.Logger.Instance(Context.OrganizationID).Log(exp, WebAPI.Common.Logger.LogType.Error, _componentpoitems);
            }
            return lst;
        }

        public List<ComponentPOitemsDetail> SearchAll(long _componentorderid)
        {
            List<ComponentPOitemsDetail> lst = new List<ComponentPOitemsDetail>();
            NpgsqlConnection conn = CreateConnection();
            try
            {
                conn.Open();
                NpgsqlCommand command = new NpgsqlCommand(SQL_SearchAll, conn);
                command.Parameters.Add("componentorderid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = _componentorderid;
                using (System.Data.IDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        ComponentPOitemsDetail ocomponentpoitems = new ComponentPOitemsDetail();
                        ocomponentpoitems.id = reader["id"] == DBNull.Value ? 0 : Convert.ToInt64(reader["id"]);
                        ocomponentpoitems.bomcategoryid = reader["bomcategoryid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["bomcategoryid"]);
                        ocomponentpoitems.categoryname = reader["categoryname"] == DBNull.Value ? "" : reader["categoryname"].ToString();
                        ocomponentpoitems.bomcategoryname = reader["categoryname"] == DBNull.Value ? "" : reader["categoryname"].ToString();
                        ocomponentpoitems.bomid = reader["bomid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["bomid"]);
                        ocomponentpoitems.bomcode = reader["bomcode"] == DBNull.Value ? "" : reader["bomcode"].ToString();
                        ocomponentpoitems.colourid = reader["colourid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["colourid"]);
                        ocomponentpoitems.colourname = reader["colourname"] == DBNull.Value ? "" : reader["colourname"].ToString();
                        ocomponentpoitems.taxid = reader["taxid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["taxid"]);
                        ocomponentpoitems.taxpercentage = reader["taxpercentage"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["taxpercentage"]);
                        ocomponentpoitems.uomid = reader["uomid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["uomid"]);
                        ocomponentpoitems.uomname = reader["uomname"] == DBNull.Value ? "" : reader["uomname"].ToString();

                        ocomponentpoitems.hsncode = reader["hsncode"] == DBNull.Value ? "" : reader["hsncode"].ToString();

                        ocomponentpoitems.colourcode = reader["colourcode"] == DBNull.Value ? "" : reader["colourcode"].ToString();
                        ocomponentpoitems.colourshade = reader["colourshade"] == DBNull.Value ? "" : reader["colourshade"].ToString();
                        ocomponentpoitems.brandname = reader["brandname"] == DBNull.Value ? "" : reader["brandname"].ToString();
                        ocomponentpoitems.designid = reader["designid"] == DBNull.Value ? "" : reader["designid"].ToString();

                        if(string.IsNullOrEmpty(ocomponentpoitems.brandname))
                            ocomponentpoitems.brandname = reader["supplierbrand"] == DBNull.Value ? "" : reader["supplierbrand"].ToString();

                        if (string.IsNullOrEmpty(ocomponentpoitems.designid))
                            ocomponentpoitems.designid = reader["designno"] == DBNull.Value ? "" : reader["designno"].ToString();

                        ocomponentpoitems.coretypeid = reader["coretypeid"] == DBNull.Value ? 0 : Convert.ToInt32(reader["coretypeid"]);
                        ocomponentpoitems.qty = reader["qty"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["qty"]);
                        ocomponentpoitems.width = reader["width"] == DBNull.Value ? "" : reader["width"].ToString();
                        ocomponentpoitems.rate = reader["rate"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["rate"]);
                        ocomponentpoitems.amount = reader["amount"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["amount"]);
                        ocomponentpoitems.imageid = reader["imageid"] == DBNull.Value ? "" : reader["imageid"].ToString();
                        ocomponentpoitems.componentorderid = reader["componentorderid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["componentorderid"]);
                        ocomponentpoitems.isactive = reader["isactive"] == DBNull.Value ? false : Convert.ToBoolean(reader["isactive"]);
                        ocomponentpoitems.createdby = reader["createdby"] == DBNull.Value ? 0 : Convert.ToInt64(reader["createdby"]);
                        ocomponentpoitems.modifiedby = reader["modifiedby"] == DBNull.Value ? 0 : Convert.ToInt64(reader["modifiedby"]);
                        ocomponentpoitems.version = reader["version"] == DBNull.Value ? 0 : Convert.ToInt32(reader["version"]);
                        ocomponentpoitems.createdon = reader["createdon"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(reader["createdon"]);
                        ocomponentpoitems.modifiedon = reader["modifiedon"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(reader["modifiedon"]);
                        lst.Add(ocomponentpoitems);
                    }
                }
                conn.Close();
            }
            catch (Exception exp)
            {
                if (conn != null) conn.Close();
                WebAPI.Common.Logger.Instance(Context.OrganizationID).Log(exp, WebAPI.Common.Logger.LogType.Error, _componentorderid);
            }
            return lst;
        }

        public ComponentPOitems Insert(ComponentPOitems oComponentpoitems)
        {

            NpgsqlTransaction transaction = null;
            try
            {
                using (NpgsqlConnection conn = CreateConnection())
                {
                    conn.Open();
                    transaction = conn.BeginTransaction();
                    NpgsqlCommand command = new NpgsqlCommand(SQL_Insert, conn, transaction);

                    Insert(transaction, conn, oComponentpoitems);
                    transaction.Commit();
                    conn.Close();
                }
            }
            catch (Exception exp)
            {
                WebAPI.Common.Logger.Instance(Context.OrganizationID).Log(exp, WebAPI.Common.Logger.LogType.Error, oComponentpoitems);
                throw exp;
            }

            return oComponentpoitems;
        }

        public ComponentPOitems Insert(NpgsqlTransaction transaction, NpgsqlConnection conn, ComponentPOitems oComponentpoitems)
        {

            NpgsqlCommand command = new NpgsqlCommand(SQL_Insert, conn, transaction);
            command.Parameters.Add("bomcategoryid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oComponentpoitems.bomcategoryid;
            command.Parameters.Add("bomid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oComponentpoitems.bomid;
            command.Parameters.Add("colourid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oComponentpoitems.colourid;
            command.Parameters.Add("taxid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oComponentpoitems.taxid;
            command.Parameters.Add("uomid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oComponentpoitems.uomid;
            command.Parameters.Add("coretypeid", NpgsqlTypes.NpgsqlDbType.Integer).Value = oComponentpoitems.coretypeid;
            command.Parameters.Add("qty", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oComponentpoitems.qty;
            command.Parameters.Add("width", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oComponentpoitems.width == null ? "" : oComponentpoitems.width;

            command.Parameters.Add("brandname", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oComponentpoitems.brandname == null ? "" : oComponentpoitems.brandname;
            command.Parameters.Add("colourcode", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oComponentpoitems.colourcode == null ? "" : oComponentpoitems.colourcode;
            command.Parameters.Add("colourshade", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oComponentpoitems.colourshade == null ? "" : oComponentpoitems.colourshade;
            command.Parameters.Add("designid", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oComponentpoitems.designid == null ? "" : oComponentpoitems.designid;

            command.Parameters.Add("rate", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oComponentpoitems.rate;
            command.Parameters.Add("amount", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oComponentpoitems.amount;
            command.Parameters.Add("imageid", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oComponentpoitems.imageid == null ? "" : oComponentpoitems.imageid;
            command.Parameters.Add("componentorderid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oComponentpoitems.componentorderid;
            command.Parameters.Add("isactive", NpgsqlTypes.NpgsqlDbType.Boolean).Value = oComponentpoitems.isactive;
            command.Parameters.Add("createdby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oComponentpoitems.createdby;
            command.Parameters.Add("modifiedby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oComponentpoitems.modifiedby;
            command.Parameters.Add("version", NpgsqlTypes.NpgsqlDbType.Integer).Value = oComponentpoitems.version;
            command.Parameters.Add("createdon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = oComponentpoitems.createdon;
            command.Parameters.Add("modifiedon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = oComponentpoitems.modifiedon;
            using (System.Data.IDataReader reader = command.ExecuteReader())
            {
                if (reader.Read())
                {
                    oComponentpoitems.id = Convert.ToInt64(reader[0].ToString());
                }
            }
            return oComponentpoitems;
        }

        public ComponentPOitems Update(ComponentPOitems oComponentpoitems)
        {

            NpgsqlTransaction transaction = null;
            try
            {
                using (NpgsqlConnection conn = CreateConnection())
                {
                    conn.Open();
                    transaction = conn.BeginTransaction();

                    Update(transaction, conn, oComponentpoitems);
                    transaction.Commit();
                    conn.Close();
                }
            }
            catch (Exception exp)
            {
                WebAPI.Common.Logger.Instance(Context.OrganizationID).Log(exp, WebAPI.Common.Logger.LogType.Error, oComponentpoitems);
                throw exp;
            }

            return oComponentpoitems;
        }

        public ComponentPOitems Update(NpgsqlTransaction transaction, NpgsqlConnection conn, ComponentPOitems oComponentpoitems)
        {

            NpgsqlCommand command = new NpgsqlCommand(SQL_Update, conn, transaction);
            command.Parameters.Add("id", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oComponentpoitems.id;
            command.Parameters.Add("bomcategoryid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oComponentpoitems.bomcategoryid;
            command.Parameters.Add("bomid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oComponentpoitems.bomid;
            command.Parameters.Add("colourid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oComponentpoitems.colourid;
            command.Parameters.Add("taxid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oComponentpoitems.taxid;
            command.Parameters.Add("uomid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oComponentpoitems.uomid;
            command.Parameters.Add("coretypeid", NpgsqlTypes.NpgsqlDbType.Integer).Value = oComponentpoitems.coretypeid;
            command.Parameters.Add("qty", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oComponentpoitems.qty;
            command.Parameters.Add("width", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oComponentpoitems.width == null ? "" : oComponentpoitems.width;

            command.Parameters.Add("brandname", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oComponentpoitems.brandname == null ? "" : oComponentpoitems.brandname;
            command.Parameters.Add("colourcode", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oComponentpoitems.colourcode == null ? "" : oComponentpoitems.colourcode;
            command.Parameters.Add("colourshade", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oComponentpoitems.colourshade == null ? "" : oComponentpoitems.colourshade;
            command.Parameters.Add("designid", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oComponentpoitems.designid == null ? "" : oComponentpoitems.designid;

            command.Parameters.Add("rate", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oComponentpoitems.rate;
            command.Parameters.Add("amount", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oComponentpoitems.amount;
            command.Parameters.Add("imageid", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oComponentpoitems.imageid == null ? "" : oComponentpoitems.imageid;
            command.Parameters.Add("componentorderid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oComponentpoitems.componentorderid;
            command.Parameters.Add("isactive", NpgsqlTypes.NpgsqlDbType.Boolean).Value = oComponentpoitems.isactive;
            command.Parameters.Add("createdby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oComponentpoitems.createdby;
            command.Parameters.Add("modifiedby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oComponentpoitems.modifiedby;
            command.Parameters.Add("version", NpgsqlTypes.NpgsqlDbType.Integer).Value = oComponentpoitems.version;
            command.Parameters.Add("createdon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = oComponentpoitems.createdon;
            command.Parameters.Add("modifiedon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = oComponentpoitems.modifiedon;
            using (System.Data.IDataReader reader = command.ExecuteReader())
            {
                if (reader.Read())
                {
                    oComponentpoitems.version = Convert.ToInt32(reader[0].ToString());
                }
            }

            return oComponentpoitems;
        }

        public bool Update(NpgsqlTransaction transaction, NpgsqlConnection conn, List<ComponentPOitemsDetail> list, long id)
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
                    item.componentorderid = id;
                    Insert(transaction, conn, item);
                }
            }
            return true;
        }

        public bool Delete(ComponentPOitems oComponentpoitems)
        {

            NpgsqlTransaction transaction = null;
            bool hasSuccessCount = false;
            try
            {
                using (NpgsqlConnection conn = CreateConnection())
                {
                    conn.Open();
                    transaction = conn.BeginTransaction();
                    hasSuccessCount = Delete(transaction, conn, oComponentpoitems);
                    transaction.Commit();
                    conn.Close();
                }
            }
            catch (Exception exp)
            {
                WebAPI.Common.Logger.Instance(Context.OrganizationID).Log(exp, WebAPI.Common.Logger.LogType.Error, oComponentpoitems);

                throw exp;
            }
            return hasSuccessCount;

        }

        public bool Delete(NpgsqlTransaction transaction, NpgsqlConnection conn, ComponentPOitems oComponentpoitems)
        {

            NpgsqlCommand command = new NpgsqlCommand(SQL_Delete, conn, transaction);
            command.Parameters.Add("isactive", NpgsqlTypes.NpgsqlDbType.Boolean).Value = oComponentpoitems.isactive;
            command.Parameters.Add("modifiedby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oComponentpoitems.modifiedby;
            command.Parameters.Add("modifiedon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = oComponentpoitems.modifiedon;
            command.Parameters.Add("version", NpgsqlTypes.NpgsqlDbType.Integer).Value = oComponentpoitems.version;
            command.Parameters.Add("id", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oComponentpoitems.id;
            int successCount = command.ExecuteNonQuery();

            return successCount > 0;
        }

        public bool DeleteAll(long componentorderid, long userid, NpgsqlConnection conn, NpgsqlTransaction transaction)
        {


            int successCount = 0;
            NpgsqlCommand command = new NpgsqlCommand(SQL_DeleteAll, conn, transaction);
            command.Parameters.Add("isactive", NpgsqlTypes.NpgsqlDbType.Boolean).Value = false;
            command.Parameters.Add("modifiedby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = userid;
            command.Parameters.Add("modifiedon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = DateTimeNow;
            command.Parameters.Add("componentorderid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = componentorderid;
            successCount = command.ExecuteNonQuery();

            return successCount > 0;
        }

        internal bool Cancelall(long id, long modifiedby, NpgsqlConnection conn, NpgsqlTransaction transaction)
        {
            int successCount = 0;
            NpgsqlCommand command = new NpgsqlCommand(SQL_CancelAll, conn, transaction);
            command.Parameters.Add("isactive", NpgsqlTypes.NpgsqlDbType.Boolean).Value = false;
            command.Parameters.Add("modifiedby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = modifiedby;
            command.Parameters.Add("modifiedon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = DateTimeNow;
            command.Parameters.Add("componentorderid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = id;
            successCount = command.ExecuteNonQuery();
            return successCount > 0;
        }
    }

}
