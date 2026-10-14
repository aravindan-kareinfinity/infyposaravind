using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using InfyPOS.Models;
using InfyPOS.Common;
using Npgsql;
namespace InfyPOS.Data

{
    public class JobworkbillitemsDataService : BaseData
    {
        const string SQL_Search = @"select jobworkbillid, oqty, iqty, dqty, rqty, modifiedon, createdon, version, modifiedby, id, sno, barcode, salablegoodsid, stockid, productid, invoiceitemid, isactive, createdby, hsncode, materialsalevalue, materialcost, chargerate, chargetax, chargetaxpercentage, chargetaxid,chargeid, chargegross, chargetotal,outproductid from jobworkbillitems where id=@id";
        const string SQL_SearchPending = @"select entrydate,invoiceno,jobworkbillid, Jobworkbillitems.oqty, iqty, dqty, rqty, 
        Jobworkbillitems.modifiedon, Jobworkbillitems.createdon, Jobworkbillitems.version, Jobworkbillitems.modifiedby, 
        Jobworkbillitems.id, jobworkbillitems.sno,jobworkbillitems.barcode, salablegoodsid, stockid, jobworkbillitems.productid, 
		jobworkbillitems.invoiceitemid,Jobworkbill.locationid,stock.itemid,stock.designid,stock.imageid,
        Jobworkbillitems.isactive, Jobworkbillitems.createdby, jobworkbillitems.hsncode, Jobworkbillitems.materialsalevalue,Jobworkbill.companyid,
        Jobworkbillitems.materialcost, chargerate, chargetax, Jobworkbill.supplierid,Jobworkbill.suppliercompanyid,
        chargetaxpercentage, chargetaxid,chargeid, chargegross, chargetotal, outproductid,buyingprice,ibuyingprice,stock.price from jobworkbillitems 
        inner join Jobworkbill on Jobworkbill.id = Jobworkbillid
        inner join stock on stock.id =stockid
        where jobworkbillitems.isactive = true and Jobworkbillitems.oqty-(rqty+dqty)>0
		";

        const string SQL_JobWorkPendingBarcodes = @"
        with pendingjobworks
		as
		(
		select (oqty-(rqty+dqty)) pqty, (oqty) oqty,
		jobworkbillitems.barcode,jobworkbillitems.salablegoodsid,
		jobworkbillid,lrinvoiceitems.productid,outproductid,suppliercompanyid from jobworkbillitems
		left outer join lrinvoiceitems on lrinvoiceitems.id = invoiceitemid
		left outer join lrinvoice on lrinvoice.id = lrinvoiceitems.lrinvoiceid
		where jobworkbillitems.isactive = true and Jobworkbillitems.oqty-(rqty+dqty)>0
		)
		select salablegoods.iqty,salablegoods.oqty,
		salablegoods.iqty-(salablegoods.oqty+salablegoods.jqty+salablegoods.dqty) stock,
		pendingjobworks.barcode,referencelist.name locationname
		from salablegoods
		inner join pendingjobworks on pendingjobworks.salablegoodsid = salablegoods.id
        inner join referencelist on referencelist.id=salablegoods.locationid
		where salablegoods.iqty-(salablegoods.oqty+salablegoods.jqty+salablegoods.dqty)!=0";


        const string SQL_JobWorkSummary = @"with pendingjobworks
		as
		(
		select sum(oqty-(rqty+dqty)) pqty, sum(oqty) oqty, jobworkbillid,lrinvoiceitems.productid,outproductid,suppliercompanyid from jobworkbillitems
		left outer join lrinvoiceitems on lrinvoiceitems.id = invoiceitemid
		left outer join lrinvoice on lrinvoice.id = lrinvoiceitems.lrinvoiceid
		where jobworkbillitems.isactive = true and Jobworkbillitems.oqty-(rqty+dqty)>0
		group by jobworkbillid,outproductid,suppliercompanyid,lrinvoiceitems.productid
		)
		select Jobworkbill.id,pendingjobworks.oqty,Jobworkbill.invoiceno,Jobworkbill.expecteddate,Jobworkbill.invoicedate,entrydate,pendingjobworks.pqty,pendingjobworks.outproductid,pendingjobworks.suppliercompanyid,
		supplier.name jobworker,suppliercompany.name sourcesupplier,inproduct.name inproduct,outproduct.name outproduct,companyid
		from Jobworkbill
		inner join pendingjobworks on pendingjobworks.jobworkbillid = Jobworkbill.id
		left outer join supplier on supplier.id = Jobworkbill.supplierid
		left outer join suppliercompany on suppliercompany.id =pendingjobworks.suppliercompanyid
		left outer join products inproduct on inproduct.id = pendingjobworks.productid
		left outer join products outproduct on outproduct.id = pendingjobworks.outproductid
        ";

        const string SQL_JobWorkItems = @" select jobworkbillitems.id,jobworkbillitems.barcode,jobworkbillitems.oqty,jobworkbillitems.rqty,
        jobworkbillitems.oqty-(jobworkbillitems.rqty+jobworkbillitems.dqty) pqty,
		Jobworkbill.invoiceno,Jobworkbill.invoicedate,jobworkbill.entrydate,
		supplier.name jobworker,suppliercompany.name sourcesupplier,outproduct.name outproduct,inproduct.name inproduct,
		jobworkbill.companyid, jobworkbillitems.materialsalevalue, jobworkbillitems.materialcost, 
		jobworkbillitems.chargerate, jobworkbillitems.chargetax, jobworkbillitems.chargegross, jobworkbillitems.chargetotal, 
        Jobworkbillgrn.invoiceno Jobworkbillgrnno,Jobworkbillgrn.entrydate Jobworkbillgrndate
		from jobworkbillitems 
        inner join Jobworkbill on jobworkbill.id = jobworkbillitems.jobworkbillid
		left outer join lrinvoiceitems on lrinvoiceitems.id = invoiceitemid
		left outer join lrinvoice on lrinvoice.id = lrinvoiceitems.lrinvoiceid
		left outer join supplier on supplier.id = Jobworkbill.supplierid
		left outer join suppliercompany on suppliercompany.id = lrinvoice.suppliercompanyid
		left outer join products inproduct on inproduct.id = jobworkbillitems.productid
		inner join products outproduct on outproduct.id = jobworkbillitems.outproductid  
        left outer join Jobworkbillgrnitems on Jobworkbillgrnitems.jobworkbillitemsid = jobworkbillitems.id
        left outer join Jobworkbillgrn on Jobworkbillgrn.id = Jobworkbillgrnid
        where jobworkbillitems.isactive = true and Jobworkbillitems.oqty-(rqty+dqty)>0 ";

