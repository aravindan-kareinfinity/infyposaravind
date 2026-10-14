using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using InfyPOS.Models;
using InfyPOS.Common;
using Npgsql;
using System.Text;
using WebAPI.Data;
using InfyPOS.Services;

namespace InfyPOS.Data

{
    public class StoretransactionDataService : SAASData
    {
        const string SQL_Search = @"select companyid, locationid, actiondate, type, attributes, createdby, id, createdon, version, isactive, modifiedby, modifiedon, walkincustomerinfo from storetransaction where id=@id";

        const string SQL_Insert = @"INSERT INTO public.storetransaction(companyid, locationid, actiondate, type, attributes, createdby, createdon, version, isactive, modifiedby, modifiedon, walkincustomerinfo) VALUES(@companyid, @locationid, @actiondate, @type, @attributes, @createdby, @createdon, @version, @isactive, @modifiedby, @modifiedon, @walkincustomerinfo) RETURNING id";

        const string SQL_Update = @"UPDATE public.storetransaction SET companyid=@companyid, locationid=@locationid, actiondate=@actiondate, type=@type, attributes=@attributes, version=@version+1, modifiedby=@modifiedby, modifiedon=@modifiedon, walkincustomerinfo=@walkincustomerinfo where id=@id and version=@version RETURNING version";

        const string SQL_Delete = @"update public.storetransaction set isactive=false,version=@version+1,modifiedby=@modifiedby,modifiedon=@modifiedon where id=@id and version=@version";

        const string SQL_DeleteType = @"update public.storetransaction set isactive=false where type=@type and actiondate=@actiondate";

