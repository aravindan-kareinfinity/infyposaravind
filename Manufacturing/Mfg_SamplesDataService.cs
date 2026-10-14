using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using InfyPOS.Models;
using InfyPOS.Common;
using Npgsql;
using InfyPOS.Controllers;
using Newtonsoft.Json;

namespace InfyPOS.Data

{
    public class Mfg_SamplesDataService : BaseData
    {
        const string SQL_Search = @"select entrydate, salesmenid, employeeid, mfg_samples.isactive, mfg_samples.locationid, remark, 
		mfg_samples.qty, receiptno,designcode, uid, mfg_samples.version, 
		mfg_samples.modifiedon, mfg_samples.createdon, mfg_samples.createdby, mfg_samples.modifiedby, 
		mfg_samples.id,salablegoodsid,returnsampleid,referencelist.id sizeid,referencelist.name size
		from mfg_samples
		inner join salablegoods on salablegoods.id = salablegoodsid
		inner join stock on stock.id = stockid
		inner join items on items.id = stock.itemid
		inner join referencelist on referencelist.id = sizeid where mfg_samples.id=@id";

        const string SQL_Insert = @"
        INSERT INTO public.mfg_samples(entrydate, salesmenid, employeeid, isactive, locationid, receiptno,
        remark, qty, designcode, uid, version, modifiedon, createdon, createdby, modifiedby,salablegoodsid,returnsampleid) 
        VALUES(@entrydate, @salesmenid,
        @employeeid, @isactive, @locationid, @receiptno, @remark, @qty, @designcode, @uid, @version, @modifiedon, @createdon, 
        @createdby, @modifiedby,@salablegoodsid,@returnsampleid) RETURNING id";


        const string SQL_Update = @"UPDATE public.mfg_samples SET entrydate=@entrydate, salesmenid=@salesmenid, 
        employeeid=@employeeid, locationid=@locationid, remark=@remark, qty=@qty, designcode=@designcode, uid=@uid, 
        version=@version+1, modifiedon=@modifiedon, modifiedby=@modifiedby,salablegoodsid=@salablegoodsid,
        returnsampleid=@returnsampleid
        where id=@id and version=@version RETURNING version";


        const string SQL_Delete = @"update public.mfg_samples set isactive=false,version=@version+1,modifiedby=@modifiedby,modifiedon=@modifiedon where id=@id and version=@version";

