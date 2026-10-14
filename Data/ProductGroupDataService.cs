using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using InfyPOS.Models;
using InfyPOS.Common;
using Npgsql;
namespace InfyPOS.Data

{
    public class ProductgroupDataService : SAASData
    {
        const string SQL_Search = @"select isactive, createdby, modifiedby, version, id, createdon, modifiedon, code, name, searchmode, materialenable, materialname, patternenable, patternname, styleenable, stylename, sizeenable, sizename, typeenable, typename, colourenable, colourname,expiryenable,expiryname,designenable,designname from productgroup where id=@id";

        const string SQL_Insert = @"INSERT INTO public.productgroup(isactive, createdby, modifiedby, version, createdon, modifiedon, code, name, searchmode, materialenable, materialname, patternenable, patternname, styleenable, stylename, sizeenable, sizename, typeenable, typename, colourenable, colourname,expiryenable,expiryname,designenable,designname) VALUES(@isactive, @createdby, @modifiedby, @version, @createdon, @modifiedon, @code, @name, @searchmode, @materialenable, @materialname, @patternenable, @patternname, @styleenable, @stylename, @sizeenable, @sizename, @typeenable, @typename, @colourenable, @colourname,@expiryenable,@expiryname,@designenable,@designname) RETURNING id";

        const string SQL_Update = @"UPDATE public.productgroup SET modifiedby=@modifiedby, version=@version+1, modifiedon=@modifiedon, code=@code, name=@name, searchmode=@searchmode, 
                                    expiryenable=@expiryenable,expiryname=@expiryname,designenable=@designenable,designname=@designname,materialenable=@materialenable, materialname=@materialname, 
                                    patternenable=@patternenable, patternname=@patternname, styleenable=@styleenable, stylename=@stylename, sizeenable=@sizeenable, sizename=@sizename, typeenable=@typeenable,
                                    typename=@typename, colourenable=@colourenable, colourname=@colourname where id=@id and version=@version RETURNING version";

        const string SQL_Delete = @"update public.productgroup set isactive=false,version=@version+1,modifiedby=@modifiedby,modifiedon=@modifiedon where id=@id and version=@version";

