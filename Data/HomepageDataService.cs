using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using InfyPOS.Models;
using InfyPOS.Common;
using Npgsql;
namespace InfyPOS.Data

{
    public class HomepageDataService : SAASData
    {
        const string SQL_Search = @"select id, isactive, createdby, createdon, modifiedby, version, modifiedon, schoolname, mobilenumber, emailid, address, images,  aboutus, slides, facilities, domain,  logo, isselected,ctitle, ititle, ftitle, systemimages, institutionid from homepage  where id=@id";

        const string SQL_Insert = @"INSERT INTO public.homepage( isactive, createdby, createdon, modifiedby, version, modifiedon, schoolname, mobilenumber, emailid, address, images, aboutus, slides, facilities, domain, institutionid, logo, isselected, ctitle, ititle, ftitle , systemimages ) VALUES( @isactive, @createdby, @createdon, @modifiedby, @version, @modifiedon, @schoolname, @mobilenumber, @emailid, @address, @images, @aboutus, @slides, @facilities ,@domain , @institutionid, @logo, @isselected, @ctitle, @ititle, @ftitle ,@systemimages ) RETURNING id";

        const string SQL_Update = @"UPDATE public.homepage SET   isactive=@isactive, modifiedby=@modifiedby, version=@version+1, modifiedon=@modifiedon, schoolname=@schoolname, mobilenumber=@mobilenumber, emailid=@emailid, address=@address, images=@images, aboutus=@aboutus, slides=@slides, facilities=@facilities , domain=@domain, institutionid=@institutionid, logo=@logo, isselected=@isselected, ctitle=@ctitle, ititle=@ititle, ftitle=@ftitle,systemimages=@systemimages where id=@id and version=@version RETURNING version";

        const string SQL_Delete = @"update public.homepage set isactive=false,version=@version+1,modifiedby=@modifiedby,modifiedon=@modifiedon where id=@id and version=@version";

