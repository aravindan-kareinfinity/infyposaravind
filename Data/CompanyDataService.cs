using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using InfyPOS.Models;
using InfyPOS.Common;
using Npgsql;
using System.Security.Cryptography;
using WebAPI.Data;
using Newtonsoft.Json;
using System.Net;
using System.Text;

namespace InfyPOS.Data

{
    public class CompanyDataService : BaseData
    {
        const string SQL_Search = @"select company.id, company.name, contactperson,billingname, contactno, address, cityid, stateid, countryid, pincode, tinanddate,gstusername,gstaccesskey, cstanddate, 
areacode, tan, pan, bankaccountname, bankaccountno, bankifci, pfno, esino, company.isactive, company.createdby, company.modifiedby,company.version, company.createdon, company.modifiedon, emailid, 
logo, servers,company.code,tallyconfig,website,company_as_supplierid,company_as_customerid,company_as_suppliercode,einvoiceusername,einvoicepwd,company_as_customercode,internalbuyer,internalmargin,
city.name cityname,state.name statename from company 
left outer join referencelist city on city.id = company.cityid
left outer join referencelist state on state.id = company.stateid where company.id=@id";

        const string SQL_Insert = @"INSERT INTO public.company(name, contactperson,billingname, contactno, address, cityid, stateid, countryid, pincode, tinanddate,gstusername,gstaccesskey, cstanddate, areacode, tan, pan, bankaccountname, bankaccountno, bankifci, pfno, esino, isactive, createdby, modifiedby, version, createdon, modifiedon, emailid, logo, servers,code,tallyconfig,website,company_as_supplierid,company_as_customerid,company_as_suppliercode,einvoiceusername,einvoicepwd,company_as_customercode,internalbuyer,internalmargin) 
                                VALUES(@name, @contactperson,@billingname, @contactno, @address, @cityid, @stateid, @countryid, @pincode, @tinanddate,@gstusername,@gstaccesskey, @cstanddate, @areacode, @tan, @pan, @bankaccountname, @bankaccountno, @bankifci, @pfno, @esino, @isactive, @createdby, @modifiedby, @version, @createdon, @modifiedon, @emailid, @logo, @servers,@code,@tallyconfig,@website,@company_as_supplierid,@company_as_customerid,@company_as_suppliercode,@einvoiceusername,@einvoicepwd,@company_as_customercode,@internalbuyer,@internalmargin) RETURNING id";

        public List<PrintServer> SearchPrinters()
        {
            return SearchPrinters(true);
        }
        public List<PrintServer> SearchPrinters(bool includeLocalhost)
        {
            NpgsqlConnection conn = CreateConnection();
            List<PrintServer> lst = new List<PrintServer>();
            try
            {
                conn.Open();
                NpgsqlCommand command = GetCommand(SQL_Search.Substring(0, SQL_Search.IndexOf("where")) + " where company.isactive=true", conn);
                using (System.Data.IDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        PrintServer oprinter = new PrintServer();
                        oprinter.id = reader["id"] == DBNull.Value ? 0 : Convert.ToInt64(reader["id"]);
                        oprinter.name = reader["name"] == DBNull.Value ? "" : reader["name"].ToString();
                        oprinter.servers_json = reader["servers"] == DBNull.Value ? "null" : reader["servers"].ToString();
                        if (oprinter.servers == null)
                            oprinter.servers = new List<Company.Servers>();

                        if (includeLocalhost)
                        {
                            if (!oprinter.servers.Exists(e => e.name.ToLower().Contains("localhost")))
                            {
                                oprinter.servers.Add(new Company.Servers()
                                {
                                    id = 0,
                                    name = "localhost",
                                    type = "printer",
                                    port = "8099"
                                });
                            }

                            if (!oprinter.servers.Exists(e => (e.name != null && e.name.ToLower().Contains("127.0.0.1")) || (e.ip != null && e.ip.ToLower().Contains("127.0.0.1"))))
                            {
                                oprinter.servers.Add(new Company.Servers()
                                {
                                    id = -1,
                                    name = "127.0.0.1",
                                    type = "printer",
                                    port = "8099"
                                });
                            }

                            if (Services.ConfigManager.Instance(Context.OrganizationID).Config.system_webprint)
                            {
                                oprinter.servers.Add(new Company.Servers()
                                {
                                    name = "Web Printer",
                                    type = "Web Printer",
                                    port = "8099",
                                    id = -10
                                });
                            }
                        }
                        lst.Add(oprinter);
                    }
                }
                conn.Close();
            }
            catch (Exception exp)
            {
                if (conn != null) conn.Close();
            }
            return lst;
        }