        public List<Storetransaction> Search(StoretransactionCreteria _storetransaction, NpgsqlConnection conn, NpgsqlTransaction transaction)
        {
            List<Storetransaction> lst = new List<Storetransaction>();
            NpgsqlCommand command = null;
            if (_storetransaction.id != 0)
            {
                command = transaction == null ? new NpgsqlCommand(SQL_Search, conn) : new NpgsqlCommand(SQL_Search, conn, transaction);
                command.Parameters.Add("id", NpgsqlTypes.NpgsqlDbType.Bigint).Value = _storetransaction.id;
            }
            else
            {
                QueryBuilder qb = new QueryBuilder();
                qb.CurrentQuery = SQL_Search.Substring(0, SQL_Search.IndexOf("where"));
                qb.AddParameter("isactive", true, "=");
                if (!string.IsNullOrEmpty(_storetransaction.type))
                    qb.AddParameter("type", _storetransaction.type, "=");
                if (_storetransaction.companyid > 0)
                    qb.AddParameter("companyid", _storetransaction.companyid, "=");
                if (_storetransaction.locationid > 0)
                    qb.AddParameter("locationid", _storetransaction.locationid, "=");

                if (_storetransaction.fromdate != DateTime.MinValue && _storetransaction.todate != DateTime.MinValue)
                {
                    qb.AddParameter("storetransaction.actiondate", "datefrom", _storetransaction.fromdate.Date, ">=");
                    qb.AddParameter("storetransaction.actiondate", "dateto", _storetransaction.todate.Date.AddDays(1), "<");
                }
                if (_storetransaction.actiondate != DateTime.MinValue)
                {
                    qb.AddParameter("storetransaction.actiondate", "actionfrom", _storetransaction.actiondate.Date, "=");
                }
                qb.SortField = "storetransaction.actiondate desc,id desc";
                command = qb.GetCommand(conn);
            }

            using (System.Data.IDataReader reader = command.ExecuteReader())
            {
                while (reader.Read())
                {
                    Storetransaction ostoretransaction = new Storetransaction();
                    ostoretransaction.companyid = reader["companyid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["companyid"]);
                    ostoretransaction.locationid = reader["locationid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["locationid"]);
                    ostoretransaction.actiondate = reader["actiondate"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(reader["actiondate"]);
                    ostoretransaction.type = reader["type"] == DBNull.Value ? "" : reader["type"].ToString();
                    ostoretransaction.attributes_json = reader["attributes"] == DBNull.Value ? null : reader["attributes"].ToString();
                    ostoretransaction.createdby = reader["createdby"] == DBNull.Value ? 0 : Convert.ToInt64(reader["createdby"]);
                    ostoretransaction.id = reader["id"] == DBNull.Value ? 0 : Convert.ToInt64(reader["id"]);
                    ostoretransaction.createdon = reader["createdon"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(reader["createdon"]);
                    ostoretransaction.version = reader["version"] == DBNull.Value ? 0 : Convert.ToInt32(reader["version"]);
                    ostoretransaction.isactive = reader["isactive"] == DBNull.Value ? false : Convert.ToBoolean(reader["isactive"]);
                    ostoretransaction.modifiedby = reader["modifiedby"] == DBNull.Value ? 0 : Convert.ToInt64(reader["modifiedby"]);
                    ostoretransaction.modifiedon = reader["modifiedon"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(reader["modifiedon"]);
                    ostoretransaction.walkincustomerinfo_json = reader["walkincustomerinfo"] == DBNull.Value ? null : reader["walkincustomerinfo"].ToString();
                    lst.Add(ostoretransaction);
                }
            }
            return lst;
        }

        internal Storetransaction.ReferralToken.PrintCount SearchReferralToken(string locationid, string token)
        {
            long _locationid = 0;
            if (long.TryParse(locationid, out _locationid))
            {
                token = token.Replace("$", "");
                var referrals = Search(new StoretransactionCreteria()
                {
                    locationid = _locationid,
                    actiondate = DateTimeNow.Date,
                    type = "Referral"
                });
                if (referrals.Count == 1 &&
                    referrals[0].ReferralTokenAttribute != null &&
                    referrals[0].ReferralTokenAttribute.printhistory.Exists(e => e.token == token))
                {
                    return referrals[0].ReferralTokenAttribute.printhistory.Find(e => e.token == token);
                }
            }
            return null;
        }

        internal WalkinSummary WalkinSummary(Storetransaction item)
        {
            var list = Search(new StoretransactionCreteria()
            {
                id = item.id
            });

            if (list.Count != 1) return null;

            item = list.First();

            List<WalkinSummary.WalkinSummaryItem> results = new List<WalkinSummary.WalkinSummaryItem>();

            var hourlysummary = Enumerable.Range(0, 24).ToList().ConvertAll(e => new WalkinSummary.WalkinSummaryHour()
            {
                hour24 = e,
                hour = e > 12 ? (e - 12).ToString("N0") + " PM" : e.ToString("N0") + " AM"
            });

            var walkintokens = Enumerable.Range(item.WalkinTokenAttribute.startno,
                item.WalkinTokenAttribute.endno).ToList().ConvertAll(e => new WalkinSummary.WalkinSummaryItem()
                {
                    tokenno = e.ToString(),
                    token = e
                });

            if (item.WalkinTokenAttribute.printhistory != null)
            {
                foreach (var ptoken in item.WalkinTokenAttribute.printhistory)
                {
                    var tokenprinted = ptoken.at.ToString("hh:mm tt");
                    var twalk = walkintokens.FindAll(e => e.token >= ptoken.from && e.token <= ptoken.to);
                    twalk.ForEach(e =>
                    {
                        e.tokenprinted = tokenprinted;
                        e.printed = ptoken.at;
                    });
                }
            }

            if (item.WalkinTokenAttribute.issuehistory != null)
            {
                foreach (var ptoken in item.WalkinTokenAttribute.issuehistory)
                {
                    var tokenissued = ptoken.at.ToString("hh:mm tt");
                    var twalk = walkintokens.FindAll(e => e.token >= ptoken.from && e.token <= ptoken.to);
                    twalk.ForEach(e =>
                    {
                        e.tokenissued = tokenissued;
                        e.issued = ptoken.at;
                    });
                }
            }

            string query = @"select cast(billattributes->>'walkintoken' as varchar) walkintoken,createdon,billno from bill
		where locationid=@locationid and billdate>=@fromdate and billdate<@todate and billattributes->>'walkintoken' is not null";
            var param = new SortedList<string, object>();
            param.Add("fromdate", item.actiondate.Date);
            param.Add("todate", item.actiondate.Date.AddDays(1));
            param.Add("locationid", item.locationid);
            param.Add("storetransactionid", item.id);

            using (var reader = ExecuteReader(query, param))
            {
                while (reader.Read())
                {
                    var tokenno = reader.GetString(0);
                    var index = walkintokens.FindIndex(e => e.tokenno == tokenno);
                    if (index >= 0)
                    {
                        walkintokens[index].billno = reader.GetString(2);
                        walkintokens[index].billprinted = reader["createdon"] == DBNull.Value ? "" :
                            reader.GetDateTime(1).ToString("hh:mm tt");
                        walkintokens[index].billed = reader["createdon"] == DBNull.Value ? item.actiondate.Date :
                            reader.GetDateTime(1);
                    }
                    else
                    {
                        walkintokens.Add(new WalkinSummary.WalkinSummaryItem()
                        {
                            tokenno = tokenno,
                            billno = reader.GetString(2),
                            billprinted = reader["createdon"] == DBNull.Value ? "" :
                            reader.GetDateTime(1).ToString("hh:mm tt"),
                            billed = reader["createdon"] == DBNull.Value ? item.actiondate.Date : reader.GetDateTime(1)
                        });
                    }
                }
            }

            query = @"		Select count(bill.billno) as billcount, date_part('hour', bill.createdon) billhrs
                            from bill 
                            where locationid=@locationid and bill.isactive = true 
                            and bill.billdate>=@fromdate and bill.billdate<@todate
                            and bill.billtype != 9 and bill.isb2b=false
                            group by date_part('hour', bill.createdon)
                            order by date_part('hour', bill.createdon)";

            foreach (var hourly in hourlysummary)
            {
                hourly.billed = walkintokens.Count(e => e.billed.Hour == hourly.hour24 && !string.IsNullOrEmpty(e.billno));
                hourly.issued = walkintokens.Count(e => e.issued.Hour == hourly.hour24 && !string.IsNullOrEmpty(e.tokenissued));
                hourly.printed = walkintokens.Count(e => e.printed.Hour == hourly.hour24 && !string.IsNullOrEmpty(e.tokenprinted));
            }
            using (var reader = ExecuteReader(query, param))
            {
                while (reader.Read())
                {
                    int billcount = reader[0] != DBNull.Value ? Convert.ToInt32(reader[0]) : 0;
                    int hour = reader[1] != DBNull.Value ? Convert.ToInt32(reader[1]) : 0;
                    if (hourlysummary.Exists(e => e.hour24 == hour))
                    {
                        hourlysummary.Find(e => e.hour24 == hour).actualbilled = billcount;
                    }
                    else
                    {
                        hourlysummary.Add(new WalkinSummary.WalkinSummaryHour()
                        {
                            actualbilled = billcount,
                            hour24 = hour,
                            hour = hour > 12 ? (hour - 12).ToString("N0") + " PM" : hour.ToString("N0") + " AM"
                        });
                    }
                }
            }

            try
            {
                query = @"select storetransaction.id, cast(walkincustomerlist->> 'token' as character varying(50)) tokenno, 
                        cast(walkincustomerlist->> 'customerid' as numeric) customerid,
                        customer.name,
                        cast(walkincustomerlist->> 'mobileno' as character varying(50)) mobileno
                        from storetransaction LEFT JOIN LATERAL
                        json_array_elements (storetransaction.walkincustomerinfo :: json) walkincustomerlist ON true
                        left outer join customer on customer.id = cast(walkincustomerlist->> 'customerid' as bigint) 
                        where storetransaction.id=@storetransactionid ";

                using (var reader = ExecuteReader(query, param))
                {
                    while (reader.Read())
                    {
                        string tokenno = reader["tokenno"] == DBNull.Value ? "" : reader["tokenno"].ToString();
                        var index = walkintokens.FindIndex(e => e.tokenno == tokenno);
                        if (index >= 0)
                        {
                            walkintokens[index].customername = reader["name"] == DBNull.Value ? "" : reader["name"].ToString();
                            walkintokens[index].mobileno = reader["mobileno"] == DBNull.Value ? "" : reader["mobileno"].ToString();
                        }
                    }
                }
            }
            catch (Exception exp)
            {
            }

            return new WalkinSummary()
            {
                List = walkintokens,
                Hourly = hourlysummary
            };
        }

        internal StoretransactionCreteria PrintReferralToken(StoretransactionCreteria item)
        {

            InfyPOS.Data.ConfigurationDataService config = new ConfigurationDataService() { Context = this.Context };
            var reportformats = config.Search(new ConfigurationCreteria
            {
                key = "WITOKEN"
            });

            if (reportformats.Count == 0)
                throw new Exception("Please configure Walk In Token");

            int from = 0;
            int to = 0;

            if (item.id == 0)
            {
                item.id = Insert(new Storetransaction()
                {
                    locationid = item.locationid,
                    actiondate = item.actiondate,
                    type = "Referral",
                    isactive = true,
                    createdby = Context.UserID,
                    modifiedby = Context.UserID,
                    createdon = DateTimeNow,
                    modifiedon = DateTimeNow,
                    version = 1,
                    ReferralTokenAttribute = new Storetransaction.ReferralToken()
                    {
                        startno = 1,
                        endno = 0
                    }
                }).id;
            }

            var walkin = Search(new StoretransactionCreteria()
            {
                id = item.id
            }).First();

            var fromtotokens = item.tokennumbers.Split('-');
            if (fromtotokens.Length == 1)
            {
                from = walkin.ReferralTokenAttribute.endno + 1;
                to = walkin.ReferralTokenAttribute.endno + int.Parse(fromtotokens[0]);
            }
            else
            {
                from = int.Parse(fromtotokens[0]);
                to = int.Parse(fromtotokens[1]);
            }

            List<Storetransaction.ReferralToken.PrintCount> prnlist = new List<Storetransaction.ReferralToken.PrintCount>();
            if (walkin.ReferralTokenAttribute.endno < to)
            {
                if (walkin.ReferralTokenAttribute.printhistory == null)
                    walkin.ReferralTokenAttribute.printhistory = new List<Storetransaction.ReferralToken.PrintCount>();
                for (var i = walkin.ReferralTokenAttribute.endno + 1; i <= to; i++)
                {
                    prnlist.Add(new Storetransaction.ReferralToken.PrintCount()
                    {
                        at = DateTimeNow,
                        no = i,
                        agentid = item.agentid,
                        discount = item.discount,
                        discountpercentage = item.discountpercentage,
                        employeeid = item.employeeid,
                        printedby = Context.EmployeeID,
                        token = WebAPI.Data.Token.RandomString(8)
                    });
                }
                walkin.ReferralTokenAttribute.printhistory.AddRange(prnlist);
                walkin.ReferralTokenAttribute.endno = to;
                Update(walkin);
            }

            item.modifiedon = walkin.modifiedon;
            item.modifiedby = walkin.modifiedby;
            item.createdon = walkin.createdon;
            item.createdby = walkin.createdby;
            item.isactive = walkin.isactive;
            item.version = walkin.version;

            var walkintokens = prnlist.ConvertAll(e => new WalkInToken()
            {
                ptoken = "$" + e.token + "$",
                token = e.token,
                discount = e.discount,
                discountpercentage = e.discountpercentage,
                title = e.printtitle
            });

            if (reportformats[0].reportformat.format.StartsWith(@"<Config>"))
            {
                var additional = new SortedDictionary<string, object>();
                var imageData = new ImageData();
                imageData.Context = new UserClaim()
                {
                    OrganizationID = Context.OrganizationID
                };

                ReportLibrary.DefaultPageSettings setting = new ReportLibrary.DefaultPageSettings();
                ReportLibrary.DotMatrixReporter labreport = new ReportLibrary.DotMatrixReporter(Context.OrganizationID,
                    new ReportLibrary.DotMatrixReporter.PrinterConfig());
                labreport.format = ReportLibrary.DosFormatParser.ParseXML(Context.OrganizationID, reportformats[0].reportformat.format);


                List<string> imageids = labreport.format.GetImages();
                labreport.Additional = new SortedDictionary<string, object>();
                foreach (var imgid in imageids.FindAll(x => x.Length > 6))
                {
                    var file = imageData.GetData(imgid.LongFromBase62());
                    if (file != null && file.data != null)
                    {
                        var img = imageData.FromBytes(file.data);
                        labreport.Additional.Add("IMG-" + imgid, img);
                    }
                }

                if (reportformats[0].reportformat.format.IndexOf("LOGO") > 0)
                {
                    CompanyDataService companyDataService = new CompanyDataService() { Context = this.Context };
                    string logo = companyDataService.Search(new CompanyCreteria() { id = item.companyid }).First().logo;
                    if (!string.IsNullOrEmpty(logo))
                    {
                        var file = imageData.GetData(logo.LongFromBase62());
                        if (file != null && file.data != null)
                        {
                            var img = imageData.FromBytes(file.data);
                            labreport.Additional.Add("LOGO", img);
                        }
                    }
                }

                StringBuilder sb = new StringBuilder();

                foreach (var pitem in walkintokens)
                {
                    labreport.DataSource = item;
                    labreport.DataItems = null;
                    var document = labreport.format.RenderBytes(new ImageData(), pitem, null, additional, true);
                    sb.Append(Convert.ToBase64String(document));
                    sb.Append(";");
                }
                item.printdocument = sb.ToString();
            }
            else
            {
                InfyPOS.Report.CitizenBarcode barcodeprinter = new Report.CitizenBarcode("",
                reportformats[0].reportformat.format, reportformats[0].reportformat.mode, reportformats[0].nooflines);
                StringBuilder sb = new StringBuilder();

                var barcodeList = walkintokens.ConvertAll(e => (Report.IBarcodeFormat)e);
                var response = barcodeprinter.Render(barcodeList);
                foreach (var pitem in response)
                {
                    var bytes = Encoding.ASCII.GetBytes(pitem);
                    sb.Append(Convert.ToBase64String(bytes));
                    sb.Append(";");
                }
                item.printdocument = sb.ToString();
            }

            return item;
        }

        internal void UpdateReferral(Billsettlementmaster oBillsettlementmaster, NpgsqlConnection conn, NpgsqlTransaction transaction)
        {
            var referrals = Search(new StoretransactionCreteria()
            {
                locationid = oBillsettlementmaster.locationid,
                actiondate = DateTimeNow.Date,
                type = "Referral"
            });
            if (referrals.Count == 1 &&
                referrals[0].ReferralTokenAttribute != null &&
                referrals[0].ReferralTokenAttribute.printhistory.Exists(e => e.token == oBillsettlementmaster.paymentinfo.referral))
            {
                var token = referrals[0].ReferralTokenAttribute.printhistory.Find(e => e.token == oBillsettlementmaster.paymentinfo.referral);
                token.agentid = oBillsettlementmaster.paymentinfo.referralagentid;
                token.employeeid = oBillsettlementmaster.paymentinfo.referralemployeeid;
                token.settlementid = oBillsettlementmaster.id;
                Update(transaction, conn, referrals[0]);
            }
        }

        internal StoretransactionCreteria CreateToken(StoretransactionCreteria item)
        {
            if (item.locationid == 0)
                throw new Exception("Please select Location");

            CustomerDataService customerDataService = new CustomerDataService() { Context = this.Context };
            if (!string.IsNullOrEmpty(item.customermobileno))
            {
                var customer = customerDataService.Search(new CustomerCreteria()
                {
                    mobileno = item.customermobileno
                });
                if (customer.Count > 0)
                {
                    item.customerid = customer.First().id;
                }
                else if (!string.IsNullOrEmpty(item.customername))
                {
                    item.customerid = customerDataService.Insert(new Customer()
                    {
                        mobileno = item.customermobileno,
                        name = item.customername,
                        registredlocationid = item.locationid,
                        customertype = 4,
                        isactive = true,
                        createdby = Context.UserID,
                        modifiedby = Context.UserID,
                        createdon = DateTimeNow,
                        modifiedon = DateTimeNow,
                        version = 1
                    }).id;
                }
            }

            var searchlist = Search(new StoretransactionCreteria()
            { actiondate = item.actiondate, locationid = item.locationid, type = "WalkIn" });

            item.tokennumbers = "1";
            item.type = "WalkIn";
            if (searchlist.Count > 0)
                item.id = searchlist.First().id;

            return PrintToken(item);
        }
        internal StoretransactionCreteria PrintToken(StoretransactionCreteria item)
        {
            InfyPOS.Data.ConfigurationDataService config = new ConfigurationDataService() { Context = this.Context };
            var reportformats = config.Search(new ConfigurationCreteria
            {
                key = "WITOKEN"
            });

            if (reportformats.Count == 0)
                throw new Exception("Please configure Walk In Token");

            int from = 0;
            int to = 0;

            if (item.id == 0)
            {
                item.id = Insert(new Storetransaction()
                {
                    locationid = item.locationid,
                    actiondate = item.actiondate,
                    type = item.type,
                    isactive = true,
                    createdby = Context.UserID,
                    modifiedby = Context.UserID,
                    createdon = DateTimeNow,
                    modifiedon = DateTimeNow,
                    version = 1,
                    WalkinTokenAttribute = new Storetransaction.WalkinToken()
                    {
                        startno = 1,
                        endno = 0
                    }
                }).id;
            }

            var walkin = Search(new StoretransactionCreteria()
            {
                id = item.id
            }).First();

            var fromtotokens = item.tokennumbers.Split('-');
            if (fromtotokens.Length == 1)
            {
                from = walkin.WalkinTokenAttribute.endno + 1;
                to = walkin.WalkinTokenAttribute.endno + int.Parse(fromtotokens[0]);
            }
            else
            {
                from = int.Parse(fromtotokens[0]);
                to = int.Parse(fromtotokens[1]);
            }

            if (walkin.WalkinTokenAttribute.endno < to)
            {
                if (walkin.WalkinTokenAttribute.printhistory == null)
                    walkin.WalkinTokenAttribute.printhistory = new List<Storetransaction.WalkinToken.PrintCount>();
                walkin.WalkinTokenAttribute.printhistory.Add(new Storetransaction.WalkinToken.PrintCount()
                {
                    at = DateTimeNow,
                    from = walkin.WalkinTokenAttribute.endno + 1,
                    to = to
                });
                walkin.WalkinTokenAttribute.endno = to;

                if (item.customerid > 0 && walkin.walkincustomerinfo == null)
                    walkin.walkincustomerinfo = new List<Storetransaction.WalkinCustomerDetails>();
                if (item.customerid > 0)
                {
                    walkin.walkincustomerinfo.Add(new Storetransaction.WalkinCustomerDetails()
                    {
                        customerid = item.customerid,
                        mobileno = item.customermobileno,
                        at = DateTimeNow,
                        token = to.ToString()
                    });
                }
                Update(walkin);
            }

            item.modifiedon = walkin.modifiedon;
            item.modifiedby = walkin.modifiedby;
            item.createdon = walkin.createdon;
            item.createdby = walkin.createdby;
            item.isactive = walkin.isactive;
            item.version = walkin.version;

            var walkintokens = Enumerable.Range(from, to - (from - 1)).ToList().ConvertAll(e => new WalkInToken()
            {
                token = e.ToString("N0"),
                ptoken = "$" + e.ToString("N0") + "$"
            });

            if (reportformats[0].reportformat.format.StartsWith(@"<Config>"))
            {
                var additional = new SortedDictionary<string, object>();
                var imageData = new ImageData();
                imageData.Context = new UserClaim()
                {
                    OrganizationID = Context.OrganizationID
                };

                ReportLibrary.DefaultPageSettings setting = new ReportLibrary.DefaultPageSettings();
                ReportLibrary.DotMatrixReporter labreport = new ReportLibrary.DotMatrixReporter(Context.OrganizationID,
                    new ReportLibrary.DotMatrixReporter.PrinterConfig());
                labreport.format = ReportLibrary.DosFormatParser.ParseXML(Context.OrganizationID, reportformats[0].reportformat.format);


                List<string> imageids = labreport.format.GetImages();
                labreport.Additional = new SortedDictionary<string, object>();
                foreach (var imgid in imageids.FindAll(x => x.Length > 6))
                {
                    var file = imageData.GetData(imgid.LongFromBase62());
                    if (file != null && file.data != null)
                    {
                        var img = imageData.FromBytes(file.data);
                        labreport.Additional.Add("IMG-" + imgid, img);
                    }
                }

                if (reportformats[0].reportformat.format.IndexOf("LOGO") > 0)
                {
                    CompanyDataService companyDataService = new CompanyDataService() { Context = this.Context };
                    string logo = companyDataService.Search(new CompanyCreteria() { id = item.companyid }).First().logo;
                    if (!string.IsNullOrEmpty(logo))
                    {
                        var file = imageData.GetData(logo.LongFromBase62());
                        if (file != null && file.data != null)
                        {
                            var img = imageData.FromBytes(file.data);
                            labreport.Additional.Add("LOGO", img);
                        }
                    }
                }

                StringBuilder sb = new StringBuilder();

                foreach (var pitem in walkintokens)
                {
                    labreport.DataSource = item;
                    labreport.DataItems = null;
                    var document = labreport.format.RenderBytes(new ImageData(), pitem, null, additional, true);
                    sb.Append(Convert.ToBase64String(document));
                    sb.Append(";");
                }
                item.printdocument = sb.ToString();
            }
            else
            {
                InfyPOS.Report.CitizenBarcode barcodeprinter = new Report.CitizenBarcode("",
                reportformats[0].reportformat.format, reportformats[0].reportformat.mode, reportformats[0].nooflines);
                StringBuilder sb = new StringBuilder();

                var barcodeList = walkintokens.ConvertAll(e => (Report.IBarcodeFormat)e);
                var response = barcodeprinter.Render(barcodeList);
                foreach (var pitem in response)
                {
                    var bytes = Encoding.ASCII.GetBytes(pitem);
                    sb.Append(Convert.ToBase64String(bytes));
                    sb.Append(";");
                }
                item.printdocument = sb.ToString();
            }

            return item;
        }

        internal void UpdateWalkinToken(Customerwalkins result)
        {
            var walkins = Search(new StoretransactionCreteria()
            {
                type = "WalkIn",
                locationid = result.locationid,
                actiondate = result.fromtime.Date
            });
            if (walkins.Count > 0)
            {
                var walkin = walkins.OrderByDescending(e => e.actiondate).First();
                if (walkin.WalkinTokenAttribute.issuehistory == null)
                    walkin.WalkinTokenAttribute.issuehistory = new List<Storetransaction.WalkinToken.WalkinCount>();

                if (result.walkintoken.StartsWith("$") &&
                    result.walkintoken.EndsWith("$") &&
                    walkin.WalkinTokenAttribute.issuehistory.Count > 0)
                {
                    var tokenno = result.walkintoken.Replace("$", "");
                    int tn = 0;
                    if (int.TryParse(tokenno, out tn))
                    {
                        if (walkin.WalkinTokenAttribute.issuehistory.Exists(e => e.to > tn))
                        {
                            result.walkincount = 0;
                        }
                        else
                        {
                            result.walkincount = tn - walkin.WalkinTokenAttribute.issuehistory.Max(e => e.to);
                        }
                    }
                }

                if (result.walkincount > 0)
                {
                    walkin.WalkinTokenAttribute.issuehistory.Add(new Storetransaction.WalkinToken.WalkinCount()
                    {
                        at = result.fromtime,
                        count = result.walkincount,
                        key = result.walkintoken
                    });
                    int from = 1;
                    foreach (var item in walkin.WalkinTokenAttribute.issuehistory.OrderBy(e => e.at))
                    {
                        item.from = from;
                        item.to = from + item.count - 1;
                        from = item.to + 1;
                    }
                    walkin.WalkinTokenAttribute.lastissuedno = walkin.WalkinTokenAttribute.issuehistory.Sum(e => e.count);
                    Update(walkin);
                }
            }
        }

        public class WalkInToken : Report.IBarcodeFormat
        {
            public bool SpecialField(string field, SortedDictionary<string, Object> data)
            {
                return true;
            }
            public SortedDictionary<string, object> KeyValue
            {
                get
                {
                    SortedDictionary<string, object> ret = new SortedDictionary<string, object>();
                    ret.Add("TOKEN", token);
                    ret.Add("PTOKEN", ptoken);
                    ret.Add("DISCOUNT", discount.ToString("N2"));
                    ret.Add("DISCOUNTPERCENTAGE", discount.ToString("N2"));
                    ret.Add("TITLE", title);
                    return ret;
                }
            }

            public SortedDictionary<string, string> KeyFormat
            {
                get
                {
                    SortedDictionary<string, string> ret = new SortedDictionary<string, string>();
                    return ret;
                }
            }
            public string token { get; set; }
            public string ptoken { get; set; }

            public decimal discount { get; set; }
            public decimal discountpercentage { get; set; }
            public string title { get; set; }
            public bool hasdiscountpercentage
            {
                get
                {
                    return discountpercentage > 0;
                }
            }
            public bool hasdiscount
            {
                get
                {
                    return discount > 0;
                }
            }
            public bool hastitle
            {
                get
                {
                    return !string.IsNullOrEmpty(title);
                }
            }
        }

        public List<Storetransaction> Search(StoretransactionCreteria _storetransaction)
        {
            NpgsqlConnection conn = CreateConnection();
            try
            {
                conn.Open();
                var lst = Search(_storetransaction, conn, null);
                if (_storetransaction.type == "WalkIn")
                {
                    lst.ForEach(e =>
                    {
                        if (e.WalkinTokenAttribute != null &&
                            e.WalkinTokenAttribute.issuehistory != null)
                        {
                            e.WalkinTokenAttribute.issuehistory = e.WalkinTokenAttribute.issuehistory.OrderBy(ex => ex.at).ToList();
                        }
                    });
                }
                conn.Close();
                return lst;
            }
            catch (Exception exp)
            {
                if (conn != null) conn.Close();
                WebAPI.Common.Logger.Instance(Context.OrganizationID).Log(exp, WebAPI.Common.Logger.LogType.Error, _storetransaction);
            }
            return null;
        }

        public Storetransaction Insert(Storetransaction oStoretransaction)
        {

            NpgsqlTransaction transaction = null;
            try
            {
                using (NpgsqlConnection conn = CreateConnection())
                {
                    conn.Open();
                    transaction = conn.BeginTransaction();

                    if (oStoretransaction.type == "SaleReturn")
                    {
                        Delete(transaction, conn, oStoretransaction.actiondate, oStoretransaction.type);
                    }

                    Insert(transaction, conn, oStoretransaction);

                    if (oStoretransaction.type == "StoreTime")
                    {
                        EmployeeinoutDataService employeeinoutDataService = new EmployeeinoutDataService() { Context = this.Context };
                        employeeinoutDataService.StoreOpening(oStoretransaction, conn, transaction);
                    }

                    transaction.Commit();
                    conn.Close();
                }
            }
            catch (Exception exp)
            {
                WebAPI.Common.Logger.Instance(Context.OrganizationID).Log(exp, WebAPI.Common.Logger.LogType.Error, oStoretransaction);
                throw exp;
            }

            return oStoretransaction;
        }

        public Storetransaction Insert(NpgsqlTransaction transaction, NpgsqlConnection conn, Storetransaction oStoretransaction)
        {

            NpgsqlCommand command = new NpgsqlCommand(SQL_Insert, conn, transaction);
            command.Parameters.Add("companyid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oStoretransaction.companyid;
            command.Parameters.Add("locationid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oStoretransaction.locationid;
            command.Parameters.Add("actiondate", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = oStoretransaction.actiondate.Date;
            command.Parameters.Add("type", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oStoretransaction.type == null ? "" : oStoretransaction.type;
            command.Parameters.Add("attributes", NpgsqlTypes.NpgsqlDbType.Json).Value = oStoretransaction.attributes_json == null ? "null" : oStoretransaction.attributes_json;
            command.Parameters.Add("createdby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = Context.UserID;
            command.Parameters.Add("createdon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = DateTimeNow;
            command.Parameters.Add("version", NpgsqlTypes.NpgsqlDbType.Integer).Value = oStoretransaction.version;
            command.Parameters.Add("isactive", NpgsqlTypes.NpgsqlDbType.Boolean).Value = oStoretransaction.isactive;
            command.Parameters.Add("modifiedby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = Context.UserID;
            command.Parameters.Add("modifiedon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = DateTimeNow;
            command.Parameters.Add("walkincustomerinfo", NpgsqlTypes.NpgsqlDbType.Json).Value = oStoretransaction.walkincustomerinfo_json == null ? "null" : oStoretransaction.walkincustomerinfo_json;
            using (System.Data.IDataReader reader = command.ExecuteReader())
            {
                if (reader.Read())
                {
                    oStoretransaction.id = Convert.ToInt64(reader[0].ToString());
                }
            }
            return oStoretransaction;
        }

        public Storetransaction Update(Storetransaction oStoretransaction)
        {

            NpgsqlTransaction transaction = null;
            try
            {
                using (NpgsqlConnection conn = CreateConnection())
                {
                    conn.Open();
                    transaction = conn.BeginTransaction();
                    Update(transaction, conn, oStoretransaction);

                    if (oStoretransaction.type == "StoreTime")
                    {
                        EmployeeinoutDataService employeeinoutDataService = new EmployeeinoutDataService() { Context = this.Context };
                        employeeinoutDataService.StoreOpening(oStoretransaction, conn, transaction);
                    }
                    transaction.Commit();
                    conn.Close();
                }
            }
            catch (Exception exp)
            {
                WebAPI.Common.Logger.Instance(Context.OrganizationID).Log(exp, WebAPI.Common.Logger.LogType.Error, oStoretransaction);
                throw exp;
            }

            return oStoretransaction;
        }

        public Storetransaction Update(NpgsqlTransaction transaction, NpgsqlConnection conn, Storetransaction oStoretransaction)
        {

            NpgsqlCommand command = new NpgsqlCommand(SQL_Update, conn, transaction);
            command.Parameters.Add("companyid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oStoretransaction.companyid;
            command.Parameters.Add("locationid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oStoretransaction.locationid;
            command.Parameters.Add("actiondate", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = oStoretransaction.actiondate.Date;
            command.Parameters.Add("type", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oStoretransaction.type == null ? "" : oStoretransaction.type;
            command.Parameters.Add("attributes", NpgsqlTypes.NpgsqlDbType.Json).Value = oStoretransaction.attributes_json == null ? "null" : oStoretransaction.attributes_json;
            command.Parameters.Add("createdby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oStoretransaction.createdby;
            command.Parameters.Add("id", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oStoretransaction.id;
            command.Parameters.Add("createdon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = oStoretransaction.createdon;
            command.Parameters.Add("version", NpgsqlTypes.NpgsqlDbType.Integer).Value = oStoretransaction.version;
            command.Parameters.Add("isactive", NpgsqlTypes.NpgsqlDbType.Boolean).Value = oStoretransaction.isactive;
            command.Parameters.Add("modifiedby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = Context.UserID;
            command.Parameters.Add("modifiedon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = DateTimeNow;
            command.Parameters.Add("walkincustomerinfo", NpgsqlTypes.NpgsqlDbType.Json).Value = oStoretransaction.walkincustomerinfo_json == null ? "null" : oStoretransaction.walkincustomerinfo_json;
            using (System.Data.IDataReader reader = command.ExecuteReader())
            {
                if (reader.Read())
                {
                    oStoretransaction.version = Convert.ToInt32(reader[0].ToString());
                }
            }

            return oStoretransaction;
        }

        public bool Delete(Storetransaction oStoretransaction)
        {

            NpgsqlTransaction transaction = null;
            bool hasSuccessCount = false;
            try
            {
                using (NpgsqlConnection conn = CreateConnection())
                {
                    conn.Open();
                    transaction = conn.BeginTransaction();
                    hasSuccessCount = Delete(transaction, conn, oStoretransaction);
                    transaction.Commit();
                    conn.Close();
                }
            }
            catch (Exception exp)
            {
                WebAPI.Common.Logger.Instance(Context.OrganizationID).Log(exp, WebAPI.Common.Logger.LogType.Error, oStoretransaction);

                throw exp;
            }
            return hasSuccessCount;

        }

        public bool Delete(NpgsqlTransaction transaction, NpgsqlConnection conn, DateTime date, string type)
        {

            NpgsqlCommand command = new NpgsqlCommand(SQL_DeleteType, conn, transaction);
            command.Parameters.Add("actiondate", NpgsqlTypes.NpgsqlDbType.Date).Value = date;
            command.Parameters.Add("type", NpgsqlTypes.NpgsqlDbType.Varchar).Value = type;
            int successCount = command.ExecuteNonQuery();

            return successCount > 0;
        }

        public bool Delete(NpgsqlTransaction transaction, NpgsqlConnection conn, Storetransaction oStoretransaction)
        {

            NpgsqlCommand command = new NpgsqlCommand(SQL_Delete, conn, transaction);
            command.Parameters.Add("isactive", NpgsqlTypes.NpgsqlDbType.Boolean).Value = oStoretransaction.isactive;
            command.Parameters.Add("modifiedby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oStoretransaction.modifiedby;
            command.Parameters.Add("modifiedon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = oStoretransaction.modifiedon;
            command.Parameters.Add("version", NpgsqlTypes.NpgsqlDbType.Integer).Value = oStoretransaction.version;
            command.Parameters.Add("id", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oStoretransaction.id;
            int successCount = command.ExecuteNonQuery();

            return successCount > 0;
        }

        internal Storetransaction.StoreReturnBarcodes StoreReturnBarcode(Storetransaction item)
        {
            if (item.locationid == 0) throw new Exception("Please select location.");
            item.isactive = true;
            item.version = 1;
            return Insert(item).SalesReturnAtribute;
        }

        internal Storetransaction SearchOutOfStock(Storetransaction oStoretransaction)
        {
            long id = 0;
            var query = @"select id,actiondate from storetransaction where type='OutOfStock' 
                                and isactive=true and locationid=@locationid and companyid=@companyid
                                order by actiondate desc
                                limit 1";

            var param = new SortedList<string, object>();
            param.Add("companyid", oStoretransaction.companyid);
            param.Add("locationid", oStoretransaction.locationid);
            using (var reader = ExecuteReader(query, param))
            {
                if (reader.Read())
                {
                    id = reader.GetInt64(0);
                }
            }

            if (id > 0)
            {
                var response = Search(new StoretransactionCreteria()
                {
                    id = id
                }).First();
                CacheManager.Instance(Context.OrganizationID).SetOutOfStockProducts(response);
                return response;
            }
            return new Storetransaction() { companyid = oStoretransaction.companyid, locationid = oStoretransaction.locationid, OutOfStocks = new List<long>() };
        }

        internal Storetransaction SaveOutOfStock(Storetransaction oStoretransaction)
        {

            NpgsqlTransaction transaction = null;
            try
            {
                using (NpgsqlConnection conn = CreateConnection())
                {
                    conn.Open();
                    transaction = conn.BeginTransaction();
                    if (oStoretransaction.id > 0)
                    {
                        var last = Search(new StoretransactionCreteria() { id = oStoretransaction.id }).First();
                        if (last.actiondate.Date == oStoretransaction.actiondate.Date)
                        {
                            if(last.version != oStoretransaction.version)
                            {
                                throw new Exception("Other user changed the out of stock, please refresh and do it again");
                            }
                            Update(transaction, conn, oStoretransaction);
                        }
                        else
                        {
                            Insert(transaction, conn, oStoretransaction);
                        }
                    }
                    else
                    {
                        Insert(transaction, conn, oStoretransaction);
                    }
                    transaction.Commit();
                    conn.Close();
                }
            }
            catch (Exception exp)
            {
                WebAPI.Common.Logger.Instance(Context.OrganizationID).Log(exp, WebAPI.Common.Logger.LogType.Error, oStoretransaction);
                throw exp;
            }
            return oStoretransaction;
        }
    }

}