        public List<Homepage> Search(HomepageCreteria _homepage)
        {
            List<Homepage> lst = new List<Homepage>();
            NpgsqlConnection conn = CreateConnection();
            try
            {
                conn.Open();

                NpgsqlCommand command = null;
                if (_homepage.id != 0)
                {
                    command = GetCommand(SQL_Search, conn);
                    command.Parameters.Add("id", NpgsqlTypes.NpgsqlDbType.Bigint).Value = _homepage.id;
                }
                else
                {
                    QueryBuilder qb = new QueryBuilder();
                    qb.CurrentQuery = SQL_Search.Substring(0, SQL_Search.IndexOf("where"));
                    qb.AddParameter("isactive", true, "=");

                    if(!string.IsNullOrEmpty(_homepage.domain))
                    {
                        qb.AddParameter("domain", _homepage.domain, "=");
                    }
                    if (_homepage.institutionid > 0)
                        qb.AddParameter("institutionid", _homepage.institutionid, "=");
                    command = qb.GetCommand(conn);
                }

                using (System.Data.IDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        Homepage ohomepage = new Homepage();
                        ohomepage.id = reader["id"] == DBNull.Value ? 0 : Convert.ToInt64(reader["id"]);
                        ohomepage.isactive = reader["isactive"] == DBNull.Value ? false : Convert.ToBoolean(reader["isactive"]);
                        ohomepage.isselected = reader["isselected"] == DBNull.Value ? false : Convert.ToBoolean(reader["isselected"]);
                        ohomepage.modifiedon = reader["modifiedon"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(reader["modifiedon"]);
                        ohomepage.createdon = reader["createdon"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(reader["createdon"]);
                        ohomepage.createdby = reader["createdby"] == DBNull.Value ? 0 : Convert.ToInt64(reader["createdby"]);
                        ohomepage.modifiedby = reader["modifiedby"] == DBNull.Value ? 0 : Convert.ToInt64(reader["modifiedby"]);
                        ohomepage.version = reader["version"] == DBNull.Value ? 0 : Convert.ToInt32(reader["version"]);
                        ohomepage.schoolname = reader["schoolname"] == DBNull.Value ? "" : reader["schoolname"].ToString();
                        ohomepage.mobilenumber = reader["mobilenumber"] == DBNull.Value ? "" : reader["mobilenumber"].ToString();
                        ohomepage.emailid = reader["emailid"] == DBNull.Value ? "" : reader["emailid"].ToString();
                        ohomepage.address = reader["address"] == DBNull.Value ? "" : reader["address"].ToString();
                        ohomepage.images_json = reader["images"] == DBNull.Value ? "null" : reader["images"].ToString();
                        ohomepage.aboutus_json = reader["aboutus"] == DBNull.Value ? "null" : reader["aboutus"].ToString();
                        ohomepage.slides_json = reader["slides"] == DBNull.Value ? "null" : reader["slides"].ToString();
                        ohomepage.facilities_json = reader["facilities"] == DBNull.Value ? "null" : reader["facilities"].ToString();
                        ohomepage.domain = reader["domain"] == DBNull.Value ? "" : reader["domain"].ToString();
                        ohomepage.logo = reader["logo"] == DBNull.Value ? "null" : reader["logo"].ToString();
                        ohomepage.ctitle = reader["ctitle"] == DBNull.Value ? "" : reader["ctitle"].ToString();
                        ohomepage.ititle = reader["ititle"] == DBNull.Value ? "" : reader["ititle"].ToString();
                        ohomepage.ftitle = reader["ftitle"] == DBNull.Value ? "" : reader["ftitle"].ToString();
                        ohomepage.systemimages_json = reader["systemimages"] == DBNull.Value ? "null" : reader["systemimages"].ToString();
                        ohomepage.institutionid = reader["institutionid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["institutionid"]);
                        lst.Add(ohomepage);
                    }
                }
                conn.Close();
            }
            catch (Exception exp)
            {
                if (conn != null) conn.Close();
                WebAPI.Common.Logger.Instance(Context.OrganizationID).Log(exp, WebAPI.Common.Logger.LogType.Error, _homepage);
            }
            return lst;
        }

        public Homepage Insert(Homepage oHomepage)
        {

            NpgsqlTransaction transaction = null;
            try
            {
                using (NpgsqlConnection conn = CreateConnection())
                {
                    conn.Open();
                    transaction = conn.BeginTransaction();
                    NpgsqlCommand command = GetCommand(SQL_Insert, conn, transaction);

                    Insert(transaction, conn, oHomepage);
                    transaction.Commit();
                    conn.Close();
                }
            }
            catch (Exception exp)
            {
                WebAPI.Common.Logger.Instance(Context.OrganizationID).Log(exp, WebAPI.Common.Logger.LogType.Error, oHomepage);
               
            }

            return oHomepage;
        }

        public Homepage Insert(NpgsqlTransaction transaction, NpgsqlConnection conn, Homepage oHomepage)
        {

            NpgsqlCommand command = GetCommand(SQL_Insert, conn, transaction);
            command.Parameters.Add("isactive", NpgsqlTypes.NpgsqlDbType.Boolean).Value = oHomepage.isactive;
            command.Parameters.Add("isselected", NpgsqlTypes.NpgsqlDbType.Boolean).Value = oHomepage.isselected;
            command.Parameters.Add("modifiedon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = DateTimeNow;
            command.Parameters.Add("createdon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = DateTimeNow;
            command.Parameters.Add("createdby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = Context.UserID;
            command.Parameters.Add("modifiedby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = Context.UserID;
            command.Parameters.Add("version", NpgsqlTypes.NpgsqlDbType.Integer).Value = oHomepage.version;
            command.Parameters.Add("schoolname", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oHomepage.schoolname == null ? "" : oHomepage.schoolname;
            command.Parameters.Add("mobilenumber", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oHomepage.mobilenumber == null ? "" : oHomepage.mobilenumber;
            command.Parameters.Add("emailid", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oHomepage.emailid == null ? "" : oHomepage.emailid;
            command.Parameters.Add("address", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oHomepage.address == null ? "" : oHomepage.address;
            command.Parameters.Add("images", NpgsqlTypes.NpgsqlDbType.Json).Value = oHomepage.images == null ? "null" : oHomepage.images_json;
            command.Parameters.Add("aboutus", NpgsqlTypes.NpgsqlDbType.Json).Value = oHomepage.aboutus == null ? "null" : oHomepage.aboutus_json;
            command.Parameters.Add("slides", NpgsqlTypes.NpgsqlDbType.Json).Value = oHomepage.slides == null ? "null" : oHomepage.slides_json;
            command.Parameters.Add("facilities", NpgsqlTypes.NpgsqlDbType.Json).Value = oHomepage.facilities == null ? "null" : oHomepage.facilities_json;
            command.Parameters.Add("domain", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oHomepage.domain == null ? "" : oHomepage.domain;
            command.Parameters.Add("logo", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oHomepage.logo == null ? "null" : oHomepage.logo;
            command.Parameters.Add("ctitle", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oHomepage.ctitle == null ? "" : oHomepage.ctitle;
            command.Parameters.Add("ititle", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oHomepage.ititle == null ? "" : oHomepage.ititle;
            command.Parameters.Add("ftitle", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oHomepage.ftitle == null ? "" : oHomepage.ftitle;
            command.Parameters.Add("systemimages", NpgsqlTypes.NpgsqlDbType.Json).Value = oHomepage.systemimages == null ? "null" : oHomepage.systemimages_json;
            command.Parameters.Add("institutionid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = (oHomepage.institutionid);
            
            using (System.Data.IDataReader reader = command.ExecuteReader())
            {
                if (reader.Read())
                {
                    oHomepage.id = Convert.ToInt64(reader[0].ToString());
                }
            }
            return oHomepage;
        }

        public Homepage Update(Homepage oHomepage)
        {

            NpgsqlTransaction transaction = null;
            try
            {
                using (NpgsqlConnection conn = CreateConnection())
                {
                    conn.Open();
                    transaction = conn.BeginTransaction();

                    Update(transaction, conn, oHomepage);
                    transaction.Commit();
                    conn.Close();
                }
            }
            catch (Exception exp)
            {
                WebAPI.Common.Logger.Instance(Context.OrganizationID).Log(exp, WebAPI.Common.Logger.LogType.Error, oHomepage);
               
            }

            return oHomepage;
        }

        public Homepage Update(NpgsqlTransaction transaction, NpgsqlConnection conn, Homepage oHomepage)
        {
            
            NpgsqlCommand command = GetCommand(SQL_Update, conn, transaction);
            command.Parameters.Add("isactive", NpgsqlTypes.NpgsqlDbType.Boolean).Value = oHomepage.isactive;
            command.Parameters.Add("isselected", NpgsqlTypes.NpgsqlDbType.Boolean).Value = oHomepage.isselected;
            command.Parameters.Add("modifiedon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = DateTimeNow;
            command.Parameters.Add("createdon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = DateTimeNow;
            command.Parameters.Add("createdby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oHomepage.createdby;
            command.Parameters.Add("id", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oHomepage.id;
            command.Parameters.Add("modifiedby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = Context.UserID;
            command.Parameters.Add("version", NpgsqlTypes.NpgsqlDbType.Integer).Value = oHomepage.version;
            command.Parameters.Add("schoolname", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oHomepage.schoolname == null ? "" : oHomepage.schoolname;
            command.Parameters.Add("mobilenumber", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oHomepage.mobilenumber == null ? "" : oHomepage.mobilenumber;
            command.Parameters.Add("emailid", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oHomepage.emailid == null ? "" : oHomepage.emailid;
            command.Parameters.Add("address", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oHomepage.address == null ? "" : oHomepage.address;
            command.Parameters.Add("images", NpgsqlTypes.NpgsqlDbType.Json).Value = oHomepage.images_json == null ? "null" : oHomepage.images_json;
            command.Parameters.Add("aboutus", NpgsqlTypes.NpgsqlDbType.Json).Value = oHomepage.aboutus_json == null ? "null" : oHomepage.aboutus_json;
            command.Parameters.Add("slides", NpgsqlTypes.NpgsqlDbType.Json).Value = oHomepage.slides_json == null ? "null" : oHomepage.slides_json;
            command.Parameters.Add("facilities", NpgsqlTypes.NpgsqlDbType.Json).Value = oHomepage.facilities_json == null ? "null" : oHomepage.facilities_json;
            command.Parameters.Add("domain", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oHomepage.domain == null ? "" : oHomepage.domain;
            command.Parameters.Add("logo", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oHomepage.logo == null ? "null" : oHomepage.logo;
            command.Parameters.Add("ctitle", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oHomepage.ctitle == null ? "" : oHomepage.ctitle;
            command.Parameters.Add("ititle", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oHomepage.ititle == null ? "" : oHomepage.ititle;
            command.Parameters.Add("ftitle", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oHomepage.ftitle == null ? "" : oHomepage.ftitle;
            command.Parameters.Add("systemimages", NpgsqlTypes.NpgsqlDbType.Json).Value = oHomepage.systemimages_json == null ? "null" : oHomepage.systemimages_json;
            command.Parameters.Add("institutionid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = (oHomepage.institutionid);

            using (System.Data.IDataReader reader = command.ExecuteReader())
            {
                if (reader.Read())
                {
                    oHomepage.version = Convert.ToInt32(reader[0].ToString());
                }
            }

            return oHomepage;
        }


        public bool Delete(Homepage oHomepage)
        {

            NpgsqlTransaction transaction = null;
            bool hasSuccessCount = false;
            try
            {
                using (NpgsqlConnection conn = CreateConnection())
                {
                    conn.Open();
                    transaction = conn.BeginTransaction();
                    hasSuccessCount = Delete(transaction, conn, oHomepage);
                    transaction.Commit();
                    conn.Close();
                }
            }
            catch (Exception exp)
            {
                WebAPI.Common.Logger.Instance(Context.OrganizationID).Log(exp, WebAPI.Common.Logger.LogType.Error, oHomepage);

                
            }
            return hasSuccessCount;

        }

        public bool Delete(NpgsqlTransaction transaction, NpgsqlConnection conn, Homepage oHomepage)
        {

            NpgsqlCommand command = GetCommand(SQL_Delete, conn, transaction);
            command.Parameters.Add("isactive", NpgsqlTypes.NpgsqlDbType.Boolean).Value = oHomepage.isactive;
            command.Parameters.Add("modifiedby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = Context.UserID;
            command.Parameters.Add("modifiedon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = DateTimeNow;
            command.Parameters.Add("version", NpgsqlTypes.NpgsqlDbType.Integer).Value = oHomepage.version;
            command.Parameters.Add("id", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oHomepage.id;
            int successCount = command.ExecuteNonQuery();

            return successCount > 0;
        }

    }

}