        internal LicenseInfo CreateClientLicense(PrinterConfig request)
        {
            UsersDataService usersDataService = new UsersDataService() { Context = this.Context };
            if (!usersDataService.MatchCurrentPasscode(request.employeecode, request.passcode))
                return new LicenseInfo()
                {
                    passcode = false
                };

            SecuritykeysDataService securitykeysDataService = new SecuritykeysDataService() { Context = this.Context };
            var result = securitykeysDataService.Search(new SecuritykeysCreteria()
            {
                locationkey = request.licensekey
            });

            if (result.Count == 1)
            {
                if (result[0].disable)
                {
                    return new LicenseInfo()
                    {
                        passcode = true,
                        disabled = true
                    };
                }
                result[0].locationkey = request.licensekey;
                result[0].computername = request.computername;
                result[0].createdon = DateTimeNow;
                result[0].isactive = true;
                result[0].systemrole = request.systemrole;
                result[0].ipaddress = request.ipaddress;
                securitykeysDataService.Update(result[0]);
                return new LicenseInfo()
                {
                    exist = true,
                    passcode = true,
                    publickey = result[0].publickey
                };
            }

            Securitykeys securitykeys = new Securitykeys();
            securitykeys.locationkey = request.licensekey;
            securitykeys.computername = request.computername;
            securitykeys.systemrole = request.systemrole;
            securitykeys.createdon = DateTimeNow;
            securitykeys.isactive = true;
            securitykeys.ipaddress = request.ipaddress;
            var rsa = new RSACryptoServiceProvider();
            securitykeys.privatekey = rsa.ToXmlString(true);
            securitykeys.publickey = rsa.ToXmlString(false);
            securitykeysDataService.Insert(securitykeys);

            return new LicenseInfo()
            {
                created = true,
                passcode = true,
                publickey = securitykeys.publickey
            };
        }



        internal bool AddPrinter(PrinterConfig request)
        {
            var searchlist = Search(new CompanyCreteria()
            {
                code = request.companycode
            });

            if (searchlist.Count == 0)
            {
                searchlist = Search(new CompanyCreteria()
                {

                }).OrderBy(e => e.id).ToList();
            }
            if (searchlist.Count > 0)
            {
                if (searchlist[0].servers == null)
                    searchlist[0].servers = new List<Company.Servers>();
                if (searchlist[0].servers.Exists(e => e.name == request.server))
                {
                    var server = searchlist[0].servers.Find(e => e.name == request.server || e.name == request.computername);
                    if (server.port == request.port && server.ip == request.ipaddress)
                        return false;

                    server.port = request.port;
                    server.ip = request.ipaddress;
                    server.location = request.locationcode;
                }
                else
                {
                    searchlist[0].servers.Add(new Company.Servers()
                    {
                        name = request.server,
                        port = request.port,
                        location = request.locationcode,
                        ip = request.ipaddress,
                        type = "Printer"
                    });
                }
                Update(searchlist[0]);
                return true;
            }
            return false;
        }

        const string SQL_Update = @"UPDATE public.company SET name=@name, contactperson=@contactperson,billingname=@billingname, contactno=@contactno, address=@address, cityid=@cityid, stateid=@stateid, 
                                    countryid=@countryid, pincode=@pincode, tinanddate=@tinanddate,gstusername=@gstusername,gstaccesskey=@gstaccesskey, cstanddate=@cstanddate, areacode=@areacode, tan=@tan, pan=@pan, bankaccountname=@bankaccountname, 
                                    bankaccountno=@bankaccountno, bankifci=@bankifci, pfno=@pfno, esino=@esino, modifiedby=@modifiedby, version=@version+1, modifiedon=@modifiedon, emailid=@emailid, 
                                    tallyconfig = @tallyconfig, logo=@logo, servers=@servers, code=@code,website=@website,internalbuyer=@internalbuyer,internalmargin=@internalmargin,
                                    company_as_supplierid=@company_as_supplierid,company_as_customerid=@company_as_customerid,einvoiceusername=@einvoiceusername,einvoicepwd=@einvoicepwd,
                                    company_as_suppliercode=@company_as_suppliercode,company_as_customercode=@company_as_customercode
                                    where id=@id and version=@version RETURNING version";

        const string SQL_Delete = @"update public.company set isactive=false,version=@version+1,modifiedby=@modifiedby,modifiedon=@modifiedon where id=@id and version=@version";

