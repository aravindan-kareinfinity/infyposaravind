using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using InfyPOS.Models;
using InfyPOS.Common;
using Npgsql;
namespace InfyPOS.Data

{
    public class EmployeeinchargesDataService : BaseData
    {
        /*
update Employeeincharges set isactive=false where id in (select id from (
select id,row_number() over (partition by inchargetype,employeeid,inchargeid order by id) rownumber
from Employeeincharges
) x where x.rownumber>1)*/

        const string SQL_Search = @"select incentive, locationid, activefrom, activeto, isactive, version, createdon, createdby, id, modifiedon, employeeid, modifiedby, inchargeid, incasvalue, inchargetype from employeeincharges where id=@id";


        const string SQL_Insert = @"INSERT INTO public.employeeincharges(incentive, locationid, activefrom, activeto, isactive, version, createdon, createdby, modifiedon, employeeid, modifiedby, inchargeid, incasvalue, inchargetype) VALUES(@incentive, @locationid, @activefrom, @activeto, @isactive, @version, @createdon, @createdby, @modifiedon, @employeeid, @modifiedby, @inchargeid, @incasvalue, @inchargetype) RETURNING id";


        const string SQL_Update = @"UPDATE public.employeeincharges SET incentive=@incentive, locationid=@locationid, activefrom=@activefrom, activeto=@activeto, version=@version+1, modifiedon=@modifiedon, employeeid=@employeeid, modifiedby=@modifiedby, inchargeid=@inchargeid, incasvalue=@incasvalue, inchargetype=@inchargetype where id=@id and version=@version RETURNING version";

        const string SQL_Delete = @"update public.employeeincharges set isactive=false,version=@version+1,modifiedby=@modifiedby,modifiedon=@modifiedon where id=@id and version=@version";

