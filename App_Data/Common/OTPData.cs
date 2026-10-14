using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using WebAPI.Data.Sql;
using WebAPI.Models;

namespace WebAPI.Data
{
    public class OTPData
    {

        const string SQL_Insert = @"INSERT INTO public.otp(userid,usertype,mobileno, securitytoken, createdon, expiredon,status)
                                    VALUES (@userid,@usertype,@mobileno,@securitytoken, @createdon, @expiredon,@status);";

        const string SQL_Find = @"Select userid,usertype,mobileno, securitytoken, createdon, expiredon 
                                from public.otp where userid=@userid and usertype=@usertype
                                order by createdon desc
                                LIMIT 1";

        const string SQL_FindTop = @"Select userid,usertype,mobileno, securitytoken, createdon, expiredon 
                                from public.otp 
                                order by createdon desc
                                LIMIT 100";

        const string SQL_UpdateStatus = @"UPDATE public.token SET status=@status where userid=@userid and usertype=@usertype and securitytoken=@securitytoken";

        public List<OTP> FindTop100()
        {
            List<OTP> lst = new List<OTP>();
            NpgsqlDatabase db = DatabaseFactory<NpgsqlDatabase>.CreateDatabase("InfyPOS");
            var dbCommand = db.GetSqlStringCommand(SQL_FindTop);

            using (System.Data.IDataReader reader = db.ExecuteReader(dbCommand))
            {
                while (reader.Read())
                {
                    OTP token = new OTP();
                    token.UserID = reader.GetInt64(0);
                    token.IsDoctor = reader.GetString(1) == "D";
                    token.MobileNo = reader.GetString(2);
                    token.Value = reader.GetString(3);
                    token.CreatedOn = reader.GetDateTime(4);
                    token.ExpiredOn = reader.GetDateTime(5);
                    lst.Add(token);
                }
            }
            return lst;
        }

        public OTP Find(long userid,bool isDoctor)
        {
            NpgsqlDatabase db = DatabaseFactory<NpgsqlDatabase>.CreateDatabase("InfyPOS");
            var dbCommand = db.GetSqlStringCommand(SQL_Find);
            db.AddInParameter(dbCommand, "usertype", NpgsqlTypes.NpgsqlDbType.Varchar, isDoctor?"D":"P");
            db.AddInParameter(dbCommand, "userid", NpgsqlTypes.NpgsqlDbType.Bigint, userid);
            using (System.Data.IDataReader reader = db.ExecuteReader(dbCommand))
            {
                if(reader.Read())
                {
                    OTP token = new OTP();
                    token.UserID = reader.GetInt64(0);
                    token.IsDoctor = reader.GetString(1)=="D";
                    token.MobileNo = reader.GetString(2);
                    token.Value = reader.GetString(3);
                    token.CreatedOn = reader.GetDateTime(4);
                    token.ExpiredOn = reader.GetDateTime(5);
                    return token;
                }
            }
            return new OTP();
        }

        public bool Remove(long userid,bool isDoctor,string securitytoken)
        {
            NpgsqlDatabase db = DatabaseFactory<NpgsqlDatabase>.CreateDatabase("InfyPOS");
            var dbCommand = db.GetSqlStringCommand(SQL_UpdateStatus);
            db.AddInParameter(dbCommand, "userid", NpgsqlTypes.NpgsqlDbType.Varchar, userid);
            db.AddInParameter(dbCommand, "securitytoken", NpgsqlTypes.NpgsqlDbType.Varchar, securitytoken);
            db.AddInParameter(dbCommand, "usertype", NpgsqlTypes.NpgsqlDbType.Char, isDoctor ? 'D' : 'P');
            db.AddInParameter(dbCommand, "status", NpgsqlTypes.NpgsqlDbType.Varchar, "D");
            return db.ExecuteNonQuery(dbCommand) > 0;
        }
        public bool Create(OTP token,bool IsDoctor)
        {
            try
            {
                NpgsqlDatabase db = DatabaseFactory<NpgsqlDatabase>.CreateDatabase("InfyPOS");
                var dbCommand = db.GetSqlStringCommand(SQL_Insert);
                //db.AddOutParameter(dbCommand, "mrnid", NpgsqlTypes.NpgsqlDbType.Bigint, 8);
                db.AddInParameter(dbCommand, "userid", NpgsqlTypes.NpgsqlDbType.Bigint, token.UserID);
                db.AddInParameter(dbCommand, "mobileno", NpgsqlTypes.NpgsqlDbType.Varchar, token.MobileNo);
                db.AddInParameter(dbCommand, "securitytoken", NpgsqlTypes.NpgsqlDbType.Varchar, token.Value);
                db.AddInParameter(dbCommand, "createdon", NpgsqlTypes.NpgsqlDbType.TimestampTZ, token.CreatedOn);
                db.AddInParameter(dbCommand, "status", NpgsqlTypes.NpgsqlDbType.Char, 'A');
                db.AddInParameter(dbCommand, "usertype", NpgsqlTypes.NpgsqlDbType.Char, IsDoctor? 'D' : 'P');
                db.AddInParameter(dbCommand, "expiredon", NpgsqlTypes.NpgsqlDbType.TimestampTZ, token.ExpiredOn);
                db.ExecuteNonQuery(dbCommand);
                return true;
            }
            catch (Exception exp)
            {
                return false;
            }
        }

    }
}