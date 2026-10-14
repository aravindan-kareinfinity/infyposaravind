using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using InfyPOS.Models;
using InfyPOS.Common;
using Npgsql;
namespace InfyPOS.Data

{
    public class BrandDataService : SAASData
    {
        const string SQL_Search = @"select id, code, name, printingname, barcodeprefix,logoid, barcodesuffix,discountmode, discountvalue, preferredsupplierid,marginlist, isactive, createdby, modifiedby, version, createdon, modifiedon,groupid,minmargin,maxmargin from brand where id=@id";

        const string SQL_Insert = @"INSERT INTO public.brand(code, name, printingname, barcodeprefix,logoid, barcodesuffix,discountmode, discountvalue, preferredsupplierid,marginlist, isactive, createdby, modifiedby, version, createdon, modifiedon,groupid,minmargin,maxmargin) 
        VALUES(@code, @name, @printingname, @barcodeprefix,@logoid, @barcodesuffix,@discountmode, @discountvalue, @preferredsupplierid,@marginlist, @isactive, @createdby, @modifiedby, @version, @createdon, @modifiedon,@groupid,@minmargin,@maxmargin) RETURNING id";

        const string SQL_Update = @"UPDATE public.brand SET code=@code, name=@name, printingname=@printingname, barcodeprefix=@barcodeprefix,logoid=@logoid, barcodesuffix=@barcodesuffix,groupid=@groupid, discountmode=@discountmode, discountvalue=@discountvalue, 
        preferredsupplierid=@preferredsupplierid,marginlist=@marginlist, modifiedby=@modifiedby, version=@version+1, modifiedon=@modifiedon, isactive=@isactive,minmargin=@minmargin,maxmargin=@maxmargin where id=@id and version=@version RETURNING version";

        const string SQL_Delete = @"update public.brand set isactive=false,version=@version+1,modifiedby=@modifiedby,modifiedon=@modifiedon where id=@id and version=@version";

