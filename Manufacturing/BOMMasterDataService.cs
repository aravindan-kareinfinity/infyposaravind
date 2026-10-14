using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using InfyPOS.Models;
using InfyPOS.Common;
using Npgsql;
namespace InfyPOS.Data

{
    public class BOMMasterDataService : SAASData
    {
        const string SQL_Search = @"select BOMMaster.id id, BOMMaster.name as name, printingname,  categoryid, uomid, colorid, supplierid,
                                    width,hsncode,imageid, taxid,BOMMaster.isactive as isactive, BOMMaster.createdby as createdby, BOMMaster.modifiedby, BOMMaster.version, 
                                    BOMMaster.createdon, bommastercode,supplierbrand,designno,romin,romax,tskuid,tproductid,
                                    BOMMaster.modifiedon,coretypeid,landingcost,buyingrate,colourlist,multisupplier,supplierlist,colourmode from bommaster where BOMMaster.id=@id";

        const string SQL_Insert = @"INSERT INTO public.BOMMaster(name, printingname, categoryid, uomid, colorid,supplierid, width,hsncode,imageid,taxid,isactive, createdby, modifiedby, version, createdon, modifiedon,coretypeid,landingcost,buyingrate,colourlist,multisupplier,supplierlist,colourmode,bommastercode,supplierbrand,designno,activestatus,romin,romax,tproductid,tskuid) 
        VALUES(@name, @printingname, @categoryid, @uomid, @colorid,@supplierid, @width,@hsncode,@imageid, @taxid, @isactive, @createdby, @modifiedby, @version, @createdon, @modifiedon,@coretypeid,@landingcost,@buyingrate,@colourlist,@multisupplier,@supplierlist,@colourmode,@bommastercode,@supplierbrand,@designno,@activestatus,@romin,@romax,@tproductid,@tskuid) RETURNING id";


        const string SQL_Update = @"UPDATE public.BOMMaster SET name=@name, printingname=@printingname, categoryid=@categoryid, uomid=@uomid, colorid=@colorid,supplierid=@supplierid, 
                                    taxid=@taxid, width=@width,hsncode=@hsncode, modifiedby=@modifiedby,imageid=@imageid,romin=@romin,romax=@romax,tskuid=@tskuid,tproductid=@tproductid,
                                    landingcost=@landingcost,buyingrate=@buyingrate,coretypeid=@coretypeid, version=@version+1, modifiedon=@modifiedon,isactive=@isactive,colourlist=@colourlist,
                                    colourmode=@colourmode,supplierbrand=@supplierbrand,designno=@designno,multisupplier=@multisupplier,supplierlist=@supplierlist where id=@id and version=@version  RETURNING version";

        const string SQL_Delete = @"update public.BOMMaster set isactive=false,version=@version+1,modifiedby=@modifiedby,modifiedon=@modifiedon where id=@id and version=@version  RETURNING version";

