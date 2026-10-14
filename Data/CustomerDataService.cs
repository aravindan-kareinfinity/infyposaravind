using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using InfyPOS.Models;
using InfyPOS.Common;
using Npgsql;
using System.Text;
using System.Reflection;
using Newtonsoft.Json;

namespace InfyPOS.Data
{

    public class CustomerDataService : SAASData
    {
        const string SQL_Search = @"select customer.id, customer.isactive, customer.createdby, customer.modifiedby,
                                    customer.version, customer.createdon, customer.modifiedon, customer.srmaxdays,referredby, customer.supportcredit, 
                                    creditlimit,creditdays, creditrightsgivenby,mobileno, otherno, customertype, customer.name, dateofbirth, marriagedate, 
                                    loyelty, gender, area,areaid, married, malekids, femalekids, loyeltyno, customer.code,gstno,pan,tan,categoryid,
                                    howoftenid,regsourceid,customer.attachment,
                                    cast(referencelist.additionalinfo->>'rewardpoint' as numeric) rewardpoint,
                                    customer.cityid, customer.districtid,customer.agentid,customer.preferredtransportid,
                                    city.name cityname,state.name statename, district.name districtname, referencelist.name categoryname,
                                    cast(referencelist.additionalinfo->>'billdiscount' as numeric) discount,
                                    cast(referencelist.additionalinfo->>'billdiscountinclude' as bool) billdiscountinclude,
                                    referencelist.additionalinfo->>'billdiscountconfig' billdiscountconfig,
									cast(referencelist.additionalinfo->>'ccmargin' as numeric) ccmargin,
                                    cast(referencelist.additionalinfo->>'ccmarginmode' as bool) ccmarginmode,
                                    cast(referencelist.additionalinfo->>'ccmarginmrp' as numeric) ccmarginmrp,
                                    cast(referencelist.additionalinfo->>'ccmarginprice' as numeric) ccmarginprice,
                                    cast(referencelist.additionalinfo->>'markdownmode' as numeric) markdownmode,
                                    customer.emailid,customer.registredlocationid,customergroupid,customer.pricesource,customer.mrpsource, customer.supplytype,
                                    customer.address, customer.deliveryaddress, customer.cityid, customer.stateid, customer.countryid, customer.pincode, customer.contactperson, 
                                    customer.bankaccountname, customer.bankaccountno, customer.bankifci, customer.isinterstatesale,customer.billingname,customer.religionid,
                                    customer.family,customer.deliveryaddresslist,customer.disableloyalty,customer.activestatus,
                                    portloadingid, portdischargeid, country.name countryname,loading.name loadingname,discharge.name dischargename from customer 
                                    left outer join referencelist city on city.id = customer.cityid
                                    left outer join referencelist state on state.id = customer.stateid
                                    left outer join referencelist district on district.id = customer.districtid
                                    left outer join referencelist on referencelist.id = customer.categoryid
                                    left outer join referencelist country on country.id = customer.countryid
                                    left outer join referencelist loading on loading.id = customer.portloadingid
                                    left outer join referencelist discharge on discharge.id = customer.portdischargeid
                                    left outer join (select id,cast(additionalinfo->>'parentid' as bigint) saleareagroup1,
                                    cast(additionalinfo->>'floorid' as bigint) saleareagroup2 from referencelist where type ='AREA') customerarea on customerarea.id = customer.areaid";



        const string SQL_SearchRewardOLD = @"with discounttype as
(
select bill.id, sum(billitems.additionaldiscount) addlndiscount, sum(billitems.schemediscount) schdiscount, sum(billitems.receivable) receivable
	from customer inner join bill on bill.customerid = customer.id
	inner join billitems on billitems.billid = bill.id and bill.isactive = true and billitems.isactive=true
	where bill.billtype != 9 and bill.customerid > 0 and COALESCE(bill.settled,true) = true
	{0}
group by bill.id
)
select customerrewards.id, customer.id customerid, customer.name, customer.mobileno, customer.customertype,
rewards, adjusted, expiry, customerrewards.billno, transactiondate, customerrewards.locationid,
addlndiscount, schdiscount, receivable, customerrewards.billvalue, customerrewards.altered, customerrewards.remark
from customer 
left outer join  customerrewards  on customerrewards.customerid = customer.id
left outer join discounttype on discounttype.id = customerrewards.billid ";

        const string SQL_SearchReward = @"
        select customerrewards.id, customer.id customerid, customer.name, customer.mobileno, customer.customertype,
        rewards, adjusted, expiry, customerrewards.billno, transactiondate, customerrewards.locationid,
        additionaldiscount addlndiscount,schemediscount schdiscount, receivable, customerrewards.billvalue, 
        customerrewards.altered, customerrewards.remark
        from customerrewards
        inner join customer on customer.id = customerrewards.customerid
        left outer join bill on bill.id = customerrewards.billid";

        const string SQL_SearchBills = @"select bill.locationid, bill.counterid, bill.id billid,bill.billtype, bill.billno, bill.billdate, bill.receivable+bill.rounding billvalue,
                                        customer.id customerid, customer.name, customer.gstno, customer.mobileno, customer.otherno, customer.categoryid, bill.isinterstatesale,
                                    customer.supplytype
                                    from bill left outer join customer on customer.id = bill.customerid";


        const string SQL_Insert = @"INSERT INTO public.customer(isactive, createdby, modifiedby, version, createdon, modifiedon, supportcredit,srmaxdays,referredby, creditlimit,creditdays, creditrightsgivenby, mobileno, customertype, name, dateofbirth, marriagedate, loyelty, gender, area,areaid, married, malekids, femalekids, loyeltyno, code,gstno,pan,tan,categoryid, emailid,address, cityid, stateid, countryid, pincode, contactperson, bankaccountname, bankaccountno, bankifci, isinterstatesale,registredlocationid,customergroupid,pricesource,mrpsource,billingname,religionid,agentid,preferredtransportid,districtid,family,deliveryaddresslist,disableloyalty,deliveryaddress,otherno,supplytype,portloadingid,portdischargeid,howoftenid,regsourceid,attachment) 
                                    VALUES(@isactive, @createdby, @modifiedby, @version, @createdon, @modifiedon, @supportcredit,@srmaxdays,@referredby, @creditlimit,@creditdays, @creditrightsgivenby, @mobileno, @customertype, @name, @dateofbirth, @marriagedate, @loyelty, @gender, @area,@areaid, @married, @malekids, @femalekids, @loyeltyno, @code,@gstno,@pan,@tan,@categoryid, @emailid,@address, @cityid, @stateid, @countryid, @pincode, @contactperson, @bankaccountname, @bankaccountno, @bankifci, @isinterstatesale,@registredlocationid,@customergroupid,@pricesource,@mrpsource,@billingname,@religionid,@agentid,@preferredtransportid,@districtid,@family,@deliveryaddresslist,@disableloyalty,@deliveryaddress,@otherno,@supplytype,@portloadingid,@portdischargeid,@howoftenid,@regsourceid,@attachment) RETURNING id";



        const string SQL_Birthday = @"select name, gender, mobileno,dateofbirth, marriagedate,gstno,pincode,address,loyelty,area  from customer 
                                        where isactive=true and dateofbirth>'1904-01-01'
                                        and ('1904-'||to_char(dateofbirth,'MM-dd'))::date >= ('1904-'||to_char(@fromdate::date,'MM-dd'))::date
                                        and ('1904-'||to_char(dateofbirth,'MM-dd'))::date <= ('1904-'||to_char(@todate::date,'MM-dd'))::date
                                        order by dateofbirth";



        const string SQL_KidBirthday = @"select cast(itemlist->> 'name' as character varying(100)) as name, cast(itemlist->> 'gender' as BIGINT) gender, mobileno, 
                                        cast(itemlist->> 'dateofbirth' as date) dateofbirth, marriagedate,gstno,pincode,address,loyelty,area  from customer INNER JOIN LATERAL json_array_elements (customer.family :: json) itemlist ON true 
                                        where isactive = true and mobileno is not null and length(mobileno)>=10 AND json_typeof(customer.family)='array' 
                                        and cast(itemlist->> 'dateofbirth' as timestamp without time zone)>'1904-01-01'
                                        and ('1904-'||to_char(cast(itemlist->> 'dateofbirth' as timestamp without time zone),'MM-dd'))::date >= ('1904-'||to_char(@fromdate::date,'MM-dd'))::date
                                        and ('1904-'||to_char(cast(itemlist->> 'dateofbirth' as timestamp without time zone),'MM-dd'))::date <= ('1904-'||to_char(@todate::date,'MM-dd'))::date ";

        const string SQL_Marriageday = @"select name, gender, mobileno, dateofbirth,marriagedate,gstno,pincode,address,
loyelty,area  from customer 
where isactive=true and marriagedate>'1904-01-01'
and ('1904-'||to_char(marriagedate,'MM-dd'))::date >= ('1904-'||to_char(@fromdate::date,'MM-dd'))::date
and ('1904-'||to_char(marriagedate,'MM-dd'))::date <= ('1904-'||to_char(@todate::date,'MM-dd'))::date
order by marriagedate";


        const string SQL_SMSLog = @"Select schedule.id, schedule.name, schedule.description, smshistory.message, smshistory.mobileno, smshistory.actionon
from schedule left outer join smshistory on smshistory.scheduleid = schedule.id ";

        const string SQL_Update = @"UPDATE public.customer SET modifiedby=@modifiedby, version=@version+1, modifiedon=@modifiedon, isactive=@isactive, supportcredit=@supportcredit,srmaxdays=@srmaxdays,referredby=@referredby, creditlimit=@creditlimit,creditdays=@creditdays, creditrightsgivenby=@creditrightsgivenby, mobileno=@mobileno, customertype=@customertype, name=@name, dateofbirth=@dateofbirth, 
                                    marriagedate=@marriagedate, loyelty=@loyelty, gender=@gender, area=@area,areaid=@areaid, married=@married, malekids=@malekids, femalekids=@femalekids, loyeltyno=@loyeltyno, code=@code, gstno=@gstno,tan=@tan,pan=@pan,categoryid=@categoryid,emailid=@emailid,supplytype=@supplytype,
                                    address=@address, cityid=@cityid, districtid=@districtid, stateid=@stateid, countryid=@countryid, pincode=@pincode, registredlocationid=@registredlocationid,customergroupid=@customergroupid,pricesource=@pricesource,mrpsource=@mrpsource, contactperson=@contactperson, bankaccountname=@bankaccountname, bankaccountno=@bankaccountno, bankifci=@bankifci, 
                                    isinterstatesale=@isinterstatesale,billingname=@billingname,religionid=@religionid,agentid=@agentid,preferredtransportid=@preferredtransportid,family=@family,deliveryaddresslist=@deliveryaddresslist,disableloyalty=@disableloyalty,deliveryaddress=@deliveryaddress,otherno=@otherno,
                                    portloadingid=@portloadingid,portdischargeid=@portdischargeid,howoftenid=@howoftenid,regsourceid=@regsourceid,attachment=@attachment where id=@id and version=@version RETURNING version";

        const string SQL_UpdateReward = @"UPDATE public.customer SET modifiedby=@modifiedby, version=version+1, modifiedon=@modifiedon, loyelty=COALESCE(loyelty,0)+@loyelty where id=@id RETURNING loyelty";

        const string SQL_Delete = @"update public.customer set isactive=false,version=@version+1,modifiedby=@modifiedby,modifiedon=@modifiedon where id=@id and version=@version";
        const string SQL_DeleteWith = @"update public.customer set mobileno=concat('~', mobileno), isactive=false,version=@version+1,modifiedby=@modifiedby,modifiedon=@modifiedon where id=@id and version=@version";

        const string SQL_HighValued = @"select customer.name, customer.mobileno, customer.area,  
                                    sum(bill.receivable+bill.rounding) billvalue,
									count(bill.billno) billnos
                                    from billitems 
                                    inner join bill on bill.id = billitems.billid
                                    inner join salablegoods on salablegoods.id = billitems.salablegoodsid
                                    inner join stock on stock.id = billitems.stockid
                                    left outer join items on items.id = stock.itemid
                                    left outer join customer on customer.id = bill.customerid";

        const string SQL_InactiveCustomer = @"with activebill as 
(
select bill.customerid, sum(bill.receivable + bill.rounding) billvalue, count(bill.billno) billnos
from bill Where bill.isactive=True And bill.billtype!=9 And bill.customerid > 0  
	{0} 
group by bill.customerid	
), inactivebill as (
select bill.customerid, sum(bill.receivable + bill.rounding) billvalue, count(bill.billno) billnos
from bill Where bill.isactive=True And bill.billtype!=9 And bill.customerid > 0  
    {1}
group by bill.customerid	
)
select customer.name, customer.gender, customer.mobileno, dateofbirth,marriagedate,gstno,
customer.pincode,customer.address,customer.loyelty,customer.area,customer.cityid,city.name cityname,
customer.billingname,customer.religionid, activebill.customerid, activebill.billvalue activebillvalue, activebill.billnos activebillnos, 
inactivebill.customerid, inactivebill.billvalue inactivebillvalue, inactivebill.billnos inactivebillnos
from customer inner join activebill on activebill.customerid = customer.id 
left outer join inactivebill on activebill.customerid = inactivebill.customerid
left outer join referencelist city on city.id = customer.cityid";

        const string SQL_RewardBill = @"select billno,billdate,cast(billattributes->>'spentreward' as numeric) rewards
from bill inner join customer on customer.id=bill.customerid
where bill.isactive=true and bill.billtype!=9 and cast(billattributes->>'spentreward' as numeric) > 0 
and bill.customerid=@customerid";

        const string SQL_MyCustomer = @"with billdetails as 
(
select bill.customerid, sum(bill.receivable + bill.rounding) billvalue, count(bill.billno) billnos
from bill Where bill.isactive=True And bill.billtype!=9 And bill.customerid > 0 {0} 
group by bill.customerid	
)
select customer.name, customer.gender, customer.mobileno, dateofbirth,marriagedate,gstno,
customer.pincode,customer.address,customer.deliveryaddress,customer.loyelty,customer.area,customer.cityid,city.name cityname,
customer.billingname,customer.religionid, billdetails.billvalue, billdetails.billnos
from customer 
left outer join billdetails on billdetails.customerid = customer.id 
left outer join referencelist city on city.id = customer.cityid 
left outer join (select id,cast(additionalinfo->>'parentid' as bigint) saleareagroup1,
cast(additionalinfo->>'floorid' as bigint) saleareagroup2 from referencelist where type ='AREA') customerarea on customerarea.id = customer.areaid
 where customer.isactive = true ";

