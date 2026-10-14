using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using InfyPOS.Models;
using InfyPOS.Common;
using Npgsql;
namespace InfyPOS.Data

{
    public class ServicecentersDataService : SAASData
    {
        const string SQL_Search = @"select modifiedon, createdby, modifiedby, isactive, id, name, address, city, contactno, contactperson, emailid, workinghours, createdon, version, brandid, productid, locations from servicecenters where id=@id";


        const string SQL_Insert = @"INSERT INTO public.servicecenters(modifiedon, createdby, modifiedby, isactive, name, address, city, contactno, contactperson, emailid, workinghours, createdon, version, brandid, productid, locations) VALUES(@modifiedon, @createdby, @modifiedby, @isactive, @name, @address, @city, @contactno, @contactperson, @emailid, @workinghours, @createdon, @version, @brandid, @productid, @locations) RETURNING id";


        const string SQL_Update = @"UPDATE public.servicecenters SET modifiedon=@modifiedon, modifiedby=@modifiedby, name=@name, address=@address, city=@city, contactno=@contactno, contactperson=@contactperson, emailid=@emailid, workinghours=@workinghours, version=@version+1, brandid=@brandid, productid=@productid, locations=@locations where id=@id and version=@version RETURNING version";

        const string SQL_Delete = @"update public.servicecenters set isactive=false,version=@version+1,modifiedby=@modifiedby,modifiedon=@modifiedon where id=@id and version=@version";