        const string SQL_SearchAll = @"select jobworkbillid, oqty, iqty, dqty, rqty, jobworkbillitems.modifiedon, jobworkbillitems.createdon, jobworkbillitems.version, jobworkbillitems.modifiedby,
jobworkbillitems.id, jobworkbillitems.sno, jobworkbillitems.barcode, salablegoodsid, stockid, jobworkbillitems.productid, jobworkbillitems.invoiceitemid, 
jobworkbillitems.isactive, jobworkbillitems.createdby, jobworkbillitems.hsncode, 
materialsalevalue, materialcost, chargerate, chargetax, chargetaxpercentage, chargetaxid,chargeid, chargegross, chargetotal, 
outproductid,outproduct.name outproductname,products.name productname,brand.name brandname,style.name stylename,type.name typename,
stock.buyingprice,stock.ibuyingprice,stock.price from jobworkbillitems 
left outer join lrinvoiceitems on lrinvoiceitems.id = invoiceitemid
inner join stock on stock.id = stockid
inner join items on items.id = stock.itemid
left outer join products on products.id = items.productid
left outer join products outproduct on outproduct.id = jobworkbillitems.outproductid
left outer join brand on brand.id = items.brandid
left outer join referencelist style on style.id = items.styleid
left outer join referencelist type on type.id = items.typeid
        where jobworkbillitems.isactive=true and jobworkbillitems.jobworkbillid=@jobworkbillid";


        const string SQL_Insert = @"
        update stock set transfered=transfered-@iqty where id=@stockid;
        update salablegoods set iqty=iqty-@iqty where id=@salablegoodsid; 
        INSERT INTO public.jobworkbillitems(jobworkbillid, oqty, iqty, dqty, rqty, modifiedon, createdon, version, modifiedby, sno, barcode, salablegoodsid, stockid, productid, invoiceitemid, isactive, createdby, hsncode, materialsalevalue, materialcost, chargerate, chargetax, chargetaxpercentage, chargetaxid,chargeid, chargegross, chargetotal, outproductid) 
        VALUES(@jobworkbillid, @oqty, @iqty, @dqty, @rqty, @modifiedon, @createdon, @version, @modifiedby, @sno, @barcode, @salablegoodsid, @stockid, @productid, @invoiceitemid, @isactive, @createdby, @hsncode, @materialsalevalue, @materialcost, @chargerate, @chargetax, @chargetaxpercentage, @chargetaxid,@chargeid, @chargegross, @chargetotal, @outproductid) RETURNING id";


        const string SQL_Update = @"
        update stock set transfered=transfered+jobworkbillitems.iqty-@iqty from jobworkbillitems
        where jobworkbillitems.id = @id and stock.id=jobworkbillitems.stockid;

        update salablegoods set iqty=salablegoods.iqty+jobworkbillitems.iqty-@iqty 
        from jobworkbillitems
        where jobworkbillitems.id = @id and salablegoods.id=jobworkbillitems.salablegoodsid;

        UPDATE public.jobworkbillitems SET jobworkbillid=@jobworkbillid, oqty=@oqty, iqty=@iqty, dqty=@dqty, rqty=@rqty, modifiedon=@modifiedon, version=@version+1, modifiedby=@modifiedby, sno=@sno, barcode=@barcode, salablegoodsid=@salablegoodsid, stockid=@stockid, productid=@productid, invoiceitemid=@invoiceitemid, hsncode=@hsncode, outproductid=@outproductid,
        materialsalevalue=@materialsalevalue, materialcost=@materialcost, chargerate=@chargerate, chargetax=@chargetax, chargetaxpercentage=@chargetaxpercentage, chargetaxid=@chargetaxid,chargeid=@chargeid, chargegross=@chargegross, chargetotal=@chargetotal where id=@id and version=@version RETURNING version";

        const string SQL_Delete = @"
        update stock set transfered=transfered+jobworkbillitems.iqty from jobworkbillitems
        where jobworkbillitems.id = @id and stock.id=jobworkbillitems.stockid;

        update salablegoods set iqty=salablegoods.iqty+jobworkbillitems.iqty 
        from jobworkbillitems
        where jobworkbillitems.id = @id and salablegoods.id=jobworkbillitems.salablegoodsid;

        update public.jobworkbillitems set isactive=false,version=@version+1,modifiedby=@modifiedby,modifiedon=@modifiedon where id=@id and version=@version";

        const string SQL_DeleteAll = @"
        update stock set transfered=transfered+jobworkbillitems.iqty from jobworkbillitems
        where jobworkbillitems.jobworkbillid = @jobworkbillid and stock.id=jobworkbillitems.stockid;

        update salablegoods set iqty=salablegoods.iqty+jobworkbillitems.iqty 
        from jobworkbillitems
        where jobworkbillitems.jobworkbillid = @jobworkbillid and salablegoods.id=jobworkbillitems.salablegoodsid;

