using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using InfyPOS.Models;
using InfyPOS.Common;
using Npgsql;
namespace InfyPOS.Data

{
    public class RefillboxDataService : SAASData
    {
        const string SQL_Search = @"select id, isactive, createdby, modifiedby, version, createdon, modifiedon, companyid, boxno, floorid, sectionid, boxtype, capacity, productid, brandid, sizeid, materialid, indisplay,locationid,length,width,height from refillbox where id=@id";

        const string SQL_SearchAll = @"select refillbox.id,boxno,floor.name as floorname,section.name as sectionname,
            boxtypes.name as boxtypename,location.name locationname from refillbox
            inner join referencelist floor on floor.id = refillbox.floorid
            inner join referencelist section on section.id = refillbox.sectionid
            inner join referencelist boxtypes on boxtypes.id = refillbox.boxtype
            inner join referencelist location on location.id = refillbox.locationid
            where refillbox.isactive=true and indisplay=true";


        const string SQL_Insert = @"INSERT INTO public.refillbox(isactive, createdby, modifiedby, version, createdon, modifiedon, companyid, boxno, floorid, sectionid, boxtype, capacity, productid, brandid, sizeid, materialid, indisplay,locationid,length,width,height)
                                    VALUES(@isactive, @createdby, @modifiedby, @version, @createdon, @modifiedon, @companyid, @boxno, @floorid, @sectionid, @boxtype, @capacity, @productid, @brandid, @sizeid, @materialid, @indisplay,@locationid,@length,@width,@height) RETURNING id";


        const string SQL_Update = @"UPDATE public.refillbox SET modifiedby=@modifiedby, version=@version+1, modifiedon=@modifiedon, companyid=@companyid, boxno=@boxno, floorid=@floorid, sectionid=@sectionid, boxtype=@boxtype, capacity=@capacity, productid=@productid, brandid=@brandid, 
                                    sizeid=@sizeid, materialid=@materialid, indisplay=@indisplay,locationid=@locationid,length=@length,width=@width,height=@height  where id=@id and version=@version RETURNING version";

        const string SQL_Delete = @"update public.refillbox set isactive=false,version=@version+1,modifiedby=@modifiedby,modifiedon=@modifiedon where id=@id and version=@version";

        public List<RefillBoxInfo> SearchAll()
        {
            List<RefillBoxInfo> lst = new List<RefillBoxInfo>();
            NpgsqlConnection conn = CreateConnection();
            try
            {
                conn.Open();

                NpgsqlCommand command = GetCommand(SQL_SearchAll, conn);
                  
                using (System.Data.IDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        RefillBoxInfo orefillbox = new RefillBoxInfo();
                        orefillbox.id = reader["id"] == DBNull.Value ? 0 : Convert.ToInt64(reader["id"]);
                        orefillbox.boxno = reader["boxno"] == DBNull.Value ? "" : reader["boxno"].ToString();
                        orefillbox.boxtypename = reader["boxtypename"] == DBNull.Value ? "" : reader["boxtypename"].ToString();
                        orefillbox.floorname = reader["floorname"] == DBNull.Value ? "" : reader["floorname"].ToString();
                        orefillbox.sectionname = reader["sectionname"] == DBNull.Value ? "" : reader["sectionname"].ToString();
                        orefillbox.locationname = reader["locationname"] == DBNull.Value ? "" : reader["locationname"].ToString();
                        lst.Add(orefillbox);
                    }
                }
                conn.Close();
            }
            catch (Exception exp)
            {
                if (conn != null) conn.Close();
                WebAPI.Common.Logger.Instance(Context.OrganizationID).Log(exp, WebAPI.Common.Logger.LogType.Error, "SearchAll");
            }
            return lst;
        }

