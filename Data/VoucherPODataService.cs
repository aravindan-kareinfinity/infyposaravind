using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using InfyPOS.Models;
using InfyPOS.Common;
using Npgsql;
using InfyPOS.Tally;
using Newtonsoft.Json;


namespace InfyPOS.Data
{
    public class VoucherPODataService : SAASData
    {
        const string SQL_Search = @"select voucher.id, vouchertype,vouchertypeid,purchasetypeid, voucherdate, voucherno,bwise, narration, amount, 
                                    voucher.isactive, voucher.createdby, voucher.modifiedby,sourcetype, voucher.version, voucher.createdon, 
                                    voucher.modifiedon,adjustmentcredit,adjustmentledgerid,locationid,voucher.employeeid,voucher.companyid,
                                    attributes,muser.employeeid memployeeid,cuser.employeeid cemployeeid,voucher.uniqueuid
                                    from voucherpo voucher
                                    left outer join users cuser on cuser.id = voucher.createdby 
                                    left outer join users muser on muser.id = voucher.modifiedby
                                    where voucher.id=@id";

        const string SQL_Insert = @"INSERT INTO public.voucherpo(vouchertype,vouchertypeid,purchasetypeid, voucherdate, voucherno,bwise, narration, amount, isactive, createdby, modifiedby, version, createdon, modifiedon, adjustmentcredit,adjustmentledgerid,locationid,employeeid,sourcetype,attributes,companyid,uniqueuid)
                                    VALUES(@vouchertype,@vouchertypeid,@purchasetypeid, @voucherdate, @voucherno,@bwise, @narration, @amount, @isactive, @createdby, @modifiedby, @version, @createdon, @modifiedon, @adjustmentcredit, @adjustmentledgerid,@locationid,@employeeid,@sourcetype,@attributes,@companyid,@uniqueuid) RETURNING id";

        const string SQL_Update = @"UPDATE public.voucherpo SET vouchertype=@vouchertype,vouchertypeid=@vouchertypeid,purchasetypeid=@purchasetypeid, attributes=@attributes, voucherdate=@voucherdate,locationid=@locationid,employeeid=@employeeid, voucherno=@voucherno,bwise=@bwise,
                                    narration=@narration, amount=@amount, modifiedby=@modifiedby, version=@version+1,modifiedon=@modifiedon, adjustmentcredit=@adjustmentcredit,companyid=@companyid,
                                    adjustmentledgerid=@adjustmentledgerid,sourcetype=@sourcetype where id=@id and version=@version RETURNING version";


        const string SQL_Delete = @"update public.voucherpo set isactive=false,version=@version+1,modifiedby=@modifiedby,modifiedon=@modifiedon where id=@id and version=@version";