        public List<Company> Search(CompanyCreteria _company)
        {
            List<Company> lst = new List<Company>();
            NpgsqlConnection conn = CreateConnection();
            try
            {
                conn.Open();
                NpgsqlCommand command = null;
                if (_company.id != 0)
                {
                    command = GetCommand(SQL_Search, conn);
                }
                else
                {
                    QueryBuilder qb = new QueryBuilder();
                    qb.CurrentQuery = SQL_Search.Substring(0, SQL_Search.IndexOf("where"));
                    qb.AddParameter("company.isactive", true, "=");

                    if (_company.onlyseller)
                        qb.AddParameter("company.internalbuyer", false, "=");

                    if (!string.IsNullOrEmpty(_company.name))
                    {
                        if (_company.uniquename)
                            qb.AddParameter("company.name", _company.name, "=");
                        else
                            qb.AddParameter("company.name", "%" + _company.name + "%", " ilike ");
                    }
                    if (!string.IsNullOrEmpty(_company.contactperson))
                    {
                        qb.AddParameter("company.contactperson", "%" + _company.contactperson + "%", " ilike ");
                    }
                    if (!string.IsNullOrEmpty(_company.code))
                        qb.AddParameter("company.code", _company.code, "=");
                    command = qb.GetCommand(conn);
                }
                command.Parameters.Add("id", NpgsqlTypes.NpgsqlDbType.Bigint).Value = _company.id;
                using (System.Data.IDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        Company ocompany = new Company();
                        ocompany.id = reader["id"] == DBNull.Value ? 0 : Convert.ToInt64(reader["id"]);
                        ocompany.name = reader["name"] == DBNull.Value ? "" : reader["name"].ToString();
                        ocompany.contactperson = reader["contactperson"] == DBNull.Value ? "" : reader["contactperson"].ToString();
                        ocompany.contactno = reader["contactno"] == DBNull.Value ? "" : reader["contactno"].ToString();
                        ocompany.billingname = reader["billingname"] == DBNull.Value ? "" : reader["billingname"].ToString();

                        ocompany.einvoiceusername = reader["einvoiceusername"] == DBNull.Value ? "" : reader["einvoiceusername"].ToString();
                        ocompany.einvoicepwd = reader["einvoicepwd"] == DBNull.Value ? "" : reader["einvoicepwd"].ToString();

                        ocompany.company_as_suppliercode = reader["company_as_suppliercode"] == DBNull.Value ? "" : reader["company_as_suppliercode"].ToString();
                        ocompany.company_as_customercode = reader["company_as_customercode"] == DBNull.Value ? "" : reader["company_as_customercode"].ToString();
                        ocompany.company_as_supplierid = reader["company_as_supplierid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["company_as_supplierid"]);
                        ocompany.company_as_customerid = reader["company_as_customerid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["company_as_customerid"]);

                        ocompany.address = reader["address"] == DBNull.Value ? "" : reader["address"].ToString();
                        ocompany.parseaddress();
                        ocompany.cityid = reader["cityid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["cityid"]);
                        ocompany.stateid = reader["stateid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["stateid"]);
                        ocompany.countryid = reader["countryid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["countryid"]);
                        ocompany.pincode = reader["pincode"] == DBNull.Value ? "" : reader["pincode"].ToString();
                        ocompany.tinanddate = reader["tinanddate"] == DBNull.Value ? "" : reader["tinanddate"].ToString();
                        ocompany.gstusername = reader["gstusername"] == DBNull.Value ? "" : reader["gstusername"].ToString();
                        ocompany.gstaccesskey = reader["gstaccesskey"] == DBNull.Value ? "" : reader["gstaccesskey"].ToString();
                        ocompany.cstanddate = reader["cstanddate"] == DBNull.Value ? "" : reader["cstanddate"].ToString();
                        ocompany.areacode = reader["areacode"] == DBNull.Value ? "" : reader["areacode"].ToString();
                        ocompany.tan = reader["tan"] == DBNull.Value ? "" : reader["tan"].ToString();
                        ocompany.pan = reader["pan"] == DBNull.Value ? "" : reader["pan"].ToString();
                        ocompany.bankaccountname = reader["bankaccountname"] == DBNull.Value ? "" : reader["bankaccountname"].ToString();
                        ocompany.bankaccountno = reader["bankaccountno"] == DBNull.Value ? "" : reader["bankaccountno"].ToString();
                        ocompany.bankifci = reader["bankifci"] == DBNull.Value ? "" : reader["bankifci"].ToString();
                        ocompany.pfno = reader["pfno"] == DBNull.Value ? "" : reader["pfno"].ToString();
                        ocompany.esino = reader["esino"] == DBNull.Value ? "" : reader["esino"].ToString();
                        ocompany.isactive = reader["isactive"] == DBNull.Value ? false : Convert.ToBoolean(reader["isactive"]);
                        ocompany.createdby = reader["createdby"] == DBNull.Value ? 0 : Convert.ToInt64(reader["createdby"]);
                        ocompany.modifiedby = reader["modifiedby"] == DBNull.Value ? 0 : Convert.ToInt64(reader["modifiedby"]);
                        ocompany.version = reader["version"] == DBNull.Value ? 0 : Convert.ToInt32(reader["version"]);
                        ocompany.createdon = reader["createdon"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(reader["createdon"]);
                        ocompany.modifiedon = reader["modifiedon"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(reader["modifiedon"]);
                        ocompany.emailid = reader["emailid"] == DBNull.Value ? "" : reader["emailid"].ToString();
                        ocompany.code = reader["code"] == DBNull.Value ? "" : reader["code"].ToString();
                        ocompany.logo = reader["logo"] == DBNull.Value ? "" : reader["logo"].ToString();
                        ocompany.servers_json = reader["servers"] == DBNull.Value ? "null" : reader["servers"].ToString();
                        ocompany.tallyconfig_json = reader["tallyconfig"] == DBNull.Value ? "null" : reader["tallyconfig"].ToString();
                        ocompany.website = reader["website"] == DBNull.Value ? "" : reader["website"].ToString();
                        ocompany.cityname = reader["cityname"] == DBNull.Value ? "" : reader["cityname"].ToString();
                        ocompany.statename = reader["statename"] == DBNull.Value ? "" : reader["statename"].ToString();

                        ocompany.internalmargin = reader["internalmargin"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["internalmargin"]);
                        ocompany.internalbuyer = reader["internalbuyer"] == DBNull.Value ? false : Convert.ToBoolean(reader["internalbuyer"]);
                        lst.Add(ocompany);
                    }
                }
                conn.Close();
            }
            catch (Exception exp)
            {
                if (conn != null) conn.Close();
                WebAPI.Common.Logger.Instance(Context.OrganizationID).Log(exp, WebAPI.Common.Logger.LogType.Error, _company);
            }
            return lst;
        }

        public Company Insert(Company oCompany)
        {
            if (!CanRunONSAAS())
            {
                throw new Exception("SAAS Application can't run without organization information");
            }
            NpgsqlTransaction transaction = null;
            try
            {
                using (NpgsqlConnection conn = CreateConnection())
                {
                    conn.Open();
                    transaction = conn.BeginTransaction();
                    NpgsqlCommand command = GetCommand(SQL_Insert, conn, transaction);

                    Insert(transaction, conn, oCompany);
                    transaction.Commit();
                    conn.Close();
                }
            }
            catch (Exception exp)
            {
                WebAPI.Common.Logger.Instance(Context.OrganizationID).Log(exp, WebAPI.Common.Logger.LogType.Error, exp.StackTrace);

            }

            return oCompany;
        }

        public Company Insert(NpgsqlTransaction transaction, NpgsqlConnection conn, Company oCompany)
        {
            NpgsqlCommand command = GetCommand(SQL_Insert, conn, transaction);
            command.Parameters.Add("name", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oCompany.name == null ? "" : oCompany.name;
            command.Parameters.Add("contactperson", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oCompany.contactperson == null ? "" : oCompany.contactperson;
            command.Parameters.Add("billingname", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oCompany.billingname == null ? "" : oCompany.billingname;
            

            command.Parameters.Add("contactno", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oCompany.contactno == null ? "" : oCompany.contactno;

            command.Parameters.Add("einvoicepwd", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oCompany.einvoicepwd == null ? "" : oCompany.einvoicepwd;
            command.Parameters.Add("einvoiceusername", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oCompany.einvoiceusername == null ? "" : oCompany.einvoiceusername;

            command.Parameters.Add("company_as_suppliercode", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oCompany.company_as_suppliercode == null ? "" : oCompany.company_as_suppliercode;
            command.Parameters.Add("company_as_customercode", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oCompany.company_as_customercode == null ? "" : oCompany.company_as_customercode;
            command.Parameters.Add("company_as_customerid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oCompany.company_as_customerid;
            command.Parameters.Add("company_as_supplierid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oCompany.company_as_supplierid;
            command.Parameters.Add("address", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oCompany.address == null ? "" : oCompany.address;
            command.Parameters.Add("cityid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oCompany.cityid;
            command.Parameters.Add("stateid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oCompany.stateid;
            command.Parameters.Add("countryid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oCompany.countryid;
            command.Parameters.Add("pincode", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oCompany.pincode == null ? "" : oCompany.pincode;
            command.Parameters.Add("tinanddate", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oCompany.tinanddate == null ? "" : oCompany.tinanddate;
            command.Parameters.Add("gstusername", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oCompany.gstusername == null ? "" : oCompany.gstusername;
            command.Parameters.Add("gstaccesskey", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oCompany.gstaccesskey == null ? "" : oCompany.gstaccesskey;
            command.Parameters.Add("cstanddate", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oCompany.cstanddate == null ? "" : oCompany.cstanddate;
            command.Parameters.Add("areacode", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oCompany.areacode == null ? "" : oCompany.areacode;
            command.Parameters.Add("tan", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oCompany.tan == null ? "" : oCompany.tan;
            command.Parameters.Add("pan", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oCompany.pan == null ? "" : oCompany.pan;
            command.Parameters.Add("bankaccountname", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oCompany.bankaccountname == null ? "" : oCompany.bankaccountname;
            command.Parameters.Add("bankaccountno", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oCompany.bankaccountno == null ? "" : oCompany.bankaccountno;
            command.Parameters.Add("bankifci", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oCompany.bankifci == null ? "" : oCompany.bankifci;
            command.Parameters.Add("pfno", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oCompany.pfno == null ? "" : oCompany.pfno;
            command.Parameters.Add("esino", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oCompany.esino == null ? "" : oCompany.esino;
            command.Parameters.Add("isactive", NpgsqlTypes.NpgsqlDbType.Boolean).Value = oCompany.isactive;
            command.Parameters.Add("createdby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = Context.UserID;
            command.Parameters.Add("modifiedby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = Context.UserID;
            command.Parameters.Add("version", NpgsqlTypes.NpgsqlDbType.Integer).Value = oCompany.version;
            command.Parameters.Add("createdon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = DateTimeNow;
            command.Parameters.Add("modifiedon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = DateTimeNow;
            command.Parameters.Add("emailid", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oCompany.emailid == null ? "" : oCompany.emailid;
            command.Parameters.Add("logo", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oCompany.logo == null ? "" : oCompany.logo;
            command.Parameters.Add("servers", NpgsqlTypes.NpgsqlDbType.Json).Value = oCompany.servers_json == null ? "null" : oCompany.servers_json;
            command.Parameters.Add("tallyconfig", NpgsqlTypes.NpgsqlDbType.Json).Value = oCompany.tallyconfig_json == null ? "null" : oCompany.tallyconfig_json;
            command.Parameters.Add("code", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oCompany.code == null ? "" : oCompany.code;
            command.Parameters.Add("website", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oCompany.website == null ? "" : oCompany.website;

            command.Parameters.Add("internalmargin", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oCompany.internalmargin;
            command.Parameters.Add("internalbuyer", NpgsqlTypes.NpgsqlDbType.Boolean).Value = oCompany.internalbuyer;

            using (System.Data.IDataReader reader = command.ExecuteReader())
            {
                if (reader.Read())
                {
                    oCompany.id = Convert.ToInt64(reader[0].ToString());
                }
            }
            return oCompany;
        }

        public List<GST_Exclude> SearchGSTExclude(DateTime fromdate, DateTime todate, List<long> ids)
        {
            List<GST_Exclude> response = new List<GST_Exclude>();
            var param = new SortedList<string, object>();
            param.Add("fromdate", fromdate);
            param.Add("todate", todate);
            string query = "select gstno,invoiceno,invoicedate,lrinvoiceid,gstinboxid,reason,tradename " +
                "from gst_exlude where (invoicedate>=@fromdate and invoicedate<@todate) ";

            if (ids.Count > 0)
                query += " or gstinboxid in (" + string.Join(",", ids) + ")";

            using (var reader = ExecuteReader(query, param))
            {
                while (reader.Read())
                {
                    response.Add(new GST_Exclude()
                    {
                        gstinboxid = Convert.ToInt64(reader["gstinboxid"]),
                        lrinvoiceid = Convert.ToInt64(reader["lrinvoiceid"]),
                        gstno = Convert.ToString(reader["gstno"]),
                        invoiceno = Convert.ToString(reader["invoiceno"]),
                        invoicedate = Convert.ToDateTime(reader["invoicedate"]),
                        reason = Convert.ToString(reader["reason"])
                    });
                }
            }
            return response;
        }
        public GST_Exclude InsertExclude(NpgsqlConnection conn, GST_Exclude gstinfo)
        {
            NpgsqlCommand command = GetCommand(@"INSERT INTO public.gst_exlude(gstno,invoiceno,invoicedate,lrinvoiceid,gstinboxid,reason,isreserve) 
                                                VALUES(@gstno,@invoiceno,@invoicedate,@lrinvoiceid,@gstinboxid,@reason,@isreserve)", conn);
            command.Parameters.Add("invoiceno", NpgsqlTypes.NpgsqlDbType.Varchar).Value = gstinfo.invoiceno == null ? "" : gstinfo.invoiceno;
            command.Parameters.Add("gstno", NpgsqlTypes.NpgsqlDbType.Varchar).Value = gstinfo.gstno == null ? "" : gstinfo.gstno;
            command.Parameters.Add("reason", NpgsqlTypes.NpgsqlDbType.Varchar).Value = gstinfo.reason == null ? "" : gstinfo.reason;
            command.Parameters.Add("invoicedate", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = gstinfo.invoicedate;
            command.Parameters.Add("gstinboxid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = gstinfo.gstinboxid;
            command.Parameters.Add("lrinvoiceid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = gstinfo.lrinvoiceid;
            command.Parameters.Add("isreserve", NpgsqlTypes.NpgsqlDbType.Boolean).Value = gstinfo.isreserve;
            command.ExecuteNonQuery();
            return gstinfo;
        }
        public GST_Info InsertGSTN(NpgsqlConnection conn, GST_Info gstinfo)
        {
            NpgsqlCommand command = GetCommand(@"INSERT INTO public.gst_info(gstno, name, data) VALUES(@gstno, @name, @data)", conn);
            command.Parameters.Add("name", NpgsqlTypes.NpgsqlDbType.Varchar).Value = gstinfo.name == null ? "" : gstinfo.name;
            command.Parameters.Add("gstno", NpgsqlTypes.NpgsqlDbType.Varchar).Value = gstinfo.gstno == null ? "" : gstinfo.gstno;
            command.Parameters.Add("data", NpgsqlTypes.NpgsqlDbType.Json).Value = gstinfo.data_json;
            command.ExecuteNonQuery();
            return gstinfo;
        }

        public List<GST_Info> SearchGSTFilingInfo(DateTime after,
            List<string> gstno,
            bool fetch)
        {
            List<GST_Info> list = new List<GST_Info>();
            gstno.RemoveAll(x => string.IsNullOrEmpty(x));

            using (var reader = ExecuteReader(@"select tinanddate gstno,filedmonth,filedon,id from suppliercompany where tinanddate in (" +
                string.Join(",", gstno.ConvertAll(x => "'" + WebAPI.Data.Utility.SafeSQL(x) + "'")) + ")", null))
            {
                while (reader.Read())
                {
                    list.Add(new GST_Info()
                    {
                        gstno = reader.GetString(0),
                        filedmonth = reader.IsDBNull(1) ? "" : reader.GetString(1),
                        filedon = reader.IsDBNull(2) ? DateTime.MinValue : reader.GetDateTime(2),
                        supplierid = reader.GetInt64(3)
                    });
                }
            }
            gstno.RemoveAll(x => list.Exists(y => y.gstno == x));

            if (gstno.Count > 0)
            {
                using (var reader = ExecuteReader(@"select gstno,filedmonth,filedon from gst_info where gstno in (" +
                    string.Join(",", gstno.ConvertAll(x => "'" + WebAPI.Data.Utility.SafeSQL(x) + "'")) + ")", null))
                {
                    while (reader.Read())
                    {
                        list.Add(new GST_Info()
                        {
                            gstno = reader.GetString(0),
                            filedmonth = reader.IsDBNull(1) ? "" : reader.GetString(1),
                            filedon = reader.IsDBNull(2) ? DateTime.MinValue : reader.GetDateTime(2)
                        });
                    }
                }
            }

            if (fetch)
            {
                var gstfilingurl = @"https://services.gst.gov.in/services/api/search/taxpayerReturnDetails";
                byte[] resByte;
                string resString;
                byte[] reqString;
                Dictionary<string, object> keyValues = new Dictionary<string, object>();
                var fy = after.Month > 3 ? after.Year.ToString("F0") : (after.Year + 1).ToString("F0");
                keyValues.Add("gstin", "");
                keyValues.Add("fy", fy);
                foreach (var gstitem in list.FindAll(x => x.filedon < after))
                {
                    keyValues["gstin"] = gstitem.gstno;
                    using (WebClient webClient = new WebClient())
                    {
                        try
                        {
                            webClient.Headers["content-type"] = "application/json";
                            reqString = Encoding.Default.GetBytes(JsonConvert.SerializeObject(keyValues, Formatting.Indented));
                            resByte = webClient.UploadData(gstfilingurl, "post", reqString);
                            resString = Encoding.Default.GetString(resByte);
                            resString = resString.Replace("[[", "[").Replace("]]", "]");
                            var filinginfo = JsonConvert.DeserializeObject<GstFilingInfo>(resString);
                            if (filinginfo.FilingStatus != null)
                            {
                                gstitem.filedon = BaseEntity.GetDataFromString(filinginfo.FilingStatus.OrderByDescending(x => x.Dof).First().Dof, "dd/MM/yyyy");
                                var month = filinginfo.FilingStatus.OrderByDescending(x => x.Dof).First().Taxp;
                                gstitem.filedmonth = (month.Length > 3 ? month.Substring(0, 3) : month) + "-" + fy;
                                gstitem.update = true;
                            }
                        }
                        catch (Exception e)
                        {

                        }
                    }
                }
            }
            return list;
        }



        public partial class GstFilingInfo
        {
            [JsonProperty("filingStatus")]
            public List<Filing> FilingStatus { get; set; }

            [JsonProperty("status")]
            public string Status { get; set; }

            public partial class Filing
            {
                [JsonProperty("fy")]
                public string Fy { get; set; }

                [JsonProperty("taxp")]
                public string Taxp { get; set; }

                [JsonProperty("mof")]
                public string Mof { get; set; }

                [JsonProperty("dof")]
                public string Dof { get; set; }

                [JsonProperty("rtntype")]
                public string Rtntype { get; set; }

                [JsonProperty("arn")]
                public string Arn { get; set; }

                [JsonProperty("status")]
                public string Status { get; set; }
            }
        }



        public Company Update(Company oCompany)
        {
            if (!CanRunONSAAS())
            {
                throw new Exception("SAAS Application can't run without organization information");
            }
            NpgsqlTransaction transaction = null;
            try
            {
                using (NpgsqlConnection conn = CreateConnection())
                {
                    conn.Open();
                    transaction = conn.BeginTransaction();

                    Update(transaction, conn, oCompany);
                    transaction.Commit();
                    conn.Close();
                }
            }
            catch (Exception exp)
            {

                WebAPI.Common.Logger.Instance(Context.OrganizationID).Log(exp, WebAPI.Common.Logger.LogType.Error, exp.StackTrace);
            }

            return oCompany;
        }

        public Company Update(NpgsqlTransaction transaction, NpgsqlConnection conn, Company oCompany)
        {

            NpgsqlCommand command = GetCommand(SQL_Update, conn, transaction);
            command.Parameters.Add("id", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oCompany.id;
            command.Parameters.Add("name", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oCompany.name == null ? "" : oCompany.name;
            command.Parameters.Add("contactperson", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oCompany.contactperson == null ? "" : oCompany.contactperson;
            command.Parameters.Add("billingname", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oCompany.billingname == null ? "" : oCompany.billingname;
            command.Parameters.Add("contactno", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oCompany.contactno == null ? "" : oCompany.contactno;
            command.Parameters.Add("address", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oCompany.address == null ? "" : oCompany.address;
            command.Parameters.Add("cityid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oCompany.cityid;
            command.Parameters.Add("stateid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oCompany.stateid;
            command.Parameters.Add("countryid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oCompany.countryid;
            command.Parameters.Add("pincode", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oCompany.pincode == null ? "" : oCompany.pincode;
            command.Parameters.Add("tinanddate", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oCompany.tinanddate == null ? "" : oCompany.tinanddate;
            command.Parameters.Add("gstusername", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oCompany.gstusername == null ? "" : oCompany.gstusername;
            command.Parameters.Add("gstaccesskey", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oCompany.gstaccesskey == null ? "" : oCompany.gstaccesskey;
            command.Parameters.Add("cstanddate", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oCompany.cstanddate == null ? "" : oCompany.cstanddate;
            command.Parameters.Add("areacode", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oCompany.areacode == null ? "" : oCompany.areacode;
            command.Parameters.Add("tan", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oCompany.tan == null ? "" : oCompany.tan;
            command.Parameters.Add("pan", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oCompany.pan == null ? "" : oCompany.pan;
            command.Parameters.Add("bankaccountname", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oCompany.bankaccountname == null ? "" : oCompany.bankaccountname;
            command.Parameters.Add("bankaccountno", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oCompany.bankaccountno == null ? "" : oCompany.bankaccountno;
            command.Parameters.Add("bankifci", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oCompany.bankifci == null ? "" : oCompany.bankifci;
            command.Parameters.Add("pfno", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oCompany.pfno == null ? "" : oCompany.pfno;
            command.Parameters.Add("esino", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oCompany.esino == null ? "" : oCompany.esino;
            command.Parameters.Add("isactive", NpgsqlTypes.NpgsqlDbType.Boolean).Value = oCompany.isactive;
            command.Parameters.Add("createdby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oCompany.createdby;
            command.Parameters.Add("modifiedby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = Context.UserID;
            command.Parameters.Add("version", NpgsqlTypes.NpgsqlDbType.Integer).Value = oCompany.version;
            command.Parameters.Add("createdon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = oCompany.createdon;
            command.Parameters.Add("modifiedon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = DateTimeNow;
            command.Parameters.Add("emailid", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oCompany.emailid == null ? "" : oCompany.emailid;
            command.Parameters.Add("logo", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oCompany.logo == null ? "" : oCompany.logo;
            command.Parameters.Add("servers", NpgsqlTypes.NpgsqlDbType.Json).Value = oCompany.servers_json == null ? "null" : oCompany.servers_json;
            command.Parameters.Add("tallyconfig", NpgsqlTypes.NpgsqlDbType.Json).Value = oCompany.tallyconfig_json == null ? "null" : oCompany.tallyconfig_json;
            command.Parameters.Add("code", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oCompany.code == null ? "" : oCompany.code;
            command.Parameters.Add("website", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oCompany.website == null ? "" : oCompany.website;
            command.Parameters.Add("einvoicepwd", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oCompany.einvoicepwd == null ? "" : oCompany.einvoicepwd;
            command.Parameters.Add("einvoiceusername", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oCompany.einvoiceusername == null ? "" : oCompany.einvoiceusername;
            command.Parameters.Add("company_as_suppliercode", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oCompany.company_as_suppliercode == null ? "" : oCompany.company_as_suppliercode;
            command.Parameters.Add("company_as_customercode", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oCompany.company_as_customercode == null ? "" : oCompany.company_as_customercode;
            command.Parameters.Add("company_as_customerid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oCompany.company_as_customerid;
            command.Parameters.Add("company_as_supplierid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oCompany.company_as_supplierid;
            command.Parameters.Add("internalmargin", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oCompany.internalmargin;
            command.Parameters.Add("internalbuyer", NpgsqlTypes.NpgsqlDbType.Boolean).Value = oCompany.internalbuyer;

            using (System.Data.IDataReader reader = command.ExecuteReader())
            {
                if (reader.Read())
                {
                    oCompany.version = Convert.ToInt32(reader[0]);
                }
            }

            return oCompany;
        }


        public bool Delete(Company oCompany)
        {
            if (!CanRunONSAAS())
            {
                throw new Exception("SAAS Application can't run without organization information");
            }
            NpgsqlTransaction transaction = null;
            bool hasSuccessCount = false;
            try
            {
                using (NpgsqlConnection conn = CreateConnection())
                {
                    conn.Open();
                    transaction = conn.BeginTransaction();
                    hasSuccessCount = Delete(transaction, conn, oCompany);
                    transaction.Commit();
                    conn.Close();
                }
            }
            catch (Exception exp)
            {

                WebAPI.Common.Logger.Instance(Context.OrganizationID).Log(exp, WebAPI.Common.Logger.LogType.Error, exp.StackTrace);
            }
            return hasSuccessCount;

        }

        public bool Delete(NpgsqlTransaction transaction, NpgsqlConnection conn, Company oCompany)
        {
            NpgsqlCommand command = GetCommand(SQL_Delete, conn, transaction);
            command.Parameters.Add("isactive", NpgsqlTypes.NpgsqlDbType.Boolean).Value = oCompany.isactive;
            command.Parameters.Add("modifiedby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = Context.UserID;
            command.Parameters.Add("modifiedon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = DateTimeNow;
            command.Parameters.Add("version", NpgsqlTypes.NpgsqlDbType.Integer).Value = oCompany.version;
            command.Parameters.Add("id", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oCompany.id;
            int successCount = command.ExecuteNonQuery();
            return successCount > 0;
        }
        public List<InternalTransfers> InternalVendorConfig()
        {
            string query = @"select company.id companyid,company_as_supplierid,
                            suppliercompany.id suppliercompanyid,company_as_customerid from company
                            left outer join suppliercompany on suppliercompany.supplierid  = company_as_supplierid
                            where company.isactive=true";
            List<InternalTransfers> response = new List<InternalTransfers>();
            using (var reader = ExecuteReader(query, null))
            {
                while(reader.Read())
                {
                    response.Add(new InternalTransfers()
                    {
                        companyid = reader["companyid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["companyid"]),
                        supplierid = reader["company_as_supplierid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["company_as_supplierid"]),
                        suppliercompanyid = reader["suppliercompanyid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["suppliercompanyid"]),
                        customerid = reader["company_as_customerid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["company_as_customerid"]),
                    });
                }
            }

            query = @"select company.id companyid,customer.id customerid from company
            inner join customer on customer.pan != '' and customer.pan = company.pan
            where company.pan != '' and company.isactive=true";
            using (var reader = ExecuteReader(query, null))
            {
                while (reader.Read())
                {
                    response.Add(new InternalTransfers()
                    {
                        companyid = reader["companyid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["companyid"]),
                        customerid = reader["customerid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["customerid"]),
                    });
                }
            }
            return response;
        }
        public class InternalTransfers
        {
            public long companyid { get; set; }
            public long supplierid { get; set; }
            public long suppliercompanyid { get; set; }
            public long customerid { get; set; }
        }

        internal object InsertImport(NpgsqlConnection conn, GST_Expense gstinfo)
        {
            NpgsqlCommand command = GetCommand(@"INSERT INTO public.gst_exlude(gstno,invoiceno,invoicedate,lrinvoiceid,gstinboxid,reason,tradename,taxable,tax)
            VALUES(@gstno,@invoiceno,@invoicedate,@lrinvoiceid,@gstinboxid,@reason,@tradename,@taxable,@tax)", conn);
            command.Parameters.Add("invoiceno", NpgsqlTypes.NpgsqlDbType.Varchar).Value = gstinfo.invoiceno == null ? "" : gstinfo.invoiceno;
            command.Parameters.Add("gstno", NpgsqlTypes.NpgsqlDbType.Varchar).Value = gstinfo.gstno == null ? "" : gstinfo.gstno;
            command.Parameters.Add("tradename", NpgsqlTypes.NpgsqlDbType.Varchar).Value = gstinfo.tradename == null ? "" : gstinfo.tradename;
            command.Parameters.Add("reason", NpgsqlTypes.NpgsqlDbType.Varchar).Value = "Import";
            command.Parameters.Add("invoicedate", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = gstinfo.invoicedate;
            command.Parameters.Add("gstinboxid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = 0;
            command.Parameters.Add("lrinvoiceid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = 0;
            command.Parameters.Add("tax", NpgsqlTypes.NpgsqlDbType.Numeric).Value = gstinfo.tax;
            command.Parameters.Add("taxable", NpgsqlTypes.NpgsqlDbType.Numeric).Value = gstinfo.taxable;
            command.ExecuteNonQuery();
            return gstinfo;
        }
    }

}
