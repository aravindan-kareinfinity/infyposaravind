using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using InfyPOS.Models;
using InfyPOS.Common;
using Npgsql;
namespace InfyPOS.Data

{
    public class HolidayDataService : SAASData
    {
        const string SQL_Search = @"select id, name, description, holidayon, holidaytype, isactive, createdby, modifiedby, version, createdon, modifiedon, payable,leaveencashment from holiday where id=@id";


        const string SQL_Insert = @"INSERT INTO public.holiday(name, description, holidayon, holidaytype, isactive, createdby, modifiedby, version, createdon, modifiedon, payable,leaveencashment) 
        VALUES(@name, @description, @holidayon, @holidaytype, @isactive, @createdby, @modifiedby, @version, @createdon, @modifiedon, @payable,@leaveencashment) RETURNING id";


        const string SQL_Update = @"UPDATE public.holiday SET name=@name, description=@description, holidayon=@holidayon, holidaytype=@holidaytype, 
        modifiedby=@modifiedby, version=@version+1, modifiedon=@modifiedon, payable=@payable , leaveencashment=@leaveencashment
        where id=@id and version=@version RETURNING version";

        const string SQL_Delete = @"update public.holiday set isactive=false,version=@version+1,modifiedby=@modifiedby,modifiedon=@modifiedon where id=@id and version=@version";

