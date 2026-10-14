using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using InfyPOS.Models;
using InfyPOS.Common;
using Npgsql;
namespace InfyPOS.Data
{
    public class BOMinvoiceDataService : SAASData
    {

        const string SQL_Insert = @"INSERT INTO public.BOMinvoice(companyid, invoiceno, invoicedate, noofpieces, noofbundle, baseamount, grossamount,taxamount, totalamount, lrid, otheramount, modifiedon, createdon, createdby, modifiedby, version, suppliercompanyid,supplierid, isactive, discount,isinterstatesale,billvalue,isinclusivetax,isinclusiveall,isinclusivediscount,invoiceentryno,entrydate,isdirectpurchase,statusid,invoicetypeid,locationid,jobcardno,isjobinward,mfgjoborderid) 
                                                        VALUES(@companyid, @invoiceno, @invoicedate, @noofpieces, @noofbundle, @baseamount, @grossamount,@taxamount,@totalamount, @lrid, @otheramount, @modifiedon, @createdon, @createdby, @modifiedby, @version, @suppliercompanyid,@supplierid, @isactive, @discount,@isinterstatesale,@billvalue,@isinclusivetax,@isinclusiveall,@isinclusivediscount,@invoiceentryno,@entrydate,@isdirectpurchase,@statusid,@invoicetypeid,@locationid,@jobcardno,@isjobinward,@mfgjoborderid) RETURNING id";

        const string SQL_Search = @"select companyid, BOMinvoice.invoiceno, BOMinvoice.invoicedate, BOMinvoice.noofpieces, noofbundle, baseamount, grossamount,taxamount, discount, BOMinvoice.totalamount, lrid, BOMinvoice.id, isinterstatesale,invoiceentryno,jobcardno, supplier.supportpo,isdirectpurchase,statusid,invoicetypeid,BOMinvoice.locationid,
        otheramount, BOMinvoice.modifiedon, BOMinvoice.createdon, BOMinvoice.createdby, BOMinvoice.modifiedby, BOMinvoice.version, BOMinvoice.suppliercompanyid,BOMinvoice.supplierid, BOMinvoice.isactive,billvalue,isinclusivetax,isinclusiveall,isinclusivediscount, BOMinvoice.entrydate,isjobinward,mfgjoborderid from BOMinvoice
                                    left outer join bomlrentry on  bomlrentry.id=BOMinvoice.lrid 
                                    left outer join supplier on supplier.id = BOMinvoice.supplierid
                                    where BOMinvoice.id=@id";

        const string SQL_Update = @"UPDATE public.BOMinvoice SET companyid=@companyid, invoiceno=@invoiceno, invoicedate=@invoicedate, noofpieces=@noofpieces,isinterstatesale=@isinterstatesale,billvalue=@billvalue, noofbundle=@noofbundle, baseamount=@baseamount, 
        grossamount=@grossamount,invoiceentryno=@invoiceentryno,jobcardno=@jobcardno,isdirectpurchase=@isdirectpurchase,statusid=@statusid,invoicetypeid=@invoicetypeid,locationid=@locationid,
        isinclusivetax=@isinclusivetax,isinclusiveall=@isinclusiveall,isinclusivediscount=@isinclusivediscount,totalamount=@totalamount, lrid=@lrid, isjobinward=@isjobinward,mfgjoborderid=@mfgjoborderid,
        otheramount=@otheramount,taxamount = @taxamount, modifiedon=@modifiedon, modifiedby=@modifiedby, version=@version+1,supplierid=@supplierid, suppliercompanyid=@suppliercompanyid, discount=@discount,entrydate=@entrydate where id=@id and version=@version RETURNING version";

        const string SQL_Delete = @"update public.BOMinvoice set isactive=false,version=@version+1,modifiedby=@modifiedby,modifiedon=@modifiedon where id=@id and version=@version  RETURNING version";

        const string SQL_DeleteAll = @"update public.BOMinvoice set isactive=false,modifiedby=@modifiedby,modifiedon=@modifiedon where lrid=@lrid";

        public BOMinvoiceCover SearchDetail(BOMinvoiceCreteria _BOMinvoice, bool showitems)
        {
            BOMinvoiceCover cover = new BOMinvoiceCover();
            List<BOMinvoice> result = Search(_BOMinvoice);
            if (_BOMinvoice.lrid > 0 || (_BOMinvoice.id > 0 && result.Count == 1))
            {
                cover.Items = result;
                cover.lrid = _BOMinvoice.lrid > 0 ? _BOMinvoice.lrid : result[0].lrid;
                BOMLrentryDataService LrentryinvoiceData = new BOMLrentryDataService() { Context = this.Context };
                var lrinfo = LrentryinvoiceData.Search(new BOMLrentryCreteria()
                {
                    id = cover.lrid
                });
                if (lrinfo != null && lrinfo.Count == 1)
                {
                    cover.BOMLrentryno = lrinfo[0].BOMLrentryno;
                    cover.lrno = lrinfo[0].lrno;
                    cover.supplierid = lrinfo[0].supplierid;
                    cover.sourcesupplierid = lrinfo[0].sourcesupplierid;
                    cover.status = lrinfo[0].Status;
                    cover.modeofdelivery = lrinfo[0].modeofdelivery;
                    cover.agentid = lrinfo[0].agentid;
                    SupplierDataService suppdata = new SupplierDataService() { Context = this.Context };
                    var supplierlist = suppdata.Search(new SupplierCreteria()
                    {
                        id = cover.supplierid
                    });
                    if (supplierlist.Count > 0)
                    {
                        cover.suppliername = supplierlist[0].name;
                        cover.sourcesuppliername = supplierlist[0].name;
                        cover.suppliercityname = supplierlist[0].cityname;
                    }
                    if (cover.sourcesupplierid > 0 && cover.sourcesupplierid != cover.supplierid)
                    {
                        supplierlist = suppdata.Search(new SupplierCreteria()
                        {
                            id = cover.sourcesupplierid
                        });
                        if (supplierlist.Count > 0)
                            cover.sourcesuppliername = supplierlist[0].name;
                    }
                    if (cover.agentid > 0)
                    {
                        AgentDataService agentdata = new AgentDataService() { Context = this.Context };
                        var agentlist = agentdata.Search(new AgentCreteria()
                        {
                            id = cover.agentid
                        });
                        if (agentlist.Count > 0)
                            cover.agentname = agentlist[0].name;
                    }
                }

                if (_BOMinvoice.id > 0)
                {
                    if (result[0].suppliercompanyid > 0)
                    {
                        SuppliercompanyDataService sDataService = new SuppliercompanyDataService() { Context = this.Context };
                        List<Suppliercompany> suppliercompany = sDataService.Search(new SuppliercompanyCreteria()
                        { id = result[0].suppliercompanyid });
                        if (suppliercompany.Count == 1)
                            cover.suppliercompanyname = suppliercompany[0].name;
                    }

                    if (result[0].companyid > 0)
                    {
                        CompanyDataService sDataService = new CompanyDataService() { Context = this.Context };
                        List<Company> buyingcompany = sDataService.Search(new CompanyCreteria()
                        {
                            id = result[0].companyid
                        }
                        );
                        if (buyingcompany.Count == 1)
                        {
                            cover.companyname = buyingcompany[0].name;
                            cover.companytitle = buyingcompany[0].title;
                            cover.companyid = buyingcompany[0].id;
                        }
                    }
                    if (showitems)
                    {
                        result[0].bominvoiceitems = result[0].bominvoiceitems.OrderBy(e => e.sno).ToList();
                    }
                }
            }
            return cover;
        }