        const string SQL_MyCustomerbyPurchase = @"with billdetails as 
(
select bill.customerid, sum(billitems.receivable) billvalue
from bill inner join billitems on  billitems.billid = bill.id
inner join salablegoods on salablegoods.id = billitems.salablegoodsid
inner join stock on stock.id = salablegoods.stockid 
inner join items on items.id = stock.itemid 
Where bill.isactive=True And bill.billtype!=9 And bill.customerid > 0 {0} 
group by bill.customerid	
)
select customer.name, customer.gender, customer.mobileno, dateofbirth,marriagedate,gstno,
customer.pincode,customer.address,customer.deliveryaddress,customer.loyelty,customer.area,customer.cityid,city.name cityname,
customer.billingname,customer.religionid, billdetails.billvalue, 1 billnos
from customer 
left outer join billdetails on billdetails.customerid = customer.id 
left outer join referencelist city on city.id = customer.cityid 
left outer join (select id,cast(additionalinfo->>'parentid' as bigint) saleareagroup1,
cast(additionalinfo->>'floorid' as bigint) saleareagroup2 from referencelist where type ='AREA') customerarea on customerarea.id = customer.areaid
 where customer.isactive = true ";

        internal bool ConfirmOTP(Customer item)
        {
            if (!string.IsNullOrEmpty(InfyPOS.Services.ConfigManager.Instance(Context.OrganizationID).Config.integration_crm))
            {
                CustomerDataService customerDataService = new CustomerDataService() { Context = this.Context };
                var integration = Assembly.GetExecutingAssembly().GetType(InfyPOS.Services.ConfigManager.Instance(Context.OrganizationID).Config.integration_crm);
                if (integration != null)
                {
                    ICustomerrewardsDataService bright = Activator.CreateInstance(integration) as ICustomerrewardsDataService;
                    bright.Context = this.Context;
                    return bright.ValidateOTP(item);
                }
                return false;
            }

            string query = "select otp from customer where id=@id limit 1";
            var param = new SortedList<string, object>();
            param.Add("id", item.id);
            using (var reader = ExecuteReader(query, param))
            {
                if (reader.Read())
                    return (!reader.IsDBNull(0) && reader.GetString(0) == item.otp);
            }
            return false;
        }