        public List<Productgroup> Search(ProductgroupCreteria _productgroup)
        {
            List<Productgroup> lst = new List<Productgroup>();
            NpgsqlConnection conn = CreateConnection();
            try
            {
                conn.Open();

                NpgsqlCommand command = null;
                if (_productgroup.id != 0)
                {
                    command = GetCommand(SQL_Search, conn);
                    command.Parameters.Add("id", NpgsqlTypes.NpgsqlDbType.Bigint).Value = _productgroup.id;
                }
                else
                {
                    QueryBuilder qb = new QueryBuilder();
                    qb.CurrentQuery = SQL_Search.Substring(0, SQL_Search.IndexOf("where"));
                    qb.AddParameter("isactive", true, "=");

                    //if (!string.IsNullOrEmpty(_productgroup.Code))
                    //    qb.AddParameter("code", _productgroup.Code, "=");

                    command = qb.GetCommand(conn);
                }

                using (System.Data.IDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        Productgroup oproductgroup = new Productgroup();
                        oproductgroup.isactive = reader["isactive"] == DBNull.Value ? false : Convert.ToBoolean(reader["isactive"]);
                        oproductgroup.createdby = reader["createdby"] == DBNull.Value ? 0 : Convert.ToInt64(reader["createdby"]);
                        oproductgroup.modifiedby = reader["modifiedby"] == DBNull.Value ? 0 : Convert.ToInt64(reader["modifiedby"]);
                        oproductgroup.version = reader["version"] == DBNull.Value ? 0 : Convert.ToInt32(reader["version"]);
                        oproductgroup.id = reader["id"] == DBNull.Value ? 0 : Convert.ToInt64(reader["id"]);
                        oproductgroup.createdon = reader["createdon"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(reader["createdon"]);
                        oproductgroup.modifiedon = reader["modifiedon"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(reader["modifiedon"]);
                        oproductgroup.code = reader["code"] == DBNull.Value ? "" : reader["code"].ToString();
                        oproductgroup.name = reader["name"] == DBNull.Value ? "" : reader["name"].ToString();
                        oproductgroup.searchmode = reader["searchmode"] == DBNull.Value ? 0 : Convert.ToInt32(reader["searchmode"]);
                        oproductgroup.materialenable = reader["materialenable"] == DBNull.Value ? false : Convert.ToBoolean(reader["materialenable"]);
                        oproductgroup.materialname = reader["materialname"] == DBNull.Value ? "" : reader["materialname"].ToString();
                        oproductgroup.patternenable = reader["patternenable"] == DBNull.Value ? false : Convert.ToBoolean(reader["patternenable"]);
                        oproductgroup.patternname = reader["patternname"] == DBNull.Value ? "" : reader["patternname"].ToString();
                        oproductgroup.styleenable = reader["styleenable"] == DBNull.Value ? false : Convert.ToBoolean(reader["styleenable"]);
                        oproductgroup.stylename = reader["stylename"] == DBNull.Value ? "" : reader["stylename"].ToString();
                        oproductgroup.sizeenable = reader["sizeenable"] == DBNull.Value ? false : Convert.ToBoolean(reader["sizeenable"]);
                        oproductgroup.sizename = reader["sizename"] == DBNull.Value ? "" : reader["sizename"].ToString();
                        oproductgroup.typeenable = reader["typeenable"] == DBNull.Value ? false : Convert.ToBoolean(reader["typeenable"]);
                        oproductgroup.typename = reader["typename"] == DBNull.Value ? "" : reader["typename"].ToString();
                        oproductgroup.colourenable = reader["colourenable"] == DBNull.Value ? false : Convert.ToBoolean(reader["colourenable"]);
                        oproductgroup.colourname = reader["colourname"] == DBNull.Value ? "" : reader["colourname"].ToString();

                        oproductgroup.expiryenable = reader["expiryenable"] == DBNull.Value ? false : Convert.ToBoolean(reader["expiryenable"]);
                        oproductgroup.expiryname = reader["expiryname"] == DBNull.Value ? "" : reader["expiryname"].ToString();
                        oproductgroup.designenable = reader["designenable"] == DBNull.Value ? false : Convert.ToBoolean(reader["designenable"]);
                        oproductgroup.designname = reader["designname"] == DBNull.Value ? "" : reader["designname"].ToString();

                        lst.Add(oproductgroup);
                    }
                }
                conn.Close();
            }
            catch (Exception exp)
            {
                if (conn != null) conn.Close();
                WebAPI.Common.Logger.Instance(Context.OrganizationID).Log(exp, WebAPI.Common.Logger.LogType.Error, _productgroup);
            }
            return lst;
        }

        public Productgroup Insert(Productgroup oProductgroup)
        {

            NpgsqlTransaction transaction = null;
            try
            {
                using (NpgsqlConnection conn = CreateConnection())
                {
                    conn.Open();
                    transaction = conn.BeginTransaction();
                    NpgsqlCommand command = GetCommand(SQL_Insert, conn, transaction);

                    Insert(transaction, conn, oProductgroup);
                    transaction.Commit();
                    conn.Close();
                }
            }
            catch (Exception exp)
            {
                WebAPI.Common.Logger.Instance(Context.OrganizationID).Log(exp, WebAPI.Common.Logger.LogType.Error, oProductgroup);
                throw exp;
            }

            return oProductgroup;
        }

        public Productgroup Insert(NpgsqlTransaction transaction, NpgsqlConnection conn, Productgroup oProductgroup)
        {

            NpgsqlCommand command = GetCommand(SQL_Insert, conn, transaction);
            command.Parameters.Add("isactive", NpgsqlTypes.NpgsqlDbType.Boolean).Value = oProductgroup.isactive;
            command.Parameters.Add("createdby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = Context.UserID;
            command.Parameters.Add("modifiedby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = Context.UserID;
            command.Parameters.Add("version", NpgsqlTypes.NpgsqlDbType.Integer).Value = oProductgroup.version;
            command.Parameters.Add("createdon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = DateTimeNow;
            command.Parameters.Add("modifiedon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = DateTimeNow;
            command.Parameters.Add("code", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oProductgroup.code == null ? "" : oProductgroup.code;
            command.Parameters.Add("name", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oProductgroup.name == null ? "" : oProductgroup.name;
            command.Parameters.Add("searchmode", NpgsqlTypes.NpgsqlDbType.Integer).Value = oProductgroup.searchmode;
            command.Parameters.Add("materialenable", NpgsqlTypes.NpgsqlDbType.Boolean).Value = oProductgroup.materialenable;
            command.Parameters.Add("materialname", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oProductgroup.materialname == null ? "" : oProductgroup.materialname;
            command.Parameters.Add("patternenable", NpgsqlTypes.NpgsqlDbType.Boolean).Value = oProductgroup.patternenable;
            command.Parameters.Add("patternname", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oProductgroup.patternname == null ? "" : oProductgroup.patternname;
            command.Parameters.Add("styleenable", NpgsqlTypes.NpgsqlDbType.Boolean).Value = oProductgroup.styleenable;
            command.Parameters.Add("stylename", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oProductgroup.stylename == null ? "" : oProductgroup.stylename;
            command.Parameters.Add("sizeenable", NpgsqlTypes.NpgsqlDbType.Boolean).Value = oProductgroup.sizeenable;
            command.Parameters.Add("sizename", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oProductgroup.sizename == null ? "" : oProductgroup.sizename;
            command.Parameters.Add("typeenable", NpgsqlTypes.NpgsqlDbType.Boolean).Value = oProductgroup.typeenable;
            command.Parameters.Add("typename", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oProductgroup.typename == null ? "" : oProductgroup.typename;
            command.Parameters.Add("colourenable", NpgsqlTypes.NpgsqlDbType.Boolean).Value = oProductgroup.colourenable;
            command.Parameters.Add("colourname", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oProductgroup.colourname == null ? "" : oProductgroup.colourname;
            command.Parameters.Add("designenable", NpgsqlTypes.NpgsqlDbType.Boolean).Value = oProductgroup.designenable;
            command.Parameters.Add("designname", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oProductgroup.designname == null ? "" : oProductgroup.designname;
            command.Parameters.Add("expiryenable", NpgsqlTypes.NpgsqlDbType.Boolean).Value = oProductgroup.expiryenable;
            command.Parameters.Add("expiryname", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oProductgroup.expiryname == null ? "" : oProductgroup.expiryname;
            using (System.Data.IDataReader reader = command.ExecuteReader())
            {
                if (reader.Read())
                {
                    oProductgroup.id = Convert.ToInt64(reader[0].ToString());
                }
            }
            return oProductgroup;
        }

        public Productgroup Update(Productgroup oProductgroup)
        {

            NpgsqlTransaction transaction = null;
            try
            {
                using (NpgsqlConnection conn = CreateConnection())
                {
                    conn.Open();
                    transaction = conn.BeginTransaction();

                    Update(transaction, conn, oProductgroup);
                    transaction.Commit();
                    conn.Close();
                }
            }
            catch (Exception exp)
            {
                WebAPI.Common.Logger.Instance(Context.OrganizationID).Log(exp, WebAPI.Common.Logger.LogType.Error, oProductgroup);
                throw exp;
            }

            return oProductgroup;
        }

        public Productgroup Update(NpgsqlTransaction transaction, NpgsqlConnection conn, Productgroup oProductgroup)
        {

            NpgsqlCommand command = GetCommand(SQL_Update, conn, transaction);
            command.Parameters.Add("isactive", NpgsqlTypes.NpgsqlDbType.Boolean).Value = oProductgroup.isactive;
            command.Parameters.Add("createdby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oProductgroup.createdby;
            command.Parameters.Add("modifiedby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = Context.UserID;
            command.Parameters.Add("version", NpgsqlTypes.NpgsqlDbType.Integer).Value = oProductgroup.version;
            command.Parameters.Add("id", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oProductgroup.id;
            command.Parameters.Add("createdon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = oProductgroup.createdon;
            command.Parameters.Add("modifiedon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = DateTimeNow;
            command.Parameters.Add("code", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oProductgroup.code == null ? "" : oProductgroup.code;
            command.Parameters.Add("name", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oProductgroup.name == null ? "" : oProductgroup.name;
            command.Parameters.Add("searchmode", NpgsqlTypes.NpgsqlDbType.Integer).Value = oProductgroup.searchmode;
            command.Parameters.Add("materialenable", NpgsqlTypes.NpgsqlDbType.Boolean).Value = oProductgroup.materialenable;
            command.Parameters.Add("materialname", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oProductgroup.materialname == null ? "" : oProductgroup.materialname;
            command.Parameters.Add("patternenable", NpgsqlTypes.NpgsqlDbType.Boolean).Value = oProductgroup.patternenable;
            command.Parameters.Add("patternname", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oProductgroup.patternname == null ? "" : oProductgroup.patternname;
            command.Parameters.Add("styleenable", NpgsqlTypes.NpgsqlDbType.Boolean).Value = oProductgroup.styleenable;
            command.Parameters.Add("stylename", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oProductgroup.stylename == null ? "" : oProductgroup.stylename;
            command.Parameters.Add("sizeenable", NpgsqlTypes.NpgsqlDbType.Boolean).Value = oProductgroup.sizeenable;
            command.Parameters.Add("sizename", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oProductgroup.sizename == null ? "" : oProductgroup.sizename;
            command.Parameters.Add("typeenable", NpgsqlTypes.NpgsqlDbType.Boolean).Value = oProductgroup.typeenable;
            command.Parameters.Add("typename", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oProductgroup.typename == null ? "" : oProductgroup.typename;
            command.Parameters.Add("colourenable", NpgsqlTypes.NpgsqlDbType.Boolean).Value = oProductgroup.colourenable;
            command.Parameters.Add("colourname", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oProductgroup.colourname == null ? "" : oProductgroup.colourname;
            command.Parameters.Add("designenable", NpgsqlTypes.NpgsqlDbType.Boolean).Value = oProductgroup.designenable;
            command.Parameters.Add("designname", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oProductgroup.designname == null ? "" : oProductgroup.designname;
            command.Parameters.Add("expiryenable", NpgsqlTypes.NpgsqlDbType.Boolean).Value = oProductgroup.expiryenable;
            command.Parameters.Add("expiryname", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oProductgroup.expiryname == null ? "" : oProductgroup.expiryname;
            using (System.Data.IDataReader reader = command.ExecuteReader())
            {
                if (reader.Read())
                {
                    oProductgroup.version = Convert.ToInt32(reader[0].ToString());
                }
            }

            return oProductgroup;
        }


        public bool Delete(Productgroup oProductgroup)
        {

            NpgsqlTransaction transaction = null;
            bool hasSuccessCount = false;
            try
            {
                using (NpgsqlConnection conn = CreateConnection())
                {
                    conn.Open();
                    transaction = conn.BeginTransaction();
                    hasSuccessCount = Delete(transaction, conn, oProductgroup);
                    transaction.Commit();
                    conn.Close();
                }
            }
            catch (Exception exp)
            {
                WebAPI.Common.Logger.Instance(Context.OrganizationID).Log(exp, WebAPI.Common.Logger.LogType.Error, oProductgroup);

                throw exp;
            }
            return hasSuccessCount;

        }

        public bool Delete(NpgsqlTransaction transaction, NpgsqlConnection conn, Productgroup oProductgroup)
        {

            NpgsqlCommand command = GetCommand(SQL_Delete, conn, transaction);
            command.Parameters.Add("isactive", NpgsqlTypes.NpgsqlDbType.Boolean).Value = oProductgroup.isactive;
            command.Parameters.Add("modifiedby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = Context.UserID;
            command.Parameters.Add("modifiedon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = DateTimeNow;
            command.Parameters.Add("version", NpgsqlTypes.NpgsqlDbType.Integer).Value = oProductgroup.version;
            command.Parameters.Add("id", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oProductgroup.id;
            int successCount = command.ExecuteNonQuery();

            return successCount > 0;
        }

    }

}