        public BOMinvoiceDetail Print(BOMinvoiceCreteria _BOMinvoice)
        {
            BOMinvoiceDetail cover = new BOMinvoiceDetail();
            _BOMinvoice.showitems = true;
            List<BOMinvoice> result = Search(_BOMinvoice);
            if (_BOMinvoice.lrid > 0 || (_BOMinvoice.id > 0 && result.Count == 1))
            {
                cover = Newtonsoft.Json.JsonConvert.DeserializeObject<BOMinvoiceDetail>(Newtonsoft.Json.JsonConvert.SerializeObject(result[0]));
                cover.lrid = _BOMinvoice.lrid > 0 ? _BOMinvoice.lrid : result[0].lrid;
                BOMLrentryDataService LrentryinvoiceData = new BOMLrentryDataService() { Context = this.Context };
                var lrinfo = LrentryinvoiceData.Search(new BOMLrentryCreteria()
                {
                    id = cover.lrid
                });
                if (lrinfo != null && lrinfo.Count == 1)
                {
                    cover.lrentrydetail = lrinfo[0];
                    cover.lrno = lrinfo[0].lrno;
                    cover.supplierid = lrinfo[0].supplierid;
                    cover.status = lrinfo[0].Status;
                    SupplierDataService suppdata = new SupplierDataService() { Context = this.Context };
                    var supplierlist = suppdata.Search(new SupplierCreteria()
                    {
                        id = cover.supplierid
                    });
                    if (supplierlist.Count > 0)
                    {
                        cover.supplierinfo = supplierlist[0];
                    }
                }

                if (_BOMinvoice.id > 0)
                {
                    if (result[0].suppliercompanyid > 0)
                    {
                        SuppliercompanyDataService sDataService = new SuppliercompanyDataService() { Context = this.Context };
                        List<Suppliercompany> suppliercompany = sDataService.Search(new SuppliercompanyCreteria()
                        { id = result[0].suppliercompanyid });
                        if (suppliercompany.Count == 1)
                            cover.suppliercompanyinfo = suppliercompany[0];
                    }

                    if (result[0].companyid > 0)
                    {
                        CompanyDataService sDataService = new CompanyDataService() { Context = this.Context };
                        List<Company> buyingcompany = sDataService.Search(new CompanyCreteria()
                        {
                            id = result[0].companyid
                        }
                        );
                        if (buyingcompany.Count == 1)
                        {
                            cover.company = buyingcompany[0];
                        }
                    }

                    result[0].bominvoiceitems = result[0].bominvoiceitems.OrderBy(e => e.sno).ToList();

                }
            }
            return cover;
        }
        public List<BOMinvoice> Search(BOMinvoiceCreteria _BOMinvoice)
        {
            List<BOMinvoice> lst = new List<BOMinvoice>();
            NpgsqlConnection conn = CreateConnection();
            try
            {
                conn.Open();

                NpgsqlCommand command = null;

                if (!string.IsNullOrEmpty(_BOMinvoice.barcode))
                {
                    command = GetCommand(@"select BOMinvoiceid  from stock 
				                 inner join BOMinvoiceitems on BOMinvoiceitems.id = invoiceitemid and BOMinvoiceitems.isactive=true
                                    where barcode=@barcode and stock.isactive=true", conn);
                    command.Parameters.Add("barcode", NpgsqlTypes.NpgsqlDbType.Varchar).Value = _BOMinvoice.barcode;
                    using (System.Data.IDataReader reader = command.ExecuteReader())
                    {
                        if (reader.Read())
                            _BOMinvoice.id = reader.GetInt64(0);
                    }
                }

                if (!string.IsNullOrEmpty(_BOMinvoice.designid))
                {
                    command = GetCommand(@"select BOMinvoiceid  from  BOMinvoiceitems 
                                    where designid = @designid and  BOMinvoiceitems.isactive=true", conn);
                    command.Parameters.Add("designid", NpgsqlTypes.NpgsqlDbType.Varchar).Value = _BOMinvoice.designid;
                    using (System.Data.IDataReader reader = command.ExecuteReader())
                    {
                        if (reader.Read())
                            _BOMinvoice.id = reader.GetInt64(0);
                    }
                }
                if (_BOMinvoice.id != 0)
                {
                    command = GetCommand(SQL_Search + (Services.ConfigManager.Instance(Context.OrganizationID).Config.intray_barcode ? ";select barcodegenerationapproval from BOMinvoice where id=@id;" : ";"), conn);
                    command.Parameters.Add("id", NpgsqlTypes.NpgsqlDbType.Bigint).Value = _BOMinvoice.id;
                }
                else
                {
                    QueryBuilder qb = new QueryBuilder();
                    qb.CurrentQuery = SQL_Search.Substring(0, SQL_Search.IndexOf("where"));
                    qb.AddParameter("BOMinvoice.isactive", "isactive", true, "=");

                    if (_BOMinvoice.isjobinward)
                    {
                        qb.AddParameter("BOMinvoice.isjobinward", "isjobinward", true, "=");
                        qb.AddParameter("BOMinvoice.mfgjoborderid", "mfgjoborderid", _BOMinvoice.mfgjoborderid, "=");
                    }

                    if (!string.IsNullOrEmpty(_BOMinvoice.ids) && _BOMinvoice.ids.Length > 0)
                    {
                        qb.AddParameter("BOMinvoice.id", "ids", _BOMinvoice.ids, " in ", "(" + _BOMinvoice.ids + ")");
                    }

                    if (_BOMinvoice.companyid > 0 && InfyPOS.Services.ConfigManager.Instance(Context.OrganizationID).Config.system_multicompany)
                    {
                        qb.AddParameter("BOMinvoice.companyid", "companyid", _BOMinvoice.companyid, "=");
                    }

                    if (_BOMinvoice.lrid > 0)
                        qb.AddParameter("lrid", _BOMinvoice.lrid, "=");
                    if (_BOMinvoice.id > 0)
                        qb.AddParameter("id", _BOMinvoice.id, "=");

                    if (!string.IsNullOrEmpty(_BOMinvoice.invoiceentryno))
                        qb.AddParameter("BOMinvoice.invoiceentryno", "invoiceentryno", _BOMinvoice.invoiceentryno, "=");

                    if (!string.IsNullOrEmpty(_BOMinvoice.invoiceno))
                        qb.AddParameter("BOMinvoice.invoiceno", "invoiceno", _BOMinvoice.invoiceno, "=");

                    if (!string.IsNullOrEmpty(_BOMinvoice.lrno))
                        qb.AddParameter("BOMLrentryno.lrno", "lrno", _BOMinvoice.lrno, "=");

                    if (!string.IsNullOrEmpty(_BOMinvoice.BOMLrentryno))
                    {
                        qb.AddParameter("bomlrentryno", _BOMinvoice.BOMLrentryno, "=");
                    }
                    else
                    {

                        if (_BOMinvoice.supplierid > 0)
                            qb.AddParameter("bomlrentry.supplierid", "supplierid", _BOMinvoice.supplierid, "=");

                        if (_BOMinvoice.fromdate.Year > 1900)
                            qb.AddParameter("BOMinvoice.invoicedate", "fromdate", _BOMinvoice.fromdate.Date, ">=");
                        if (_BOMinvoice.todate.Year > 1900)
                            qb.AddParameter("BOMinvoice.invoicedate", "todate", _BOMinvoice.todate.Date.AddDays(1), "<");

                        if (_BOMinvoice.entryfromdate.Year > 1900)
                            qb.AddParameter("BOMinvoice.createdon", "entryfromdate", _BOMinvoice.entryfromdate.Date, ">=");
                        if (_BOMinvoice.entrytodate.Year > 1900)
                            qb.AddParameter("BOMinvoice.createdon", "entrytodate", _BOMinvoice.entrytodate.Date.AddDays(1), "<");
                    }
                    if (_BOMinvoice.LimitOne)
                    {
                        qb.Limit = 1;
                        qb.SortField = "BOMinvoice.id desc";
                    }
                    else if (!IsMigration)
                    {
                        qb.Limit = 100;
                    }
                    command = qb.GetCommand(conn);
                }
                using (System.Data.IDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        BOMinvoice oBOMinvoice = new BOMinvoice();
                        oBOMinvoice.companyid = reader["companyid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["companyid"].ToString());
                        oBOMinvoice.invoiceno = reader["invoiceno"] == DBNull.Value ? "" : reader["invoiceno"].ToString();
                        oBOMinvoice.invoicedate = reader["invoicedate"] == DBNull.Value ? DateTime.MinValue : (DateTime)reader["invoicedate"];
                        oBOMinvoice.entrydate = reader["entrydate"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(reader["entrydate"]);
                        oBOMinvoice.baseamount = reader["baseamount"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["baseamount"].ToString());
                        oBOMinvoice.grossamount = reader["grossamount"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["grossamount"].ToString());
                        oBOMinvoice.totalamount = reader["totalamount"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["totalamount"].ToString());
                        oBOMinvoice.lrid = reader["lrid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["lrid"].ToString());
                        oBOMinvoice.id = reader["id"] == DBNull.Value ? 0 : Convert.ToInt64(reader["id"]);
                        oBOMinvoice.noofbundle = reader["noofbundle"] == DBNull.Value ? 0 : Convert.ToInt32(reader["noofbundle"]);
                        oBOMinvoice.noofpieces = reader["noofpieces"] == DBNull.Value ? 0 : Convert.ToInt32(reader["noofpieces"]);
                        oBOMinvoice.otheramount = reader["otheramount"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["otheramount"].ToString());
                        oBOMinvoice.taxamount = reader["taxamount"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["taxamount"].ToString());
                        oBOMinvoice.modifiedon = reader["modifiedon"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(reader["modifiedon"]);
                        oBOMinvoice.createdon = reader["createdon"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(reader["createdon"]);
                        oBOMinvoice.createdby = reader["createdby"] == DBNull.Value ? 0 : Convert.ToInt64(reader["createdby"]);
                        oBOMinvoice.modifiedby = reader["modifiedby"] == DBNull.Value ? 0 : Convert.ToInt64(reader["modifiedby"]);
                        oBOMinvoice.locationid = reader["locationid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["locationid"]);

                        oBOMinvoice.isjobinward = reader["isjobinward"] == DBNull.Value ? false : Convert.ToBoolean(reader["isjobinward"]);
                        oBOMinvoice.mfgjoborderid = reader["mfgjoborderid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["mfgjoborderid"]);

                        oBOMinvoice.suppliercompanyid = reader["suppliercompanyid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["suppliercompanyid"].ToString());
                        oBOMinvoice.version = reader["version"] == DBNull.Value ? 0 : Convert.ToInt32(reader["version"].ToString());
                        oBOMinvoice.isactive = reader["isactive"] == DBNull.Value ? false : Convert.ToBoolean(reader["isactive"]);
                        oBOMinvoice.isinterstatesale = reader["isinterstatesale"] == DBNull.Value ? false : Convert.ToBoolean(reader["isinterstatesale"]);
                        oBOMinvoice.discount = reader["discount"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["discount"].ToString());
                        oBOMinvoice.billvalue = reader["billvalue"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["billvalue"].ToString());
                        oBOMinvoice.supplierid = reader["supplierid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["supplierid"]);
                        oBOMinvoice.invoiceentryno = reader["invoiceentryno"] == DBNull.Value ? "" : reader["invoiceentryno"].ToString();
                        oBOMinvoice.jobcardno = reader["jobcardno"] == DBNull.Value ? "" : reader["jobcardno"].ToString();
                        oBOMinvoice.isinclusivetax = reader["isinclusivetax"] == DBNull.Value ? false : Convert.ToBoolean(reader["isinclusivetax"]);
                        oBOMinvoice.statusid = reader["statusid"] == DBNull.Value ? 0 : Convert.ToInt32(reader["statusid"]);
                        oBOMinvoice.invoicetypeid = reader["invoicetypeid"] == DBNull.Value ? 0 : Convert.ToInt32(reader["invoicetypeid"]);
                        
                        oBOMinvoice.isdirectpurchase = reader["isdirectpurchase"] == DBNull.Value ? false : Convert.ToBoolean(reader["isdirectpurchase"]);
                        oBOMinvoice.isinclusiveall = reader["isinclusiveall"] == DBNull.Value ? false : Convert.ToBoolean(reader["isinclusiveall"]);
                        oBOMinvoice.isinclusivediscount = reader["isinclusivediscount"] == DBNull.Value ? false : Convert.ToBoolean(reader["isinclusivediscount"]);
                        oBOMinvoice.supportpo = reader["supportpo"] == DBNull.Value ? false : Convert.ToBoolean(reader["supportpo"]);
                        lst.Add(oBOMinvoice);
                    }
                }

                if ((_BOMinvoice.id > 0 || _BOMinvoice.showitems || _BOMinvoice.isjobinward) && lst.Count == 1)
                {
                    BominvoiceitemsDataService subBOMinvoiceitems = new BominvoiceitemsDataService() { Context = this.Context };
                    subBOMinvoiceitems.Context = this.Context;
                    lst[0].bominvoiceitems = subBOMinvoiceitems.SearchAll(lst[0].id);
                }
                BOMinvoicetaxDataService subtaxservice = new BOMinvoicetaxDataService() { Context = this.Context };
                subtaxservice.Context = this.Context;
                if (!IsMigration)
                {
                    foreach (var item in lst)
                    {
                        item.bominvoicetaxitems = subtaxservice.SearchAll(item.id);
                    }
                }
                conn.Close();
            }
            catch (Exception exp)
            {
                if (conn != null) conn.Close();
                WebAPI.Common.Logger.Instance(Context.OrganizationID).Log(exp, WebAPI.Common.Logger.LogType.Error, _BOMinvoice);
            }
            return lst;
        }


        public List<BOMInvoiceReportDetail> ReportSummary(BOMinvoiceCreteria _ComponentPO)
        {
            List<BOMInvoiceReportDetail> lst = new List<BOMInvoiceReportDetail>();
            NpgsqlConnection conn = CreateConnection();
            try
            {
                conn.Open();

                NpgsqlCommand command = null;

                QueryBuilder qb = new QueryBuilder();
                qb.CurrentQuery = @"with allbomdetail
as
(
select id,categoryid,width,taxid,colourmode,printingname,name,cast(schemelist->>'id' as bigint) colourid,schemelist->>'name' shade from bommaster
                            LEFT JOIN LATERAL
                            json_array_elements (cast(bommaster.colourlist as json)) schemelist ON true
)
select allbomdetail.*,supplier.name suppliername,referencelist.name cityname,buyingqty,poinfo,
buyingprice,amount,bominwardcode,invoiceno,invoicedate,entrydate,colour.name colour,bominvoiceitems.id uniqueid from bominvoice
inner join bominvoiceitems on bominvoiceitems.bominvoiceid = bominvoice.id
left outer join allbomdetail on allbomdetail.id = bominvoiceitems.bomid 
and (bominvoiceitems.colourid = 0 or allbomdetail.colourid = bominvoiceitems.colourid)
inner join supplier on supplier.id = bominvoice.supplierid
inner join suppliercompany on suppliercompany.supplierid = bominvoice.supplierid
left outer join referencelist colour on colour.id = bominvoiceitems.colourid
left outer join referencelist on referencelist.id = suppliercompany.cityid ";


                qb.AddParameter("bominvoice.isactive", true, "=");
                qb.AddParameter("bominvoiceitems.isactive", true, "=");
                if (_ComponentPO.supplierid > 0)
                    qb.AddParameter("bominvoice.supplierid", _ComponentPO.supplierid, "=");

                if (_ComponentPO.entryfromdate != DateTime.MinValue)
                {
                    qb.AddParameter("entrydate", "entrydatefrom", _ComponentPO.entryfromdate.Date, ">=");
                    qb.AddParameter("entrydate", "entrydateto", _ComponentPO.entrytodate.Date.AddDays(1), "<");
                }
                if (_ComponentPO.fromdate != DateTime.MinValue)
                {
                    qb.AddParameter("invoicedate", "invoicedatefrom", _ComponentPO.fromdate.Date, ">=");
                    qb.AddParameter("invoicedate", "invoicedateto", _ComponentPO.todate.Date.AddDays(1), "<");
                }

                command = qb.GetCommand(conn);


                using (System.Data.IDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        BOMInvoiceReportDetail oComponentPO = new BOMInvoiceReportDetail();
                        oComponentPO.uniqueid = reader["uniqueid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["uniqueid"]);
                        oComponentPO.invoicedate = reader["invoicedate"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(reader["invoicedate"]);
                        oComponentPO.entrydate = reader["entrydate"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(reader["entrydate"]);
                        oComponentPO.invoiceno = reader["invoiceno"] == DBNull.Value ? "" : reader["invoiceno"].ToString();
                        oComponentPO.buyingqty = reader["buyingqty"] == DBNull.Value ? 0 : Convert.ToInt32(reader["buyingqty"]);
                        oComponentPO.buyingprice = reader["buyingprice"] == DBNull.Value ? 0 : Convert.ToInt32(reader["buyingprice"]);
                        oComponentPO.amount = reader["amount"] == DBNull.Value ? 0 : Convert.ToInt32(reader["amount"]);

                        oComponentPO.colour = reader["colour"] == DBNull.Value ? "" : reader["colour"].ToString();
                        oComponentPO.cityname = reader["cityname"] == DBNull.Value ? "" : reader["cityname"].ToString();
                        oComponentPO.suppliername = reader["suppliername"] == DBNull.Value ? "" : reader["suppliername"].ToString();
                        oComponentPO.bomcode = reader["name"] == DBNull.Value ? "" : reader["name"].ToString();
                        oComponentPO.name = reader["printingname"] == DBNull.Value ? "" : reader["printingname"].ToString();
                        oComponentPO.shade = reader["shade"] == DBNull.Value ? "" : reader["shade"].ToString();
                        oComponentPO.colour = reader["colour"] == DBNull.Value ? "" : reader["colour"].ToString();
                        oComponentPO.width = reader["width"] == DBNull.Value ? "" : reader["width"].ToString();

                        string poinfojson = reader["poinfo"] == DBNull.Value ? "null" : reader["poinfo"].ToString();

                        var poinfo = Newtonsoft.Json.JsonConvert.DeserializeObject<List<Bominvoiceitems.Poinfo>>(poinfojson);
                        if (poinfo != null && poinfo.Count > 0)
                        {
                            oComponentPO.OrderQty = poinfo.Sum(x => x.aqty);
                            oComponentPO.ponumber = string.Join(",", poinfo.ConvertAll(x => x.pono));
                        }
                        if (lst.Exists(x => x.uniqueid == oComponentPO.uniqueid)) continue;
                        lst.Add(oComponentPO);
                    }
                }

                conn.Close();
            }
            catch (Exception exp)
            {
                if (conn != null) conn.Close();
                WebAPI.Common.Logger.Instance(Context.OrganizationID).Log(exp, WebAPI.Common.Logger.LogType.Error, _ComponentPO);
            }
            if (_ComponentPO.mode == 0)
            {
                lst = lst.GroupBy(x => new { x.name, x.bomcode, x.colour, x.shade }).ToList().ConvertAll(x => new BOMInvoiceReportDetail()
                {
                    name = x.Key.name,
                    bomcode = x.Key.bomcode,
                    colour = x.Key.colour,
                    shade = x.Key.shade,
                    amount = x.Sum(y => y.amount),
                    buyingqty = x.Sum(y => y.buyingqty),
                    OrderQty = x.Sum(y => y.OrderQty),
                    buyingprice = x.Average(y => y.buyingprice)
                });
            }
            return lst;
        }

        public BOMinvoice UpdateDirectPurchase(BOMinvoiceDetail item)
        {
            var result = Search(new BOMinvoiceCreteria()
            {
                id = item.id
            });

            if (result.Count == 1)
            {
                return Update(item);
            }
            return item;
        }

        public BOMinvoice InsertDirectPurchase(BOMinvoiceDetail item)
        {
            return Insert(item);
        }
        public BOMinvoiceDetail UpdateItems(BOMinvoiceDetail oBOMinvoice)
        {


            NpgsqlTransaction transaction = null;
            try
            {
                using (NpgsqlConnection conn = CreateConnection())
                {
                    conn.Open();
                    transaction = conn.BeginTransaction();

                    BominvoiceitemsDataService subBOMinvoiceitems = new BominvoiceitemsDataService() { Context = this.Context };
                    subBOMinvoiceitems.Context = this.Context;
                    subBOMinvoiceitems.Update(transaction, conn, oBOMinvoice.bominvoiceitems, oBOMinvoice);

                    if (Services.ConfigManager.Instance(Context.OrganizationID).Config.intray_barcode)
                    {
                        var param = new SortedList<string, object>();
                        IntrayDataService intrayDataService = new IntrayDataService() { Context = this.Context };
                        var existingIntray = intrayDataService.Search(new IntrayCreteria()
                        {
                            referencetype = (int)Services.IntrayManager.EntryType.BarcodeGeneration,
                            referenceid = oBOMinvoice.id
                        });

                        if (existingIntray.Count == 0 || existingIntray.All(e => e.ActionType == Intray.IntrayActionTypes.Approved))
                        {
                            SuppliercompanyDataService supplierdata = new SuppliercompanyDataService() { Context = this.Context };
                            var supplier = supplierdata.Search(new SuppliercompanyCreteria()
                            {
                                supplierid = oBOMinvoice.supplierid
                            }).First();

                            Services.IntrayManager.Instance(this.Context.OrganizationID).Add(this.Context.OrganizationID, Services.IntrayManager.EntryType.BarcodeGeneration,
                                oBOMinvoice.id, string.Format("{0} / {1} on {2} / {3}", supplier.name, oBOMinvoice.invoiceno,
                                oBOMinvoice.invoicedate.ToString("dd-MM-yyyy"), oBOMinvoice.totalamount.ToString("F0")),
                                Context.UserID, 0, transaction, Context);
                        }
                    }
                    transaction.Commit();
                    conn.Close();
                }
            }
            catch (Exception exp)
            {
                WebAPI.Common.Logger.Instance(Context.OrganizationID).Log(exp, WebAPI.Common.Logger.LogType.Error, oBOMinvoice);
            }
            return oBOMinvoice;
        }

        public bool Delete(BOMinvoiceCover item)
        {
            NpgsqlTransaction transaction = null;
            int successCount = 0;
            try
            {
                using (NpgsqlConnection conn = CreateConnection())
                {
                    conn.Open();
                    transaction = conn.BeginTransaction();
                    NpgsqlCommand command = GetCommand(SQL_DeleteAll, conn, transaction);
                    command.Parameters.Add("isactive", NpgsqlTypes.NpgsqlDbType.Boolean).Value = false;
                    command.Parameters.Add("modifiedby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = Context.UserID;
                    command.Parameters.Add("modifiedon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = DateTimeNow;
                    command.Parameters.Add("lrid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = item.lrid;
                    successCount = command.ExecuteNonQuery();
                }
            }
            catch (Exception exp)
            {
                WebAPI.Common.Logger.Instance(Context.OrganizationID).Log(exp, WebAPI.Common.Logger.LogType.Error, item);


            }
            return successCount > 0;
        }
        private bool DuplicateValidation(BOMinvoiceCover oBOMinvoice)
        {
            string _Query = "select count(id) items from BOMinvoice where suppliercompanyid=@suppliercompanyid and invoiceno=@invoiceno and id!=@id";
            int count = 0;
            using (NpgsqlConnection conn = CreateConnection())
            {
                conn.Open();
                NpgsqlCommand command = GetCommand(_Query, conn);
                command.Parameters.Add("id", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oBOMinvoice.Items[0].id;
                command.Parameters.Add("suppliercompanyid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oBOMinvoice.Items[0].suppliercompanyid;
                command.Parameters.Add("invoiceno", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oBOMinvoice.Items[0].invoiceno;
                using (System.Data.IDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        count = reader["items"] == DBNull.Value ? 0 : Convert.ToInt32(reader["items"]);
                    }
                }
                conn.Close();
            }
            return count > 0;
        }

        public BOMinvoice Insert(BOMinvoiceDetail oBOMinvoice)
        {
            NpgsqlTransaction transaction = null;
            try
            {
                var sequence = SequenceManager.Instance(Context.OrganizationID).Get("invoice.invoiceentryno", 0);
                using (NpgsqlConnection conn = CreateConnection())
                {
                    conn.Open();
                    transaction = conn.BeginTransaction();
                    InsertInternal(oBOMinvoice, conn, transaction);
                    transaction.Commit();
                    conn.Close();

                }
            }
            catch (Exception exp)
            {
                WebAPI.Common.Logger.Instance(Context.OrganizationID).Log(exp, WebAPI.Common.Logger.LogType.Error, oBOMinvoice);
            }

            return oBOMinvoice;
        }

        public BOMinvoice InsertInternal(BOMinvoiceDetail oBOMinvoice, NpgsqlConnection conn, NpgsqlTransaction transaction)
        {


            var sequence = SequenceManager.Instance(Context.OrganizationID).Get("invoice.invoiceentryno", 0);

            oBOMinvoice.entrydate = DateTimeNow;
            oBOMinvoice.invoiceentryno = sequence.GetNextNumber(Context, conn, transaction);
            if (oBOMinvoice.lrid <= 0 && oBOMinvoice.isdirectpurchase)
            {

                BOMLrentryDataService lrentry = new BOMLrentryDataService() { Context = this.Context };
                lrentry.Context = this.Context;
                BOMLrentry newentry = lrentry.Insert(new BOMLrentry()
                {
                    invoiceno = oBOMinvoice.invoiceno,
                    invoicedate = oBOMinvoice.invoicedate,
                    supplierid = oBOMinvoice.supplierid,
                    createdon = DateTimeNow,
                    createdby = Context.UserID,
                    lrdate = oBOMinvoice.lrdate,
                    receiveddate = DateTimeNow,
                    modeofdelivery = 2,
                    BOMLrentrydate = oBOMinvoice.entrydate,
                    BOMLrentryno = oBOMinvoice.invoiceentryno,
                    lrcompanyid = oBOMinvoice.companyid,
                    retaillocationid = oBOMinvoice.retaillocationid,
                    transportid = oBOMinvoice.transportid,
                    noofbundels = oBOMinvoice.bundles,
                    lrno = oBOMinvoice.lrno
                });
                oBOMinvoice.lrid = newentry.id;
            }
            Insert(transaction, conn, oBOMinvoice);
            BominvoiceitemsDataService subBOMinvoiceitems = new BominvoiceitemsDataService() { Context = this.Context };
            subBOMinvoiceitems.Context = this.Context;


            var bominwardsequence = SequenceManager.Instance(Context.OrganizationID).Get("bominvoiceitem.bominwardcode", 0);

            foreach (var item in oBOMinvoice.bominvoiceitems.GroupBy(e => e.bomid))
            {
                var items = item.ToList();
                if (items.Exists(e => !string.IsNullOrEmpty(e.bominwardcode)))
                {
                    var maxid = items.Max(e =>
                    {
                        if (string.IsNullOrEmpty(e.bominwardcode)) return 0;
                        int intout = 0;
                        var bomcodes = e.bominwardcode.Split('-');
                        if (bomcodes.Length == 2 && int.TryParse(e.bominwardcode.Split('-')[1], out intout)) return intout;
                        return 0;
                    });
                    items = items.FindAll(e => string.IsNullOrEmpty(e.bominwardcode));
                    var uniqecode = bominwardsequence.GetNextNumber(Context, conn, transaction);
                    for (var i = maxid; i < items.Count; i++)
                    {
                        items[i].bominwardcode = uniqecode + "-" + (i + 1);
                    }
                }
                else
                {
                    var uniqecode = bominwardsequence.GetNextNumber(Context, conn, transaction);
                    for (var i = 0; i < items.Count; i++)
                    {
                        items[i].bominwardcode = uniqecode + "-" + (i + 1);
                    }
                }
            }

            foreach (var item in oBOMinvoice.bominvoiceitems)
            {
                item.bominvoiceid = oBOMinvoice.id;
                subBOMinvoiceitems.Insert(transaction, conn, item);
            }

            BomstocktransactionDataService bomstocktransactionDataService = new BomstocktransactionDataService() { Context = this.Context };
            bomstocktransactionDataService.Insert(Bomstocktransaction.TransactionType.Purchase,
                conn, transaction, oBOMinvoice, oBOMinvoice.bominvoiceitems);

            BOMinvoicetaxDataService subBOMinvoicetaxs = new BOMinvoicetaxDataService() { Context = this.Context };
            subBOMinvoicetaxs.Context = this.Context;
            if(oBOMinvoice.bominvoicetaxitems == null)
            {
                oBOMinvoice.bominvoicetaxitems = new List<BOMinvoicetax>();
                foreach(var item in oBOMinvoice.bominvoiceitems.GroupBy(x=>new { x.taxid,x.purchasetaxpercentage }))
                {
                    oBOMinvoice.bominvoicetaxitems.Add(new BOMinvoicetax()
                    {
                        taxid = item.Key.taxid,
                        percentage = item.Key.purchasetaxpercentage,
                        taxamount = item.Sum(x => x.purchasetax),
                        amounton = item.Sum(x => x.amount - x.purchasetax),
                        netamount = item.Sum(x => x.amount),
                        isinterstatesale = oBOMinvoice.isinterstatesale
                    });
                }
            }
            oBOMinvoice.bominvoicetaxitems.ForEach(e => e.isinterstatesale = oBOMinvoice.isinterstatesale);
            foreach (var item in oBOMinvoice.bominvoicetaxitems)
            {
                item.invoiceid = oBOMinvoice.id;
                item.createdon = oBOMinvoice.createdon;
                item.modifiedon = DateTimeNow;
                item.createdby = Context.UserID;
                subBOMinvoicetaxs.Insert(transaction, conn, item);
            }

            UpdateRetailStock(transaction, conn, oBOMinvoice, false);

            return oBOMinvoice;
        }
        public Lrinvoice UpdateRetailStock(NpgsqlTransaction transaction,
           NpgsqlConnection conn, BOMinvoiceDetail oMfgworkordergrn, bool validateexisting)
        {
            long lrentryid = 0;
            Lrinvoice lrinvoice = null;
            Lrentry lrentry = null;
            if (validateexisting)
            {
                using (var reader = ExecuteReader("select id from lrentry where isactive = true and importreferencetype=2 and importreferenceid=" + oMfgworkordergrn.id, null))
                {
                    if (reader.Read())
                        lrentryid = reader.GetInt64(0);
                }
            }

            LrentryDataService lrentryDataService = new LrentryDataService() { Context = this.Context };
            LrinvoiceDataService lrinvoiceDataService = new LrinvoiceDataService() { Context = this.Context };

            if (lrentryid > 0)
            {
                lrentry = lrentryDataService.Search(new LrentryCreteria()
                {
                    id = lrentryid
                }).First();
                lrinvoice = lrinvoiceDataService.Search(new LrinvoiceCreteria()
                {
                    lrid = lrentry.id,
                    showitems = true
                }).First();
                lrinvoiceDataService.Delete(transaction, conn, lrinvoice);
            }
            else
            {
                lrentry = new Lrentry();
            }

            var fgitems = oMfgworkordergrn.bominvoiceitems.FindAll(x => x.fgproductid > 0);
            if (fgitems.Count == 0)
                return null;

            BOMMasterDataService bOMMasterDataService = new BOMMasterDataService() { Context = this.Context };
            var allfgmaster = bOMMasterDataService.Search(new BOMMasterCreteria()
            {
                ids = fgitems.ConvertAll(x => x.bomid)
            });

            MfgdesignDataService mfgdesignDataService = new MfgdesignDataService() { Context = this.Context };
            var allskus = mfgdesignDataService.Search(new MfgdesignCreteria()
            {
                codes = fgitems.ConvertAll(x => x.fgskuid),
                includedetail = true
            });

            if (InfyPOS.Services.ConfigManager.Instance(this.Context.OrganizationID).Config.mfg_fabricbasedorder)
            {
                foreach (var fgitem in fgitems)
                {
                    var designsku = allskus.Find(x => x.code == fgitem.fgskuid);
                    if (designsku != null)
                    {
                        if (fgitem.colourid != 0 && designsku.sizegroup != null &&
                            designsku.sizegroup.items != null && designsku.sizegroup.items.Exists(x => x.colourid == fgitem.colourid))
                        {
                            fgitem.skudesignitem = designsku.sizegroup.items.Find(x => x.colourid == fgitem.colourid);
                            if (fgitem.bominwardcode != fgitem.skudesignitem.uid)
                            {
                                fgitem.bominwardcodechanged = true;
                            }
                        }
                        else if (designsku.sizegroup.items.Count == 1)
                        {
                            fgitem.skudesignitem = designsku.sizegroup.items[0];
                            if (fgitem.bominwardcode != fgitem.skudesignitem.uid)
                            {
                                fgitem.bominwardcodechanged = true;
                            }
                        }
                        else
                        {
                            throw new Exception(fgitem.fgskuid + " Design contains multiple colour, please select the colour");
                        }
                    }
                    else
                    {
                        throw new Exception(fgitem.fgskuid + " Design not available in the SKU Configuration");
                    }
                }
            }

            ProductsDataService productsDataService = new ProductsDataService() { Context = this.Context };
            var productlist = productsDataService.Search(new ProductsCreteria()
            {
                ids = oMfgworkordergrn.bominvoiceitems.ConvertAll(e => e.fgproductid).Distinct().ToList()
            });



            if (lrentryid != 0)
            {
                lrentry.locationid = oMfgworkordergrn.locationid;
                lrentry.lrno = oMfgworkordergrn.lrno;
                lrentry.entrylocationid = oMfgworkordergrn.locationid;
                lrentry.lrcompanyid = oMfgworkordergrn.companyid;
                lrentry.lrdate = oMfgworkordergrn.lrdate;
                lrentry.lrentrydate = oMfgworkordergrn.entrydate;
                lrentry.invoicedate = oMfgworkordergrn.invoicedate;
                lrentry.invoiceno = oMfgworkordergrn.invoiceno;
                lrentry.modeofdelivery = 2;
                lrentry.entrylocationid = oMfgworkordergrn.locationid;

                //if (lrentry.retaillocationid == 0)
                //    lrentry.retaillocationid = oMfgworkordergrn.receivinglocationid;

                lrentry.supplierid = oMfgworkordergrn.supplierid;
                lrentryDataService.Update(transaction, conn, lrentry);

                lrinvoice.id = 0;
                lrinvoice.lrid = lrentry.id;
                //lrinvoice.isimported = true;
                lrinvoice.baseamount = oMfgworkordergrn.taxamount;
                lrinvoice.billvalue = oMfgworkordergrn.totalamount;
                lrinvoice.companyid = oMfgworkordergrn.companyid;
                lrinvoice.entrydate = oMfgworkordergrn.entrydate;
                lrinvoice.grossamount = oMfgworkordergrn.grossamount;
                lrinvoice.invoicedate = oMfgworkordergrn.invoicedate;
                lrinvoice.invoiceno = oMfgworkordergrn.invoiceno;
                lrinvoice.isinclusiveall = true;
                lrinvoice.isinterstatesale = oMfgworkordergrn.isinterstatesale;
                lrinvoice.Lrinvoicetaxitems = new List<Lrinvoicetax>();
                lrinvoice.Lrinvoiceitems = new List<LrinvoiceItemDetail>();

                foreach (var item in oMfgworkordergrn.bominvoicetaxitems.GroupBy(e => new { e.percentage, e.taxid }))
                {
                    lrinvoice.Lrinvoicetaxitems.Add(new Lrinvoicetax()
                    {
                        taxamount = item.Sum(e => e.taxamount),
                        taxid = item.Key.taxid,
                        percentage = item.Key.percentage,
                        amounton = item.Sum(e => e.netamount - e.taxamount),
                        isinterstatesale = lrinvoice.isinterstatesale,
                        type = Lrinvoicetax.EntryType.Taxable,
                        isactive = true,
                        version = 1
                    });
                }

                foreach (var sitem in oMfgworkordergrn.bominvoiceitems)
                {
                    if (sitem.buyingqty > 0)
                    {
                        lrinvoice.Lrinvoiceitems.Add(new LrinvoiceItemDetail()
                        {
                            basebuyingprice = sitem.basebuyingprice,
                            buyingprice = sitem.buyingprice,
                            amount = sitem.amount,
                            buyingqty = sitem.buyingqty,
                            designid = sitem.fgskuid,
                            mrp = sitem.skudesignitem.mrp,
                            itemid = sitem.skudesignitem.itemid,
                            productid = sitem.fgproductid,
                            ptaxid = productlist.Find(e => e.id == sitem.fgproductid).salestaxid,
                            hsncode = productlist.Find(e => e.id == sitem.fgproductid).hsncode,
                            supplierid = oMfgworkordergrn.supplierid,
                            taxid = productlist.Find(e => e.id == sitem.fgproductid).salestaxid,
                            Details = new Items()
                            {
                                grnitemid = sitem.id,
                                productid = sitem.fgproductid,
                                colourid = sitem.colourid,
                                gln = sitem.skudesignitem.uid,
                                serialno = sitem.skudesignitem.uid,
                                support_gln = true,
                                productname = productlist.Find(e => e.id == sitem.fgproductid).name,
                                sellingname = productlist.Find(e => e.id == sitem.fgproductid).name
                            },
                            purchasetax = 0,
                            purchasetaxpercentage = 0,
                            sellingprice = sitem.skudesignitem.salerate,
                            wsrate = sitem.skudesignitem.wsrate,
                            dealerprice = sitem.skudesignitem.dealerprice,
                            sno = lrinvoice.Lrinvoiceitems.Count + 1,
                            stocklocationid = lrentry.retaillocationid,
                            version = 1,
                            isactive = true
                        });
                    }
                }
                lrinvoiceDataService.ImportInvoice(lrinvoice, transaction, conn);
            }
            else
            {
                lrentry.lrno = oMfgworkordergrn.lrno;
                lrentry.entrylocationid = oMfgworkordergrn.locationid;
                lrentry.lrcompanyid = oMfgworkordergrn.companyid;
                lrentry.lrdate = oMfgworkordergrn.lrdate;
                lrentry.lrentrydate = oMfgworkordergrn.entrydate;
                lrentry.modeofdelivery = 2;
                lrentry.supplierid = oMfgworkordergrn.supplierid;
                lrentry.locationid = oMfgworkordergrn.locationid;

                lrentryDataService.Insert(transaction, conn, lrentry);
                ExecuteQuery("update lrentry set importreferencetype=1,importreferenceid=" + oMfgworkordergrn.id + " where id=" + lrentry.id, new SortedList<string, object>(), conn, transaction);
                InboxManager.Instance(Context.OrganizationID).Update(Context, true, lrentry, transaction, conn);

                lrinvoice = new Lrinvoice();
                lrinvoice.lrid = lrentry.id;
                //lrinvoice.isimported = true;
                lrinvoice.baseamount = oMfgworkordergrn.taxamount;
                lrinvoice.billvalue = oMfgworkordergrn.totalamount;
                lrinvoice.companyid = oMfgworkordergrn.companyid;
                lrinvoice.entrydate = oMfgworkordergrn.entrydate;
                lrinvoice.grossamount = oMfgworkordergrn.grossamount;
                lrinvoice.invoicedate = oMfgworkordergrn.invoicedate;
                lrinvoice.invoiceno = oMfgworkordergrn.invoiceno;
                lrinvoice.isinclusiveall = true;
                lrinvoice.isactive = true;
                lrinvoice.version = 1;
                lrinvoice.isinterstatesale = oMfgworkordergrn.isinterstatesale;
                lrinvoice.Lrinvoicetaxitems = new List<Lrinvoicetax>();
                lrinvoice.Lrinvoiceitems = new List<LrinvoiceItemDetail>();
                foreach (var item in oMfgworkordergrn.bominvoicetaxitems.GroupBy(e => new { e.percentage, e.taxid }))
                {
                    lrinvoice.Lrinvoicetaxitems.Add(new Lrinvoicetax()
                    {
                        taxamount = item.Sum(e => e.taxamount),
                        taxid = item.Key.taxid,
                        percentage = item.Key.percentage,
                        amounton = item.Sum(e => e.netamount - e.taxamount),
                        isinterstatesale = lrinvoice.isinterstatesale,
                        type = Lrinvoicetax.EntryType.Taxable,
                        isactive = true,
                        version = 1
                    });
                }

                foreach (var sitem in oMfgworkordergrn.bominvoiceitems)
                {
                    if (sitem.buyingqty > 0)
                    {
                        var skuitem = sitem.skudesignitem;
                        if (skuitem == null)
                            skuitem = new Mfgdesign.Sizegroup.Item();
                        lrinvoice.Lrinvoiceitems.Add(new LrinvoiceItemDetail()
                        {
                            basebuyingprice = sitem.basebuyingprice,
                            buyingprice = sitem.buyingprice,
                            amount = sitem.amount,
                            buyingqty = sitem.buyingqty,
                            designid = sitem.fgskuid,
                            mrp = skuitem.mrp,
                            itemid = skuitem.itemid,
                            productid = sitem.fgproductid,
                            ptaxid = productlist.Find(e => e.id == sitem.fgproductid).salestaxid,
                            hsncode = productlist.Find(e => e.id == sitem.fgproductid).hsncode,
                            supplierid = oMfgworkordergrn.supplierid,
                            taxid = productlist.Find(e => e.id == sitem.fgproductid).salestaxid,
                            Details = new Items()
                            {
                                grnitemid = sitem.id,
                                productid = sitem.fgproductid,
                                colourid = sitem.colourid,
                                gln = skuitem.uid,
                                serialno = skuitem.uid,
                                support_gln = true,
                                productname = productlist.Find(e => e.id == sitem.fgproductid).name,
                                sellingname = productlist.Find(e => e.id == sitem.fgproductid).name
                            },
                            purchasetax = 0,
                            purchasetaxpercentage = 0,
                            sellingprice = skuitem.salerate,
                            wsrate = skuitem.wsrate,
                            dealerprice = skuitem.dealerprice,
                            sno = lrinvoice.Lrinvoiceitems.Count + 1,
                            stocklocationid = lrentry.retaillocationid,
                            version = 1,
                            isactive = true
                        });
                    }
                }

                lrinvoiceDataService.ImportInvoice(lrinvoice, transaction, conn);
            }

            if (oMfgworkordergrn.bominvoiceitems.Exists(x => x.bominwardcodechanged))
            {
                foreach (var sitem in oMfgworkordergrn.bominvoiceitems.FindAll(x => x.bominwardcodechanged))
                {
                    sitem.bominwardcode = sitem.skudesignitem.uid;
                    var param = new SortedList<string, object>();
                    param.Add("id", sitem.id);
                    param.Add("bominwardcode", sitem.bominwardcode);
                    ExecuteReader("update bominvoiceitems set bominwardcode=@bominwardcode where id=@id", param, conn, transaction);
                }
            }
            return lrinvoice;
        }

        public BOMinvoice Insert(NpgsqlTransaction transaction, NpgsqlConnection conn, BOMinvoice oBOMinvoice)
        {

            if (!string.IsNullOrEmpty(oBOMinvoice.invoiceno) && oBOMinvoice.invoiceno.Length > 50)
                oBOMinvoice.invoiceno = oBOMinvoice.invoiceno.Substring(0, 50);

            NpgsqlCommand command = GetCommand(SQL_Insert, conn, transaction);
            command.Parameters.Add("companyid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oBOMinvoice.companyid;
            command.Parameters.Add("invoiceno", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oBOMinvoice.invoiceno == null ? "" : oBOMinvoice.invoiceno;
            command.Parameters.Add("invoicedate", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = oBOMinvoice.invoicedate == null ? DateTime.MinValue : oBOMinvoice.invoicedate;
            command.Parameters.Add("entrydate", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = oBOMinvoice.entrydate == DateTime.MinValue ? DateTimeNow : oBOMinvoice.entrydate;
            command.Parameters.Add("noofpieces", NpgsqlTypes.NpgsqlDbType.Integer).Value = oBOMinvoice.noofpieces;
            command.Parameters.Add("noofbundle", NpgsqlTypes.NpgsqlDbType.Integer).Value = oBOMinvoice.noofbundle;
            command.Parameters.Add("baseamount", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oBOMinvoice.baseamount;
            command.Parameters.Add("grossamount", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oBOMinvoice.grossamount;
            command.Parameters.Add("taxamount", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oBOMinvoice.taxamount;
            command.Parameters.Add("totalamount", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oBOMinvoice.totalamount;
            command.Parameters.Add("lrid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oBOMinvoice.lrid;
            command.Parameters.Add("otheramount", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oBOMinvoice.otheramount;
            command.Parameters.Add("modifiedon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = DateTimeNow;
            command.Parameters.Add("createdon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = DateTimeNow;
            command.Parameters.Add("createdby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = Context.UserID;
            command.Parameters.Add("modifiedby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = Context.UserID;
            command.Parameters.Add("version", NpgsqlTypes.NpgsqlDbType.Integer).Value = oBOMinvoice.version;
            command.Parameters.Add("suppliercompanyid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oBOMinvoice.suppliercompanyid;
            command.Parameters.Add("supplierid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oBOMinvoice.supplierid;
            command.Parameters.Add("isactive", NpgsqlTypes.NpgsqlDbType.Boolean).Value = true;
            command.Parameters.Add("isinterstatesale", NpgsqlTypes.NpgsqlDbType.Boolean).Value = oBOMinvoice.isinterstatesale;
            command.Parameters.Add("discount", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oBOMinvoice.discount;
            command.Parameters.Add("billvalue", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oBOMinvoice.billvalue;
            command.Parameters.Add("invoiceentryno", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oBOMinvoice.invoiceentryno == null ? "" : oBOMinvoice.invoiceentryno;
            command.Parameters.Add("jobcardno", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oBOMinvoice.jobcardno == null ? "" : oBOMinvoice.jobcardno;
            command.Parameters.Add("entrydate", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = oBOMinvoice.entrydate != DateTime.MinValue ? oBOMinvoice.entrydate : DateTimeNow;
            command.Parameters.Add("isinclusivetax", NpgsqlTypes.NpgsqlDbType.Boolean).Value = oBOMinvoice.isinclusivetax;

            command.Parameters.Add("mfgjoborderid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oBOMinvoice.mfgjoborderid;
            command.Parameters.Add("isjobinward", NpgsqlTypes.NpgsqlDbType.Boolean).Value = oBOMinvoice.isjobinward;

            command.Parameters.Add("isdirectpurchase", NpgsqlTypes.NpgsqlDbType.Boolean).Value = oBOMinvoice.isdirectpurchase;
            command.Parameters.Add("statusid", NpgsqlTypes.NpgsqlDbType.Integer).Value = oBOMinvoice.statusid;
            command.Parameters.Add("invoicetypeid", NpgsqlTypes.NpgsqlDbType.Integer).Value = oBOMinvoice.invoicetypeid;
            command.Parameters.Add("locationid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oBOMinvoice.locationid;

            command.Parameters.Add("isinclusiveall", NpgsqlTypes.NpgsqlDbType.Boolean).Value = oBOMinvoice.isinclusiveall;
            command.Parameters.Add("isinclusivediscount", NpgsqlTypes.NpgsqlDbType.Boolean).Value = oBOMinvoice.isinclusivediscount;
            using (System.Data.IDataReader reader = command.ExecuteReader())
            {
                if (reader.Read())
                {
                    oBOMinvoice.id = Convert.ToInt64(reader[0]);
                }
            }

            return oBOMinvoice;
        }

        public BOMinvoiceCover Update(BOMinvoiceCover oBOMinvoice)
        {

            if (oBOMinvoice.lrid <= 0)
                throw new Exception("Can't update invoice,because LRID missing in the parameter.");

            //if (DuplicateValidation(oBOMinvoice)) throw new Exception("Duplicate Invoice entry");

            BOMinvoiceCreteria creteria = new BOMinvoiceCreteria();
            creteria.lrid = oBOMinvoice.lrid;
            List<BOMinvoice> deleteItems = new List<BOMinvoice>();
            var existingItems = Search(creteria);
            if (existingItems != null && existingItems.Count > 0)
            {
                deleteItems = existingItems.FindAll(e => !oBOMinvoice.Items.Exists(f => f.id == e.id));
            }

            if (deleteItems.Count > 1)
            {
                throw new Exception("Can't delte more than one invoice at a time");
            }
            NpgsqlTransaction transaction = null;
            try
            {
                var sequence = SequenceManager.Instance(Context.OrganizationID).Get("invoice.invoiceentryno", 0);
                BOMinvoicetaxDataService subLrtaxitems = new BOMinvoicetaxDataService() { Context = this.Context };
                subLrtaxitems.Context = this.Context;
                using (NpgsqlConnection conn = CreateConnection())
                {
                    conn.Open();
                    transaction = conn.BeginTransaction();


                    foreach (var item in deleteItems)
                        Delete(item);
                    foreach (var item in oBOMinvoice.Items.Where(e => e.id > 0))
                    {
                        item.lrid = oBOMinvoice.lrid;
                        item.entrydate = oBOMinvoice.entrydate;
                        Update(transaction, conn, item);

                        subLrtaxitems.Update(transaction, conn, item.bominvoicetaxitems, item.id);
                    }
                    foreach (var item in oBOMinvoice.Items.Where(e => e.id == 0))
                    {
                        item.lrid = oBOMinvoice.lrid;
                        item.invoiceentryno = sequence.GetNextNumber(Context, conn, transaction);
                        item.entrydate = oBOMinvoice.entrydate;
                        Insert(transaction, conn, item);
                        subLrtaxitems.Update(transaction, conn, item.bominvoicetaxitems, item.id);
                    }
                    transaction.Commit();
                    conn.Close();
                }
            }
            catch (Exception exp)
            {
                WebAPI.Common.Logger.Instance(Context.OrganizationID).Log(exp, WebAPI.Common.Logger.LogType.Error, oBOMinvoice);
            }
            return oBOMinvoice;
        }


        public BOMinvoice Update(BOMinvoiceDetail oBOMinvoice)
        {

            NpgsqlTransaction transaction = null;
            try
            {
                using (NpgsqlConnection conn = CreateConnection())
                {
                    conn.Open();
                    transaction = conn.BeginTransaction();

                    UpdateInternal(oBOMinvoice, conn, transaction);

                    transaction.Commit();
                    conn.Close();

                }
            }
            catch (Exception exp)
            {
                WebAPI.Common.Logger.Instance(Context.OrganizationID).Log(exp, WebAPI.Common.Logger.LogType.Error, oBOMinvoice);
                throw exp;
            }

            return oBOMinvoice;
        }

        public BOMinvoice UpdateInternal(BOMinvoiceDetail oBOMinvoice, NpgsqlConnection conn, NpgsqlTransaction transaction)
        {
            if (oBOMinvoice.isdirectpurchase)
            {
                BOMLrentryDataService lrentryDataService = new BOMLrentryDataService() { Context = this.Context };
                var lrentry = lrentryDataService.Search(new BOMLrentryCreteria()
                {
                    id = oBOMinvoice.lrid
                }).First();
                lrentry.invoiceno = oBOMinvoice.invoiceno;
                lrentry.invoicedate = oBOMinvoice.invoicedate;
                lrentry.supplierid = oBOMinvoice.supplierid;
                lrentry.modifiedon = DateTimeNow;
                lrentry.modifiedby = Context.UserID;
                lrentry.lrdate = oBOMinvoice.lrdate;
                lrentry.lrcompanyid = oBOMinvoice.companyid;
                lrentry.transportid = oBOMinvoice.transportid;
                lrentry.noofbundels = oBOMinvoice.bundles;
                lrentry.lrno = oBOMinvoice.lrno;
                lrentryDataService.Update(transaction, conn, lrentry);
            }

            Update(transaction, conn, oBOMinvoice);
            BominvoiceitemsDataService subBOMinvoiceitems = new BominvoiceitemsDataService() { Context = this.Context };
            subBOMinvoiceitems.Context = this.Context;

            var bominwardsequence = SequenceManager.Instance(Context.OrganizationID).Get("bominvoiceitem.bominwardcode", 0);
            foreach (var item in oBOMinvoice.bominvoiceitems.GroupBy(e => e.bomid))
            {
                var items = item.ToList();
                if (items.Exists(e => !string.IsNullOrEmpty(e.bominwardcode)))
                {
                    var maxid = items.Max(e =>
                    {
                        if (string.IsNullOrEmpty(e.bominwardcode)) return 0;
                        int intout = 0;
                        var bomcodes = e.bominwardcode.Split('-');
                        if (bomcodes.Length == 2 && int.TryParse(e.bominwardcode.Split('-')[1], out intout)) return intout;
                        return 0;
                    });
                    items = items.FindAll(e => string.IsNullOrEmpty(e.bominwardcode));
                    var uniqecode = bominwardsequence.GetNextNumber(Context, conn, transaction);
                    for (var i = maxid; i < items.Count; i++)
                    {
                        items[i].bominwardcode = uniqecode + "-" + (i + 1);
                    }
                }
                else
                {
                    var uniqecode = bominwardsequence.GetNextNumber(Context, conn, transaction);
                    for (var i = 0; i < items.Count; i++)
                    {
                        items[i].bominwardcode = uniqecode + "-" + (i + 1);
                    }
                }
            }

            subBOMinvoiceitems.Update(transaction, conn, oBOMinvoice.bominvoiceitems, oBOMinvoice);
            if (oBOMinvoice.bominvoicetaxitems == null)
            {
                oBOMinvoice.bominvoicetaxitems = new List<BOMinvoicetax>();
                foreach (var item in oBOMinvoice.bominvoiceitems.GroupBy(x => new { x.taxid, x.purchasetaxpercentage }))
                {
                    oBOMinvoice.bominvoicetaxitems.Add(new BOMinvoicetax()
                    {
                        taxid = item.Key.taxid,
                        percentage = item.Key.purchasetaxpercentage,
                        taxamount = item.Sum(x => x.purchasetax),
                        amounton = item.Sum(x => x.amount - x.purchasetax),
                        netamount = item.Sum(x => x.amount),
                        isinterstatesale = oBOMinvoice.isinterstatesale
                    });
                }
            }
            oBOMinvoice.bominvoicetaxitems.ForEach(e => e.isinterstatesale = oBOMinvoice.isinterstatesale);

            BOMinvoicetaxDataService subLrtaxitems = new BOMinvoicetaxDataService() { Context = this.Context };
            subLrtaxitems.Context = this.Context;
            subLrtaxitems.Update(transaction, conn, oBOMinvoice.bominvoicetaxitems, oBOMinvoice.id);

            UpdateRetailStock(transaction, conn, oBOMinvoice, false);

            return oBOMinvoice;
        }


        public BOMinvoice Update(NpgsqlTransaction transaction, NpgsqlConnection conn, BOMinvoice oBOMinvoice)
        {

            NpgsqlCommand command = GetCommand(SQL_Update, conn, transaction);
            command.Parameters.Add("companyid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oBOMinvoice.companyid;
            command.Parameters.Add("invoiceno", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oBOMinvoice.invoiceno == null ? "" : oBOMinvoice.invoiceno;
            command.Parameters.Add("invoicedate", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = oBOMinvoice.invoicedate;
            command.Parameters.Add("entrydate", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = oBOMinvoice.entrydate;
            command.Parameters.Add("noofpieces", NpgsqlTypes.NpgsqlDbType.Integer).Value = oBOMinvoice.noofpieces;
            command.Parameters.Add("noofbundle", NpgsqlTypes.NpgsqlDbType.Integer).Value = oBOMinvoice.noofbundle;
            command.Parameters.Add("baseamount", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oBOMinvoice.baseamount;
            command.Parameters.Add("grossamount", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oBOMinvoice.grossamount;
            command.Parameters.Add("taxamount", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oBOMinvoice.taxamount;
            command.Parameters.Add("totalamount", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oBOMinvoice.totalamount;
            command.Parameters.Add("lrid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oBOMinvoice.lrid;
            command.Parameters.Add("id", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oBOMinvoice.id;
            command.Parameters.Add("otheramount", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oBOMinvoice.otheramount;
            command.Parameters.Add("modifiedon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = DateTimeNow;
            command.Parameters.Add("createdon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = DateTimeNow;
            command.Parameters.Add("createdby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = Context.UserID;
            command.Parameters.Add("modifiedby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = Context.UserID;
            command.Parameters.Add("version", NpgsqlTypes.NpgsqlDbType.Integer).Value = oBOMinvoice.version;
            command.Parameters.Add("suppliercompanyid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oBOMinvoice.suppliercompanyid;
            command.Parameters.Add("supplierid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oBOMinvoice.supplierid;
            command.Parameters.Add("isactive", NpgsqlTypes.NpgsqlDbType.Boolean).Value = true;
            command.Parameters.Add("billvalue", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oBOMinvoice.billvalue;
            command.Parameters.Add("discount", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oBOMinvoice.discount;
            command.Parameters.Add("isdirectpurchase", NpgsqlTypes.NpgsqlDbType.Boolean).Value = oBOMinvoice.isdirectpurchase;
            command.Parameters.Add("mfgjoborderid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oBOMinvoice.mfgjoborderid;
            command.Parameters.Add("isjobinward", NpgsqlTypes.NpgsqlDbType.Boolean).Value = oBOMinvoice.isjobinward;
            command.Parameters.Add("statusid", NpgsqlTypes.NpgsqlDbType.Integer).Value = oBOMinvoice.statusid;
            command.Parameters.Add("invoicetypeid", NpgsqlTypes.NpgsqlDbType.Integer).Value = oBOMinvoice.invoicetypeid;
            command.Parameters.Add("locationid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oBOMinvoice.locationid;
            command.Parameters.Add("isinterstatesale", NpgsqlTypes.NpgsqlDbType.Boolean).Value = oBOMinvoice.isinterstatesale;
            command.Parameters.Add("isinclusivetax", NpgsqlTypes.NpgsqlDbType.Boolean).Value = oBOMinvoice.isinclusivetax;
            command.Parameters.Add("isinclusiveall", NpgsqlTypes.NpgsqlDbType.Boolean).Value = oBOMinvoice.isinclusiveall;
            command.Parameters.Add("isinclusivediscount", NpgsqlTypes.NpgsqlDbType.Boolean).Value = oBOMinvoice.isinclusivediscount;
            command.Parameters.Add("invoiceentryno", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oBOMinvoice.invoiceentryno == null ? "" : oBOMinvoice.invoiceentryno;
            command.Parameters.Add("jobcardno", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oBOMinvoice.jobcardno == null ? "" : oBOMinvoice.jobcardno;

            using (System.Data.IDataReader reader = command.ExecuteReader())
            {
                if (reader.Read())
                {
                    oBOMinvoice.version = Convert.ToInt32(reader[0]);
                }
            }
            return oBOMinvoice;
        }

        public bool IsMigration { get; set; }
        public bool Delete(BOMinvoice oBOMinvoice)
        {
            bool successCount = false;

            if (oBOMinvoice.id > 0)
            {
                NpgsqlTransaction transaction = null;
                try
                {
                    using (NpgsqlConnection conn = CreateConnection())
                    {
                        conn.Open();
                        transaction = conn.BeginTransaction();
                        Delete(transaction, conn, oBOMinvoice);
                        BominvoiceitemsDataService subBOMinvoiceitems = new BominvoiceitemsDataService() { Context = this.Context };
                        subBOMinvoiceitems.Context = this.Context;
                        successCount = subBOMinvoiceitems.DeleteAll(oBOMinvoice.id,
                            oBOMinvoice.modifiedby, conn, transaction);

                        BomstocktransactionDataService bomstocktransactionDataService = new BomstocktransactionDataService() { Context = this.Context };
                        bomstocktransactionDataService.Delete(Bomstocktransaction.TransactionType.Purchase,
                        transaction, conn, oBOMinvoice.id);

                        BOMinvoicetaxDataService subBOMinvoicetaxitems = new BOMinvoicetaxDataService() { Context = this.Context };
                        subBOMinvoicetaxitems.Context = this.Context;
                        successCount = subBOMinvoicetaxitems.DeleteAll(oBOMinvoice.id, oBOMinvoice.modifiedby, conn, transaction);

                        transaction.Commit();
                        conn.Close();
                    }
                }
                catch (Exception exp)
                {
                    WebAPI.Common.Logger.Instance(Context.OrganizationID).Log(exp, WebAPI.Common.Logger.LogType.Error, oBOMinvoice);
                }
            }
            return successCount;

        }



        public bool Delete(NpgsqlTransaction transaction, NpgsqlConnection conn, BOMinvoice oBOMinvoice)
        {

            NpgsqlCommand command = GetCommand(SQL_Delete, conn, transaction);
            command.Parameters.Add("isactive", NpgsqlTypes.NpgsqlDbType.Boolean).Value = false;
            command.Parameters.Add("modifiedby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = Context.UserID;
            command.Parameters.Add("modifiedon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = DateTimeNow;
            command.Parameters.Add("version", NpgsqlTypes.NpgsqlDbType.Integer).Value = oBOMinvoice.version;
            command.Parameters.Add("id", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oBOMinvoice.id;
            int successCount = command.ExecuteNonQuery();

            return successCount > 0;
        }

    }

}