        public List<VoucherDetail> Search(VoucherCreteria _voucher)
        {
            List<VoucherDetail> lst = new List<VoucherDetail>();
            NpgsqlConnection conn = CreateConnection();
            try
            {
                conn.Open();

                NpgsqlCommand command = null;
                if (_voucher.id != 0)
                {
                    command = GetCommand(SQL_Search, conn);
                    command.Parameters.Add("id", NpgsqlTypes.NpgsqlDbType.Bigint).Value = _voucher.id;
                }
                else
                {
                    QueryBuilder qb = new QueryBuilder();
                    qb.CurrentQuery = SQL_Search.Substring(0, SQL_Search.IndexOf("where"));
                    qb.AddParameter("voucher.isactive", true, "=");


                    if (!string.IsNullOrEmpty(_voucher.voucherno))
                    {
                        qb.AddParameter("voucherno", _voucher.voucherno, "=");
                    }
                    else if (_voucher.cashposting)
                    {
                        qb.AddParameter("vouchertype", 4, "=");
                        qb.AddInParamer("sourcetype", "3,4,5");
                        qb.AddParameter("voucherdate", "fromdate", _voucher.voucherdate.Date, ">=");
                        qb.AddParameter("voucherdate", "todate", _voucher.voucherdate.Date.AddDays(1), "<");
                        if (_voucher.locationid > 0)
                            qb.AddParameter("locationid", _voucher.locationid, "=");
                    }
                    else
                    {

                        if (_voucher.fromdate.Year > 2000 && _voucher.todate.Year > 2000)
                        {
                            qb.AddParameter("voucherdate", "fromdate", _voucher.fromdate.Date, ">=");
                            qb.AddParameter("voucherdate", "todate", _voucher.todate.Date.AddDays(1), "<");
                        }
                        else if (_voucher.voucherdate.Year > 2000)
                        {
                            qb.AddParameter("voucherdate", "fromdate", _voucher.voucherdate.Date, ">=");
                            qb.AddParameter("voucherdate", "todate", _voucher.voucherdate.Date.AddDays(1), "<");
                        }

                        //if (_voucher.attributes != null && _voucher.attributes.date1.Year > 2000)
                        //{
                        //    qb.AddParameter("cast(attributes->> 'date1' as timestamp without timezone)", "fromdate", _voucher.attributes.date1.Date, ">=");
                        //    qb.AddParameter("cast(attributes->> 'date1' as timestamp without timezone)", "todate", _voucher.attributes.date1.Date.AddDays(1), "<");
                        //}


                        if (_voucher.vouchertype == 99)
                        {
                            qb.AddParameter("vouchertype", 4, "=");
                            qb.AddInParamer("sourcetype", "3,4,5");
                        }
                        else
                        {
                            if (_voucher.attributes != null &&
                               _voucher.attributes.counterid > 0)
                            {
                                qb.AddParameter("cast(attributes->>'counterid' as bigint)", "counterid",
                                    _voucher.attributes.counterid, "=");
                            }
                            if (_voucher.sourcetype > 0)
                                qb.AddParameter("sourcetype", _voucher.sourcetype, "=");

                            if (_voucher.vouchertype > 0)
                                qb.AddParameter("vouchertype", _voucher.vouchertype, "=");

                            if (_voucher.vouchertypeid > 0)
                                qb.AddParameter("vouchertypeid", _voucher.vouchertypeid, "=");
                            if (_voucher.purchasetypeid > 0)
                                qb.AddParameter("purchasetypeid", _voucher.purchasetypeid, "=");
                        }

                        if (_voucher.locationid > 0)
                            qb.AddParameter("locationid", _voucher.locationid, "=");

                        if (_voucher.employeeid > 0)
                            qb.AddParameter("voucher.employeeid", _voucher.employeeid, "=");

                        if (_voucher.companyid > 0)
                            qb.AddParameter("voucher.companyid", _voucher.companyid, "=");

                        if (_voucher.ledgerid > 0)
                            qb.AddParameter("adjustmentledgerid", _voucher.ledgerid, "=");

                        if (_voucher.adjustmentledgerid > 0)
                            qb.AddParameter("adjustmentledgerid", _voucher.adjustmentledgerid, "=");


                        if (_voucher.filtertype == 2)
                        {

                            qb.CurrentQuery = @"with duplicatelist as
                            (
                            select id,adjustmentledgerid,amount,attributes->>'invoicedate' invoicedate,
                            row_number() OVER (PARTITION BY adjustmentledgerid,amount,attributes->>'invoicedate' ORDER BY id ASC) rownumber
                            from voucher 
                            where isactive=true
                            and voucherdate>=@filterfrom and voucherdate<@filterto
                            ),filteredduplicate as
                            (
                                select adup.id from duplicatelist dup
                                inner join duplicatelist adup on adup.adjustmentledgerid = dup.adjustmentledgerid and 
                                adup.invoicedate = dup.invoicedate and adup.amount = dup.amount
                                where dup.rownumber>1
                            )" + qb.CurrentQuery;
                            qb.Add2Query(" voucher.id in (select id from filteredduplicate)");

                        }
                        if (_voucher.filtertype == 9)
                        {
                            QueryBuilder qbfilter = new QueryBuilder();
                            var CurrentQuery = "";
                            if (_voucher.filterledgerid > 0)
                            {
                                CurrentQuery = "select id from voucher where isactive=true and adjustmentledgerid=" + _voucher.filterledgerid + " and voucherdate>=@filterfrom and voucherdate<@filterto";
                                if (_voucher.amountfrom > 0)
                                {
                                    CurrentQuery += " and voucher.amount>=@amountfrom";
                                }
                                if (_voucher.amountfrom > 0)
                                {
                                    CurrentQuery += " and voucher.amount<=@amountto";
                                }
                                CurrentQuery += " union all select voucherid from voucheritems inner join voucher on voucher.id = voucheritems.voucherid  and voucherdate>=@filterfrom and voucherdate<@filterto";
                                CurrentQuery += " and voucheritems.ledgerid=" + _voucher.filterledgerid;

                                if (_voucher.amountfrom > 0)
                                {
                                    CurrentQuery += " and voucheritems.amount>=@amountfrom";
                                }
                                if (_voucher.amountfrom > 0)
                                {
                                    CurrentQuery += " and voucheritems.amount<=@amountto";
                                }
                            }
                            else if (_voucher.amountfrom > 0 || _voucher.amountto > 0)
                            {
                                CurrentQuery = "select id from voucher where isactive=true  and voucherdate>=@filterfrom and voucherdate<@filterto";
                                if (_voucher.amountfrom > 0)
                                {
                                    CurrentQuery += " and voucher.amount>=@amountfrom";
                                }
                                if (_voucher.amountfrom > 0)
                                {
                                    CurrentQuery += " and voucher.amount<=@amountto";
                                }
                                qbfilter.CurrentQuery += " union all select voucherid from voucheritems inner join voucher on voucher.id = voucheritems.voucherid  and voucherdate>=@filterfrom and voucherdate<@filterto";
                                if (_voucher.amountfrom > 0)
                                {
                                    CurrentQuery += " and voucheritems.amount>=@amountfrom";
                                }
                                if (_voucher.amountfrom > 0)
                                {
                                    CurrentQuery += " and voucheritems.amount<=@amountto";
                                }
                            }

                            if (CurrentQuery != "")
                            {
                                qb.Add2Query(" voucher.id in (" + CurrentQuery + ")");
                            }
                        }
                    }
                    command = qb.GetCommand(conn);
                    command.Parameters.Add("amountfrom", NpgsqlTypes.NpgsqlDbType.Numeric).Value = _voucher.amountfrom;
                    command.Parameters.Add("amountto", NpgsqlTypes.NpgsqlDbType.Numeric).Value = _voucher.amountto;
                    command.Parameters.Add("filterfrom", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = _voucher.fromdate;
                    command.Parameters.Add("filterto", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = _voucher.todate.AddDays(1);
                }

                using (System.Data.IDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        VoucherDetail ovoucher = new VoucherDetail();
                        ovoucher.id = reader["id"] == DBNull.Value ? 0 : Convert.ToInt64(reader["id"]);
                        ovoucher.adjustmentledgerid = reader["adjustmentledgerid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["adjustmentledgerid"]);
                        ovoucher.adjustmentcredit = reader["adjustmentcredit"] == DBNull.Value ? false : Convert.ToBoolean(reader["adjustmentcredit"]);
                        ovoucher.vouchertype = reader["vouchertype"] == DBNull.Value ? 0 : Convert.ToInt64(reader["vouchertype"]);
                        ovoucher.vouchertypeid = reader["vouchertypeid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["vouchertypeid"]);
                        ovoucher.purchasetypeid = reader["purchasetypeid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["purchasetypeid"]);
                        ovoucher.voucherdate = reader["voucherdate"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(reader["voucherdate"]);
                        ovoucher.uniqueuid = reader["uniqueuid"] == DBNull.Value ? "" : reader["uniqueuid"].ToString();
                        ovoucher.attributes_json = reader["attributes"] == DBNull.Value ? "null" : reader["attributes"].ToString();

                        ovoucher.voucherno = reader["voucherno"] == DBNull.Value ? "" : reader["voucherno"].ToString();
                        ovoucher.narration = reader["narration"] == DBNull.Value ? "" : reader["narration"].ToString();
                        ovoucher.amount = reader["amount"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["amount"]);
                        //ovoucher.opening = reader["opening"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["opening"]);
                        ovoucher.isactive = reader["isactive"] == DBNull.Value ? false : Convert.ToBoolean(reader["isactive"]);
                        ovoucher.bwise = reader["bwise"] == DBNull.Value ? 0 : Convert.ToInt32(reader["bwise"]);

                        ovoucher.createdby = reader["createdby"] == DBNull.Value ? 0 : Convert.ToInt64(reader["createdby"]);
                        ovoucher.locationid = reader["locationid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["locationid"]);
                        ovoucher.employeeid = reader["employeeid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["employeeid"]);
                        ovoucher.companyid = reader["companyid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["companyid"]);
                        ovoucher.sourcetype = reader["sourcetype"] == DBNull.Value ? 0 : Convert.ToInt32(reader["sourcetype"]);

                        ovoucher.modifiedby = reader["modifiedby"] == DBNull.Value ? 0 : Convert.ToInt64(reader["modifiedby"]);
                        ovoucher.version = reader["version"] == DBNull.Value ? 0 : Convert.ToInt32(reader["version"]);
                        ovoucher.createdon = reader["createdon"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(reader["createdon"]);
                        ovoucher.modifiedon = reader["modifiedon"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(reader["modifiedon"]);

                        ovoucher.cempid = reader["cemployeeid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["cemployeeid"]);
                        ovoucher.mempid = reader["memployeeid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["memployeeid"]);

                        lst.Add(ovoucher);
                    }
                }

                if (_voucher.filtertype == 2)
                {
                    lst = lst.OrderBy(x => x.adjustmentledgerid).ThenBy(x => x.voucherdate).ToList();
                }
                else
                {
                    lst = lst.OrderBy(x => x.voucherdate).ToList();
                }

                var empids = lst.ConvertAll(x => x.cempid);
                empids.AddRange(lst.ConvertAll(y => y.mempid));


                EmployeeDataService employeeDataService = new EmployeeDataService() { Context = this.Context };
                var names = employeeDataService.SearchHeader(new EmployeeCreteria()
                {
                    ids = empids.Distinct().ToList()
                });

                lst.ForEach(x =>
                {
                    if (names.Exists(y => y.id == x.cempid))
                    {
                        x.cemp = names.Find(y => y.id == x.cempid).name;
                    }
                    if (names.Exists(y => y.id == x.mempid))
                    {
                        x.memp = names.Find(y => y.id == x.mempid).name;
                    }
                });



                if (_voucher.id > 0 || lst.Count == 1 || _voucher.showdetail)
                {
                    VoucherPOitemsDataService subVoucheritems = new VoucherPOitemsDataService() { Context = this.Context };
                    subVoucheritems.Context = Context;
                    foreach (var item in lst)
                    {
                        item.voucheritems = subVoucheritems.SearchAll(item.id);
                    }

                    if (_voucher.id > 0 && lst.Count == 1 &&
                        (lst[0].vouchertype == 3 || lst[0].vouchertype == 4))
                    {
                        VoucheritembillwiseDataService voucheritembillwiseDataService = new VoucheritembillwiseDataService() { Context = this.Context };
                        voucheritembillwiseDataService.Context = Context;
                        var billwise = voucheritembillwiseDataService.Search(new VoucheritembillwiseCreteria()
                        {
                            voucherid = _voucher.id
                        });
                        if (lst[0].attributes == null) lst[0].attributes = new Voucher.voucharattributesExtension();
                        lst[0].attributes.adjustedvouchervalue = billwise.Sum(x => x.adjusted);

                        lst[0].attributes.adjustedvouchers = string.Join(",", voucheritembillwiseDataService.Search(new VoucheritembillwiseCreteria()
                        {
                            sourceitemids = billwise.ConvertAll(x => x.id)
                        }).ConvertAll(x => x.referenceno));
                    }
                }
                conn.Close();
            }
            catch (Exception exp)
            {
                if (conn != null) conn.Close();
                WebAPI.Common.Logger.Instance(Context.OrganizationID).Log(exp, WebAPI.Common.Logger.LogType.Error, _voucher);
            }
            return lst;
        }

        public Voucher Insert(Voucher oVoucher)
        {
            if (InfyPOS.Services.ConfigManager.Instance(Context.OrganizationID).Config.fin_lockdays > 0)
            {
                if (DateTimeNow.Subtract(oVoucher.voucherdate).TotalDays > InfyPOS.Services.ConfigManager.Instance(Context.OrganizationID).Config.fin_lockdays)
                {
                    if (!Context.HasPermission(PermissionSet.advfinance, InfyPOS.Common.Action.View))
                    {
                        throw new Exception("You don't have permission to create old vouchers beyond the limit");
                    }
                }
            }

            NpgsqlTransaction transaction = null;
            try
            {

                var vouchertype = oVoucher.vouchertype;

                if (oVoucher.adjustmentledgerid <= 0)
                    throw new Exception("Please select the party ledger");


                if (oVoucher.voucheritems.Exists(x => x.ledgerid <= 0))
                    throw new Exception("Please enter appropriate ledgers");


                var sequence = SequenceManager.Instance(Context.OrganizationID).Get("voucher.voucherno",
                    Services.ConfigManager.Instance(Context.OrganizationID).Config.system_voucherno_location ? oVoucher.locationid : 0,
                    Services.ConfigManager.Instance(Context.OrganizationID).Config.system_voucherno_company ? oVoucher.companyid : 0,
                    Services.ConfigManager.Instance(Context.OrganizationID).Config.system_voucherno_type ? vouchertype : 0);

                using (NpgsqlConnection conn = CreateConnection())
                {
                    conn.Open();
                    transaction = conn.BeginTransaction();
                    NpgsqlCommand command = GetCommand(SQL_Insert, conn, transaction);
                    oVoucher.voucherno = sequence.GetNextNumber(Context, conn, transaction);


                    oVoucher.attributes.hasgst = oVoucher.voucheritems != null &&
                        oVoucher.voucheritems.Count > 0 &&
                        oVoucher.voucheritems.Exists(x => x.hastax && x.attributes != null && x.attributes.hasgst);

                    Insert(transaction, conn, oVoucher);
                    VoucherPOitemsDataService subVoucheritems = new VoucherPOitemsDataService() { Context = this.Context };
                    subVoucheritems.Context = Context;

                    //if (oVoucher.voucheritems.Exists(x => x.attributes != null &&
                    //x.attributes.ledgertdsid > 0))
                    //{
                    //    var tdsledgersource = oVoucher.voucheritems.Find(x => x.attributes != null &&
                    //                        x.attributes.ledgertdsid > 0);
                    //    if (oVoucher.voucheritems.Exists(x => x.ledgerid == tdsledgersource.attributes.ledgertdsid))
                    //    {
                    //        oVoucher.voucheritems.Find(x => x.ledgerid == tdsledgersource.attributes.ledgertdsid).sledgerid = tdsledgersource.ledgerid;
                    //    }
                    //}

                    foreach (var item in oVoucher.voucheritems)
                    {
                        item.voucherid = oVoucher.id;
                        //item.iscredit = !oVoucher.adjustmentcredit;
                        subVoucheritems.Insert(transaction, conn, oVoucher, item);
                    }

                    transaction.Commit();
                    conn.Close();
                }
            }
            catch (Exception exp)
            {
                WebAPI.Common.Logger.Instance(Context.OrganizationID).Log(exp, WebAPI.Common.Logger.LogType.Error, oVoucher);

            }

            return oVoucher;
        }

        public Voucher Insert(NpgsqlTransaction transaction, NpgsqlConnection conn, Voucher oVoucher)
        {

            NpgsqlCommand command = GetCommand(SQL_Insert, conn, transaction);
            command.Parameters.Add("uniqueuid", NpgsqlTypes.NpgsqlDbType.Uuid).Value = string.IsNullOrEmpty(oVoucher.uniqueuid) ? Guid.NewGuid().ToString() : oVoucher.uniqueuid;
            command.Parameters.Add("vouchertype", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oVoucher.vouchertype;
            command.Parameters.Add("purchasetypeid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oVoucher.purchasetypeid;
            command.Parameters.Add("vouchertypeid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oVoucher.vouchertypeid;
            command.Parameters.Add("adjustmentledgerid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oVoucher.adjustmentledgerid;

            command.Parameters.Add("companyid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oVoucher.companyid;
            command.Parameters.Add("locationid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oVoucher.locationid;
            command.Parameters.Add("employeeid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oVoucher.employeeid;
            command.Parameters.Add("sourcetype", NpgsqlTypes.NpgsqlDbType.Integer).Value = oVoucher.sourcetype;

            command.Parameters.Add("adjustmentcredit", NpgsqlTypes.NpgsqlDbType.Boolean).Value = oVoucher.adjustmentcredit;
            command.Parameters.Add("voucherdate", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = oVoucher.voucherdate;

            command.Parameters.Add("attributes", NpgsqlTypes.NpgsqlDbType.Json).Value = oVoucher.attributes_json;
            command.Parameters.Add("voucherno", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oVoucher.voucherno == null ? "" : oVoucher.voucherno;
            command.Parameters.Add("narration", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oVoucher.narration == null ? "" : oVoucher.narration;
            command.Parameters.Add("amount", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oVoucher.amount;
            command.Parameters.Add("bwise", NpgsqlTypes.NpgsqlDbType.Integer).Value = oVoucher.bwise;
            //command.Parameters.Add("opening", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oVoucher.opening;
            command.Parameters.Add("isactive", NpgsqlTypes.NpgsqlDbType.Boolean).Value = oVoucher.isactive;
            command.Parameters.Add("createdby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = Context.UserID;
            command.Parameters.Add("modifiedby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = Context.UserID;
            command.Parameters.Add("version", NpgsqlTypes.NpgsqlDbType.Integer).Value = oVoucher.version;
            command.Parameters.Add("createdon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = DateTimeNow;
            command.Parameters.Add("modifiedon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = DateTimeNow;
            using (System.Data.IDataReader reader = command.ExecuteReader())
            {
                if (reader.Read())
                {
                    oVoucher.id = Convert.ToInt64(reader[0].ToString());
                }
            }
            return oVoucher;
        }

        public Voucher Update(Voucher oVoucher)
        {
            NpgsqlTransaction transaction = null;
            try
            {

               
                using (NpgsqlConnection conn = CreateConnection())
                {
                    conn.Open();
                    transaction = conn.BeginTransaction();

                    if (oVoucher.attributes == null)
                        oVoucher.attributes = new Voucher.voucharattributesExtension();

                    oVoucher.attributes.hasgst = oVoucher.voucheritems != null &&
                    oVoucher.voucheritems.Count > 0 &&
                    oVoucher.voucheritems.Exists(x => x.hastax && x.attributes != null && x.attributes.hasgst);

                    Update(transaction, conn, oVoucher);

                    VoucherPOitemsDataService subVoucheritems = new VoucherPOitemsDataService() { Context = this.Context };
                    subVoucheritems.Context = Context;
                    foreach (var item in oVoucher.voucheritems)
                    {
                        //item.iscredit = !oVoucher.adjustmentcredit;
                        item.voucherid = oVoucher.id;
                    }
                    if (oVoucher.voucheritems.Exists(x => x.attributes != null &&
                    x.attributes.ledgertdsid > 0))
                    {
                        var tdsledgersource = oVoucher.voucheritems.Find(x => x.attributes != null &&
                                            x.attributes.ledgertdsid > 0);
                        if (oVoucher.voucheritems.Exists(x => x.ledgerid == tdsledgersource.attributes.ledgertdsid))
                        {
                            oVoucher.voucheritems.Find(x => x.ledgerid == tdsledgersource.attributes.ledgertdsid).sledgerid = tdsledgersource.ledgerid;
                        }
                    }
                    subVoucheritems.Update(transaction, conn, oVoucher.voucheritems, oVoucher);
                    
                    transaction.Commit();
                    conn.Close();
                }
            }
            catch (Exception exp)
            {
                WebAPI.Common.Logger.Instance(Context.OrganizationID).Log(exp, WebAPI.Common.Logger.LogType.Error, oVoucher);

            }

            return oVoucher;
        }

        public Voucher Update(NpgsqlTransaction transaction, NpgsqlConnection conn, Voucher oVoucher)
        {
            if (InfyPOS.Services.ConfigManager.Instance(Context.OrganizationID).Config.fin_lockdays > 0)
            {
                if (DateTimeNow.Subtract(oVoucher.voucherdate).TotalDays > InfyPOS.Services.ConfigManager.Instance(Context.OrganizationID).Config.fin_lockdays)
                {
                    if (!Context.HasPermission(PermissionSet.advfinance, InfyPOS.Common.Action.View))
                    {
                        throw new Exception("You don't have permission to update old vouchers");
                    }
                }
            }

            NpgsqlCommand command = GetCommand(SQL_Update, conn, transaction);
            command.Parameters.Add("id", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oVoucher.id;
            command.Parameters.Add("vouchertype", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oVoucher.vouchertype;
            command.Parameters.Add("purchasetypeid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oVoucher.purchasetypeid;
            command.Parameters.Add("vouchertypeid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oVoucher.vouchertypeid;
            command.Parameters.Add("adjustmentledgerid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oVoucher.adjustmentledgerid;
            command.Parameters.Add("adjustmentcredit", NpgsqlTypes.NpgsqlDbType.Boolean).Value = oVoucher.adjustmentcredit;
            command.Parameters.Add("companyid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oVoucher.companyid;
            command.Parameters.Add("locationid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oVoucher.locationid;
            command.Parameters.Add("employeeid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oVoucher.employeeid;
            command.Parameters.Add("sourcetype", NpgsqlTypes.NpgsqlDbType.Integer).Value = oVoucher.sourcetype;
            //command.Parameters.Add("opening", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oVoucher.opening;
            command.Parameters.Add("voucherdate", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = oVoucher.voucherdate;
            command.Parameters.Add("voucherno", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oVoucher.voucherno == null ? "" : oVoucher.voucherno;
            command.Parameters.Add("narration", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oVoucher.narration == null ? "" : oVoucher.narration;
            command.Parameters.Add("attributes", NpgsqlTypes.NpgsqlDbType.Json).Value = oVoucher.attributes_json;
            command.Parameters.Add("bwise", NpgsqlTypes.NpgsqlDbType.Integer).Value = oVoucher.bwise;
            command.Parameters.Add("amount", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oVoucher.amount;
            command.Parameters.Add("isactive", NpgsqlTypes.NpgsqlDbType.Boolean).Value = oVoucher.isactive;
            command.Parameters.Add("createdby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oVoucher.createdby;
            command.Parameters.Add("modifiedby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = Context.UserID;
            command.Parameters.Add("version", NpgsqlTypes.NpgsqlDbType.Integer).Value = oVoucher.version;
            command.Parameters.Add("createdon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = oVoucher.createdon;
            command.Parameters.Add("modifiedon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = DateTimeNow;
            using (System.Data.IDataReader reader = command.ExecuteReader())
            {
                if (reader.Read())
                {
                    oVoucher.version = Convert.ToInt32(reader[0].ToString());
                }
            }
            return oVoucher;
        }

        public bool Delete(Voucher oVoucher)
        {

            NpgsqlTransaction transaction = null;
            bool hasSuccessCount = false;
            try
            {
                using (NpgsqlConnection conn = CreateConnection())
                {
                    conn.Open();
                    transaction = conn.BeginTransaction();
                    hasSuccessCount = Delete(transaction, conn, oVoucher);
                    VoucherPOitemsDataService subVoucheritems = new VoucherPOitemsDataService() { Context = this.Context };
                    subVoucheritems.Context = Context;
                    subVoucheritems.DeleteAll(oVoucher.id, oVoucher.modifiedby, conn, transaction);
                    transaction.Commit();
                    conn.Close();
                }
            }
            catch (Exception exp)
            {
                WebAPI.Common.Logger.Instance(Context.OrganizationID).Log(exp, WebAPI.Common.Logger.LogType.Error, oVoucher);
            }
            return hasSuccessCount;

        }

        public VoucherPrint PrintVoucher(long voucherid)
        {
            List<vouchertype> VoucherTypes = new List<vouchertype>(new vouchertype[] {
               new vouchertype (){ id = -1, name = "Payment Voucher" },
               new vouchertype (){ id=-2, name= "Receipt Voucher" },
               new vouchertype (){ id=-3, name= "Purchase Voucher" },
               new vouchertype (){ id=-4, name= "Sales Voucher" },
               new vouchertype (){ id=-5, name= "Credit Note" },
               new vouchertype (){ id=-6, name= "Debit Note" },
               new vouchertype (){ id=-7, name= "Contra CR" },
               new vouchertype (){ id=-8, name= "Journal CR" },
               new vouchertype (){ id=-9, name= "Cash Deposit" },
               new vouchertype (){ id=-10, name= "Cash Widhrawal" },
               new vouchertype (){ id=-11, name= "Main Cash to Petty Cash" },
               new vouchertype (){ id=-12, name= "Petty Cash to Main Cash" },
               new vouchertype (){ id=-13, name= "Cash Counter Expense" },
               new vouchertype (){ id=-14, name= "Advance Payment Voucher" },
               new vouchertype() { id = -15, name = "Petty Cash Expense" },
               new vouchertype() { id = -33, name = "Purchase Order" },
               new vouchertype() { id = -16, name = "Journal DR" },
               new vouchertype (){ id=-17, name= "Contra DR" }});

            ReferencelistDataService referencelistDataService = new ReferencelistDataService() { Context = this.Context };
            var vouchertypes = referencelistDataService.SearchList(new ReferencelistCreteria() { type = "VOUCHERTYPE" });
            if (vouchertypes != null && vouchertypes.Count > 0)
            {
                vouchertypes.ForEach(x =>
                {
                    VoucherTypes.Add(new vouchertype()
                    {
                        id = x.id,
                        name = x.name
                    });
                });
            }

            VoucherPrint ovoucher = new VoucherPrint()
            {
                childledgers = new List<VoucherPrint.VoucherItemPrint>(),
                taxledgers = new List<VoucherPrint.VoucherItemPrint>()
            };
            List<VoucherPrint.VoucherItemPrint> lstvoucheritem = new List<VoucherPrint.VoucherItemPrint>();

            const string SQL_Voucher = @"SELECT voucher.id, vouchertype,vouchertypeid, voucherdate, voucherno, narration, 
                                    voucher.bwise, amount,ledger.typeid typeid,cast(ledger.attributes->>'hasbank' as boolean) hasbank,ledger.bankid bankid,
                                    adjustmentcredit, adjustmentledgerid, ledger.displayname,voucher.locationid,
                                    voucher.companyid,voucher.narration, voucher.createdon, voucher.modifiedon,
                                    voucher.adjustmentcredit,voucher.locationid,voucher.employeeid,voucher.companyid,
                                    voucher.attributes,voucher.modifiedby,voucher.createdby,referencelist.name purchasetype
                                    FROM voucherpo voucher 
                                    INNER JOIN ledger ON voucher.adjustmentledgerid = ledger.id 
                                    LEFT OUTER JOIN referencelist on referencelist.id = purchasetypeid
                                    WHERE voucher.isactive = true AND voucher.id = @id";

            const string SQL_Voucheritem = @"SELECT amount, iscredit, ledger.displayname,
                                    cast(voucheritems.attributes->>'hasgst' as boolean) hasgst,
                                    voucheritems.rate,voucheritems.qty,ledger.taxpercentage,
                                    voucheritems.taxable,ledger.taxmode,ledger.typeid,
                                    cast(ledger.attributes->>'issupplier' as boolean) issupplier,ledger.supplierid,
                                    cast(ledger.attributes->>'hasbank' as boolean) hasbank,
                                    ledger.hsncode,ledger.bankid,voucheritems.ledgerid
                                    FROM voucherpoitems voucheritems 
                                    INNER JOIN ledger ON voucheritems.ledgerid = ledger.id
                                    WHERE voucherid=@voucherid AND voucheritems.isactive = true";

            NpgsqlConnection conn = CreateConnection();
            try
            {
                conn.Open();

                NpgsqlCommand command = null;
                command = GetCommand(SQL_Voucher, conn);
                command.Parameters.Add("id", NpgsqlTypes.NpgsqlDbType.Bigint).Value = voucherid;
                using (System.Data.IDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        ovoucher.ispo = true;
                        ovoucher.vouchertype = reader["vouchertype"] == DBNull.Value ? 0 : Convert.ToInt64(reader["vouchertype"]);
                        ovoucher.vouchertypeid = reader["vouchertypeid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["vouchertypeid"]);
                        if (ovoucher.vouchertypeid > 0)
                            ovoucher.displaytitle = VoucherTypes.Find(e => e.id == ovoucher.vouchertypeid).name+" ( Purchase Order )";
                        else
                            ovoucher.displaytitle = VoucherTypes.Find(e => e.id == ovoucher.vouchertype * -1).name + " ( Purchase Order )";
                        ovoucher.voucherdate = reader["voucherdate"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(reader["voucherdate"]);
                        ovoucher.voucherno = reader["voucherno"] == DBNull.Value ? "" : reader["voucherno"].ToString();
                        ovoucher.narration = reader["narration"] == DBNull.Value ? "" : reader["narration"].ToString();
                        ovoucher.purchasetype = reader["purchasetype"] == DBNull.Value ? "" : reader["purchasetype"].ToString();
                        ovoucher.amount = reader["amount"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["amount"]);
                        ovoucher.adjustmentledgerid = reader["adjustmentledgerid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["adjustmentledgerid"]);
                        ovoucher.companyid = reader["companyid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["companyid"]);
                        ovoucher.locationid = reader["locationid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["locationid"]);
                        ovoucher.bwise = reader["bwise"] == DBNull.Value ? 0 : Convert.ToInt32(reader["bwise"]);
                        ovoucher.attributes_json = reader["attributes"] == DBNull.Value ? "null" : reader["attributes"].ToString();
                        ovoucher.employeeid = reader["employeeid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["employeeid"]);
                        ovoucher.modifiedby = reader["modifiedby"] == DBNull.Value ? 0 : Convert.ToInt64(reader["modifiedby"]);
                        ovoucher.createdby = reader["createdby"] == DBNull.Value ? 0 : Convert.ToInt64(reader["createdby"]);
                        ovoucher.createdon = reader["createdon"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(reader["createdon"]);
                        ovoucher.modifiedon = reader["modifiedon"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(reader["modifiedon"]);
                        ovoucher.adjustmentcredit = reader["adjustmentcredit"] == DBNull.Value ? false : Convert.ToBoolean(reader["adjustmentcredit"]);
                        ovoucher.adjustmentledgertypeid = reader["typeid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["typeid"]);
                        if (reader["hasbank"] != DBNull.Value && Convert.ToBoolean(reader["hasbank"]))
                        {
                            ovoucher.bankid = reader["bankid"] == DBNull.Value ? 0 : Convert.ToInt32(reader["bankid"]);
                        }

                        VoucherPrint.VoucherItemPrint item = new VoucherPrint.VoucherItemPrint();
                        item.name = reader["displayname"] == DBNull.Value ? "" : reader["displayname"].ToString();
                        bool iscredit = reader["adjustmentcredit"] == DBNull.Value ? false : Convert.ToBoolean(reader["adjustmentcredit"]);
                        if (iscredit)
                            item.credit = reader["amount"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["amount"]);
                        else
                            item.debit = reader["amount"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["amount"]);
                        item.isparty = true;
                        ovoucher.parentledger = item;
                        lstvoucheritem.Add(item);
                    }
                }

                LedgerDataService ledger = new LedgerDataService() { Context = this.Context };
                ReferencelistDataService location = new ReferencelistDataService() { Context = this.Context };
                CompanyDataService company = new CompanyDataService() { Context = this.Context };
                EmployeeDataService employee = new EmployeeDataService() { Context = this.Context };
                ovoucher.ledger = ledger.Search(new LedgerCreteria() { id = ovoucher.adjustmentledgerid }).First();
                ovoucher.ledger.parseaddress();
                if (ovoucher.companyid > 0)
                    ovoucher.company = company.Search(new CompanyCreteria() { id = ovoucher.companyid }).First();
                if (ovoucher.locationid > 0)
                {
                    var locationdata = location.Search(new ReferencelistCreteria() { id = ovoucher.locationid }).First();

                    ovoucher.location = new CompanyLocation(locationdata);
                    if (locationdata.additionalinfo != null && locationdata.additionalinfo.locationconfig != null
                        && locationdata.additionalinfo.locationconfig.cityid > 0)
                    {
                        var cityinfo = referencelistDataService.Search(new ReferencelistCreteria()
                        {
                            id = locationdata.additionalinfo.locationconfig.cityid
                        });
                        if (cityinfo.Count > 0)
                            ovoucher.location.cityname = cityinfo[0].name;
                    }
                    if (locationdata.additionalinfo != null && locationdata.additionalinfo.locationconfig != null
                        && locationdata.additionalinfo.locationconfig.stateid > 0)
                    {
                        var stateinfo = referencelistDataService.Search(new ReferencelistCreteria()
                        {
                            id = locationdata.additionalinfo.locationconfig.stateid
                        });
                        if (stateinfo.Count > 0)
                            ovoucher.location.statename = stateinfo[0].name;
                    }

                }
                if (ovoucher.employeeid > 0)
                    ovoucher.employee = employee.Search(new EmployeeCreteria()
                    {
                        id = ovoucher.employeeid,
                        hidesalaryinfo = true
                    }).First();
                if (ovoucher.ledger.stateid > 0)
                    ovoucher.ledger.State = location.SearchList(new ReferencelistCreteria() { id = ovoucher.ledger.stateid }).First();

                command = GetCommand(SQL_Voucheritem, conn);
                command.Parameters.Add("voucherid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = voucherid;
                using (System.Data.IDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        VoucherPrint.VoucherItemPrint item = new VoucherPrint.VoucherItemPrint();
                        item.hasgst = reader["hasgst"] == DBNull.Value ? false : Convert.ToBoolean(reader["hasgst"]);
                        item.taxmode = reader["taxmode"] == DBNull.Value ? 0 : Convert.ToInt32(reader["taxmode"]);
                        item.typeid = reader["typeid"] == DBNull.Value ? 0 : Convert.ToInt32(reader["typeid"]);
                        item.name = reader["displayname"] == DBNull.Value ? "" : reader["displayname"].ToString();
                        item.hsncode = reader["hsncode"] == DBNull.Value ? "" : reader["hsncode"].ToString();

                        if (reader["issupplier"] != DBNull.Value && Convert.ToBoolean(reader["issupplier"]))
                        {
                            item.supplierid = reader["supplierid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["supplierid"]);
                        }

                        if (reader["hasbank"] != DBNull.Value && Convert.ToBoolean(reader["hasbank"]))
                        {
                            item.bankid = reader["bankid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["bankid"]);
                        }

                        item.ledgerid = reader["ledgerid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["ledgerid"]);


                        if (item.typeid == 16 && !item.hasgst)
                            item.hasgst = true;

                        if (item.hasgst)
                        {
                            item.hasgst = (item.typeid == 16 &&
                            item.taxmode >= 3 &&
                            item.taxmode <= 5);
                        }

                        if (item.typeid == 16 && item.taxmode == 6)
                        {
                            item.hastds = true;
                        }

                        if (item.typeid == 16 && item.taxmode == 7)
                        {
                            item.hastcs = true;
                        }

                        item.rate = reader["rate"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["rate"]);
                        item.qty = reader["qty"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["qty"]);
                        item.taxpercentage = reader["taxpercentage"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["taxpercentage"]);
                        item.taxable = reader["taxable"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["taxable"]);

                        bool iscredit = reader["iscredit"] == DBNull.Value ? false : Convert.ToBoolean(reader["iscredit"]);
                        if (iscredit)
                            item.credit = reader["amount"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["amount"]);
                        else
                            item.debit = reader["amount"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["amount"]);

                        if (item.typeid == 30)
                        {
                            ovoucher.rounding = item.debit + item.credit;
                            if (iscredit == ovoucher.adjustmentcredit)
                                ovoucher.rounding = ovoucher.rounding * -1;
                        }
                        else
                        {
                            lstvoucheritem.Add(item);
                            ovoucher.childledgers.Add(item);
                        }
                    }
                }

                var ledgers = ledger.Search(new LedgerCreteria() { ids = ovoucher.childledgers.ConvertAll(x => x.ledgerid) });
                lstvoucheritem.ForEach(x =>
                {
                    if (ledgers.Exists(y => y.id == x.ledgerid))
                    {
                        x.ledger = ledgers.Find(y => y.id == x.ledgerid);
                        x.ledger.parseaddress();
                    }
                });

                ovoucher.voucheritem = lstvoucheritem;
                ovoucher.taxledgers = ovoucher.childledgers.FindAll(x => x.hasgst);
                ovoucher.taxableledgers = ovoucher.childledgers.FindAll(x => !x.hasgst);
                if (ovoucher.attributes != null && ovoucher.attributes.hasrcm)
                {
                    ovoucher.childledgers.RemoveAll(x => x.hasgst);
                    ovoucher.voucheritem.RemoveAll(x => x.hasgst);
                }

                if (lstvoucheritem.Exists(x => x.hastds))
                {
                    ovoucher.tdsledger = ovoucher.childledgers.First(x => x.hastds);
                    ovoucher.childledgers.RemoveAll(x => x.hastds);
                    ovoucher.voucheritem.RemoveAll(x => x.hastds);
                }

                if (lstvoucheritem.Exists(x => x.hastcs))
                {
                    ovoucher.tcsledger = ovoucher.childledgers.First(x => x.hastcs);
                    ovoucher.childledgers.RemoveAll(x => x.hastcs);
                    ovoucher.voucheritem.RemoveAll(x => x.hastcs);
                }

                ovoucher.transactions = ovoucher.voucheritem.FindAll(x => !x.isparty && !x.hasgst);

                ovoucher.partyledger = ovoucher.ledger;
                if (ovoucher.transactions.Count == 1 &&
                    (ovoucher.adjustmentledgertypeid == 7))
                {
                    ovoucher.isbankpayment = true;
                    ovoucher.partyledger = ovoucher.transactions[0].ledger;
                }

                if (ovoucher.transactions.Count == 1 &&
                    (ovoucher.adjustmentledgertypeid == 6))
                {
                    ovoucher.iscashpayment = true;
                    ovoucher.partyledger = ovoucher.transactions[0].ledger;
                }

                if (ovoucher.createdby > 0)
                {
                    using (var reader = ExecuteReader("select employee.name from users inner join employee on employeeid = employee.id where users.id=" + ovoucher.createdby, null))
                    {
                        if (reader.Read())
                            ovoucher.enteredby = reader.IsDBNull(0) ? "" : reader.GetString(0);
                    }
                }

                if (ovoucher.modifiedby > 0)
                {
                    using (var reader = ExecuteReader("select employee.name from users inner join employee on employeeid = employee.id where users.id=" + ovoucher.modifiedby, null))
                    {
                        if (reader.Read())
                            ovoucher.alteredby = reader.IsDBNull(0) ? "" : reader.GetString(0);
                    }
                }

                ovoucher.gross = ovoucher.transactions.Sum(x => x.amount);
                ovoucher.igst = ovoucher.taxledgers.Sum(x => x.taxmode == 3 ? x.credit + x.debit : 0);
                ovoucher.sgst = ovoucher.taxledgers.Sum(x => x.taxmode == 4 ? x.credit + x.debit : 0);
                ovoucher.cgst = ovoucher.taxledgers.Sum(x => x.taxmode == 5 ? x.credit + x.debit : 0);
                if (ovoucher.tdsledger != null)
                    ovoucher.tds = ovoucher.tdsledger.debit + ovoucher.tdsledger.credit;
                if (ovoucher.tcsledger != null)
                    ovoucher.tcs = ovoucher.tcsledger.debit + ovoucher.tcsledger.credit;


                if (ovoucher.vouchertype == 1 && ovoucher.adjustmentledgertypeid == 7 &&
                    lstvoucheritem.Exists(x => x.typeid == 12 || x.typeid == 11) &&
                    ovoucher.transactions.Count == 1)
                {

                }
                ovoucher.payable = (ovoucher.gross + ovoucher.igst + ovoucher.cgst + ovoucher.sgst + ovoucher.tcs);
                ovoucher.netpayable = (ovoucher.gross + ovoucher.igst + ovoucher.cgst + ovoucher.sgst + ovoucher.tcs) - ovoucher.tds + ovoucher.rounding;
                ovoucher.numberinword = WebAPI.Data.Numberconvertor.rupees(ovoucher.netpayable);
                conn.Close();
            }
            catch (Exception exp)
            {
                if (conn != null) conn.Close();
                WebAPI.Common.Logger.Instance(Context.OrganizationID).Log(exp, WebAPI.Common.Logger.LogType.Error, voucherid);
            }
            return ovoucher;
        }
        public bool Delete(NpgsqlTransaction transaction, NpgsqlConnection conn, Voucher oVoucher)
        {

            NpgsqlCommand command = GetCommand(SQL_Delete, conn, transaction);
            command.Parameters.Add("isactive", NpgsqlTypes.NpgsqlDbType.Boolean).Value = oVoucher.isactive;
            command.Parameters.Add("modifiedby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = Context.UserID;
            command.Parameters.Add("modifiedon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = DateTimeNow;
            command.Parameters.Add("version", NpgsqlTypes.NpgsqlDbType.Integer).Value = oVoucher.version;
            command.Parameters.Add("id", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oVoucher.id;
            int successCount = command.ExecuteNonQuery();

            return successCount > 0;
        }
    }

}
