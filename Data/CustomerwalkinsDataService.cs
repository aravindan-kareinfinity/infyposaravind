using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using InfyPOS.Models;
using InfyPOS.Common;
using Npgsql;
namespace InfyPOS.Data

{
    public class CustomerwalkinsDataService : SAASData
    {
        const string SQL_Search = @"select createdby, id, createdon, version, isactive, modifiedby, modifiedon, 
        walkincount, mobileno, name, gender, ageset, reason, brand, fromtime, totime,colourreason,sizereason,billno,
        rating,feedback,companyid,locationid,sectionid from customerwalkins where id=@id";

        const string SQL_SearchSummary = @"select to_char(fromtime,'HH12AM') walkinhour,
to_char(fromtime,'DD-MM-YY') walkindate,
to_timestamp(to_char(fromtime,'DD MONTH YYYY HH12 AM'),'DD MONTH YYYY HH12 AM') walkinat,
sum(coalesce(walkincount,1)) walkincount,company.name company,
section.name section,location.name ""location""
from customerwalkins
left outer join referencelist section on section.id = sectionid
left outer join referencelist location on location.id = locationid
left outer join company on company.id = companyid
where fromtime>=@fromtime and totime<@totime
group by 
companyid,locationid,sectionid
to_char(fromtime,'HH12AM') ,
to_char(fromtime,'DD-MM-YY') ,
to_timestamp(to_char(fromtime,'DD MONTH YYYY HH12 AM'),'DD MONTH YYYY HH12 AM')";

        const string SQL_SearchCustomerSummary = @"Select to_char( bill.billdate,'HH12AM') walkinhour,
to_char( bill.billdate,'DD-MM-YY') walkindate,to_timestamp(to_char( bill.billdate,'DD MONTH YYYY HH12 AM'),'DD MONTH YYYY HH12 AM') walkinat,
count(bill.billno) as billcount,sum(case when customerid=0 then 1 else 0 end) billedcustomercount
                            from bill inner join referencelist location on location.id = bill.locationid
                            where bill.isactive = true and bill.billdate>=@fromtime and bill.billdate<@totime
                            and bill.billtype != 9
                            group by to_char( bill.billdate,'HH12AM') ,to_char( bill.billdate,'DD-MM-YY') ,to_timestamp(to_char( bill.billdate,'DD MONTH YYYY HH12 AM'),'DD MONTH YYYY HH12 AM')
							order by to_timestamp(to_char(bill.billdate,'DD MONTH YYYY HH12 AM'),'DD MONTH YYYY HH12 AM')
							
Select to_timestamp(to_char( bill.billdate,'DD MONTH YYYY HH12 AM'),'DD MONTH YYYY HH12 AM') walkinat,
bill.customerid as customerid
                            from bill inner join referencelist location on location.id = bill.locationid
                            where bill.isactive = true and bill.billdate>=@fromtime and bill.billdate<@totime
                            and bill.billtype != 9 and customerid>0
                            group by bill.customerid
							order by to_timestamp(to_char(bill.billdate,'DD MONTH YYYY HH12 AM'),'DD MONTH YYYY HH12 AM')";

        const string SQL_Insert = @"INSERT INTO public.customerwalkins(createdby, createdon, version, isactive, modifiedby, modifiedon, walkincount, mobileno, name, gender, ageset, reason, brand, fromtime, totime,colourreason,sizereason,billno,rating,feedback,companyid,locationid,sectionid)
        VALUES(@createdby, @createdon, @version, @isactive, @modifiedby, @modifiedon, @walkincount, @mobileno, @name, @gender, @ageset, @reason, @brand, @fromtime, @totime,@colourreason,@sizereason,@billno,@rating,@feedback,@companyid,@locationid,@sectionid) RETURNING id";


        const string SQL_Update = @"UPDATE public.customerwalkins SET version=@version+1, modifiedby=@modifiedby, modifiedon=@modifiedon, walkincount=@walkincount, mobileno=@mobileno, name=@name, gender=@gender, ageset=@ageset, reason=@reason, brand=@brand, fromtime=@fromtime, totime=@totime,colourreason=@colourreason,sizereason=@sizereason,
        billno=@billno,rating=@rating,feedback=@feedback,companyid=@companyid,locationid=@locationid,sectionid=@sectionid where id=@id and version=@version RETURNING version";

