using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using InfyPOS.Models;
using InfyPOS.Common;
using Npgsql;
namespace InfyPOS.Data

{
    public class MfgworkorderDataService : SAASData
    {
        const string SQL_Search = @"select id, workorderno,jobcardno, locationid, wodate, expectdate, deliverydate, issuedby, isactive, createdby, modifiedby, version, createdon, modifiedon, tax, taxable, total, statusid, supplierid, suppliercompanyid, variance, 
        totalqty,fabricqty, receivedby, woflowid,onlybom,companyid,mode,product,design,sizelist,remarks,designid,lotno,customerid,mfgsampleorderid,proddivid from mfgworkorder where id=@id";

        const string SQL_Insert = @"INSERT INTO public.mfgworkorder(workorderno,jobcardno, locationid, wodate, expectdate, deliverydate, issuedby, isactive, createdby, modifiedby, version, createdon, modifiedon, tax, taxable, total, statusid, supplierid, suppliercompanyid, variance, 
        totalqty,fabricqty, receivedby, woflowid,onlybom,companyid,mode,product,design,sizelist,remarks,designid,lotno,customerid,mfgsampleorderid,proddivid) 
        VALUES(@workorderno,@jobcardno, @locationid, @wodate, @expectdate, @deliverydate, @issuedby, @isactive, @createdby, @modifiedby, @version, @createdon, @modifiedon, @tax, @taxable, @total, @statusid, @supplierid, @suppliercompanyid, @variance, 
        @totalqty,@fabricqty, @receivedby, @woflowid,@onlybom,@companyid,@mode,@product,@design,@sizelist,@remarks,@designid,@lotno,@customerid,@mfgsampleorderid,@proddivid) RETURNING id";

        const string SQL_WODetails = @"select workorderno,jobcardno, locationid, wodate, expectdate, deliverydate, productid,brandid,
sleeveid,colourid,products.name productname,brand.name brandname,colour.name colourname,sleeves.name sleevename,
issuedby, mfgworkorder.createdby, mfgworkorder.createdon, mfgworkorder.modifiedon, fabricqty,
tax, taxable, total, statusid, mfgworkorder.supplierid, suppliercompanyid, variance, totalqty, proddivid,
receivedby, woflowid,onlybom,companyid,mode,product,design,sizelist,remarks,designid,lotno,customerid,mfgsampleorderid
from mfgworkorder 
inner join mfgdesign on mfgdesign.code=mfgworkorder.design 
inner join products on products.id=mfgdesign.productid 
inner join brand on brand.id=mfgdesign.brandid 
left outer join referencelist colour on colour.id = mfgdesign.colourid
left outer join referencelist sleeves on sleeves.id = mfgdesign.sleeveid where mfgworkorder.id=@id";

        const string SQL_SODetails = @"select mfgdesign.code,workorderno,jobcardno, mfgworkorder.locationid, wodate, expectdate, 
deliverydate, mfgdesign.productid,brandid,
sleeveid,colourid,products.name productname,brand.name brandname,colour.name colourname,sleeves.name sleevename,
issuedby, mfgworkorder.createdby, mfgworkorder.createdon, mfgworkorder.modifiedon, fabricqty,
tax, taxable, total, mfgworkorder.statusid, mfgworkorder.supplierid, suppliercompanyid, variance, totalqty, proddivid,
receivedby, woflowid,onlybom,mfgworkorder.companyid,mode,product,design,sizelist,remarks,designid,lotno,
mfgworkorder.customerid,mfgsampleorderid
from mfgworkorder 
inner join mfgsampleorder on mfgsampleorder.id =mfgsampleorderid 
inner join mfgdesign on mfgdesign.id=mfgsampleorder.mfgdesignid 
inner join products on products.id=mfgdesign.productid 
left outer join brand on brand.id=mfgdesign.brandid 
left outer join referencelist colour on colour.id = mfgdesign.colourid
left outer join referencelist sleeves on sleeves.id = mfgdesign.sleeveid where mfgworkorder.id=@id";

        const string SQL_Update = @"UPDATE public.mfgworkorder SET workorderno=@workorderno,jobcardno=@jobcardno, locationid=@locationid, wodate=@wodate, expectdate=@expectdate, deliverydate=@deliverydate, issuedby=@issuedby, modifiedby=@modifiedby, version=@version+1, modifiedon=@modifiedon, 
        tax=@tax, taxable=@taxable, total=@total, statusid=@statusid, supplierid=@supplierid, suppliercompanyid=@suppliercompanyid, variance=@variance, totalqty=@totalqty, fabricqty=@fabricqty,lotno=@lotno,
        customerid=@customerid,mfgsampleorderid=@mfgsampleorderid,proddivid=@proddivid,receivedby=@receivedby, woflowid=@woflowid,onlybom=@onlybom, companyid=@companyid,mode=@mode,product=@product,design=@design,sizelist=@sizelist,remarks=@remarks,designid=@designid where id=@id and version=@version RETURNING version";

        const string SQL_Delete = @"update public.mfgworkorder set isactive=false,version=@version+1,modifiedby=@modifiedby,modifiedon=@modifiedon where id=@id and version=@version";

