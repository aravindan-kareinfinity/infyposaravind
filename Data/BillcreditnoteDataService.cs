using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using InfyPOS.Models;
using InfyPOS.Common;
using Npgsql;
namespace InfyPOS.Data
{
    public class BillcreditnoteDataService : BaseData
    {
        const string SQL_Search = @"select customer.name as customername, billcreditnote.customerid, tax, amount, totalamount, adjustedtax, adjustedvalue, adjustedtotal, remarks,
        companyid, creditnotedate, creditnoteno, billcreditnote.modifiedon, billcreditnote.createdon, billcreditnote.version, billcreditnote.modifiedby, billcreditnote.createdby, billcreditnote.isactive, billcreditnote.id, 
        returnreasonid, rounding, billcreditnote.locationid, billcreditnote.isinterstatesale, voucherinfo, mastersettlementid from billcreditnote 
        inner join customer on customer.id = customerid
        where billcreditnote.id=@id";


        const string SQL_Insert = @"INSERT INTO public.billcreditnote(customerid, tax, amount, totalamount, adjustedtax, adjustedvalue, adjustedtotal,remarks, companyid, creditnotedate, creditnoteno, modifiedon, createdon, version, modifiedby, createdby, isactive, returnreasonid, rounding, locationid, isinterstatesale, voucherinfo, mastersettlementid) VALUES(@customerid, @tax, @amount, @totalamount, @adjustedtax, @adjustedvalue, @adjustedtotal,@remarks, @companyid, @creditnotedate, @creditnoteno, @modifiedon, @createdon, @version, @modifiedby, @createdby, @isactive, @returnreasonid, @rounding, @locationid, @isinterstatesale, @voucherinfo, @mastersettlementid) RETURNING id";


        const string SQL_Update = @"UPDATE public.billcreditnote SET customerid=@customerid, tax=@tax, amount=@amount, totalamount=@totalamount, adjustedtax=@adjustedtax, adjustedvalue=@adjustedvalue, adjustedtotal=@adjustedtotal,remarks=@remarks, companyid=@companyid, creditnotedate=@creditnotedate, creditnoteno=@creditnoteno, modifiedon=@modifiedon, version=@version+1, modifiedby=@modifiedby, returnreasonid=@returnreasonid, rounding=@rounding, locationid=@locationid, isinterstatesale=@isinterstatesale, voucherinfo=@voucherinfo, mastersettlementid=@mastersettlementid where id=@id and version=@version RETURNING version";

        const string SQL_Delete = @"update public.billcreditnote set isactive=false,version=@version+1,modifiedby=@modifiedby,modifiedon=@modifiedon where id=@id and version=@version";