        internal string SendOTP(Customer item)
        {
            var newcode = WebAPI.Data.Token.RandomString(4, "0123456789");
            try
            {
                if (string.IsNullOrEmpty(item.name))
                {
                    var customers = Search(new CustomerCreteria()
                    {
                        id = item.id
                    });
                    if (customers.Count == 1)
                    {
                        item.name = customers[0].name;
                        item.mobileno = customers[0].mobileno;
                        item.loyeltyno = customers[0].loyeltyno;
                    }
                }
                using (NpgsqlConnection conn = CreateConnection())
                {
                    conn.Open();
                    string query = "Update customer set otp = @otp where id=@id";
                    var param = new SortedList<string, object>();
                    param.Add("otp", newcode);
                    param.Add("id", item.id);
                    ExecuteQuery(query, param);
                    conn.Close();


                    if (Processors.Schedule_Actions.Instance.HasConfig(this.Context.OrganizationID,
                        Processors.Schedule_Actions.SourceType.OTP, Processors.Schedule_Actions.Actions.Otp_Loyelty))
                    {
                        Processors.Schedule_Actions.Instance.OnChange(this.Context.OrganizationID, new BillOTP()
                        {
                            OTP = newcode,
                            MOBILE = item.mobileno,
                            CUSTOMER = item.name,
                            CLAIMAMOUNT = item.claimamount,
                            CLAIMPOINT = item.claimpoint
                        }, Processors.Schedule_Actions.Actions.Otp_Loyelty);
                    }
                    else
                    {
                        string otptext = @"Your otp to claim your reward is " + newcode;

                        if (!string.IsNullOrEmpty(InfyPOS.Services.ConfigManager.Instance(Context.OrganizationID).Config.customerotpfmt))
                        {
                            otptext = InfyPOS.Services.ConfigManager.Instance(Context.OrganizationID).Config.customerotpfmt.Replace("[OTP]", newcode);
                            otptext = otptext.Replace("[AMOUNT]", Math.Round(item.claimamount, 2).ToString());
                            otptext = otptext.Replace("[POINT]", Math.Round(item.claimamount, 2).ToString());
                            otptext = otptext.Replace("[NAME]", item.name);
                        }
                        InfyPOS.Services.SMSManager.Instance.SendSMS(Context.OrganizationID, item.mobileno, otptext);
                    }
                }
            }
            catch (Exception exp)
            {
                WebAPI.Common.Logger.Instance(Context.OrganizationID).Log(exp, WebAPI.Common.Logger.LogType.Error, item);
            }
            return "Generated";
        }
        public List<Customer> SearchByWalkinToken(CustomerCreteria _customer)
        {
            string _SQL = @"select id,locationid,actiondate,type,cast(itemlist->>'mobileno' as varchar) mobileno 
from storetransaction INNER JOIN LATERAL json_array_elements (case when 
                                                                    storetransaction.walkincustomerinfo is null or storetransaction.walkincustomerinfo::text ='null' then '[]'::json 
															  else storetransaction.walkincustomerinfo :: json end) itemlist ON true
where itemlist->>'token' = @token and actiondate=@actiondate and locationid=@locationid and type='WalkIn' limit 1";

            NpgsqlConnection conn = CreateConnection();
            string mobileno = "";
            try
            {
                conn.Open();
                NpgsqlCommand command = GetCommand(_SQL, conn);
                command.Parameters.Add("locationid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = _customer.locationid;
                command.Parameters.Add("actiondate", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = DateTimeNow.Date;
                command.Parameters.Add("token", NpgsqlTypes.NpgsqlDbType.Varchar).Value = _customer.walkintoken;
                using (System.Data.IDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        mobileno = reader["mobileno"] == DBNull.Value ? "" : Convert.ToString(reader["mobileno"]);
                    }
                }
                conn.Close();
            }
            catch (Exception exp)
            {
                if (conn != null) conn.Close();
                WebAPI.Common.Logger.Instance(Context.OrganizationID).Log(exp, WebAPI.Common.Logger.LogType.Error, "Ranged Tax Products");
            }

            if (!string.IsNullOrEmpty(mobileno))
                return Search(new CustomerCreteria()
                {
                    mobileno = mobileno,
                    locationid = _customer.locationid
                });
            return new List<Customer>();
        }

        public List<Customer> Search(CustomerCreteria _customer)
        {
            List<Customer> lst = new List<Customer>();
            NpgsqlConnection conn = CreateConnection();
            try
            {
                conn.Open();

                NpgsqlCommand command = null;
                if (_customer.id != 0)
                {
                    command = GetCommand(SQL_Search+ " where customer.id=@id", conn);
                    command.Parameters.Add("id", NpgsqlTypes.NpgsqlDbType.Bigint).Value = _customer.id;
                }
                else if (_customer.ids != null && _customer.ids.Count > 0)
                {
                    QueryBuilder qb = new QueryBuilder();
                    qb.CurrentQuery = SQL_Search+ " where customer.isactive=true ";
                    qb.AddInParamer("Customer.id", string.Join(",", _customer.ids));
                    command = qb.GetCommand(conn);
                }
                else
                {
                    QueryBuilder qb = new QueryBuilder();
                    qb.CurrentQuery = SQL_Search;

                    if (_customer.inactive)
                        qb.CurrentQuery += " where customer.isactive=false ";
                    else
                        qb.CurrentQuery += " where customer.isactive=true ";

                    if (!string.IsNullOrEmpty(_customer.code))
                        qb.AddParameter("customer.code", _customer.code, "=");

                    if (_customer.customertype > 0)
                        qb.AddParameter("customer.customertype", _customer.customertype, "=");
                    if (_customer.categoryid > 0)
                        qb.AddParameter("customer.categoryid", _customer.categoryid, "=");
                    if (_customer.pricesource > 0)
                        qb.AddParameter("customer.pricesource", _customer.pricesource, "=");
                    if (_customer.religionid > 0)
                        qb.AddParameter("customer.religionid", _customer.religionid, "=");
                    if (_customer.supportcredit.Equals(true))
                        qb.AddParameter("customer.supportcredit", true, "=");

                    if (!string.IsNullOrEmpty(_customer.mobileno))
                    {
                        if (_customer.likemobileno && _customer.mobileno.Length < 10)
                            qb.AddParameter("customer.mobileno", _customer.mobileno + "%", " like ");
                        else
                            qb.AddParameter("customer.mobileno", _customer.mobileno, "=");
                    }
                    if (!string.IsNullOrEmpty(_customer.emailid))
                        qb.AddParameter("customer.emailid", _customer.emailid, "=");
                    if (!string.IsNullOrEmpty(_customer.loyeltyno))
                        qb.AddParameter("customer.loyeltyno", _customer.loyeltyno, "=");

                    if (!string.IsNullOrEmpty(_customer.name))
                    {
                        if (_customer.name.IndexOf('%') >= 0)
                            qb.AddParameter("customer.name", _customer.name, " ilike ");
                        else
                            qb.AddParameter("customer.name", _customer.name, " = ");
                    }

                    if (!string.IsNullOrEmpty(_customer.billingname))
                    {
                        qb.AddParameter("customer.billingname", "%" + _customer.billingname + "%", " ilike ");
                    }
                    if (!string.IsNullOrEmpty(_customer.area))
                        qb.AddParameter("customer.area", "%" + _customer.area + "%", " ilike ");

                    if(_customer.areaid>0)
                        qb.AddParameter("customer.areaid", _customer.areaid, " = ");

                    if (_customer.areagroup1id > 0)
                        qb.AddParameter("customerarea.saleareagroup1", _customer.areagroup1id, " = ");

                    if (_customer.areagroup2id > 0)
                        qb.AddParameter("customerarea.saleareagroup2", _customer.areagroup2id, " = ");

                    if (_customer.frompoint > 0)
                        qb.Add2Query(" customer.loyelty>=" + _customer.frompoint + "");
                    if (_customer.topoint > 0)
                        qb.Add2Query(" customer.loyelty<=" + _customer.topoint + "");

                    if (_customer.hasgst)
                    {
                        //qb.AddParameter("customer.gstno", "null", "!= ");
                        qb.AddParameter("customer.gstno", "", " != ");
                    }
                    if (_customer.newcustomer)
                        qb.AddParameter("customer.version", 1, "=");

                    if (_customer.withaddress)
                        qb.Add2Query(" length(customer.address)>10");
                    if (_customer.withoutaddress)
                        qb.Add2Query(" length(customer.address)<10");

                    if (_customer.registredlocationid > 0)
                    {
                        qb.AddParameter("COALESCE(registredlocationid,@registredlocationid)", "registredlocationid", _customer.registredlocationid, "=");
                    }
                    if (_customer.pricesource > 0)
                    {
                        qb.AddParameter("pricesource", "pricesource", _customer.pricesource, "=");
                    }
                    if (_customer.customergroupid > 0)
                    {
                        qb.AddParameter("customergroupid", "customergroupid", _customer.customergroupid, "=");
                    }

                    if (_customer.fromdate != DateTime.MinValue && _customer.todate != DateTime.MinValue)
                    {
                        qb.AddParameter("customer.createdon", "datefrom", _customer.fromdate.Date, ">=");
                        qb.AddParameter("customer.createdon", "dateto", _customer.todate.Date.AddDays(1), "<");
                    }

                    qb.SortField = "customer.id desc";
                    if (_customer.limit > 0)
                        qb.Limit = _customer.limit;
                    else if (_customer.limit != -9)
                        qb.Limit = 1000;

                    command = qb.GetCommand(conn);
                    command.CommandTimeout = 10000;
                }
                using (System.Data.IDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        Customer ocustomer = new Customer();
                        ocustomer.id = reader["id"] == DBNull.Value ? 0 : Convert.ToInt64(reader["id"]);
                        ocustomer.categoryid = reader["categoryid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["categoryid"]);
                        ocustomer.categoryname = reader["categoryname"] == DBNull.Value ? "" : reader["categoryname"].ToString();
                        ocustomer.isactive = reader["isactive"] == DBNull.Value ? false : Convert.ToBoolean(reader["isactive"]);
                        ocustomer.createdby = reader["createdby"] == DBNull.Value ? 0 : Convert.ToInt64(reader["createdby"]);
                        ocustomer.modifiedby = reader["modifiedby"] == DBNull.Value ? 0 : Convert.ToInt64(reader["modifiedby"]);
                        ocustomer.version = reader["version"] == DBNull.Value ? 0 : Convert.ToInt32(reader["version"]);
                        ocustomer.createdon = reader["createdon"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(reader["createdon"]);
                        ocustomer.modifiedon = reader["modifiedon"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(reader["modifiedon"]);
                        ocustomer.supportcredit = reader["supportcredit"] == DBNull.Value ? false : Convert.ToBoolean(reader["supportcredit"]);
                        ocustomer.creditlimit = reader["creditlimit"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["creditlimit"]);
                        ocustomer.srmaxdays = reader["srmaxdays"] == DBNull.Value ? 0 : Convert.ToInt32(reader["srmaxdays"]);
                        ocustomer.referredby = reader["referredby"] == DBNull.Value ? "" : Convert.ToString(reader["referredby"]);
                        ocustomer.creditdays = reader["creditdays"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["creditdays"]);
                        ocustomer.creditrightsgivenby = reader["creditrightsgivenby"] == DBNull.Value ? 0 : Convert.ToInt64(reader["creditrightsgivenby"]);
                        ocustomer.mobileno = reader["mobileno"] == DBNull.Value ? "" : reader["mobileno"].ToString();
                        ocustomer.otherno = reader["otherno"] == DBNull.Value ? "" : reader["otherno"].ToString();
                        ocustomer.customertype = reader["customertype"] == DBNull.Value ? 0 : Convert.ToInt32(reader["customertype"]);
                        ocustomer.name = reader["name"] == DBNull.Value ? "" : reader["name"].ToString();
                        ocustomer.code = reader["code"] == DBNull.Value ? "" : reader["code"].ToString();
                        ocustomer.billingname = reader["billingname"] == DBNull.Value ? "" : reader["billingname"].ToString();
                        ocustomer.dateofbirth = reader["dateofbirth"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(reader["dateofbirth"]);
                        ocustomer.marriagedate = reader["marriagedate"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(reader["marriagedate"]);
                        ocustomer.loyelty = reader["loyelty"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["loyelty"]);
                        ocustomer.emailid = reader["emailid"] == DBNull.Value ? "" : reader["emailid"].ToString();
                        ocustomer.rewardunit = reader["rewardpoint"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["rewardpoint"]);

                        ocustomer.agentid = reader["agentid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["agentid"].ToString());
                        ocustomer.preferredtransportid = reader["preferredtransportid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["preferredtransportid"].ToString());
                        //agentid,preferredtransportid
                        ocustomer.discount = reader["discount"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["discount"]);
                        ocustomer.billdiscountinclude = reader["billdiscountinclude"] == DBNull.Value ? false : Convert.ToBoolean(reader["billdiscountinclude"]);

                        ocustomer.ccmargin = reader["ccmargin"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["ccmargin"]);
                        ocustomer.ccmarginmode = reader["ccmarginmode"] == DBNull.Value ? false : Convert.ToBoolean(reader["ccmarginmode"]);
                        ocustomer.ccmarginmrp = reader["ccmarginmrp"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["ccmarginmrp"]);
                        ocustomer.ccmarginprice = reader["ccmarginprice"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["ccmarginprice"]);
                        ocustomer.cccomputemode = reader["markdownmode"] == DBNull.Value ? 0 : Convert.ToInt32(reader["markdownmode"]);
                        ocustomer.activestatus = reader["activestatus"] == DBNull.Value ? 0 : Convert.ToInt32(reader["activestatus"]);
                        ocustomer.gender = reader["gender"] == DBNull.Value ? "" : reader["gender"].ToString();
                        ocustomer.area = reader["area"] == DBNull.Value ? "" : reader["area"].ToString();
                        ocustomer.gstno = reader["gstno"] == DBNull.Value ? "" : reader["gstno"].ToString();
                        ocustomer.supplytype = reader["supplytype"] == DBNull.Value ? "" : reader["supplytype"].ToString();

                        ocustomer.pan = reader["pan"] == DBNull.Value ? "" : reader["pan"].ToString();
                        ocustomer.tan = reader["tan"] == DBNull.Value ? "" : reader["tan"].ToString();

                        ocustomer.married = reader["married"] == DBNull.Value ? false : Convert.ToBoolean(reader["married"]);
                        ocustomer.malekids = reader["malekids"] == DBNull.Value ? "" : reader["malekids"].ToString();
                        ocustomer.femalekids = reader["femalekids"] == DBNull.Value ? "" : reader["femalekids"].ToString();
                        ocustomer.loyeltyno = reader["loyeltyno"] == DBNull.Value ? "" : reader["loyeltyno"].ToString();
                        ocustomer.code = reader["code"] == DBNull.Value ? "" : reader["code"].ToString();
                        ocustomer.address = reader["address"] == DBNull.Value ? "" : reader["address"].ToString();
                        ocustomer.deliveryaddress = reader["deliveryaddress"] == DBNull.Value ? "" : reader["deliveryaddress"].ToString();
                        ocustomer.cityid = reader["cityid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["cityid"].ToString());
                        ocustomer.districtid = reader["districtid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["districtid"].ToString());
                        ocustomer.stateid = reader["stateid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["stateid"].ToString());
                        ocustomer.countryid = reader["countryid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["countryid"].ToString());

                        ocustomer.registredlocationid = reader["registredlocationid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["registredlocationid"].ToString());
                        ocustomer.customergroupid = reader["customergroupid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["customergroupid"].ToString());

                        ocustomer.pricesource = reader["pricesource"] == DBNull.Value ? 0 : Convert.ToInt32(reader["pricesource"].ToString());
                        ocustomer.mrpsource = reader["mrpsource"] == DBNull.Value ? 0 : Convert.ToInt32(reader["mrpsource"].ToString());
                        ocustomer.disableloyalty = reader["disableloyalty"] == DBNull.Value ? false : Convert.ToBoolean(reader["disableloyalty"].ToString());

                        ocustomer.pincode = reader["pincode"] == DBNull.Value ? "" : reader["pincode"].ToString();
                        ocustomer.contactperson = reader["contactperson"] == DBNull.Value ? "" : reader["contactperson"].ToString();
                        ocustomer.bankaccountname = reader["bankaccountname"] == DBNull.Value ? "" : reader["bankaccountname"].ToString();
                        ocustomer.bankaccountno = reader["bankaccountno"] == DBNull.Value ? "" : reader["bankaccountno"].ToString();
                        ocustomer.bankifci = reader["bankifci"] == DBNull.Value ? "" : reader["bankifci"].ToString();
                        ocustomer.isinterstatesale = reader["isinterstatesale"] == DBNull.Value ? false : Convert.ToBoolean(reader["isinterstatesale"]);
                        ocustomer.cityname = reader["cityname"] == DBNull.Value ? "" : reader["cityname"].ToString();
                        ocustomer.statename = reader["statename"] == DBNull.Value ? "" : reader["statename"].ToString();
                        ocustomer.districtname = reader["districtname"] == DBNull.Value ? "" : reader["districtname"].ToString();
                        ocustomer.billingname = reader["billingname"] == DBNull.Value ? "" : reader["billingname"].ToString();
                        ocustomer.religionid = reader["religionid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["religionid"]);
                        ocustomer.countryname = reader["countryname"] == DBNull.Value ? "" : reader["countryname"].ToString();
                        ocustomer.portloadingid = reader["portloadingid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["portloadingid"].ToString());
                        ocustomer.portdischargeid = reader["portdischargeid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["portdischargeid"].ToString());
                        ocustomer.loadingname = reader["loadingname"] == DBNull.Value ? "" : reader["loadingname"].ToString();
                        ocustomer.dischargename = reader["dischargename"] == DBNull.Value ? "" : reader["dischargename"].ToString();
                        ocustomer.family_json = reader["family"] == DBNull.Value ? "null" : reader["family"].ToString();
                        ocustomer.deliveryaddresslist_json = reader["deliveryaddresslist"] == DBNull.Value ? "null" : reader["deliveryaddresslist"].ToString();
                        ocustomer.attachment_json = reader["attachment"] == DBNull.Value ? "null" : reader["attachment"].ToString();
                        ocustomer.howoftenid = reader["howoftenid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["howoftenid"]);
                        ocustomer.regsourceid = reader["regsourceid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["regsourceid"]);

                        if (_customer.locationid > 0 && reader["billdiscountconfig"] != DBNull.Value)
                        {
                            try
                            {
                                List<Referencelist.BilldiscountConfig> billdiscountConfigs = Newtonsoft.Json.JsonConvert.DeserializeObject<List<Referencelist.BilldiscountConfig>>(reader["billdiscountconfig"].ToString());
                                if (billdiscountConfigs.Exists(e => e.id == _customer.locationid && e.upgrade))
                                {
                                    ocustomer.discount = billdiscountConfigs.Find(e => e.id == _customer.locationid).billdiscount;
                                    ocustomer.rewardunit = billdiscountConfigs.Find(e => e.id == _customer.locationid).rewardpoint;
                                    ocustomer.billdiscountinclude = billdiscountConfigs.Find(e => e.id == _customer.locationid).billdiscountinclude;
                                    ocustomer.discountotp = billdiscountConfigs.Find(e => e.id == _customer.locationid).otp;
                                }

                            }
                            catch (Exception exp)
                            {

                            }
                        }
                        lst.Add(ocustomer);

                    }
                }

                if (_customer.includebills && lst != null && lst.Count == 1)
                {
                    lst[0].customerbill = SearchCustomerReward(new Customerbillrequest() { customerid = lst[0].id });
                }

                if (lst != null && lst.Count == 1)
                {
                    if (lst[0].disableloyalty)
                        lst[0].rewardunit = 0;
                    else
                    {
                        CustomerrewardsDataService CustomerrewardsData = new CustomerrewardsDataService() { Context = this.Context };
                        decimal reward = CustomerrewardsData.GetCustomerReward(new CustomerrewardsCreteria()
                        {
                            customerid = lst[0].id,
                            locationid = _customer.locationid
                        });
                        lst[0].rewardunit = reward;
                    }
                }

                if (lst.Count == 1 && _customer.id > 0 && lst[0].supportcredit &&
                    (lst[0].creditlimit > 0 || lst[0].creditdays > 0) &&
                    InfyPOS.Services.ConfigManager.Instance(Context.OrganizationID).Config.system_creditlimit)
                {
                    decimal pendingamount = 0;
                    decimal delayamount = 0;
                    var param = new SortedList<string, object>();
                    param.Add("customerid", _customer.id);
                    string query = @"select sum(round(customerinvoice.receivable,0)-(customerinvoice.paid+customerinvoice.discount)) totalbalance,
                    sum(case when transactiondate>=@duedate then round(customerinvoice.receivable,0)-(customerinvoice.paid+customerinvoice.discount) else 0 end)
                    from customerinvoice where customerid=@customerid and customerinvoice.type in (1,2,5)";
                    if (lst[0].creditdays > 0)
                    {
                        param.Add("duedate", DateTimeNow.Date.AddDays((int)lst[0].creditdays * -1));
                    }
                    using (var reader = ExecuteReader(query, param, conn))
                    {
                        if (reader.Read())
                        {
                            pendingamount = reader.IsDBNull(0) ? 0 : reader.GetDecimal(0);
                            delayamount = reader.IsDBNull(1) ? 0 : reader.GetDecimal(1);
                        }
                    }
                    lst[0].ErrorMessage = "";
                    lst[0].creditlimit_pending = pendingamount;
                    lst[0].creditlimit_delay = delayamount;
                    if (lst[0].creditlimit > 0 && lst[0].creditlimit < pendingamount)
                    {
                        lst[0].creditlimit_disable = true;
                        lst[0].ErrorMessage = "Exceed the credit limit of " + lst[0].creditlimit.ToString() + ", Balance :" + pendingamount;
                    }
                    if (lst[0].creditdays > 0 && delayamount > 0)
                    {
                        lst[0].creditlimit_disable = true;
                        lst[0].ErrorMessage += (!string.IsNullOrEmpty(lst[0].ErrorMessage) ? "\r\n" : "") + "Exceed the credit days of " + lst[0].creditdays.ToString() + ", Balance :" + delayamount;
                    }
                }
                conn.Close();
            }
            catch (Exception exp)
            {
                if (conn != null) conn.Close();
                WebAPI.Common.Logger.Instance(Context.OrganizationID).Log(exp, WebAPI.Common.Logger.LogType.Error, _customer);
            }
            return lst;
        }

        internal bool UpdateStatus(NpgsqlTransaction transaction, NpgsqlConnection conn, Customer customer)
        {
            NpgsqlCommand command = GetCommand("update customer set isactive=@isactive,activestatus=@activestatus,modifiedby=@modifiedby,modifiedon=@modifiedon where id=@id", conn, transaction);
            command.Parameters.Add("isactive", NpgsqlTypes.NpgsqlDbType.Boolean).Value = customer.isactive;
            command.Parameters.Add("modifiedby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = Context.UserID;
            command.Parameters.Add("modifiedon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = DateTimeNow;
            command.Parameters.Add("activestatus", NpgsqlTypes.NpgsqlDbType.Integer).Value = customer.activestatus;
            command.Parameters.Add("id", NpgsqlTypes.NpgsqlDbType.Bigint).Value = customer.id;
            int successCount = command.ExecuteNonQuery();
            return successCount > 0;
        }

        internal List<CustomerLedger> CustomerLedger(CustomerCreteria customerCreteria)
        {
            List<CustomerLedger> lst = new List<CustomerLedger>();
            NpgsqlConnection conn = CreateConnection();
            try
            {
                conn.Open();
                string query = @"select customer.id,customer.name,gstno,address,stateid,cityid,customertype,
                cast(referencelist.additionalinfo->> 'billdiscount' as numeric) billdiscount,citylist.name city,
                referencelist.additionalinfo->> 'marginmode' marginmode,pricesource,mrpsource,customer.agentid,
                cast(referencelist.additionalinfo->> 'ccmarginmrp' as numeric) ccmarginmrp,
                cast(referencelist.additionalinfo->> 'ccmarginprice' as numeric) ccmarginprice from customer
                left outer join referencelist on referencelist.id = customer.categoryid
                left outer join referencelist citylist on citylist.id = customer.cityid
                where customer.isactive = true and customertype in (2, 7, 6,8)
                order by customer.name";
                
                
                if (customerCreteria.restricted &&
                    Context.HasCustomerRestricted )
                {
                    query = query.Replace("customer.isactive = true", @"customer.id in (SELECT itemlist::text::bigint AS customer_id
                    FROM users
                    CROSS JOIN LATERAL json_array_elements(users.accessiblecustomer::json) AS itemlist
                    WHERE id = @userid) AND customer.isactive = true ");
                }

                if (!(Services.ConfigManager.Instance(Context.OrganizationID).Config == null ||
                Services.ConfigManager.Instance(Context.OrganizationID).Config.intray_customer == false))
                {
                    if (customerCreteria.includeunapproved)
                        query = query.Replace("customer.isactive = true", "(customer.isactive = true or activestatus=1)");
                }

                NpgsqlCommand command = GetCommand(query, conn);
                command.Parameters.Add("userid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = Context.UserID;

                using (System.Data.IDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        CustomerLedger osupplier = new CustomerLedger();
                        osupplier.type = reader["customertype"] == DBNull.Value ? 0 : Convert.ToInt32(reader["customertype"]);
                        osupplier.marginmode = reader["marginmode"] == DBNull.Value ? "" : Convert.ToString(reader["marginmode"]);
                        osupplier.billdiscount = reader["billdiscount"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["billdiscount"]);
                        if (osupplier.marginmode == "mrp")
                            osupplier.marginprice = reader["ccmarginmrp"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["ccmarginmrp"]);
                        else if (osupplier.marginmode == "price")
                            osupplier.marginprice = reader["ccmarginprice"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["ccmarginprice"]);
                        osupplier.id = reader["id"] == DBNull.Value ? 0 : Convert.ToInt64(reader["id"]);
                        osupplier.agentid = reader["agentid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["agentid"]);
                        osupplier.psource = reader["pricesource"] == DBNull.Value ? 0 : Convert.ToInt32(reader["pricesource"]);
                        osupplier.msource = reader["mrpsource"] == DBNull.Value ? 0 : Convert.ToInt32(reader["mrpsource"]);
                        osupplier.name = reader["name"] == DBNull.Value ? "" : reader["name"].ToString();
                        osupplier.city = reader["city"] == DBNull.Value ? "" : reader["city"].ToString();
                        osupplier.address = reader["address"] == DBNull.Value ? "" : reader["address"].ToString();
                        osupplier.gstno = reader["gstno"] == DBNull.Value ? "" : reader["gstno"].ToString();
                        osupplier.stateid = reader["stateid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["stateid"]);
                        osupplier.cityid = reader["cityid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["cityid"]);
                        if(!lst.Exists(x=>x.id == osupplier.id))
                            lst.Add(osupplier);
                    }
                }

                var duplicatenames = lst.GroupBy(x => x.name.Trim().ToLower()).ToList().FindAll(x => x.Count() > 1);
                foreach (var g in duplicatenames)
                {
                    var duplicatecity = g.ToList().GroupBy(x => x.city).Where(y => y.Count() > 1).ToList();
                    if (duplicatecity.Count > 0)
                    {
                        g.ToList().ForEach(x => x.name = x.name + " - " + x.city + "(" + x.address + ")");
                    }
                    else
                    {
                        g.ToList().ForEach(x => x.name = x.name + " " + x.city);
                    }

                }
                conn.Close();
            }
            catch (Exception exp)
            {
                if (conn != null) conn.Close();
                WebAPI.Common.Logger.Instance(Context.OrganizationID).Log(exp, WebAPI.Common.Logger.LogType.Error, exp.StackTrace);
                throw exp;
            }
            return lst;
        }

        public List<CustomerBills> SearchCustomerRewardSummary(Customerbillrequest _customer)
        {
            List<CustomerBills> lst = new List<CustomerBills>();
            NpgsqlConnection conn = CreateConnection();
            StringBuilder sb = new StringBuilder();
            string _SQL = @"select customer.loyeltyno, customer.name, customer.mobileno, customer.customertype,
customer.categoryid, referencelist.name cityname,
sum(rewards) rewards, sum(adjusted) adjusted, sum(rewards - adjusted) balance from customer
  left outer join  customerrewards on customerrewards.customerid = customer.id
  left outer join referencelist on referencelist.id = customer.cityid 
where customer.isactive = true and customerrewards.isactive=true {0}
group by customer.loyeltyno, customer.name, 
customer.mobileno, customer.customertype, customer.categoryid, referencelist.name ";

            try
            {
                conn.Open();
                QueryBuilder qb = new QueryBuilder();
                qb.CurrentQuery = _SQL;

                sb.Append(string.Format(" and (customerrewards.expiry>='{0}' or rewards<0)", DateTimeNow.Date.ToString("yyyy-MM-dd")));
                if (_customer.customerid > 0)
                {
                    sb.Append(string.Format(" and customer.id={0}", _customer.customerid));
                }
                if (!string.IsNullOrEmpty(_customer.mobileno))
                {
                    sb.Append(string.Format(" and customer.mobileno='{0}'", WebAPI.Data.Utility.SafeSQL(_customer.mobileno)));
                }
                if (!string.IsNullOrEmpty(_customer.cardno))
                {
                    sb.Append(string.Format(" and customer.loyeltyno ilike '{0}'", WebAPI.Data.Utility.SafeSQL(_customer.cardno)));
                }
                if (!string.IsNullOrEmpty(_customer.emailid))
                {
                    sb.Append(string.Format(" and customer.emailid ilike '{0}'", WebAPI.Data.Utility.SafeSQL(_customer.emailid)));
                }
                if (!string.IsNullOrEmpty(_customer.customername))
                {
                    sb.Append(string.Format(" and customer.name ilike '%{0}%'", WebAPI.Data.Utility.SafeSQL(_customer.customername)));
                }
                qb.SortField = "customer.name, customer.mobileno";
                qb.CurrentQuery = string.Format(qb.CurrentQuery, sb.ToString());
                var command = qb.GetCommand(conn);
                using (System.Data.IDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        CustomerBills ocustomer = new CustomerBills();
                        ocustomer.customername = reader["name"] == DBNull.Value ? "" : reader["name"].ToString();
                        ocustomer.mobileno = reader["mobileno"] == DBNull.Value ? "" : reader["mobileno"].ToString();
                        ocustomer.loyeltyno = reader["loyeltyno"] == DBNull.Value ? "" : reader["loyeltyno"].ToString();
                        ocustomer.loyelty = reader["rewards"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["rewards"]);
                        ocustomer.adjusted = reader["adjusted"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["adjusted"]);
                        ocustomer.balancepoint = reader["balance"] == DBNull.Value ? 0 : Convert.ToInt64(reader["balance"]);
                        ocustomer.categoryid = reader["categoryid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["categoryid"]);
                        ocustomer.cityname = reader["cityname"] == DBNull.Value ? "" : reader["cityname"].ToString();
                        lst.Add(ocustomer);
                    }
                }
                conn.Close();
            }
            catch (Exception exp)
            {
                if (conn != null) conn.Close();
                WebAPI.Common.Logger.Instance(Context.OrganizationID).Log(exp, WebAPI.Common.Logger.LogType.Error, _customer);
            }
            return lst;
        }
        public List<CustomerBills> SearchCustomerReward(Customerbillrequest _customer)
        {
            if (_customer.issummary)
            {
                return SearchCustomerRewardSummary(_customer);
            }
            List<CustomerBills> lst = new List<CustomerBills>();
            NpgsqlConnection conn = CreateConnection();
            StringBuilder sb = new StringBuilder();
            try
            {
                conn.Open();

                QueryBuilder qb = new QueryBuilder();
                qb.CurrentQuery = SQL_SearchReward;
                qb.AddParameter("customer.isactive", true, "=");
                qb.AddParameter("customerrewards.isactive", true, "=");
                qb.Add2Query("COALESCE(bill.billtype,0) != 9");
                qb.Add2Query("COALESCE(bill.settled,true) = true");

                if (_customer.fromdate != DateTime.MinValue)
                    qb.AddParameter("bill.billdate", _customer.fromdate, ">=");
                else if (string.IsNullOrEmpty(_customer.mobileno))
                    throw new Exception("Please enter start date");

                if (_customer.todate != DateTime.MinValue)
                    qb.AddParameter("bill.billdate", _customer.todate.AddDays(1), "<");
                else if (string.IsNullOrEmpty(_customer.mobileno))
                    throw new Exception("Please enter end date");

                if (_customer.customerid > 0)
                {
                    qb.AddParameter("customer.id", _customer.customerid, "=");
                    sb.Append(string.Format(" and customer.id={0}", _customer.customerid));
                }
                if (!string.IsNullOrEmpty(_customer.mobileno))
                {
                    qb.AddParameter("customer.mobileno", _customer.mobileno, "=");
                    sb.Append(string.Format(" and customer.mobileno='{0}'", WebAPI.Data.Utility.SafeSQL(_customer.mobileno)));
                }
                if (!string.IsNullOrEmpty(_customer.cardno))
                {
                    qb.AddParameter("customer.loyeltyno", _customer.cardno, " ilike ");
                    sb.Append(string.Format(" and customer.loyeltyno ilike '{0}'", WebAPI.Data.Utility.SafeSQL(_customer.cardno)));
                }
                if (!string.IsNullOrEmpty(_customer.emailid))
                {
                    qb.AddParameter("customer.emailid", _customer.emailid, "  ilike ");
                    sb.Append(string.Format(" and customer.emailid ilike '{0}'", WebAPI.Data.Utility.SafeSQL(_customer.emailid)));
                }
                if (!string.IsNullOrEmpty(_customer.customername))
                {
                    qb.AddParameter("customer.name", "%" + _customer.customername + "%", " ilike ");
                    sb.Append(string.Format(" and customer.name ilike '%{0}%'", WebAPI.Data.Utility.SafeSQL(_customer.customername)));
                }
                qb.SortField = "customer.mobileno, customerrewards.id desc";
                qb.CurrentQuery = string.Format(qb.CurrentQuery, sb.ToString());
                var command = qb.GetCommand(conn);
                using (System.Data.IDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        CustomerBills ocustomer = new CustomerBills();
                        ocustomer.id = reader["id"] == DBNull.Value ? 0 : Convert.ToInt64(reader["id"]);
                        ocustomer.locationid = reader["locationid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["locationid"]);
                        ocustomer.customerid = reader["customerid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["customerid"]);
                        ocustomer.customername = reader["name"] == DBNull.Value ? "" : reader["name"].ToString();
                        ocustomer.mobileno = reader["mobileno"] == DBNull.Value ? "" : reader["mobileno"].ToString();
                        ocustomer.billdate = reader["transactiondate"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(reader["transactiondate"]);
                        ocustomer.expiry = reader["expiry"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(reader["expiry"]);
                        ocustomer.billno = reader["billno"] == DBNull.Value ? "" : reader["billno"].ToString();
                        ocustomer.loyelty = reader["rewards"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["rewards"]);
                        ocustomer.adjusted = reader["adjusted"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["adjusted"]);
                        ocustomer.billvalue = reader["billvalue"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["billvalue"]);
                        ocustomer.altered = reader["altered"] == DBNull.Value ? false : Convert.ToBoolean(reader["altered"]);
                        ocustomer.remark = reader["remark"] == DBNull.Value ? "" : reader["remark"].ToString();
                        ocustomer.hasaddlndiscount = (reader["addlndiscount"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["addlndiscount"])) > 0 ? true : false;
                        ocustomer.hasschemediscount = (reader["schdiscount"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["schdiscount"])) > 0 ? true : false;
                        if (ocustomer.expiry.Date < DateTimeNow.Date)
                            ocustomer.expired = ocustomer.loyelty - ocustomer.adjusted;
                        lst.Add(ocustomer);
                    }
                }
                conn.Close();
            }
            catch (Exception exp)
            {
                if (conn != null) conn.Close();
                WebAPI.Common.Logger.Instance(Context.OrganizationID).Log(exp, WebAPI.Common.Logger.LogType.Error, _customer);
            }
            return lst;
        }

        public List<CustomerBills> SearchCustomerBills(Customerbillrequest _customer)
        {
            List<CustomerBills> lst = new List<CustomerBills>();
            NpgsqlConnection conn = CreateConnection();
            try
            {
                conn.Open();

                QueryBuilder qb = new QueryBuilder();
                qb.CurrentQuery = SQL_SearchBills;
                qb.AddParameter("bill.isactive", true, "=");
                qb.Add2Query("bill.billtype!=9");
                if (!string.IsNullOrEmpty(_customer.cardno))
                    qb.AddParameter("customer.loyeltyno", _customer.cardno, "=");
                if (!string.IsNullOrEmpty(_customer.emailid))
                    qb.AddParameter("customer.emailid", _customer.emailid, "=");
                if (_customer.locationid > 0)
                    qb.AddParameter("bill.locationid", _customer.locationid, "=");
                if (_customer.customerid > 0)
                    qb.AddParameter("bill.customerid", _customer.customerid, "=");
                if (_customer.categoryid > 0)
                    qb.AddParameter("customer.categoryid", _customer.categoryid, "=");
                if (!string.IsNullOrEmpty(_customer.mobileno))
                    qb.AddParameter("customer.mobileno", _customer.mobileno, "=");
                if (!string.IsNullOrEmpty(_customer.billno))
                    qb.AddParameter("bill.billno", _customer.billno, "=");
                if (!string.IsNullOrEmpty(_customer.customername))
                    qb.AddParameter("customer.name", "%" + _customer.customername + "%", " ilike ");
                if (_customer.fromprice > 0)
                    qb.Add2Query(" bill.receivable>=" + _customer.fromprice + "");
                if (_customer.toprice > 0)
                    qb.Add2Query(" bill.receivable<=" + _customer.toprice + "");

                if (_customer.fromdate != DateTime.MinValue)
                {
                    qb.AddParameter("bill.billdate", "billdatefrom", _customer.fromdate.Date, ">=");
                }
                if (_customer.todate != DateTime.MinValue)
                {
                    qb.AddParameter("bill.billdate", "billdateto", _customer.todate.AddDays(1).Date, "<");
                }
                qb.SortField = "bill.billdate desc";

                if (_customer.limit > 0)
                    qb.Limit = _customer.limit;
                else if (_customer.fromdate == DateTime.MinValue || _customer.fromdate.Date != _customer.todate.Date)
                    qb.Limit = 2000;

                var command = qb.GetCommand(conn);
                using (System.Data.IDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        CustomerBills ocustomer = new CustomerBills();
                        ocustomer.id = reader["billid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["billid"]);
                        ocustomer.billid = reader["billid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["billid"]);
                        ocustomer.billtype = reader["billtype"] == DBNull.Value ? 0 : Convert.ToInt64(reader["billtype"]);
                        ocustomer.customerid = reader["customerid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["customerid"]);
                        ocustomer.customername = reader["name"] == DBNull.Value ? "" : reader["name"].ToString();
                        ocustomer.gstno = reader["gstno"] == DBNull.Value ? "" : reader["gstno"].ToString();
                        ocustomer.mobileno = reader["mobileno"] == DBNull.Value ? "" : reader["mobileno"].ToString();
                        ocustomer.otherno = reader["otherno"] == DBNull.Value ? "" : reader["otherno"].ToString();
                        ocustomer.categoryid = reader["categoryid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["categoryid"]);
                        ocustomer.locationid = reader["locationid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["locationid"]);
                        ocustomer.counterid = reader["counterid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["counterid"]);
                        ocustomer.billdate = reader["billdate"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(reader["billdate"]);
                        ocustomer.billno = reader["billno"] == DBNull.Value ? "" : reader["billno"].ToString();
                        ocustomer.billvalue = reader["billvalue"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["billvalue"]);
                        ocustomer.isinterstatesale = reader["isinterstatesale"] == DBNull.Value ? false : Convert.ToBoolean(reader["isinterstatesale"]);
                        lst.Add(ocustomer);
                    }
                }
                conn.Close();
            }
            catch (Exception exp)
            {
                if (conn != null) conn.Close();
                WebAPI.Common.Logger.Instance(Context.OrganizationID).Log(exp, WebAPI.Common.Logger.LogType.Error, _customer);
            }
            return lst;
        }
        public List<CustomerBills> SearchRewardBill(Customerbillrequest _customer)
        {
            if (_customer.customerid == 0) return new List<CustomerBills>();
            List<CustomerBills> lst = new List<CustomerBills>();
            NpgsqlConnection conn = CreateConnection();
            try
            {
                conn.Open();
                NpgsqlCommand command = null;
                command = GetCommand(SQL_RewardBill, conn);

                command.Parameters.Add("customerid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = _customer.customerid;
                using (System.Data.IDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        CustomerBills ocustomer = new CustomerBills();
                        ocustomer.billdate = reader["billdate"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(reader["billdate"]);
                        ocustomer.billno = reader["billno"] == DBNull.Value ? "" : reader["billno"].ToString();
                        ocustomer.loyelty = reader["rewards"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["rewards"]);
                        lst.Add(ocustomer);
                    }
                }
                conn.Close();
            }
            catch (Exception exp)
            {
                if (conn != null) conn.Close();
                WebAPI.Common.Logger.Instance(Context.OrganizationID).Log(exp, WebAPI.Common.Logger.LogType.Error, _customer);
            }
            return lst;
        }

        public int DuplicateValidation(Customer oCustomer)
        {
            if (string.IsNullOrEmpty(oCustomer.mobileno)) return 0;

            string _Query = @"select count(id) items from customer where mobileno=@mobileno and
                case when @id=0 then id <> 0 else id!=@id end";
            int count = 0;
            using (NpgsqlConnection conn = CreateConnection())
            {
                conn.Open();
                NpgsqlCommand command = GetCommand(_Query, conn);
                command.Parameters.Add("id", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oCustomer.id;
                command.Parameters.Add("mobileno", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oCustomer.mobileno;
                using (System.Data.IDataReader reader = command.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        count = reader["items"] == DBNull.Value ? 0 : Convert.ToInt32(reader["items"]);
                    }
                }
                conn.Close();
            }
            return count;
        }
        public bool isMigration = false;
        public Customer Insert(Customer oCustomer)
        {

            NpgsqlTransaction transaction = null;
            try
            {
                if (!isMigration && string.IsNullOrEmpty(oCustomer.mobileno) && string.IsNullOrEmpty(oCustomer.otherno))
                {
                    oCustomer.ErrorMessage = "Mobileno/Other no is mandatory.";
                    return oCustomer;
                }

                if (!isMigration && DuplicateValidation(oCustomer) > 0)
                {
                    oCustomer.ErrorMessage = "Mobileno already exists. please check.";
                    return oCustomer;
                }
                if (oCustomer.categoryid > 0 && Context.HasPermission(PermissionSet.hidecustomercat, InfyPOS.Common.Action.View))
                {
                    oCustomer.ErrorMessage = "Don't have permission to create customer with category.";
                    return oCustomer;
                }

                var sequence = SequenceManager.Instance(Context.OrganizationID).Get("customer.code", 0);
                var loyeltysequence = SequenceManager.Instance(Context.OrganizationID).Get("customer.loyeltyno", oCustomer.registredlocationid);
                using (NpgsqlConnection conn = CreateConnection())
                {
                    conn.Open();
                    transaction = conn.BeginTransaction();
                    if (oCustomer.categoryid == 0 && oCustomer.registredlocationid > 0)
                    {
                        if (InfyPOS.Services.ConfigManager.Instance(Context.OrganizationID).LocationConfig != null &&
                            InfyPOS.Services.ConfigManager.Instance(Context.OrganizationID).LocationConfig.ContainsKey(oCustomer.registredlocationid))
                            oCustomer.categoryid = InfyPOS.Services.ConfigManager.Instance(Context.OrganizationID).LocationConfig[oCustomer.registredlocationid].sale_defaultcustomercategory;

                        //oCustomer.categoryid = InfyPOS.Services.ConfigManager.Instance(Context.OrganizationID).Config.sale_defaultcustomercategory;
                    }
                    oCustomer.code = string.IsNullOrEmpty(oCustomer.code) ? sequence.GetNextNumber(Context, conn, transaction) : oCustomer.code;
                    //oCustomer.loyeltyno = string.IsNullOrEmpty(oCustomer.loyeltyno) ? loyeltysequence.GetNextNumber(Context,conn, transaction) : oCustomer.loyeltyno;
                    if (string.IsNullOrEmpty(oCustomer.name)) oCustomer.name = string.Concat("New Customer - ", oCustomer.code);

                    Insert(transaction, conn, oCustomer);

                    if (!isMigration && Services.ConfigManager.Instance(Context.OrganizationID).Config.intray_customer)
                    {
                        Services.IntrayManager.Instance(Context.OrganizationID).Add(Context.OrganizationID, Services.IntrayManager.EntryType.Customer,
                        oCustomer.id, string.Format("New Customer {0} registered.", oCustomer.name),
                        Context.UserID, oCustomer.registredlocationid, transaction, this.Context);
                    }


                    if(oCustomer.regsourceid == -999 && 
                        Context.HasCustomerRestricted)
                    {
                        UsersDataService usersDataService = new UsersDataService() { Context = this.Context };
                        usersDataService.SetMyCustomer(oCustomer,conn, transaction);
                    }
                    transaction.Commit();

                    if (!isMigration)
                    {
                        if (InfyPOS.Services.ConfigManager.Instance(Context.OrganizationID).LocationConfig != null &&
                            InfyPOS.Services.ConfigManager.Instance(Context.OrganizationID).LocationConfig.ContainsKey(oCustomer.registredlocationid))
                        {
                            oCustomer = Search(new CustomerCreteria()
                            {
                                id = oCustomer.id,
                                locationid = oCustomer.registredlocationid
                            }).First();
                        }
                        if (oCustomer.id > 0)
                        {
                            
                            InfyPOS.Processors.CoreDBListener.SendNotification(Context.OrganizationID, new InfyPOS.Processors.Config() { id = oCustomer.id, type = "CACHE", subtype = "CUSTOMER", actionon = oCustomer.modifiedon }, conn);
                            Processors.Schedule_Actions.Instance.OnChange(Context.OrganizationID, oCustomer, Processors.Schedule_Actions.Actions.New);
                        }
                    }
                    conn.Close();
                }
            }
            catch (Exception exp)
            {
                WebAPI.Common.Logger.Instance(Context.OrganizationID).Log(exp, WebAPI.Common.Logger.LogType.Error, oCustomer);
            }
            oCustomer.ErrorMessage = "";
            return oCustomer;
        }

        public Customer Insert(NpgsqlTransaction transaction, NpgsqlConnection conn, Customer oCustomer)
        {
            NpgsqlCommand command = GetCommand(SQL_Insert, conn, transaction);
            if (isMigration)
            {
                oCustomer.isactive = true;
                oCustomer.Activestatus = BaseEntity.ActiveStatus.Approved;
            }
            else
            {
                if (string.IsNullOrEmpty(oCustomer.code))
                {
                    var sequence = SequenceManager.Instance(Context.OrganizationID).Get("customer.code", 0);
                    oCustomer.code = sequence.GetNextNumber(Context, conn, transaction);
                }

                if ((Services.ConfigManager.Instance(Context.OrganizationID).Config == null ||
                    Services.ConfigManager.Instance(Context.OrganizationID).Config.intray_customer == false))
                {
                    oCustomer.isactive = true;
                    oCustomer.Activestatus = BaseEntity.ActiveStatus.Approved;
                }
                else
                {
                    oCustomer.isactive = false;
                    oCustomer.Activestatus = BaseEntity.ActiveStatus.UnApproved;
                }
            }
            command.Parameters.Add("isactive", NpgsqlTypes.NpgsqlDbType.Boolean).Value = oCustomer.isactive;
            command.Parameters.Add("activestatus", NpgsqlTypes.NpgsqlDbType.Integer).Value = oCustomer.activestatus;
            command.Parameters.Add("createdby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = Context.UserID;
            command.Parameters.Add("categoryid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oCustomer.categoryid;
            command.Parameters.Add("modifiedby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = Context.UserID;
            command.Parameters.Add("version", NpgsqlTypes.NpgsqlDbType.Integer).Value = oCustomer.version;
            command.Parameters.Add("createdon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = oCustomer.createdon == DateTime.MinValue ? DateTimeNow : oCustomer.createdon;
            command.Parameters.Add("modifiedon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = DateTimeNow;
            command.Parameters.Add("supportcredit", NpgsqlTypes.NpgsqlDbType.Boolean).Value = oCustomer.supportcredit;
            command.Parameters.Add("creditlimit", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oCustomer.creditlimit;
            command.Parameters.Add("srmaxdays", NpgsqlTypes.NpgsqlDbType.Integer).Value = oCustomer.srmaxdays;
            command.Parameters.Add("creditdays", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oCustomer.creditdays;
            command.Parameters.Add("creditrightsgivenby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oCustomer.creditrightsgivenby;
            command.Parameters.Add("mobileno", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oCustomer.mobileno == null ? "" : oCustomer.mobileno;
            command.Parameters.Add("otherno", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oCustomer.otherno == null ? "" : oCustomer.otherno;
            command.Parameters.Add("emailid", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oCustomer.emailid == null ? "" : oCustomer.emailid;
            command.Parameters.Add("referredby", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oCustomer.referredby == null ? "" : oCustomer.referredby;
            command.Parameters.Add("customertype", NpgsqlTypes.NpgsqlDbType.Integer).Value = oCustomer.customertype;
            command.Parameters.Add("name", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oCustomer.name == null ? "" : oCustomer.name.ToUpper();
            command.Parameters.Add("dateofbirth", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = oCustomer.dateofbirth;
            command.Parameters.Add("marriagedate", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = oCustomer.marriagedate;
            command.Parameters.Add("loyelty", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oCustomer.loyelty;
            command.Parameters.Add("gender", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oCustomer.gender == null ? "" : oCustomer.gender;
            command.Parameters.Add("area", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oCustomer.area == null ? "" : oCustomer.area;
            command.Parameters.Add("areaid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oCustomer.areaid;
            command.Parameters.Add("gstno", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oCustomer.gstno == null ? "" : oCustomer.gstno;

            command.Parameters.Add("supplytype", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oCustomer.supplytype == null ? "" : oCustomer.supplytype;

            command.Parameters.Add("pan", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oCustomer.pan == null ? "" : oCustomer.pan;
            command.Parameters.Add("tan", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oCustomer.tan == null ? "" : oCustomer.tan;

            command.Parameters.Add("married", NpgsqlTypes.NpgsqlDbType.Boolean).Value = oCustomer.married;
            command.Parameters.Add("malekids", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oCustomer.malekids == null ? "" : oCustomer.malekids;
            command.Parameters.Add("femalekids", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oCustomer.femalekids == null ? "" : oCustomer.femalekids;
            command.Parameters.Add("loyeltyno", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oCustomer.loyeltyno == null ? "" : oCustomer.loyeltyno;
            command.Parameters.Add("code", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oCustomer.code == null ? "" : oCustomer.code;
            command.Parameters.Add("address", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oCustomer.address == null ? "" : oCustomer.address;
            command.Parameters.Add("deliveryaddress", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oCustomer.deliveryaddress == null ? "" : oCustomer.deliveryaddress;
            command.Parameters.Add("cityid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oCustomer.cityid;
            command.Parameters.Add("districtid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oCustomer.districtid;
            command.Parameters.Add("preferredtransportid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oCustomer.preferredtransportid;
            command.Parameters.Add("agentid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oCustomer.agentid;
            command.Parameters.Add("billingname", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oCustomer.billingname == null ? "" : oCustomer.billingname;
            command.Parameters.Add("religionid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oCustomer.religionid;

            command.Parameters.Add("pricesource", NpgsqlTypes.NpgsqlDbType.Integer).Value = oCustomer.pricesource;
            command.Parameters.Add("mrpsource", NpgsqlTypes.NpgsqlDbType.Integer).Value = oCustomer.mrpsource;

            command.Parameters.Add("customergroupid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oCustomer.customergroupid;

            if (oCustomer.registredlocationid <= 0)
                command.Parameters.Add("registredlocationid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = DBNull.Value;
            else
                command.Parameters.Add("registredlocationid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oCustomer.registredlocationid;
            command.Parameters.Add("stateid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oCustomer.stateid;
            command.Parameters.Add("countryid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oCustomer.countryid;
            command.Parameters.Add("pincode", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oCustomer.pincode == null ? "" : oCustomer.pincode;
            command.Parameters.Add("contactperson", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oCustomer.contactperson == null ? "" : oCustomer.contactperson;
            command.Parameters.Add("bankaccountname", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oCustomer.bankaccountname == null ? "" : oCustomer.bankaccountname;
            command.Parameters.Add("bankaccountno", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oCustomer.bankaccountno == null ? "" : oCustomer.bankaccountno;
            command.Parameters.Add("bankifci", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oCustomer.bankifci == null ? "" : oCustomer.bankifci;
            command.Parameters.Add("isinterstatesale", NpgsqlTypes.NpgsqlDbType.Boolean).Value = oCustomer.isinterstatesale;
            command.Parameters.Add("family", NpgsqlTypes.NpgsqlDbType.Json).Value = oCustomer.family_json == null ? "null" : oCustomer.family_json;
            command.Parameters.Add("deliveryaddresslist", NpgsqlTypes.NpgsqlDbType.Json).Value = oCustomer.deliveryaddresslist_json == null ? "null" : oCustomer.deliveryaddresslist_json;
            command.Parameters.Add("attachment", NpgsqlTypes.NpgsqlDbType.Json).Value = oCustomer.attachment_json == null ? "null" : oCustomer.attachment_json;
            command.Parameters.Add("disableloyalty", NpgsqlTypes.NpgsqlDbType.Boolean).Value = oCustomer.disableloyalty;
            command.Parameters.Add("portloadingid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oCustomer.portloadingid;
            command.Parameters.Add("portdischargeid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oCustomer.portdischargeid;
            command.Parameters.Add("howoftenid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oCustomer.howoftenid;
            command.Parameters.Add("regsourceid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oCustomer.regsourceid;

            using (System.Data.IDataReader reader = command.ExecuteReader())
            {
                if (reader.Read())
                {
                    oCustomer.id = Convert.ToInt64(reader[0].ToString());
                }
            }
            return oCustomer;
        }

        public Customer Update(Customer oCustomer)
        {
            if (string.IsNullOrEmpty(oCustomer.mobileno) && string.IsNullOrEmpty(oCustomer.otherno))
            {
                oCustomer.ErrorMessage = "Mobile no/Other no is mandatory.";
                return oCustomer;
            }

            if (!isMigration && DuplicateValidation(oCustomer) > 0)
            {
                oCustomer.ErrorMessage = "Mobileno already exists. please check.";
                return oCustomer;
            }
            if (!isMigration && oCustomer.categoryid > 0 && Context.HasPermission(PermissionSet.hidecustomercat, InfyPOS.Common.Action.View))
            {
                oCustomer.ErrorMessage = "Don't have permission to create customer with category.";
                return oCustomer;
            }

            NpgsqlTransaction transaction = null;
            try
            {
                using (NpgsqlConnection conn = CreateConnection())
                {
                    conn.Open();
                    transaction = conn.BeginTransaction();

                    Update(transaction, conn, oCustomer);
                    if (!isMigration && Services.ConfigManager.Instance(Context.OrganizationID).Config.intray_customer)
                    {
                        Services.IntrayManager.Instance(Context.OrganizationID).Add(Context.OrganizationID, Services.IntrayManager.EntryType.Customer,
                        oCustomer.id, string.Format("Customer {0} information updated.", oCustomer.name),
                        Context.UserID, oCustomer.registredlocationid, transaction, this.Context);
                    }

                    transaction.Commit();
                    if (!isMigration && oCustomer.id > 0)
                    {
                        InfyPOS.Processors.CoreDBListener.SendNotification(Context.OrganizationID, new InfyPOS.Processors.Config() { id = oCustomer.id, type = "CACHE", subtype = "CUSTOMER", actionon = oCustomer.modifiedon }, conn);
                    }
                    conn.Close();
                }
            }
            catch (Exception exp)
            {
                WebAPI.Common.Logger.Instance(Context.OrganizationID).Log(exp, WebAPI.Common.Logger.LogType.Error, oCustomer);
            }
            oCustomer.ErrorMessage = "";
            return oCustomer;
        }

        internal bool IntegrateInBound(CustomerIntegration request)
        {
            try
            {
                if (request.customer != null)
                {
                    var existingcustomer = Search(new CustomerCreteria()
                    {
                        mobileno = request.customer.mobileno,
                        loyeltyno = request.customer.id,
                        name = request.customer.name
                    });
                    if (existingcustomer.Count == 0)
                    {
                        existingcustomer.Add(new Customer()
                        {
                            name = request.customer.name,
                            loyeltyno = request.customer.id,
                            mobileno = request.customer.mobileno,
                            isactive = true,
                            version = -99,
                            createdon = DateTimeNow,
                            modifiedon = DateTimeNow,
                            createdby = -99,
                            modifiedby = -99
                        });
                    }
                    if (request.customer.loyelty > 0)
                    {
                        NpgsqlTransaction transaction = null;
                        using (NpgsqlConnection conn = CreateConnection())
                        {
                            conn.Open();
                            transaction = conn.BeginTransaction();
                            AddReward(transaction, conn, existingcustomer.First().id, (int)request.customer.loyelty,
                                DateTime.MaxValue, new Bill() { billno = "Inbound", billdate = DateTimeNow }, 0);
                            transaction.Commit();
                            conn.Close();
                        }
                    }
                    if (request.vouchers != null)
                    {

                        CustomerCouponsDataService voucherdata = new CustomerCouponsDataService() { Context = this.Context };
                        voucherdata.Context = this.Context;

                        NpgsqlTransaction transaction = null;
                        using (NpgsqlConnection conn = CreateConnection())
                        {
                            conn.Open();
                            transaction = conn.BeginTransaction();


                            foreach (var svoucher in request.vouchers)
                            {
                                if (!svoucher.no.StartsWith("#")) svoucher.no = "#" + svoucher.no;
                                var voucher = new CustomerCoupons()
                                {
                                    issuedby = Context.UserID,
                                    issueto = existingcustomer.First().id,
                                    amount = svoucher.issued,
                                    code = svoucher.no,
                                    isactive = true,
                                    vouchertype = CustomerCoupons.CouponType.Coupon,
                                    issuedon = svoucher.issuedon,
                                    validupto = svoucher.expiryon,
                                    minbillvalue = 0,
                                    acceptablelocationid = 0,
                                    unlimited = false,
                                    maxbilldiscountpercentage = 0,
                                    filterschemeid = 0
                                };
                                voucherdata.Insert(transaction, conn, voucher);
                            }
                            transaction.Commit();
                            conn.Close();
                        }
                    }
                }
            }
            catch (Exception exp)
            {
                WebAPI.Common.Logger.Instance(Context.OrganizationID).Log(exp, WebAPI.Common.Logger.LogType.Error, request);
                return false;
            }
            return true;
        }

        internal CustomerIntegrationGroup IntegrateOutBound(CustomerIntegrationGroup request)
        {
            string customerquery = @" select id,name,mobileno,loyeltyno from customer
                                         order by id
                                         limit @limit offset @offset";

            List<CustomerIntegration.Customer> customerlist = new List<CustomerIntegration.Customer>();
            var param = new SortedList<string, object>();
            param.Add("limit", request.pagesize);
            param.Add("offset", request.pageindex * request.pagesize);
            using (var reader = ExecuteReader(customerquery, param))
            {
                while (reader.Read())
                {
                    customerlist.Add(new CustomerIntegration.Customer()
                    {
                        _id = reader.GetInt64(0),
                        name = reader.GetString(1),
                        mobileno = reader.IsDBNull(2) ? "" : reader.GetString(2),
                        id = reader.IsDBNull(3) ? "" : reader.GetString(3)
                    });
                }
            }

            customerquery = @"select customerid,sum(rewards-COALESCE(adjusted,0)) rewards from customerrewards
                                              left outer join bill on bill.id = customerrewards.billid
                                              where customerrewards.isactive = true 
                                              and (bill.id is null or (expiry>=@expiry 
                                              and (COALESCE(bill.settled,true) = true)))
                                              and (bill.id is null or bill.billtype != 9)
                                              and customerrewards.customerid in (" + string.Join(",",
                                              customerlist.ConvertAll(e => e.id)) + ")";

            param.Add("expiry", DateTimeNow);
            using (var reader = ExecuteReader(customerquery, param))
            {
                while (reader.Read())
                {
                    var customerid = reader.GetInt64(0);
                    var rewards = reader.IsDBNull(1) ? 0 : Convert.ToDecimal(reader[1]);
                    customerlist.Find(e => e._id == customerid).loyelty = rewards;
                }
            }

            customerquery = @"select 
                            issueto, amount, validupto,code
                            from vouchers 
                            left outer join coupons on coupons.id = vouchers.coupenid
                            where vouchertype in (1,2) and billid = 0 and vouchers.status = 0
                            and issueto in (" + string.Join(",", customerlist.ConvertAll(e => e.id)) + ")";

            using (var reader = ExecuteReader(customerquery, param))
            {
                while (reader.Read())
                {
                    var customerid = reader.GetInt64(0);
                    var rewards = reader.IsDBNull(1) ? 0 : Convert.ToDecimal(reader[1]);
                    var validupto = reader.IsDBNull(2) ? DateTime.MaxValue : Convert.ToDateTime(reader[2]);
                    var code = reader.IsDBNull(3) ? "" : reader.GetString(3);
                    var customer = customerlist.Find(e => e._id == customerid);
                    if (customer.vouchers == null) customer.vouchers = new List<CustomerIntegration.Vouchers>();
                    customer.vouchers.Add(new CustomerIntegration.Vouchers()
                    {
                        expiryon = validupto,
                        issued = rewards,
                        no = code,
                    });
                }
            }
            request.list = customerlist;
            return request;
        }
        public Customer Update(NpgsqlTransaction transaction, NpgsqlConnection conn, Customer oCustomer)
        {

            NpgsqlCommand command = GetCommand(SQL_Update, conn, transaction);
            command.Parameters.Add("id", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oCustomer.id;
            command.Parameters.Add("categoryid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oCustomer.categoryid;
            command.Parameters.Add("isactive", NpgsqlTypes.NpgsqlDbType.Boolean).Value = oCustomer.isactive;
            command.Parameters.Add("createdby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = Context.UserID;
            command.Parameters.Add("modifiedby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = Context.UserID;
            command.Parameters.Add("version", NpgsqlTypes.NpgsqlDbType.Integer).Value = oCustomer.version;
            command.Parameters.Add("createdon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = oCustomer.createdon;
            command.Parameters.Add("modifiedon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = DateTimeNow;
            command.Parameters.Add("supportcredit", NpgsqlTypes.NpgsqlDbType.Boolean).Value = oCustomer.supportcredit;
            command.Parameters.Add("srmaxdays", NpgsqlTypes.NpgsqlDbType.Integer).Value = oCustomer.srmaxdays;
            command.Parameters.Add("creditlimit", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oCustomer.creditlimit;
            command.Parameters.Add("creditdays", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oCustomer.creditdays;
            command.Parameters.Add("creditrightsgivenby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oCustomer.creditrightsgivenby;
            command.Parameters.Add("referredby", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oCustomer.referredby == null ? "" : oCustomer.referredby;
            command.Parameters.Add("mobileno", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oCustomer.mobileno == null ? "" : oCustomer.mobileno;
            command.Parameters.Add("otherno", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oCustomer.otherno == null ? "" : oCustomer.otherno;
            command.Parameters.Add("emailid", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oCustomer.emailid == null ? "" : oCustomer.emailid;
            command.Parameters.Add("customertype", NpgsqlTypes.NpgsqlDbType.Integer).Value = oCustomer.customertype;
            command.Parameters.Add("name", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oCustomer.name == null ? "" : oCustomer.name;
            command.Parameters.Add("dateofbirth", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = oCustomer.dateofbirth;
            command.Parameters.Add("marriagedate", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = oCustomer.marriagedate;
            command.Parameters.Add("loyelty", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oCustomer.loyelty;
            command.Parameters.Add("preferredtransportid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oCustomer.preferredtransportid;
            command.Parameters.Add("agentid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oCustomer.agentid;
            command.Parameters.Add("gender", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oCustomer.gender == null ? "" : oCustomer.gender;
            command.Parameters.Add("area", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oCustomer.area == null ? "" : oCustomer.area;
            command.Parameters.Add("areaid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oCustomer.areaid;
            command.Parameters.Add("gstno", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oCustomer.gstno == null ? "" : oCustomer.gstno;
            command.Parameters.Add("supplytype", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oCustomer.supplytype == null ? "" : oCustomer.supplytype;
            command.Parameters.Add("pan", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oCustomer.pan == null ? "" : oCustomer.pan;
            command.Parameters.Add("tan", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oCustomer.tan == null ? "" : oCustomer.tan;
            command.Parameters.Add("married", NpgsqlTypes.NpgsqlDbType.Boolean).Value = oCustomer.married;
            command.Parameters.Add("malekids", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oCustomer.malekids == null ? "" : oCustomer.malekids;
            command.Parameters.Add("femalekids", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oCustomer.femalekids == null ? "" : oCustomer.femalekids;
            command.Parameters.Add("loyeltyno", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oCustomer.loyeltyno == null ? "" : oCustomer.loyeltyno;
            command.Parameters.Add("code", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oCustomer.code == null ? "" : oCustomer.code;
            command.Parameters.Add("address", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oCustomer.address == null ? "" : oCustomer.address;
            command.Parameters.Add("deliveryaddress", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oCustomer.deliveryaddress == null ? "" : oCustomer.deliveryaddress;
            command.Parameters.Add("cityid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oCustomer.cityid;
            command.Parameters.Add("districtid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oCustomer.districtid;
            command.Parameters.Add("stateid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oCustomer.stateid;
            command.Parameters.Add("countryid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oCustomer.countryid;
            command.Parameters.Add("pincode", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oCustomer.pincode == null ? "" : oCustomer.pincode;
            command.Parameters.Add("contactperson", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oCustomer.contactperson == null ? "" : oCustomer.contactperson;
            command.Parameters.Add("bankaccountname", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oCustomer.bankaccountname == null ? "" : oCustomer.bankaccountname;
            command.Parameters.Add("bankaccountno", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oCustomer.bankaccountno == null ? "" : oCustomer.bankaccountno;
            command.Parameters.Add("bankifci", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oCustomer.bankifci == null ? "" : oCustomer.bankifci;
            command.Parameters.Add("billingname", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oCustomer.billingname == null ? "" : oCustomer.billingname;
            command.Parameters.Add("religionid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oCustomer.religionid;
            command.Parameters.Add("pricesource", NpgsqlTypes.NpgsqlDbType.Integer).Value = oCustomer.pricesource;
            command.Parameters.Add("mrpsource", NpgsqlTypes.NpgsqlDbType.Integer).Value = oCustomer.mrpsource;
            command.Parameters.Add("isinterstatesale", NpgsqlTypes.NpgsqlDbType.Boolean).Value = oCustomer.isinterstatesale;
            command.Parameters.Add("customergroupid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oCustomer.customergroupid;
            if (oCustomer.registredlocationid <= 0)
                command.Parameters.Add("registredlocationid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = DBNull.Value;
            else
                command.Parameters.Add("registredlocationid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oCustomer.registredlocationid;
            command.Parameters.Add("family", NpgsqlTypes.NpgsqlDbType.Json).Value = oCustomer.family_json == null ? "null" : oCustomer.family_json;
            command.Parameters.Add("deliveryaddresslist", NpgsqlTypes.NpgsqlDbType.Json).Value = oCustomer.deliveryaddresslist_json == null ? "null" : oCustomer.deliveryaddresslist_json;
            command.Parameters.Add("attachment", NpgsqlTypes.NpgsqlDbType.Json).Value = oCustomer.attachment_json == null ? "null" : oCustomer.attachment_json;
            command.Parameters.Add("disableloyalty", NpgsqlTypes.NpgsqlDbType.Boolean).Value = oCustomer.disableloyalty;
            command.Parameters.Add("portloadingid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oCustomer.portloadingid;
            command.Parameters.Add("portdischargeid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oCustomer.portdischargeid;
            command.Parameters.Add("howoftenid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oCustomer.howoftenid;
            command.Parameters.Add("regsourceid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oCustomer.regsourceid;

            using (System.Data.IDataReader reader = command.ExecuteReader())
            {
                if (reader.Read())
                {
                    oCustomer.version = Convert.ToInt32(reader[0].ToString());
                }
            }

            return oCustomer;
        }

        public List<Customer> SearchBirthday(CustomerCreteria _reportcreteria)
        {
            List<Customer> lst = new List<Customer>();
            DateTime today = _reportcreteria.fromdate.Date;
            DateTime nextdate = _reportcreteria.todate.Date;
            NpgsqlConnection conn = CreateConnection();
            try
            {
                conn.Open();

                NpgsqlCommand command = null;

                if (_reportcreteria.ismarriageday)
                    command = GetCommand(SQL_Marriageday, conn);
                else
                    command = GetCommand((_reportcreteria.forkids) ? SQL_KidBirthday : SQL_Birthday, conn);
                command.Parameters.Add("fromdate", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = today;
                command.Parameters.Add("todate", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = nextdate;
                using (System.Data.IDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        Customer oitem = new Customer();
                        oitem.mobileno = reader["mobileno"] == DBNull.Value ? "" : reader["mobileno"].ToString();
                        oitem.name = reader["name"] == DBNull.Value ? "" : reader["name"].ToString();
                        oitem.gender = reader["gender"] == DBNull.Value ? "" : reader["gender"].ToString();
                        oitem.dateofbirth = reader["dateofbirth"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(reader["dateofbirth"]);
                        oitem.marriagedate = reader["marriagedate"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(reader["marriagedate"]);
                        oitem.gstno = reader["gstno"] == DBNull.Value ? "" : reader["gstno"].ToString();
                        oitem.pincode = reader["pincode"] == DBNull.Value ? "" : reader["pincode"].ToString();
                        oitem.address = reader["address"] == DBNull.Value ? "" : reader["address"].ToString();
                        oitem.loyelty = reader["loyelty"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["loyelty"]);
                        oitem.area = reader["area"] == DBNull.Value ? "" : reader["area"].ToString();
                        lst.Add(oitem);
                    }
                }
                conn.Close();
            }
            catch (Exception exp)
            {
                if (conn != null) conn.Close();
                WebAPI.Common.Logger.Instance(Context.OrganizationID).Log(exp, WebAPI.Common.Logger.LogType.Error, _reportcreteria);
            }
            return lst;
        }

        public List<SMSMessageLog> SearchSMSLog(SMSMessageLog _reportcreteria)
        {
            List<SMSMessageLog> lst = new List<SMSMessageLog>();
            NpgsqlConnection conn = CreateConnection();

            try
            {
                conn.Open();
                NpgsqlCommand command = null;

                QueryBuilder qb = new QueryBuilder();
                qb.CurrentQuery = SQL_SMSLog;

                if (!string.IsNullOrEmpty(_reportcreteria.taskname))
                    qb.AddParameter("schedule.name", _reportcreteria.taskname, "=");
                if (_reportcreteria.fromdate != DateTime.MinValue && _reportcreteria.todate != DateTime.MinValue)
                {
                    qb.AddParameter("actionon", "datefrom", _reportcreteria.fromdate.Date, ">=");
                    qb.AddParameter("actionon", "dateto", _reportcreteria.todate.Date.AddDays(1), "<");
                }

                qb.SortField = "actionon desc";
                qb.Limit = 1000;
                command = qb.GetCommand(conn);
                using (System.Data.IDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        SMSMessageLog oitem = new SMSMessageLog();
                        oitem.mobileno = reader["mobileno"] == DBNull.Value ? "" : reader["mobileno"].ToString();
                        oitem.name = reader["name"] == DBNull.Value ? "" : reader["name"].ToString();
                        oitem.actionon = reader["actionon"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(reader["actionon"]);
                        oitem.description = reader["description"] == DBNull.Value ? "" : reader["description"].ToString();
                        oitem.message = reader["message"] == DBNull.Value ? "" : reader["message"].ToString();
                        lst.Add(oitem);
                    }
                }
                conn.Close();
            }
            catch (Exception exp)
            {
                if (conn != null) conn.Close();
                WebAPI.Common.Logger.Instance(Context.OrganizationID).Log(exp, WebAPI.Common.Logger.LogType.Error, _reportcreteria);
            }
            return lst;
        }

        public List<Customer> SearchMyCustomer(CustomerCreteria _reportcreteria)
        {
            List<Customer> lst = new List<Customer>();
            NpgsqlConnection conn = CreateConnection();
            try
            {
                conn.Open();
                NpgsqlCommand command = null;

                StringBuilder SQL_Where = new StringBuilder();
                QueryBuilder qb = new QueryBuilder();
                if (_reportcreteria.productid > 0 || _reportcreteria.sizeid > 0 || _reportcreteria.brandid > 0)
                    qb.CurrentQuery = SQL_MyCustomerbyPurchase;
                else
                    qb.CurrentQuery = SQL_MyCustomer;

                if (_reportcreteria.locationid > 0)
                    SQL_Where.Append(" and bill.locationid=" + _reportcreteria.locationid);
                if (_reportcreteria.floorid > 0)
                    SQL_Where.Append(" and bill.floorid=" + _reportcreteria.floorid);
                if (_reportcreteria.counterid > 0)
                    SQL_Where.Append(" and bill.counterid=" + _reportcreteria.counterid);

                if (_reportcreteria.fromprice > 0)
                    SQL_Where.Append(" and bill.receivable>=" + _reportcreteria.fromprice + "");
                if (_reportcreteria.toprice > 0)
                    SQL_Where.Append(" and bill.receivable<=" + _reportcreteria.toprice + "");
                if (_reportcreteria.fromdate != DateTime.MinValue && _reportcreteria.todate != DateTime.MinValue)
                {
                    SQL_Where.Append(" and bill.billdate>='" + _reportcreteria.fromdate.Date.ToString("yyyy-MM-dd")
                        + "' and bill.billdate<'" + _reportcreteria.todate.AddDays(1).Date.ToString("yyyy-MM-dd") + "'");
                }
                if (_reportcreteria.productid > 0)
                    SQL_Where.Append(" and stock.productid=" + _reportcreteria.productid);
                if (_reportcreteria.brandid > 0)
                    SQL_Where.Append(" and items.brandid=" + _reportcreteria.brandid);
                if (_reportcreteria.sizeid > 0)
                    SQL_Where.Append(" and items.sizeid=" + _reportcreteria.sizeid);

                if (_reportcreteria.frompoint > 0)
                    qb.Add2Query(" customer.loyelty>=" + _reportcreteria.frompoint + "");
                if (_reportcreteria.topoint > 0)
                    qb.Add2Query(" customer.loyelty<=" + _reportcreteria.topoint + "");
                if (_reportcreteria.cityid > 0)
                    qb.AddParameter("customer.cityid", _reportcreteria.cityid, "=");

                if (_reportcreteria.areaid > 0)
                    qb.AddParameter("customer.areaid", _reportcreteria.areaid, "=");
                if (_reportcreteria.areagroup1id > 0)
                    qb.AddParameter("customerarea.saleareagroup1", _reportcreteria.areagroup1id, " = ");
                if (_reportcreteria.areagroup2id > 0)
                    qb.AddParameter("customerarea.saleareagroup2", _reportcreteria.areagroup2id, " = ");

                if (_reportcreteria.religionid > 0)
                    qb.AddParameter("customer.religionid", _reportcreteria.religionid, "=");
                if (_reportcreteria.customergroupid > 0)
                    qb.AddParameter("customer.customergroupid", _reportcreteria.customergroupid, "=");
                if (_reportcreteria.registredlocationid > 0)
                {
                    qb.AddParameter("COALESCE(registredlocationid,@registredlocationid)", "registredlocationid", _reportcreteria.registredlocationid, "=");
                }
                if (_reportcreteria.dobfromdate != DateTime.MinValue && _reportcreteria.dobtodate != DateTime.MinValue)
                {
                    qb.Add2Query(" dateofbirth > '1904-01-01' ");
                    qb.AddParameter("('1904-' || to_char(dateofbirth, 'MM-dd'))::date", "@dateofbirthfrom", _reportcreteria.dobfromdate, ">=", "('1904-' || to_char(@dateofbirthfrom::date, 'MM-dd'))::date");
                    qb.AddParameter("('1904-' || to_char(dateofbirth, 'MM-dd'))::date", "@dateofbirthto", _reportcreteria.dobtodate, "<=", "('1904-' || to_char(@dateofbirthto::date, 'MM-dd'))::date");
                }
                if (_reportcreteria.customertype > 0)
                    qb.AddParameter("customer.customertype", _reportcreteria.customertype, "=");
                if (_reportcreteria.categoryid > 0)
                    qb.AddParameter("customer.categoryid", _reportcreteria.categoryid, "=");

                if (_reportcreteria.withaddress)
                    qb.Add2Query(" length(customer.address)>10");
                if (_reportcreteria.withoutaddress)
                    qb.Add2Query(" length(customer.address)<10");

                if(SQL_Where.ToString() != "")
                {
                    qb.CurrentQuery = qb.CurrentQuery.Replace("left outer join billdetails on", "inner join billdetails on");
                }
                qb.CurrentQuery = string.Format(qb.CurrentQuery, SQL_Where);
                command = qb.GetCommand(conn);

                //qb.Limit = 10;

                using (System.Data.IDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        CustomerCreteria ostock = new CustomerCreteria();
                        ostock.mobileno = reader["mobileno"] == DBNull.Value ? "" : reader["mobileno"].ToString();
                        ostock.name = reader["name"] == DBNull.Value ? "" : reader["name"].ToString();
                        ostock.gender = reader["gender"] == DBNull.Value ? "" : reader["gender"].ToString();
                        ostock.dateofbirth = reader["dateofbirth"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(reader["dateofbirth"]);
                        ostock.marriagedate = reader["marriagedate"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(reader["marriagedate"]);
                        ostock.gstno = reader["gstno"] == DBNull.Value ? "" : reader["gstno"].ToString();
                        ostock.pincode = reader["pincode"] == DBNull.Value ? "" : reader["pincode"].ToString();
                        ostock.address = reader["address"] == DBNull.Value ? "" : reader["address"].ToString();
                        ostock.loyelty = reader["loyelty"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["loyelty"]);
                        ostock.cityid = reader["cityid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["cityid"]);
                        ostock.cityname = reader["cityname"] == DBNull.Value ? "" : reader["cityname"].ToString();
                        ostock.area = reader["area"] == DBNull.Value ? "" : reader["area"].ToString();
                        ostock.billvalue = reader["billvalue"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["billvalue"]);
                        ostock.totalbills = reader["billnos"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["billnos"]);
                        ostock.billingname = reader["billingname"] == DBNull.Value ? "" : reader["billingname"].ToString();
                        ostock.religionid = reader["religionid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["religionid"]);

                        lst.Add(ostock);
                    }
                }
                conn.Close();
            }
            catch (Exception exp)
            {
                if (conn != null) conn.Close();
                WebAPI.Common.Logger.Instance(Context.OrganizationID).Log(exp, WebAPI.Common.Logger.LogType.Error, _reportcreteria);
            }
            return lst;
        }
        public List<Customer> SearchTopCustomers(CustomerCreteria _reportcreteria)
        {
            List<Customer> lst = new List<Customer>();
            NpgsqlConnection conn = CreateConnection();
            try
            {
                conn.Open();
                NpgsqlCommand command = null;

                QueryBuilder qb = new QueryBuilder();
                qb.CurrentQuery = SQL_HighValued;

                if (_reportcreteria.locationid > 0)
                    qb.AddParameter("bill.locationid", _reportcreteria.locationid, "=");
                if (_reportcreteria.fromdate.Year > 1900)
                    qb.AddParameter("bill.billdate", "fromdate", _reportcreteria.fromdate.Date, ">=");
                if (_reportcreteria.todate.Year > 1900)
                    qb.AddParameter("bill.billdate", "todate", _reportcreteria.todate.Date.AddDays(1), "<");
                if (!string.IsNullOrEmpty(_reportcreteria.area))
                {
                    qb.AddParameter("area", _reportcreteria.area, "=");
                }
                qb.CurrentQuery += @" group by customer.name, customer.mobileno, customer.area
                                      order by customer.name, customer.mobileno, customer.area";

                command = qb.GetCommand(conn);

                //qb.Limit = 10;

                using (System.Data.IDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        CustomerCreteria ostock = new CustomerCreteria();
                        ostock.name = reader["name"] == DBNull.Value ? "" : reader["name"].ToString();
                        ostock.area = reader["area"] == DBNull.Value ? "" : reader["area"].ToString();

                        ostock.mobileno = reader["mobileno"] == DBNull.Value ? "" : reader["mobileno"].ToString();
                        ostock.billvalue = reader["billvalue"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["billvalue"]);
                        ostock.totalbills = reader["billnos"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["billnos"]);

                        lst.Add(ostock);
                    }
                }
                conn.Close();
            }
            catch (Exception exp)
            {
                if (conn != null) conn.Close();
                WebAPI.Common.Logger.Instance(Context.OrganizationID).Log(exp, WebAPI.Common.Logger.LogType.Error, _reportcreteria);
            }
            return lst;
        }

        public List<Customer> SearchInactiveCustomers(CustomerCreteria _reportcreteria)
        {
            List<Customer> lst = new List<Customer>();
            NpgsqlConnection conn = CreateConnection();
            try
            {
                conn.Open();
                NpgsqlCommand command = null;

                QueryBuilder qb = new QueryBuilder();
                qb.CurrentQuery = SQL_InactiveCustomer;
                StringBuilder SQL_Where = new StringBuilder();
                StringBuilder SQL_Where1 = new StringBuilder();

                if (_reportcreteria.locationid > 0)
                {
                    SQL_Where.Append(" and bill.locationid=" + _reportcreteria.locationid);
                    SQL_Where1.Append(" and bill.locationid=" + _reportcreteria.locationid);
                }
                if (_reportcreteria.fromprice > 0)
                    SQL_Where.Append(" and bill.receivable>=" + _reportcreteria.fromprice + "");
                if (_reportcreteria.toprice > 0)
                    SQL_Where.Append(" and bill.receivable<=" + _reportcreteria.toprice + "");

                if (_reportcreteria.fromdate != DateTime.MinValue && _reportcreteria.todate != DateTime.MinValue)
                {
                    SQL_Where.Append(" and bill.billdate>='" + _reportcreteria.fromdate.Date.ToString("yyyy-MM-dd")
                        + "' and bill.billdate<'" + _reportcreteria.todate.AddDays(1).Date.ToString("yyyy-MM-dd") + "'");
                }
                if (_reportcreteria.inactivefrom != DateTime.MinValue && _reportcreteria.inactiveto != DateTime.MinValue)
                {
                    SQL_Where1.Append(" and bill.billdate>='" + _reportcreteria.inactivefrom.Date.ToString("yyyy-MM-dd")
                        + "' and bill.billdate<'" + _reportcreteria.inactiveto.AddDays(1).Date.ToString("yyyy-MM-dd") + "'");
                }

                qb.CurrentQuery = string.Format(SQL_InactiveCustomer, SQL_Where, SQL_Where1);
                command = qb.GetCommand(conn);
                using (System.Data.IDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        CustomerCreteria ostock = new CustomerCreteria();
                        ostock.mobileno = reader["mobileno"] == DBNull.Value ? "" : reader["mobileno"].ToString();
                        ostock.name = reader["name"] == DBNull.Value ? "" : reader["name"].ToString();
                        ostock.gender = reader["gender"] == DBNull.Value ? "" : reader["gender"].ToString();
                        ostock.dateofbirth = reader["dateofbirth"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(reader["dateofbirth"]);
                        ostock.marriagedate = reader["marriagedate"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(reader["marriagedate"]);
                        ostock.gstno = reader["gstno"] == DBNull.Value ? "" : reader["gstno"].ToString();
                        ostock.pincode = reader["pincode"] == DBNull.Value ? "" : reader["pincode"].ToString();
                        ostock.address = reader["address"] == DBNull.Value ? "" : reader["address"].ToString();
                        ostock.loyelty = reader["loyelty"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["loyelty"]);
                        ostock.cityid = reader["cityid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["cityid"]);
                        ostock.cityname = reader["cityname"] == DBNull.Value ? "" : reader["cityname"].ToString();
                        ostock.area = reader["area"] == DBNull.Value ? "" : reader["area"].ToString();
                        ostock.billingname = reader["billingname"] == DBNull.Value ? "" : reader["billingname"].ToString();
                        ostock.religionid = reader["religionid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["religionid"]);
                        ostock.billvalue = reader["activebillvalue"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["activebillvalue"]);
                        ostock.totalbills = reader["activebillnos"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["activebillnos"]);
                        ostock.billvalue2 = reader["inactivebillvalue"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["inactivebillvalue"]);
                        ostock.totalbills2 = reader["inactivebillnos"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["inactivebillnos"]);

                        lst.Add(ostock);
                    }
                }
                conn.Close();
            }
            catch (Exception exp)
            {
                if (conn != null) conn.Close();
                WebAPI.Common.Logger.Instance(Context.OrganizationID).Log(exp, WebAPI.Common.Logger.LogType.Error, _reportcreteria);
            }
            return lst;
        }

        public bool Delete(Customer oCustomer)
        {

            NpgsqlTransaction transaction = null;
            bool hasSuccessCount = false;
            try
            {
                using (NpgsqlConnection conn = CreateConnection())
                {
                    conn.Open();
                    transaction = conn.BeginTransaction();
                    hasSuccessCount = Delete(transaction, conn, oCustomer, false);
                    transaction.Commit();
                    conn.Close();
                }
            }
            catch (Exception exp)
            {
                WebAPI.Common.Logger.Instance(Context.OrganizationID).Log(exp, WebAPI.Common.Logger.LogType.Error, oCustomer);



            }
            return hasSuccessCount;

        }

        //UPDATE public.customer SET modifiedby=@modifiedby, version=version+1, modifiedon=@modifiedon, loyelty=loyelty+@loyelty where id=@id RETURNING loyelty

        public decimal AddReward(NpgsqlTransaction transaction, NpgsqlConnection conn, long customerid, decimal reward,
            DateTime expiry,
            Billsettlementmaster billsettlementmaster, decimal adjustable)
        {
            CustomerrewardsDataService customerrewardsDataService = new CustomerrewardsDataService() { Context = this.Context };
            var customerreward = customerrewardsDataService.Insert(transaction, conn, new Customerrewards()
            {
                customerid = customerid,
                rewards = reward,
                billvalue = billsettlementmaster.receivable,
                expiry = expiry,
                transactiondate = billsettlementmaster.settlementon,
                version = 1,
                locationid = billsettlementmaster.locationid,
                billid = billsettlementmaster.Billsettlement.First().billid,
                billno = string.Join(",", billsettlementmaster.Billsettlement.ConvertAll<string>(e => e.billno)),
                createdby = Context.UserID,
                modifiedby = Context.UserID,
                createdon = DateTimeNow,
                isactive = true,
                modifiedon = DateTimeNow,
                adjusted = (adjustable > 0) ? reward : 0
            });

            NpgsqlCommand command = GetCommand(SQL_UpdateReward, conn, transaction);
            command.Parameters.Add("modifiedby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = Context.UserID;
            command.Parameters.Add("modifiedon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = DateTimeNow;
            command.Parameters.Add("loyelty", NpgsqlTypes.NpgsqlDbType.Numeric).Value = reward - adjustable;
            command.Parameters.Add("id", NpgsqlTypes.NpgsqlDbType.Bigint).Value = customerid;

            using (System.Data.IDataReader reader = command.ExecuteReader())
            {
                if (reader.Read())
                {
                    return reader[0] == DBNull.Value ? 0 : Convert.ToDecimal(reader[0]);
                }
            }
            return 0;
        }

        public decimal AddReward(NpgsqlTransaction transaction, NpgsqlConnection conn, long customerid, int reward,
    DateTime expiry,
    Bill bill, long adjustable)
        {
            CustomerrewardsDataService customerrewardsDataService = new CustomerrewardsDataService() { Context = this.Context };
            var customerreward = customerrewardsDataService.Insert(transaction, conn, new Customerrewards()
            {
                customerid = customerid,
                rewards = reward,
                billvalue = bill.receivable,
                expiry = expiry,
                transactiondate = bill.billdate,
                version = 1,
                locationid = bill.locationid,
                billid = bill.id,
                billno = bill.billno,
                createdby = Context.UserID,
                modifiedby = Context.UserID,
                createdon = DateTimeNow,
                isactive = true,
                modifiedon = DateTimeNow,
                adjusted = (adjustable > 0) ? reward : 0
            });

            NpgsqlCommand command = GetCommand(SQL_UpdateReward, conn, transaction);
            command.Parameters.Add("modifiedby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = Context.UserID;
            command.Parameters.Add("modifiedon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = DateTimeNow;
            command.Parameters.Add("loyelty", NpgsqlTypes.NpgsqlDbType.Numeric).Value = reward - adjustable;
            command.Parameters.Add("id", NpgsqlTypes.NpgsqlDbType.Bigint).Value = customerid;

            using (System.Data.IDataReader reader = command.ExecuteReader())
            {
                if (reader.Read())
                {
                    return Convert.ToDecimal(reader[0].ToString());
                }
            }
            return 0;
        }

        internal string Send2CRM(List<Customer> items)
        {
            int successcount = 0;
            int failurecount = 0;
            StringBuilder stringBuilder = new StringBuilder();
            foreach (var item in items)
            {
                try
                {
                    if (InfyPOS.Services.ConfigManager.Instance(this.Context.OrganizationID).Config.intray_notification_crm)
                        Processors.Schedule_Actions.Instance.OnChange(this.Context.OrganizationID, item, Processors.Schedule_Actions.Actions.CRM);
                    successcount++;
                }
                catch (Exception exp)
                {
                    stringBuilder.AppendLine(exp.ToString());
                    failurecount++;
                }
            }
            return "Success " + successcount + ",Failure " + failurecount + "\r\n" + stringBuilder.ToString();
        }
        internal bool Activate(Customer _customer)
        {
            try
            {
                using (NpgsqlConnection conn = CreateConnection())
                {
                    conn.Open();

                    NpgsqlCommand command = GetCommand("update customer set isactive=true,modifiedby=@modifiedby,modifiedon=@modifiedon where id=@id", conn);
                    command.Parameters.Add("id", NpgsqlTypes.NpgsqlDbType.Bigint).Value = _customer.id;
                    command.Parameters.Add("modifiedby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = Context.UserID;
                    command.Parameters.Add("modifiedon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = DateTimeNow;
                    int successCount = command.ExecuteNonQuery();
                    conn.Close();
                    return successCount > 0;
                }
            }
            catch (Exception exp)
            {
                return false;
            }
        }
        internal bool ChangeStatus(ChangeAttributes item)
        {

            try
            {
                using (NpgsqlConnection conn = CreateConnection())
                {
                    conn.Open();

                    NpgsqlCommand command = GetCommand("update customer set activestatus=@activestatus,modifiedby=@modifiedby,modifiedon=@modifiedon where id in (" + string.Join(",", item.ids) + ")", conn);
                    command.Parameters.Add("activestatus", NpgsqlTypes.NpgsqlDbType.Bigint).Value = item.statusid;
                    command.Parameters.Add("modifiedby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = Context.UserID;
                    command.Parameters.Add("modifiedon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = DateTimeNow;
                    int successCount = command.ExecuteNonQuery();
                    conn.Close();
                    return successCount > 0;
                }
            }
            catch (Exception exp)
            {
                return false;
            }
        }

        internal bool Marge(MergeCustomerList item)
        {
            Customer _newitem = null;
            if (item.id == 0)
            {
                item.ErrorMessage = "There is no Item selected as Merge To";
                return false;
            }
            NpgsqlTransaction transaction = null;
            try
            {
                if (!item.deletelist.Exists(e => e.id == item.id))
                {
                    item.ErrorMessage = "Merge To Item must be selected from master list of items.";
                    return false;
                }
                if (item.deletelist.Exists(e => e.id == item.id))
                    _newitem = item.deletelist.Find(e => e.id == item.id);

                if (_newitem == null)
                {
                    item.ErrorMessage = "Merge To Item must be selected from master list of items.";
                    return false;
                }
                using (NpgsqlConnection conn = CreateConnection())
                {
                    conn.Open();
                    transaction = conn.BeginTransaction();
                    item.deletelist.RemoveAll(e => e.id == item.id);
                    foreach (var ditem in item.deletelist)
                    {
                        Delete(transaction, conn, ditem, true);
                    }

                    string query = @"update customerrewards set altered=true,remark=customerid,customerid=@newid where customerid=@oldid;";

                    NpgsqlCommand command = new NpgsqlCommand(query, conn, transaction);
                    command.Parameters.Add("newid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = item.id;
                    command.Parameters.Add("oldid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = 0;
                    foreach (var ditem in item.deletelist)
                    {
                        command.Parameters["oldid"].Value = ditem.id;
                        int successCount = command.ExecuteNonQuery();
                    }
                    transaction.Commit();
                    conn.Close();
                }
            }
            catch (Exception exp)
            {
                WebAPI.Common.Logger.Instance(Context.OrganizationID).Log(exp, WebAPI.Common.Logger.LogType.Error, exp.StackTrace);
                return false;
            }
            return true;
        }

        public bool Delete(NpgsqlTransaction transaction, NpgsqlConnection conn, Customer oCustomer, bool isMask)
        {

            NpgsqlCommand command = GetCommand(isMask ? SQL_DeleteWith : SQL_Delete, conn, transaction);
            command.Parameters.Add("isactive", NpgsqlTypes.NpgsqlDbType.Boolean).Value = oCustomer.isactive;
            command.Parameters.Add("modifiedby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = Context.UserID;
            command.Parameters.Add("modifiedon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = DateTimeNow;
            command.Parameters.Add("version", NpgsqlTypes.NpgsqlDbType.Integer).Value = oCustomer.version;
            command.Parameters.Add("id", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oCustomer.id;
            int successCount = command.ExecuteNonQuery();

            return successCount > 0;
        }

        internal List<CustomerLedger> SearchFranchise()
        {
            List<CustomerLedger> lst = new List<CustomerLedger>();
            NpgsqlConnection conn = CreateConnection();
            try
            {
                conn.Open();
                string query = @"select customer.id,customer.name,gstno,address,stateid,cityid,customertype,
                cast(referencelist.additionalinfo->> 'billdiscount' as numeric) billdiscount,citylist.name city,
                referencelist.additionalinfo->> 'marginmode' marginmode,pricesource,mrpsource,customer.agentid,
                cast(referencelist.additionalinfo->> 'ccmarginmrp' as numeric) ccmarginmrp,
                cast(referencelist.additionalinfo->> 'ccmarginprice' as numeric) ccmarginprice from customer
                left outer join referencelist on referencelist.id = customer.categoryid
                left outer join referencelist citylist on citylist.id = customer.cityid
                where customer.isactive = true and customertype = 6 and registredlocationid in ({0})
                order by customer.name";

                NpgsqlCommand command = GetCommand(string.Format(query, String.Join(",", Context.accessiblelocation)), conn);

                using (System.Data.IDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        CustomerLedger osupplier = new CustomerLedger();
                        osupplier.type = reader["customertype"] == DBNull.Value ? 0 : Convert.ToInt32(reader["customertype"]);
                        osupplier.marginmode = reader["marginmode"] == DBNull.Value ? "" : Convert.ToString(reader["marginmode"]);
                        osupplier.billdiscount = reader["billdiscount"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["billdiscount"]);
                        if (osupplier.marginmode == "mrp")
                            osupplier.marginprice = reader["ccmarginmrp"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["ccmarginmrp"]);
                        else if (osupplier.marginmode == "price")
                            osupplier.marginprice = reader["ccmarginprice"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["ccmarginprice"]);
                        osupplier.id = reader["id"] == DBNull.Value ? 0 : Convert.ToInt64(reader["id"]);
                        osupplier.agentid = reader["agentid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["agentid"]);
                        osupplier.psource = reader["pricesource"] == DBNull.Value ? 0 : Convert.ToInt32(reader["pricesource"]);
                        osupplier.msource = reader["mrpsource"] == DBNull.Value ? 0 : Convert.ToInt32(reader["mrpsource"]);
                        osupplier.name = reader["name"] == DBNull.Value ? "" : reader["name"].ToString();
                        osupplier.city = reader["city"] == DBNull.Value ? "" : reader["city"].ToString();
                        osupplier.address = reader["address"] == DBNull.Value ? "" : reader["address"].ToString();
                        osupplier.gstno = reader["gstno"] == DBNull.Value ? "" : reader["gstno"].ToString();
                        osupplier.stateid = reader["stateid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["stateid"]);
                        osupplier.cityid = reader["cityid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["cityid"]);
                        lst.Add(osupplier);
                    }
                }

                var duplicatenames = lst.GroupBy(x => x.name.Trim().ToLower()).ToList().FindAll(x => x.Count() > 1);
                foreach (var g in duplicatenames)
                {
                    g.ToList().ForEach(x => x.name = x.name + " " + x.city);
                }
                conn.Close();
            }
            catch (Exception exp)
            {
                if (conn != null) conn.Close();
                WebAPI.Common.Logger.Instance(Context.OrganizationID).Log(exp, WebAPI.Common.Logger.LogType.Error, exp.StackTrace);
                throw exp;
            }
            return lst;
        }

        internal Customer Insert(Company item)
        {
            Customer customer = new Customer();
            customer.name = item.name;
            customer.code = item.code;
            customer.isactive = true;
            customer.customertype = 2;
            customer.createdby = Context.UserID;
            customer.modifiedby = Context.UserID;
            customer.createdon = DateTime.Now;
            customer.modifiedon = DateTime.Now;
            customer.address = item.address;
            customer.gstno = item.gstno;
            customer.pincode = item.pincode;
            customer.stateid = item.stateid;
            customer.cityid = item.cityid;
            customer.countryid = item.countryid;
            customer.mobileno = item.contactno;
            customer.contactperson = item.contactperson;
            if (string.IsNullOrEmpty(customer.mobileno))
                customer.mobileno = "0-" + item.id;

            var existing = Search(new CustomerCreteria()
            {
                code = item.code
            });
            if (existing != null &&
                existing.Count > 0)
                return existing[0];
            isMigration = true;
            return Insert(customer);
        }

        internal CustomerRack SearchByRackNo(CustomerRack _customer)
        {
            string _SQL = @"select customerid,loyeltycardno,code,id,billno from deliveryrack where code=@code AND isactive=true AND disabled=false";
            if (!string.IsNullOrEmpty(_customer.cardno))
                _SQL = @"select customerid,loyeltycardno,code,id,billno from deliveryrack where loyeltycardno=@loyeltycardno AND isactive=true AND disabled=false";
            
            NpgsqlConnection conn = CreateConnection();
            try
            {
                conn.Open();
                NpgsqlCommand command = GetCommand(_SQL, conn);
                command.Parameters.Add("code", NpgsqlTypes.NpgsqlDbType.Varchar).Value = _customer.rackno;
                command.Parameters.Add("loyeltycardno", NpgsqlTypes.NpgsqlDbType.Varchar).Value = _customer.cardno;
                using (System.Data.IDataReader reader = command.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        string billno = reader["billno"] == DBNull.Value ? "" : Convert.ToString(reader["billno"]);
                        _customer.customerid = reader["customerid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["customerid"]);
                        _customer.rackid = reader["id"] == DBNull.Value ? 0 : Convert.ToInt64(reader["id"]);
                        _customer.cardno = reader["loyeltycardno"] == DBNull.Value ? "" : Convert.ToString(reader["loyeltycardno"]);
                        _customer.rackno = reader["code"] == DBNull.Value ? "" : Convert.ToString(reader["code"]);

                        if (!string.IsNullOrEmpty(billno))
                        {
                            if(!string.IsNullOrEmpty(_customer.cardno))
                                throw new Exception(_customer.cardno + " is already in use. billno " + billno);
                            else
                                throw new Exception(_customer.rackno + " is already in use. billno " + billno);
                        }
                    }
                }
                conn.Close();
            }
            catch (Exception exp)
            {
                if (conn != null) conn.Close();
                WebAPI.Common.Logger.Instance(Context.OrganizationID).Log(exp, WebAPI.Common.Logger.LogType.Error,_customer);
            }
            if (_customer.customerid > 0)
            {
                var customer = Search(new CustomerCreteria()
                {
                    id = _customer.customerid,
                    locationid = _customer.locationid
                });
                if (customer.Count > 0)
                {
                    CustomerRack customerRack = JsonConvert.DeserializeObject<CustomerRack>(JsonConvert.SerializeObject(customer[0]));
                    customerRack.customerid = _customer.customerid;
                    customerRack.cardno = _customer.cardno;
                    customerRack.rackno = _customer.rackno;
                    customerRack.rackid = _customer.rackid;
                    return customerRack;
                }
            }
            return _customer;
        }
    }

}