        public List<Servicecenters> Search(ServicecentersCreteria _servicecenters)
        {
            List<Servicecenters> lst = new List<Servicecenters>();
            NpgsqlConnection conn = CreateConnection();
            try
            {
                conn.Open();

                NpgsqlCommand command = null;
                if (_servicecenters.id != 0)
                {
                    command = new NpgsqlCommand(SQL_Search, conn);
                    command.Parameters.Add("id", NpgsqlTypes.NpgsqlDbType.Bigint).Value = _servicecenters.id;
                }
                else
                {
                    QueryBuilder qb = new QueryBuilder();
                    qb.CurrentQuery = SQL_Search.Substring(0, SQL_Search.IndexOf("where"));
                    qb.AddParameter("isactive", true, "=");

                    //if (!string.IsNullOrEmpty(_servicecenters.Code))
                    //    qb.AddParameter("code", _servicecenters.Code, "=");

                    command = qb.GetCommand(conn);
                }

                using (System.Data.IDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        Servicecenters oservicecenters = new Servicecenters();
                        oservicecenters.modifiedon = reader["modifiedon"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(reader["modifiedon"]);
                        oservicecenters.createdby = reader["createdby"] == DBNull.Value ? 0 : Convert.ToInt64(reader["createdby"]);
                        oservicecenters.modifiedby = reader["modifiedby"] == DBNull.Value ? 0 : Convert.ToInt64(reader["modifiedby"]);
                        oservicecenters.isactive = reader["isactive"] == DBNull.Value ? false : Convert.ToBoolean(reader["isactive"]);
                        oservicecenters.id = reader["id"] == DBNull.Value ? 0 : Convert.ToInt64(reader["id"]);
                        oservicecenters.name = reader["name"] == DBNull.Value ? "" : reader["name"].ToString();
                        oservicecenters.address = reader["address"] == DBNull.Value ? "" : reader["address"].ToString();
                        oservicecenters.city = reader["city"] == DBNull.Value ? "" : reader["city"].ToString();
                        oservicecenters.contactno = reader["contactno"] == DBNull.Value ? "" : reader["contactno"].ToString();
                        oservicecenters.contactperson = reader["contactperson"] == DBNull.Value ? "" : reader["contactperson"].ToString();
                        oservicecenters.emailid = reader["emailid"] == DBNull.Value ? "" : reader["emailid"].ToString();
                        oservicecenters.workinghours = reader["workinghours"] == DBNull.Value ? "" : reader["workinghours"].ToString();
                        oservicecenters.createdon = reader["createdon"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(reader["createdon"]);
                        oservicecenters.version = reader["version"] == DBNull.Value ? 0 : Convert.ToInt32(reader["version"]);
                        oservicecenters.brandid = reader["brandid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["brandid"]);
                        oservicecenters.productid = reader["productid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["productid"]);
                        oservicecenters.locations_json = reader["locations"] == DBNull.Value ? null : reader["locations"].ToString();
                        lst.Add(oservicecenters);
                    }
                }
                conn.Close();
            }
            catch (Exception exp)
            {
                if (conn != null) conn.Close();
                WebAPI.Common.Logger.Instance(Context.OrganizationID).Log(exp, WebAPI.Common.Logger.LogType.Error, _servicecenters);
            }
            return lst;
        }

        public Servicecenters Insert(Servicecenters oServicecenters)
        {

            NpgsqlTransaction transaction = null;
            try
            {
                using (NpgsqlConnection conn = CreateConnection())
                {
                    conn.Open();
                    transaction = conn.BeginTransaction();
                    NpgsqlCommand command = new NpgsqlCommand(SQL_Insert, conn, transaction);

                    Insert(transaction, conn, oServicecenters);
                    transaction.Commit();
                    conn.Close();
                }
            }
            catch (Exception exp)
            {
                WebAPI.Common.Logger.Instance(Context.OrganizationID).Log(exp, WebAPI.Common.Logger.LogType.Error, oServicecenters);
                throw exp;
            }

            return oServicecenters;
        }

        public Servicecenters Insert(NpgsqlTransaction transaction, NpgsqlConnection conn, Servicecenters oServicecenters)
        {

            NpgsqlCommand command = new NpgsqlCommand(SQL_Insert, conn, transaction);
            command.Parameters.Add("modifiedon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = oServicecenters.modifiedon;
            command.Parameters.Add("createdby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oServicecenters.createdby;
            command.Parameters.Add("modifiedby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oServicecenters.modifiedby;
            command.Parameters.Add("isactive", NpgsqlTypes.NpgsqlDbType.Boolean).Value = oServicecenters.isactive;
            command.Parameters.Add("name", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oServicecenters.name == null ? "" : oServicecenters.name;
            command.Parameters.Add("address", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oServicecenters.address == null ? "" : oServicecenters.address;
            command.Parameters.Add("city", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oServicecenters.city == null ? "" : oServicecenters.city;
            command.Parameters.Add("contactno", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oServicecenters.contactno == null ? "" : oServicecenters.contactno;
            command.Parameters.Add("contactperson", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oServicecenters.contactperson == null ? "" : oServicecenters.contactperson;
            command.Parameters.Add("emailid", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oServicecenters.emailid == null ? "" : oServicecenters.emailid;
            command.Parameters.Add("workinghours", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oServicecenters.workinghours == null ? "" : oServicecenters.workinghours;
            command.Parameters.Add("createdon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = oServicecenters.createdon;
            command.Parameters.Add("version", NpgsqlTypes.NpgsqlDbType.Integer).Value = oServicecenters.version;
            command.Parameters.Add("brandid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oServicecenters.brandid;
            command.Parameters.Add("productid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oServicecenters.productid;
            command.Parameters.Add("locations", NpgsqlTypes.NpgsqlDbType.Json).Value = oServicecenters.locations_json == null ? "null" : oServicecenters.locations_json;
            using (System.Data.IDataReader reader = command.ExecuteReader())
            {
                if (reader.Read())
                {
                    oServicecenters.id = Convert.ToInt64(reader[0].ToString());
                }
            }
            return oServicecenters;
        }

        public Servicecenters Update(Servicecenters oServicecenters)
        {

            NpgsqlTransaction transaction = null;
            try
            {
                using (NpgsqlConnection conn = CreateConnection())
                {
                    conn.Open();
                    transaction = conn.BeginTransaction();

                    Update(transaction, conn, oServicecenters);
                    transaction.Commit();
                    conn.Close();
                }
            }
            catch (Exception exp)
            {
                WebAPI.Common.Logger.Instance(Context.OrganizationID).Log(exp, WebAPI.Common.Logger.LogType.Error, oServicecenters);
                throw exp;
            }

            return oServicecenters;
        }

        public Servicecenters Update(NpgsqlTransaction transaction, NpgsqlConnection conn, Servicecenters oServicecenters)
        {

            NpgsqlCommand command = new NpgsqlCommand(SQL_Update, conn, transaction);
            command.Parameters.Add("modifiedon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = oServicecenters.modifiedon;
            command.Parameters.Add("createdby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oServicecenters.createdby;
            command.Parameters.Add("modifiedby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oServicecenters.modifiedby;
            command.Parameters.Add("isactive", NpgsqlTypes.NpgsqlDbType.Boolean).Value = oServicecenters.isactive;
            command.Parameters.Add("id", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oServicecenters.id;
            command.Parameters.Add("name", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oServicecenters.name == null ? "" : oServicecenters.name;
            command.Parameters.Add("address", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oServicecenters.address == null ? "" : oServicecenters.address;
            command.Parameters.Add("city", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oServicecenters.city == null ? "" : oServicecenters.city;
            command.Parameters.Add("contactno", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oServicecenters.contactno == null ? "" : oServicecenters.contactno;
            command.Parameters.Add("contactperson", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oServicecenters.contactperson == null ? "" : oServicecenters.contactperson;
            command.Parameters.Add("emailid", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oServicecenters.emailid == null ? "" : oServicecenters.emailid;
            command.Parameters.Add("workinghours", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oServicecenters.workinghours == null ? "" : oServicecenters.workinghours;
            command.Parameters.Add("createdon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = oServicecenters.createdon;
            command.Parameters.Add("version", NpgsqlTypes.NpgsqlDbType.Integer).Value = oServicecenters.version;
            command.Parameters.Add("brandid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oServicecenters.brandid;
            command.Parameters.Add("productid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oServicecenters.productid;
            command.Parameters.Add("locations", NpgsqlTypes.NpgsqlDbType.Json).Value = oServicecenters.locations_json == null ? "null" : oServicecenters.locations_json;
            using (System.Data.IDataReader reader = command.ExecuteReader())
            {
                if (reader.Read())
                {
                    oServicecenters.version = Convert.ToInt32(reader[0].ToString());
                }
            }

            return oServicecenters;
        }


        public bool Delete(Servicecenters oServicecenters)
        {

            NpgsqlTransaction transaction = null;
            bool hasSuccessCount = false;
            try
            {
                using (NpgsqlConnection conn = CreateConnection())
                {
                    conn.Open();
                    transaction = conn.BeginTransaction();
                    hasSuccessCount = Delete(transaction, conn, oServicecenters);
                    transaction.Commit();
                    conn.Close();
                }
            }
            catch (Exception exp)
            {
                WebAPI.Common.Logger.Instance(Context.OrganizationID).Log(exp, WebAPI.Common.Logger.LogType.Error, oServicecenters);

                throw exp;
            }
            return hasSuccessCount;

        }

        public bool Delete(NpgsqlTransaction transaction, NpgsqlConnection conn, Servicecenters oServicecenters)
        {

            NpgsqlCommand command = new NpgsqlCommand(SQL_Delete, conn, transaction);
            command.Parameters.Add("isactive", NpgsqlTypes.NpgsqlDbType.Boolean).Value = oServicecenters.isactive;
            command.Parameters.Add("modifiedby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oServicecenters.modifiedby;
            command.Parameters.Add("modifiedon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = oServicecenters.modifiedon;
            command.Parameters.Add("version", NpgsqlTypes.NpgsqlDbType.Integer).Value = oServicecenters.version;
            command.Parameters.Add("id", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oServicecenters.id;
            int successCount = command.ExecuteNonQuery();

            return successCount > 0;
        }

        internal void UpdateServiceCenter(Bill source)
        {
            string barcode = source.Billitems.OrderBy(e => e.rate).Last().barcode;
            string query = @"select stock.itemid,items.brandid,items.productid,servicecenters.* from stock
                            inner join items on items.id = itemid
                            inner join servicecenters on (case when servicecenters.productid = 0 then servicecenters.productid else items.productid end) = items.productid
                            and (case when servicecenters.brandid = 0 then servicecenters.productid else items.brandid end) = items.brandid
                            where servicecenters.isactive = true and barcode = '" + WebAPI.Data.Utility.SafeSQL(barcode) + "'";

            source.servicecenter = new Servicecenters();
            List<Servicecenters> lst = new List<Servicecenters>();
            NpgsqlConnection conn = CreateConnection();
            try
            {
                conn.Open();
                QueryBuilder qb = new QueryBuilder();
                qb.CurrentQuery = SQL_Search.Substring(0, SQL_Search.IndexOf("where"));
                var command = qb.GetCommand(conn);

                using (System.Data.IDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        Servicecenters oservicecenters = new Servicecenters();
                        oservicecenters.modifiedon = reader["modifiedon"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(reader["modifiedon"]);
                        oservicecenters.createdby = reader["createdby"] == DBNull.Value ? 0 : Convert.ToInt64(reader["createdby"]);
                        oservicecenters.modifiedby = reader["modifiedby"] == DBNull.Value ? 0 : Convert.ToInt64(reader["modifiedby"]);
                        oservicecenters.isactive = reader["isactive"] == DBNull.Value ? false : Convert.ToBoolean(reader["isactive"]);
                        oservicecenters.id = reader["id"] == DBNull.Value ? 0 : Convert.ToInt64(reader["id"]);
                        oservicecenters.name = reader["name"] == DBNull.Value ? "" : reader["name"].ToString();
                        oservicecenters.address = reader["address"] == DBNull.Value ? "" : reader["address"].ToString();
                        oservicecenters.city = reader["city"] == DBNull.Value ? "" : reader["city"].ToString();
                        oservicecenters.contactno = reader["contactno"] == DBNull.Value ? "" : reader["contactno"].ToString();
                        oservicecenters.contactperson = reader["contactperson"] == DBNull.Value ? "" : reader["contactperson"].ToString();
                        oservicecenters.emailid = reader["emailid"] == DBNull.Value ? "" : reader["emailid"].ToString();
                        oservicecenters.workinghours = reader["workinghours"] == DBNull.Value ? "" : reader["workinghours"].ToString();
                        oservicecenters.createdon = reader["createdon"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(reader["createdon"]);
                        oservicecenters.version = reader["version"] == DBNull.Value ? 0 : Convert.ToInt32(reader["version"]);
                        oservicecenters.brandid = reader["brandid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["brandid"]);
                        oservicecenters.productid = reader["productid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["productid"]);
                        oservicecenters.locations_json = reader["locations"] == DBNull.Value ? null : reader["locations"].ToString();
                        if(oservicecenters.locations != null)
                        {
                            if(oservicecenters.locations.Exists(e=>e.id == source.locationid))
                            {
                                source.servicecenter = oservicecenters;
                            }
                        }
                    }
                }
                conn.Close();
            }
            catch (Exception exp)
            {
                if (conn != null) conn.Close();
            }
        }
    }

}