        public List<BOMMaster> Search(BOMMasterCreteria _BOMMaster)
        {
            List<BOMMaster> lst = new List<BOMMaster>();
            NpgsqlConnection conn = CreateConnection();
            try
            {
                conn.Open();

                NpgsqlCommand command = null;
                if (_BOMMaster.id != 0)
                {
                    command = GetCommand(SQL_Search, conn);
                    command.Parameters.AddWithValue("id", _BOMMaster.id);
                }
                else
                {
                    QueryBuilder qb = new QueryBuilder();
                    qb.CurrentQuery = SQL_Search.Substring(0, SQL_Search.IndexOf("where"));

                    if(_BOMMaster.coretypeid>0)
                        qb.AddParameter("coretypeid", _BOMMaster.coretypeid, "=");

                    if (_BOMMaster.categoryid > 0)
                        qb.AddParameter("categoryid", _BOMMaster.categoryid, "=");

                    if (!string.IsNullOrEmpty(_BOMMaster.name))
                        qb.AddParameter("name", _BOMMaster.name, "=");

                    if (_BOMMaster.ids != null && _BOMMaster.ids.Count > 0)
                        qb.AddInParamer("id", string.Join(",", _BOMMaster.ids));

                    if(_BOMMaster.inactive)
                        qb.AddParameter("isactive", false, "=");
                    else
                        qb.AddParameter("isactive", true, "=");


                    if (Services.ConfigManager.Instance(Context.OrganizationID).Config.intray_mfgdesign)
                    {
                        qb.AddParameter("activestatus", 2, "=");
                    }

                    command = qb.GetCommand(conn);

                }

                using (System.Data.IDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        BOMMaster oBOMMaster = new BOMMaster();
                        oBOMMaster.id = reader["id"] == DBNull.Value ? 0 : Convert.ToInt64(reader["id"]);
                        oBOMMaster.bommastercode = reader["bommastercode"] == DBNull.Value ? "" : reader["bommastercode"].ToString();
                        oBOMMaster.name = reader["name"] == DBNull.Value ? "" : reader["name"].ToString();
                        oBOMMaster.printingname = reader["printingname"] == DBNull.Value ? "" : reader["printingname"].ToString();

                        oBOMMaster.supplierbrand = reader["supplierbrand"] == DBNull.Value ? "" : reader["supplierbrand"].ToString();
                        oBOMMaster.designno = reader["designno"] == DBNull.Value ? "" : reader["designno"].ToString();

                        oBOMMaster.tproductid = reader["tproductid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["tproductid"]);
                        oBOMMaster.tskuid = reader["tskuid"] == DBNull.Value ? "" : reader["tskuid"].ToString();

                        oBOMMaster.categoryid = reader["categoryid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["categoryid"]);
                        oBOMMaster.uomid = reader["uomid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["uomid"]);
                        oBOMMaster.taxid = reader["taxid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["taxid"]);
                        oBOMMaster.colorid = reader["colorid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["colorid"]);
                        oBOMMaster.supplierid = reader["supplierid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["supplierid"]);
                        oBOMMaster.coretypeid = reader["coretypeid"] == DBNull.Value ? 0 : Convert.ToInt32(reader["coretypeid"]);
                        oBOMMaster.imageid = reader["imageid"] == DBNull.Value ? "" : reader["imageid"].ToString();
                        oBOMMaster.width = reader["width"] == DBNull.Value ? "" : reader["width"].ToString();
                        oBOMMaster.hsncode = reader["hsncode"] == DBNull.Value ? "" : reader["hsncode"].ToString();
                        oBOMMaster.colourlist_json = reader["colourlist"] == DBNull.Value ? "null" : reader["colourlist"].ToString();
                        oBOMMaster.colourmode = reader["colourmode"] == DBNull.Value ? (BOMMaster.ColourMode.None) : (BOMMaster.ColourMode)Convert.ToInt32(reader["colourmode"]);

                        oBOMMaster.supplierlist_json = reader["supplierlist"] == DBNull.Value ? "null" : reader["supplierlist"].ToString();
                        oBOMMaster.multisupplier = reader["multisupplier"] == DBNull.Value ? false: Convert.ToBoolean(reader["multisupplier"]);

                        oBOMMaster.landingcost = reader["landingcost"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["landingcost"]);
                        oBOMMaster.buyingrate = reader["buyingrate"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["buyingrate"]);

                        oBOMMaster.romax = reader["romax"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["romax"]);
                        oBOMMaster.romin = reader["romin"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["romin"]);

                        oBOMMaster.isactive = reader["isactive"] == DBNull.Value ? false : Convert.ToBoolean(reader["isactive"]);
                        oBOMMaster.createdby = reader["createdby"] == DBNull.Value ? 0 : Convert.ToInt64(reader["createdby"]);
                        oBOMMaster.modifiedby = reader["modifiedby"] == DBNull.Value ? 0 : Convert.ToInt64(reader["modifiedby"]);
                        oBOMMaster.version = reader["version"] == DBNull.Value ? 0 : Convert.ToInt32(reader["version"].ToString());
                        oBOMMaster.createdon = reader["createdon"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(reader["createdon"]);
                        oBOMMaster.modifiedon = reader["modifiedon"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(reader["modifiedon"]);

                        lst.Add(oBOMMaster);
                    }
                }

                conn.Close();
            }


            catch (Exception exp)
            {
                WebAPI.Common.Logger.Instance(Context.OrganizationID).Log(exp, WebAPI.Common.Logger.LogType.Error, exp.StackTrace);
            }
            return lst;
        }

        public List<BOMBase> SearchList(BOMMasterCreteria _BOMMaster)
        {
            List<BOMBase> lst = new List<BOMBase>();
            NpgsqlConnection conn = CreateConnection();
            try
            {
                conn.Open();

                NpgsqlCommand command = null;
                if (_BOMMaster.id != 0)
                {
                    command = GetCommand(SQL_Search, conn);
                    command.Parameters.AddWithValue("id", _BOMMaster.id);
                }
                else
                {
                    QueryBuilder qb = new QueryBuilder();
                    qb.CurrentQuery = SQL_Search.Substring(0, SQL_Search.IndexOf("where"));

                    if (_BOMMaster.coretypeid > 0)
                        qb.AddParameter("coretypeid", _BOMMaster.coretypeid, "=");

                    if (_BOMMaster.categoryid > 0)
                        qb.AddParameter("categoryid", _BOMMaster.categoryid, "=");

                    if (!string.IsNullOrEmpty(_BOMMaster.name))
                        qb.AddParameter("name", _BOMMaster.name, "=");

                    if (_BOMMaster.ids != null && _BOMMaster.ids.Count > 0)
                        qb.AddInParamer("id", string.Join(",", _BOMMaster.ids));

                    qb.AddParameter("isactive", true, "=");


                    if (Services.ConfigManager.Instance(Context.OrganizationID).Config.intray_mfgdesign)
                    {
                        qb.AddParameter("activestatus", 2, "=");
                    }

                    command = qb.GetCommand(conn);

                }

                bool isfabricbased = InfyPOS.Services.ConfigManager.Instance(this.Context.OrganizationID).Config.mfg_fabricbasedorder;
                using (System.Data.IDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        BOMBase oBOMMaster = new BOMBase();
                        oBOMMaster.id = reader["id"] == DBNull.Value ? 0 : Convert.ToInt64(reader["id"]);
                        oBOMMaster.bommastercode = reader["bommastercode"] == DBNull.Value ? "" : reader["bommastercode"].ToString();
                        oBOMMaster.name = reader["name"] == DBNull.Value ? "" : reader["name"].ToString();
                        oBOMMaster.printingname = reader["printingname"] == DBNull.Value ? "" : reader["printingname"].ToString();
                        oBOMMaster.tproductid = reader["tproductid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["tproductid"]);
                        oBOMMaster.tskuid = reader["tskuid"] == DBNull.Value ? "" : reader["tskuid"].ToString();
                        oBOMMaster.categoryid = reader["categoryid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["categoryid"]);
                        oBOMMaster.coretypeid = reader["coretypeid"] == DBNull.Value ? 0 : Convert.ToInt32(reader["coretypeid"]);
                        oBOMMaster.colourlist_json = reader["colourlist"] == DBNull.Value ? "null" : reader["colourlist"].ToString();
                        oBOMMaster.taxid = reader["taxid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["taxid"]);
                        oBOMMaster.buyingrate = reader["buyingrate"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["buyingrate"]);

                        oBOMMaster.colourmode = reader["colourmode"] == DBNull.Value ? (BOMMaster.ColourMode.None) : (BOMMaster.ColourMode)Convert.ToInt32(reader["colourmode"]);
                        if (!isfabricbased && !string.IsNullOrEmpty(oBOMMaster.printingname))
                        {
                            oBOMMaster.title = oBOMMaster.printingname + "(" + oBOMMaster.name + ")";
                        }
                        else
                        {
                            oBOMMaster.title = oBOMMaster.name;
                        }

                        lst.Add(oBOMMaster);
                    }
                }
                conn.Close();
            }
            catch (Exception exp)
            {
                WebAPI.Common.Logger.Instance(Context.OrganizationID).Log(exp, WebAPI.Common.Logger.LogType.Error, exp.StackTrace);
            }
            return lst;
        }
        internal bool ChangeStatus(ChangeAttributes item)
        {

            try
            {
                using (NpgsqlConnection conn = CreateConnection())
                {
                    conn.Open();

                    NpgsqlCommand command = GetCommand("update bommaster set activestatus=@activestatus,modifiedby=@modifiedby,modifiedon=@modifiedon where id in (" + string.Join(",", item.ids) + ")", conn);
                    command.Parameters.Add("activestatus", NpgsqlTypes.NpgsqlDbType.Bigint).Value = item.statusid;
                    command.Parameters.Add("modifiedby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = Context.UserID;
                    command.Parameters.Add("modifiedon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = DateTimeNow;
                    int successCount = command.ExecuteNonQuery();
                    conn.Close();
                    return successCount > 0;
                }
            }
            catch (Exception exp)
            {
                return false;
            }
        }

        public BOMMaster Insert(BOMMaster oBOMMaster)
        {

            NpgsqlTransaction transaction = null;
            try
            {
                using (NpgsqlConnection conn = CreateConnection())
                {
                    conn.Open();
                    transaction = conn.BeginTransaction();
                    var sequence = SequenceManager.Instance(Context.OrganizationID).Get("bommaster.bommastercode", 0);
                    oBOMMaster.bommastercode = sequence.GetNextNumber(Context, conn, transaction);
                    Insert(transaction, conn, oBOMMaster);
                    if (Services.ConfigManager.Instance(Context.OrganizationID).Config.intray_bommaster)
                    {
                        Services.IntrayManager.Instance(Context.OrganizationID).Add(Context.OrganizationID,
                            Services.IntrayManager.EntryType.BomMaster,
                        oBOMMaster.id, string.Format("New BOM {0} registered.", oBOMMaster.name),
                        Context.UserID, 0, transaction, this.Context);
                    }
                    transaction.Commit();
                    conn.Close();
                }
            }
            catch (Exception exp)
            {

                WebAPI.Common.Logger.Instance(Context.OrganizationID).Log(exp, WebAPI.Common.Logger.LogType.Error, exp.StackTrace);
            }

            return oBOMMaster;
        }

        public BOMMaster Insert(NpgsqlTransaction transaction, NpgsqlConnection conn, BOMMaster oBOMMaster)
        {

            NpgsqlCommand command = GetCommand(SQL_Insert, conn, transaction);

            if ((Services.ConfigManager.Instance(Context.OrganizationID).Config == null ||
                Services.ConfigManager.Instance(Context.OrganizationID).Config.intray_bommaster== false))
            {
                oBOMMaster.isactive = true;
                oBOMMaster.Activestatus = BaseEntity.ActiveStatus.Approved;
            }
            else
            {
                oBOMMaster.isactive = false;
                oBOMMaster.Activestatus = BaseEntity.ActiveStatus.UnApproved;
            }
            command.Parameters.Add("isactive", NpgsqlTypes.NpgsqlDbType.Boolean).Value = oBOMMaster.isactive;
            command.Parameters.Add("activestatus", NpgsqlTypes.NpgsqlDbType.Integer).Value = oBOMMaster.activestatus;
            command.Parameters.Add("name", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oBOMMaster.name == null ? "" : oBOMMaster.name;
            command.Parameters.Add("bommastercode", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oBOMMaster.bommastercode == null ? "" : oBOMMaster.bommastercode;
            command.Parameters.Add("printingname", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oBOMMaster.printingname == null ? "" : oBOMMaster.printingname;

            command.Parameters.Add("supplierbrand", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oBOMMaster.supplierbrand == null ? "" : oBOMMaster.supplierbrand;
            command.Parameters.Add("designno", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oBOMMaster.designno == null ? "" : oBOMMaster.designno;

            command.Parameters.Add("categoryid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oBOMMaster.categoryid;
            command.Parameters.Add("coretypeid", NpgsqlTypes.NpgsqlDbType.Integer).Value = oBOMMaster.coretypeid;
            command.Parameters.Add("landingcost", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oBOMMaster.landingcost;
            command.Parameters.Add("buyingrate", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oBOMMaster.buyingrate;

            command.Parameters.Add("tskuid", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oBOMMaster.tskuid == null ? "" : oBOMMaster.tskuid;
            command.Parameters.Add("tproductid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oBOMMaster.tproductid;

            command.Parameters.Add("romin", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oBOMMaster.romin;
            command.Parameters.Add("romax", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oBOMMaster.romax;

            command.Parameters.Add("colourlist", NpgsqlTypes.NpgsqlDbType.Json).Value = oBOMMaster.colourlist == null ? "[]" : oBOMMaster.colourlist_json;
            command.Parameters.Add("colourmode", NpgsqlTypes.NpgsqlDbType.Integer).Value = (int)oBOMMaster.colourmode;

            command.Parameters.Add("supplierlist", NpgsqlTypes.NpgsqlDbType.Json).Value = oBOMMaster.supplierlist == null ? "[]" : oBOMMaster.supplierlist_json;
            command.Parameters.Add("multisupplier", NpgsqlTypes.NpgsqlDbType.Boolean).Value = oBOMMaster.multisupplier;

            command.Parameters.Add("uomid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oBOMMaster.uomid;
            command.Parameters.Add("colorid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oBOMMaster.colorid;
            command.Parameters.Add("supplierid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oBOMMaster.supplierid;
            command.Parameters.Add("taxid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oBOMMaster.taxid;
            command.Parameters.Add("width", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oBOMMaster.width == null ? "" : oBOMMaster.width;
            command.Parameters.Add("hsncode", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oBOMMaster.hsncode == null ? "" : oBOMMaster.hsncode;

            command.Parameters.Add("isactive", NpgsqlTypes.NpgsqlDbType.Boolean).Value = oBOMMaster.isactive;
            command.Parameters.Add("createdby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = Context.UserID;
            command.Parameters.Add("modifiedby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = Context.UserID;
            command.Parameters.Add("version", NpgsqlTypes.NpgsqlDbType.Integer).Value = oBOMMaster.version;
            command.Parameters.Add("createdon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = DateTimeNow;
            command.Parameters.Add("modifiedon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = DateTimeNow;
            command.Parameters.Add("imageid", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oBOMMaster.imageid == null ? "" : oBOMMaster.imageid;

            using (System.Data.IDataReader reader = command.ExecuteReader())
            {
                if (reader.Read())
                {
                    oBOMMaster.id = Convert.ToInt64(reader[0].ToString());
                }
            }

            return oBOMMaster;
        }

        public BOMMaster Update(BOMMaster oBOMMaster)
        {

            NpgsqlTransaction transaction = null;
            try
            {
                using (NpgsqlConnection conn = CreateConnection())
                {
                    conn.Open();
                    transaction = conn.BeginTransaction();

                    Update(transaction, conn, oBOMMaster);
                    if (Services.ConfigManager.Instance(Context.OrganizationID).Config.intray_bommaster)
                    {
                        Services.IntrayManager.Instance(Context.OrganizationID).Add(Context.OrganizationID, Services.IntrayManager.EntryType.Customer,
                        oBOMMaster.id, string.Format("BOM {0} information updated.", oBOMMaster.name),
                        Context.UserID, 0, transaction, this.Context);
                    }
                    transaction.Commit();
                    conn.Close();
                }
            }
            catch (Exception exp)
            {

                WebAPI.Common.Logger.Instance(Context.OrganizationID).Log(exp, WebAPI.Common.Logger.LogType.Error, exp.StackTrace);
            }

            return oBOMMaster;
        }

        public BOMMaster Update(NpgsqlTransaction transaction, NpgsqlConnection conn, BOMMaster oBOMMaster)
        {

            NpgsqlCommand command = GetCommand(SQL_Update, conn, transaction);
            command.Parameters.Add("id", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oBOMMaster.id;
            command.Parameters.Add("name", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oBOMMaster.name == null ? "" : oBOMMaster.name;
            command.Parameters.Add("printingname", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oBOMMaster.printingname == null ? "" : oBOMMaster.printingname;
            command.Parameters.Add("categoryid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oBOMMaster.categoryid;
            command.Parameters.Add("uomid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oBOMMaster.uomid;
            command.Parameters.Add("colorid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oBOMMaster.colorid;
            command.Parameters.Add("supplierid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oBOMMaster.supplierid;
            command.Parameters.Add("taxid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oBOMMaster.taxid;
            command.Parameters.Add("romin", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oBOMMaster.romin;
            command.Parameters.Add("romax", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oBOMMaster.romax;
            command.Parameters.Add("width", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oBOMMaster.width == null ? "" : oBOMMaster.width;
            command.Parameters.Add("hsncode", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oBOMMaster.hsncode == null ? "" : oBOMMaster.hsncode;
            command.Parameters.Add("isactive", NpgsqlTypes.NpgsqlDbType.Boolean).Value = oBOMMaster.isactive;
            command.Parameters.Add("createdby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = Context.UserID;
            command.Parameters.Add("modifiedby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = Context.UserID;
            command.Parameters.Add("version", NpgsqlTypes.NpgsqlDbType.Integer).Value = oBOMMaster.version;
            command.Parameters.Add("createdon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = DateTimeNow;
            command.Parameters.Add("landingcost", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oBOMMaster.landingcost;
            command.Parameters.Add("buyingrate", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oBOMMaster.buyingrate;
            command.Parameters.Add("tskuid", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oBOMMaster.tskuid == null ? "" : oBOMMaster.tskuid;
            command.Parameters.Add("tproductid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oBOMMaster.tproductid;
            command.Parameters.Add("supplierbrand", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oBOMMaster.supplierbrand == null ? "" : oBOMMaster.supplierbrand;
            command.Parameters.Add("designno", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oBOMMaster.designno == null ? "" : oBOMMaster.designno;
            command.Parameters.Add("colourlist", NpgsqlTypes.NpgsqlDbType.Json).Value = oBOMMaster.colourlist == null ? "[]" : oBOMMaster.colourlist_json;
            command.Parameters.Add("supplierlist", NpgsqlTypes.NpgsqlDbType.Json).Value = oBOMMaster.supplierlist == null ? "[]" : oBOMMaster.supplierlist_json;
            command.Parameters.Add("multisupplier", NpgsqlTypes.NpgsqlDbType.Boolean).Value = oBOMMaster.multisupplier;

            command.Parameters.Add("colourmode", NpgsqlTypes.NpgsqlDbType.Integer).Value = (int)oBOMMaster.colourmode;
            command.Parameters.Add("imageid", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oBOMMaster.imageid == null ? "" : oBOMMaster.imageid;
            command.Parameters.Add("modifiedon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = DateTimeNow;
            command.Parameters.Add("width", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oBOMMaster.width == null ? "" : oBOMMaster.width;
            command.Parameters.Add("hsncode", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oBOMMaster.hsncode == null ? "" : oBOMMaster.hsncode;
            command.Parameters.Add("coretypeid", NpgsqlTypes.NpgsqlDbType.Integer).Value = oBOMMaster.coretypeid;
            using (System.Data.IDataReader reader = command.ExecuteReader())
            {
                if (reader.Read())
                {
                    oBOMMaster.version = Convert.ToInt32(reader[0]);
                }
            }

            return oBOMMaster;
        }


        public bool Delete(BOMMaster oBOMMaster)
        {

            NpgsqlTransaction transaction = null;
            bool hasSuccessCount = false;
            try
            {
                using (NpgsqlConnection conn = CreateConnection())
                {
                    conn.Open();
                    transaction = conn.BeginTransaction();
                    hasSuccessCount = Delete(transaction, conn, oBOMMaster);
                    transaction.Commit();
                    conn.Close();
                }
            }
            catch (Exception exp)
            {
                WebAPI.Common.Logger.Instance(Context.OrganizationID).Log(exp, WebAPI.Common.Logger.LogType.Error, exp.StackTrace);

            }
            return hasSuccessCount;

        }


        internal bool UpdateStatus(NpgsqlTransaction transaction, NpgsqlConnection conn, BOMMaster oBOMMaster)
        {
            NpgsqlCommand command = GetCommand("update BOMMaster set isactive=@isactive,activestatus=@activestatus,modifiedby=@modifiedby,modifiedon=@modifiedon where id=@id", conn, transaction);
            command.Parameters.Add("isactive", NpgsqlTypes.NpgsqlDbType.Boolean).Value = oBOMMaster.isactive;
            command.Parameters.Add("modifiedby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = Context.UserID;
            command.Parameters.Add("modifiedon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = DateTimeNow;
            command.Parameters.Add("activestatus", NpgsqlTypes.NpgsqlDbType.Integer).Value = oBOMMaster.activestatus;
            command.Parameters.Add("id", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oBOMMaster.id;
            int successCount = command.ExecuteNonQuery();
            return successCount > 0;
        }
        public bool Delete(NpgsqlTransaction transaction, NpgsqlConnection conn, BOMMaster oBOMMaster)
        {

            NpgsqlCommand command = GetCommand(SQL_Delete, conn, transaction);
            command.Parameters.Add("isactive", NpgsqlTypes.NpgsqlDbType.Boolean).Value = oBOMMaster.isactive;
            command.Parameters.Add("modifiedby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = Context.UserID;
            command.Parameters.Add("modifiedon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = DateTimeNow;
            command.Parameters.Add("version", NpgsqlTypes.NpgsqlDbType.Integer).Value = oBOMMaster.version;
            command.Parameters.Add("id", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oBOMMaster.id;
            int successCount = command.ExecuteNonQuery();

            return successCount > 0;
        }

    }

}