        const string SQL_Delete = @"update public.customerwalkins set isactive=false,version=@version+1,modifiedby=@modifiedby,modifiedon=@modifiedon where id=@id and version=@version";

        const string SQL_SearchByMobileNo = @"select id,mobileno, customertype, customer.name, dateofbirth, 
                                    loyelty, gender from customer where mobileno=@mobileno";

        const string SQL_SearchByBillNo = @"Select customer.id, customer.mobileno, customer.name, customer.dateofbirth, 
                                    customer.loyelty, customer.gender, billno
                                        from bill inner join customer on customer.id = bill.customerid 
                                        where billno=@billno";

        public List<CustomerWalkinSummary> SearchSummary(CustomerwalkinsCreteria _customerwalkins)
        {
            List<CustomerWalkinSummary> lst = new List<CustomerWalkinSummary>();
            NpgsqlConnection conn = CreateConnection();
            try
            {
                conn.Open();
                var command = new NpgsqlCommand(SQL_SearchSummary, conn);
                command.Parameters.Add("fromtime", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = _customerwalkins.fromtime;
                command.Parameters.Add("totime", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = _customerwalkins.totime.AddMinutes(1);
                using (System.Data.IDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        CustomerWalkinSummary ocustomerwalkins = new CustomerWalkinSummary();
                        ocustomerwalkins.walkincount = reader["walkincount"] == DBNull.Value ? 0 : Convert.ToInt32(reader["walkincount"]);
                        ocustomerwalkins.walkinhour = reader["walkinhour"] == DBNull.Value ? "" : reader["walkinhour"].ToString();
                        ocustomerwalkins.walkindate = reader["walkindate"] == DBNull.Value ? "" : reader["walkindate"].ToString();
                        ocustomerwalkins.walkinat = reader["walkinat"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(reader["walkinat"]);
                        ocustomerwalkins.company = reader["company"] == DBNull.Value ? "" : Convert.ToString(reader["company"]);
                        ocustomerwalkins.location = reader["location"] == DBNull.Value ? "" : Convert.ToString(reader["location"]);
                        ocustomerwalkins.section = reader["section"] == DBNull.Value ? "" : Convert.ToString(reader["section"]);
                        lst.Add(ocustomerwalkins);
                    }
                }

                //command.CommandText = SQL_SearchCustomerSummary;
                //using (System.Data.IDataReader reader = command.ExecuteReader())
                //{
                //    while (reader.Read())
                //    {
                //        CustomerWalkinSummary ocustomerwalkins = new CustomerWalkinSummary();
                //        ocustomerwalkins.walkincount = reader["walkincount"] == DBNull.Value ? 0 : Convert.ToInt32(reader["walkincount"]);
                //        ocustomerwalkins.walkinat = reader["walkinat"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(reader["walkinat"]);
                //        ocustomerwalkins.walkinhour = reader["walkinhour"] == DBNull.Value ? "" : reader["walkinhour"].ToString();
                //        ocustomerwalkins.walkindate = reader["walkindate"] == DBNull.Value ? "" : reader["walkindate"].ToString();
                //        ocustomerwalkins.billcount = reader["billcount"] == DBNull.Value ? 0 : Convert.ToInt32(reader["billcount"]);
                //        ocustomerwalkins.billedcustomercount = reader["billedcustomercount"] == DBNull.Value ? 0 : Convert.ToInt32(reader["billedcustomercount"]);
                //        if (lst.Exists(e => e.walkinat == ocustomerwalkins.walkinat))
                //        {
                //            var ccustomer = lst.Find(e => e.walkinat == ocustomerwalkins.walkinat);
                //            ccustomer.billcount += ocustomerwalkins.billcount;
                //            ccustomer.billedcustomercount += ocustomerwalkins.billedcustomercount;
                //        }

                //        lst.Add(ocustomerwalkins);
                //    }

                //    if (reader.NextResult())
                //    {
                //        while (reader.Read())
                //        {
                //            DateTime walinat = reader["walkinat"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(reader["walkinat"]);
                //            if (lst.Exists(e => e.walkinat == walinat))
                //            {
                //                lst.Find(e => e.walkinat == walinat).billedcustomercount += 1;
                //            }
                //        }
                //    }
                //}
            }
            catch (Exception exp)
            {

            }

            return lst;
        }
        public List<Customerwalkins> Search(CustomerwalkinsCreteria _customerwalkins)
        {
            List<Customerwalkins> lst = new List<Customerwalkins>();
            NpgsqlConnection conn = CreateConnection();
            try
            {
                conn.Open();

                NpgsqlCommand command = null;
                if (_customerwalkins.id != 0)
                {
                    command = new NpgsqlCommand(SQL_Search, conn);
                    command.Parameters.Add("id", NpgsqlTypes.NpgsqlDbType.Bigint).Value = _customerwalkins.id;
                }
                else
                {
                    QueryBuilder qb = new QueryBuilder();
                    qb.CurrentQuery = SQL_Search.Substring(0, SQL_Search.IndexOf("where"));
                    qb.AddParameter("isactive", true, "=");

                    //if (!string.IsNullOrEmpty(_customerwalkins.Code))
                    //    qb.AddParameter("code", _customerwalkins.Code, "=");

                    command = qb.GetCommand(conn);
                }

                using (System.Data.IDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        Customerwalkins ocustomerwalkins = new Customerwalkins();
                        ocustomerwalkins.createdby = reader["createdby"] == DBNull.Value ? 0 : Convert.ToInt64(reader["createdby"]);
                        ocustomerwalkins.id = reader["id"] == DBNull.Value ? 0 : Convert.ToInt64(reader["id"]);

                        ocustomerwalkins.companyid = reader["companyid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["companyid"]);
                        ocustomerwalkins.sectionid = reader["sectionid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["sectionid"]);
                        ocustomerwalkins.locationid = reader["locationid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["locationid"]);

                        ocustomerwalkins.createdon = reader["createdon"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(reader["createdon"]);
                        ocustomerwalkins.version = reader["version"] == DBNull.Value ? 0 : Convert.ToInt32(reader["version"]);
                        ocustomerwalkins.isactive = reader["isactive"] == DBNull.Value ? false : Convert.ToBoolean(reader["isactive"]);
                        ocustomerwalkins.modifiedby = reader["modifiedby"] == DBNull.Value ? 0 : Convert.ToInt64(reader["modifiedby"]);
                        ocustomerwalkins.modifiedon = reader["modifiedon"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(reader["modifiedon"]);
                        ocustomerwalkins.walkincount = reader["walkincount"] == DBNull.Value ? 0 : Convert.ToInt32(reader["walkincount"]);
                        ocustomerwalkins.mobileno = reader["mobileno"] == DBNull.Value ? "" : reader["mobileno"].ToString();
                        ocustomerwalkins.name = reader["name"] == DBNull.Value ? "" : reader["name"].ToString();
                        ocustomerwalkins.gender = reader["gender"] == DBNull.Value ? "" : reader["gender"].ToString();
                        ocustomerwalkins.ageset = reader["ageset"] == DBNull.Value ? "" : reader["ageset"].ToString();
                        ocustomerwalkins.reason = reader["reason"] == DBNull.Value ? "" : reader["reason"].ToString();
                        ocustomerwalkins.brand = reader["brand"] == DBNull.Value ? "" : reader["brand"].ToString();
                        ocustomerwalkins.fromtime = reader["fromtime"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(reader["fromtime"]);
                        ocustomerwalkins.totime = reader["totime"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(reader["totime"]);
                        ocustomerwalkins.colourreason = reader["colourreason"] == DBNull.Value ? "" : reader["colourreason"].ToString();
                        ocustomerwalkins.sizereason = reader["sizereason"] == DBNull.Value ? "" : reader["sizereason"].ToString();
                        ocustomerwalkins.billno = reader["billno"] == DBNull.Value ? "" : reader["billno"].ToString();
                        ocustomerwalkins.rating = reader["rating"] == DBNull.Value ? 0 : Convert.ToInt32(reader["rating"]);
                        ocustomerwalkins.feedback = reader["feedback"] == DBNull.Value ? "" : reader["feedback"].ToString();


                        lst.Add(ocustomerwalkins);
                    }
                }
                conn.Close();
            }
            catch (Exception exp)
            {
                if (conn != null) conn.Close();
                WebAPI.Common.Logger.Instance(Context.OrganizationID).Log(exp, WebAPI.Common.Logger.LogType.Error, _customerwalkins);
            }
            return lst;
        }

        public Customerwalkins Insert(Customerwalkins oCustomerwalkins)
        {

            NpgsqlTransaction transaction = null;
            try
            {
                using (NpgsqlConnection conn = CreateConnection())
                {
                    conn.Open();
                    transaction = conn.BeginTransaction();
                    

                    Insert(transaction, conn, oCustomerwalkins);
                    transaction.Commit();
                    conn.Close();
                }
            }
            catch (Exception exp)
            {
                WebAPI.Common.Logger.Instance(Context.OrganizationID).Log(exp, WebAPI.Common.Logger.LogType.Error, oCustomerwalkins);
            }

            return oCustomerwalkins;
        }

        public Customerwalkins Insert(NpgsqlTransaction transaction, NpgsqlConnection conn, Customerwalkins oCustomerwalkins)
        {

            if (oCustomerwalkins.walkincount == 0) oCustomerwalkins.walkincount = 1;
            NpgsqlCommand command = new NpgsqlCommand(SQL_Insert, conn, transaction);

            command.Parameters.Add("locationid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oCustomerwalkins.locationid;
            command.Parameters.Add("sectionid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oCustomerwalkins.sectionid;
            command.Parameters.Add("companyid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oCustomerwalkins.companyid;

            command.Parameters.Add("createdby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oCustomerwalkins.createdby;
            command.Parameters.Add("createdon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = oCustomerwalkins.createdon;
            command.Parameters.Add("version", NpgsqlTypes.NpgsqlDbType.Integer).Value = oCustomerwalkins.version;
            command.Parameters.Add("isactive", NpgsqlTypes.NpgsqlDbType.Boolean).Value = oCustomerwalkins.isactive;
            command.Parameters.Add("modifiedby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oCustomerwalkins.modifiedby;
            command.Parameters.Add("modifiedon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = oCustomerwalkins.modifiedon;
            command.Parameters.Add("walkincount", NpgsqlTypes.NpgsqlDbType.Integer).Value = oCustomerwalkins.walkincount;
            command.Parameters.Add("mobileno", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oCustomerwalkins.mobileno == null ? "" : oCustomerwalkins.mobileno;
            command.Parameters.Add("name", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oCustomerwalkins.name == null ? "" : oCustomerwalkins.name;
            command.Parameters.Add("gender", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oCustomerwalkins.gender == null ? "" : oCustomerwalkins.gender;
            command.Parameters.Add("ageset", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oCustomerwalkins.ageset == null ? "" : oCustomerwalkins.ageset;
            command.Parameters.Add("reason", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oCustomerwalkins.reason == null ? "" : oCustomerwalkins.reason;
            command.Parameters.Add("brand", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oCustomerwalkins.brand == null ? "" : oCustomerwalkins.brand;
            command.Parameters.Add("fromtime", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = oCustomerwalkins.fromtime;
            command.Parameters.Add("totime", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = oCustomerwalkins.totime;
            command.Parameters.Add("colourreason", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oCustomerwalkins.colourreason == null ? "" : oCustomerwalkins.colourreason;
            command.Parameters.Add("sizereason", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oCustomerwalkins.sizereason == null ? "" : oCustomerwalkins.sizereason;
            command.Parameters.Add("billno", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oCustomerwalkins.billno == null ? "" : oCustomerwalkins.billno;
            command.Parameters.Add("feedback", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oCustomerwalkins.feedback == null ? "" : oCustomerwalkins.feedback;
            command.Parameters.Add("rating", NpgsqlTypes.NpgsqlDbType.Integer).Value = oCustomerwalkins.rating;

            using (System.Data.IDataReader reader = command.ExecuteReader())
            {
                if (reader.Read())
                {
                    oCustomerwalkins.id = Convert.ToInt64(reader[0].ToString());
                }
            }
            return oCustomerwalkins;
        }

        public Customerwalkins Update(Customerwalkins oCustomerwalkins)
        {

            NpgsqlTransaction transaction = null;
            try
            {
                using (NpgsqlConnection conn = CreateConnection())
                {
                    conn.Open();
                    transaction = conn.BeginTransaction();

                    Update(transaction, conn, oCustomerwalkins);
                    transaction.Commit();
                    conn.Close();
                }
            }
            catch (Exception exp)
            {
                WebAPI.Common.Logger.Instance(Context.OrganizationID).Log(exp, WebAPI.Common.Logger.LogType.Error, oCustomerwalkins);
            }

            return oCustomerwalkins;
        }

        public Customerwalkins Update(NpgsqlTransaction transaction, NpgsqlConnection conn, Customerwalkins oCustomerwalkins)
        {

            NpgsqlCommand command = new NpgsqlCommand(SQL_Update, conn, transaction);
            command.Parameters.Add("createdby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oCustomerwalkins.createdby;
            command.Parameters.Add("id", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oCustomerwalkins.id;
            command.Parameters.Add("createdon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = oCustomerwalkins.createdon;
            command.Parameters.Add("version", NpgsqlTypes.NpgsqlDbType.Integer).Value = oCustomerwalkins.version;
            command.Parameters.Add("isactive", NpgsqlTypes.NpgsqlDbType.Boolean).Value = oCustomerwalkins.isactive;
            command.Parameters.Add("modifiedby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oCustomerwalkins.modifiedby;
            command.Parameters.Add("modifiedon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = oCustomerwalkins.modifiedon;
            command.Parameters.Add("walkincount", NpgsqlTypes.NpgsqlDbType.Integer).Value = oCustomerwalkins.walkincount;
            command.Parameters.Add("mobileno", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oCustomerwalkins.mobileno == null ? "" : oCustomerwalkins.mobileno;
            command.Parameters.Add("name", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oCustomerwalkins.name == null ? "" : oCustomerwalkins.name;
            command.Parameters.Add("gender", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oCustomerwalkins.gender == null ? "" : oCustomerwalkins.gender;
            command.Parameters.Add("ageset", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oCustomerwalkins.ageset == null ? "" : oCustomerwalkins.ageset;
            command.Parameters.Add("reason", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oCustomerwalkins.reason == null ? "" : oCustomerwalkins.reason;
            command.Parameters.Add("brand", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oCustomerwalkins.brand == null ? "" : oCustomerwalkins.brand;
            command.Parameters.Add("fromtime", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = oCustomerwalkins.fromtime;
            command.Parameters.Add("totime", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = oCustomerwalkins.totime;
            command.Parameters.Add("colourreason", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oCustomerwalkins.colourreason == null ? "" : oCustomerwalkins.colourreason;
            command.Parameters.Add("sizereason", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oCustomerwalkins.sizereason == null ? "" : oCustomerwalkins.sizereason;
            command.Parameters.Add("billno", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oCustomerwalkins.billno == null ? "" : oCustomerwalkins.billno;
            command.Parameters.Add("feedback", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oCustomerwalkins.feedback == null ? "" : oCustomerwalkins.feedback;
            command.Parameters.Add("rating", NpgsqlTypes.NpgsqlDbType.Integer).Value = oCustomerwalkins.rating;
            command.Parameters.Add("locationid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oCustomerwalkins.locationid;
            command.Parameters.Add("sectionid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oCustomerwalkins.sectionid;
            command.Parameters.Add("companyid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oCustomerwalkins.companyid;

            using (System.Data.IDataReader reader = command.ExecuteReader())
            {
                if (reader.Read())
                {
                    oCustomerwalkins.version = Convert.ToInt32(reader[0].ToString());
                }
            }

            return oCustomerwalkins;
        }

        public Customerwalkins SearchCustomerByMobileNo(CustomerwalkinsCreteria _customerwalkins)
        {
            Customerwalkins lst = new Customerwalkins();
            NpgsqlConnection conn = CreateConnection();
            try
            {
                conn.Open();

                NpgsqlCommand command = null;

                QueryBuilder qb = new QueryBuilder();
                qb.CurrentQuery = SQL_SearchByMobileNo.Substring(0, SQL_SearchByMobileNo.IndexOf("where"));
                qb.AddParameter("customer.isactive", true, "=");

                if (!string.IsNullOrEmpty(_customerwalkins.mobileno))
                    qb.AddParameter("customer.mobileno", _customerwalkins.mobileno, "=");

                command = qb.GetCommand(conn);


                using (System.Data.IDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        Customerwalkins ocustomer = new Customerwalkins();
                        ocustomer.id = reader["id"] == DBNull.Value ? 0 : Convert.ToInt64(reader["id"]);

                        ocustomer.mobileno = reader["mobileno"] == DBNull.Value ? "" : reader["mobileno"].ToString();
                        ocustomer.name = reader["name"] == DBNull.Value ? "" : reader["name"].ToString();
                        //ocustomer.dateofbirth = reader["dateofbirth"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(reader["dateofbirth"]);
                        ocustomer.gender = reader["gender"] == DBNull.Value ? "" : reader["gender"].ToString();
                    }
                }


                conn.Close();
            }
            catch (Exception exp)
            {
                if (conn != null) conn.Close();
                WebAPI.Common.Logger.Instance(Context.OrganizationID).Log(exp, WebAPI.Common.Logger.LogType.Error, _customerwalkins);
            }
            return lst;
        }

        public Customerwalkins SearchCustomerByBillNo(CustomerwalkinsCreteria _customerwalkins)
        {
            Customerwalkins lst = new Customerwalkins();
            NpgsqlConnection conn = CreateConnection();
            try
            {
                conn.Open();

                NpgsqlCommand command = null;

                QueryBuilder qb = new QueryBuilder();
                qb.CurrentQuery = SQL_SearchByBillNo.Substring(0, SQL_SearchByBillNo.IndexOf("where"));

                if (!string.IsNullOrEmpty(_customerwalkins.billno))
                    qb.AddParameter("bill.billno", _customerwalkins.billno, "=");

                qb.Limit = 1;
                command = qb.GetCommand(conn);


                using (System.Data.IDataReader reader = command.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        lst.id = reader["id"] == DBNull.Value ? 0 : Convert.ToInt64(reader["id"]);
                        lst.mobileno = reader["mobileno"] == DBNull.Value ? "" : reader["mobileno"].ToString();
                        lst.name = reader["name"] == DBNull.Value ? "" : reader["name"].ToString();
                        lst.billno = reader["billno"] == DBNull.Value ? "" : reader["billno"].ToString();
                        lst.gender = reader["gender"] == DBNull.Value ? "" : reader["gender"].ToString();
                    }
                }


                conn.Close();
            }
            catch (Exception exp)
            {
                if (conn != null) conn.Close();
                WebAPI.Common.Logger.Instance(Context.OrganizationID).Log(exp, WebAPI.Common.Logger.LogType.Error, _customerwalkins);
            }
            return lst;
        }


        public bool Delete(Customerwalkins oCustomerwalkins)
        {

            NpgsqlTransaction transaction = null;
            bool hasSuccessCount = false;
            try
            {
                using (NpgsqlConnection conn = CreateConnection())
                {
                    conn.Open();
                    transaction = conn.BeginTransaction();
                    hasSuccessCount = Delete(transaction, conn, oCustomerwalkins);
                    transaction.Commit();
                    conn.Close();
                }
            }
            catch (Exception exp)
            {
                WebAPI.Common.Logger.Instance(Context.OrganizationID).Log(exp, WebAPI.Common.Logger.LogType.Error, oCustomerwalkins);
            }
            return hasSuccessCount;

        }

        public bool Delete(NpgsqlTransaction transaction, NpgsqlConnection conn, Customerwalkins oCustomerwalkins)
        {

            NpgsqlCommand command = new NpgsqlCommand(SQL_Delete, conn, transaction);
            command.Parameters.Add("isactive", NpgsqlTypes.NpgsqlDbType.Boolean).Value = oCustomerwalkins.isactive;
            command.Parameters.Add("modifiedby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oCustomerwalkins.modifiedby;
            command.Parameters.Add("modifiedon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = oCustomerwalkins.modifiedon;
            command.Parameters.Add("version", NpgsqlTypes.NpgsqlDbType.Integer).Value = oCustomerwalkins.version;
            command.Parameters.Add("id", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oCustomerwalkins.id;
            int successCount = command.ExecuteNonQuery();

            return successCount > 0;
        }

        internal int GetWalkinCount(DateTime today)
        {
            string query = "select sum(walkincount) from customerwalkins where fromtime>=@from and totime<@to";

            if (!Context.accessiblelocation.Exists(e => e == -1))
            {
                if (Context.accessiblelocation.Count == 1)
                    query += " and customerwalkins.locationid = " + Context.accessiblelocation[0].ToString();
                if (Context.accessiblelocation.Count > 1)
                    query += " and customerwalkins.locationid in (" + string.Join(",", Context.accessiblelocation) + ")";
            }

            var param = new SortedList<string, object>();
            param.Add("from", today.Date);
            param.Add("to", today.Date.AddDays(1));
            using (var reader = ExecuteReader(query, param))
            {
                if(reader.Read())
                {
                    int count = reader.IsDBNull(0) ? 0 : reader.GetInt32(0);
                    reader.Close();
                    return count;
                }
                
            }
            return 0;
        }
    }

}