        update public.jobworkbillitems set isactive=false,modifiedby=@modifiedby,modifiedon=@modifiedon where jobworkbillid=@jobworkbillid";
        public List<Jobworkbillitems> Search(JobworkbillitemsCreteria _jobworkbillitems)
        {
            List<Jobworkbillitems> lst = new List<Jobworkbillitems>();
            NpgsqlConnection conn = CreateConnection();
            try
            {
                conn.Open();

                NpgsqlCommand command = null;
                if (_jobworkbillitems.id != 0)
                {
                    command = new NpgsqlCommand(SQL_Search, conn);
                    command.Parameters.Add("id", NpgsqlTypes.NpgsqlDbType.Bigint).Value = _jobworkbillitems.id;
                }
                else
                {
                    QueryBuilder qb = new QueryBuilder();
                    qb.CurrentQuery = SQL_Search.Substring(0, SQL_Search.IndexOf("where"));
                    qb.AddParameter("isactive", true, "=");
                    if (_jobworkbillitems.jobworkbillid > 0)
                        qb.AddParameter("jobworkbillid", _jobworkbillitems.jobworkbillid, "=");
                    command = qb.GetCommand(conn);
                }

                using (System.Data.IDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        Jobworkbillitems ojobworkbillitems = new Jobworkbillitems();
                        ojobworkbillitems.jobworkbillid = reader["jobworkbillid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["jobworkbillid"]);
                        ojobworkbillitems.oqty = reader["oqty"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["oqty"]);
                        ojobworkbillitems.iqty = reader["iqty"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["iqty"]);
                        ojobworkbillitems.dqty = reader["dqty"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["dqty"]);
                        ojobworkbillitems.rqty = reader["rqty"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["rqty"]);
                        ojobworkbillitems.modifiedon = reader["modifiedon"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(reader["modifiedon"]);
                        ojobworkbillitems.createdon = reader["createdon"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(reader["createdon"]);
                        ojobworkbillitems.version = reader["version"] == DBNull.Value ? 0 : Convert.ToInt32(reader["version"]);
                        ojobworkbillitems.modifiedby = reader["modifiedby"] == DBNull.Value ? 0 : Convert.ToInt64(reader["modifiedby"]);
                        ojobworkbillitems.id = reader["id"] == DBNull.Value ? 0 : Convert.ToInt64(reader["id"]);
                        ojobworkbillitems.sno = reader["sno"] == DBNull.Value ? 0 : Convert.ToInt32(reader["sno"]);
                        ojobworkbillitems.barcode = reader["barcode"] == DBNull.Value ? "" : reader["barcode"].ToString();
                        ojobworkbillitems.salablegoodsid = reader["salablegoodsid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["salablegoodsid"]);
                        ojobworkbillitems.stockid = reader["stockid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["stockid"]);
                        ojobworkbillitems.productid = reader["productid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["productid"]);
                        ojobworkbillitems.outproductid = reader["outproductid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["outproductid"]);
                        ojobworkbillitems.invoiceitemid = reader["invoiceitemid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["invoiceitemid"]);
                        ojobworkbillitems.isactive = reader["isactive"] == DBNull.Value ? false : Convert.ToBoolean(reader["isactive"]);
                        ojobworkbillitems.createdby = reader["createdby"] == DBNull.Value ? 0 : Convert.ToInt64(reader["createdby"]);
                        ojobworkbillitems.hsncode = reader["hsncode"] == DBNull.Value ? "" : reader["hsncode"].ToString();
                        ojobworkbillitems.materialsalevalue = reader["materialsalevalue"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["materialsalevalue"]);
                        ojobworkbillitems.materialcost = reader["materialcost"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["materialcost"]);
                        ojobworkbillitems.chargerate = reader["chargerate"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["chargerate"]);
                        ojobworkbillitems.chargetax = reader["chargetax"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["chargetax"]);
                        ojobworkbillitems.chargetaxpercentage = reader["chargetaxpercentage"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["chargetaxpercentage"]);
                        ojobworkbillitems.chargetaxid = reader["chargetaxid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["chargetaxid"]);
                        ojobworkbillitems.chargeid = reader["chargeid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["chargeid"]);
                        ojobworkbillitems.chargegross = reader["chargegross"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["chargegross"]);
                        ojobworkbillitems.chargetotal = reader["chargetotal"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["chargetotal"]);
                        lst.Add(ojobworkbillitems);
                    }
                }
                conn.Close();
            }
            catch (Exception exp)
            {
                if (conn != null) conn.Close();
                WebAPI.Common.Logger.Instance(Context.OrganizationID).Log(exp, WebAPI.Common.Logger.LogType.Error, _jobworkbillitems);
            }
            return lst;
        }

        internal List<Jobworkitems> SearchJobWorkItems(JobworkbillCreteria _jobworbill)
        {
            List<Jobworkitems> lst = new List<Jobworkitems>();
            NpgsqlConnection conn = CreateConnection();
            try
            {
                conn.Open();
                NpgsqlCommand command = null;
                QueryBuilder qb = new QueryBuilder();
                qb.CurrentQuery = SQL_JobWorkItems;
                qb.AddParameter("jobworkbill.isactive", true, "=");
               
                if (_jobworbill.companyid > 0)
                    qb.AddParameter("jobworkbill.companyid", _jobworbill.companyid, "=");
                if (_jobworbill.supplierid > 0)
                    qb.AddParameter("jobworkbill.supplierid", _jobworbill.supplierid, "=");
                if (_jobworbill.entryfrom != DateTime.MinValue && _jobworbill.entryto != DateTime.MinValue)
                {
                    qb.AddParameter("jobworkbill.entrydate", "datefrom", _jobworbill.entryfrom.Date, ">=");
                    qb.AddParameter("jobworkbill.entrydate", "dateto", _jobworbill.entryto.Date.AddDays(1), "<");
                }

                string statuscondition = @"and Jobworkbillitems.oqty-(rqty+dqty)>0";
                if (_jobworbill.status > 0)
                {
                    if (_jobworbill.status == 1)
                        statuscondition = @"and Jobworkbillitems.oqty-(rqty+dqty)=0";
                    else if (_jobworbill.status == 2)
                        statuscondition = "";
                }
                qb.CurrentQuery = qb.CurrentQuery.Replace(@"and Jobworkbillitems.oqty-(rqty+dqty)>0", statuscondition);
                qb.SortField = "jobworkbill.id desc ";
                command = qb.GetCommand(conn);
                using (System.Data.IDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        Jobworkitems ojobworkbillitems = new Jobworkitems();
                        ojobworkbillitems.id = reader["id"] == DBNull.Value ? 0 : Convert.ToInt64(reader["id"]);
                        ojobworkbillitems.barcode = reader["barcode"] == DBNull.Value ? "" : reader["barcode"].ToString();
                        ojobworkbillitems.companyid = reader["companyid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["companyid"]);
                        ojobworkbillitems.oqty = reader["oqty"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["oqty"]);
                        ojobworkbillitems.rqty = reader["rqty"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["rqty"]);
                        ojobworkbillitems.pqty = reader["pqty"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["pqty"]);
                        ojobworkbillitems.entrydate = reader["entrydate"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(reader["entrydate"]);
                        ojobworkbillitems.invoicedate = reader["invoicedate"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(reader["invoicedate"]);
                        ojobworkbillitems.invoiceno = reader["invoiceno"] == DBNull.Value ? "" : reader["invoiceno"].ToString();
                        ojobworkbillitems.jobworker = reader["jobworker"] == DBNull.Value ? "" : reader["jobworker"].ToString();
                        ojobworkbillitems.sourcesupplier = reader["sourcesupplier"] == DBNull.Value ? "" : reader["sourcesupplier"].ToString();
                        ojobworkbillitems.inproduct = reader["inproduct"] == DBNull.Value ? "" : reader["inproduct"].ToString();
                        ojobworkbillitems.outproduct = reader["outproduct"] == DBNull.Value ? "" : reader["outproduct"].ToString();
                        ojobworkbillitems.materialsalevalue = reader["materialsalevalue"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["materialsalevalue"]);
                        ojobworkbillitems.materialcost = reader["materialcost"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["materialcost"]);
                        ojobworkbillitems.chargerate = reader["chargerate"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["chargerate"]);
                        ojobworkbillitems.chargegross = reader["chargegross"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["chargegross"]);
                        ojobworkbillitems.chargetotal = reader["chargetotal"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["chargetotal"]);
                        ojobworkbillitems.grndate = reader["Jobworkbillgrndate"] == DBNull.Value ? "" : Convert.ToDateTime(reader["Jobworkbillgrndate"]).ToString("dd-MM-yyyy");
                        ojobworkbillitems.grnno = reader["Jobworkbillgrnno"] == DBNull.Value ? "" : reader["Jobworkbillgrnno"].ToString();

                        lst.Add(ojobworkbillitems);
                    }
                }
                conn.Close();

            }
            catch (Exception exp)
            {
                if (conn != null) conn.Close();
                WebAPI.Common.Logger.Instance(Context.OrganizationID).Log(exp, WebAPI.Common.Logger.LogType.Error, "");
            }
            return lst;
        }

        internal List<JobworkbillitemsSummary> SearchSummary(JobworkbillCreteria _jobworbill)
        {
            List<JobworkbillitemsSummary> lst = new List<JobworkbillitemsSummary>();
            NpgsqlConnection conn = CreateConnection();
            try
            {
                conn.Open();
                NpgsqlCommand command = null;
                QueryBuilder qb = new QueryBuilder();
                qb.CurrentQuery = SQL_JobWorkSummary;
                qb.AddParameter("jobworkbill.isactive", true, "=");
                if (_jobworbill.companyid > 0)
                    qb.AddParameter("jobworkbill.companyid", _jobworbill.companyid, "=");
                if (_jobworbill.supplierid > 0)
                    qb.AddParameter("jobworkbill.supplierid", _jobworbill.supplierid, "=");


                string statuscondition = @"and Jobworkbillitems.oqty-(rqty+dqty)>0";
                if(_jobworbill.status > 0)
                {
                    if (_jobworbill.status == 1)
                        statuscondition = @"and Jobworkbillitems.oqty-(rqty+dqty)=0";
                    else if (_jobworbill.status == 2)
                        statuscondition = "";
                }
                qb.CurrentQuery = qb.CurrentQuery.Replace(@"and Jobworkbillitems.oqty-(rqty+dqty)>0", statuscondition);
                qb.SortField = "jobworkbill.id desc ";
                command = qb.GetCommand(conn);
                using (System.Data.IDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        JobworkbillitemsSummary ojobworkbillitems = new JobworkbillitemsSummary();
                        ojobworkbillitems.id = reader["id"] == DBNull.Value ? 0 : Convert.ToInt64(reader["id"]);
                        ojobworkbillitems.companyid = reader["companyid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["companyid"]);
                        ojobworkbillitems.outproductid = reader["outproductid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["outproductid"]);
                        ojobworkbillitems.suppliercompanyid = reader["suppliercompanyid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["suppliercompanyid"]);
                        ojobworkbillitems.oqty = reader["oqty"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["oqty"]);
                        ojobworkbillitems.pqty = reader["pqty"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["pqty"]);
                        ojobworkbillitems.entrydate = reader["entrydate"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(reader["entrydate"]);
                        ojobworkbillitems.invoicedate = reader["invoicedate"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(reader["invoicedate"]);
                        ojobworkbillitems.expecteddate = reader["expecteddate"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(reader["expecteddate"]); 
                        ojobworkbillitems.invoiceno = reader["invoiceno"] == DBNull.Value ? "" : reader["invoiceno"].ToString();
                        ojobworkbillitems.jobworker = reader["jobworker"] == DBNull.Value ? "" : reader["jobworker"].ToString();
                        ojobworkbillitems.sourcesupplier = reader["sourcesupplier"] == DBNull.Value ? "" : reader["sourcesupplier"].ToString();
                        ojobworkbillitems.inproduct = reader["inproduct"] == DBNull.Value ? "" : reader["inproduct"].ToString();
                        ojobworkbillitems.outproduct = reader["outproduct"] == DBNull.Value ? "" : reader["outproduct"].ToString();
                        lst.Add(ojobworkbillitems);
                    }
                }
                conn.Close();

            }
            catch (Exception exp)
            {
                if (conn != null) conn.Close();
                WebAPI.Common.Logger.Instance(Context.OrganizationID).Log(exp, WebAPI.Common.Logger.LogType.Error, "");
            }
            return lst;
        }
        internal List<JobworkbillitemsPending> SearchPending(JobworkbillitemsCreteria _jobworkbillitems)
        {
            List<JobworkbillitemsPending> lst = new List<JobworkbillitemsPending>();
            NpgsqlConnection conn = CreateConnection();
            try
            {
                conn.Open();
                NpgsqlCommand command = null;
                QueryBuilder qb = new QueryBuilder();
                qb.CurrentQuery = SQL_SearchPending;
                
                if(_jobworkbillitems.jobworkbillgrnid>0)
                {
                    qb.CurrentQuery = SQL_SearchPending.Replace("and Jobworkbillitems.oqty-(rqty+dqty)>0",
                        "and (Jobworkbillitems.oqty-(rqty+dqty)>0 or Jobworkbillitems.id in (select jobworkbillitemsid from jobworkbillgrnitems where jobworkbillgrnid=@jobworkbillgrnid))");
                }

                //if (!string.IsNullOrEmpty(_jobworkbillitems.invoiceno))
                //    qb.AddParameter("Jobworkbill.invoiceno", _jobworkbillitems.invoiceno, "=");
                if (_jobworkbillitems.supplierid>0)
                    qb.AddParameter("Jobworkbill.supplierid", _jobworkbillitems.supplierid, "=");
                if (_jobworkbillitems.suppliercompanyid > 0)
                    qb.AddParameter("Jobworkbill.suppliercompanyid", _jobworkbillitems.suppliercompanyid, "=");
                if (_jobworkbillitems.companyid > 0)
                    qb.AddParameter("Jobworkbill.companyid", _jobworkbillitems.companyid, "=");
                if (_jobworkbillitems.locationid > 0)
                    qb.AddParameter("Jobworkbill.locationid", _jobworkbillitems.locationid, "=");
                if (_jobworkbillitems.jobworkids != null && _jobworkbillitems.jobworkids.Count > 0)
                    qb.AddInParamer("Jobworkbill.id", string.Join(",", _jobworkbillitems.jobworkids));

                command = qb.GetCommand(conn);
                command.Parameters.AddWithValue("@jobworkbillgrnid", _jobworkbillitems.jobworkbillgrnid);
                using (System.Data.IDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        JobworkbillitemsPending ojobworkbillitems = new JobworkbillitemsPending();
                        ojobworkbillitems.jobworkbillid = reader["jobworkbillid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["jobworkbillid"]);
                        ojobworkbillitems.oqty = reader["oqty"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["oqty"]);
                        ojobworkbillitems.iqty = reader["iqty"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["iqty"]);
                        ojobworkbillitems.dqty = reader["dqty"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["dqty"]);
                        ojobworkbillitems.rqty = reader["rqty"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["rqty"]);
                        ojobworkbillitems.entrydate = reader["entrydate"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(reader["entrydate"]);
                        ojobworkbillitems.modifiedon = reader["modifiedon"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(reader["modifiedon"]);
                        ojobworkbillitems.createdon = reader["createdon"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(reader["createdon"]);
                        ojobworkbillitems.version = reader["version"] == DBNull.Value ? 0 : Convert.ToInt32(reader["version"]);
                        ojobworkbillitems.modifiedby = reader["modifiedby"] == DBNull.Value ? 0 : Convert.ToInt64(reader["modifiedby"]);

                        ojobworkbillitems.suppliercompanyid = reader["suppliercompanyid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["suppliercompanyid"]);
                        ojobworkbillitems.supplierid = reader["supplierid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["supplierid"]);
                        ojobworkbillitems.locationid = reader["locationid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["locationid"]);
                        ojobworkbillitems.companyid = reader["companyid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["companyid"]);
                        ojobworkbillitems.itemid = reader["itemid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["itemid"]);
                        ojobworkbillitems.id = reader["id"] == DBNull.Value ? 0 : Convert.ToInt64(reader["id"]);
                        ojobworkbillitems.sno = reader["sno"] == DBNull.Value ? 0 : Convert.ToInt32(reader["sno"]);
                        ojobworkbillitems.barcode = reader["barcode"] == DBNull.Value ? "" : reader["barcode"].ToString();
                        ojobworkbillitems.imageid = reader["imageid"] == DBNull.Value ? "" : reader["imageid"].ToString(); 
                        ojobworkbillitems.designid = reader["designid"] == DBNull.Value ? "" : reader["designid"].ToString();
                        ojobworkbillitems.invoiceno = reader["invoiceno"] == DBNull.Value ? "" : reader["invoiceno"].ToString();
                        ojobworkbillitems.salablegoodsid = reader["salablegoodsid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["salablegoodsid"]);
                        ojobworkbillitems.stockid = reader["stockid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["stockid"]);
                        ojobworkbillitems.productid = reader["productid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["productid"]);
                        ojobworkbillitems.outproductid = reader["outproductid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["outproductid"]);
                        ojobworkbillitems.invoiceitemid = reader["invoiceitemid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["invoiceitemid"]);
                        ojobworkbillitems.isactive = reader["isactive"] == DBNull.Value ? false : Convert.ToBoolean(reader["isactive"]);
                        ojobworkbillitems.createdby = reader["createdby"] == DBNull.Value ? 0 : Convert.ToInt64(reader["createdby"]);
                        ojobworkbillitems.hsncode = reader["hsncode"] == DBNull.Value ? "" : reader["hsncode"].ToString();
                        ojobworkbillitems.materialsalevalue = reader["materialsalevalue"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["materialsalevalue"]);
                        ojobworkbillitems.materialcost = reader["materialcost"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["materialcost"]);
                        ojobworkbillitems.chargerate = reader["chargerate"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["chargerate"]);
                        ojobworkbillitems.chargetax = reader["chargetax"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["chargetax"]);
                        ojobworkbillitems.chargetaxpercentage = reader["chargetaxpercentage"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["chargetaxpercentage"]);
                        ojobworkbillitems.chargetaxid = reader["chargetaxid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["chargetaxid"]);
                        ojobworkbillitems.chargeid = reader["chargeid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["chargeid"]);
                        ojobworkbillitems.chargegross = reader["chargegross"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["chargegross"]);
                        ojobworkbillitems.chargetotal = reader["chargetotal"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["chargetotal"]);
                        if(lst.Count>0 && lst[0].supplierid != ojobworkbillitems.supplierid &&
                            ojobworkbillitems.locationid != lst[0].supplierid)
                        {
                            continue;
                        }
                        lst.Add(ojobworkbillitems);
                    }
                }
                

                if (_jobworkbillitems.jobworkbillgrnid > 0)
                {
                    JobworkbillgrnitemsDataService jobworkbillgrnitemsDataService = new JobworkbillgrnitemsDataService() { Context = this.Context };
                    var items = jobworkbillgrnitemsDataService.SearchAll(_jobworkbillitems.jobworkbillgrnid);
                    foreach (var item in items)
                    {
                        if (lst.Exists(e => e.id == item.jobworkbillitemsid))
                        {
                            lst.Find(e => e.id == item.jobworkbillitemsid).rqty -= item.qty;
                        }
                    }
                }

                if(_jobworkbillitems.includeitemdetails)
                {
                    ItemsDataService itemsDataService = new ItemsDataService() { Context = this.Context };
                    var ids = lst.ConvertAll(e => e.itemid);
                    if (ids.Count > 0)
                    {
                        var items = itemsDataService.SearchInfo(ids, conn);
                        foreach (var item in items)
                        {
                            lst.FindAll(e => e.itemid == item.id).ForEach(x => x.iteminfo = item);
                        }
                    }
                }
                conn.Close();
            }
            catch (Exception exp)
            {
                if (conn != null) conn.Close();
                WebAPI.Common.Logger.Instance(Context.OrganizationID).Log(exp, WebAPI.Common.Logger.LogType.Error, _jobworkbillitems);
            }
            return lst;
        }

        public List<JobworkbillitemsDetail> SearchAll(long _jobworkbillid)
        {
            List<JobworkbillitemsDetail> lst = new List<JobworkbillitemsDetail>();
            NpgsqlConnection conn = CreateConnection();
            try
            {
                conn.Open();

                NpgsqlCommand command = new NpgsqlCommand(SQL_SearchAll, conn);
                command.Parameters.Add("jobworkbillid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = _jobworkbillid;
                using (System.Data.IDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        JobworkbillitemsDetail ojobworkbillitems = new JobworkbillitemsDetail();
                        ojobworkbillitems.jobworkbillid = reader["jobworkbillid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["jobworkbillid"]);
                        ojobworkbillitems.oqty = reader["oqty"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["oqty"]);
                        ojobworkbillitems.iqty = reader["iqty"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["iqty"]);
                        ojobworkbillitems.dqty = reader["dqty"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["dqty"]);
                        ojobworkbillitems.rqty = reader["rqty"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["rqty"]);
                        ojobworkbillitems.modifiedon = reader["modifiedon"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(reader["modifiedon"]);
                        ojobworkbillitems.createdon = reader["createdon"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(reader["createdon"]);
                        ojobworkbillitems.version = reader["version"] == DBNull.Value ? 0 : Convert.ToInt32(reader["version"]);
                        ojobworkbillitems.modifiedby = reader["modifiedby"] == DBNull.Value ? 0 : Convert.ToInt64(reader["modifiedby"]);
                        ojobworkbillitems.id = reader["id"] == DBNull.Value ? 0 : Convert.ToInt64(reader["id"]);
                        ojobworkbillitems.sno = reader["sno"] == DBNull.Value ? 0 : Convert.ToInt32(reader["sno"]);
                        ojobworkbillitems.barcode = reader["barcode"] == DBNull.Value ? "" : reader["barcode"].ToString();
                        ojobworkbillitems.salablegoodsid = reader["salablegoodsid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["salablegoodsid"]);
                        ojobworkbillitems.stockid = reader["stockid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["stockid"]);
                        ojobworkbillitems.productid = reader["productid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["productid"]);
                        ojobworkbillitems.outproductid = reader["outproductid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["outproductid"]);
                        ojobworkbillitems.invoiceitemid = reader["invoiceitemid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["invoiceitemid"]);
                        ojobworkbillitems.isactive = reader["isactive"] == DBNull.Value ? false : Convert.ToBoolean(reader["isactive"]);
                        ojobworkbillitems.createdby = reader["createdby"] == DBNull.Value ? 0 : Convert.ToInt64(reader["createdby"]);
                        ojobworkbillitems.hsncode = reader["hsncode"] == DBNull.Value ? "" : reader["hsncode"].ToString();
                        ojobworkbillitems.materialsalevalue = reader["materialsalevalue"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["materialsalevalue"]);
                        ojobworkbillitems.materialcost = reader["materialcost"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["materialcost"]);
                        ojobworkbillitems.chargerate = reader["chargerate"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["chargerate"]);
                        ojobworkbillitems.chargetax = reader["chargetax"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["chargetax"]);
                        ojobworkbillitems.chargetaxpercentage = reader["chargetaxpercentage"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["chargetaxpercentage"]);
                        ojobworkbillitems.chargetaxid = reader["chargetaxid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["chargetaxid"]);
                        ojobworkbillitems.chargeid = reader["chargeid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["chargeid"]);
                        ojobworkbillitems.chargegross = reader["chargegross"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["chargegross"]);
                        ojobworkbillitems.chargetotal = reader["chargetotal"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["chargetotal"]);

                        ojobworkbillitems.outproductname = reader["outproductname"] == DBNull.Value ? "" : reader["outproductname"].ToString();
                        ojobworkbillitems.productname = reader["productname"] == DBNull.Value ? "" : reader["productname"].ToString();
                        ojobworkbillitems.brandname = reader["brandname"] == DBNull.Value ? "" : reader["brandname"].ToString();
                        ojobworkbillitems.stylename = reader["outproductname"] == DBNull.Value ? "" : reader["stylename"].ToString();
                        ojobworkbillitems.typename = reader["typename"] == DBNull.Value ? "" : reader["typename"].ToString();

                        lst.Add(ojobworkbillitems);
                    }
                }
                conn.Close();
            }
            catch (Exception exp)
            {
                if (conn != null) conn.Close();
                WebAPI.Common.Logger.Instance(Context.OrganizationID).Log(exp, WebAPI.Common.Logger.LogType.Error, _jobworkbillid);
            }
            return lst;
        }

        public Jobworkbillitems Insert(Jobworkbillitems oJobworkbillitems)
        {

            NpgsqlTransaction transaction = null;
            try
            {
                using (NpgsqlConnection conn = CreateConnection())
                {
                    conn.Open();
                    transaction = conn.BeginTransaction();
                    NpgsqlCommand command = new NpgsqlCommand(SQL_Insert, conn, transaction);

                    Insert(transaction, conn, oJobworkbillitems);
                    transaction.Commit();
                    conn.Close();
                }
            }
            catch (Exception exp)
            {
                WebAPI.Common.Logger.Instance(Context.OrganizationID).Log(exp, WebAPI.Common.Logger.LogType.Error, oJobworkbillitems);
            }

            return oJobworkbillitems;
        }

        public Jobworkbillitems Insert(NpgsqlTransaction transaction, NpgsqlConnection conn, Jobworkbillitems oJobworkbillitems)
        {

            NpgsqlCommand command = new NpgsqlCommand(SQL_Insert, conn, transaction);
            command.Parameters.Add("jobworkbillid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oJobworkbillitems.jobworkbillid;
            command.Parameters.Add("oqty", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oJobworkbillitems.oqty;
            command.Parameters.Add("iqty", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oJobworkbillitems.iqty;
            command.Parameters.Add("dqty", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oJobworkbillitems.dqty;
            command.Parameters.Add("rqty", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oJobworkbillitems.rqty;
            command.Parameters.Add("modifiedon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = oJobworkbillitems.modifiedon;
            command.Parameters.Add("createdon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = oJobworkbillitems.createdon;
            command.Parameters.Add("version", NpgsqlTypes.NpgsqlDbType.Integer).Value = oJobworkbillitems.version;
            command.Parameters.Add("modifiedby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oJobworkbillitems.modifiedby;
            command.Parameters.Add("sno", NpgsqlTypes.NpgsqlDbType.Integer).Value = oJobworkbillitems.sno;
            command.Parameters.Add("barcode", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oJobworkbillitems.barcode == null ? "" : oJobworkbillitems.barcode;
            command.Parameters.Add("salablegoodsid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oJobworkbillitems.salablegoodsid;
            command.Parameters.Add("stockid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oJobworkbillitems.stockid;
            command.Parameters.Add("productid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oJobworkbillitems.productid;
            command.Parameters.Add("outproductid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oJobworkbillitems.outproductid;
            command.Parameters.Add("invoiceitemid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oJobworkbillitems.invoiceitemid;
            command.Parameters.Add("isactive", NpgsqlTypes.NpgsqlDbType.Boolean).Value = oJobworkbillitems.isactive;
            command.Parameters.Add("createdby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oJobworkbillitems.createdby;
            command.Parameters.Add("hsncode", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oJobworkbillitems.hsncode == null ? "" : oJobworkbillitems.hsncode;
            command.Parameters.Add("materialsalevalue", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oJobworkbillitems.materialsalevalue;
            command.Parameters.Add("materialcost", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oJobworkbillitems.materialcost;
            command.Parameters.Add("chargerate", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oJobworkbillitems.chargerate;
            command.Parameters.Add("chargetax", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oJobworkbillitems.chargetax;
            command.Parameters.Add("chargetaxpercentage", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oJobworkbillitems.chargetaxpercentage;
            command.Parameters.Add("chargetaxid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oJobworkbillitems.chargetaxid;
            command.Parameters.Add("chargeid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oJobworkbillitems.chargeid;
            command.Parameters.Add("chargegross", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oJobworkbillitems.chargegross;
            command.Parameters.Add("chargetotal", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oJobworkbillitems.chargetotal;
            using (System.Data.IDataReader reader = command.ExecuteReader())
            {
                if (reader.Read())
                {
                    oJobworkbillitems.id = Convert.ToInt64(reader[0].ToString());
                }
            }
            return oJobworkbillitems;
        }

        public Jobworkbillitems Update(Jobworkbillitems oJobworkbillitems)
        {

            NpgsqlTransaction transaction = null;
            try
            {
                using (NpgsqlConnection conn = CreateConnection())
                {
                    conn.Open();
                    transaction = conn.BeginTransaction();

                    Update(transaction, conn, oJobworkbillitems);
                    transaction.Commit();
                    conn.Close();
                }
            }
            catch (Exception exp)
            {
                WebAPI.Common.Logger.Instance(Context.OrganizationID).Log(exp, WebAPI.Common.Logger.LogType.Error, oJobworkbillitems);
            }

            return oJobworkbillitems;
        }

        public Jobworkbillitems Update(NpgsqlTransaction transaction, NpgsqlConnection conn, Jobworkbillitems oJobworkbillitems)
        {

            NpgsqlCommand command = new NpgsqlCommand(SQL_Update, conn, transaction);
            command.Parameters.Add("jobworkbillid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oJobworkbillitems.jobworkbillid;
            command.Parameters.Add("oqty", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oJobworkbillitems.oqty;
            command.Parameters.Add("iqty", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oJobworkbillitems.iqty;
            command.Parameters.Add("dqty", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oJobworkbillitems.dqty;
            command.Parameters.Add("rqty", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oJobworkbillitems.rqty;
            command.Parameters.Add("modifiedon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = oJobworkbillitems.modifiedon;
            command.Parameters.Add("createdon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = oJobworkbillitems.createdon;
            command.Parameters.Add("version", NpgsqlTypes.NpgsqlDbType.Integer).Value = oJobworkbillitems.version;
            command.Parameters.Add("modifiedby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oJobworkbillitems.modifiedby;
            command.Parameters.Add("id", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oJobworkbillitems.id;
            command.Parameters.Add("sno", NpgsqlTypes.NpgsqlDbType.Integer).Value = oJobworkbillitems.sno;
            command.Parameters.Add("barcode", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oJobworkbillitems.barcode == null ? "" : oJobworkbillitems.barcode;
            command.Parameters.Add("salablegoodsid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oJobworkbillitems.salablegoodsid;
            command.Parameters.Add("stockid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oJobworkbillitems.stockid;
            command.Parameters.Add("productid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oJobworkbillitems.productid;
            command.Parameters.Add("outproductid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oJobworkbillitems.outproductid;
            command.Parameters.Add("invoiceitemid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oJobworkbillitems.invoiceitemid;
            command.Parameters.Add("isactive", NpgsqlTypes.NpgsqlDbType.Boolean).Value = oJobworkbillitems.isactive;
            command.Parameters.Add("createdby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oJobworkbillitems.createdby;
            command.Parameters.Add("hsncode", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oJobworkbillitems.hsncode == null ? "" : oJobworkbillitems.hsncode;
            command.Parameters.Add("materialsalevalue", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oJobworkbillitems.materialsalevalue;
            command.Parameters.Add("materialcost", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oJobworkbillitems.materialcost;
            command.Parameters.Add("chargerate", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oJobworkbillitems.chargerate;
            command.Parameters.Add("chargetax", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oJobworkbillitems.chargetax;
            command.Parameters.Add("chargetaxpercentage", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oJobworkbillitems.chargetaxpercentage;
            command.Parameters.Add("chargetaxid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oJobworkbillitems.chargetaxid;
            command.Parameters.Add("chargeid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oJobworkbillitems.chargeid;
            command.Parameters.Add("chargegross", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oJobworkbillitems.chargegross;
            command.Parameters.Add("chargetotal", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oJobworkbillitems.chargetotal;
            using (System.Data.IDataReader reader = command.ExecuteReader())
            {
                if (reader.Read())
                {
                    oJobworkbillitems.version = Convert.ToInt32(reader[0].ToString());
                }
            }

            return oJobworkbillitems;
        }

        public bool Update(NpgsqlTransaction transaction, NpgsqlConnection conn, List<JobworkbillitemsDetail> list, long id)
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
                if (item.id > 0)
                {
                    Update(transaction, conn, item);
                }
                else
                {
                    item.jobworkbillid = id;
                    item.createdon = DateTimeNow;
                    item.modifiedon = DateTimeNow;
                    item.createdby = Context.UserID;
                    item.modifiedby = Context.UserID;
                    item.isactive = true;
                    item.version = 1;

                    Insert(transaction, conn, item);
                }
            }
            return true;
        }

        public bool Delete(Jobworkbillitems oJobworkbillitems)
        {

            NpgsqlTransaction transaction = null;
            bool hasSuccessCount = false;
            try
            {
                using (NpgsqlConnection conn = CreateConnection())
                {
                    conn.Open();
                    transaction = conn.BeginTransaction();
                    hasSuccessCount = Delete(transaction, conn, oJobworkbillitems);
                    transaction.Commit();
                    conn.Close();
                }
            }
            catch (Exception exp)
            {
                WebAPI.Common.Logger.Instance(Context.OrganizationID).Log(exp, WebAPI.Common.Logger.LogType.Error, oJobworkbillitems);

            }
            return hasSuccessCount;

        }

        public bool Delete(NpgsqlTransaction transaction, NpgsqlConnection conn, Jobworkbillitems oJobworkbillitems)
        {
            var param = new SortedList<string, object>();
            param.Add("id", oJobworkbillitems.id);
            using (var reader = ExecuteReader("select count(*) from jobworkbillitems where id=@id and isactive=true and rqty>0", 
                param, conn, transaction))
            {
                if(reader.Read())
                {
                    if(!reader.IsDBNull(0) &&
                        Convert.ToInt32(reader[0])>0)
                    {
                        throw new Exception("Can't delete order items because already these items were received");
                    }
                }
            }

            NpgsqlCommand command = new NpgsqlCommand(SQL_Delete, conn, transaction);
            command.Parameters.Add("isactive", NpgsqlTypes.NpgsqlDbType.Boolean).Value = oJobworkbillitems.isactive;
            command.Parameters.Add("modifiedby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oJobworkbillitems.modifiedby;
            command.Parameters.Add("modifiedon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = oJobworkbillitems.modifiedon;
            command.Parameters.Add("version", NpgsqlTypes.NpgsqlDbType.Integer).Value = oJobworkbillitems.version;
            command.Parameters.Add("id", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oJobworkbillitems.id;
            int successCount = command.ExecuteNonQuery();

            return successCount > 0;
        }

        public bool DeleteAll(long jobworkbillid, long userid, NpgsqlConnection conn, NpgsqlTransaction transaction)
        {

            var param = new SortedList<string, object>();
            param.Add("id", jobworkbillid);
            using (var reader = ExecuteReader("select count(*) from jobworkbillitems where jobworkbillid=@id and isactive=true and rqty>0",
                param, conn, transaction))
            {
                if (reader.Read())
                {
                    if (!reader.IsDBNull(0) &&
                        Convert.ToInt32(reader[0]) > 0)
                    {
                        throw new Exception("Can't delete order items because already these items were received");
                    }
                }
            }
            int successCount = 0;
            NpgsqlCommand command = new NpgsqlCommand(SQL_DeleteAll, conn, transaction);
            command.Parameters.Add("isactive", NpgsqlTypes.NpgsqlDbType.Boolean).Value = false;
            command.Parameters.Add("modifiedby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = userid;
            command.Parameters.Add("modifiedon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = DateTimeNow;
            command.Parameters.Add("jobworkbillid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = jobworkbillid;
            successCount = command.ExecuteNonQuery();

            return successCount > 0;
        }

    }

}