        public List<Billcreditnote> Search(BillcreditnoteCreteria _billcreditnote)
        {
            List<Billcreditnote> lst = new List<Billcreditnote>();
            NpgsqlConnection conn = CreateConnection();
            try
            {
                conn.Open();

                NpgsqlCommand command = null;
                if (_billcreditnote.id != 0)
                {
                    command = new NpgsqlCommand(SQL_Search, conn);
                    command.Parameters.Add("id", NpgsqlTypes.NpgsqlDbType.Bigint).Value = _billcreditnote.id;
                }
                else
                {
                    QueryBuilder qb = new QueryBuilder();
                    qb.CurrentQuery = SQL_Search.Substring(0, SQL_Search.IndexOf("where"));
                    qb.AddParameter("billcreditnote.isactive", true, "=");

                    if (!string.IsNullOrEmpty(_billcreditnote.creditnoteno))
                        qb.AddParameter("billcreditnote.creditnoteno", _billcreditnote.creditnoteno, "=");

                    if (_billcreditnote.customerid > 0)
                        qb.AddParameter("billcreditnote.customerid", _billcreditnote.customerid, "=");
                    if (_billcreditnote.fromdate != DateTime.MinValue && _billcreditnote.todate != DateTime.MinValue)
                    {
                        qb.AddParameter("billcreditnote.creditnotedate", "datefrom", _billcreditnote.fromdate.Date, ">=");
                        qb.AddParameter("billcreditnote.creditnotedate", "dateto", _billcreditnote.todate.AddDays(1).Date, "<");
                    }

                    if(!string.IsNullOrEmpty(_billcreditnote.billno))
                    {
                        qb.AddInParamer("billcreditnote.id", "select billcreditnoteid from Billcreditnoteitems where billno = @billno");
                    }

                    qb.SortField = "billcreditnote.id desc";
                    command = qb.GetCommand(conn);
                    if(!string.IsNullOrEmpty(_billcreditnote.billno))
                        command.Parameters.AddWithValue("@billno", _billcreditnote.billno);
                }

                using (System.Data.IDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        Billcreditnote obillcreditnote = new Billcreditnote();
                        obillcreditnote.customerid = reader["customerid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["customerid"]);
                        obillcreditnote.customername = reader["customername"] == DBNull.Value ? "" : reader["customername"].ToString();
                        obillcreditnote.tax = reader["tax"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["tax"]);
                        obillcreditnote.amount = reader["amount"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["amount"]);
                        obillcreditnote.totalamount = reader["totalamount"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["totalamount"]);
                        obillcreditnote.adjustedtax = reader["adjustedtax"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["adjustedtax"]);
                        obillcreditnote.adjustedvalue = reader["adjustedvalue"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["adjustedvalue"]);
                        obillcreditnote.adjustedtotal = reader["adjustedtotal"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["adjustedtotal"]);
                        obillcreditnote.companyid = reader["companyid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["companyid"]);
                        obillcreditnote.creditnotedate = reader["creditnotedate"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(reader["creditnotedate"]);
                        obillcreditnote.creditnoteno = reader["creditnoteno"] == DBNull.Value ? "" : reader["creditnoteno"].ToString();
                        obillcreditnote.remarks = reader["remarks"] == DBNull.Value ? "" : reader["remarks"].ToString();
                        obillcreditnote.modifiedon = reader["modifiedon"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(reader["modifiedon"]);
                        obillcreditnote.createdon = reader["createdon"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(reader["createdon"]);
                        obillcreditnote.version = reader["version"] == DBNull.Value ? 0 : Convert.ToInt32(reader["version"]);
                        obillcreditnote.modifiedby = reader["modifiedby"] == DBNull.Value ? 0 : Convert.ToInt64(reader["modifiedby"]);
                        obillcreditnote.createdby = reader["createdby"] == DBNull.Value ? 0 : Convert.ToInt64(reader["createdby"]);
                        obillcreditnote.isactive = reader["isactive"] == DBNull.Value ? false : Convert.ToBoolean(reader["isactive"]);
                        obillcreditnote.id = reader["id"] == DBNull.Value ? 0 : Convert.ToInt64(reader["id"]);
                        obillcreditnote.returnreasonid = reader["returnreasonid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["returnreasonid"]);
                        obillcreditnote.rounding = reader["rounding"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["rounding"]);
                        obillcreditnote.locationid = reader["locationid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["locationid"]);
                        obillcreditnote.isinterstatesale = reader["isinterstatesale"] == DBNull.Value ? false : Convert.ToBoolean(reader["isinterstatesale"]);
                        obillcreditnote.voucherinfo_json = reader["voucherinfo"] == DBNull.Value ? null : reader["voucherinfo"].ToString();
                        obillcreditnote.mastersettlementid = reader["mastersettlementid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["mastersettlementid"]);
                        lst.Add(obillcreditnote);
                    }
                }
                if (_billcreditnote.id > 0 && lst.Count == 1)
                {
                    BillcreditnoteitemsDataService subBillcreditnoteitems = new BillcreditnoteitemsDataService() { Context = this.Context };
                    lst[0].Billcreditnoteitems = subBillcreditnoteitems.SearchAll(_billcreditnote.id);
                }
                conn.Close();
            }
            catch (Exception exp)
            {
                if (conn != null) conn.Close();
                WebAPI.Common.Logger.Instance(Context.OrganizationID).Log(exp, WebAPI.Common.Logger.LogType.Error, _billcreditnote);
            }
            return lst;
        }

        public BillcreditnotePrint Print(long id)
        {
            var returninvoice = Search(new BillcreditnoteCreteria()
            {
                id = id
            });

            BillcreditnotePrint ret = new BillcreditnotePrint();

            ret.companyid = returninvoice[0].companyid;
            ret.customerid = returninvoice[0].customerid;
            ret.creditnoteno = returninvoice[0].creditnoteno;
            ret.creditnotedate = returninvoice[0].creditnotedate;
            ret.adjustedvalue = returninvoice[0].adjustedvalue;
            ret.adjustedtotal = returninvoice[0].adjustedtotal;
            ret.adjustedtax = returninvoice[0].adjustedtax;
            ret.amount = returninvoice[0].amount;
            ret.tax = returninvoice[0].tax;
            ret.rounding = returninvoice[0].rounding;
            ret.Billcreditnoteitems = returninvoice[0].Billcreditnoteitems;
            ret.inwords = WebAPI.Data.Numberconvertor.rupees(ret.adjustedtotal);
            ret.isinterstatesale = returninvoice[0].isinterstatesale;

            if (ret.Billcreditnoteitems.Count == 1)
                ret.invoiceinfo = ret.Billcreditnoteitems.First().billno + " - " + ret.Billcreditnoteitems.First().billdate.ToString("dd-MM-yyyy");
            
            if(ret.Billcreditnoteitems.Count > 0)
            {
                var invno = ret.Billcreditnoteitems.Select(o => o.billno).Distinct();
                var invdate = ret.Billcreditnoteitems.Select(o => o.billdate.Date.ToString("dd/MM/yy")).Distinct();
                ret.invoicenos = string.Join(",", invno);
                ret.invoicedates = string.Join(",", invdate);
            }
            ret.remarks = returninvoice[0].remarks;

            if (returninvoice[0].createdby > 0)
            {
                UsersDataService usersDataService = new UsersDataService();
                var user = usersDataService.Search(new UsersCreteria()
                {
                    id = returninvoice[0].createdby
                });
                if (user.Count > 0)
                    ret.enteredby = user.First().title;
            }
            if (returninvoice[0].locationid > 0)
            {
                ReferencelistDataService referencelistDataService = new ReferencelistDataService() { Context = this.Context };
                var location = referencelistDataService.Search(new ReferencelistCreteria()
                {
                    id = returninvoice[0].locationid
                }).First();

                ret.location = new CompanyLocation(location);
                ret.location.name = location.name;
                ret.location.phonenumber = location.additionalinfo == null ? "" : location.additionalinfo.formula;
                ret.location.address = location.additionalinfo == null ? "" : location.additionalinfo.address;
                ret.location.addresslines = new List<CompanyLocation.Address>();
                if (!string.IsNullOrEmpty(ret.location.address))
                {
                    var addresslist = ret.location.address.Split(new string[] { "\r\n", "\r", "\n" }, StringSplitOptions.None);
                    addresslist.ToList().ForEach(e =>
                    {
                        ret.location.addresslines.Add(new CompanyLocation.Address()
                        {
                            value = e
                        });
                    });
                }
            }
            else
            {
                ret.location = new CompanyLocation();
                ret.location.name = "WAREHOUSE";
            }
            Data.CompanyDataService companyservice = new CompanyDataService() { Context = this.Context };
            companyservice.Context = this.Context;
            var comnanylist = companyservice.Search(new CompanyCreteria()
            {
                id = returninvoice[0].companyid
            });

            ret.company = comnanylist[0];
            ret.company.parseaddress();

            Data.CustomerDataService scompanyservice = new CustomerDataService() { Context = this.Context };
            var suppliercompany = scompanyservice.Search(new CustomerCreteria()
            {
                id = ret.customerid
            });
            if (suppliercompany.Count > 0)
            {
                ret.customer = suppliercompany[0];
                ret.customer.addresslines = new List<Customer.AddressLine>();
                var addresslist = ret.customer.address.Split(new string[] { "\r\n", "\r", "\n" }, StringSplitOptions.None);
                addresslist.ToList().ForEach(e =>
                {
                    ret.customer.addresslines.Add(new Customer.AddressLine()
                    {
                        value = e
                    });
                });
            }

            ret.taxlist = new List<TaxSplit>();
            foreach (var titem in ret.Billcreditnoteitems.GroupBy(e => e.taxpercentage))
            {
                TaxSplit taxitem = new TaxSplit();
                taxitem.name = titem.First().taxpercentage.ToString("N2");
                taxitem.isinterstatesale = ret.isinterstatesale;
                taxitem.totalamount = titem.Sum(e => e.adjustedvalue);
                if (taxitem.totalamount > 0)
                {
                    if (ret.isinterstatesale)
                    {
                        taxitem.igsttaxamount = titem.Sum(e => e.adjustedtax);
                        taxitem.cgsttaxamount = 0;
                        taxitem.sgsttaxamount = 0;
                    }
                    else
                    {
                        taxitem.cgsttaxamount = titem.Sum(e => e.adjustedtax) / 2;
                        taxitem.sgsttaxamount = titem.Sum(e => e.adjustedtax) / 2;
                        taxitem.cgsttaxpercentage = titem.First().taxpercentage / 2;
                        taxitem.sgsttaxpercentage = titem.First().taxpercentage / 2;
                        taxitem.secondtax = "SGST";
                    }
                }
                ret.taxlist.Add(taxitem);
            }

            return ret;
        }
        public Billcreditnote Insert(Billcreditnote oBillcreditnote)
        {
            NpgsqlTransaction transaction = null;
            try
            {
                var sequence = SequenceManager.Instance(Context.OrganizationID).Get("billcreditnote.creditnoteno",0,oBillcreditnote.companyid);
                using (NpgsqlConnection conn = CreateConnection())
                {
                    conn.Open();
                    transaction = conn.BeginTransaction();
                    NpgsqlCommand command = new NpgsqlCommand(SQL_Insert, conn, transaction);
                    oBillcreditnote.creditnoteno = sequence.GetNextNumber(this.Context,conn, transaction);

                    Insert(transaction, conn, oBillcreditnote);
                    BillcreditnoteitemsDataService subBillcreditnoteitems = new BillcreditnoteitemsDataService() { Context = this.Context };
                    foreach (var item in oBillcreditnote.Billcreditnoteitems)
                    {
                        item.billcreditnoteid = oBillcreditnote.id;
                        subBillcreditnoteitems.Insert(transaction, conn, oBillcreditnote, item);
                    }
                    transaction.Commit();
                    conn.Close();
                }
            }
            catch (Exception exp)
            {
                WebAPI.Common.Logger.Instance(Context.OrganizationID).Log(exp, WebAPI.Common.Logger.LogType.Error, oBillcreditnote);
            }

            return oBillcreditnote;
        }

        public Billcreditnote Insert(NpgsqlTransaction transaction, NpgsqlConnection conn, Billcreditnote oBillcreditnote)
        {

            NpgsqlCommand command = new NpgsqlCommand(SQL_Insert, conn, transaction);
            command.Parameters.Add("customerid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oBillcreditnote.customerid;
            command.Parameters.Add("tax", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oBillcreditnote.tax;
            command.Parameters.Add("amount", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oBillcreditnote.amount;
            command.Parameters.Add("totalamount", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oBillcreditnote.totalamount;
            command.Parameters.Add("adjustedtax", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oBillcreditnote.adjustedtax;
            command.Parameters.Add("adjustedvalue", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oBillcreditnote.adjustedvalue;
            command.Parameters.Add("adjustedtotal", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oBillcreditnote.adjustedtotal;
            command.Parameters.Add("companyid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oBillcreditnote.companyid;
            command.Parameters.Add("creditnotedate", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = oBillcreditnote.creditnotedate;
            command.Parameters.Add("creditnoteno", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oBillcreditnote.creditnoteno == null ? "" : oBillcreditnote.creditnoteno;
            command.Parameters.Add("remarks", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oBillcreditnote.remarks == null ? "" : oBillcreditnote.remarks;
            command.Parameters.Add("modifiedon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = DateTimeNow;
            command.Parameters.Add("createdon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = DateTimeNow;
            command.Parameters.Add("version", NpgsqlTypes.NpgsqlDbType.Integer).Value = oBillcreditnote.version;
            command.Parameters.Add("modifiedby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = Context.UserID;
            command.Parameters.Add("createdby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = Context.UserID;
            command.Parameters.Add("isactive", NpgsqlTypes.NpgsqlDbType.Boolean).Value = true;
            command.Parameters.Add("returnreasonid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oBillcreditnote.returnreasonid;
            command.Parameters.Add("rounding", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oBillcreditnote.rounding;
            command.Parameters.Add("locationid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oBillcreditnote.locationid;
            command.Parameters.Add("isinterstatesale", NpgsqlTypes.NpgsqlDbType.Boolean).Value = oBillcreditnote.isinterstatesale;
            command.Parameters.Add("voucherinfo", NpgsqlTypes.NpgsqlDbType.Json).Value = oBillcreditnote.voucherinfo_json == null ? "null" : oBillcreditnote.voucherinfo_json;
            command.Parameters.Add("mastersettlementid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oBillcreditnote.mastersettlementid;
            using (System.Data.IDataReader reader = command.ExecuteReader())
            {
                if (reader.Read())
                {
                    oBillcreditnote.id = Convert.ToInt64(reader[0].ToString());
                }
            }
            return oBillcreditnote;
        }

        public Billcreditnote Update(Billcreditnote oBillcreditnote)
        {

            NpgsqlTransaction transaction = null;
            try
            {
                using (NpgsqlConnection conn = CreateConnection())
                {
                    conn.Open();
                    transaction = conn.BeginTransaction();

                    Update(transaction, conn, oBillcreditnote);
                    BillcreditnoteitemsDataService subBillcreditnoteitems = new BillcreditnoteitemsDataService() { Context = this.Context };
                    subBillcreditnoteitems.Update(transaction, conn, oBillcreditnote,oBillcreditnote.Billcreditnoteitems, oBillcreditnote.id);
                    transaction.Commit();
                    conn.Close();
                }
            }
            catch (Exception exp)
            {
                WebAPI.Common.Logger.Instance(Context.OrganizationID).Log(exp, WebAPI.Common.Logger.LogType.Error, oBillcreditnote);
            }

            return oBillcreditnote;
        }

        public Billcreditnote Update(NpgsqlTransaction transaction, NpgsqlConnection conn, Billcreditnote oBillcreditnote)
        {

            NpgsqlCommand command = new NpgsqlCommand(SQL_Update, conn, transaction);
            command.Parameters.Add("customerid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oBillcreditnote.customerid;
            command.Parameters.Add("tax", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oBillcreditnote.tax;
            command.Parameters.Add("amount", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oBillcreditnote.amount;
            command.Parameters.Add("totalamount", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oBillcreditnote.totalamount;
            command.Parameters.Add("adjustedtax", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oBillcreditnote.adjustedtax;
            command.Parameters.Add("adjustedvalue", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oBillcreditnote.adjustedvalue;
            command.Parameters.Add("adjustedtotal", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oBillcreditnote.adjustedtotal;
            command.Parameters.Add("companyid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oBillcreditnote.companyid;
            command.Parameters.Add("creditnotedate", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = oBillcreditnote.creditnotedate;
            command.Parameters.Add("creditnoteno", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oBillcreditnote.creditnoteno == null ? "" : oBillcreditnote.creditnoteno;
            command.Parameters.Add("remarks", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oBillcreditnote.remarks == null ? "" : oBillcreditnote.remarks;
            command.Parameters.Add("modifiedon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = DateTimeNow;
            command.Parameters.Add("version", NpgsqlTypes.NpgsqlDbType.Integer).Value = oBillcreditnote.version;
            command.Parameters.Add("modifiedby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = Context.UserID;
            command.Parameters.Add("isactive", NpgsqlTypes.NpgsqlDbType.Boolean).Value = oBillcreditnote.isactive;
            command.Parameters.Add("id", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oBillcreditnote.id;
            command.Parameters.Add("returnreasonid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oBillcreditnote.returnreasonid;
            command.Parameters.Add("rounding", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oBillcreditnote.rounding;
            command.Parameters.Add("locationid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oBillcreditnote.locationid;
            command.Parameters.Add("isinterstatesale", NpgsqlTypes.NpgsqlDbType.Boolean).Value = oBillcreditnote.isinterstatesale;
            command.Parameters.Add("voucherinfo", NpgsqlTypes.NpgsqlDbType.Json).Value = oBillcreditnote.voucherinfo_json == null ? "null" : oBillcreditnote.voucherinfo_json;
            command.Parameters.Add("mastersettlementid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oBillcreditnote.mastersettlementid;
            using (System.Data.IDataReader reader = command.ExecuteReader())
            {
                if (reader.Read())
                {
                    oBillcreditnote.version = Convert.ToInt32(reader[0].ToString());
                }
            }

            return oBillcreditnote;
        }


        public bool Delete(Billcreditnote oBillcreditnote)
        {

            NpgsqlTransaction transaction = null;
            bool hasSuccessCount = false;
            try
            {
                using (NpgsqlConnection conn = CreateConnection())
                {
                    conn.Open();
                    transaction = conn.BeginTransaction();
                    hasSuccessCount = Delete(transaction, conn, oBillcreditnote);
                    BillcreditnoteitemsDataService subBillcreditnoteitems = new BillcreditnoteitemsDataService() { Context = this.Context };
                    subBillcreditnoteitems.DeleteAll(oBillcreditnote.id, oBillcreditnote.modifiedby, conn, transaction);
                    transaction.Commit();
                    conn.Close();
                }
            }
            catch (Exception exp)
            {
                WebAPI.Common.Logger.Instance(Context.OrganizationID).Log(exp, WebAPI.Common.Logger.LogType.Error, oBillcreditnote);

            }
            return hasSuccessCount;

        }

        public bool Delete(NpgsqlTransaction transaction, NpgsqlConnection conn, Billcreditnote oBillcreditnote)
        {

            NpgsqlCommand command = new NpgsqlCommand(SQL_Delete, conn, transaction);
            command.Parameters.Add("isactive", NpgsqlTypes.NpgsqlDbType.Boolean).Value = oBillcreditnote.isactive;
            command.Parameters.Add("modifiedby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oBillcreditnote.modifiedby;
            command.Parameters.Add("modifiedon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = oBillcreditnote.modifiedon;
            command.Parameters.Add("version", NpgsqlTypes.NpgsqlDbType.Integer).Value = oBillcreditnote.version;
            command.Parameters.Add("id", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oBillcreditnote.id;
            int successCount = command.ExecuteNonQuery();

            return successCount > 0;
        }

    }

}