        public List<Refillbox> Search(RefillboxCreteria _refillbox)
        {
            List<Refillbox> lst = new List<Refillbox>();
            NpgsqlConnection conn = CreateConnection();
            try
            {
                conn.Open();

                NpgsqlCommand command = null;
                if (_refillbox.id != 0)
                {
                    command = GetCommand(SQL_Search, conn);
                    command.Parameters.Add("id", NpgsqlTypes.NpgsqlDbType.Bigint).Value = _refillbox.id;
                }
                else
                {
                    QueryBuilder qb = new QueryBuilder();
                    qb.CurrentQuery = SQL_Search.Substring(0, SQL_Search.IndexOf("where"));

                    qb.AddParameter("isactive", true, "=");

                    if (!string.IsNullOrEmpty(_refillbox.boxno))
                        qb.AddParameter("boxno", _refillbox.boxno, "=");

                    if (_refillbox.companyid != 0)
                        qb.AddParameter("companyid", _refillbox.companyid, "=");

                    if (_refillbox.boxtype !=0)
                        qb.AddParameter("boxtype", _refillbox.boxtype, "=");

                    if (_refillbox.productid != 0)
                        qb.AddParameter("productid", _refillbox.productid, "=");

                    command = qb.GetCommand(conn);
                }

                using (System.Data.IDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        Refillbox orefillbox = new Refillbox();
                        orefillbox.id = reader["id"] == DBNull.Value ? 0 : Convert.ToInt64(reader["id"]);
                        orefillbox.isactive = reader["isactive"] == DBNull.Value ? false : Convert.ToBoolean(reader["isactive"]);
                        orefillbox.createdby = reader["createdby"] == DBNull.Value ? 0 : Convert.ToInt64(reader["createdby"]);
                        orefillbox.modifiedby = reader["modifiedby"] == DBNull.Value ? 0 : Convert.ToInt64(reader["modifiedby"]);
                        orefillbox.version = reader["version"] == DBNull.Value ? 0 : Convert.ToInt32(reader["version"]);
                        orefillbox.createdon = reader["createdon"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(reader["createdon"]);
                        orefillbox.modifiedon = reader["modifiedon"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(reader["modifiedon"]);
                        orefillbox.companyid = reader["companyid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["companyid"]);
                        orefillbox.boxno = reader["boxno"] == DBNull.Value ? "" : reader["boxno"].ToString();
                        orefillbox.floorid = reader["floorid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["floorid"]);
                        orefillbox.sectionid = reader["sectionid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["sectionid"]);
                        orefillbox.boxtype = reader["boxtype"] == DBNull.Value ? 0 : Convert.ToInt32(reader["boxtype"]);
                        orefillbox.capacity = reader["capacity"] == DBNull.Value ? 0 : Convert.ToInt32(reader["capacity"]);
                        orefillbox.productid = reader["productid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["productid"]);
                        orefillbox.brandid = reader["brandid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["brandid"]);
                        orefillbox.sizeid = reader["sizeid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["sizeid"]);
                        orefillbox.materialid = reader["materialid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["materialid"]);
                        orefillbox.indisplay = reader["indisplay"] == DBNull.Value ? false : Convert.ToBoolean(reader["indisplay"]);

                        orefillbox.locationid = reader["locationid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["locationid"]);
                        orefillbox.height = reader["height"] == DBNull.Value ? 0 : Convert.ToInt32(reader["height"]);
                        orefillbox.width = reader["width"] == DBNull.Value ? 0 : Convert.ToInt32(reader["width"]);
                        orefillbox.length = reader["length"] == DBNull.Value ? 0 : Convert.ToInt32(reader["length"]);

                        lst.Add(orefillbox);
                    }
                }
                conn.Close();
            }
            catch (Exception exp)
            {
                if (conn != null) conn.Close();
                WebAPI.Common.Logger.Instance(Context.OrganizationID).Log(exp, WebAPI.Common.Logger.LogType.Error, _refillbox);
            }
            return lst;
        }

        public Refillbox Insert(Refillbox oRefillbox)
        {

            NpgsqlTransaction transaction = null;
            try
            {
                using (NpgsqlConnection conn = CreateConnection())
                {
                    conn.Open();
                    transaction = conn.BeginTransaction();
                    NpgsqlCommand command = GetCommand(SQL_Insert, conn, transaction);

                    Insert(transaction, conn, oRefillbox);
                    transaction.Commit();
                    conn.Close();
                }
            }
            catch (Exception exp)
            {
                WebAPI.Common.Logger.Instance(Context.OrganizationID).Log(exp, WebAPI.Common.Logger.LogType.Error, oRefillbox);
                
                    
            }

            return oRefillbox;
        }

        public Refillbox Insert(NpgsqlTransaction transaction, NpgsqlConnection conn, Refillbox oRefillbox)
        {

            NpgsqlCommand command = GetCommand(SQL_Insert, conn, transaction);
            command.Parameters.Add("isactive", NpgsqlTypes.NpgsqlDbType.Boolean).Value = oRefillbox.isactive;
            command.Parameters.Add("createdby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = Context.UserID;
            command.Parameters.Add("modifiedby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = Context.UserID;
            command.Parameters.Add("version", NpgsqlTypes.NpgsqlDbType.Integer).Value = oRefillbox.version;
            command.Parameters.Add("createdon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = DateTimeNow;
            command.Parameters.Add("modifiedon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = DateTimeNow;
            command.Parameters.Add("companyid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oRefillbox.companyid;
            command.Parameters.Add("boxno", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oRefillbox.boxno == null ? "" : oRefillbox.boxno;
            command.Parameters.Add("floorid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oRefillbox.floorid;
            command.Parameters.Add("sectionid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oRefillbox.sectionid;
            command.Parameters.Add("boxtype", NpgsqlTypes.NpgsqlDbType.Integer).Value = oRefillbox.boxtype;
            command.Parameters.Add("capacity", NpgsqlTypes.NpgsqlDbType.Integer).Value = oRefillbox.capacity;
            command.Parameters.Add("productid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oRefillbox.productid;
            command.Parameters.Add("brandid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oRefillbox.brandid;
            command.Parameters.Add("sizeid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oRefillbox.sizeid;
            command.Parameters.Add("materialid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oRefillbox.materialid;
            command.Parameters.Add("indisplay", NpgsqlTypes.NpgsqlDbType.Boolean).Value = oRefillbox.indisplay;

            command.Parameters.Add("locationid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oRefillbox.locationid;
            command.Parameters.Add("height", NpgsqlTypes.NpgsqlDbType.Integer).Value = oRefillbox.height;
            command.Parameters.Add("length", NpgsqlTypes.NpgsqlDbType.Integer).Value = oRefillbox.length;
            command.Parameters.Add("width", NpgsqlTypes.NpgsqlDbType.Integer).Value = oRefillbox.width;

            using (System.Data.IDataReader reader = command.ExecuteReader())
            {
                if (reader.Read())
                {
                    oRefillbox.id = Convert.ToInt64(reader[0].ToString());
                }
            }
            return oRefillbox;
        }

        public Refillbox Update(Refillbox oRefillbox)
        {

            NpgsqlTransaction transaction = null;
            try
            {
                using (NpgsqlConnection conn = CreateConnection())
                {
                    conn.Open();
                    transaction = conn.BeginTransaction();

                    Update(transaction, conn, oRefillbox);
                    transaction.Commit();
                    conn.Close();
                }
            }
            catch (Exception exp)
            {
                WebAPI.Common.Logger.Instance(Context.OrganizationID).Log(exp, WebAPI.Common.Logger.LogType.Error, oRefillbox);
                
                    
            }

            return oRefillbox;
        }

        public Refillbox Update(NpgsqlTransaction transaction, NpgsqlConnection conn, Refillbox oRefillbox)
        {

            NpgsqlCommand command = GetCommand(SQL_Update, conn, transaction);
            command.Parameters.Add("id", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oRefillbox.id;
            command.Parameters.Add("isactive", NpgsqlTypes.NpgsqlDbType.Boolean).Value = oRefillbox.isactive;
            command.Parameters.Add("createdby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oRefillbox.createdby;
            command.Parameters.Add("modifiedby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = Context.UserID;
            command.Parameters.Add("version", NpgsqlTypes.NpgsqlDbType.Integer).Value = oRefillbox.version;
            command.Parameters.Add("createdon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = oRefillbox.createdon;
            command.Parameters.Add("modifiedon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = DateTimeNow;
            command.Parameters.Add("companyid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oRefillbox.companyid;
            command.Parameters.Add("boxno", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oRefillbox.boxno == null ? "" : oRefillbox.boxno;
            command.Parameters.Add("floorid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oRefillbox.floorid;
            command.Parameters.Add("sectionid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oRefillbox.sectionid;
            command.Parameters.Add("boxtype", NpgsqlTypes.NpgsqlDbType.Integer).Value = oRefillbox.boxtype;
            command.Parameters.Add("capacity", NpgsqlTypes.NpgsqlDbType.Integer).Value = oRefillbox.capacity;
            command.Parameters.Add("productid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oRefillbox.productid;
            command.Parameters.Add("brandid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oRefillbox.brandid;
            command.Parameters.Add("sizeid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oRefillbox.sizeid;
            command.Parameters.Add("materialid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oRefillbox.materialid;
            command.Parameters.Add("indisplay", NpgsqlTypes.NpgsqlDbType.Boolean).Value = oRefillbox.indisplay;
            command.Parameters.Add("locationid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oRefillbox.locationid;
            command.Parameters.Add("height", NpgsqlTypes.NpgsqlDbType.Integer).Value = oRefillbox.height;
            command.Parameters.Add("length", NpgsqlTypes.NpgsqlDbType.Integer).Value = oRefillbox.length;
            command.Parameters.Add("width", NpgsqlTypes.NpgsqlDbType.Integer).Value = oRefillbox.width;

            using (System.Data.IDataReader reader = command.ExecuteReader())
            {
                if (reader.Read())
                {
                    oRefillbox.version = Convert.ToInt32(reader[0].ToString());
                }
            }

            return oRefillbox;
        }


        public bool Delete(Refillbox oRefillbox)
        {

            NpgsqlTransaction transaction = null;
            bool hasSuccessCount = false;
            try
            {
                using (NpgsqlConnection conn = CreateConnection())
                {
                    conn.Open();
                    transaction = conn.BeginTransaction();
                    hasSuccessCount = Delete(transaction, conn, oRefillbox);
                    transaction.Commit();
                    conn.Close();
                }
            }
            catch (Exception exp)
            {
                WebAPI.Common.Logger.Instance(Context.OrganizationID).Log(exp, WebAPI.Common.Logger.LogType.Error, oRefillbox);

                
                    
            }
            return hasSuccessCount;

        }

        public bool Delete(NpgsqlTransaction transaction, NpgsqlConnection conn, Refillbox oRefillbox)
        {

            NpgsqlCommand command = GetCommand(SQL_Delete, conn, transaction);
            command.Parameters.Add("isactive", NpgsqlTypes.NpgsqlDbType.Boolean).Value = oRefillbox.isactive;
            command.Parameters.Add("modifiedby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = Context.UserID;
            command.Parameters.Add("modifiedon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = DateTimeNow;
            command.Parameters.Add("version", NpgsqlTypes.NpgsqlDbType.Integer).Value = oRefillbox.version;
            command.Parameters.Add("id", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oRefillbox.id;
            int successCount = command.ExecuteNonQuery();

            return successCount > 0;
        }

    }

}
