using InfyPOS.Data;
using InfyPOS.Models;
using InfyPOS.Processors.Migration;
using Npgsql;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace InfyPOS.Processors
{
    public class CommandManager
    {
        long organizationid;
        public CommandManager(long organizationid)
        {
            this.organizationid = organizationid;
        }
        public class Request : Common.BaseEntity
        {
            public long fileid { get; set; }
            public string command { get; set; }
            public string username { get; set; }
            public string password { get; set; }
            public bool isview { get; set; }
            public bool ispreview { get; set; }
            public Parameter parameter { get; set; }
            public bool isrevert { get; set; }
            public byte[] data { get; set; }
            public class Parameter
            {
                public DateTime fromdate { get; set; }
                public DateTime todate { get; set; }
                public string schemename { get; set; }
                public string companyname { get; set; }
                public string locationname { get; set; }
                public decimal amount { get; set; }
                public bool amountinpercentage { get; set; }
                public bool adjustprice { get; set; }
                public decimal taxpercentage { get; set; }
                public bool keeptaxslab { get; set; }
                public bool excludecustomers { get; set; }
                public bool excludegstcustomers { get; set; }
                public bool excludecreditcustomer { get; set; }
                public decimal minimumbillvalue { get; set; }
                public decimal minimumprofit { get; set; }
                public decimal excludeloyeltyclaims { get; set; }
                public bool autoround { get; set; }
                public string billno { get; set; }
                public int billcount { get; set; }
                public bool validatecost { get; set; }
                [Newtonsoft.Json.JsonIgnore]
                public long schemeid { get; set; }
                public long companyid { get; set; }
                public long locationid { get; set; }
            }
        }

        public class Response : Common.BaseEntity
        {
            public override string ToString()
            {
                if (responsebuilder == null) return "";
                return responsebuilder.ToString();
            }
            public Request request { get; set; }
            public bool success { get; set; }
            public string exception { get; set; }
            public List<string> messages { get; set; }
            [Newtonsoft.Json.JsonIgnore]
            public System.Text.StringBuilder responsebuilder { get; set; }
            public string ToHTML(string type)
            {
                return "<HTML><HEAD>" + defaultstyle + "</HEAD><BODY>" +
                    (type != "UpdateScheme" ? "" : "<H4>Don't run this on live system, take backup and run on backup data (it's purly analytical purpose)</H4>") + responsebuilder.ToString() + " </BODY></HTML>";
            }

            const string defaultstyle = @"
<style>
TABLE {
                                                font-family: 'Trebuchet MS', Arial, Helvetica, sans-serif;
                                                border-collapse: collapse;
                                                width: 100%;
                                            }

                                            TABLE td, TABLE th {
                                                border: 1px solid #ddd;
                                                padding: 8px;
                                            }

                                            TABLE tr:nth-child(even){background-color: #f2f2f2;}

                                            TABLE tr:hover {background-color: #ddd;}

                                            TABLE th {
                                                padding-top: 12px;
                                                padding-bottom: 12px;
                                                text-align: left;
                                                background-color: #4CAF50;
                                                color: white;
                                            }
</style>";
        }

        public class UpdateSchema_BillIDS
        {
            public long billitemid { get; set; }
            public long billid { get; set; }
        }

        public ProgressStatus SchemeDiscountRevert(long organizationid, Request request)
        {
            ProgressStatus progressStatus = InfyPOS.Processors.ProcessManager.Instance(organizationid).GetEmptyStatus();
            System.Threading.Thread thread = new System.Threading.Thread(new System.Threading.ParameterizedThreadStart(SchemeDiscountRevert));
            thread.Start(new Tuple<long, Request, ProgressStatus>(organizationid, request, progressStatus));
            return progressStatus;
        }

        public void SchemeDiscountRevert(object obj)
        {
            var tobj = obj as Tuple<long, Request, ProgressStatus>;
            long organizationid = tobj.Item1;
            Request request = tobj.Item2;
            var progressStatus = tobj.Item3;
            Response response = new Response();
            response.responsebuilder = responsebuilder;
            progressStatus.hasstarted = true;
            PromotionsDataService promotionsDataService = new PromotionsDataService() { Context = new Common.UserClaim() { OrganizationID = organizationid } }; ;
            var promotions = promotionsDataService.Search(new PromotionsCreteria()
            {
                name = request.parameter.schemename
            });
            if (promotions.Count == 0)
            {
                progressStatus.hascompleted = true;
                progressStatus.result = response;
                progressStatus.Exception = new Exception("Given scheme not available");
                responsebuilder.Append("Given scheme not available");
                progressStatus.result = response;
                return;
            }
            request.parameter.schemeid = promotions.First().id;

            var query = @"with conflictbills 
as
(
select billid,sum(billitems.schemediscount) schemediscount from billitems
inner join bill on bill.id = billitems.billid and bill.isactive = true 
where billitems.isactive = true and schemeid = @schemeid and bill.companyid=@companyid and bill.locationid=@locationid
and billdate>=@fromdate and billdate<@todate
group by billid
)
select bill.id,conflictbills.schemediscount from bill
inner join conflictbills on conflictbills.billid = bill.id";

            var param = new SortedList<string, object>();

            if (!string.IsNullOrEmpty(request.parameter.companyname))
            {
                CompanyDataService companyDataService = new CompanyDataService() { Context = new Common.UserClaim() { OrganizationID = organizationid } }; ;
                var companies = companyDataService.Search(new CompanyCreteria()
                {
                    name = request.parameter.companyname
                });
                if (companies.Count == 1)
                {
                    request.parameter.companyid = companies[0].id;
                }
            }


            if (!string.IsNullOrEmpty(request.parameter.locationname))
            {
                ReferencelistDataService companyDataService = new ReferencelistDataService() { Context = new Common.UserClaim() { OrganizationID = organizationid } };
                var companies = companyDataService.Search(new ReferencelistCreteria()
                {
                    type = "LOCATION",
                    name = request.parameter.locationname
                });
                if (companies.Count > 0 && companies.Exists(e => e.name == request.parameter.locationname))
                {
                    request.parameter.locationid = companies.Find(e => e.name == request.parameter.locationname).id;
                }
            }

            if (request.parameter.locationid == 0)
            {
                query = query.Replace("and bill.locationid=@locationid", "");
            }

            if (request.parameter.companyid == 0)
            {
                query = query.Replace("and bill.companyid=@companyid", "");

            }

            param.Add("locationid", request.parameter.locationid);
            param.Add("companyid", request.parameter.companyid);
            param.Add("fromdate", request.parameter.fromdate);
            param.Add("schemeid", request.parameter.schemeid);
            param.Add("todate", request.parameter.todate.AddDays(1));
            Common.BaseData baseData = new Common.BaseData() { Context = new Common.UserClaim() { OrganizationID = organizationid } };

            SortedDictionary<long, decimal> billids = new SortedDictionary<long, decimal>();
            using (var reader = baseData.ExecuteReader(query, param, 100000))
            {
                while (reader.Read())
                {
                    billids.Add(reader.GetInt64(0), reader.GetDecimal(1));
                }
            }

            if (billids.Count == 0)
            {
                responsebuilder.Append("No Bill found for rollback");
                progressStatus.hascompleted = true;
                progressStatus.Exception = new Exception("No Bill found for rollback");
                progressStatus.result = response;
                return;
            }

            query = @"select billsettlementmaster.id from billsettlementmaster
                      inner join billsettlement on billsettlement.billsettlementmasterid = billsettlementmaster.id
                      where billsettlement.billid in (" + string.Join<long>(",", billids.Keys) + ")";

            List<long> billsettlementids = new List<long>();
            using (var reader = baseData.ExecuteReader(query, param, 100000))
            {
                while (reader.Read())
                {
                    if (!billsettlementids.Contains(reader.GetInt64(0)))
                        billsettlementids.Add(reader.GetInt64(0));
                }
            }

            Data.TaxDataService taxDataService = new TaxDataService() { Context = new Common.UserClaim() { OrganizationID = organizationid } };
            BillsettlementmasterDataService billsettlementdata = new BillsettlementmasterDataService() { Context = new Common.UserClaim() { OrganizationID = organizationid } };
            BillsettlementDataService subBillsettlement = new BillsettlementDataService() { Context = new Common.UserClaim() { OrganizationID = organizationid } };
            subBillsettlement.Context = new Common.UserClaim()
            {
                UserID = 0
            };
            var taxsplits = taxDataService.RangedTaxProducts(string.Join<long>(",", billids.Keys));

            NpgsqlTransaction transaction = null;
            using (NpgsqlConnection conn = baseData.CreateConnection())
            {
                List<Bill> modifiedbills = new List<Bill>();
                SortedDictionary<long, decimal> billdiscounts = new SortedDictionary<long, decimal>();
                conn.Open();
                transaction = conn.BeginTransaction();
                try
                {

                    responsebuilder.Append("<table>\r\n");

                    responsebuilder.AppendFormat("<tr><th>{0}</th><th>{1}</th><th>{2}</th>", "billno", "Before Bill Value", "Before tax");
                    responsebuilder.AppendFormat("<th>{0}</th><th>{1}</th><th>{2}</th></tr>\r\n", "After Bill Value", "After tax", "Discount");

                    var billidlist = billids.Keys.ToList();
                    progressStatus.totalrows = billidlist.Count;
                    for (int i = 0; i < billidlist.Count; i++)
                    {
                        progressStatus.completedrows++;
                        progressStatus.percentage = (int)(((double)progressStatus.completedrows / (double)progressStatus.totalrows) * 100);
                        decimal adjustable = 0;
                        modifiedbills.Add(DiscountOnSettlementRevert(transaction, conn, taxsplits, billidlist[i], ref adjustable, request));
                        billdiscounts.Add(billidlist[i], adjustable);

                    }
                    responsebuilder.Append("</table>\r\n");

                    foreach (var id in billsettlementids)
                    {
                        var billsettlement = billsettlementdata.Search(new BillsettlementmasterCreteria()
                        {
                            id = id
                        });
                        decimal totaldiscount = 0;
                        for (int j = 0; j < billsettlement.First().Billsettlement.Count; j++)
                        {
                            var sitem = billsettlement.First().Billsettlement[j];
                            if (billdiscounts.ContainsKey(sitem.billid))
                            {
                                var billdiscount = billdiscounts[sitem.billid];
                                //sitem.additionaldiscount -= billdiscount;
                                totaldiscount += billdiscount;
                                sitem.billamount += billdiscount;
                                sitem.receivable += billdiscount;
                                sitem.paid += billdiscount;
                            }
                        }
                        billsettlement.First().billamount += totaldiscount;
                        billsettlement.First().paymentinfo.cash_paid += totaldiscount;
                        if (!request.isview)
                        {
                            billsettlementdata.Update(transaction, conn, billsettlement.First());
                            foreach (var bs in billsettlement.First().Billsettlement)
                            {
                                subBillsettlement.Update(transaction, conn, bs);
                            }
                        }
                    }
                    transaction.Commit();
                    conn.Close();
                }

                catch (Exception exp)
                {
                    WebAPI.Common.Logger.Instance(organizationid).Log(exp, WebAPI.Common.Logger.LogType.Error, request);
                }
            }
            response.success = true;

            progressStatus.hascompleted = true;
            progressStatus.result = response;
            return;
        }

        public Bill DiscountOnSettlementRevert(NpgsqlTransaction transaction, NpgsqlConnection conn,
            SortedList<string, Tax.TaxSplit> taxsplits, long billid, ref decimal billdiscount, Request request)
        {
            Data.BillDataService billdataservice = new BillDataService() { Context = new Common.UserClaim() { OrganizationID = organizationid } };
            var bills = billdataservice.Search(new BillCreteria()
            {
                id = billid
            });

            bills[0].DontChangeStock = true;
            billdiscount = 0;
            var list = bills[0].Billitems.OrderByDescending(e => e.receivable).ToList();
            for (int i = 0; i < bills[0].Billitems.Count; i++)
            {

                var itemdiscount = bills[0].Billitems[i].schemediscount;
                var newPrice = Math.Round(bills[0].Billitems[i].price - (bills[0].Billitems[i].additionaldiscount), 0);

                if (!request.parameter.keeptaxslab && taxsplits.ContainsKey(bills[0].Billitems[i].barcode))
                {
                    var newtax = taxsplits[bills[0].Billitems[i].barcode].Find(newPrice);
                    if (newtax.taxpercent != bills[0].Billitems[i].taxpercentage)
                    {
                        bills[0].Billitems[i].taxpercentage = newtax.taxpercent;
                    }
                }
                bills[0].Billitems[i].discount -= bills[0].Billitems[i].schemediscount;
                bills[0].Billitems[i].schemeid = 0;
                bills[0].Billitems[i].schemediscount = 0;
                bills[0].Billitems[i].tax = Math.Round(bills[0].Billitems[i].qty * (newPrice / (100 + bills[0].Billitems[i].taxpercentage) * bills[0].Billitems[i].taxpercentage), 2);
                bills[0].Billitems[i].itemtax = Math.Round(newPrice / (100 + bills[0].Billitems[i].taxpercentage) * bills[0].Billitems[i].taxpercentage, 2);
                bills[0].Billitems[i].rate = newPrice - bills[0].Billitems[i].itemtax;
                bills[0].Billitems[i].saleamount = (newPrice - bills[0].Billitems[i].itemtax) * bills[0].Billitems[i].qty;
                billdiscount += itemdiscount;
                bills[0].Billitems[i].receivable = Math.Round(newPrice * bills[0].Billitems[i].qty, 0);
            }

            responsebuilder.AppendFormat("<tr><td>{0}</td><td>{1}</td><td>{2}</td>", bills[0].billno, bills[0].receivable, bills[0].tax);

            bills[0].schemediscount = 0;
            bills[0].receivable += billdiscount;
            bills[0].tax = bills[0].Billitems.Sum(e => e.tax);
            bills[0].rounding = Math.Round(bills[0].receivable, MidpointRounding.AwayFromZero) - bills[0].receivable;
            bills[0].RecalculateTax();

            responsebuilder.AppendFormat("<td>{0}</td><td>{1}</td><td>{2}</td></tr>\r\n", bills[0].receivable, bills[0].tax, billdiscount);
            if (!request.isview)
                billdataservice.Update(transaction, conn, bills[0], true);

            return bills[0];
        }

        public class BillIDs
        {
            public string billno { get; set; }
            public long billid { get; set; }
            public decimal receivable { get; set; }
        }
        public class ItemwiseTax
        {
            internal decimal creceivable;

            public long productid { get; set; }
            public decimal taxadjusted { get; set; }
            public decimal saleadjusted { get; set; }
            public decimal qty { get; set; }
            public decimal breceivable { get; set; }
        }
        SortedDictionary<long, ItemwiseTax> itemwiseSales = new SortedDictionary<long, ItemwiseTax>();

        public ProgressStatus UpdateScheme(long organizationid, Request request)
        {
            ProgressStatus progressStatus = InfyPOS.Processors.ProcessManager.Instance(organizationid).GetEmptyStatus();
            System.Threading.Thread thread = new System.Threading.Thread(new System.Threading.ParameterizedThreadStart(UpdateScheme));
            thread.Start(new Tuple<long, Request, ProgressStatus>(organizationid, request, progressStatus));
            return progressStatus;
        }

        public void UpdateScheme(object obj)
        {
            var tobj = obj as Tuple<long, Request, ProgressStatus>;
            long organizationid = tobj.Item1;
            Request request = tobj.Item2;
            var progressStatus = tobj.Item3;
            progressStatus.hasstarted = true;
            Response response = new Response();
            response.responsebuilder = responsebuilder;
            if (!string.IsNullOrEmpty(request.parameter.schemename))
            {
                PromotionsDataService promotionsDataService = new PromotionsDataService() { Context = new Common.UserClaim() { OrganizationID = organizationid } };
                var promotions = promotionsDataService.Search(new PromotionsCreteria()
                {
                    name = request.parameter.schemename
                });
                if (promotions.Count == 0)
                {
                    response.success = false;
                    responsebuilder.Append("Given scheme not available");
                    progressStatus.hascompleted = true;
                    progressStatus.result = response;
                    progressStatus.Exception = new Exception("Given scheme not available");
                    return;
                }
                request.parameter.schemeid = promotions.First().id;
            }

            if (!request.parameter.adjustprice && request.parameter.schemeid == 0)
            {
                response.success = false;
                responsebuilder.Append("Scheme is mandatory for analysing");
                progressStatus.hascompleted = true;
                progressStatus.result = response;
                progressStatus.Exception = new Exception("Scheme is mandatory for analysing");
                return;
            }

            progressStatus.status = "Processing...";

            var query = @"
with conflictbills 
as
(
select distinct billid from billitems
inner join bill on bill.id = billitems.billid and bill.isactive = true 
left outer join customer on customer.id = bill.customerid
where billitems.isactive = true and bill.billtype = 0 and (schemeid != 0 or billitems.receivable<0)
and billdate>=@fromdate and billdate<@todate and coalesce(customer.gstno,'') = ''
and cast(billattributes->>'spentreward' as numeric)>0
)
select sum(billitems.receivable),sum(stock.buyingprice::numeric*billitems.qty::numeric),sum(billitems.tax) from bill
inner join billitems on billitems.billid = bill.id and billitems.isactive=true
inner join salablegoods on salablegoods.id = billitems.salablegoodsid
inner join stock on stock.id = salablegoods.stockid and stock.buyingprice>0
inner join billsettlement on billsettlement.billid = bill.id
inner join billsettlementmaster on billsettlementmaster.id =billsettlement.billsettlementmasterid
left outer join customer on customer.id = bill.customerid
where bill.receivable>=@minimumbillvalue and bill.id not in (select billid from conflictbills) and billtype=0 and settled=true and billsettlementmaster.paid_cash>0 and
billsettlementmaster.paid_card=0 and billsettlementmaster.paid_upi=0 and billsettlement.isactive = true and billitems.taxpercentage::numeric=@taxpercentage
and billdate>=@fromdate and billdate<@todate and schemeid = 0 and billitems.receivable>0 and coalesce(customer.gstno,'') = ''";

            long companyid = 0;
            long locationid = 0;
            if (request.parameter.schemeid > 0)
            {
                query = query.Replace("schemeid != 0", "(schemeid != 0 and schemeid != @schemeid)");
            }
            else if (request.parameter.adjustprice)
            {
                query = query.Replace("schemeid != 0 or ", "");
            }

            if (!request.parameter.excludecreditcustomer)
                query = query.Replace("and cast(billattributes->>'spentreward' as numeric)>0", "");

            if (request.parameter.taxpercentage == 0)
            {
                query = query.Replace("and billitems.taxpercentage::numeric=@taxpercentage", "");
            }

            if (!string.IsNullOrEmpty(request.parameter.companyname))
            {
                CompanyDataService companyDataService = new CompanyDataService() { Context = new Common.UserClaim() { OrganizationID = organizationid } };
                var companies = companyDataService.Search(new CompanyCreteria()
                {
                    name = request.parameter.companyname,
                    uniquename = true
                });
                if (companies.Count == 1)
                {
                    query = query.Replace("settled=true", "settled=true and bill.companyid = @companyid");
                    companyid = companies[0].id;
                }
                else
                {

                    response.success = false;
                    responsebuilder.Append("Given company name not available - " + request.parameter.companyname);
                    progressStatus.hascompleted = true;
                    progressStatus.result = response;
                    progressStatus.Exception = new Exception("Given company name not available - " + request.parameter.companyname);
                    return;
                }
            }

            if (!string.IsNullOrEmpty(request.parameter.locationname))
            {
                ReferencelistDataService companyDataService = new ReferencelistDataService() { Context = new Common.UserClaim() { OrganizationID = organizationid } };
                var companies = companyDataService.Search(new ReferencelistCreteria()
                {
                    type = "LOCATION",
                    name = request.parameter.locationname
                });
                if (companies.Count > 0 && companies.Exists(e => e.name == request.parameter.locationname))
                {
                    query = query.Replace("settled=true", "settled=true and bill.locationid = @locationid");
                    locationid = companies.Find(e => e.name == request.parameter.locationname).id;
                }
                else
                {
                    response.success = false;
                    responsebuilder.Append("Given location name not available - " + request.parameter.locationname);
                    progressStatus.hascompleted = true;
                    progressStatus.result = response;
                    progressStatus.Exception = new Exception("Given location name not available - " + request.parameter.locationname);
                    return;
                }

            }


            decimal available = 0;
            decimal receivable = 0;
            var param = new SortedList<string, object>();
            param.Add("fromdate", request.parameter.fromdate);
            param.Add("schemeid", request.parameter.schemeid);
            param.Add("taxpercentage", request.parameter.taxpercentage);
            param.Add("companyid", companyid);
            param.Add("locationid", locationid);
            param.Add("todate", request.parameter.todate.AddDays(1));
            param.Add("minimumbillvalue", request.parameter.minimumbillvalue);
            Common.BaseData baseData = new Common.BaseData() { Context = new Common.UserClaim() { OrganizationID = organizationid } };
            using (var reader = baseData.ExecuteReader(query, param, 100000))
            {
                if (reader.Read())
                {
                    receivable = reader.IsDBNull(0) ? 0 : reader.GetDecimal(0);
                    var stockvalue = reader.IsDBNull(1) ? 0 : reader.GetDecimal(1);
                    var tax = reader.IsDBNull(2) ? 0 : reader.GetDecimal(2);
                    available = receivable - (stockvalue + (stockvalue * (decimal)request.parameter.minimumprofit / (decimal)100));
                    progressStatus.status = "Value : \r\n" + receivable + "\r\n Stock : \r\n" + stockvalue + "\r\n Tax : \r\n" + tax + "\r\n Available :\r\n" + available;
                }
            }

            if (!request.parameter.amountinpercentage &&
                request.parameter.validatecost && available < request.parameter.amount)
            {
                response.success = false;
                responsebuilder.Append("Sale value is less than the expected correction. Maximum limit is - <b>" + Math.Round(available, 0) + "</b>");
                progressStatus.hascompleted = true;
                progressStatus.result = response;
                progressStatus.Exception = new Exception("Sale value is less than the expected correction. Maximum limit is - <b>" + Math.Round(available, 0) + "</b>");
                return;

            }

            query = @"with conflictbills 
as
(
select distinct billid from billitems
inner join bill on bill.id = billitems.billid and bill.isactive = true 
left outer join customer on customer.id = bill.customerid
where billitems.isactive = true and bill.billtype = 0 and (schemeid != 0 or billitems.receivable<0)
and billdate>=@fromdate and billdate<@todate and coalesce(customer.gstno,'') = ''
and cast(billattributes->>'spentreward' as numeric)>0
), allbills 
as
(
select bill.id,bill.billno,sum(billitems.receivable) receivable from bill
inner join billitems on billitems.billid = bill.id and billitems.isactive=true
inner join billsettlement on billsettlement.billid = bill.id
inner join billsettlementmaster on billsettlementmaster.id =billsettlement.billsettlementmasterid
left outer join customer on customer.id = bill.customerid
where bill.receivable>=@minimumbillvalue and bill.id not in (select billid from conflictbills) and billtype=0 
and settled=true and billsettlementmaster.paid_cash>0 and
billsettlementmaster.paid_card=0 and billsettlementmaster.paid_upi=0 and bill.billtype = 0 
and billdate>=@fromdate and billdate<@todate and bill.isactive = true 
and billsettlement.isactive = true and billsettlementmaster.isactive = true
and billitems.taxpercentage::numeric=@taxpercentage and schemeid = 0 and billitems.receivable>0 and coalesce(customer.gstno,'') = ''
group by bill.id,bill.billno
)

select id,receivable,billno from allbills order by receivable desc";

            if (request.parameter.schemeid > 0)
            {
                query = query.Replace("schemeid != 0", "(schemeid != 0 and schemeid != @schemeid)");
            }
            else if (request.parameter.adjustprice)
            {
                query = query.Replace("schemeid != 0 or ", "");
            }
            if (!request.parameter.excludecreditcustomer)
                query = query.Replace("and cast(billattributes->>'spentreward' as numeric)>0", "");

            if (request.parameter.taxpercentage == 0)
            {
                query = query.Replace("and billitems.taxpercentage::numeric=@taxpercentage", "");
            }

            if (!string.IsNullOrEmpty(request.parameter.companyname))
            {
                CompanyDataService companyDataService = new CompanyDataService() { Context = new Common.UserClaim() { OrganizationID = organizationid } };
                var companies = companyDataService.Search(new CompanyCreteria()
                {
                    name = request.parameter.companyname,
                    uniquename = true
                });
                if (companies.Count == 1)
                {
                    query = query.Replace("settled=true", "settled=true and bill.companyid = @companyid");
                    companyid = companies[0].id;
                }
                else
                {

                    response.success = false;
                    responsebuilder.Append("Given company name not available - " + request.parameter.companyname);
                    progressStatus.hascompleted = true;
                    progressStatus.result = response;
                    progressStatus.Exception = new Exception("Given company name not available - " + request.parameter.companyname);
                    return;
                }
            }

            if (!string.IsNullOrEmpty(request.parameter.locationname))
            {
                ReferencelistDataService companyDataService = new ReferencelistDataService() { Context = new Common.UserClaim() { OrganizationID = organizationid } };
                var companies = companyDataService.Search(new ReferencelistCreteria()
                {
                    type = "LOCATION",
                    name = request.parameter.locationname
                });
                if (companies.Count > 0 && companies.Exists(e => e.name == request.parameter.locationname))
                {
                    query = query.Replace("settled=true", "settled=true and bill.locationid = @locationid");
                    locationid = companies.Find(e => e.name == request.parameter.locationname).id;
                }
                else
                {
                    response.success = false;
                    responsebuilder.Append("Given location name not available - " + request.parameter.locationname);
                    progressStatus.hascompleted = true;
                    progressStatus.result = response;
                    progressStatus.Exception = new Exception("Given location name not available - " + request.parameter.locationname);
                    return;
                }
            }
            if (request.parameter.billcount > 0)
            {
                query += "\r\nlimit " + request.parameter.billcount;
            }

            List<BillIDs> billids = new List<BillIDs>();
            List<long> billid = new List<long>();
            using (var reader = baseData.ExecuteReader(query, param, 100000))
            {
                while (reader.Read())
                {
                    billids.Add(new BillIDs()
                    {
                        billid = reader.GetInt64(0),
                        receivable = reader.GetDecimal(1),
                        billno = reader.GetString(2)
                    });
                    billid.Add(reader.GetInt64(0));
                }
            }

            if (billid.Count == 0)
            {
                response.success = false;
                responsebuilder.Append("There is no bills available for corrections.");
                progressStatus.hascompleted = true;
                progressStatus.result = response;
                progressStatus.Exception = new Exception("There is no bills available for corrections.");
                return;
            }
            query = @"select distinct billsettlementmaster.id from billsettlementmaster
                      inner join billsettlement on billsettlement.billsettlementmasterid = billsettlementmaster.id
                      where billsettlement.billid in (" + string.Join<long>(",", billid) + ")";

            List<long> billsettlementids = new List<long>();
            using (var reader = baseData.ExecuteReader(query, param, 100000))
            {
                while (reader.Read())
                {
                    if (!billsettlementids.Contains(reader.GetInt64(0)))
                        billsettlementids.Add(reader.GetInt64(0));
                }
            }

            if (request.parameter.billcount > 0)
            {
                query = @"select sum(billitems.receivable),sum(stock.buyingprice::numeric*billitems.qty::numeric),sum(billitems.tax) from bill
                inner join billitems on billitems.billid = bill.id and billitems.isactive=true
                inner join salablegoods on salablegoods.id = billitems.salablegoodsid
                inner join stock on stock.id = salablegoods.stockid and stock.buyingprice>0
                inner join billsettlement on billsettlement.billid = bill.id
                inner join billsettlementmaster on billsettlementmaster.id =billsettlement.billsettlementmasterid
                where billsettlement.billid in (" + string.Join<long>(",", billid) + ")";

                if (request.parameter.taxpercentage != 0)
                {
                    query = query + " and billitems.taxpercentage::numeric=@taxpercentage";
                    if (!param.ContainsKey("taxpercentage"))
                        param.Add("taxpercentage", request.parameter.taxpercentage);
                }

                using (var reader = baseData.ExecuteReader(query, param, 100000))
                {
                    if (reader.Read())
                    {
                        receivable = reader.GetDecimal(0);
                        var stockvalue = reader.GetDecimal(1);
                        var tax = reader.GetDecimal(2);
                        available = receivable - (stockvalue + (stockvalue * (decimal)request.parameter.minimumprofit / (decimal)100));
                        progressStatus.status = "Value : " + receivable + " Stock : " + stockvalue + " Tax : " + tax + " Available :" + available;
                        responsebuilder.Append("Value : " + receivable + " Stock : " + stockvalue + " Tax : " + tax + " Available :" + available + "</br>");
                    }
                }

                if (!request.parameter.amountinpercentage && request.parameter.validatecost && available < request.parameter.amount)
                {
                    response.success = false;
                    responsebuilder.Append("Sale value is less than the expected correction. Maximum limit is - <b>" + Math.Round(available, 0) + "</b>");
                    progressStatus.result = response;
                    progressStatus.hascompleted = true;
                    progressStatus.result = response;
                    progressStatus.Exception = new Exception("Sale value is less than the expected correction. Maximum limit is - <b>" + Math.Round(available, 0) + "</b>");
                    return;
                }
            }

            if (request.ispreview)
            {
                responsebuilder.Append("Preview Completed");
                progressStatus.hascompleted = true;
                progressStatus.result = response;
                return;
            }
            Data.TaxDataService taxDataService = new TaxDataService() { Context = new Common.UserClaim() { OrganizationID = organizationid } };
            BillsettlementmasterDataService billsettlementdata = new BillsettlementmasterDataService() { Context = new Common.UserClaim() { OrganizationID = organizationid } };
            BillsettlementDataService subBillsettlement = new BillsettlementDataService() { Context = new Common.UserClaim() { OrganizationID = organizationid } };
            subBillsettlement.Context = new Common.UserClaim()
            {
                UserID = 0
            };
            var taxsplits = taxDataService.RangedTaxProducts(string.Join<long>(",", billid));

            NpgsqlTransaction transaction = null;
            using (NpgsqlConnection conn = baseData.CreateConnection())
            {
                List<Bill> modifiedbills = new List<Bill>();
                SortedDictionary<long, decimal> billdiscounts = new SortedDictionary<long, decimal>();
                conn.Open();
                transaction = conn.BeginTransaction();
                try
                {
                    decimal adjustable = request.parameter.amount;
                    decimal adjustablepercentage = Math.Round(request.parameter.amount / receivable * 100, 4);
                    if (request.parameter.amountinpercentage)
                    {
                        adjustable = Math.Round(receivable * request.parameter.amount / 100, 0);
                        adjustablepercentage = request.parameter.amount;
                    }

                    responsebuilder.AppendFormat("<span>Requested : {0}</span> ", request.parameter.amount.ToString("F2"));
                    responsebuilder.AppendFormat("<span>Available : {0}</span> ", receivable.ToString("F2"));
                    responsebuilder.AppendFormat("<span>Percentage : {0}</span> ", adjustablepercentage.ToString("F4"));
                    responsebuilder.AppendFormat("<span>Bill Count : {0}</span> ", request.parameter.billcount.ToString("F0"));

                    responsebuilder.Append("<table>\r\n");
                    responsebuilder.AppendFormat("<tr><th>{0}</th><th>{1}</th><th>{2}</th>", "billno", "Before Bill Value", "Before tax");
                    responsebuilder.AppendFormat("<th>{0}</th><th>{1}</th><th>{2}</th></tr>\r\n", "After Bill Value", "After tax",
                        request.parameter.adjustprice ? "Price Adjusted" : "Discount");

                    decimal adjustedtotalvalue = 0;
                    decimal adjustbletotalvalue = adjustable;

                    progressStatus.totalrows = billids.Count;

                    if (!string.IsNullOrEmpty(request.parameter.billno))
                    {
                        billids.RemoveAll(x => x.billno != request.parameter.billno);
                    }

                    for (int i = 0; i < billids.Count; i++)
                    {
                        progressStatus.completedrows++;
                        progressStatus.percentage = (int)(((double)progressStatus.completedrows / (double)progressStatus.totalrows) * 100);
                        if (i == billids.Count - 1 && adjustable > 0 && billids[i].receivable > adjustable)
                        {
                            var expected = adjustable;
                            modifiedbills.Add(DiscountOnSettlement(transaction, conn, taxsplits, billids[i].receivable, billids[i].billid,
                                ref adjustable, request));

                            billdiscounts.Add(billids[i].billid, adjustable);
                            adjustedtotalvalue += adjustable;
                        }
                        else if (adjustable > 0)
                        {
                            var tadjust = Math.Round(billids[i].receivable * adjustablepercentage / 100, 0);
                            if (tadjust > adjustable)
                                tadjust = adjustable;

                            if (tadjust > 0)
                            {
                                var expected = tadjust;
                                modifiedbills.Add(DiscountOnSettlement(transaction, conn, taxsplits, billids[i].receivable, billids[i].billid,
                                    ref tadjust, request));

                                adjustable -= tadjust;
                                billdiscounts.Add(billids[i].billid, tadjust);
                                adjustedtotalvalue += tadjust;
                            }
                        }
                    }

                    responsebuilder.AppendFormat("<tr><td colspan='5'>&nbsp;</td><td>{0}</td></tr>", adjustedtotalvalue.ToString("F2"));

                    responsebuilder.Append("</table>\r\n");
                    responsebuilder.Append("<br><hr><br><table>\r\n");
                    responsebuilder.AppendFormat("<tr><th>{0}</th><th>{1}</th><th>{2}</th><th>{3}</th><th>{4}</th><th>{5}</th></tr>\r\n",
                        "Produt", "Qty", "Adjusted Value", "Adjusted tax", "Before", "After");
                    ProductsDataService productsDataService = new ProductsDataService() { Context = new Common.UserClaim() { OrganizationID = organizationid } };
                    var products = productsDataService.Search(new ProductsCreteria());
                    foreach (var prod in itemwiseSales.Values)
                    {
                        string productname = "Unknown";
                        if (products.Exists(e => e.id == prod.productid))
                        {
                            productname = products.Find(e => e.id == prod.productid).name;
                        }
                        responsebuilder.AppendFormat("<tr><td>{0}</td><td>{1}</td><td>{2}</td><td>{3}</td><td>{4}</td><td>{5}</td></tr>\r\n", productname,
                            prod.qty, prod.saleadjusted.ToString("F2")
                            , prod.taxadjusted.ToString("F2")
                            , prod.breceivable.ToString("F2")
                            , prod.creceivable.ToString("F2"));
                    }

                    responsebuilder.AppendFormat("<tr><td>{0}</td><td>{1}</td><td>{2}</td><td>{3}</td><td>{4}</td><td>{5}</td></tr>\r\n", "Total",
                        itemwiseSales.Values.Sum(e => e.qty),
                        itemwiseSales.Values.Sum(e => e.saleadjusted).ToString("F2"),
                        itemwiseSales.Values.Sum(e => e.taxadjusted).ToString("F2"),
                        itemwiseSales.Values.Sum(e => e.breceivable).ToString("F2"),
                        itemwiseSales.Values.Sum(e => e.creceivable).ToString("F2"));

                    responsebuilder.Append("</table>\r\n");
                    if (!request.isview)
                    {
                        foreach (var id in billsettlementids)
                        {
                            var billsettlement = billsettlementdata.Search(new BillsettlementmasterCreteria()
                            {
                                id = id
                            });
                            decimal totaldiscount = 0;
                            for (int j = 0; j < billsettlement.First().Billsettlement.Count; j++)
                            {
                                var sitem = billsettlement.First().Billsettlement[j];
                                if (billdiscounts.ContainsKey(sitem.billid))
                                {
                                    var billdiscount = billdiscounts[sitem.billid];
                                    //sitem.additionaldiscount += billdiscount;
                                    totaldiscount += billdiscount;
                                    sitem.billamount -= billdiscount;
                                    sitem.receivable -= billdiscount;
                                    sitem.paid -= billdiscount;
                                }
                            }
                            billsettlement.First().billamount -= totaldiscount;
                            billsettlement.First().paidamount -= totaldiscount;
                            billsettlement.First().receivable -= totaldiscount;
                            billsettlement.First().paymentinfo.cash_paid -= totaldiscount;
                            if(billsettlement.First().Billsettlement != null)
                            {
                                var bs = billsettlement.First().Billsettlement.OrderByDescending(x => x.paid).First();
                                bs.receivable -= totaldiscount;
                                bs.paid -= totaldiscount;
                                bs.billamount -= totaldiscount;
                            }
                                
                            if (billsettlement.First().paidamount > 0 &&
                                request.parameter.autoround)
                            {
                                billsettlement.First().paymentinfo.cash_paid -= billsettlement.First().paymentinfo.cash_return;
                                billsettlement.First().paymentinfo.cash_return = 0;
                                billsettlement.First().tax = 0;
                            }
                            if (!request.isview)
                            {
                                billsettlementdata.Update(transaction, conn, billsettlement.First());
                                subBillsettlement.Update(transaction, conn, billsettlement.First().Billsettlement, billsettlement.First().id);
                            }
                        }
                    }
                    transaction.Commit();
                    conn.Close();

                }
                catch (Exception exp)
                {
                    progressStatus.Exception = exp;
                    WebAPI.Common.Logger.Instance(organizationid).Log(exp, WebAPI.Common.Logger.LogType.Error, request);
                }
            }
            response.success = true;
            responsebuilder.Append("Sale value is less than the expected correction. Maximum limit is - <b>" + Math.Round(available, 0) + "</b>");
            progressStatus.hascompleted = true;
            progressStatus.result = response;
            if (progressStatus.Exception == null)
                progressStatus.Exception = new Exception("Sale value is less than the expected correction. Maximum limit is - <b>" + Math.Round(available, 0) + "</b>");
            return;
        }
        public static decimal CustomRound(decimal value)
        {
            if (value <= 100)
            {
                // No rounding
                return value;
            }
            else if (value > 100 && value <= 1000)
            {
                // Round to nearest 5
                return Math.Round(value / 5m, MidpointRounding.AwayFromZero) * 5m;
            }
            else
            {
                // Round to nearest 10
                return Math.Round(value / 10m, MidpointRounding.AwayFromZero) * 10m;
            }
        }
        public Bill DiscountOnSettlement(NpgsqlTransaction transaction, NpgsqlConnection conn,
            SortedList<string, Tax.TaxSplit> taxsplits, decimal billreceivable,
            long billid, ref decimal billdiscount, Request request)
        {
            Data.BillDataService billdataservice = new BillDataService() { Context = new Common.UserClaim() { OrganizationID = organizationid } };
            var bills = billdataservice.Search(new BillCreteria()
            {
                id = billid
            });
            return DiscountOnSettlement(transaction, conn,
            taxsplits, billreceivable,
            bills, ref billdiscount, request);
        }
        public Bill DiscountOnSettlement(NpgsqlTransaction transaction, NpgsqlConnection conn,
            SortedList<string, Tax.TaxSplit> taxsplits, decimal billreceivable,
            List<Bill> bills, ref decimal billdiscount, Request request)
        {
            Data.BillDataService billdataservice = new BillDataService() { Context = new Common.UserClaim() { OrganizationID = organizationid } };

            //if (bills[0].billno == "M-223077")
            //{
            //    var billno = bills[0].billno;
            //}


            bills[0].DontChangeStock = true;
            //var billreceivable = bills[0].total;
            decimal schemediscountadded = 0;
            decimal planneddiscount = 0;
            bool taxchange = false;
            var list = bills[0].Billitems.OrderByDescending(e => e.receivable).ToList();
            var totaladjustment = billdiscount;

            var billitems = bills[0].Billitems;
            if (request.parameter.taxpercentage > 0)
            {
                billitems = billitems.FindAll(x => x.taxpercentage == request.parameter.taxpercentage);
                billreceivable = billitems.Sum(x => x.receivable);
            }
            for (int i = 0; i < billitems.Count; i++)
            {

                if (!itemwiseSales.ContainsKey(billitems[i].productid))
                    itemwiseSales.Add(billitems[i].productid, new ItemwiseTax()
                    {
                        productid = billitems[i].productid
                    });

                itemwiseSales[billitems[i].productid].breceivable += billitems[i].receivable;

                var itemdiscountpercent = Math.Round(billitems[i].receivable / billreceivable * 100, 2);
                var itemdiscount = Math.Round((totaladjustment * itemdiscountpercent / 100), 0);
                if (billdiscount < itemdiscount)
                    itemdiscount = billdiscount;

                if (i == billitems.Count - 1)
                {
                    itemdiscount = billdiscount;
                }

                if (request.parameter.autoround)
                {
                    var schemediscount = itemdiscount;
                    var currentprice = billitems[i].price - billitems[i].additionaldiscount;
                    var roundnewPrice = Math.Round(billitems[i].price - (billitems[i].additionaldiscount +
                    (schemediscount > 0 ? (schemediscount / billitems[i].qty) : 0)), 0);
                    var newprice = CustomRound(roundnewPrice);
                    if(newprice> currentprice)
                    {
                        itemdiscount = (newprice- currentprice)*billitems[i].qty;
                    }
                    else
                    {
                        itemdiscount = (currentprice-newprice) * billitems[i].qty;
                    }
                    if (itemdiscount < 1)
                        continue;
                }


                billdiscount -= itemdiscount;


                if (billitems[i].schemediscount > 0 &&
                    billitems[i].schemeid == 0)
                {
                    billitems[i].schemediscount = 0;
                }

                billitems[i].schemediscount += itemdiscount;
                schemediscountadded += itemdiscount;

                var newPrice = Math.Round(billitems[i].price - (billitems[i].additionaldiscount +
                    (billitems[i].schemediscount > 0 ? (billitems[i].schemediscount / billitems[i].qty) : 0)), 0);

                var current = (billitems[i].price -
                    (billitems[i].schemediscount - itemdiscount > 0 ? ((billitems[i].schemediscount - itemdiscount) / billitems[i].qty) : 0) -
                    billitems[i].additionaldiscount) * billitems[i].qty;

                //var newrece = (newPrice) * billitems[i].qty;
                //if(newrece+itemdiscount!=current)
                //{
                //    var diff = current - (newrece + (itemdiscount/ billitems[i].qty));
                //    newPrice += Math.Round(diff / billitems[i].qty,2);
                //}


                if (!request.parameter.keeptaxslab && taxsplits.ContainsKey(billitems[i].barcode))
                {
                    var newtax = taxsplits[billitems[i].barcode].Find(newPrice);
                    if (newtax.taxpercent != billitems[i].taxpercentage)
                    {
                        billitems[i].taxpercentage = newtax.taxpercent;
                        taxchange = true;
                    }
                }

                var currenttax = billitems[i].tax;
                if (billitems[i].schemeid == 0)
                {
                    billitems[i].schemeid = request.parameter.schemeid;
                }
                else
                {
                    if (billitems[i].attributes == null) billitems[i].attributes = new BillItemAttributes();
                    billitems[i].attributes.schemeid = billitems[i].schemeid;
                    billitems[i].attributes.schemediscount = (billitems[i].schemediscount - itemdiscount);
                    billitems[i].schemeid = request.parameter.schemeid;
                }
                if (!request.parameter.adjustprice)
                    billitems[i].discount += itemdiscount;

                billitems[i].receivable -= itemdiscount;

                billitems[i].tax = Math.Round((billitems[i].receivable / (100 + billitems[i].taxpercentage) * billitems[i].taxpercentage), 4);
                billitems[i].itemtax = Math.Round(billitems[i].tax > 0 ? (billitems[i].tax / billitems[i].qty) : 0, 4);
                billitems[i].rate = Math.Round((billitems[i].receivable / (100 + billitems[i].taxpercentage) * 100), 4);
                billitems[i].saleamount = Math.Round((billitems[i].rate) * billitems[i].qty, 4);

                planneddiscount += itemdiscount;
                itemwiseSales[billitems[i].productid].creceivable += billitems[i].receivable;
                itemwiseSales[billitems[i].productid].saleadjusted += billitems[i].schemediscount;
                itemwiseSales[billitems[i].productid].taxadjusted += (currenttax - billitems[i].tax);
                itemwiseSales[billitems[i].productid].qty += billitems[i].qty;

                if (request.parameter.adjustprice)
                {
                    billitems[i].schemediscount = 0;
                    billitems[i].price = newPrice;
                    billitems[i].salerate = newPrice + billitems[i].additionaldiscount+ billitems[i].discount;
                    if(billitems[i].discount>0 || billitems[i].discountpercentage>0)
                        billitems[i].discountpercentage = Math.Round(billitems[i].discount / billitems[i].salerate * 100, 2);

                    if (billitems[i].additionaldiscount > 0)
                    {
                        billitems[i].additionaldiscountpercentage = Math.Round(billitems[i].additionaldiscount / billitems[i].salerate*100, 2);
                    }
                    else
                    {
                        billitems[i].additionaldiscountpercentage = 0;
                    }
                    if (billitems[i].schemeid == request.parameter.schemeid)
                        billitems[i].schemeid = 0;
                }
            }

            responsebuilder.AppendFormat("<tr><td>{0}</td><td>{1}</td><td>{2}</td>", bills[0].billno + (taxchange ? "*" : ""), bills[0].total, bills[0].tax);

            var currentbillvalue = bills[0].receivable + bills[0].rounding;

            bills[0].receivable = bills[0].Billitems.Sum(e => e.receivable);
            bills[0].tax = bills[0].Billitems.Sum(e => e.tax);
            bills[0].discount = bills[0].Billitems.Sum(e => e.discount);
            bills[0].additionaldiscount = bills[0].Billitems.Sum(e => e.additionaldiscount * e.qty);
            bills[0].rounding = Math.Round(bills[0].receivable, MidpointRounding.AwayFromZero) - bills[0].receivable;
            bills[0].RecalculateTax();

            var computeddiscount = billdiscount = currentbillvalue - (bills[0].receivable + bills[0].rounding);
            billdiscount = schemediscountadded + bills[0].rounding;

            if (computeddiscount != billdiscount)
                billdiscount = computeddiscount;


            if (bills[0].billattributes != null &&
                bills[0].billattributes.settlement != null)
            {
                bills[0].billattributes.settlement.discount = bills[0].discount;
                bills[0].billattributes.settlement.paidamount -= billdiscount;
                bills[0].billattributes.settlement.billamount -= billdiscount;
                bills[0].billattributes.settlement.paymentinfo.cash_paid -= billdiscount;
                if(bills[0].billattributes.settlement.paymentinfo.cash_return>0)
                {
                    bills[0].billattributes.settlement.paymentinfo.cash_paid -= bills[0].billattributes.settlement.paymentinfo.cash_return;
                    bills[0].billattributes.settlement.paymentinfo.cash_return = 0;
                }
                bills[0].billattributes.settlement.receivable -= billdiscount;
                if (bills[0].billattributes.settlement.Billsettlement != null)
                {
                    bills[0].billattributes.settlement.Billsettlement[0].paid -= billdiscount;
                    bills[0].billattributes.settlement.Billsettlement[0].billamount -= billdiscount;
                    bills[0].billattributes.settlement.Billsettlement[0].receivable -= billdiscount;
                    bills[0].billattributes.settlement.Billsettlement[0].discount = bills[0].discount;
                    bills[0].billattributes.settlement.Billsettlement[0].additionaldiscount = bills[0].additionaldiscount;
                }
            }

            if (!request.isview)
            {
                bills[0].DontChangeStock = true;
                billdataservice.Update(transaction, conn, bills[0], true);
            }
            //billdiscount = billreceivable - bills[0].total;
           
            responsebuilder.AppendFormat("<td>{0}</td><td>{1}</td><td>{2}</td></tr>\r\n", bills[0].total, bills[0].tax, billdiscount);
            //responsebuilder.AppendFormat("<td>{0}</td><td>{1}</td><td>{2}~{3}</td></tr>\r\n", bills[0].total, bills[0].tax, billdiscount, bills[0].Billitems.Sum(e=>e.schemediscount));
            return bills[0];
        }

        internal static ProgressStatus Run(long organizationid, long id, byte[] data)
        {
            var request = Newtonsoft.Json.JsonConvert.DeserializeObject<Request>(System.Text.ASCIIEncoding.ASCII.GetString(data));
            if (request == null ||
                string.IsNullOrEmpty(request.command))
            {
                new Exception("Given data is not in correct format");
            }
            request.fileid = id;
            return Run(organizationid, request);
        }

        public System.Text.StringBuilder responsebuilder = new System.Text.StringBuilder();


        public static ProgressStatus Run(long organizationid, Request source)
        {
            CommandManager commandManager = new CommandManager(organizationid);
            if (source.command == "UpdateScheme")
            {
                if (source.isrevert)
                    return commandManager.SchemeDiscountRevert(organizationid, source);
                return commandManager.UpdateScheme(organizationid, source);
            }
            throw new Exception("Can't find the command on the request");
        }

        //private Response UpdateLicense(Request source)
        //{
        //    ConfigurationDataService configurationDataService = new ConfigurationDataService() { Context = new Common.UserClaim() { OrganizationID = organizationid } };
        //    var config = (new ConfigurationCreteria()
        //    {
        //        createdby = source.fileid,
        //        version = 1,
        //        isactive = true,
        //        key = "LICENSE",
        //        name = source.parameter.schemename,
        //        reportformat = new Configuration.ReportFormat()
        //        {
        //            format = Convert.ToBase64String(source.data)
        //        }
        //    });
        //    var existing = configurationDataService.Search(new ConfigurationCreteria()
        //    {
        //        key = "LICENSE",
        //        name = source.parameter.schemename,
        //        createdby = source.fileid
        //    });

        //    var response = new Response()
        //    {
        //        responsebuilder = new System.Text.StringBuilder()
        //    };
        //    if (existing.Count > 0)
        //    {
        //        response.responsebuilder.Append("Given License (" + source.parameter.schemename + ") already available in this system");
        //    }
        //    else
        //    {
        //        InfyPOS.Data.CompanyDataService CompanyData = new InfyPOS.Data.CompanyDataService() { Context = new Common.UserClaim() { OrganizationID = organizationid } };
        //        var result = CompanyData.Search(new InfyPOS.Models.CompanyCreteria());

        //        if (result.Count == 0)
        //        {
        //            response.responsebuilder.Append("No company available in the database (" + source.parameter.schemename + ")");
        //            return response;
        //        }
        //        //comment here for development
        //        var license = LicenseManager.License.Create(result.OrderBy(e => e.id).First().name,System.Text.ASCIIEncoding.ASCII.GetString(source.data));
        //        if (license.valid)
        //        {
        //            configurationDataService.Insert(config);
        //            response.responsebuilder.Append("New License added (" + source.parameter.schemename + ")");
        //        }
        //        else
        //        {
        //            response.responsebuilder.Append("Given License (" + source.parameter.schemename + ") is not a valid file");
        //            response.responsebuilder.AppendLine();
        //            response.responsebuilder.Append(license.message);
        //        }
        //    }
        //    return response;
        //}
    }
}