        public List<Mfg_Samples> Search(Mfg_SamplesCreteria _mfg_samples)
        {
            List<Mfg_Samples> lst = new List<Mfg_Samples>();
            NpgsqlConnection conn = CreateConnection();
            try
            {
                conn.Open();
                NpgsqlCommand command = null;
                if (_mfg_samples.id != 0)
                {
                    command = new NpgsqlCommand(SQL_Search, conn);
                    command.Parameters.Add("id", NpgsqlTypes.NpgsqlDbType.Bigint).Value = _mfg_samples.id;
                }
                else
                {
                    QueryBuilder qb = new QueryBuilder();
                    qb.CurrentQuery = SQL_Search.Substring(0, SQL_Search.IndexOf("where"));
                    qb.AddParameter("mfg_samples.isactive", true, "=");

                    if (_mfg_samples.salesmenid > 0)
                        qb.AddParameter("mfg_samples.salesmenid", _mfg_samples.salesmenid, "=");

                    if (_mfg_samples.fromdate.Year > 2000)
                        qb.AddParameter("mfg_samples.entrydate", "fromentrydate", _mfg_samples.fromdate.Date, ">=");

                    if (_mfg_samples.todate.Year > 2000)
                        qb.AddParameter("mfg_samples.entrydate", "toentrydate", _mfg_samples.todate.AddDays(1).Date, "<");

                    if (!string.IsNullOrEmpty(_mfg_samples.designcode))
                    {
                        if (_mfg_samples.designcode.IndexOf("%") >= 0)
                        {
                            qb.AddParameter("mfg_samples.designcode", _mfg_samples.designcode, " ilike ");
                        }
                        else
                        {
                            qb.AddParameter("mfg_samples.designcode", _mfg_samples.designcode, " = ");
                        }
                    }
                    if (!string.IsNullOrEmpty(_mfg_samples.receiptno))
                        qb.AddParameter("mfg_samples.receiptno", _mfg_samples.receiptno, " = ");

                    if (_mfg_samples.mode == 0)
                        qb.AddParameter("mfg_samples.qty", 0, ">");

                    if (_mfg_samples.mode == 1)
                        qb.AddParameter("mfg_samples.qty", 0, "<");

                    if (_mfg_samples.mode == 2)
                    {
                        qb.AddParameter("mfg_samples.returnsampleid", 0, "=");
                        qb.AddParameter("mfg_samples.hasreturned", false, "=");
                    }

                    command = qb.GetCommand(conn);
                }

                using (System.Data.IDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        Mfg_Samples omfg_samples = new Mfg_Samples();
                        omfg_samples.entrydate = reader["entrydate"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(reader["entrydate"]);
                        omfg_samples.salesmenid = reader["salesmenid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["salesmenid"]);
                        omfg_samples.employeeid = reader["employeeid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["employeeid"]);
                        omfg_samples.salablegoodsid = reader["salablegoodsid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["salablegoodsid"]);
                        omfg_samples.returnsampleid = reader["returnsampleid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["returnsampleid"]);
                        omfg_samples.isactive = reader["isactive"] == DBNull.Value ? false : Convert.ToBoolean(reader["isactive"]);
                        omfg_samples.locationid = reader["locationid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["locationid"]);
                        omfg_samples.remark = reader["remark"] == DBNull.Value ? "" : reader["remark"].ToString();
                        omfg_samples.receiptno = reader["receiptno"] == DBNull.Value ? "" : reader["receiptno"].ToString();
                        omfg_samples.qty = reader["qty"] == DBNull.Value ? 0 : Convert.ToInt32(reader["qty"]);
                        omfg_samples.designcode = reader["designcode"] == DBNull.Value ? "" : reader["designcode"].ToString();
                        omfg_samples.uid = reader["uid"] == DBNull.Value ? "" : reader["uid"].ToString();
                        omfg_samples.size = reader["size"] == DBNull.Value ? "" : reader["size"].ToString();
                        omfg_samples.sizeid = reader["sizeid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["sizeid"]);

                        omfg_samples.version = reader["version"] == DBNull.Value ? 0 : Convert.ToInt32(reader["version"]);
                        omfg_samples.modifiedon = reader["modifiedon"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(reader["modifiedon"]);
                        omfg_samples.createdon = reader["createdon"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(reader["createdon"]);
                        omfg_samples.createdby = reader["createdby"] == DBNull.Value ? 0 : Convert.ToInt64(reader["createdby"]);
                        omfg_samples.modifiedby = reader["modifiedby"] == DBNull.Value ? 0 : Convert.ToInt64(reader["modifiedby"]);
                        omfg_samples.id = reader["id"] == DBNull.Value ? 0 : Convert.ToInt64(reader["id"]);
                        lst.Add(omfg_samples);
                    }
                }
                conn.Close();
            }
            catch (Exception exp)
            {
                if (conn != null) conn.Close();
                WebAPI.Common.Logger.Instance(Context.OrganizationID).Log(exp, WebAPI.Common.Logger.LogType.Error, _mfg_samples);
            }

            if (_mfg_samples.includeitems && lst.Count == 1 && !string.IsNullOrEmpty(lst[0].receiptno))
            {
                lst[0].sampleitems = Search(new Mfg_SamplesCreteria()
                {
                    receiptno = lst[0].receiptno
                });
                CompanyDataService companyDataService = new CompanyDataService() { Context = this.Context };
                ReferencelistDataService referencelistDataService = new ReferencelistDataService() { Context = this.Context };
                EmployeeDataService employeeDataService = new EmployeeDataService() { Context = this.Context };
                lst[0].company = companyDataService.Search(new CompanyCreteria() { }).First();
                var employees = employeeDataService.Search(new EmployeeCreteria()
                {
                    id = lst[0].salesmenid,
                    hidesalaryinfo = true
                });
                if (employees.Count > 0)
                    lst[0].salesmen = employees.First();

                employees = employeeDataService.Search(new EmployeeCreteria()
                {
                    id = lst[0].employeeid,
                    hidesalaryinfo = true
                });
                if (employees.Count > 0)
                    lst[0].employee = employees.First();

                var locations = referencelistDataService.Search(new ReferencelistCreteria() { id = lst[0].locationid });
                if (locations.Count > 0)
                    lst[0].location = locations[0];
            }
            return lst;
        }


        internal List<Mfg_SampleSummary> Summary(Mfg_SamplesCreteria item)
        {
            List<Mfg_SampleSummary> lst = new List<Mfg_SampleSummary>();
            NpgsqlConnection conn = CreateConnection();
            try
            {
                conn.Open();
                NpgsqlCommand command = null;
                QueryBuilder qb = new QueryBuilder();
                qb.CurrentQuery = @"select designcode,sum(qty) qty from mfg_samples
                where isactive=true and salesmenid = @salesmenid
                group by designcode
                having sum(qty) != 0;

                select mfgdesign.code designcode,
                sum(Mfg_Swatches_History.qty) qty from Mfg_Swatches_History 
                inner join mfg_swatches_folder on mfg_swatches_folder.id=Mfg_Swatches_History.folderid 
				left outer join mfgdesign on mfgdesign.id = Mfg_Swatches_Folder.designid
                where Mfg_Swatches_History.isactive=true and Mfg_Swatches_History.referenceid=@salesmenid
                group by mfgdesign.code,Mfg_Swatches_History.referenceid
                having sum(Mfg_Swatches_History.qty) != 0;";

                command = qb.GetCommand(conn);
                command.Parameters.AddWithValue("salesmenid", item.salesmenid);

                using (System.Data.IDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        Mfg_SampleSummary omfg_samples = new Mfg_SampleSummary();
                        omfg_samples.samplestock = reader["qty"] == DBNull.Value ? 0 : Convert.ToInt32(reader["qty"]);
                        omfg_samples.designcode = reader["designcode"] == DBNull.Value ? "" : reader["designcode"].ToString();
                        omfg_samples.designcode = omfg_samples.designcode.Substring(0, omfg_samples.designcode.IndexOf('/'));
                        omfg_samples.salesorders = new List<Mfg_SampleSummary.Item>();
                        omfg_samples.productstocks = new List<Mfg_SampleSummary.Item>();
                        lst.Add(omfg_samples);
                    }
                    if (reader.NextResult())
                    {
                        while (reader.Read())
                        {
                            Mfg_SampleSummary omfg_samples = new Mfg_SampleSummary();
                            omfg_samples.folderstock = (reader["qty"] == DBNull.Value ? 0 : Convert.ToInt32(reader["qty"])) * -1;
                            omfg_samples.designcode = reader["designcode"] == DBNull.Value ? "" : reader["designcode"].ToString();
                            if (lst.Exists(x => x.designcode == omfg_samples.designcode))
                            {
                                lst.Find(x => x.designcode == omfg_samples.designcode).folderstock = omfg_samples.folderstock;
                            }
                            else
                            {
                                omfg_samples.salesorders = new List<Mfg_SampleSummary.Item>();
                                omfg_samples.productstocks = new List<Mfg_SampleSummary.Item>();
                                lst.Add(omfg_samples);
                            }
                        }
                    }
                }

                if (lst.Count == 0)
                {
                    conn.Close();
                    return new List<Mfg_SampleSummary>();
                }

                string query = @"select stock.designid designcode,stock.serialno,
            sum(salablegoods.iqty-(salablegoods.oqty+salablegoods.dqty+salablegoods.jqty)) qty from salablegoods
            inner join stock on stock.id = stockid
            where salablegoods.hasstock=true and salablegoods.isactive=true and stock.isactive=true {0}
            group by stock.designid,stock.serialno";

                if (InfyPOS.Services.ConfigManager.Instance(Context.OrganizationID).Config.system_hyper_uan)
                    query = @"select stock.designid designcode,stock.barcode as serialno,
                    sum(salablegoods.iqty-(salablegoods.oqty+salablegoods.dqty+salablegoods.jqty)) qty from salablegoods
                    inner join stock on stock.id = stockid
                    where salablegoods.hasstock=true and salablegoods.isactive=true and stock.isactive=true {0}
                    group by stock.designid,stock.barcode";


                List<string> conditions = new List<string>();
                var designlist = lst.ConvertAll(x => x.designcode).Distinct().ToList();
                if (designlist.Count > 1)
                {
                    conditions.Add(" stock.designid in (" + string.Join(",", designlist.ConvertAll(e => "'" + WebAPI.Data.Utility.SafeSQL(e) + "'")) + ") ");
                }
                else if (designlist.Count == 1)
                {
                    conditions.Add(" stock.designid = '" + WebAPI.Data.Utility.SafeSQL(designlist.First()) + "' ");
                }
                query = string.Format(query, (conditions.Count > 0 ? " and " : "") + string.Join(" and ", conditions));
                using (var reader = ExecuteReader(query, null, conn))
                {
                    while (reader.Read())
                    {
                        Mfg_SampleSummary omfg_samples = new Mfg_SampleSummary();
                        omfg_samples.productstock = reader["qty"] == DBNull.Value ? 0 : Convert.ToInt32(reader["qty"]);
                        omfg_samples.designcode = reader["designcode"] == DBNull.Value ? "" : reader["designcode"].ToString();
                        Mfg_SampleSummary.Item stitem = new Mfg_SampleSummary.Item();
                        stitem.uid = reader["serialno"] == DBNull.Value ? "" : reader["serialno"].ToString();
                        stitem.qty = omfg_samples.productstock;
                        if (lst.Exists(x => x.designcode == omfg_samples.designcode))
                        {
                            lst.Find(x => x.designcode == omfg_samples.designcode).productstocks.Add(stitem);
                            lst.Find(x => x.designcode == omfg_samples.designcode).productstock += omfg_samples.productstock;
                        }
                        else
                        {
                            omfg_samples.salesorders = new List<Mfg_SampleSummary.Item>();
                            omfg_samples.productstocks = new List<Mfg_SampleSummary.Item>();
                            lst.Add(omfg_samples);
                            lst.Find(x => x.designcode == omfg_samples.designcode).productstocks.Add(stitem);
                        }
                    }
                }


                query = @"select sum(oqty-(bqty+cqty+rqty)) qty,designid designcode,uid
                from salesorderitems 
				inner join salesorder on salesorder.id =salesorderid
				where salesorderitems.isactive=true and salesorder.isactive=true and
                oqty-(bqty+cqty+rqty)>0 and salesorder.salesmenid=@salesmenid {0}
                group by designid,uid";
                conditions = new List<string>();
                if (designlist.Count > 1)
                {
                    conditions.Add(" salesorderitems.designid in (" + String.Join(",", designlist.ConvertAll(e => "'" + WebAPI.Data.Utility.SafeSQL(e) + "'")) + ") ");
                }
                else if (designlist.Count == 1)
                {
                    conditions.Add(" salesorderitems.designid = '" + WebAPI.Data.Utility.SafeSQL(designlist.First()) + "' ");
                }
                query = string.Format(query, (conditions.Count > 0 ? " and " : "") + string.Join(" and ", conditions));
                var param = new SortedList<string, object>();
                param.Add("salesmenid", item.salesmenid);
                using (var reader = ExecuteReader(query, param, conn))
                {
                    while (reader.Read())
                    {
                        Mfg_SampleSummary omfg_samples = new Mfg_SampleSummary();
                        omfg_samples.salesorder = reader["qty"] == DBNull.Value ? 0 : Convert.ToInt32(reader["qty"]);
                        omfg_samples.designcode = reader["designcode"] == DBNull.Value ? "" : reader["designcode"].ToString();
                        Mfg_SampleSummary.Item stitem = new Mfg_SampleSummary.Item();
                        stitem.uid = reader["uid"] == DBNull.Value ? "" : reader["uid"].ToString();
                        stitem.qty = omfg_samples.productstock;
                        if (lst.Exists(x => x.designcode == omfg_samples.designcode))
                        {
                            lst.Find(x => x.designcode == omfg_samples.designcode).salesorder += omfg_samples.salesorder;
                            lst.Find(x => x.designcode == omfg_samples.designcode).salesorders.Add(stitem);
                        }
                        else
                        {
                            omfg_samples.salesorders = new List<Mfg_SampleSummary.Item>();
                            omfg_samples.productstocks = new List<Mfg_SampleSummary.Item>();
                            lst.Add(omfg_samples);
                            lst.Find(x => x.designcode == omfg_samples.designcode).salesorders.Add(stitem);
                        }
                    }
                }


                conn.Close();
            }
            catch (Exception exp)
            {
                if (conn != null) conn.Close();
                WebAPI.Common.Logger.Instance(Context.OrganizationID).Log(exp, WebAPI.Common.Logger.LogType.Error, item);
            }
            MfgdesignDataService mfgdesignDataService = new MfgdesignDataService() { Context = this.Context };
            var designs = mfgdesignDataService.Search(new MfgdesignCreteria()
            {
                codes = lst.ConvertAll(x => x.designcode).Distinct().ToList(),
                includedetail = true
            });
            lst.ForEach(x =>
            {
                if (designs.Exists(y => y.code == x.designcode))
                {
                    var design = designs.Find(y => y.code == x.designcode);
                    x.productid = design.productid;
                    x.brandid = design.brandid;
                    if (x.productstocks != null)
                    {
                        foreach (var pstock in x.productstocks)
                        {
                            if (design.sizegroup.items.Exists(z => z.uid == pstock.uid))
                            {
                                var ditem = design.sizegroup.items.Find(z => z.uid == pstock.uid);
                                pstock.designcode = ditem.code;
                                pstock.sizeid = ditem.sizeid;
                            }
                        }
                    }
                    if (x.salesorders != null)
                    {
                        foreach (var pstock in x.salesorders)
                        {
                            if (design.sizegroup.items.Exists(z => z.uid == pstock.uid))
                            {
                                var ditem = design.sizegroup.items.Find(z => z.uid == pstock.uid);
                                pstock.designcode = ditem.code;
                                pstock.sizeid = ditem.sizeid;
                            }
                        }
                    }
                }
            });
            return lst;
        }

        public Mfg_Samples Insert(Mfg_Samples oMfg_Samples)
        {

            NpgsqlTransaction transaction = null;
            try
            {
                using (NpgsqlConnection conn = CreateConnection())
                {
                    conn.Open();
                    transaction = conn.BeginTransaction();

                    var sequence = SequenceManager.Instance(Context.OrganizationID).Get("sample.receiptno", 0);
                    var receiptno = sequence.GetNextNumber(Context, conn, transaction);
                    oMfg_Samples.sampleitems = new List<Mfg_Samples>();
                    if (oMfg_Samples.items != null && oMfg_Samples.items.Count > 0)
                    {
                        foreach (var item in oMfg_Samples.items)
                        {
                            oMfg_Samples.id = 0;
                            oMfg_Samples.receiptno = receiptno;
                            oMfg_Samples.uid = item.uid;
                            oMfg_Samples.designcode = item.designcode;
                            oMfg_Samples.qty = oMfg_Samples.optionid == 2 ? 1 : -1;
                            oMfg_Samples.sampleitems.Add(JsonConvert.DeserializeObject<Mfg_Samples>(JsonConvert.SerializeObject(Insert(transaction, conn, oMfg_Samples))));
                        }
                    }
                    else
                    {
                        if (oMfg_Samples.qty == 0)
                            oMfg_Samples.qty = oMfg_Samples.optionid == 2 ? 1 : -1;
                        oMfg_Samples.receiptno = receiptno;
                        oMfg_Samples.sampleitems.Add(JsonConvert.DeserializeObject<Mfg_Samples>(JsonConvert.SerializeObject(Insert(transaction, conn, oMfg_Samples))));
                    }
                    transaction.Commit();
                    conn.Close();
                }
            }
            catch (Exception exp)
            {
                WebAPI.Common.Logger.Instance(Context.OrganizationID).Log(exp, WebAPI.Common.Logger.LogType.Error, oMfg_Samples);
            }

            return oMfg_Samples;
        }

        public Mfg_Samples Insert(NpgsqlTransaction transaction, NpgsqlConnection conn, Mfg_Samples oMfg_Samples)
        {
            AdjustStock(transaction, conn, oMfg_Samples);
            NpgsqlCommand command = new NpgsqlCommand(SQL_Insert, conn, transaction);
            command.Parameters.Add("entrydate", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = oMfg_Samples.entrydate;
            command.Parameters.Add("salesmenid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oMfg_Samples.salesmenid;
            command.Parameters.Add("salablegoodsid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oMfg_Samples.salablegoodsid;
            command.Parameters.Add("returnsampleid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oMfg_Samples.returnsampleid;
            command.Parameters.Add("employeeid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oMfg_Samples.employeeid;
            command.Parameters.Add("isactive", NpgsqlTypes.NpgsqlDbType.Boolean).Value = oMfg_Samples.isactive;
            command.Parameters.Add("locationid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oMfg_Samples.locationid;
            command.Parameters.Add("remark", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oMfg_Samples.remark == null ? "" : oMfg_Samples.remark;
            command.Parameters.Add("receiptno", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oMfg_Samples.receiptno == null ? "" : oMfg_Samples.receiptno;
            command.Parameters.Add("qty", NpgsqlTypes.NpgsqlDbType.Integer).Value = oMfg_Samples.qty;
            command.Parameters.Add("designcode", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oMfg_Samples.designcode == null ? "" : oMfg_Samples.designcode;
            command.Parameters.Add("uid", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oMfg_Samples.uid == null ? "" : oMfg_Samples.uid;
            command.Parameters.Add("version", NpgsqlTypes.NpgsqlDbType.Integer).Value = oMfg_Samples.version;
            command.Parameters.Add("modifiedon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = oMfg_Samples.modifiedon;
            command.Parameters.Add("createdon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = oMfg_Samples.createdon;
            command.Parameters.Add("createdby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oMfg_Samples.createdby;
            command.Parameters.Add("modifiedby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oMfg_Samples.modifiedby;
            using (System.Data.IDataReader reader = command.ExecuteReader())
            {
                if (reader.Read())
                {
                    oMfg_Samples.id = Convert.ToInt64(reader[0].ToString());
                }
            }
            return oMfg_Samples;
        }

        public Mfg_Samples Update(Mfg_Samples oMfg_Samples)
        {

            NpgsqlTransaction transaction = null;
            try
            {
                using (NpgsqlConnection conn = CreateConnection())
                {
                    conn.Open();
                    transaction = conn.BeginTransaction();

                    Update(transaction, conn, oMfg_Samples);
                    transaction.Commit();
                    conn.Close();
                }
            }
            catch (Exception exp)
            {
                WebAPI.Common.Logger.Instance(Context.OrganizationID).Log(exp, WebAPI.Common.Logger.LogType.Error, oMfg_Samples);

            }

            return oMfg_Samples;
        }

        public Mfg_Samples Update(NpgsqlTransaction transaction, NpgsqlConnection conn, Mfg_Samples oMfg_Samples)
        {

            NpgsqlCommand command = new NpgsqlCommand(SQL_Update, conn, transaction);
            command.Parameters.Add("entrydate", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = oMfg_Samples.entrydate;
            command.Parameters.Add("salablegoodsid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oMfg_Samples.salablegoodsid;
            command.Parameters.Add("returnsampleid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oMfg_Samples.returnsampleid;
            command.Parameters.Add("salesmenid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oMfg_Samples.salesmenid;
            command.Parameters.Add("employeeid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oMfg_Samples.employeeid;
            command.Parameters.Add("isactive", NpgsqlTypes.NpgsqlDbType.Boolean).Value = oMfg_Samples.isactive;
            command.Parameters.Add("locationid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oMfg_Samples.locationid;
            command.Parameters.Add("remark", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oMfg_Samples.remark == null ? "" : oMfg_Samples.remark;
            command.Parameters.Add("qty", NpgsqlTypes.NpgsqlDbType.Integer).Value = oMfg_Samples.qty;
            command.Parameters.Add("designcode", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oMfg_Samples.designcode == null ? "" : oMfg_Samples.designcode;
            command.Parameters.Add("uid", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oMfg_Samples.uid == null ? "" : oMfg_Samples.uid;
            command.Parameters.Add("version", NpgsqlTypes.NpgsqlDbType.Integer).Value = oMfg_Samples.version;
            command.Parameters.Add("modifiedon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = oMfg_Samples.modifiedon;
            command.Parameters.Add("createdon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = oMfg_Samples.createdon;
            command.Parameters.Add("createdby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oMfg_Samples.createdby;
            command.Parameters.Add("modifiedby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oMfg_Samples.modifiedby;
            command.Parameters.Add("id", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oMfg_Samples.id;
            using (System.Data.IDataReader reader = command.ExecuteReader())
            {
                if (reader.Read())
                {
                    oMfg_Samples.version = Convert.ToInt32(reader[0].ToString());
                }
            }

            return oMfg_Samples;
        }


        public bool Delete(Mfg_Samples oMfg_Samples)
        {

            NpgsqlTransaction transaction = null;
            bool hasSuccessCount = false;
            try
            {
                using (NpgsqlConnection conn = CreateConnection())
                {
                    conn.Open();
                    transaction = conn.BeginTransaction();
                    oMfg_Samples = Search(new Mfg_SamplesCreteria()
                    {
                        id = oMfg_Samples.id
                    }).First();
                    hasSuccessCount = Delete(transaction, conn, oMfg_Samples);
                    transaction.Commit();
                    conn.Close();
                }
            }
            catch (Exception exp)
            {
                WebAPI.Common.Logger.Instance(Context.OrganizationID).Log(exp, WebAPI.Common.Logger.LogType.Error, oMfg_Samples);

                
            }
            return hasSuccessCount;

        }


        internal Mfg_SampleStock Stock(Mfg_SamplesCreteria item)
        {
            MfgdesignDataService mfgdesignDataService = new MfgdesignDataService() { Context = this.Context };
            var design = mfgdesignDataService.Search(new MfgdesignCreteria()
            {
                uancode = item.uid,
                includedetail = true
            });

            if (design.Count == 0) throw new Exception("Given UID - " + item.uid + " not found in the system");

            Mfg_SampleStock mfg_SampleStock = new Mfg_SampleStock()
            {
                design = design.First().code,
                designcode =
                design.First().sizegroup.items.Find(x => x.uid == item.uid).code
            };

            string query = @"select sum(salablegoods.iqty-(salablegoods.oqty+salablegoods.dqty+salablegoods.jqty)) stockqty from salablegoods
			inner join stock on stock.id = stockid 
            where salablegoods.hasstock=true and salablegoods.isactive=true and stock.isactive=true and
			salablegoods.locationid=@locationid and serialno=@uid";

            if (InfyPOS.Services.ConfigManager.Instance(Context.OrganizationID).Config.system_hyper_uan)
                query = @"select sum(salablegoods.iqty-(salablegoods.oqty+salablegoods.dqty+salablegoods.jqty)) stockqty from salablegoods
			inner join stock on stock.id = stockid 
            where salablegoods.hasstock=true and salablegoods.isactive=true and stock.isactive=true and
			salablegoods.locationid=@locationid and stock.barcode=@uid";

            var param = new SortedList<string, object>();
            param.Add("locationid", item.locationid);
            param.Add("uid", item.uid);
            param.Add("salesmenid", item.salesmenid);
            using (var reader = ExecuteReader(query, param))
            {
                if (reader.Read())
                {
                    mfg_SampleStock.locationstock = reader.IsDBNull(0) ? 0 : Convert.ToInt32(reader[0]);
                }
            }

            query = @"select sum(qty) stockqty from mfg_samples
            where mfg_samples.isactive=true and uid=@uid and salesmenid=@salesmenid";
            using (var reader = ExecuteReader(query, param))
            {
                if (reader.Read())
                {
                    mfg_SampleStock.selfstock = reader.IsDBNull(0) ? 0 : Convert.ToInt32(reader[0]);
                }
            }
            return mfg_SampleStock;
        }
        public bool AdjustStock(NpgsqlTransaction transaction, NpgsqlConnection conn, Mfg_Samples oMfg_Samples)
        {
            if (oMfg_Samples.qty > 0)
            {
                var param = new SortedList<string, object>();
                param.Add("serialno", oMfg_Samples.uid);
                param.Add("qty", oMfg_Samples.qty);
                var query = @"select salablegoods.id from salablegoods inner join stock on stock.id = stockid 
                            where salablegoods.isactive=true and salablegoods.hasstock and stock.serialno=@serialno and (iqty-(oqty+dqty+jqty+aqty)>@qty) limit 1";

                if (InfyPOS.Services.ConfigManager.Instance(Context.OrganizationID).Config.system_hyper_uan)
                    query = @"select salablegoods.id from salablegoods inner join stock on stock.id = stockid 
                            where salablegoods.isactive=true and salablegoods.hasstock and stock.barcode=@serialno and (iqty-(oqty+dqty+jqty+aqty)>@qty) limit 1";


                using (var reader = ExecuteReader(query,
                                param,
                                conn, transaction))
                {
                    if (reader.Read())
                    {
                        oMfg_Samples.salablegoodsid = reader.GetInt64(0);
                    }
                }
                if (oMfg_Samples.salablegoodsid == 0)
                    throw new Exception("Expected stock not available in that location - " + oMfg_Samples.uid);

                param.Add("id", oMfg_Samples.salablegoodsid);
                return ExecuteQuery(@"update salablegoods set oqty=oqty+@qty where id=@id",
                                param,
                                conn, transaction) > 0;
            }
            else
            {
                var param = new SortedList<string, object>();
                param.Add("serialno", oMfg_Samples.uid);
                param.Add("salesmenid", oMfg_Samples.salesmenid);
                using (var reader = ExecuteReader(@"select salablegoodsid,id from mfg_samples where uid=@serialno and salesmenid=@salesmenid and hasreturned=false limit 1",
                                param,
                                conn, transaction))
                {
                    if (reader.Read())
                    {
                        oMfg_Samples.salablegoodsid = reader.GetInt64(0);
                        oMfg_Samples.returnsampleid = reader.GetInt64(1);
                    }
                }
                if (oMfg_Samples.salablegoodsid == 0)
                    throw new Exception("Expected stock not available with salesmen - " + oMfg_Samples.uid);

                param.Add("qty", Math.Abs(oMfg_Samples.qty));
                param.Add("id", oMfg_Samples.salablegoodsid);
                param.Add("returnsampleid", oMfg_Samples.returnsampleid);
                if (ExecuteQuery(@"update mfg_samples set hasreturned=true where id=@returnsampleid and salesmenid=@salesmenid and hasreturned=false",
                                param,
                                conn, transaction) > 0)
                {
                    return ExecuteQuery(@"update salablegoods set oqty=oqty-@qty where id=@id",
                                    param,
                                    conn, transaction) > 0;
                }
                throw new Exception("Expected stock not available with salesmen - " + oMfg_Samples.uid);
            }
        }
        public bool Delete(NpgsqlTransaction transaction, NpgsqlConnection conn, Mfg_Samples oMfg_Samples)
        {
            if (oMfg_Samples.qty > 0)
            {
                string query = "Update salablegoods set oqty=oqty-@qty where id=@id";
                var param = new SortedList<string, object>();
                param.Add("qty", oMfg_Samples.qty);
                param.Add("id", oMfg_Samples.salablegoodsid);
                ExecuteQuery(query, param, conn, transaction);
            }
            else
            {
                string query = "Update salablegoods set oqty=oqty+@qty where id=@id";
                var param = new SortedList<string, object>();
                param.Add("qty", Math.Abs(oMfg_Samples.qty));
                param.Add("id", oMfg_Samples.salablegoodsid);
                ExecuteQuery(query, param, conn, transaction);
                query = "Update mfg_samples set hasreturned=false where id=@returnsampleid";
                param.Add("returnsampleid", oMfg_Samples.returnsampleid);
                ExecuteQuery(query, param, conn, transaction);
            }
            NpgsqlCommand command = new NpgsqlCommand(SQL_Delete, conn, transaction);
            command.Parameters.Add("isactive", NpgsqlTypes.NpgsqlDbType.Boolean).Value = oMfg_Samples.isactive;
            command.Parameters.Add("modifiedby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oMfg_Samples.modifiedby;
            command.Parameters.Add("modifiedon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = oMfg_Samples.modifiedon;
            command.Parameters.Add("version", NpgsqlTypes.NpgsqlDbType.Integer).Value = oMfg_Samples.version;
            command.Parameters.Add("id", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oMfg_Samples.id;
            int successCount = command.ExecuteNonQuery();

            return successCount > 0;
        }

        internal MfgReportServiceController.StockReport SearchDesignwiseStock(Mfg_SamplesCreteria item)
        {
            MfgReportDataService mfgReportDataService = new MfgReportDataService() { Context = this.Context };

            List<Mfg_SampleSummary> lst = new List<Mfg_SampleSummary>();
            NpgsqlConnection conn = CreateConnection();
            try
            {
                conn.Open();
                NpgsqlCommand command = null;
                QueryBuilder qb = new QueryBuilder();
                qb.CurrentQuery = @"select designcode,sum(qty) qty from mfg_samples
                where isactive=true and salesmenid = @salesmenid
                group by designcode
                having sum(qty) != 0;

                select mfgdesign.code designcode,
                sum(Mfg_Swatches_History.qty) qty from Mfg_Swatches_History 
                inner join mfg_swatches_folder on mfg_swatches_folder.id=Mfg_Swatches_History.folderid 
				left outer join mfgdesign on mfgdesign.id = Mfg_Swatches_Folder.designid
                where Mfg_Swatches_History.isactive=true and Mfg_Swatches_History.referenceid=@salesmenid
                group by mfgdesign.code,Mfg_Swatches_History.referenceid
                having sum(Mfg_Swatches_History.qty) != 0;";

                command = qb.GetCommand(conn);
                command.Parameters.AddWithValue("salesmenid", item.salesmenid);

                using (System.Data.IDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        Mfg_SampleSummary omfg_samples = new Mfg_SampleSummary();
                        omfg_samples.samplestock = reader["qty"] == DBNull.Value ? 0 : Convert.ToInt32(reader["qty"]);
                        omfg_samples.designcode = reader["designcode"] == DBNull.Value ? "" : reader["designcode"].ToString();
                        omfg_samples.designcode = omfg_samples.designcode.Substring(0, omfg_samples.designcode.IndexOf('/'));
                        lst.Add(omfg_samples);
                    }
                    if (reader.NextResult())
                    {
                        while (reader.Read())
                        {
                            Mfg_SampleSummary omfg_samples = new Mfg_SampleSummary();
                            omfg_samples.folderstock = (reader["qty"] == DBNull.Value ? 0 : Convert.ToInt32(reader["qty"])) * -1;
                            omfg_samples.designcode = reader["designcode"] == DBNull.Value ? "" : reader["designcode"].ToString();
                            if (lst.Exists(x => x.designcode == omfg_samples.designcode))
                            {
                                lst.Find(x => x.designcode == omfg_samples.designcode).folderstock = omfg_samples.folderstock;
                            }
                            else
                            {
                                omfg_samples.salesorders = new List<Mfg_SampleSummary.Item>();
                                omfg_samples.productstocks = new List<Mfg_SampleSummary.Item>();
                                lst.Add(omfg_samples);
                            }
                        }
                    }
                }
                conn.Close();
            }
            catch (Exception exp)
            {
                conn.Close();
            }

            if (lst.Count > 0)
            {
                var reports = mfgReportDataService.SearchDesignwiseStock(new MfgReportServiceController.StockReportCreteria()
                {
                    designid = String.Join(",", lst.ConvertAll(x => x.designcode)),
                    ShowRemaining = true,
                    ShowSO = true,
                    locationid = item.locationid,
                    ShowPrice = true
                });
                reports.items.ForEach(x =>
                {
                    if (lst.Exists(y => y.designcode == x.designid))
                    {
                        var sm = lst.Find(y => y.designcode == x.designid);
                        x.fabricstock = sm.samplestock;
                        x.forecast = sm.folderstock;
                    }
                });
                return reports;
            }
            return new MfgReportServiceController.StockReport()
            {
                items = new List<MfgReportServiceController.StockReport.DesignStock>(),
                sizeids = new List<long>(),
                sizenames = new List<string>()
            };
        }
    }

}