        public List<Mfgworkorder> Search(MfgworkorderCreteria _mfgworkorder)
        {
            List<Mfgworkorder> lst = new List<Mfgworkorder>();
            NpgsqlConnection conn = CreateConnection();
            try
            {
                conn.Open();

                NpgsqlCommand command = null;
                if (_mfgworkorder.id != 0)
                {
                    command = new NpgsqlCommand(SQL_Search, conn);
                    command.Parameters.Add("id", NpgsqlTypes.NpgsqlDbType.Bigint).Value = _mfgworkorder.id;
                }
                else
                {
                    QueryBuilder qb = new QueryBuilder();
                    qb.CurrentQuery = SQL_Search.Substring(0, SQL_Search.IndexOf("where"));
                    qb.AddParameter("isactive", true, "=");

                    if (!string.IsNullOrEmpty(_mfgworkorder.workorderno))
                        qb.AddParameter("workorderno", _mfgworkorder.workorderno, "=");

                    if (_mfgworkorder.companyid > 0)
                        qb.AddParameter("companyid", _mfgworkorder.companyid, "=");

                    if (!string.IsNullOrEmpty(_mfgworkorder.mode) &&
                        _mfgworkorder.mode != "All")
                        qb.AddParameter("mode", _mfgworkorder.mode, "=");

                    if (_mfgworkorder.suppliercompanyid > 0)
                        qb.AddParameter("suppliercompanyid", _mfgworkorder.suppliercompanyid, "=");

                    if (_mfgworkorder.fromdate.Year > 2000)
                        qb.AddParameter("wodate", "wofromdate", _mfgworkorder.fromdate.Date, ">=");

                    if (_mfgworkorder.todate.Year > 2000)
                        qb.AddParameter("wodate", "wotodate", _mfgworkorder.todate.Date.AddDays(1), "<");

                    if (_mfgworkorder.duefromdate.Year > 2000)
                        qb.AddParameter("deliverydate", "duefromdate", _mfgworkorder.duefromdate.Date, ">=");

                    if (_mfgworkorder.duetodate.Year > 2000)
                        qb.AddParameter("deliverydate", "duetodate", _mfgworkorder.duetodate.Date.AddDays(1), "<");

                    if (_mfgworkorder.showonlypending)
                        qb.AddParameter("statusid", 9, "!=");

                    command = qb.GetCommand(conn);
                }

                using (System.Data.IDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        Mfgworkorder omfgworkorder = new Mfgworkorder();
                        omfgworkorder.id = reader["id"] == DBNull.Value ? 0 : Convert.ToInt64(reader["id"]);

                        omfgworkorder.mode = reader["mode"] == DBNull.Value ? "" : reader["mode"].ToString();
                        omfgworkorder.companyid = reader["companyid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["companyid"]);

                        omfgworkorder.mfgsampleorderid = reader["mfgsampleorderid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["mfgsampleorderid"]);
                        omfgworkorder.customerid = reader["customerid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["customerid"]);
                        omfgworkorder.proddivid = reader["proddivid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["proddivid"]);

                        omfgworkorder.design = reader["design"] == DBNull.Value ? "" : reader["design"].ToString();
                        omfgworkorder.product = reader["product"] == DBNull.Value ? "" : reader["product"].ToString();
                        omfgworkorder.sizelist_json = reader["sizelist"] == DBNull.Value ? null : reader["sizelist"].ToString();
                        omfgworkorder.jobcardno = reader["jobcardno"] == DBNull.Value ? "" : reader["jobcardno"].ToString();
                        omfgworkorder.lotno = reader["lotno"] == DBNull.Value ? "" : reader["lotno"].ToString();
                        omfgworkorder.workorderno = reader["workorderno"] == DBNull.Value ? "" : reader["workorderno"].ToString();
                        omfgworkorder.locationid = reader["locationid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["locationid"]);
                        omfgworkorder.wodate = reader["wodate"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(reader["wodate"]);
                        omfgworkorder.expectdate = reader["expectdate"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(reader["expectdate"]);
                        omfgworkorder.deliverydate = reader["deliverydate"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(reader["deliverydate"]);
                        omfgworkorder.issuedby = reader["issuedby"] == DBNull.Value ? 0 : Convert.ToInt64(reader["issuedby"]);
                        omfgworkorder.isactive = reader["isactive"] == DBNull.Value ? false : Convert.ToBoolean(reader["isactive"]);
                        omfgworkorder.createdby = reader["createdby"] == DBNull.Value ? 0 : Convert.ToInt64(reader["createdby"]);
                        omfgworkorder.modifiedby = reader["modifiedby"] == DBNull.Value ? 0 : Convert.ToInt64(reader["modifiedby"]);
                        omfgworkorder.version = reader["version"] == DBNull.Value ? 0 : Convert.ToInt32(reader["version"]);
                        omfgworkorder.createdon = reader["createdon"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(reader["createdon"]);
                        omfgworkorder.modifiedon = reader["modifiedon"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(reader["modifiedon"]);
                        omfgworkorder.tax = reader["tax"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["tax"]);
                        omfgworkorder.taxable = reader["taxable"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["taxable"]);
                        omfgworkorder.fabricqty = reader["fabricqty"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["fabricqty"]);
                        omfgworkorder.total = reader["total"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["total"]);
                        omfgworkorder.statusid = reader["statusid"] == DBNull.Value ? 0 : Convert.ToInt32(reader["statusid"]);
                        omfgworkorder.supplierid = reader["supplierid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["supplierid"]);
                        omfgworkorder.suppliercompanyid = reader["suppliercompanyid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["suppliercompanyid"]);
                        omfgworkorder.variance = reader["variance"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["variance"]);
                        omfgworkorder.totalqty = reader["totalqty"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["totalqty"]);
                        omfgworkorder.receivedby = reader["receivedby"] == DBNull.Value ? 0 : Convert.ToInt64(reader["receivedby"]);
                        omfgworkorder.woflowid = reader["woflowid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["woflowid"]);
                        omfgworkorder.onlybom = reader["onlybom"] == DBNull.Value ? false : Convert.ToBoolean(reader["onlybom"]);
                        omfgworkorder.remarks = reader["remarks"] == DBNull.Value ? "" : reader["remarks"].ToString();
                        omfgworkorder.designid = reader["designid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["designid"]);
                        lst.Add(omfgworkorder);
                    }
                }
                if (_mfgworkorder.id > 0 && lst.Count == 1)
                {
                    MfgworkorderitemsDataService subMfgworkorderitems = new MfgworkorderitemsDataService() { Context = this.Context };
                    lst[0].Mfgworkorderitems = subMfgworkorderitems.SearchAll(_mfgworkorder.id);
                    MfgworkorderitemsizesDataService subMfgworkorderitemsizes = new MfgworkorderitemsizesDataService() { Context = this.Context };
                    var Mfgworkorderitemsizes = subMfgworkorderitemsizes.SearchAll(_mfgworkorder.id);
                    foreach (var item in lst[0].Mfgworkorderitems)
                    {
                        item.sizelist = Mfgworkorderitemsizes.FindAll(e => e.mfgworkorderitemsid == item.id);
                        item.sizelist.ForEach(e =>
                        {
                            e.name =
                            lst[0].sizelist.Exists(x => x.sizeid == e.sizeid) ?
                            lst[0].sizelist.Find(x => x.sizeid == e.sizeid).name : "";
                        });
                    }
                }
                conn.Close();
            }
            catch (Exception exp)
            {
                if (conn != null) conn.Close();
                WebAPI.Common.Logger.Instance(Context.OrganizationID).Log(exp, WebAPI.Common.Logger.LogType.Error, _mfgworkorder);
            }
            return lst;
        }

        public List<Mfgworkorder> SearchDetail(MfgworkorderCreteria _mfgworkorder)
        {
            List<Mfgworkorder> lst = new List<Mfgworkorder>();
            NpgsqlConnection conn = CreateConnection();
            try
            {
                conn.Open();

                NpgsqlCommand command = null;
                string query = InfyPOS.Services.ConfigManager.Instance(Context.OrganizationID).Config.mfg_mode != 6 ? SQL_WODetails : SQL_SODetails;
                if (_mfgworkorder.id != 0)
                {
                    command = new NpgsqlCommand(query, conn);
                    command.Parameters.Add("id", NpgsqlTypes.NpgsqlDbType.Bigint).Value = _mfgworkorder.id;
                }
                else
                {
                    QueryBuilder qb = new QueryBuilder();
                    qb.CurrentQuery = query.Substring(0, query.IndexOf("where"));
                    qb.AddParameter("isactive", true, "=");
                    command = qb.GetCommand(conn);
                }

                using (System.Data.IDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        Mfgworkorder omfgworkorder = new Mfgworkorder();
                        omfgworkorder.companyid = reader["companyid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["companyid"]);
                        omfgworkorder.design = reader["design"] == DBNull.Value ? "" : reader["design"].ToString();
                        omfgworkorder.product = reader["product"] == DBNull.Value ? "" : reader["product"].ToString();
                        omfgworkorder.workorderno = reader["workorderno"] == DBNull.Value ? "" : reader["workorderno"].ToString();
                        omfgworkorder.jobcardno = reader["jobcardno"] == DBNull.Value ? "" : reader["jobcardno"].ToString();
                        omfgworkorder.lotno = reader["lotno"] == DBNull.Value ? "" : reader["lotno"].ToString();
                        omfgworkorder.mfgsampleorderid = reader["mfgsampleorderid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["mfgsampleorderid"]);
                        omfgworkorder.proddivid = reader["proddivid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["proddivid"]);
                        omfgworkorder.customerid = reader["customerid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["customerid"]);
                        omfgworkorder.locationid = reader["locationid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["locationid"]);
                        omfgworkorder.wodate = reader["wodate"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(reader["wodate"]);
                        omfgworkorder.expectdate = reader["expectdate"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(reader["expectdate"]);
                        omfgworkorder.deliverydate = reader["deliverydate"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(reader["deliverydate"]);
                        omfgworkorder.issuedby = reader["issuedby"] == DBNull.Value ? 0 : Convert.ToInt64(reader["issuedby"]);
                        omfgworkorder.createdby = reader["createdby"] == DBNull.Value ? 0 : Convert.ToInt64(reader["createdby"]);
                        omfgworkorder.createdon = reader["createdon"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(reader["createdon"]);
                        omfgworkorder.modifiedon = reader["modifiedon"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(reader["modifiedon"]);
                        omfgworkorder.tax = reader["tax"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["tax"]);
                        omfgworkorder.taxable = reader["taxable"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["taxable"]);
                        omfgworkorder.total = reader["total"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["total"]);
                        omfgworkorder.statusid = reader["statusid"] == DBNull.Value ? 0 : Convert.ToInt32(reader["statusid"]);
                        omfgworkorder.supplierid = reader["supplierid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["supplierid"]);
                        omfgworkorder.suppliercompanyid = reader["suppliercompanyid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["suppliercompanyid"]);
                        omfgworkorder.variance = reader["variance"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["variance"]);
                        omfgworkorder.totalqty = reader["totalqty"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["totalqty"]);
                        omfgworkorder.receivedby = reader["receivedby"] == DBNull.Value ? 0 : Convert.ToInt64(reader["receivedby"]);
                        omfgworkorder.woflowid = reader["woflowid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["woflowid"]);
                        omfgworkorder.onlybom = reader["onlybom"] == DBNull.Value ? false : Convert.ToBoolean(reader["onlybom"]);
                        omfgworkorder.designid = reader["designid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["designid"]);
                        omfgworkorder.remarks = reader["remarks"] == DBNull.Value ? "" : reader["remarks"].ToString();
                        omfgworkorder.brandname = reader["brandname"] == DBNull.Value ? "" : reader["brandname"].ToString();
                        omfgworkorder.product = reader["productname"] == DBNull.Value ? "" : reader["productname"].ToString();
                        omfgworkorder.colourname = reader["colourname"] == DBNull.Value ? "" : reader["colourname"].ToString();
                        omfgworkorder.sleevename = reader["sleevename"] == DBNull.Value ? "" : reader["sleevename"].ToString();
                        omfgworkorder.fabricqty = reader["fabricqty"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["fabricqty"]);
                        lst.Add(omfgworkorder);
                    }
                }
                if (_mfgworkorder.id > 0 && lst.Count == 1)
                {
                    MfgworkorderitemsDataService subMfgworkorderitems = new MfgworkorderitemsDataService() { Context = this.Context };
                    lst[0].Mfgworkorderitems = subMfgworkorderitems.SearchAll(_mfgworkorder.id);
                    MfgworkorderitemsizesDataService subMfgworkorderitemsizes = new MfgworkorderitemsizesDataService() { Context = this.Context };
                    var Mfgworkorderitemsizes = subMfgworkorderitemsizes.SearchAll(_mfgworkorder.id);
                    foreach (var item in lst[0].Mfgworkorderitems)
                        item.sizelist = Mfgworkorderitemsizes.FindAll(e => e.mfgworkorderitemsid == item.id);
                }
                conn.Close();
            }
            catch (Exception exp)
            {
                if (conn != null) conn.Close();
                WebAPI.Common.Logger.Instance(Context.OrganizationID).Log(exp, WebAPI.Common.Logger.LogType.Error, _mfgworkorder);
            }
            return lst;
        }

        public MfgworkorderPrint Print(MfgworkorderCreteria _mfgworkorders)
        {
            var mfgworkorders = SearchDetail(_mfgworkorders);

            MfgworkorderPrint ret = new MfgworkorderPrint();
            ret.id = mfgworkorders[0].id;
            ret.variance = mfgworkorders[0].variance;
            ret.companyid = mfgworkorders[0].companyid;
            ret.locationid = mfgworkorders[0].locationid;
            ret.wodate = mfgworkorders[0].wodate;
            ret.workorderno = mfgworkorders[0].workorderno;
            ret.lotno = mfgworkorders[0].lotno;
            ret.jobcardno = mfgworkorders[0].jobcardno;
            ret.expectdate = mfgworkorders[0].expectdate;
            ret.supplierid = mfgworkorders[0].supplierid;
            ret.customerid = mfgworkorders[0].customerid;
            ret.mode = mfgworkorders[0].mode;
            ret.Mfgworkorderitems = mfgworkorders[0].Mfgworkorderitems;
            ret.createdby = mfgworkorders[0].createdby;
            ret.createdon = mfgworkorders[0].createdon;
            ret.modifiedby = mfgworkorders[0].modifiedby;
            ret.modifiedon = mfgworkorders[0].modifiedon;
            ret.issuedby = mfgworkorders[0].issuedby;
            ret.createdby = mfgworkorders[0].createdby;
            ret.product = mfgworkorders[0].product;
            ret.brand = mfgworkorders[0].brandname;
            ret.remarks = mfgworkorders[0].remarks;
            ret.design = mfgworkorders[0].design;
            ret.mfgsampleorderid = mfgworkorders[0].mfgsampleorderid;
            Data.CompanyDataService companyservice = new CompanyDataService() { Context = this.Context };
            companyservice.Context = this.Context;
            var comnanylist = companyservice.Search(new CompanyCreteria()
            {
                id = ret.companyid
            });

            ret.company = comnanylist[0];
            ret.company.parseaddress();

            Data.SupplierDataService _supplierdata = new SupplierDataService() { Context = this.Context };
            var supplier = _supplierdata.Search(new SupplierCreteria()
            {
                id = ret.supplierid
            });

            Data.ReferencelistDataService _refdata = new ReferencelistDataService() { Context = this.Context };
            var _deliverylocation = _refdata.Search(new ReferencelistCreteria()
            {
                id = ret.locationid
            });

            if (_deliverylocation != null && _deliverylocation.Count > 0 && _deliverylocation[0].additionalinfo != null)
            {
                ret.location = new CompanyLocation(_deliverylocation[0]);
            }

            if (supplier != null && supplier.Count > 0)
            {
                ret.supplier = supplier[0];
                ret.suppliercompany = supplier[0].companylist != null && supplier[0].companylist.Count > 0 ? supplier[0].companylist[0] : null;
            }

            if (ret.customerid > 0)
            {
                CustomerDataService customerDataService = new CustomerDataService() { Context = this.Context };
                var customers = customerDataService.Search(new CustomerCreteria() { id = ret.customerid });
                if(customers.Count>0)
                {
                    ret.customer = customers[0];
                }
            }

            List<long> userids = new List<long>();
            userids.Add(ret.issuedby);
            userids.Add(ret.createdby);
            Data.EmployeeDataService _empdata = new EmployeeDataService() { Context = this.Context };
            var emplist = _empdata.GetUserName(userids);
            if (emplist != null && emplist.Count > 0)
            {
                if (emplist.Exists(e => e.id == ret.createdby)) ret.whocreated = emplist.Find(e => e.id == ret.createdby).name;
                if (emplist.Exists(e => e.id == ret.issuedby)) ret.whoapproved = emplist.Find(e => e.id == ret.issuedby).name;
            }

            ProductsDataService productsDataService = new ProductsDataService() { Context = this.Context };
            var products = productsDataService.SearchList(new ProductsCreteria()
            {
                ids = ret.Mfgworkorderitems.ConvertAll(x => x.productid)
            });
            ret.Mfgworkorderitems.ForEach(x =>
            {
                x.productname = products.Exists(y => y.id == x.productid) ? products.Find(y => y.id == x.productid).name : ret.product;
            });

            if (ret.mfgsampleorderid > 0)
            {
                MfgsampleorderDataService mfgsampleorderDataService = new MfgsampleorderDataService() { Context = this.Context };
                var sampleorder = mfgsampleorderDataService.Search(new MfgsampleorderCreteria()
                {
                    id = ret.mfgsampleorderid
                });
                ret.mfgsampleorder = sampleorder[0];
                if (sampleorder[0].prddivisionid > 0)
                {
                    ret.division = _refdata.SearchList(new ReferencelistCreteria() { id = sampleorder[0].prddivisionid }).First().name;
                }
                if (sampleorder[0].productformid > 0)
                {
                    ret.productform = _refdata.SearchList(new ReferencelistCreteria() { id = sampleorder[0].productformid }).First().name;
                }
                if (sampleorder[0].config != null && 
                    sampleorder[0].config.machine > 0)
                {
                    ret.machine = _refdata.Search(new ReferencelistCreteria() { id = sampleorder[0].config.machine }).First();
                }
                if (sampleorder[0].config != null && sampleorder[0].config.quality>0)
                {
                    ret.quality = _refdata.SearchList(new ReferencelistCreteria() { id = sampleorder[0].config.quality }).First().name;
                }

                ret.mfgsamplesizes = sampleorder[0].Sizes;
                ret.mfgsampledesign = sampleorder[0].Designs.Find(x => ret.Mfgworkorderitems.Exists(y => y.designitemname == x.design_code ||
                y.designname == x.design_name));

                List<long> bomids = new List<long>();
                List<long> refids = new List<long>();
                bomids.AddRange(ret.mfgsampledesign.items.ConvertAll(x => x.design_bom).ToList());
                bomids.AddRange(ret.mfgsampledesign.items.ConvertAll(x => x.design_shade).ToList());
                refids.AddRange(ret.mfgsampledesign.items.ConvertAll(x => x.colourid).ToList());
                refids.AddRange(ret.mfgsampledesign.items.ConvertAll(x => x.design_colour).ToList());
                refids.AddRange(ret.mfgsampledesign.items.ConvertAll(x => x.design_frontcolour).ToList());
                refids.AddRange(ret.mfgsampledesign.items.ConvertAll(x => x.design_backcolour).ToList());

                refids.Add(ret.mfgsampleorder.productformid);
                refids.Add(ret.mfgsampleorder.config.quality);
                refids.Add(ret.mfgsampleorder.config.machine);
                refids.AddRange(ret.mfgsampleorder.Sizes.ConvertAll(x => x.id));

                refids.RemoveAll(x => x == 0);
                bomids.RemoveAll(x => x == 0);


                BOMMasterDataService bOMMasterDataService = new BOMMasterDataService() { Context = this.Context };
                List<BOMBase> bOMMasters = new List<BOMBase>();
                List<ReferenceItem> referenceItems = new List<ReferenceItem>();
                if(bomids.Count>0)
                {
                    bOMMasters.AddRange(bOMMasterDataService.SearchList(new BOMMasterCreteria() { ids = bomids }));
                }
                if(refids.Count>0)
                {
                    referenceItems.AddRange(_refdata.SearchList(new ReferencelistCreteria() { ids = refids }));
                }
                ret.mfgsampledesign.items.ForEach(x =>
                {
                    if(x.design_bom>0 && bOMMasters.Exists(y=>y.id == x.design_bom))
                    {
                        x.design_bomname = bOMMasters.Find(y => y.id == x.design_bom).name;
                    }
                    if (x.design_shade > 0 && bOMMasters.Exists(y => y.id == x.design_shade))
                    {
                        x.design_shadename = bOMMasters.Find(y => y.id == x.design_shade).name;
                    }
                    if (x.colourid > 0 && referenceItems.Exists(y => y.id == x.colourid))
                    {
                        x.colourname = referenceItems.Find(y => y.id == x.colourid).name;
                    }
                    if (x.design_colour > 0 && referenceItems.Exists(y => y.id == x.design_colour))
                    {
                        x.design_colourname = referenceItems.Find(y => y.id == x.design_colour).name;
                    }
                    if (x.design_frontcolour > 0 && referenceItems.Exists(y => y.id == x.design_frontcolour))
                    {
                        x.design_frontcolourname = referenceItems.Find(y => y.id == x.design_frontcolour).name;
                    }
                    if (x.design_backcolour > 0 && referenceItems.Exists(y => y.id == x.design_backcolour))
                    {
                        x.design_backcolourname = referenceItems.Find(y => y.id == x.design_backcolour).name;
                    }
                });


                var sizelist = _refdata.SearchList(new ReferencelistCreteria() { ids = ret.mfgsamplesizes.ConvertAll(x => x.id) });
                ret.mfgsamplesizes.ForEach(x =>
                {
                    if (sizelist.Exists(y => y.id == x.id))
                    {
                        x.sizename = sizelist.Find(y => y.id == x.id).name;
                    }
                });
                if (sampleorder[0].agentid > 0)
                {
                    using (var reader = ExecuteReader(@"select agent.name from agent
                                where id = " + sampleorder[0].agentid, null))
                    {
                        if (reader.Read() &&
                            reader["name"] != DBNull.Value)
                        {
                            ret.agent = reader["name"].ToString();
                        }
                    }
                }

                ret.mfgsamplesizes.RemoveAll(x =>
                {
                    return !ret.Mfgworkorderitems.Exists(y => y.sizelist.Exists(z => z.sizeid == x.id));
                });

                ret.Mfgworkorderitems.ForEach(x =>
                {
                    foreach (var sitem in x.sizelist)
                    {
                        if(ret.mfgsamplesizes.Exists(z=>z.id == sitem.sizeid))
                        {
                            ret.mfgsamplesizes.Find(z => z.id == sitem.sizeid).qty += sitem.qty;
                        }
                    }
                });

                BomrequestDataService BOMConsumptionData = new BomrequestDataService();
                List<BOMConsumptionitemsDetail> result = BOMConsumptionData.ImportBOM(new BOMRequest()
                {
                    mfgworkorderid = _mfgworkorders.id
                });
                var items = ret.mfgsampledesign.items;
                result.ForEach(x =>
                {
                    if (x.bomid > 0 && items.Exists(y => y.design_bom == x.bomid))
                    {
                        var bomitems = items.FindAll(y => y.design_bom == x.bomid);
                        if(bomitems.Count>1)
                        {
                            var perpics = x.qty/bomitems.Sum(y => y.design_picks);
                            bomitems.ForEach(y =>
                            {
                                y.design_yarnqty = Math.Round(perpics * y.design_picks,4);
                            });
                        }
                        else
                        {
                            bomitems[0].design_yarnqty = x.qty;
                        }
                    }
                    if (x.bomid > 0 && items.Exists(y => y.design_shade == x.bomid))
                    {
                        var bomitems = items.FindAll(y => y.design_shade == x.bomid);
                        if (bomitems.Count > 1)
                        {
                            var perpics = x.qty/bomitems.Sum(y => y.design_picks);
                            bomitems.ForEach(y =>
                            {
                                y.design_yarnqty = Math.Round(perpics * y.design_picks, 4);
                            });
                        }
                        else
                        {
                            bomitems[0].design_yarnqty = x.qty;
                        }
                    }
                });
                if(ret.mfgsampledesign != null &&
                     ret.mfgsampledesign.items != null)
                {
                    ret.mfgsampledesign.tpicks = ret.mfgsampledesign.items.Sum(x => x.design_picks);
                }

                if(ret.mfgsampledesign != null &&
                    ret.mfgsampledesign.tpicks>0 && 
                    ret.machine != null && ret.machine.additionalinfo != null &&
                    ret.machine.additionalinfo.ccmargin>0)
                {
                    ret.workinghours = Math.Round((ret.machine.additionalinfo.ccmargin / ret.mfgsampledesign.tpicks * 
                        (ret.mfgsampleorder.config.repeat==0 ? 1: ret.mfgsampleorder.config.repeat)) / ret.mfgsamplesizes.Sum(x => x.qty),2);

                    ret.workinghours = Math.Round(ret.mfgsamplesizes.Sum(x => x.qty) / (ret.mfgsampleorder.config.repeat == 0 ? 1 : ret.mfgsampleorder.config.repeat) * ret.mfgsampledesign.tpicks / ret.machine.additionalinfo.ccmargin, 2);

                    if (ret.workinghours > 0)
                    {
                        int totalMinutes = (int)Math.Round(ret.workinghours * 60,0);
                        int hours = totalMinutes / 60;
                        int minutes = totalMinutes % 60;
                        if(hours>0)
                            ret.workinghour = hours.ToString("N0") + " Hours " + minutes.ToString("N0")+" Minutes";
                        else
                            ret.workinghour = minutes.ToString("N0") + " Minutes";
                    }
                }
            }

            if (ret.Mfgworkorderitems.Exists(x => x.salesorderid > 0))
            {
                using (var reader = ExecuteReader(@"select agent.name from salesorder
                                inner join agent on agent.id = salesorder.agentid
                                where salesorder.id = " + ret.Mfgworkorderitems.First(x => x.salesorderid > 0).salesorderid, null))
                {
                    if (reader.Read() &&
                        reader["name"] != DBNull.Value)
                    {
                        ret.agent = reader["name"].ToString();
                    }
                }
            }
            return ret;

        }
        public Mfgworkorder Insert(Mfgworkorder oMfgworkorder)
        {

            NpgsqlTransaction transaction = null;
            try
            {
                if (oMfgworkorder == null ||
                    oMfgworkorder.Mfgworkorderitems == null ||
                    oMfgworkorder.Mfgworkorderitems.Count == 0 ||
                    oMfgworkorder.Mfgworkorderitems.Exists(x => x.sizelist == null || x.sizelist.Count == 0))
                {
                    bool showexception = true;
                    if (oMfgworkorder.woflowid > 0)
                    {
                        MfgworkfloworchestrationDataService mfgworkfloworchestrationDataService = new MfgworkfloworchestrationDataService() { Context = this.Context };
                        var workflows = mfgworkfloworchestrationDataService.Search(new MfgworkfloworchestrationCreteria()
                        {
                            id = oMfgworkorder.woflowid
                        });
                        if (workflows.Count == 1 &&
                            !string.IsNullOrEmpty(workflows[0].fgsupport) &&
                            workflows[0].fgsupport.ToLower() == "no")
                        {
                            showexception = false;
                        }
                    }
                    if(showexception)
                        throw new Exception("Empty work order can't be saved");
                }

                using (NpgsqlConnection conn = CreateConnection())
                {
                    conn.Open();
                    transaction = conn.BeginTransaction();
                    NpgsqlCommand command = new NpgsqlCommand(SQL_Insert, conn, transaction);
                    var sequence = SequenceManager.Instance(Context.OrganizationID).Get("mfgworkorder.workorderno", 0, InfyPOS.Services.ConfigManager.Instance(Context.OrganizationID).Config.system_supportcompanywiseentryno ? oMfgworkorder.companyid : 0);
                    oMfgworkorder.workorderno = sequence.GetNextNumber(Context, conn, transaction);

                    var param = new SortedList<string, object>();
                    if (InfyPOS.Services.ConfigManager.Instance(Context.OrganizationID).Config.mfg_mode != 6 &&
                        oMfgworkorder.suppliercompanyid>0)
                    {
                        oMfgworkorder.design = string.Join(",", oMfgworkorder.Mfgworkorderitems.ConvertAll(e => e.designname).Distinct());
                        oMfgworkorder.product = string.Join(",", oMfgworkorder.Mfgworkorderitems.ConvertAll(e => e.productname).Distinct());

                        if (!oMfgworkorder.onlybom &&
                            oMfgworkorder.Mfgworkorderitems.Exists(e => e.designid > 0))
                        {
                            oMfgworkorder.designid = oMfgworkorder.Mfgworkorderitems.Find(e => e.designid > 0).designid;
                            param.Add("designid", oMfgworkorder.designid);
                            using (var reader = ExecuteReader(@"select supplier.name,suppliercompany.supplierid,suppliercompany.id suppliercompanyid from mfgdesign
                                                        inner join supplier on supplier.id= mfgdesign.supplierid
                                                        inner join suppliercompany on suppliercompany.supplierid =  mfgdesign.supplierid
                                                        where mfgdesign.id = @designid", param))
                            {
                                if (reader.Read())
                                {
                                    oMfgworkorder.suppliercompanyid = reader.GetInt64(2);
                                    oMfgworkorder.supplierid = reader.GetInt64(1);
                                    oMfgworkorder.suppliername = reader.GetString(0);
                                }
                            }
                        }

                        if (!oMfgworkorder.onlybom && oMfgworkorder.Mfgworkorderitems.Exists(x => x.sizelist.GroupBy(y => y.sizeid).ToList().Exists(z => z.Count() > 1)))
                        {
                            var duplicateitem = oMfgworkorder.Mfgworkorderitems.Find(x => x.sizelist.GroupBy(y => y.sizeid).ToList().Exists(z => z.Count() > 1));
                            transaction.Rollback();
                            WebAPI.Common.Logger.Instance(Context.OrganizationID).Log(new Exception("WO - Duplicate UID failed - " + duplicateitem.designitemname + " conflict"), WebAPI.Common.Logger.LogType.Error, oMfgworkorder);
                            throw new Exception("WO - Duplicate UID failed  ,please contact support team - " + duplicateitem.designitemname + " conflict");
                        }
                        oMfgworkorder.fabricqty = oMfgworkorder.Mfgworkorderitems.Sum(e => e.fabricid > 0 ? e.inqty : 0);
                    }

                    Insert(transaction, conn, oMfgworkorder);
                    MfgworkorderitemsDataService subMfgworkorderitems = new MfgworkorderitemsDataService() { Context = this.Context };
                    foreach (var item in oMfgworkorder.Mfgworkorderitems)
                    {
                        item.mfgworkorderid = oMfgworkorder.id;
                        subMfgworkorderitems.Insert(transaction, conn, oMfgworkorder, item);
                    }
                    MfgworkorderinboxDataService mfgworkorderinboxDataService = new MfgworkorderinboxDataService() { Context = this.Context };
                    mfgworkorderinboxDataService.Insert(oMfgworkorder, conn, transaction);

                    if (!oMfgworkorder.onlybom &&
                        InfyPOS.Services.ConfigManager.Instance(Context.OrganizationID).Config.mfg_mode != 6)
                    {
                        string desgincodes = string.Join(",", oMfgworkorder.Mfgworkorderitems.ConvertAll(x => "'" + x.designname + "'"));
                        string validatemfgdesign = string.Format(@"with alluid
as
(
select mfgdesign.code,schemelist->>'sizegroup' sizegroup,schemelist->>'uid' uid,cast(schemelist->>'sizeid' as bigint) sizeid,
cast(schemelist->>'wsrate' as numeric) wsrate,cast(schemelist->>'dealerprice' as numeric) dealerprice,
cast(schemelist->>'salerate' as numeric) salerate,cast(schemelist->>'mrp' as numeric) mrp,
schemelist->>'code' dname,cast(schemelist->>'colourid' as bigint) colourid,
	case when position('/' in schemelist->>'code') = 0 then '' else
	substring(schemelist->>'code',position('/' in schemelist->>'code')+1)
	end oldcolourcode,
    case when substring(schemelist->>'code',length(CODE)+1,1)='/'  then substring(schemelist->>'code',length(CODE)+2) else substring(schemelist->>'code',length(CODE)+1) END colourcode
	from mfgdesign 
LEFT JOIN LATERAL
json_array_elements (cast(mfgdesign.sizegroup->>'items' as json)) schemelist ON true
	where isactive=true and mfgdesign.code in ({0})
)

select MFGWORKORDERITEMS.id,Mfgworkorderitemsizes.id,alluid.dname,alluid.colourid,alluid.colourcode from mfgworkorder 
INNER JOIN MFGWORKORDERITEMS ON MFGWORKORDERITEMS.MFGWORKORDERID = mfgworkorder.ID and MFGWORKORDERITEMS.isactive=true
INNER JOIN Mfgworkorderitemsizes ON Mfgworkorderitemsizes.mfgworkorderitemsid = MFGWORKORDERITEMS.ID and Mfgworkorderitemsizes.isactive=true
left outer join alluid on alluid.uid = Mfgworkorderitemsizes.uid
WHERE mfgworkorder.id=@id and (alluid.dname is null or DESIGNITEMNAME != alluid.dname or MFGWORKORDERITEMS.colourid != alluid.colourid  or
Mfgworkorderitemsizes.colourid != alluid.colourid)", desgincodes);

                        param.Add("id", oMfgworkorder.id);
                        using (var reader = ExecuteReader(validatemfgdesign, param, conn, transaction))
                        {
                            if (reader.Read())
                            {
                                int failedcount = reader.IsDBNull(0) ? 0 : reader.GetInt32(0);
                                if (failedcount > 0)
                                {

                                    WebAPI.Common.Logger.Instance(Context.OrganizationID).Log(new Exception("WO - Validation failed- " + reader.GetString(2) + " conflict"), WebAPI.Common.Logger.LogType.Error, oMfgworkorder);
                                    reader.Close();
                                    transaction.Rollback();
                                    throw new Exception("WO Validation failed ,please contact support team - " + reader.GetString(2) + " conflict");
                                }
                            }
                        }
                    }

                    transaction.Commit();
                    conn.Close();
                }
            }
            catch (Exception exp)
            {
                WebAPI.Common.Logger.Instance(Context.OrganizationID).Log(exp, WebAPI.Common.Logger.LogType.Error, oMfgworkorder);
                throw exp;
            }

            return oMfgworkorder;
        }

        public Mfgworkorder Insert(NpgsqlTransaction transaction, NpgsqlConnection conn, Mfgworkorder oMfgworkorder)
        {
            if (!string.IsNullOrEmpty(oMfgworkorder.product) && oMfgworkorder.product.Length > 100)
                oMfgworkorder.product = oMfgworkorder.product.Substring(0, 98) + "..";
            if (!string.IsNullOrEmpty(oMfgworkorder.design) && oMfgworkorder.design.Length > 100)
                oMfgworkorder.design = oMfgworkorder.design.Substring(0, 98) + "..";

            NpgsqlCommand command = new NpgsqlCommand(SQL_Insert, conn, transaction);
            command.Parameters.Add("workorderno", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oMfgworkorder.workorderno == null ? "" : oMfgworkorder.workorderno;
            command.Parameters.Add("jobcardno", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oMfgworkorder.jobcardno == null ? "" : oMfgworkorder.jobcardno;

            command.Parameters.Add("lotno", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oMfgworkorder.lotno == null ? "" : oMfgworkorder.lotno;

            command.Parameters.Add("mode", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oMfgworkorder.mode == null ? "" : oMfgworkorder.mode;

            command.Parameters.Add("product", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oMfgworkorder.product == null ? "" : oMfgworkorder.product;
            command.Parameters.Add("design", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oMfgworkorder.design == null ? "" : oMfgworkorder.design;
            command.Parameters.Add("sizelist", NpgsqlTypes.NpgsqlDbType.Json).Value = oMfgworkorder.sizelist_json == null ? "null" : oMfgworkorder.sizelist_json;

            command.Parameters.Add("companyid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oMfgworkorder.companyid;

            command.Parameters.Add("proddivid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oMfgworkorder.proddivid;
            command.Parameters.Add("mfgsampleorderid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oMfgworkorder.mfgsampleorderid;
            command.Parameters.Add("customerid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oMfgworkorder.customerid;

            command.Parameters.Add("locationid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oMfgworkorder.locationid;
            command.Parameters.Add("wodate", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = oMfgworkorder.wodate;
            command.Parameters.Add("expectdate", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = oMfgworkorder.expectdate;
            command.Parameters.Add("deliverydate", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = oMfgworkorder.deliverydate;
            command.Parameters.Add("issuedby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oMfgworkorder.issuedby;
            command.Parameters.Add("isactive", NpgsqlTypes.NpgsqlDbType.Boolean).Value = oMfgworkorder.isactive;
            command.Parameters.Add("createdby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oMfgworkorder.createdby;
            command.Parameters.Add("modifiedby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oMfgworkorder.modifiedby;
            command.Parameters.Add("version", NpgsqlTypes.NpgsqlDbType.Integer).Value = oMfgworkorder.version;
            command.Parameters.Add("createdon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = DateTimeNow;
            command.Parameters.Add("modifiedon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = DateTimeNow;
            command.Parameters.Add("tax", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oMfgworkorder.tax;
            command.Parameters.Add("taxable", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oMfgworkorder.taxable;
            command.Parameters.Add("total", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oMfgworkorder.total;
            command.Parameters.Add("statusid", NpgsqlTypes.NpgsqlDbType.Integer).Value = oMfgworkorder.statusid;
            command.Parameters.Add("supplierid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oMfgworkorder.supplierid;
            command.Parameters.Add("suppliercompanyid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oMfgworkorder.suppliercompanyid;
            command.Parameters.Add("variance", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oMfgworkorder.variance;
            command.Parameters.Add("totalqty", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oMfgworkorder.totalqty;
            command.Parameters.Add("fabricqty", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oMfgworkorder.fabricqty;
            command.Parameters.Add("receivedby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oMfgworkorder.receivedby;
            command.Parameters.Add("woflowid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oMfgworkorder.woflowid;
            command.Parameters.Add("onlybom", NpgsqlTypes.NpgsqlDbType.Boolean).Value = oMfgworkorder.onlybom;
            command.Parameters.Add("designid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oMfgworkorder.designid;
            command.Parameters.Add("remarks", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oMfgworkorder.remarks == null ? "" : oMfgworkorder.remarks;

            using (System.Data.IDataReader reader = command.ExecuteReader())
            {
                if (reader.Read())
                {
                    oMfgworkorder.id = Convert.ToInt64(reader[0].ToString());
                }
            }
            return oMfgworkorder;
        }

        public Mfgworkorder Update(Mfgworkorder oMfgworkorder)
        {

            NpgsqlTransaction transaction = null;
            try
            {
                if (oMfgworkorder == null ||
                                   oMfgworkorder.Mfgworkorderitems == null ||
                                   oMfgworkorder.Mfgworkorderitems.Count == 0 ||
                                   oMfgworkorder.Mfgworkorderitems.Exists(x => x.sizelist == null || x.sizelist.Count == 0))
                {
                    bool showexception = true;
                    if (oMfgworkorder.woflowid > 0)
                    {
                        MfgworkfloworchestrationDataService mfgworkfloworchestrationDataService = new MfgworkfloworchestrationDataService() { Context = this.Context };
                        var workflows = mfgworkfloworchestrationDataService.Search(new MfgworkfloworchestrationCreteria()
                        {
                            id = oMfgworkorder.woflowid
                        });
                        if (workflows.Count == 1 &&
                            !string.IsNullOrEmpty(workflows[0].fgsupport) &&
                            workflows[0].fgsupport.ToLower() == "no")
                        {
                            showexception = false;
                        }
                    }
                    if (showexception)
                        throw new Exception("Empty work order can't be saved");
                }

                using (NpgsqlConnection conn = CreateConnection())
                {
                    conn.Open();
                    transaction = conn.BeginTransaction();
                    if (InfyPOS.Services.ConfigManager.Instance(Context.OrganizationID).Config.mfg_mode != 6)
                    {
                        if (!oMfgworkorder.onlybom)
                        {
                            oMfgworkorder.design = string.Join(",", oMfgworkorder.Mfgworkorderitems.ConvertAll(e => e.designname).Distinct());
                            oMfgworkorder.product = string.Join(",", oMfgworkorder.Mfgworkorderitems.ConvertAll(e => e.productname).Distinct());

                            if (oMfgworkorder.Mfgworkorderitems.Exists(x => x.sizelist.GroupBy(y => y.sizeid).ToList().Exists(z => z.Count() > 1)))
                            {
                                var duplicateitem = oMfgworkorder.Mfgworkorderitems.Find(x => x.sizelist.GroupBy(y => y.sizeid).ToList().Exists(z => z.Count() > 1));
                                transaction.Rollback();
                                WebAPI.Common.Logger.Instance(Context.OrganizationID).Log(new Exception("WO - Duplicate UID failed - " + duplicateitem.designitemname + " conflict"), WebAPI.Common.Logger.LogType.Error, oMfgworkorder);
                                throw new Exception("WO - Duplicate UID failed  ,please contact support team - " + duplicateitem.designitemname + " conflict");
                            }
                        }
                    }

                    Update(transaction, conn, oMfgworkorder);
                    MfgworkorderitemsDataService subMfgworkorderitems = new MfgworkorderitemsDataService() { Context = this.Context };
                    subMfgworkorderitems.Update(transaction, conn, oMfgworkorder.Mfgworkorderitems, oMfgworkorder);

                    MfgworkorderinboxDataService mfgworkorderinboxDataService = new MfgworkorderinboxDataService() { Context = this.Context };
                    mfgworkorderinboxDataService.Update(oMfgworkorder, conn, transaction);

                    if (InfyPOS.Services.ConfigManager.Instance(Context.OrganizationID).Config.mfg_mode != 6)
                    {
                        if (!oMfgworkorder.onlybom)
                        {
                            string desgincodes = string.Join(",", oMfgworkorder.Mfgworkorderitems.ConvertAll(x => "'" + x.designname + "'"));
                            string validatemfgdesign = string.Format(@"with alluid
as
(
select mfgdesign.code,schemelist->>'sizegroup' sizegroup,schemelist->>'uid' uid,cast(schemelist->>'sizeid' as bigint) sizeid,
cast(schemelist->>'wsrate' as numeric) wsrate,cast(schemelist->>'dealerprice' as numeric) dealerprice,
cast(schemelist->>'salerate' as numeric) salerate,cast(schemelist->>'mrp' as numeric) mrp,
schemelist->>'code' dname,cast(schemelist->>'colourid' as bigint) colourid,
	case when position('/' in schemelist->>'code') = 0 then '' else
	substring(schemelist->>'code',position('/' in schemelist->>'code')+1)
	end oldcolourcode,
    case when substring(schemelist->>'code',length(CODE)+1,1)='/'  then substring(schemelist->>'code',length(CODE)+2) else substring(schemelist->>'code',length(CODE)+1) END colourcode
	from mfgdesign 
LEFT JOIN LATERAL
json_array_elements (cast(mfgdesign.sizegroup->>'items' as json)) schemelist ON true
	where isactive=true and mfgdesign.code in ({0})
)

select MFGWORKORDERITEMS.id,Mfgworkorderitemsizes.id,MFGWORKORDERITEMS.designname,alluid.dname,alluid.colourid,alluid.colourcode from mfgworkorder 
INNER JOIN MFGWORKORDERITEMS ON MFGWORKORDERITEMS.MFGWORKORDERID = mfgworkorder.ID and MFGWORKORDERITEMS.isactive=true
INNER JOIN Mfgworkorderitemsizes ON Mfgworkorderitemsizes.mfgworkorderitemsid = MFGWORKORDERITEMS.ID and Mfgworkorderitemsizes.isactive=true
left outer join alluid on alluid.uid = Mfgworkorderitemsizes.uid
WHERE mfgworkorder.id=@id and (alluid.dname is null or DESIGNITEMNAME != alluid.dname or MFGWORKORDERITEMS.colourid != alluid.colourid  or
Mfgworkorderitemsizes.colourid != alluid.colourid)", desgincodes);

                            var param = new SortedList<string, object>();
                            param.Add("designid", oMfgworkorder.designid);
                            param.Add("id", oMfgworkorder.id);
                            using (var reader = ExecuteReader(validatemfgdesign, param, conn, transaction))
                            {
                                if (reader.Read())
                                {
                                    int failedcount = reader.IsDBNull(0) ? 0 : reader.GetInt32(0);
                                    if (failedcount > 0)
                                    {

                                        WebAPI.Common.Logger.Instance(Context.OrganizationID).Log(new Exception("WO - Validation failed- " + reader.GetString(2) + " conflict"), WebAPI.Common.Logger.LogType.Error, oMfgworkorder);
                                        reader.Close();
                                        transaction.Rollback();
                                        throw new Exception("WO Validation failed ,please contact support team - " + reader.GetString(2) + " conflict");
                                    }
                                }
                            }
                        }
                    }
                    transaction.Commit();
                    conn.Close();
                }
            }
            catch (Exception exp)
            {
                WebAPI.Common.Logger.Instance(Context.OrganizationID).Log(exp, WebAPI.Common.Logger.LogType.Error, oMfgworkorder);
                throw exp;
            }
            return oMfgworkorder;
        }

        public Mfgworkorder Update(NpgsqlTransaction transaction, NpgsqlConnection conn, Mfgworkorder oMfgworkorder)
        {

            if (!string.IsNullOrEmpty(oMfgworkorder.product) && oMfgworkorder.product.Length > 100)
                oMfgworkorder.product = oMfgworkorder.product.Substring(0, 98) + "..";
            if (!string.IsNullOrEmpty(oMfgworkorder.design) && oMfgworkorder.design.Length > 100)
                oMfgworkorder.design = oMfgworkorder.design.Substring(0, 98) + "..";

            NpgsqlCommand command = new NpgsqlCommand(SQL_Update, conn, transaction);
            command.Parameters.Add("id", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oMfgworkorder.id;
            command.Parameters.Add("mfgsampleorderid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oMfgworkorder.mfgsampleorderid;
            command.Parameters.Add("proddivid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oMfgworkorder.proddivid;
            command.Parameters.Add("customerid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oMfgworkorder.customerid;
            command.Parameters.Add("mode", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oMfgworkorder.mode == null ? "" : oMfgworkorder.mode;
            command.Parameters.Add("product", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oMfgworkorder.product == null ? "" : oMfgworkorder.product;
            command.Parameters.Add("design", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oMfgworkorder.design == null ? "" : oMfgworkorder.design;
            command.Parameters.Add("sizelist", NpgsqlTypes.NpgsqlDbType.Json).Value = oMfgworkorder.sizelist_json == null ? "null" : oMfgworkorder.sizelist_json;
            command.Parameters.Add("lotno", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oMfgworkorder.lotno == null ? "" : oMfgworkorder.lotno;
            command.Parameters.Add("companyid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oMfgworkorder.companyid;
            command.Parameters.Add("workorderno", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oMfgworkorder.workorderno == null ? "" : oMfgworkorder.workorderno;
            command.Parameters.Add("jobcardno", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oMfgworkorder.jobcardno == null ? "" : oMfgworkorder.jobcardno;
            command.Parameters.Add("locationid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oMfgworkorder.locationid;
            command.Parameters.Add("wodate", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = oMfgworkorder.wodate;
            command.Parameters.Add("expectdate", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = oMfgworkorder.expectdate;
            command.Parameters.Add("deliverydate", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = oMfgworkorder.deliverydate;
            command.Parameters.Add("issuedby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oMfgworkorder.issuedby;
            command.Parameters.Add("isactive", NpgsqlTypes.NpgsqlDbType.Boolean).Value = oMfgworkorder.isactive;
            command.Parameters.Add("createdby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oMfgworkorder.createdby;
            command.Parameters.Add("modifiedby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oMfgworkorder.modifiedby;
            command.Parameters.Add("version", NpgsqlTypes.NpgsqlDbType.Integer).Value = oMfgworkorder.version;
            command.Parameters.Add("createdon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = oMfgworkorder.createdon;
            command.Parameters.Add("modifiedon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = oMfgworkorder.modifiedon;
            command.Parameters.Add("tax", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oMfgworkorder.tax;
            command.Parameters.Add("taxable", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oMfgworkorder.taxable;
            command.Parameters.Add("total", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oMfgworkorder.total;
            command.Parameters.Add("statusid", NpgsqlTypes.NpgsqlDbType.Integer).Value = oMfgworkorder.statusid;
            command.Parameters.Add("supplierid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oMfgworkorder.supplierid;
            command.Parameters.Add("suppliercompanyid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oMfgworkorder.suppliercompanyid;
            command.Parameters.Add("variance", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oMfgworkorder.variance;
            command.Parameters.Add("totalqty", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oMfgworkorder.totalqty;
            command.Parameters.Add("fabricqty", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oMfgworkorder.fabricqty;
            command.Parameters.Add("receivedby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oMfgworkorder.receivedby;
            command.Parameters.Add("woflowid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oMfgworkorder.woflowid;
            command.Parameters.Add("onlybom", NpgsqlTypes.NpgsqlDbType.Boolean).Value = oMfgworkorder.onlybom;
            command.Parameters.Add("designid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oMfgworkorder.designid;
            command.Parameters.Add("remarks", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oMfgworkorder.remarks == null ? "" : oMfgworkorder.remarks;

            using (System.Data.IDataReader reader = command.ExecuteReader())
            {
                if (reader.Read())
                {
                    oMfgworkorder.version = Convert.ToInt32(reader[0].ToString());
                }
            }

            return oMfgworkorder;
        }


        public bool Delete(Mfgworkorder oMfgworkorder)
        {

            NpgsqlTransaction transaction = null;
            bool hasSuccessCount = false;
            try
            {
                string query = "select workorderno,entryno,entrydate from mfgworkordergrn where mfgworkordergrn.workorderid=@mfgworkorderid and mfgworkordergrn.isactive=true";
                var param = new SortedList<string, object>();
                param.Add("mfgworkorderid", oMfgworkorder.id);
                using (var reader = ExecuteReader(query, param))
                {
                    if (reader.Read())
                    {
                        string workorderno = reader.GetString(0);
                        string entryno = reader.GetString(1);
                        DateTime entrydate = reader.GetDateTime(2);
                        throw new Exception("GRN Generated for " + workorderno + " on " + entrydate.ToString("dd-MM-yyyy") + "(" + entryno + ")");
                    }
                }
                using (NpgsqlConnection conn = CreateConnection())
                {
                    conn.Open();
                    transaction = conn.BeginTransaction();

                    hasSuccessCount = Delete(transaction, conn, oMfgworkorder);
                    MfgworkorderitemsDataService subMfgworkorderitems = new MfgworkorderitemsDataService() { Context = this.Context };
                    subMfgworkorderitems.DeleteAll(oMfgworkorder.id, oMfgworkorder.modifiedby, conn, transaction);
                    MfgworkorderitemsizesDataService subMfgworkorderitemsizes = new MfgworkorderitemsizesDataService() { Context = this.Context };
                    subMfgworkorderitemsizes.DeleteAll(oMfgworkorder.id, oMfgworkorder.modifiedby, conn, transaction);
                    transaction.Commit();
                    conn.Close();
                }
            }
            catch (Exception exp)
            {
                WebAPI.Common.Logger.Instance(Context.OrganizationID).Log(exp, WebAPI.Common.Logger.LogType.Error, oMfgworkorder);

                throw exp;
            }
            return hasSuccessCount;
        }

        public List<MfgworkorderSummary> Report(MfgworkorderCreteria _mfgworkorder)
        {
            List<MfgworkorderSummary> lst = new List<MfgworkorderSummary>();
            NpgsqlConnection conn = CreateConnection();
            try
            {
                conn.Open();

                NpgsqlCommand command = null;

                QueryBuilder qb = new QueryBuilder();
                var currentQuery = @"

with alljobissued as
(
select Mfgjoborderitems.mfgworkorderid,designitemname,Mfgjoborderitems.colourid,mfgjoborder.corestageid,
	mfgjoborder.stagename,Mfgworkorderitems.productid,
sum(oqty) issuedqty  from Mfgjoborderitems 
inner join Mfgworkorderitems on Mfgworkorderitems.id = mfgworkorderitemsid
inner join mfgjoborder on mfgjoborder.id = mfgjoborderid
where inwardjob = false and isbom=false 
group by Mfgjoborderitems.mfgworkorderid,designitemname,Mfgjoborderitems.colourid,mfgjoborder.corestageid,mfgjoborder.stagename,Mfgworkorderitems.productid
),alljobreceived as
(
	select Mfgjoborderitems.mfgworkorderid,designitemname,Mfgjoborderitems.colourid,mfgjoborder.corestageid,
	mfgjoborder.stagename,Mfgworkorderitems.productid,
sum(rqty) receivedqty  from Mfgjoborderitems 
inner join Mfgworkorderitems on Mfgworkorderitems.id = mfgworkorderitemsid
inner join mfgjoborder on mfgjoborder.id = mfgjoborderid
where inwardjob = true and isbom=false 
group by Mfgjoborderitems.mfgworkorderid,designitemname,Mfgjoborderitems.colourid,mfgjoborder.corestageid,mfgjoborder.stagename,Mfgworkorderitems.productid
),compained as 
(
select alljobissued.*,colourlist.name colourname,alljobreceived.receivedqty from alljobissued
left outer join alljobreceived on alljobreceived.mfgworkorderid = alljobissued.mfgworkorderid
and alljobreceived.designitemname = alljobissued.designitemname and alljobreceived.colourid = alljobissued.colourid
	and alljobreceived.corestageid = alljobissued.corestageid and alljobreceived.stagename = alljobissued.stagename
	 and alljobreceived.productid = alljobissued.productid
left outer join referencelist colourlist on colourlist.id =  alljobissued.colourid
),grn as
(
  select sum(qty) qty ,mfgworkordergrnitemsizes.mfgworkorderitemsizesid mfgworkorderitemsizesid from Mfgworkordergrn
 inner join Mfgworkordergrnitems on Mfgworkordergrnitems.mfgworkordergrnid = Mfgworkordergrn.id
 inner join mfgworkordergrnitemsizes on mfgworkordergrnitemsizes.mfgworkorderitemsgrnid = Mfgworkordergrnitems.id
 group by mfgworkorderitemsizesid
),consumption as
(
    select colourid,bomid,sum(qty) cqty,BOMConsumption.mfgworkorderid from BOMConsumptionitems
inner join BOMConsumption on BOMConsumption.id = BOMConsumptionitems.BOMConsumptionid
group by BOMConsumption.mfgworkorderid,bomid,colourid
)

select totalqty,locationid,Mfgworkorderitems.inqty as fabricqty,expectdate,wodate,workorderno,Mfgworkorder.supplierid,Mfgworkorder.id,
sizelist.name size,Mfgworkorder.lotno,Mfgworkorder.jobcardno,Mfgworkorderitems.cutratio acutratio,mfgdesign.cutratio scutratio,
Mfgworkorderitems.designitemname,Mfgworkorderitems.colourid,products.name productname,compained.corestageid,Mfgworkorderitemsizes.qty oqty,
compained.stagename,compained.issuedqty,compained.receivedqty,colourlist.name colourname ,grn.qty grnqty,consumption.cqty from Mfgworkorder
inner join Mfgworkorderitems on Mfgworkorderitems.mfgworkorderid = Mfgworkorder.id
inner join Mfgworkorderitemsizes on Mfgworkorderitemsizes.mfgworkorderitemsid = Mfgworkorderitems.id
inner join items on items.id = Mfgworkorderitemsizes.itemid
left outer join mfgdesign on mfgdesign.id = Mfgworkorderitems.designid
left outer join products on products.id = Mfgworkorderitems.productid
left outer join referencelist colourlist on colourlist.id = Mfgworkorderitems.colourid
left outer join referencelist sizelist on sizelist.id = Mfgworkorderitemsizes.sizeid
left outer join compained on compained.mfgworkorderid = Mfgworkorderitems.mfgworkorderid and 
compained.designitemname = Mfgworkorderitems.designitemname and compained.colourid = Mfgworkorderitems.colourid
left outer join grn on grn.mfgworkorderitemsizesid = Mfgworkorderitemsizes.id
left outer join consumption on consumption.mfgworkorderid = Mfgworkorder.id and consumption.bomid = Mfgworkorderitems.fabricid and consumption.colourid = Mfgworkorderitems.colourid
where Mfgworkorder.isactive=true and Mfgworkorderitems.isactive=true and Mfgworkorderitemsizes.isactive=true {0}";

                if (_mfgworkorder.onlybom)
                {
                    currentQuery = @"

with alljobissued as
(
select Mfgjoborderitems.mfgworkorderid,designitemname,Mfgjoborderitems.colourid,mfgjoborder.corestageid,
	mfgjoborder.stagename,Mfgworkorderitems.fabricid,Mfgworkorderitems.efabricid,Mfgworkorderitems.efabriccolourid,
sum(oqty) issuedqty  from Mfgjoborderitems 
inner join Mfgworkorderitems on Mfgworkorderitems.id = mfgworkorderitemsid
inner join mfgjoborder on mfgjoborder.id = mfgjoborderid
where inwardjob = false and isbom=true and mfgjoborder.isbomprocess=true 
group by Mfgjoborderitems.mfgworkorderid,designitemname,Mfgjoborderitems.colourid,mfgjoborder.corestageid,
mfgjoborder.stagename,Mfgworkorderitems.fabricid,Mfgworkorderitems.efabricid,Mfgworkorderitems.efabriccolourid
),alljobreceived as
(
	select Mfgjoborderitems.mfgworkorderid,designitemname,Mfgjoborderitems.colourid,mfgjoborder.corestageid,
	mfgjoborder.stagename,Mfgworkorderitems.fabricid,
sum(rqty) receivedqty  from Mfgjoborderitems 
inner join Mfgworkorderitems on Mfgworkorderitems.id = mfgworkorderitemsid
inner join mfgjoborder on mfgjoborder.id = mfgjoborderid
where inwardjob = true  and isbom=true and mfgjoborder.isbomprocess=true
group by Mfgjoborderitems.mfgworkorderid,designitemname,Mfgjoborderitems.colourid,mfgjoborder.corestageid,
mfgjoborder.stagename,Mfgworkorderitems.fabricid
),compained as 
(
select alljobissued.*,colourlist.name colourname,alljobreceived.receivedqty from alljobissued
left outer join alljobreceived on alljobreceived.mfgworkorderid = alljobissued.mfgworkorderid
and alljobreceived.designitemname = alljobissued.designitemname and alljobreceived.colourid = alljobissued.colourid
	and alljobreceived.corestageid = alljobissued.corestageid and alljobreceived.stagename = alljobissued.stagename
	 and alljobreceived.fabricid = alljobissued.fabricid
left outer join referencelist colourlist on colourlist.id =  alljobissued.colourid
)

select totalqty,locationid,Mfgworkorderitems.inqty as fabricqty,expectdate,wodate,workorderno,
Mfgworkorder.supplierid,Mfgworkorder.id,Mfgworkorder.jobcardno,'' size,
Mfgworkorderitems.designitemname,Mfgworkorderitems.colourid,Mfgworkorder.lotno,
bommaster.name ||' - '||bommaster.printingname productname,compained.corestageid,
Mfgworkorderitems.outqty oqty,ebommaster.name eproductname,Mfgworkorderitems.cutratio acutratio,mfgdesign.cutratio scutratio,
compained.stagename,compained.issuedqty,compained.receivedqty,colourlist.name colourname ,0 grnqty, 0 cqty from Mfgworkorder
inner join Mfgworkorderitems on Mfgworkorderitems.mfgworkorderid = Mfgworkorder.id
left outer join mfgdesign on mfgdesign.id = Mfgworkorderitems.designid
left outer join bommaster on bommaster.id = Mfgworkorderitems.fabricid
left outer join bommaster ebommaster  on ebommaster.id = Mfgworkorderitems.efabricid
left outer join referencelist colourlist on colourlist.id = Mfgworkorderitems.colourid
left outer join compained on compained.mfgworkorderid = Mfgworkorderitems.mfgworkorderid and 
compained.designitemname = Mfgworkorderitems.designitemname and compained.colourid = Mfgworkorderitems.colourid
where Mfgworkorder.isactive=true and Mfgworkorderitems.isactive=true 
and Mfgworkorder.onlybom = true {0}";

                }
                List<string> conditionlist = new List<string>();

                if (_mfgworkorder.supplierid > 0)
                    conditionlist.Add("supplierid=@supplierid");

                if (_mfgworkorder.productid > 0)
                    conditionlist.Add("Mfgworkorderitems.productid=@productid");

                if (_mfgworkorder.fromdate.Year > 2000)
                    conditionlist.Add("wodate>=@wofromdate");

                if (_mfgworkorder.todate.Year > 2000)
                    conditionlist.Add("wodate<@wotodate");

                if (_mfgworkorder.showonlypending)
                    conditionlist.Add("(compained.issuedqty is null or compained.receivedqty is null or compained.receivedqty<compained.issuedqty)");

                if (_mfgworkorder.duefromdate.Year > 2000)
                    conditionlist.Add("expectdate>=@duefromdate");

                if (_mfgworkorder.duetodate.Year > 2000)
                    conditionlist.Add("expectdate<@duetodate");

                if (_mfgworkorder.showonlypending)
                    conditionlist.Add("Mfgworkorder.statusid != 9");

                if (!string.IsNullOrEmpty(_mfgworkorder.design))
                {
                    string designid = string.Join(",", WebAPI.Data.Utility.SafeSQL(_mfgworkorder.design).Split(',').ToList().ConvertAll(x => "'" + x + "'"));
                    conditionlist.Add("Mfgworkorderitems.designname in (" + designid + ")");
                }

                if (_mfgworkorder.brandid > 0)
                    conditionlist.Add("items.brandid=@brandid");

                if (_mfgworkorder.locationid > 0)
                    conditionlist.Add("Mfgworkorder.locationid=@locationid");

                if (!_mfgworkorder.onlybom)
                {
                    conditionlist.Add("products.productgroup=@productgroupid");
                }

                if (_mfgworkorder.mode == "Internal")
                    conditionlist.Add("Mfgworkorder.mode = 'Internal'");

                if (_mfgworkorder.mode == "External")
                    conditionlist.Add("Mfgworkorder.mode = 'External'");

                if (conditionlist.Count > 0)
                {
                    qb.CurrentQuery = string.Format(currentQuery, " and " + string.Join(" and ", conditionlist));
                }
                else
                {
                    qb.CurrentQuery = string.Format(currentQuery, "");
                }

                command = qb.GetCommand(conn);
                command.Parameters.AddWithValue("productgroupid", _mfgworkorder.productgroupid);
                command.Parameters.AddWithValue("supplierid", _mfgworkorder.supplierid);
                command.Parameters.AddWithValue("productid", _mfgworkorder.productid);
                command.Parameters.AddWithValue("brandid", _mfgworkorder.brandid);
                command.Parameters.AddWithValue("locationid", _mfgworkorder.locationid);
                command.Parameters.AddWithValue("wofromdate", _mfgworkorder.fromdate.Date);
                command.Parameters.AddWithValue("wotodate", _mfgworkorder.todate.Date.AddDays(1));
                command.Parameters.AddWithValue("duefromdate", _mfgworkorder.duefromdate.Date);
                command.Parameters.AddWithValue("duetodate", _mfgworkorder.duetodate.Date.AddDays(1));

                using (System.Data.IDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {

                        MfgworkorderSummary omfgworkorder = new MfgworkorderSummary();
                        omfgworkorder.mfgworkorderid = reader["id"] == DBNull.Value ? 0 : Convert.ToInt64(reader["id"]);
                        omfgworkorder.designitemname = reader["designitemname"] == DBNull.Value ? "" : reader["designitemname"].ToString();
                        omfgworkorder.jobcardno = reader["jobcardno"] == DBNull.Value ? "" : reader["jobcardno"].ToString();
                        omfgworkorder.lotno = reader["lotno"] == DBNull.Value ? "" : reader["lotno"].ToString();

                        if (lst.Exists(x => x.mfgworkorderid == omfgworkorder.mfgworkorderid &&
                                x.designitemname == omfgworkorder.designitemname &&
                                x.jobcardno == omfgworkorder.jobcardno &&
                                x.lotno == omfgworkorder.lotno
                        ))
                        {
                            omfgworkorder = lst.Find(x => x.mfgworkorderid == omfgworkorder.mfgworkorderid && x.designitemname == omfgworkorder.designitemname);
                        }
                        else
                        {
                            omfgworkorder.supplierid = reader["supplierid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["supplierid"]);
                            omfgworkorder.colourname = reader["colourname"] == DBNull.Value ? "" : reader["colourname"].ToString();
                            omfgworkorder.product = reader["productname"] == DBNull.Value ? "" : reader["productname"].ToString();
                            omfgworkorder.workorderno = reader["workorderno"] == DBNull.Value ? "" : reader["workorderno"].ToString();
                            omfgworkorder.locationid = reader["locationid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["locationid"]);
                            omfgworkorder.wodate = reader["wodate"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(reader["wodate"]);
                            omfgworkorder.expectdate = reader["expectdate"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(reader["expectdate"]);
                            omfgworkorder.fabricqty = reader["fabricqty"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["fabricqty"]);

                            omfgworkorder.acutratio = reader["acutratio"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["acutratio"]);
                            omfgworkorder.scutratio = reader["scutratio"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["scutratio"]);

                            omfgworkorder.grnqty = 0;
                            omfgworkorder.cfabricqty = reader["cqty"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["cqty"]); ;
                            omfgworkorder.totalqty = 0; //reader["totalqty"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["totalqty"]);
                            omfgworkorder.stages = new List<MfgworkorderSummary.Stage>();
                            omfgworkorder.sizes = new List<MfgworkorderSummary.Size>();
                            lst.Add(omfgworkorder);
                        }


                        var stage = new MfgworkorderSummary.Stage();
                        stage.corestageid = reader["corestageid"] == DBNull.Value ? 0 : Convert.ToInt32(reader["corestageid"]);
                        stage.issuedqty = reader["issuedqty"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["issuedqty"]);
                        stage.receivedqty = reader["receivedqty"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["receivedqty"]);
                        stage.stage = reader["stagename"] == DBNull.Value ? "" : reader["stagename"].ToString();
                        omfgworkorder.stages.Add(stage);

                        if (!_mfgworkorder.onlybom)
                        {
                            var size = new MfgworkorderSummary.Size();
                            size.name = reader["size"] == DBNull.Value ? "" : reader["size"].ToString();
                            omfgworkorder.grnqty += reader["grnqty"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["grnqty"]);
                            if (!omfgworkorder.sizes.Exists(x => x.name == size.name))
                            {
                                size.qty = reader["oqty"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["oqty"]);
                                size.gqty = reader["grnqty"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["grnqty"]);
                                omfgworkorder.totalqty += size.qty;
                                omfgworkorder.sizes.Add(size);
                            }
                        }
                        else
                        {
                            omfgworkorder.eproduct = reader["eproductname"] == DBNull.Value ? "" : reader["eproductname"].ToString();
                        }
                    }
                }
                conn.Close();
            }
            catch (Exception exp)
            {
                if (conn != null) conn.Close();
                WebAPI.Common.Logger.Instance(Context.OrganizationID).Log(exp, WebAPI.Common.Logger.LogType.Error, _mfgworkorder);
            }
            return lst;
        }

        public bool Delete(NpgsqlTransaction transaction, NpgsqlConnection conn, Mfgworkorder oMfgworkorder)
        {

            NpgsqlCommand command = new NpgsqlCommand(SQL_Delete, conn, transaction);
            command.Parameters.Add("isactive", NpgsqlTypes.NpgsqlDbType.Boolean).Value = oMfgworkorder.isactive;
            command.Parameters.Add("modifiedby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oMfgworkorder.modifiedby;
            command.Parameters.Add("modifiedon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = oMfgworkorder.modifiedon;
            command.Parameters.Add("version", NpgsqlTypes.NpgsqlDbType.Integer).Value = oMfgworkorder.version;
            command.Parameters.Add("id", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oMfgworkorder.id;
            int successCount = command.ExecuteNonQuery();

            return successCount > 0;
        }

    }

}
