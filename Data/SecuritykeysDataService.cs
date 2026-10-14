using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using InfyPOS.Models;
using InfyPOS.Common;
using Npgsql;
using System.Security.Cryptography;

namespace InfyPOS.Data

{
    public class SecuritykeysDataService : SAASData
    {
        const string SQL_Search = @"select id, locationkey, privatekey, publickey, ipaddress, computername,systemrole, isactive, disable, modifiedon, createdon from securitykeys where id=@id";

        const string SQL_Insert = @"INSERT INTO public.securitykeys(locationkey, privatekey, publickey, ipaddress, computername,systemrole, isactive, disable, modifiedon, createdon) VALUES(@locationkey, @privatekey, @publickey, @ipaddress, 
        @computername,@systemrole, @isactive, @disable, @modifiedon, @createdon) RETURNING id";

        const string SQL_Update = @"UPDATE public.securitykeys SET locationkey=@locationkey, privatekey=@privatekey, publickey=@publickey, ipaddress=@ipaddress, 
        computername=@computername,systemrole=@systemrole, disable=@disable, modifiedon=@modifiedon where id=@id";

        const string SQL_Delete = @"update public.securitykeys set isactive=false,modifiedon=@modifiedon where id=@id";

        public List<Securitykeys> Search(SecuritykeysCreteria _securitykeys)
        {
            List<Securitykeys> lst = new List<Securitykeys>();
            NpgsqlConnection conn = CreateConnection();
            try
            {
                conn.Open();

                NpgsqlCommand command = null;
                if (_securitykeys.id != 0)
                {
                    command = GetCommand(SQL_Search, conn);
                    command.Parameters.Add("id", NpgsqlTypes.NpgsqlDbType.Bigint).Value = _securitykeys.id;
                }
                else
                {
                    QueryBuilder qb = new QueryBuilder();
                    qb.CurrentQuery = SQL_Search.Substring(0, SQL_Search.IndexOf("where"));
                    qb.AddParameter("isactive", true, "=");

                    if (!string.IsNullOrEmpty(_securitykeys.locationkey))
                        qb.AddParameter("locationkey", _securitykeys.locationkey, "=");

                    command = qb.GetCommand(conn);
                }

                using (System.Data.IDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        Securitykeys osecuritykeys = new Securitykeys();
                        osecuritykeys.id = reader["id"] == DBNull.Value ? 0 : Convert.ToInt64(reader["id"]);
                        osecuritykeys.locationkey = reader["locationkey"] == DBNull.Value ? "" : reader["locationkey"].ToString();
                        osecuritykeys.privatekey = reader["privatekey"] == DBNull.Value ? "" : reader["privatekey"].ToString();
                        osecuritykeys.publickey = reader["publickey"] == DBNull.Value ? "" : reader["publickey"].ToString();
                        osecuritykeys.ipaddress = reader["ipaddress"] == DBNull.Value ? "" : reader["ipaddress"].ToString();
                        osecuritykeys.computername = reader["computername"] == DBNull.Value ? "" : reader["computername"].ToString();
                        osecuritykeys.systemrole = reader["systemrole"] == DBNull.Value ? "" : reader["systemrole"].ToString();
                        osecuritykeys.isactive = reader["isactive"] == DBNull.Value ? false : Convert.ToBoolean(reader["isactive"]);
                        osecuritykeys.disable = reader["disable"] == DBNull.Value ? false : Convert.ToBoolean(reader["disable"]);
                        osecuritykeys.modifiedon = reader["modifiedon"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(reader["modifiedon"]);
                        osecuritykeys.createdon = reader["createdon"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(reader["createdon"]);
                        lst.Add(osecuritykeys);
                    }
                }
                conn.Close();
            }
            catch (Exception exp)
            {
                if (conn != null) conn.Close();
                WebAPI.Common.Logger.Instance(Context.OrganizationID).Log(exp, WebAPI.Common.Logger.LogType.Error, _securitykeys);
            }
            return lst;
        }

        public Securitykeys Insert(Securitykeys oSecuritykeys)
        {

            NpgsqlTransaction transaction = null;
            try
            {
                using (NpgsqlConnection conn = CreateConnection())
                {
                    conn.Open();
                    transaction = conn.BeginTransaction();
                    NpgsqlCommand command = GetCommand(SQL_Insert, conn, transaction);

                    Insert(transaction, conn, oSecuritykeys);
                    transaction.Commit();
                    conn.Close();
                }
            }
            catch (Exception exp)
            {
                WebAPI.Common.Logger.Instance(Context.OrganizationID).Log(exp, WebAPI.Common.Logger.LogType.Error, oSecuritykeys);
                throw exp;
            }

            return oSecuritykeys;
        }

        public Securitykeys Insert(NpgsqlTransaction transaction, NpgsqlConnection conn, Securitykeys oSecuritykeys)
        {

            NpgsqlCommand command = GetCommand(SQL_Insert, conn, transaction);
            command.Parameters.Add("locationkey", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oSecuritykeys.locationkey == null ? "" : oSecuritykeys.locationkey;
            command.Parameters.Add("privatekey", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oSecuritykeys.privatekey == null ? "" : oSecuritykeys.privatekey;
            command.Parameters.Add("publickey", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oSecuritykeys.publickey == null ? "" : oSecuritykeys.publickey;
            command.Parameters.Add("ipaddress", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oSecuritykeys.ipaddress == null ? "" : oSecuritykeys.ipaddress;
            command.Parameters.Add("computername", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oSecuritykeys.computername == null ? "" : oSecuritykeys.computername;
            command.Parameters.Add("systemrole", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oSecuritykeys.systemrole == null ? "" : oSecuritykeys.systemrole;
            command.Parameters.Add("isactive", NpgsqlTypes.NpgsqlDbType.Boolean).Value = oSecuritykeys.isactive;
            command.Parameters.Add("disable", NpgsqlTypes.NpgsqlDbType.Boolean).Value = oSecuritykeys.disable;
            command.Parameters.Add("modifiedon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = oSecuritykeys.modifiedon;
            command.Parameters.Add("createdon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = oSecuritykeys.createdon;
            using (System.Data.IDataReader reader = command.ExecuteReader())
            {
                if (reader.Read())
                {
                    oSecuritykeys.id = Convert.ToInt64(reader[0].ToString());
                }
            }
            return oSecuritykeys;
        }

        public Securitykeys Update(Securitykeys oSecuritykeys)
        {

            NpgsqlTransaction transaction = null;
            try
            {
                using (NpgsqlConnection conn = CreateConnection())
                {
                    conn.Open();
                    transaction = conn.BeginTransaction();

                    Update(transaction, conn, oSecuritykeys);
                    transaction.Commit();
                    conn.Close();
                }
            }
            catch (Exception exp)
            {
                WebAPI.Common.Logger.Instance(Context.OrganizationID).Log(exp, WebAPI.Common.Logger.LogType.Error, oSecuritykeys);
                throw exp;
            }

            return oSecuritykeys;
        }

        public Securitykeys Update(NpgsqlTransaction transaction, NpgsqlConnection conn, Securitykeys oSecuritykeys)
        {
            NpgsqlCommand command = GetCommand(SQL_Update, conn, transaction);
            command.Parameters.Add("id", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oSecuritykeys.id;
            command.Parameters.Add("locationkey", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oSecuritykeys.locationkey == null ? "" : oSecuritykeys.locationkey;
            command.Parameters.Add("privatekey", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oSecuritykeys.privatekey == null ? "" : oSecuritykeys.privatekey;
            command.Parameters.Add("publickey", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oSecuritykeys.publickey == null ? "" : oSecuritykeys.publickey;
            command.Parameters.Add("ipaddress", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oSecuritykeys.ipaddress == null ? "" : oSecuritykeys.ipaddress;
            command.Parameters.Add("computername", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oSecuritykeys.computername == null ? "" : oSecuritykeys.computername;
            command.Parameters.Add("systemrole", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oSecuritykeys.systemrole == null ? "" : oSecuritykeys.systemrole;
            command.Parameters.Add("isactive", NpgsqlTypes.NpgsqlDbType.Boolean).Value = oSecuritykeys.isactive;
            command.Parameters.Add("disable", NpgsqlTypes.NpgsqlDbType.Boolean).Value = oSecuritykeys.disable;
            command.Parameters.Add("modifiedon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = oSecuritykeys.modifiedon;
            command.Parameters.Add("createdon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = oSecuritykeys.createdon;
            return command.ExecuteNonQuery() == 1 ? oSecuritykeys : null;
        }


        public bool Delete(Securitykeys oSecuritykeys)
        {

            NpgsqlTransaction transaction = null;
            bool hasSuccessCount = false;
            try
            {
                using (NpgsqlConnection conn = CreateConnection())
                {
                    conn.Open();
                    transaction = conn.BeginTransaction();
                    hasSuccessCount = Delete(transaction, conn, oSecuritykeys);
                    transaction.Commit();
                    conn.Close();
                }
            }
            catch (Exception exp)
            {
                WebAPI.Common.Logger.Instance(Context.OrganizationID).Log(exp, WebAPI.Common.Logger.LogType.Error, oSecuritykeys);

                throw exp;
            }
            return hasSuccessCount;

        }

        public bool Delete(NpgsqlTransaction transaction, NpgsqlConnection conn, Securitykeys oSecuritykeys)
        {
            NpgsqlCommand command = GetCommand(SQL_Delete, conn, transaction);
            command.Parameters.Add("isactive", NpgsqlTypes.NpgsqlDbType.Boolean).Value = oSecuritykeys.isactive;
            command.Parameters.Add("id", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oSecuritykeys.id;
            int successCount = command.ExecuteNonQuery();
            return successCount > 0;
        }

        internal bool ValidateLocation(UsersCreteria users,Users loggedinuser)
        {
            if (users.locationinfo == null) return false;
            var licensekey = LicenseManager.License.CreateKey(users.locationinfo.sourcekey);
            SecuritykeysDataService securitykeysDataService = new SecuritykeysDataService() { Context = this.Context };
            var securityinfo = securitykeysDataService.Search(new SecuritykeysCreteria()
            {
                locationkey = licensekey
            });
            if (securityinfo.Count == 0) return false;

            loggedinuser.systemrole = securityinfo[0].systemrole;

            var rsa = new RSACryptoServiceProvider();
            rsa.FromXmlString(securityinfo[0].privatekey);
            string decryptedData = "";
            if (users.security != null && users.security.iprestriction)
            {
                if (!string.IsNullOrEmpty(users.locationinfo.publicip))
                {
                    decryptedData = System.Text.ASCIIEncoding.ASCII.GetString(rsa.Decrypt(Convert.FromBase64String(users.locationinfo.publicip), false));
                    return Services.CacheManager.Instance(Context.OrganizationID).StaticIPList.Contains(decryptedData);
                }
            }

            decryptedData = System.Text.ASCIIEncoding.ASCII.GetString(rsa.Decrypt(Convert.FromBase64String(users.locationinfo.licensekey), false));
            if (licensekey == decryptedData)
            {
                return true;
            }

            if (users.locationinfo != null && !string.IsNullOrEmpty(users.locationinfo.computername))
            {
                decryptedData = System.Text.ASCIIEncoding.ASCII.GetString(rsa.Decrypt(Convert.FromBase64String(users.locationinfo.computername), false));
                if (securityinfo[0].computername == decryptedData)
                {
                    return true;
                }
            }

            
            return false;
        }
    }

}