        public List<Holiday> Search(HolidayCreteria _holiday)
        {
            List<Holiday> lst = new List<Holiday>();
            NpgsqlConnection conn = CreateConnection();
            try
            {
                conn.Open();

                NpgsqlCommand command = null;
                if (_holiday.id != 0)
                {
                    command = GetCommand(SQL_Search, conn);
                    command.Parameters.Add("id", NpgsqlTypes.NpgsqlDbType.Bigint).Value = _holiday.id;
                }
                else
                {
                    QueryBuilder qb = new QueryBuilder();
                    qb.CurrentQuery = SQL_Search.Substring(0, SQL_Search.IndexOf("where"));
                    qb.AddParameter("isactive", true, "=");

                    if (_holiday.ids != null && _holiday.ids.Count > 0)
                    {
                        qb.AddInParamer("holiday.id", string.Join<long>(",", _holiday.ids));
                    }

                    if (!string.IsNullOrEmpty(_holiday.holidaytype))
                    {
                        qb.AddParameter("holidaytype", _holiday.holidaytype, "=");
                    }

                    if (_holiday.fromdate.Year > 2000)
                        qb.AddParameter("holidayon", "fromdate", _holiday.fromdate, ">=");

                    if (_holiday.todate.Year > 2000)
                        qb.AddParameter("holidayon", "todate", _holiday.todate.AddDays(1), "<");

                    command = qb.GetCommand(conn);
                }

                using (System.Data.IDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        Holiday oholiday = new Holiday();
                        oholiday.id = reader["id"] == DBNull.Value ? 0 : Convert.ToInt64(reader["id"]);

                        oholiday.name = reader["name"] == DBNull.Value ? "" : reader["name"].ToString();
                        oholiday.description = reader["description"] == DBNull.Value ? "" : reader["description"].ToString();
                        oholiday.holidayon = reader["holidayon"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(reader["holidayon"]);
                        oholiday.holidaytype = reader["holidaytype"] == DBNull.Value ? "" : reader["holidaytype"].ToString();
                        oholiday.isactive = reader["isactive"] == DBNull.Value ? false : Convert.ToBoolean(reader["isactive"]);
                        oholiday.createdby = reader["createdby"] == DBNull.Value ? 0 : Convert.ToInt64(reader["createdby"]);
                        oholiday.modifiedby = reader["modifiedby"] == DBNull.Value ? 0 : Convert.ToInt64(reader["modifiedby"]);
                        oholiday.version = reader["version"] == DBNull.Value ? 0 : Convert.ToInt32(reader["version"]);
                        oholiday.leaveencashment = reader["leaveencashment"] == DBNull.Value ? 0 : Convert.ToInt32(reader["leaveencashment"]);
                        oholiday.createdon = reader["createdon"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(reader["createdon"]);
                        oholiday.modifiedon = reader["modifiedon"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(reader["modifiedon"]);
                        oholiday.payable = reader["payable"] == DBNull.Value ? false : Convert.ToBoolean(reader["payable"]);

                        lst.Add(oholiday);
                    }
                }
                conn.Close();
            }
            catch (Exception exp)
            {
                if (conn != null) conn.Close();
                WebAPI.Common.Logger.Instance(Context.OrganizationID).Log(exp, WebAPI.Common.Logger.LogType.Error, _holiday);
            }
            return lst;
        }
        public class Days : BaseEntity
        {
            public DateTime fromdate { get; set; }
            public DateTime todate { get; set; }
            public long institutionid { get; set; }
        }

        public Holiday Insert(Holiday oHoliday)
        {

            NpgsqlTransaction transaction = null;
            try
            {
                using (NpgsqlConnection conn = CreateConnection())
                {
                    conn.Open();
                    transaction = conn.BeginTransaction();
                    NpgsqlCommand command = GetCommand(SQL_Insert, conn, transaction);

                    Insert(transaction, conn, oHoliday);
                    transaction.Commit();
                    conn.Close();
                }
            }
            catch (Exception exp)
            {
                WebAPI.Common.Logger.Instance(Context.OrganizationID).Log(exp, WebAPI.Common.Logger.LogType.Error, oHoliday);

            }

            return oHoliday;
        }

        public Holiday Insert(NpgsqlTransaction transaction, NpgsqlConnection conn, Holiday oHoliday)
        {

            NpgsqlCommand command = GetCommand(SQL_Insert, conn, transaction);
            command.Parameters.Add("name", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oHoliday.name == null ? "" : oHoliday.name;
            command.Parameters.Add("description", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oHoliday.description == null ? "" : oHoliday.description;
            command.Parameters.Add("holidayon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = oHoliday.holidayon;
            command.Parameters.Add("holidaytype", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oHoliday.holidaytype == null ? "" : oHoliday.holidaytype;
            command.Parameters.Add("isactive", NpgsqlTypes.NpgsqlDbType.Boolean).Value = oHoliday.isactive;
            command.Parameters.Add("createdby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = Context.UserID;
            command.Parameters.Add("modifiedby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = Context.UserID;
            command.Parameters.Add("version", NpgsqlTypes.NpgsqlDbType.Integer).Value = oHoliday.version;
            command.Parameters.Add("leaveencashment", NpgsqlTypes.NpgsqlDbType.Integer).Value = oHoliday.leaveencashment;
            command.Parameters.Add("createdon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = DateTimeNow;
            command.Parameters.Add("modifiedon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = DateTimeNow;
            command.Parameters.Add("payable", NpgsqlTypes.NpgsqlDbType.Boolean).Value = oHoliday.payable;
            using (System.Data.IDataReader reader = command.ExecuteReader())
            {
                if (reader.Read())
                {
                    oHoliday.id = Convert.ToInt64(reader[0].ToString());
                }
            }
            return oHoliday;
        }

        public Holiday Update(Holiday oHoliday)
        {

            NpgsqlTransaction transaction = null;
            try
            {
                using (NpgsqlConnection conn = CreateConnection())
                {
                    conn.Open();
                    transaction = conn.BeginTransaction();

                    Update(transaction, conn, oHoliday);
                    transaction.Commit();
                    conn.Close();
                }
            }
            catch (Exception exp)
            {
                WebAPI.Common.Logger.Instance(Context.OrganizationID).Log(exp, WebAPI.Common.Logger.LogType.Error, oHoliday);

            }

            return oHoliday;
        }

        public Holiday Update(NpgsqlTransaction transaction, NpgsqlConnection conn, Holiday oHoliday)
        {

            NpgsqlCommand command = GetCommand(SQL_Update, conn, transaction);
            command.Parameters.Add("id", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oHoliday.id;
            command.Parameters.Add("name", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oHoliday.name == null ? "" : oHoliday.name;
            command.Parameters.Add("description", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oHoliday.description == null ? "" : oHoliday.description;
            command.Parameters.Add("holidayon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = oHoliday.holidayon;
            command.Parameters.Add("holidaytype", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oHoliday.holidaytype == null ? "" : oHoliday.holidaytype;
            command.Parameters.Add("isactive", NpgsqlTypes.NpgsqlDbType.Boolean).Value = oHoliday.isactive;
            command.Parameters.Add("createdby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oHoliday.createdby;
            command.Parameters.Add("modifiedby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = Context.UserID;
            command.Parameters.Add("leaveencashment", NpgsqlTypes.NpgsqlDbType.Integer).Value = oHoliday.leaveencashment;
            command.Parameters.Add("version", NpgsqlTypes.NpgsqlDbType.Integer).Value = oHoliday.version;
            command.Parameters.Add("createdon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = oHoliday.createdon;
            command.Parameters.Add("modifiedon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = DateTimeNow;
            command.Parameters.Add("payable", NpgsqlTypes.NpgsqlDbType.Boolean).Value = oHoliday.payable;
            using (System.Data.IDataReader reader = command.ExecuteReader())
            {
                if (reader.Read())
                {
                    oHoliday.version = Convert.ToInt32(reader[0].ToString());
                }
            }

            return oHoliday;
        }

        public int Clone(HolidayCreteria oHoliday)
        {
            
            var list = Search(oHoliday);
            var existing = Search(new HolidayCreteria()
            {
                fromdate = new DateTime(oHoliday.year, 1, 1),
                todate = new DateTime(oHoliday.year, 12, 31),
            });

            NpgsqlTransaction transaction = null;
            try
            {
                var newList = new List<Holiday>();
                foreach(var item in list)
                {
                    var h = Newtonsoft.Json.JsonConvert.DeserializeObject<Holiday>(Newtonsoft.Json.JsonConvert.SerializeObject(item));
                    h.holidayon = new DateTime(oHoliday.year, h.holidayon.Month, h.holidayon.Day);
                    if(!existing.Exists(e=>e.holidayon == h.holidayon && e.holidaytype == h.holidaytype ))
                        newList.Add(h);
                }
                using (NpgsqlConnection conn = CreateConnection())
                {
                    conn.Open();
                    transaction = conn.BeginTransaction();
                    foreach(var item in newList)
                        Insert(transaction, conn, item);
                    transaction.Commit();
                    conn.Close();
                }
                return newList.Count;
            }
            catch (Exception exp)
            {
                WebAPI.Common.Logger.Instance(Context.OrganizationID).Log(exp, WebAPI.Common.Logger.LogType.Error, oHoliday);

            }
            return 0;
        }

        public bool Delete(Holiday oHoliday)
        {

            NpgsqlTransaction transaction = null;
            bool hasSuccessCount = false;
            try
            {
                using (NpgsqlConnection conn = CreateConnection())
                {
                    conn.Open();
                    transaction = conn.BeginTransaction();
                    hasSuccessCount = Delete(transaction, conn, oHoliday);
                    transaction.Commit();
                    conn.Close();
                }
            }
            catch (Exception exp)
            {
                WebAPI.Common.Logger.Instance(Context.OrganizationID).Log(exp, WebAPI.Common.Logger.LogType.Error, oHoliday);


            }
            return hasSuccessCount;

        }

        public bool Delete(NpgsqlTransaction transaction, NpgsqlConnection conn, Holiday oHoliday)
        {

            NpgsqlCommand command = GetCommand(SQL_Delete, conn, transaction);
            command.Parameters.Add("isactive", NpgsqlTypes.NpgsqlDbType.Boolean).Value = oHoliday.isactive;
            command.Parameters.Add("modifiedby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = Context.UserID;
            command.Parameters.Add("modifiedon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = DateTimeNow;
            command.Parameters.Add("version", NpgsqlTypes.NpgsqlDbType.Integer).Value = oHoliday.version;
            command.Parameters.Add("id", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oHoliday.id;
            int successCount = command.ExecuteNonQuery();

            return successCount > 0;
        }

    }

}