        public List<Brand> Search(BrandCreteria _brand)
        {
            List<Brand> lst = new List<Brand>();
            NpgsqlConnection conn = CreateConnection();
            try
            {
                conn.Open();

                NpgsqlCommand command = null;
                if (_brand.id != 0)
                {
                    command = GetCommand(SQL_Search, conn);
                    command.Parameters.Add("id", NpgsqlTypes.NpgsqlDbType.Bigint).Value = _brand.id;
                }
                else
                {
                    QueryBuilder qb = new QueryBuilder();
                    qb.CurrentQuery = SQL_Search.Substring(0, SQL_Search.IndexOf("where"));

                    if (_brand.inactive)
                        qb.AddParameter("isactive", false, "=");
                    else
                        qb.AddParameter("isactive", true, "=");

                    if (!string.IsNullOrEmpty(_brand.code))
                        qb.AddParameter("code", "%" + _brand.code + "%", " ilike ");
                    if (!string.IsNullOrEmpty(_brand.name))
                        qb.AddParameter("name", "%" + _brand.name + "%", " ilike ");

                    if (_brand.ids != null && _brand.ids.Count>0)
                        qb.AddInParamer("brand.id", string.Join<long>(",", _brand.ids));

                    qb.SortField = "name";
                    command = qb.GetCommand(conn);
                }

                using (System.Data.IDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        Brand obrand = new Brand();
                        obrand.id = reader["id"] == DBNull.Value ? 0 : Convert.ToInt64(reader["id"]);
                        obrand.groupid = reader["groupid"] == DBNull.Value ? 0 : Convert.ToInt32(reader["groupid"]);

                        obrand.minmargin = reader["minmargin"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["minmargin"]);
                        obrand.maxmargin = reader["maxmargin"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["maxmargin"]);

                        obrand.code = reader["code"] == DBNull.Value ? "" : reader["code"].ToString();
                        obrand.name = reader["name"] == DBNull.Value ? "" : reader["name"].ToString();
                        obrand.printingname = reader["printingname"] == DBNull.Value ? "" : reader["printingname"].ToString();
                        obrand.logoid = reader["logoid"] == DBNull.Value ? "" : reader["logoid"].ToString();
                        obrand.barcodeprefix = reader["barcodeprefix"] == DBNull.Value ? "" : reader["barcodeprefix"].ToString();
                        obrand.barcodesuffix = reader["barcodesuffix"] == DBNull.Value ? "" : reader["barcodesuffix"].ToString();
                        obrand.discountmode = reader["discountmode"] == DBNull.Value ? 0 : Convert.ToInt32(reader["discountmode"]);
                        obrand.discountvalue = reader["discountvalue"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["discountvalue"]);
                        obrand.marginlist_json = reader["marginlist"] == DBNull.Value ? "" : Convert.ToString(reader["marginlist"]);

                        obrand.preferredsupplierid = reader["preferredsupplierid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["preferredsupplierid"]);
                        obrand.isactive = reader["isactive"] == DBNull.Value ? false : Convert.ToBoolean(reader["isactive"]);
                        obrand.createdby = reader["createdby"] == DBNull.Value ? 0 : Convert.ToInt64(reader["createdby"]);
                        obrand.modifiedby = reader["modifiedby"] == DBNull.Value ? 0 : Convert.ToInt64(reader["modifiedby"]);
                        obrand.version = reader["version"] == DBNull.Value ? 0 : Convert.ToInt32(reader["version"]);
                        obrand.createdon = reader["createdon"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(reader["createdon"]);
                        obrand.modifiedon = reader["modifiedon"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(reader["modifiedon"]);
                        lst.Add(obrand);
                    }
                }
                conn.Close();
            }
            catch (Exception exp)
            {
                if (conn != null) conn.Close();
                WebAPI.Common.Logger.Instance(Context.OrganizationID).Log(exp, WebAPI.Common.Logger.LogType.Error, _brand);
            }
            return lst;
        }

        private bool UniqueValidation(Brand oBrand)
        {
            if (string.IsNullOrEmpty(oBrand.name)) return false;

            string _Query = @"select count(id) items from brand where lower(name)=@name and id!=@id";

            if (oBrand.id > 0)
                _Query = @"select count(id) items from brand where isactive=true and lower(name)=@name and id!=@id";
            int count = 0;
            using (NpgsqlConnection conn = CreateConnection())
            {
                conn.Open();
                NpgsqlCommand command = GetCommand(_Query, conn);
                command.Parameters.Add("id", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oBrand.id;
                command.Parameters.Add("name", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oBrand.name.ToLower();
                using (System.Data.IDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        count = reader["items"] == DBNull.Value ? 0 : Convert.ToInt32(reader["items"]);
                    }
                }
                conn.Close();
            }
            return count > 0;
        }
        public Brand Insert(Brand oBrand)
        {
            if (UniqueValidation(oBrand))
            {
                oBrand.ErrorMessage = "Duplicate Brand Name Exists. Please verify active and inactive brands";
                return oBrand;
            }

            NpgsqlTransaction transaction = null;
            try
            {
                var sequence = SequenceManager.Instance(Context.OrganizationID).Get("brand.code", 0);
                using (NpgsqlConnection conn = CreateConnection())
                {
                    conn.Open();
                    transaction = conn.BeginTransaction();
                    NpgsqlCommand command = GetCommand(SQL_Insert, conn, transaction);
                    if (string.IsNullOrEmpty(oBrand.code))
                        oBrand.code = sequence.GetNextNumber(Context,conn, transaction);

                    Insert(transaction, conn, oBrand);
                    transaction.Commit();
                    conn.Close();
                }
            }
            catch (Exception exp)
            {
                WebAPI.Common.Logger.Instance(Context.OrganizationID).Log(exp, WebAPI.Common.Logger.LogType.Error, oBrand);
            }

            return oBrand;
        }

        public Brand Insert(NpgsqlTransaction transaction, NpgsqlConnection conn, Brand oBrand)
        {

            NpgsqlCommand command = GetCommand(SQL_Insert, conn, transaction);
            command.Parameters.Add("code", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oBrand.code == null ? "" : oBrand.code;
            command.Parameters.Add("name", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oBrand.name == null ? "" : oBrand.name;
            command.Parameters.Add("printingname", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oBrand.printingname == null ? "" : oBrand.printingname;
            command.Parameters.Add("barcodeprefix", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oBrand.barcodeprefix == null ? "" : oBrand.barcodeprefix;
            command.Parameters.Add("barcodesuffix", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oBrand.barcodesuffix == null ? "" : oBrand.barcodesuffix;
            command.Parameters.Add("logoid", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oBrand.logoid == null ? "" : oBrand.logoid;
            command.Parameters.Add("discountmode", NpgsqlTypes.NpgsqlDbType.Integer).Value = oBrand.discountmode;
            command.Parameters.Add("discountvalue", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oBrand.discountvalue;
            command.Parameters.Add("preferredsupplierid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oBrand.preferredsupplierid;
            command.Parameters.Add("marginlist", NpgsqlTypes.NpgsqlDbType.Json).Value = oBrand.marginlist == null ? "null" : oBrand.marginlist_json ;
            command.Parameters.Add("isactive", NpgsqlTypes.NpgsqlDbType.Boolean).Value = (Services.ConfigManager.Instance(Context.OrganizationID).Config.intray_brand ? false : true);
            command.Parameters.Add("createdby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = Context.UserID;
            command.Parameters.Add("modifiedby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = Context.UserID;
            command.Parameters.Add("version", NpgsqlTypes.NpgsqlDbType.Integer).Value = oBrand.version;
            command.Parameters.Add("groupid", NpgsqlTypes.NpgsqlDbType.Integer).Value = oBrand.groupid;
            command.Parameters.Add("minmargin", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oBrand.minmargin;
            command.Parameters.Add("maxmargin", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oBrand.maxmargin;
            command.Parameters.Add("createdon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = DateTimeNow;
            command.Parameters.Add("modifiedon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = DateTimeNow;
            using (System.Data.IDataReader reader = command.ExecuteReader())
            {
                if (reader.Read())
                {
                    oBrand.id = Convert.ToInt64(reader[0].ToString());
                }
            }
            if (Services.ConfigManager.Instance(Context.OrganizationID).Config.intray_brand)
            {
                Services.IntrayManager.Instance(this.Context.OrganizationID).Add(this.Context.OrganizationID,Services.IntrayManager.EntryType.Brand,
                            oBrand.id, string.Format("BRAND CREATION REQUEST - {0} ", oBrand.name), 
                            Context.UserID, 0, transaction, Context);
                
            }
            return oBrand;
        }

        public Brand Update(Brand oBrand)
        {
            if (UniqueValidation(oBrand))
            {
                oBrand.ErrorMessage = "Duplicate Brand Name Exists. Please verify active and inactive brands";
                return oBrand;
            }

            NpgsqlTransaction transaction = null;
            try
            {
                using (NpgsqlConnection conn = CreateConnection())
                {
                    conn.Open();
                    transaction = conn.BeginTransaction();

                    Update(transaction, conn, oBrand);
                    transaction.Commit();
                    conn.Close();
                }
            }
            catch (Exception exp)
            {
                WebAPI.Common.Logger.Instance(Context.OrganizationID).Log(exp, WebAPI.Common.Logger.LogType.Error, oBrand);


            }

            return oBrand;
        }

        public Brand Update(NpgsqlTransaction transaction, NpgsqlConnection conn, Brand oBrand)
        {

            NpgsqlCommand command = GetCommand(SQL_Update, conn, transaction);
            command.Parameters.Add("id", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oBrand.id;
            command.Parameters.Add("code", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oBrand.code == null ? "" : oBrand.code;
            command.Parameters.Add("name", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oBrand.name == null ? "" : oBrand.name;
            command.Parameters.Add("printingname", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oBrand.printingname == null ? "" : oBrand.printingname;
            command.Parameters.Add("barcodeprefix", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oBrand.barcodeprefix == null ? "" : oBrand.barcodeprefix;
            command.Parameters.Add("logoid", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oBrand.logoid == null ? "" : oBrand.logoid;
            command.Parameters.Add("barcodesuffix", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oBrand.barcodesuffix == null ? "" : oBrand.barcodesuffix;
            command.Parameters.Add("discountmode", NpgsqlTypes.NpgsqlDbType.Integer).Value = oBrand.discountmode;
            command.Parameters.Add("discountvalue", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oBrand.discountvalue;
            command.Parameters.Add("preferredsupplierid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oBrand.preferredsupplierid;
            command.Parameters.Add("marginlist", NpgsqlTypes.NpgsqlDbType.Json).Value = oBrand.marginlist == null ? "null" : oBrand.marginlist_json;
            command.Parameters.Add("isactive", NpgsqlTypes.NpgsqlDbType.Boolean).Value = oBrand.isactive;
            command.Parameters.Add("createdby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oBrand.createdby;
            command.Parameters.Add("modifiedby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = Context.UserID;
            command.Parameters.Add("version", NpgsqlTypes.NpgsqlDbType.Integer).Value = oBrand.version;
            command.Parameters.Add("createdon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = oBrand.createdon;
            command.Parameters.Add("groupid", NpgsqlTypes.NpgsqlDbType.Integer).Value = oBrand.groupid;
            command.Parameters.Add("modifiedon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = DateTimeNow;
            command.Parameters.Add("minmargin", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oBrand.minmargin;
            command.Parameters.Add("maxmargin", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oBrand.maxmargin;
            using (System.Data.IDataReader reader = command.ExecuteReader())
            {
                if (reader.Read())
                {
                    oBrand.version = Convert.ToInt32(reader[0].ToString());
                }
            }

            return oBrand;
        }


        public bool Delete(Brand oBrand)
        {

            NpgsqlTransaction transaction = null;
            bool hasSuccessCount = false;
            try
            {
                using (NpgsqlConnection conn = CreateConnection())
                {
                    conn.Open();
                    transaction = conn.BeginTransaction();
                    hasSuccessCount = Delete(transaction, conn, oBrand);
                    transaction.Commit();
                    conn.Close();
                }
            }
            catch (Exception exp)
            {
                WebAPI.Common.Logger.Instance(Context.OrganizationID).Log(exp, WebAPI.Common.Logger.LogType.Error, oBrand);
            }
            return hasSuccessCount;
        }

        internal bool Marge(MergeBrandList item)
        {
            Brand _newitem = null;
            if (item.id == 0)
            {
                item.ErrorMessage = "There is no Item selected as Merge To";
                return false;
            }
            NpgsqlTransaction transaction = null;
            try
            {
                if (!item.deletelist.Exists(e => e.id == item.id))
                {
                    item.ErrorMessage = "Merge To Item must be selected from master list of items.";
                    return false;
                }
                if (item.deletelist.Exists(e => e.id == item.id))
                    _newitem = item.deletelist.Find(e => e.id == item.id);

                if (_newitem == null)
                {
                    item.ErrorMessage = "Merge To Item must be selected from master list of items.";
                    return false;
                }
                using (NpgsqlConnection conn = CreateConnection())
                {
                    conn.Open();
                    transaction = conn.BeginTransaction();
                    item.deletelist.RemoveAll(e => e.id == item.id);
                    foreach (var ditem in item.deletelist)
                    {
                        Delete(transaction, conn, ditem);
                    }

                    string query = @"update items set brandid=@newid, brandname=@newname where brandid=@oldid;
                                     update reorderrules set brandid=@newid where brandid=@oldid;
                                     update mfgdesign set brandid=@newid where brandid=@oldid;
                                    update jobworkbillgrnitems set brandid=@newid where brandid=@oldid;
                                    update pricecontrol set brandid=@newid where brandid=@oldid;
                                    update salesorderitems set brandid=@newid where brandid=@oldid;";

                    NpgsqlCommand command = new NpgsqlCommand(query, conn, transaction);
                    command.Parameters.Add("newid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = item.id;
                    command.Parameters.Add("newname", NpgsqlTypes.NpgsqlDbType.Varchar).Value = _newitem.name;
                    command.Parameters.Add("oldid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = 0;
                    foreach (var ditem in item.deletelist)
                    {
                        command.Parameters["oldid"].Value = ditem.id;
                        int successCount = command.ExecuteNonQuery();
                    }
                    transaction.Commit();
                    conn.Close();
                }
            }
            catch (Exception exp)
            {
                WebAPI.Common.Logger.Instance(Context.OrganizationID).Log(exp, WebAPI.Common.Logger.LogType.Error, exp.StackTrace);
                return false;
            }
            return true;
        }

        public bool Delete(NpgsqlTransaction transaction, NpgsqlConnection conn, Brand oBrand)
        {

            NpgsqlCommand command = GetCommand(SQL_Delete, conn, transaction);
            command.Parameters.Add("isactive", NpgsqlTypes.NpgsqlDbType.Boolean).Value = oBrand.isactive;
            command.Parameters.Add("modifiedby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = Context.UserID;
            command.Parameters.Add("modifiedon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = DateTimeNow;
            command.Parameters.Add("version", NpgsqlTypes.NpgsqlDbType.Integer).Value = oBrand.version;
            command.Parameters.Add("id", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oBrand.id;
            int successCount = command.ExecuteNonQuery();

            return successCount > 0;
        }

    }

}
