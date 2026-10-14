using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using InfyPOS.Models;
using InfyPOS.Common;
using Npgsql;
namespace InfyPOS.Data

{
    public class ScheduleditemsDataService : SAASData
    {
        const string SQL_Search = @"select scheduleid, hour, lastrun, id, validtill from scheduleditems where id=@id";
        const string SQL_SearchAll = @"select scheduleid, hour, lastrun, id, validtill from scheduleditems where scheduleid=@scheduleid";

        const string SQL_SearchAllReportDays = @"select scheduleid, hour, lastrun, scheduleditems.id, validtill from scheduleditems 
                                                 inner join schedule on schedule.id = scheduleid
                                                 where schedule.isactive = true and schedule.isactionbased=false";

        const string SQL_Insert = @"INSERT INTO public.scheduleditems(scheduleid, hour, lastrun, validtill) VALUES(@scheduleid, @hour, @lastrun, @validtill) RETURNING id";


        const string SQL_Update = @"UPDATE public.scheduleditems SET scheduleid=@scheduleid, hour=@hour, lastrun=@lastrun, validtill=@validtill where id=@id and version=@version RETURNING version";

        const string SQL_Delete = @"delete from public.scheduleditems where id=@id";
        const string SQL_DeleteAll = @"delete from public.scheduleditems where scheduleid=@scheduleid";
        public List<Scheduleditems> Search(ScheduleditemsCreteria _scheduleditems)
        {
            List<Scheduleditems> lst = new List<Scheduleditems>();
            NpgsqlConnection conn = CreateConnection();
            try
            {
                conn.Open();

                NpgsqlCommand command = null;

                command = new NpgsqlCommand(SQL_Search, conn);
                command.Parameters.Add("id", NpgsqlTypes.NpgsqlDbType.Bigint).Value = _scheduleditems.id;

                using (System.Data.IDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        Scheduleditems oscheduleditems = new Scheduleditems();
                        oscheduleditems.scheduleid = reader["scheduleid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["scheduleid"]);
                        oscheduleditems.hour = reader["hour"] == DBNull.Value ? 0 : Convert.ToInt32(reader["hour"]);
                        oscheduleditems.lastrun = reader["lastrun"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(reader["lastrun"]);
                        oscheduleditems.validtill = reader["validtill"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(reader["validtill"]);
                        oscheduleditems.id = reader["id"] == DBNull.Value ? 0 : Convert.ToInt64(reader["id"]);
                        lst.Add(oscheduleditems);
                    }
                }
                conn.Close();
            }
            catch (Exception exp)
            {
                if (conn != null) conn.Close();
                WebAPI.Common.Logger.Instance(Context.OrganizationID).Log(exp, WebAPI.Common.Logger.LogType.Error, _scheduleditems);
            }
            return lst;
        }

        public List<Scheduleditems> SearchAll(long _scheduleid)
        {
            List<Scheduleditems> lst = new List<Scheduleditems>();
            NpgsqlConnection conn = CreateConnection();
            try
            {
                conn.Open();

                NpgsqlCommand command = new NpgsqlCommand(_scheduleid > 0 ? SQL_SearchAll : SQL_SearchAllReportDays, conn);
                command.Parameters.Add("scheduleid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = _scheduleid;
                using (System.Data.IDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        Scheduleditems oscheduleditems = new Scheduleditems();
                        oscheduleditems.scheduleid = reader["scheduleid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["scheduleid"]);
                        oscheduleditems.hour = reader["hour"] == DBNull.Value ? 0 : Convert.ToInt32(reader["hour"]);
                        oscheduleditems.lastrun = reader["lastrun"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(reader["lastrun"]);
                        oscheduleditems.validtill = reader["validtill"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(reader["validtill"]);
                        oscheduleditems.id = reader["id"] == DBNull.Value ? 0 : Convert.ToInt64(reader["id"]);
                        lst.Add(oscheduleditems);
                    }
                }
                conn.Close();
            }
            catch (Exception exp)
            {
                if (conn != null) conn.Close();
                WebAPI.Common.Logger.Instance(Context.OrganizationID).Log(exp, WebAPI.Common.Logger.LogType.Error, _scheduleid);
            }
            return lst;
        }

        public Scheduleditems Insert(Scheduleditems oScheduleditems)
        {

            NpgsqlTransaction transaction = null;
            try
            {
                using (NpgsqlConnection conn = CreateConnection())
                {
                    conn.Open();
                    transaction = conn.BeginTransaction();
                    NpgsqlCommand command = new NpgsqlCommand(SQL_Insert, conn, transaction);

                    Insert(transaction, conn, oScheduleditems);
                    transaction.Commit();
                    conn.Close();
                }
            }
            catch (Exception exp)
            {
                WebAPI.Common.Logger.Instance(Context.OrganizationID).Log(exp, WebAPI.Common.Logger.LogType.Error, oScheduleditems);
                throw exp;
            }

            return oScheduleditems;
        }

        public Scheduleditems Insert(NpgsqlTransaction transaction, NpgsqlConnection conn, Scheduleditems oScheduleditems)
        {

            NpgsqlCommand command = new NpgsqlCommand(SQL_Insert, conn, transaction);
            command.Parameters.Add("scheduleid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oScheduleditems.scheduleid;
            command.Parameters.Add("hour", NpgsqlTypes.NpgsqlDbType.Integer).Value = oScheduleditems.hour;
            command.Parameters.Add("lastrun", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = oScheduleditems.lastrun;
            command.Parameters.Add("validtill", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = oScheduleditems.validtill;
            using (System.Data.IDataReader reader = command.ExecuteReader())
            {
                if (reader.Read())
                {
                    oScheduleditems.id = Convert.ToInt64(reader[0].ToString());
                }
            }
            return oScheduleditems;
        }

        public Scheduleditems Update(Scheduleditems oScheduleditems)
        {

            NpgsqlTransaction transaction = null;
            try
            {
                using (NpgsqlConnection conn = CreateConnection())
                {
                    conn.Open();
                    transaction = conn.BeginTransaction();

                    Update(transaction, conn, oScheduleditems);
                    transaction.Commit();
                    conn.Close();
                }
            }
            catch (Exception exp)
            {
                WebAPI.Common.Logger.Instance(Context.OrganizationID).Log(exp, WebAPI.Common.Logger.LogType.Error, oScheduleditems);
                throw exp;
            }

            return oScheduleditems;
        }

        public Scheduleditems Update(NpgsqlTransaction transaction, NpgsqlConnection conn, Scheduleditems oScheduleditems)
        {

            NpgsqlCommand command = new NpgsqlCommand(SQL_Update, conn, transaction);
            command.Parameters.Add("scheduleid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oScheduleditems.scheduleid;
            command.Parameters.Add("hour", NpgsqlTypes.NpgsqlDbType.Integer).Value = oScheduleditems.hour;
            command.Parameters.Add("lastrun", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = oScheduleditems.lastrun;
            command.Parameters.Add("validtill", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = oScheduleditems.validtill;
            command.Parameters.Add("id", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oScheduleditems.id;
            command.ExecuteNonQuery();
            return oScheduleditems;
        }

        public bool Update(NpgsqlTransaction transaction, NpgsqlConnection conn, List<Scheduleditems> list, long id)
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
                if (item.id == 0)
                {
                    item.scheduleid = id;
                    Insert(transaction, conn, item);
                }
            }
            return true;
        }

        public bool Delete(Scheduleditems oScheduleditems)
        {

            NpgsqlTransaction transaction = null;
            bool hasSuccessCount = false;
            try
            {
                using (NpgsqlConnection conn = CreateConnection())
                {
                    conn.Open();
                    transaction = conn.BeginTransaction();
                    hasSuccessCount = Delete(transaction, conn, oScheduleditems);
                    transaction.Commit();
                    conn.Close();
                }
            }
            catch (Exception exp)
            {
                WebAPI.Common.Logger.Instance(Context.OrganizationID).Log(exp, WebAPI.Common.Logger.LogType.Error, oScheduleditems);

                throw exp;
            }
            return hasSuccessCount;

        }

        public bool Delete(NpgsqlTransaction transaction, NpgsqlConnection conn, Scheduleditems oScheduleditems)
        {

            NpgsqlCommand command = new NpgsqlCommand(SQL_Delete, conn, transaction);
            command.Parameters.Add("id", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oScheduleditems.id;
            int successCount = command.ExecuteNonQuery();

            return successCount > 0;
        }

        public bool DeleteAll(long scheduleid, long userid, NpgsqlConnection conn, NpgsqlTransaction transaction)
        {
            int successCount = 0;
            NpgsqlCommand command = new NpgsqlCommand(SQL_DeleteAll, conn, transaction);
            command.Parameters.Add("scheduleid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = scheduleid;
            successCount = command.ExecuteNonQuery();

            return successCount > 0;
        }

    }

}