        public List<Employeeincharges> Search(EmployeeinchargesCreteria _employeeincharges)
        {
            List<Employeeincharges> lst = new List<Employeeincharges>();
            NpgsqlConnection conn = CreateConnection();
            try
            {
                conn.Open();

                NpgsqlCommand command = null;
                if (_employeeincharges.id != 0)
                {
                    command = new NpgsqlCommand(SQL_Search, conn);
                    command.Parameters.Add("id", NpgsqlTypes.NpgsqlDbType.Bigint).Value = _employeeincharges.id;
                }
                else
                {
                    QueryBuilder qb = new QueryBuilder();
                    qb.CurrentQuery = SQL_Search.Substring(0, SQL_Search.IndexOf("where"));
                    qb.AddParameter("isactive", true, "=");

                    if (_employeeincharges.employeeid>0)
                        qb.AddParameter("employeeid", _employeeincharges.employeeid, "=");

                    if (_employeeincharges.inchargeid > 0)
                    {
                        qb.AddParameter("inchargeid", _employeeincharges.inchargeid, "=");
                        qb.AddParameter("inchargetype", _employeeincharges.inchargetype, "=");
                    }
                    else if(!string.IsNullOrEmpty(_employeeincharges.inchargetype))
                    {
                        qb.AddParameter("inchargetype", _employeeincharges.inchargetype, "=");
                    }
                    command = qb.GetCommand(conn);
                }

                using (System.Data.IDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        Employeeincharges oemployeeincharges = new Employeeincharges();
                        oemployeeincharges.incentive = reader["incentive"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["incentive"]);
                        oemployeeincharges.locationid = reader["locationid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["locationid"]);
                        oemployeeincharges.activefrom = reader["activefrom"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(reader["activefrom"]);
                        oemployeeincharges.activeto = reader["activeto"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(reader["activeto"]);
                        oemployeeincharges.isactive = reader["isactive"] == DBNull.Value ? false : Convert.ToBoolean(reader["isactive"]);
                        oemployeeincharges.version = reader["version"] == DBNull.Value ? 0 : Convert.ToInt32(reader["version"]);
                        oemployeeincharges.createdon = reader["createdon"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(reader["createdon"]);
                        oemployeeincharges.createdby = reader["createdby"] == DBNull.Value ? 0 : Convert.ToInt64(reader["createdby"]);
                        oemployeeincharges.id = reader["id"] == DBNull.Value ? 0 : Convert.ToInt64(reader["id"]);
                        oemployeeincharges.modifiedon = reader["modifiedon"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(reader["modifiedon"]);
                        oemployeeincharges.employeeid = reader["employeeid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["employeeid"]);
                        oemployeeincharges.modifiedby = reader["modifiedby"] == DBNull.Value ? 0 : Convert.ToInt64(reader["modifiedby"]);
                        oemployeeincharges.inchargeid = reader["inchargeid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["inchargeid"]);
                        oemployeeincharges.incasvalue = reader["incasvalue"] == DBNull.Value ? false : Convert.ToBoolean(reader["incasvalue"]);
                        oemployeeincharges.inchargetype = reader["inchargetype"] == DBNull.Value ? "" : reader["inchargetype"].ToString();
                        lst.Add(oemployeeincharges);
                    }
                }
                conn.Close();
            }
            catch (Exception exp)
            {
                if (conn != null) conn.Close();
                WebAPI.Common.Logger.Instance(Context.OrganizationID).Log(exp, WebAPI.Common.Logger.LogType.Error, _employeeincharges);
            }
            return lst;
        }

        public List<Employeeincharges> SaveAll(List<Employeeincharges> oEmployeeincharges)
        {
            NpgsqlTransaction transaction = null;
            try
            {
                var existings = Search(new EmployeeinchargesCreteria()
                {
                    employeeid = oEmployeeincharges[0].employeeid
                });
                var deletelist = existings.FindAll(x => !oEmployeeincharges.Exists(y => y.id == x.id));
                using (NpgsqlConnection conn = CreateConnection())
                {
                    conn.Open();
                    transaction = conn.BeginTransaction();
                    oEmployeeincharges.FindAll(x=>x.id == 0).ForEach(x =>
                    {
                        x.isactive = true;
                        x.createdby = Context.UserID;
                        x.modifiedby = Context.UserID;
                        x.createdon = DateTimeNow;
                        x.modifiedon = DateTimeNow;
                        Insert(transaction, conn, x);
                    });
                    deletelist.ForEach(x =>
                    {
                        Delete(transaction, conn, x);
                    });
                    transaction.Commit();
                    conn.Close();
                }
            }
            catch (Exception exp)
            {
                WebAPI.Common.Logger.Instance(Context.OrganizationID).Log(exp, WebAPI.Common.Logger.LogType.Error, oEmployeeincharges);
                if (transaction != null)
                    transaction.Rollback();
            }

            return oEmployeeincharges;
        }


        public Employeeincharges Insert(Employeeincharges oEmployeeincharges)
        {

            NpgsqlTransaction transaction = null;
            try
            {
                using (NpgsqlConnection conn = CreateConnection())
                {
                    conn.Open();
                    transaction = conn.BeginTransaction();
                    NpgsqlCommand command = new NpgsqlCommand(SQL_Insert, conn, transaction);

                    Insert(transaction, conn, oEmployeeincharges);
                    transaction.Commit();
                    conn.Close();
                }
            }
            catch (Exception exp)
            {
                WebAPI.Common.Logger.Instance(Context.OrganizationID).Log(exp, WebAPI.Common.Logger.LogType.Error, oEmployeeincharges);
                if (transaction != null)
                    transaction.Rollback();
            }

            return oEmployeeincharges;
        }

        public Employeeincharges Insert(NpgsqlTransaction transaction, NpgsqlConnection conn, Employeeincharges oEmployeeincharges)
        {

            NpgsqlCommand command = new NpgsqlCommand(SQL_Insert, conn, transaction);
            command.Parameters.Add("incentive", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oEmployeeincharges.incentive;
            command.Parameters.Add("locationid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oEmployeeincharges.locationid;
            command.Parameters.Add("activefrom", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = oEmployeeincharges.activefrom;
            command.Parameters.Add("activeto", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = oEmployeeincharges.activeto;
            command.Parameters.Add("isactive", NpgsqlTypes.NpgsqlDbType.Boolean).Value = oEmployeeincharges.isactive;
            command.Parameters.Add("version", NpgsqlTypes.NpgsqlDbType.Integer).Value = oEmployeeincharges.version;
            command.Parameters.Add("createdon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = oEmployeeincharges.createdon;
            command.Parameters.Add("createdby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oEmployeeincharges.createdby;
            command.Parameters.Add("modifiedon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = oEmployeeincharges.modifiedon;
            command.Parameters.Add("employeeid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oEmployeeincharges.employeeid;
            command.Parameters.Add("modifiedby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oEmployeeincharges.modifiedby;
            command.Parameters.Add("inchargeid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oEmployeeincharges.inchargeid;
            command.Parameters.Add("incasvalue", NpgsqlTypes.NpgsqlDbType.Boolean).Value = oEmployeeincharges.incasvalue;
            command.Parameters.Add("inchargetype", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oEmployeeincharges.inchargetype == null ? "" : oEmployeeincharges.inchargetype;
            using (System.Data.IDataReader reader = command.ExecuteReader())
            {
                if (reader.Read())
                {
                    oEmployeeincharges.id = Convert.ToInt64(reader[0].ToString());
                }
            }
            return oEmployeeincharges;
        }

        public Employeeincharges Update(Employeeincharges oEmployeeincharges)
        {

            NpgsqlTransaction transaction = null;
            try
            {
                using (NpgsqlConnection conn = CreateConnection())
                {
                    conn.Open();
                    transaction = conn.BeginTransaction();

                    Update(transaction, conn, oEmployeeincharges);
                    transaction.Commit();
                    conn.Close();
                }
            }
            catch (Exception exp)
            {
                WebAPI.Common.Logger.Instance(Context.OrganizationID).Log(exp, WebAPI.Common.Logger.LogType.Error, oEmployeeincharges);
                if (transaction != null)
                    transaction.Rollback();
            }

            return oEmployeeincharges;
        }

        public Employeeincharges Update(NpgsqlTransaction transaction, NpgsqlConnection conn, Employeeincharges oEmployeeincharges)
        {

            NpgsqlCommand command = new NpgsqlCommand(SQL_Update, conn, transaction);
            command.Parameters.Add("incentive", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oEmployeeincharges.incentive;
            command.Parameters.Add("locationid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oEmployeeincharges.locationid;
            command.Parameters.Add("activefrom", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = oEmployeeincharges.activefrom;
            command.Parameters.Add("activeto", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = oEmployeeincharges.activeto;
            command.Parameters.Add("isactive", NpgsqlTypes.NpgsqlDbType.Boolean).Value = oEmployeeincharges.isactive;
            command.Parameters.Add("version", NpgsqlTypes.NpgsqlDbType.Integer).Value = oEmployeeincharges.version;
            command.Parameters.Add("createdon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = oEmployeeincharges.createdon;
            command.Parameters.Add("createdby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oEmployeeincharges.createdby;
            command.Parameters.Add("id", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oEmployeeincharges.id;
            command.Parameters.Add("modifiedon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = oEmployeeincharges.modifiedon;
            command.Parameters.Add("employeeid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oEmployeeincharges.employeeid;
            command.Parameters.Add("modifiedby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oEmployeeincharges.modifiedby;
            command.Parameters.Add("inchargeid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oEmployeeincharges.inchargeid;
            command.Parameters.Add("incasvalue", NpgsqlTypes.NpgsqlDbType.Boolean).Value = oEmployeeincharges.incasvalue;
            command.Parameters.Add("inchargetype", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oEmployeeincharges.inchargetype == null ? "" : oEmployeeincharges.inchargetype;
            using (System.Data.IDataReader reader = command.ExecuteReader())
            {
                if (reader.Read())
                {
                    oEmployeeincharges.version = Convert.ToInt32(reader[0].ToString());
                }
            }

            return oEmployeeincharges;
        }


        public bool Delete(Employeeincharges oEmployeeincharges)
        {

            NpgsqlTransaction transaction = null;
            bool hasSuccessCount = false;
            try
            {
                using (NpgsqlConnection conn = CreateConnection())
                {
                    conn.Open();
                    transaction = conn.BeginTransaction();
                    hasSuccessCount = Delete(transaction, conn, oEmployeeincharges);
                    transaction.Commit();
                    conn.Close();
                }
            }
            catch (Exception exp)
            {
                WebAPI.Common.Logger.Instance(Context.OrganizationID).Log(exp, WebAPI.Common.Logger.LogType.Error, oEmployeeincharges);

                if (transaction != null)
                    transaction.Rollback();
            }
            return hasSuccessCount;

        }

        /// <summary>Product incharge sales from bills; billdate in [sfrom, sto).</summary>
        public List<EmployeeinchargePrdBillRow> SearchPrdBillSalesByBillDate(EmployeeinchargesCreteria criteria)
        {
            var list = new List<EmployeeinchargePrdBillRow>();
            if (criteria == null)
                return list;
            var sfrom = criteria.sfrom.Date;
            var sto = criteria.sto.Date;
            if (sto <= sfrom)
                return list;

            const string sql = @"select coalesce(employee.name,'') as employeename,
coalesce(products.name,'') as productname,
coalesce(referencelist.name,'') as sectionname,
sum(billitems.qty) as billedqty,
sum(billitems.receivable-billitems.tax) as taxable,
sum(stock.buyingprice*billitems.qty) as netcost,
sum(coalesce(lrinvoiceitems.basebuyingprice, stock.buyingprice, 0)*billitems.qty) as basecost
from bill
inner join billitems on billitems.billid = bill.id and billitems.isactive=true
inner join stock on stock.id = billitems.stockid
left outer join lrinvoiceitems on lrinvoiceitems.id = stock.invoiceitemid
left outer join (select employeeid, inchargeid, row_number() over (partition by inchargeid order by id desc) as ranknumber
                   from employeeincharges where inchargetype ='PRD' and isactive=true) incharge on incharge.ranknumber = 1 and incharge.inchargeid = stock.productid
left outer join employee on employee.id = incharge.employeeid
inner join products on products.id = stock.productid
left outer join referencelist on referencelist.id = products.salescounterid
where bill.billdate >= @sfrom and bill.billdate < @sto
and bill.billtype != 9
and bill.isactive=true
group by employee.name, products.name, referencelist.name";

            NpgsqlConnection conn = CreateConnection();
            try
            {
                conn.Open();
                using (var command = new NpgsqlCommand(sql, conn))
                {
                    command.Parameters.Add("sfrom", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = sfrom;
                    command.Parameters.Add("sto", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = sto;
                    long rowId = 1;
                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            list.Add(new EmployeeinchargePrdBillRow
                            {
                                _id = rowId++,
                                employeename = reader["employeename"] == DBNull.Value ? "" : reader["employeename"].ToString(),
                                productname = reader["productname"] == DBNull.Value ? "" : reader["productname"].ToString(),
                                sectionname = reader["sectionname"] == DBNull.Value ? "" : reader["sectionname"].ToString(),
                                billedqty = reader["billedqty"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["billedqty"]),
                                taxable = reader["taxable"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["taxable"]),
                                netcost = reader["netcost"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["netcost"]),
                                basecost = reader["basecost"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["basecost"])
                            });
                        }
                    }
                }
                conn.Close();
            }
            catch (Exception exp)
            {
                if (conn != null) conn.Close();
                WebAPI.Common.Logger.Instance(Context.OrganizationID).Log(exp, WebAPI.Common.Logger.LogType.Error, criteria);
            }
            return list;
        }

        public bool Delete(NpgsqlTransaction transaction, NpgsqlConnection conn, Employeeincharges oEmployeeincharges)
        {

            NpgsqlCommand command = new NpgsqlCommand(SQL_Delete, conn, transaction);
            command.Parameters.Add("isactive", NpgsqlTypes.NpgsqlDbType.Boolean).Value = oEmployeeincharges.isactive;
            command.Parameters.Add("modifiedby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oEmployeeincharges.modifiedby;
            command.Parameters.Add("modifiedon", NpgsqlTypes.NpgsqlDbType.TimestampTZ).Value = oEmployeeincharges.modifiedon;
            command.Parameters.Add("version", NpgsqlTypes.NpgsqlDbType.Integer).Value = oEmployeeincharges.version;
            command.Parameters.Add("id", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oEmployeeincharges.id;
            int successCount = command.ExecuteNonQuery();

            return successCount > 0;
        }

    }

}
