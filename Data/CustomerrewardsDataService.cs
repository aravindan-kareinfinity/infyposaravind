using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using InfyPOS.Models;
using InfyPOS.Common;
using Npgsql;
using System.Reflection;

namespace InfyPOS.Data

{
    public class CustomerrewardsDataService : SAASData, ICustomerrewardsDataService
    {
        const string SQL_Search = @"select id, isactive, createdby, modifiedby, version, createdon, modifiedon, transactiondate, expiry, customerid, rewards, billvalue,adjusted,billid,billno,altered,audithistory,remark,locationid from customerrewards where id=@id";
        const string SQL_SettledSearch = @"select customerrewards.id, customerrewards.isactive, customerrewards.createdby, customerrewards.modifiedby, customerrewards.version, 
                                            customerrewards.createdon, customerrewards.modifiedon, customerrewards.transactiondate, customerrewards.expiry, customerrewards.customerid, 
                                            customerrewards.rewards, customerrewards.billvalue,customerrewards.adjusted,customerrewards.billid,customerrewards.billno,
                                            altered,audithistory,remark,COALESCE(bill.locationid,customerrewards.locationid) locationid  from customerrewards
                                            left outer join bill on bill.id = customerrewards.billid";

        const string SQL_Insert = @"INSERT INTO public.customerrewards(isactive, createdby, modifiedby, version, createdon, modifiedon, transactiondate, expiry, customerid, rewards, billvalue,billid,billno,adjusted,remark,locationid) VALUES(@isactive, @createdby, @modifiedby, @version, @createdon, @modifiedon, @transactiondate, @expiry, @customerid, @rewards, @billvalue,@billid,@billno,@adjusted,@remark,@locationid) RETURNING id";
        const string SQL_Update = @"UPDATE public.customerrewards SET modifiedby=@modifiedby, version=@version+1, modifiedon=@modifiedon, transactiondate=@transactiondate, expiry=@expiry, customerid=@customerid, rewards=@rewards, 
                                    adjusted=@adjusted, billvalue=@billvalue,altered=@altered,audithistory=@audithistory,remark=@remark,billno=@billno,locationid=@locationid where id=@id and version=@version RETURNING version";

        public bool CanGetRewardAtBilling()
        {
            if (!string.IsNullOrEmpty(InfyPOS.Services.ConfigManager.Instance(Context.OrganizationID).Config.integration_crm))
            {
                var integration = Assembly.GetExecutingAssembly().GetType(InfyPOS.Services.ConfigManager.Instance(Context.OrganizationID).Config.integration_crm);
                if (integration != null)
                {
                    ICustomerrewardsDataService bright = Activator.CreateInstance(integration) as ICustomerrewardsDataService;
                    bright.Context = this.Context;
                    return bright.CanGetRewardAtBilling();
                }
            }
            return true;
        }
        public CustomerRewardStatus SearchReward(CustomerrewardsCreteria item)
        {
            var returnvalue = new CustomerRewardStatus();
            decimal origionalbillvalue = item.billvalue;
            decimal existingreward = 0;

            CustomerDataService customerDataService = new CustomerDataService() { Context = this.Context };
            List<Customer> lstcustomer = customerDataService.Search(new CustomerCreteria()
            {
                id = item.customerid
            });

            if (lstcustomer != null && lstcustomer.Count > 0)
                item.customer = lstcustomer[0];

            if (InfyPOS.Services.ConfigManager.Instance(Context.OrganizationID).LocationConfig != null 
                && InfyPOS.Services.ConfigManager.Instance(Context.OrganizationID).LocationConfig.ContainsKey(item.locationid)
                && InfyPOS.Services.ConfigManager.Instance(Context.OrganizationID).LocationConfig[item.locationid].sale_canclaimloyaltyholderonly
                && item.customer != null
                && string.IsNullOrEmpty(item.customer.loyeltyno))
                return returnvalue;

            if (!string.IsNullOrEmpty(InfyPOS.Services.ConfigManager.Instance(Context.OrganizationID).Config.integration_crm))
            {
                var integration = Assembly.GetExecutingAssembly().GetType(InfyPOS.Services.ConfigManager.Instance(Context.OrganizationID).Config.integration_crm);
                if (integration != null)
                {
                    ICustomerrewardsDataService bright = Activator.CreateInstance(integration) as ICustomerrewardsDataService;
                    bright.Context = this.Context;
                    return bright.SearchReward(item);
                }
            }

            string SQL_rewardStatus = @"select sum(rewards-COALESCE(adjusted,0)) from customerrewards
                                              left outer join bill on bill.id = customerrewards.billid
                                              where customerrewards.customerid=@customerid 
                                              and customerrewards.isactive = true 
                                              and (bill.id is null or (expiry>=@expiry 
                                              and (COALESCE(bill.settled,true) = true)))
                                              and (bill.id is null or bill.billtype != 9)";

            if (InfyPOS.Services.ConfigManager.Instance(Context.OrganizationID).LocationConfig.ContainsKey(item.locationid) &&
                InfyPOS.Services.ConfigManager.Instance(Context.OrganizationID).LocationConfig[item.locationid].settlement_reward_expiry <= 0 &&
                !InfyPOS.Services.ConfigManager.Instance(Context.OrganizationID).LocationConfig[item.locationid].settlement_reward_expirybydate)
            {
                SQL_rewardStatus = @"select sum(rewards-COALESCE(adjusted,0)) from customerrewards
                                              left outer join bill on bill.id = customerrewards.billid
                                              where customerrewards.customerid=@customerid and 
                                              customerrewards.isactive = true and
                                              COALESCE(bill.settled,true) = true
                                              and (bill.id is null or bill.billtype != 9)";
            }

            if (item.billid > 0)
                SQL_rewardStatus += " and customerrewards.billid != " + item.billid;
            NpgsqlConnection conn = CreateConnection();

            try
            {
                conn.Open();
                decimal excludeValue = 0;
                decimal rewardpoint = 0;
                decimal newreward = 0;
                var categorybased = false;
                if (rewardpoint == 0 && InfyPOS.Services.ConfigManager.Instance(Context.OrganizationID).LocationConfig != null &&
                    InfyPOS.Services.ConfigManager.Instance(Context.OrganizationID).LocationConfig.ContainsKey(item.locationid))
                    rewardpoint = InfyPOS.Services.ConfigManager.Instance(Context.OrganizationID).LocationConfig[item.locationid].sale_rewardpoint;

                // Exclude reward points for the items having discount
                if (InfyPOS.Services.ConfigManager.Instance(Context.OrganizationID).LocationConfig != null &&
                    InfyPOS.Services.ConfigManager.Instance(Context.OrganizationID).LocationConfig.ContainsKey(item.locationid) &&
                    InfyPOS.Services.ConfigManager.Instance(Context.OrganizationID).LocationConfig[item.locationid].sale_excludediscount)
                {
                    if (item.billvalue > 0 && item.bill != null && item.bill.Billitems != null)
                    {
                        foreach (var _billitems in item.bill.Billitems)
                        {
                            if (_billitems.discount - (_billitems.attributes != null ? (_billitems.attributes.coupondiscount + _billitems.attributes.rewarddiscount) : 0) > 0)
                                item.billvalue -= (int)Math.Floor(_billitems.receivable);
                        }
                    }
                }

                // Exclude reward points for the items having additional discount
                if (InfyPOS.Services.ConfigManager.Instance(Context.OrganizationID).LocationConfig != null &&
                    InfyPOS.Services.ConfigManager.Instance(Context.OrganizationID).LocationConfig.ContainsKey(item.locationid) &&
                    InfyPOS.Services.ConfigManager.Instance(Context.OrganizationID).LocationConfig[item.locationid].sale_excludeadditionaldiscount)
                {
                    int excludepercent = InfyPOS.Services.ConfigManager.Instance(Context.OrganizationID).LocationConfig[item.locationid].sale_excludeadditionaldiscountabove;
                    if (item.billvalue > 0 && item.bill != null && item.bill.Billitems != null)
                    {
                        foreach (var _billitems in item.bill.Billitems)
                        {
                            if (_billitems.attributes.rewarddiscount == _billitems.attributes.coupondiscount)
                            {
                                if (_billitems.additionaldiscount - (_billitems.attributes != null ? (_billitems.attributes.coupondiscount) : 0) > 0)
                                {
                                    if (excludepercent == 0 || excludepercent < _billitems.additionaldiscountpercentage)
                                        item.billvalue -= (int)Math.Floor(_billitems.receivable);
                                }
                            }
                            else if(_billitems.attributes.coupondiscount!=0)
                            {
                                if (_billitems.additionaldiscount - (_billitems.attributes != null ? (_billitems.attributes.coupondiscount) : 0) > 0)
                                {
                                    if (excludepercent == 0 || excludepercent < _billitems.additionaldiscountpercentage)
                                        item.billvalue -= (int)Math.Floor(_billitems.receivable);
                                }
                            }
                            else if (_billitems.attributes.rewarddiscount != 0)
                            {
                                if (_billitems.additionaldiscount - (_billitems.attributes != null ? (_billitems.attributes.rewarddiscount) : 0) > 0)
                                {
                                    if (excludepercent == 0 || excludepercent < _billitems.additionaldiscountpercentage)
                                        item.billvalue -= (int)Math.Floor(_billitems.receivable);
                                }
                            }
                        }
                    }
                }

                // Exclude reward points for the items having coupon discount
                if (InfyPOS.Services.ConfigManager.Instance(Context.OrganizationID).LocationConfig != null &&
                    InfyPOS.Services.ConfigManager.Instance(Context.OrganizationID).LocationConfig.ContainsKey(item.locationid) &&
                    InfyPOS.Services.ConfigManager.Instance(Context.OrganizationID).LocationConfig[item.locationid].sale_excludecoupondiscount)
                {
                    if (item.billvalue > 0 && item.bill != null && item.bill.Billitems != null)
                    {
                        foreach (var _billitems in item.bill.Billitems)
                        {
                            if (_billitems.attributes != null && (_billitems.attributes.coupondiscount - _billitems.attributes.rewarddiscount) > 0)
                                item.billvalue -= (int)Math.Floor(_billitems.receivable);
                        }
                    }
                }

                // Exclude reward points for credit bills
                if (InfyPOS.Services.ConfigManager.Instance(Context.OrganizationID).LocationConfig != null &&
                    InfyPOS.Services.ConfigManager.Instance(Context.OrganizationID).LocationConfig.ContainsKey(item.locationid) &&
                    InfyPOS.Services.ConfigManager.Instance(Context.OrganizationID).LocationConfig[item.locationid].sale_excludecrbill)
                {
                    if (item.billvalue > 0 && item.bill != null && (item.bill.billtype == Bill.BillTypes.Credit || item.bill.isb2b))
                    {
                        item.billvalue -= (int)Math.Floor(item.bill.receivable);
                    }
                }

                // Exclude reward points for the products marked as excluded in product master
                if (InfyPOS.Services.ConfigManager.Instance(Context.OrganizationID).LocationConfig != null &&
                    InfyPOS.Services.ConfigManager.Instance(Context.OrganizationID).LocationConfig.ContainsKey(item.locationid) &&
                    InfyPOS.Services.ConfigManager.Instance(Context.OrganizationID).LocationConfig[item.locationid].sale_excludeproduct)
                {
                    if (item.billvalue > 0 && item.bill != null && item.bill.Billitems != null)
                    {
                        string excludeValueQuery = @"select cast(referencelist.additionalinfo->>'rewardpoint' as numeric) rewardpoint,dateofbirth from customer 
                                            left outer join referencelist on referencelist.id = customer.categoryid
                                            where customer.id=@customerid;
                                            select distinct stock.id from stock 
                                            inner join products on products.id = stock.productid
                                            where stock.id in (" + string.Join<long>(",", item.bill.Billitems.ConvertAll<long>(e => e.stockid)) + ") and cast(attributes->>'excludereward' as bool)=true";

                        var param = new SortedList<string, object>();
                        param.Add("customerid", item.customerid);
                        using (var reader = ExecuteReader(excludeValueQuery, param, conn, null))
                        {
                            if (reader.Read())
                            {
                                rewardpoint = reader.IsDBNull(0) ? 0 : reader.GetDecimal(0);
                                item.customerdob = reader.IsDBNull(1) ? DateTime.MinValue : reader.GetDateTime(1);
                            }
                            if (reader.NextResult())
                            {
                                while (reader.Read())
                                {
                                    var stockid = reader.IsDBNull(0) ? 0 : reader.GetInt64(0);
                                    if (item.bill.Billitems.Exists(e => e.stockid == stockid))
                                        excludeValue += item.bill.Billitems.Find(e => e.stockid == stockid).receivable;
                                }
                            }
                        }
                        if (rewardpoint == 0)
                            rewardpoint = InfyPOS.Services.ConfigManager.Instance(Context.OrganizationID).LocationConfig[item.locationid].sale_rewardpoint;

                        item.billvalue = (int)Math.Floor((item.billvalue - excludeValue));

                        if (item.billvalue < 0)
                            item.billvalue = 0;
                    }
                }
                else
                {
                    string excludeValueQuery = @"select cast(referencelist.additionalinfo->>'rewardpoint' as numeric) rewardpoint,dateofbirth from customer 
                                            left outer join referencelist on referencelist.id = customer.categoryid
                                            where customer.id=@customerid;";
                    var param = new SortedList<string, object>();
                    param.Add("customerid", item.customerid);
                    using (var reader = ExecuteReader(excludeValueQuery, param, conn, null))
                    {
                        if (reader.Read())
                        {
                            rewardpoint = reader.IsDBNull(0) ? 0 : reader.GetDecimal(0);
                            item.customerdob = reader.IsDBNull(1) ? DateTime.MinValue : reader.GetDateTime(1);
                            categorybased = (reader.IsDBNull(0) ? 0 : reader.GetDecimal(0)) > 0;
                        }
                    }

                    if (rewardpoint == 0 && InfyPOS.Services.ConfigManager.Instance(Context.OrganizationID).LocationConfig != null &&
                           InfyPOS.Services.ConfigManager.Instance(Context.OrganizationID).LocationConfig.ContainsKey(item.locationid))
                        rewardpoint = InfyPOS.Services.ConfigManager.Instance(Context.OrganizationID).LocationConfig[item.locationid].sale_rewardpoint;
                }

                if (rewardpoint > 0 && item.billvalue > 0 &&
                    InfyPOS.Services.ConfigManager.Instance(Context.OrganizationID).LocationConfig[item.locationid].sale_rewardmode == 0)
                {
                    newreward = (int)Math.Floor((item.billvalue) / rewardpoint);
                    if (rewardpoint < 100 && item.billvalue > rewardpoint)
                    {
                        newreward = Math.Round(item.billvalue / rewardpoint, 1);
                    }
                }

                if (!categorybased && InfyPOS.Services.ConfigManager.Instance(Context.OrganizationID).LocationConfig[item.locationid].sale_rewardmode == 2)
                {
                    var slaps = InfyPOS.Services.ConfigManager.Instance(Context.OrganizationID).LocationConfig[item.locationid].settlement_reward_slabs;
                    if (slaps != null && slaps.Count > 0)
                    {
                        decimal salesvalue = item.billvalue;
                        string customersalesvalues = @"select sum(receivable) from bill where customerid =" + item.customerid + " and bill.billtype != 9 and bill.id != " + item.billid + " and bill.isactive = true";
                        using (var reader = ExecuteReader(customersalesvalues, new SortedList<string, object>(), conn))
                        {
                            if (reader.Read() && !reader.IsDBNull(0))
                            {
                                salesvalue += reader.GetDecimal(0);
                            }
                        }
                        decimal rewardunit = -1;
                        foreach (var sitem in slaps.OrderBy(e => e.from))
                        {
                            if (sitem.from <= salesvalue && sitem.upto >= salesvalue)
                            {
                                rewardunit = sitem.percentage;
                                break;
                            }
                        }
                        if (rewardunit == -1)
                        {
                            if (slaps.OrderBy(e => e.from).First().from > salesvalue)
                                rewardunit = slaps.OrderBy(e => e.from).First().percentage;
                            else
                                rewardunit = slaps.OrderBy(e => e.from).Last().percentage;
                        }
                        //if (rewardpoint > rewardunit) 
                        rewardpoint = rewardunit;
                        if (rewardpoint > 0 && item.billvalue != 0)
                        {
                            newreward = (int)Math.Floor((item.billvalue) / rewardpoint);
                            if (rewardpoint < 100 && item.billvalue > rewardpoint)
                            {
                                newreward = Math.Round(item.billvalue / rewardpoint, 1);
                            }
                        }
                    }
                }

                if (!categorybased && InfyPOS.Services.ConfigManager.Instance(Context.OrganizationID).LocationConfig[item.locationid].sale_rewardmode == 3)
                {
                    var slaps = InfyPOS.Services.ConfigManager.Instance(Context.OrganizationID).LocationConfig[item.locationid].settlement_reward_slabs;
                    if (slaps != null && slaps.Count > 0)
                    {
                        decimal salesvalue = item.billvalue;
                        decimal rewardunit = -1;
                        foreach (var sitem in slaps.OrderBy(e => e.from))
                        {
                            if (sitem.from <= salesvalue && sitem.upto >= salesvalue)
                            {
                                rewardunit = sitem.percentage;
                                break;
                            }
                        }
                        if (rewardunit == -1)
                        {
                            if (slaps.OrderBy(e => e.from).First().from > salesvalue)
                                rewardunit = slaps.OrderBy(e => e.from).First().percentage;
                            else
                                rewardunit = slaps.OrderBy(e => e.from).Last().percentage;
                        }
                        //if (rewardpoint > rewardunit)
                        rewardpoint = rewardunit;
                        if (rewardpoint > 0 && item.billvalue != 0)
                        {
                            newreward = (int)Math.Floor((item.billvalue) / rewardpoint);
                            if (rewardpoint < 100 && item.billvalue > rewardpoint)
                            {
                                newreward = Math.Round(item.billvalue / rewardpoint, 1);
                            }
                        }
                    }
                }



                if (item.customerid > 0 &&
                    item.customerdob != DateTime.MinValue &&
    InfyPOS.Services.ConfigManager.Instance(Context.OrganizationID).LocationConfig[item.locationid].settlement_reward_birthday_mode > 0 &&
    InfyPOS.Services.ConfigManager.Instance(Context.OrganizationID).LocationConfig[item.locationid].settlement_birthday_multiple > 0)
                {
                    if (InfyPOS.Services.ConfigManager.Instance(Context.OrganizationID).LocationConfig[item.locationid].settlement_reward_birthday_mode == 1)
                    {
                        if (item.customerdob.Month == DateTimeNow.Month)
                        {
                            newreward = InfyPOS.Services.ConfigManager.Instance(Context.OrganizationID).LocationConfig[item.locationid].settlement_birthday_multiple * newreward;
                        }
                    }
                    else
                    {
                        var customerdob = item.customerdob.AddYears(DateTimeNow.Year - item.customerdob.Year);
                        var customerdobfrom = DateTimeNow.Date.AddDays(InfyPOS.Services.ConfigManager.Instance(Context.OrganizationID).LocationConfig[item.locationid].settlement_birthday_before * -1);
                        var customerdobafter = DateTimeNow.Date.AddDays(InfyPOS.Services.ConfigManager.Instance(Context.OrganizationID).LocationConfig[item.locationid].settlement_birthday_after);
                        if (customerdob >= customerdobfrom &&
                            customerdob <= customerdobafter)
                        {
                            newreward = InfyPOS.Services.ConfigManager.Instance(Context.OrganizationID).LocationConfig[item.locationid].settlement_birthday_multiple * newreward;
                        }
                    }
                }


                if (item.customerid > 0 &&
                    InfyPOS.Services.ConfigManager.Instance(Context.OrganizationID).LocationConfig[item.locationid].settlement_reward_first_enable &&
                    InfyPOS.Services.ConfigManager.Instance(Context.OrganizationID).LocationConfig[item.locationid].settlement_reward_first > 0)
                {
                    bool isfirstpurchase = true;
                    using (var reader = ExecuteReader("select id from bill where customerid=" + item.customerid + " and isactive = true and settled = true limit 1", null))
                    {
                        if (reader.Read())
                        {
                            isfirstpurchase = false;
                        }
                    }
                    if (isfirstpurchase)
                    {
                        newreward += InfyPOS.Services.ConfigManager.Instance(Context.OrganizationID).LocationConfig[item.locationid].settlement_reward_first;
                    }
                }

                // TO Remove reward points if bill value is lessthan the configured min bill
                if (InfyPOS.Services.ConfigManager.Instance(Context.OrganizationID).LocationConfig[item.locationid].sale_reward_minbillvalue > 0
                    && InfyPOS.Services.ConfigManager.Instance(Context.OrganizationID).LocationConfig[item.locationid].sale_reward_minbillvalue > item.billvalue)
                {
                    newreward = 0;
                }

                // TO GET THE EXISTING REWARD POINTS
                returnvalue.rewards = newreward;
                returnvalue.billrewards = newreward;
                returnvalue.billvalue = item.billvalue;
                NpgsqlCommand command = GetCommand(SQL_rewardStatus, conn);
                command.Parameters.Add("customerid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = item.customerid;
                command.Parameters.Add("expiry", NpgsqlTypes.NpgsqlDbType.Date).Value = DateTimeNow.Date;
                using (System.Data.IDataReader reader = command.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        returnvalue.rewards += reader.IsDBNull(0) ? 0 : Convert.ToInt64(reader[0]);
                        existingreward = reader.IsDBNull(0) ? 0 : Convert.ToInt64(reader[0]);
                    }
                }

                conn.Close();

                if (InfyPOS.Services.ConfigManager.Instance(Context.OrganizationID).LocationConfig != null &&
                        InfyPOS.Services.ConfigManager.Instance(Context.OrganizationID).LocationConfig.ContainsKey(item.locationid))
                {
                    if (InfyPOS.Services.ConfigManager.Instance(Context.OrganizationID).LocationConfig[item.locationid].settlement_reward_slabed)
                    {
                        if (InfyPOS.Services.ConfigManager.Instance(Context.OrganizationID).LocationConfig[item.locationid].settlement_rewardclaim_slabs != null)
                        {
                            rewardpoint = returnvalue.rewards - returnvalue.billrewards;

                            if(InfyPOS.Services.ConfigManager.Instance(Context.OrganizationID).LocationConfig[item.locationid].sale_canclaimsamebillrewards)
                                rewardpoint += returnvalue.billrewards;

                            if (InfyPOS.Services.ConfigManager.Instance(Context.OrganizationID).LocationConfig[item.locationid].settlement_rewardclaim_slabs.Exists(x =>
                            x.from <= rewardpoint && x.upto >= rewardpoint))
                            {
                                var rewardpercent = InfyPOS.Services.ConfigManager.Instance(Context.OrganizationID).LocationConfig[item.locationid].settlement_rewardclaim_slabs.Find(x =>
                                x.from <= rewardpoint && x.upto >= rewardpoint).percentage;
                                returnvalue.available = Math.Round(rewardpoint * rewardpercent / 100, 0);
                                returnvalue.convertable = returnvalue.available;
                                if (InfyPOS.Services.ConfigManager.Instance(Context.OrganizationID).LocationConfig[item.locationid].settlement_reward_maxbillvaluepercentage > 0)
                                {
                                    returnvalue.convertable = (long)Math.Round(item.billvalue * InfyPOS.Services.ConfigManager.Instance(Context.OrganizationID).LocationConfig[item.locationid].settlement_reward_maxbillvaluepercentage / 100, 0);
                                    if (returnvalue.available < returnvalue.convertable)
                                        returnvalue.convertable = returnvalue.available;
                                }
                            }
                        }
                    }
                    else if (InfyPOS.Services.ConfigManager.Instance(Context.OrganizationID).LocationConfig[item.locationid].settlement_reward_value > 0)
                    {
                        rewardpoint = returnvalue.rewards - returnvalue.billrewards;

                        if (InfyPOS.Services.ConfigManager.Instance(Context.OrganizationID).LocationConfig[item.locationid].sale_canclaimsamebillrewards)
                            rewardpoint += returnvalue.billrewards;

                        returnvalue.available = rewardpoint / InfyPOS.Services.ConfigManager.Instance(Context.OrganizationID).LocationConfig[item.locationid].settlement_reward_value;
                        returnvalue.convertable = returnvalue.available;
                        if (InfyPOS.Services.ConfigManager.Instance(Context.OrganizationID).LocationConfig[item.locationid].settlement_reward_maxbillvaluepercentage > 0)
                        {
                            returnvalue.convertable = (long)Math.Round(origionalbillvalue * InfyPOS.Services.ConfigManager.Instance(Context.OrganizationID).LocationConfig[item.locationid].settlement_reward_maxbillvaluepercentage / 100, 0);
                            if (returnvalue.available < returnvalue.convertable)
                                returnvalue.convertable = returnvalue.available;
                            // && item.billvalue > returnvalue.convertable
                            //if (!InfyPOS.Services.ConfigManager.Instance(Context.OrganizationID).LocationConfig[item.locationid].sale_rewardincludediscount)
                            //{
                            //    if (item.billvalue > returnvalue.convertable)
                            //    {
                            //        returnvalue.thisbillrewards = (int)Math.Floor((item.billvalue) / rewardpoint);
                            //        newreward = (int)Math.Floor((item.billvalue - returnvalue.convertable) / rewardpoint);
                            //        returnvalue.rewards += newreward;
                            //        returnvalue.billrewards += newreward;
                            //        returnvalue.available += newreward / InfyPOS.Services.ConfigManager.Instance(Context.OrganizationID).LocationConfig[item.locationid].settlement_reward_value;
                            //        returnvalue.convertable = (long)Math.Round(item.billvalue * InfyPOS.Services.ConfigManager.Instance(Context.OrganizationID).LocationConfig[item.locationid].settlement_reward_maxbillvaluepercentage / 100, 0);
                            //        if (returnvalue.available < returnvalue.convertable)
                            //            returnvalue.convertable = returnvalue.available;
                            //    }
                            //}
                            //else
                            //{
                            //    newreward = (int)Math.Floor((item.billvalue) / rewardpoint);
                            //    returnvalue.rewards += newreward;
                            //    returnvalue.billrewards += newreward;
                            //    returnvalue.available += newreward / InfyPOS.Services.ConfigManager.Instance(Context.OrganizationID).LocationConfig[item.locationid].settlement_reward_value;
                            //    returnvalue.convertable = (long)Math.Round(item.billvalue * InfyPOS.Services.ConfigManager.Instance(Context.OrganizationID).LocationConfig[item.locationid].settlement_reward_maxbillvaluepercentage / 100, 0);
                            //}
                        }
                    }
                    //else if ((InfyPOS.Services.ConfigManager.Instance(Context.OrganizationID).LocationConfig[item.locationid].sale_rewardmode == 2 
                    //    || InfyPOS.Services.ConfigManager.Instance(Context.OrganizationID).LocationConfig[item.locationid].sale_rewardmode == 3 ) && 
                    //    InfyPOS.Services.ConfigManager.Instance(Context.OrganizationID).LocationConfig[item.locationid].settlement_reward_slabs != null &&
                    //    InfyPOS.Services.ConfigManager.Instance(Context.OrganizationID).LocationConfig[item.locationid].settlement_reward_slabs.Count > 0)
                    //{
                    //    var slabs = InfyPOS.Services.ConfigManager.Instance(Context.OrganizationID).LocationConfig[item.locationid].settlement_reward_slabs;
                    //    var lastSlap = slabs.Last();
                    //    if (slabs.Exists(e => e.from <= returnvalue.rewards && e.upto >= returnvalue.rewards))
                    //    {
                    //        lastSlap = slabs.Find(e => e.from <= returnvalue.rewards && e.upto >= returnvalue.rewards);
                    //    }
                    //    var newvalue = returnvalue.rewards / (lastSlap.percentage + 100);
                    //    returnvalue.available = newvalue * lastSlap.percentage / 100;
                    //    returnvalue.convertable = returnvalue.available;
                    //    if (InfyPOS.Services.ConfigManager.Instance(Context.OrganizationID).LocationConfig[item.locationid].settlement_reward_maxbillvaluepercentage > 0)
                    //    {
                    //        returnvalue.convertable = (long)Math.Round(item.billvalue * InfyPOS.Services.ConfigManager.Instance(Context.OrganizationID).LocationConfig[item.locationid].settlement_reward_maxbillvaluepercentage / 100, 0);
                    //        if (returnvalue.available < returnvalue.convertable)
                    //            returnvalue.convertable = returnvalue.available;
                    //    }
                    //    if (!InfyPOS.Services.ConfigManager.Instance(Context.OrganizationID).LocationConfig[item.locationid].sale_rewardincludediscount &&
                    //            item.billvalue > returnvalue.convertable)
                    //    {
                    //        newreward = (int)Math.Floor((item.billvalue - returnvalue.convertable) / rewardpoint);
                    //        newvalue = newreward / (lastSlap.percentage + 100);
                    //        returnvalue.available += newvalue * lastSlap.percentage / 100;
                    //        returnvalue.convertable = (long)Math.Round(item.billvalue * InfyPOS.Services.ConfigManager.Instance(Context.OrganizationID).LocationConfig[item.locationid].settlement_reward_maxbillvaluepercentage / 100, 0);
                    //        if (returnvalue.available < returnvalue.convertable)
                    //            returnvalue.convertable = returnvalue.available;
                    //        returnvalue.rewards += newreward;
                    //        returnvalue.billrewards += newreward;
                    //    }
                    //}
                }


            }
            catch (Exception exp)
            {
                if (conn != null) conn.Close();
                WebAPI.Common.Logger.Instance(Context.OrganizationID).Log(exp, WebAPI.Common.Logger.LogType.Error, item);
            }
            return returnvalue;
        }

        const string SQL_Delete = @"update public.customerrewards set isactive=false,version=@version+1,modifiedby=@modifiedby,modifiedon=@modifiedon where id=@id";

        public decimal FindMaximumLoyelty(long locationid, decimal amount)
        {
            if (InfyPOS.Services.ConfigManager.Instance(Context.OrganizationID).LocationConfig[locationid].sale_rewardmode == 2 &&
                       InfyPOS.Services.ConfigManager.Instance(Context.OrganizationID).LocationConfig[locationid].settlement_reward_slabs != null &&
                       InfyPOS.Services.ConfigManager.Instance(Context.OrganizationID).LocationConfig[locationid].settlement_reward_slabs.Count > 0)
            {
                var slabs = InfyPOS.Services.ConfigManager.Instance(Context.OrganizationID).LocationConfig[locationid].settlement_reward_slabs;
                var lastSlap = slabs.Last();
                if (slabs.Exists(e => e.from <= amount && e.upto >= amount))
                {
                    lastSlap = slabs.Find(e => e.from <= amount && e.upto >= amount);
                }
                return Math.Round(amount * lastSlap.percentage / 100, 0);
            }
            if (InfyPOS.Services.ConfigManager.Instance(Context.OrganizationID).LocationConfig[locationid].settlement_reward_slabed)
            {
                if (InfyPOS.Services.ConfigManager.Instance(Context.OrganizationID).LocationConfig[locationid].settlement_rewardclaim_slabs != null)
                {
                    if (InfyPOS.Services.ConfigManager.Instance(Context.OrganizationID).LocationConfig[locationid].settlement_rewardclaim_slabs.Exists(x =>
                    x.from <= amount && x.upto >= amount))
                    {
                        var rewardpercent = InfyPOS.Services.ConfigManager.Instance(Context.OrganizationID).LocationConfig[locationid].settlement_rewardclaim_slabs.Find(x =>
                        x.from <= amount && x.upto >= amount).percentage;
                        return Math.Round(amount * rewardpercent / 100, 0);
                    }
                }
            }
            else if (InfyPOS.Services.ConfigManager.Instance(Context.OrganizationID).LocationConfig[locationid].sale_rewardmode <= 1 &&
                        InfyPOS.Services.ConfigManager.Instance(Context.OrganizationID).LocationConfig[locationid].settlement_reward_value > 0)
            {
                return Math.Round(amount / InfyPOS.Services.ConfigManager.Instance(Context.OrganizationID).LocationConfig[locationid].settlement_reward_value, 0);
            }
            return amount;
        }

        public List<Customerrewards> Search(CustomerrewardsCreteria _customerrewards, NpgsqlConnection conn, NpgsqlTransaction transaction)
        {
            List<Customerrewards> lst = new List<Customerrewards>();
            NpgsqlCommand command = null;
            if (_customerrewards.id != 0)
            {
                command = GetCommand(SQL_Search, conn);
                command.Parameters.Add("id", NpgsqlTypes.NpgsqlDbType.Bigint).Value = _customerrewards.id;
            }
            else
            {
                QueryBuilder qb = new QueryBuilder();
                qb.CurrentQuery = (_customerrewards.OnlySettled) ? SQL_SettledSearch : SQL_Search.Substring(0, SQL_Search.IndexOf("where"));
                qb.AddParameter("customerrewards.isactive", true, "=");

                if (_customerrewards.customerid > 0)
                    qb.AddParameter("customerrewards.customerid", _customerrewards.customerid, "=");

                if (_customerrewards.billid > 0)
                {
                    qb.AddParameter("customerrewards.billid", _customerrewards.billid, "=");
                }
                else
                {
                    if (_customerrewards.OnlyAdjusted)
                        qb.AddParameter("COALESCE(adjusted,0)", "adjusted", 0, ">");
                    else
                        qb.AddParameter("rewards-COALESCE(adjusted,0)", "balance", 0, ">");

                    if (Services.ConfigManager.Instance(Context.OrganizationID).LocationConfig.ContainsKey(_customerrewards.locationid) &&
                        (Services.ConfigManager.Instance(Context.OrganizationID).LocationConfig[_customerrewards.locationid].settlement_reward_expiry > 0 ||
                        Services.ConfigManager.Instance(Context.OrganizationID).LocationConfig[_customerrewards.locationid].settlement_reward_expirybydate))
                    {
                        if (_customerrewards.expiry != DateTime.MinValue)
                        {
                            qb.AddParameter("expiry", _customerrewards.expiry, ">=");
                        }
                    }
                    if (_customerrewards.OnlySettled)
                    {
                        qb.Add2Query(" (bill.id is null or bill.settled = true)");
                    }
                    if (!string.IsNullOrEmpty(_customerrewards.billno))
                    {
                        qb.AddParameter("customerrewards.billno", _customerrewards.billno, "=");
                    }
                }
                command = qb.GetCommand(conn);
            }
            if (transaction != null)
                command.Transaction = transaction;

            using (System.Data.IDataReader reader = command.ExecuteReader())
            {
                while (reader.Read())
                {
                    Customerrewards ocustomerrewards = new Customerrewards();
                    ocustomerrewards.id = reader["id"] == DBNull.Value ? 0 : Convert.ToInt64(reader["id"]);
                    ocustomerrewards.isactive = reader["isactive"] == DBNull.Value ? false : Convert.ToBoolean(reader["isactive"]);
                    ocustomerrewards.createdby = reader["createdby"] == DBNull.Value ? 0 : Convert.ToInt64(reader["createdby"]);
                    ocustomerrewards.modifiedby = reader["modifiedby"] == DBNull.Value ? 0 : Convert.ToInt64(reader["modifiedby"]);
                    ocustomerrewards.version = reader["version"] == DBNull.Value ? 0 : Convert.ToInt32(reader["version"]);
                    ocustomerrewards.createdon = reader["createdon"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(reader["createdon"]);
                    ocustomerrewards.modifiedon = reader["modifiedon"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(reader["modifiedon"]);
                    ocustomerrewards.transactiondate = reader["transactiondate"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(reader["transactiondate"]);
                    ocustomerrewards.expiry = reader["expiry"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(reader["expiry"]);
                    ocustomerrewards.customerid = reader["customerid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["customerid"]);
                    ocustomerrewards.rewards = reader["rewards"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["rewards"]);
                    ocustomerrewards.adjusted = reader["adjusted"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["adjusted"]);
                    ocustomerrewards.billid = reader["billid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["billid"]);
                    ocustomerrewards.locationid = reader["locationid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["locationid"]);
                    ocustomerrewards.billno = reader["billno"] == DBNull.Value ? "" : Convert.ToString(reader["billno"]);
                    ocustomerrewards.billvalue = reader["billvalue"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["billvalue"]);
                    ocustomerrewards.altered = reader["altered"] == DBNull.Value ? false : Convert.ToBoolean(reader["altered"]);
                    ocustomerrewards.audithistory_json = reader["audithistory"] == DBNull.Value ? "null" : reader["audithistory"].ToString();
                    ocustomerrewards.remark = reader["remark"] == DBNull.Value ? "" : Convert.ToString(reader["remark"]);

                    lst.Add(ocustomerrewards);
                }
            }
            return lst;
        }

        public void ProcessRewards(decimal schemerewardpoints, NpgsqlTransaction transaction, NpgsqlConnection conn, Billsettlementmaster oBillsettlementmaster)
        {
            var categorybased = false;
            {
                // Exclude reward points for the products marked as excluded in product master
                var receivable = oBillsettlementmaster.receivable;
                if (InfyPOS.Services.ConfigManager.Instance(Context.OrganizationID).LocationConfig[oBillsettlementmaster.locationid].sale_excludeproduct)
                {
                    var ids = oBillsettlementmaster.Billsettlement.ConvertAll<long>(e => e.billid) ;
                    string excludeValueQuery = @"select sum(receivable) from billitems
                                                            inner join stock on stock.id = billitems.stockid
                                                            inner join products on products.id = stock.productid
                                                            where billitems.billid in (" + string.Join<long>(",", ids) + ") and billitems.isactive=true and cast(products.attributes->>'excludereward' as bool)=true";

                    if(ids.Count == 1)
                    {
                        excludeValueQuery = @"select sum(receivable) from billitems
                                                            inner join stock on stock.id = billitems.stockid
                                                            inner join products on products.id = stock.productid
                                                            where billitems.billid = "+ ids.First() + " and billitems.isactive=true and cast(products.attributes->>'excludereward' as bool)=true";
                    }


                    using (var reader = ExecuteReader(excludeValueQuery, new SortedList<string, object>(), conn, transaction))
                    {
                        if (reader.Read())
                            receivable -= reader.IsDBNull(0) ? 0 : reader.GetDecimal(0);
                    }
                }


                if (oBillsettlementmaster.rewardunit == 0 && InfyPOS.Services.ConfigManager.Instance(Context.OrganizationID).LocationConfig != null
                    && InfyPOS.Services.ConfigManager.Instance(Context.OrganizationID).LocationConfig.ContainsKey(oBillsettlementmaster.locationid))
                    oBillsettlementmaster.rewardunit = InfyPOS.Services.ConfigManager.Instance(Context.OrganizationID).LocationConfig[oBillsettlementmaster.locationid].sale_rewardpoint;

                string excludeQuery = @"select cast(referencelist.additionalinfo->>'rewardpoint' as numeric) rewardpoint,dateofbirth from customer 
                                            left outer join referencelist on referencelist.id = customer.categoryid
                                            where customer.id=@customerid;";
                var param = new SortedList<string, object>();
                param.Add("customerid", oBillsettlementmaster.customerid);
                using (var reader = ExecuteReader(excludeQuery, param, conn, null))
                {
                    if (reader.Read())
                    {
                        oBillsettlementmaster.rewardunit = reader.IsDBNull(0) ? 0 : reader.GetDecimal(0);
                        oBillsettlementmaster.customerdob = reader.IsDBNull(1) ? DateTime.MinValue : reader.GetDateTime(1);
                        categorybased = oBillsettlementmaster.rewardunit > 0;
                    }
                }

                if (oBillsettlementmaster.rewardunit == 0 && InfyPOS.Services.ConfigManager.Instance(Context.OrganizationID).LocationConfig != null
                    && InfyPOS.Services.ConfigManager.Instance(Context.OrganizationID).LocationConfig.ContainsKey(oBillsettlementmaster.locationid))
                    oBillsettlementmaster.rewardunit = InfyPOS.Services.ConfigManager.Instance(Context.OrganizationID).LocationConfig[oBillsettlementmaster.locationid].sale_rewardpoint;

                // Exclude reward points for the items having discount
                if (InfyPOS.Services.ConfigManager.Instance(Context.OrganizationID).LocationConfig[oBillsettlementmaster.locationid].sale_excludediscount)
                {
                    string excludeValueQuery = @"select sum(receivable) from billitems
                                                            where billitems.billid in (" +
                                                            string.Join<long>(",", oBillsettlementmaster.Billsettlement.ConvertAll<long>(e => e.billid)) +
                                                            ")  and billitems.isactive=true and (discount-cast(attributes->>'coupondiscount' as numeric))>0";
                    using (var reader = ExecuteReader(excludeValueQuery, new SortedList<string, object>(), conn, transaction))
                    {
                        if (reader.Read())
                            receivable -= reader.IsDBNull(0) ? 0 : reader.GetDecimal(0);
                    }
                }


                // Exclude reward points for the items having additional discount
                if (InfyPOS.Services.ConfigManager.Instance(Context.OrganizationID).LocationConfig[oBillsettlementmaster.locationid].sale_excludeadditionaldiscount)
                {
                    int excludepercent = InfyPOS.Services.ConfigManager.Instance(Context.OrganizationID).LocationConfig[oBillsettlementmaster.locationid].sale_excludeadditionaldiscountabove;
                    string excludeValueQuery = @"select (receivable),
                                                additionaldiscount*qty additionaldiscount,
                                                cast(attributes->>'rewarddiscountpercentage' as numeric) rewarddiscountpercentage
                                                from billitems
                                                where billitems.billid in (" +
                                                            string.Join<long>(",", oBillsettlementmaster.Billsettlement.ConvertAll<long>(e => e.billid)) +
                                                            ")  and billitems.isactive=true and additionaldiscount>0";

                    using (var reader = ExecuteReader(excludeValueQuery, new SortedList<string, object>(), conn, transaction))
                    {
                        while (reader.Read())
                        {
                            if (reader["rewarddiscountpercentage"] == DBNull.Value)
                            {
                                receivable -= reader.IsDBNull(0) ? 0 : reader.GetDecimal(0);
                            }
                            else
                            {
                                var rewarddiscountpercentage = Convert.ToDecimal(reader["rewarddiscountpercentage"]);
                                var billitemreceivable = reader.IsDBNull(0) ? 0 : reader.GetDecimal(0);
                                decimal rewarddiscountvalue = 0;
                                if (billitemreceivable > 0 && (100 - rewarddiscountpercentage) > 0)
                                    rewarddiscountvalue = billitemreceivable / (100 - rewarddiscountpercentage) * rewarddiscountpercentage;
                                var additionaldiscount = Convert.ToDecimal(reader["additionaldiscount"]);
                                decimal additionaldiscountpercentage = 0;
                                if (additionaldiscount > 0 && billitemreceivable > 0)
                                    additionaldiscountpercentage = Math.Round((additionaldiscount * 100 ) / billitemreceivable, 2);
                                if (Math.Abs(additionaldiscount - rewarddiscountvalue) > 5)
                                {
                                    if (excludepercent == 0 || excludepercent < additionaldiscountpercentage)
                                        receivable -= billitemreceivable;
                                }
                            }
                        }
                    }
                }

                // Exclude reward points for the items having coupon discount
                if (InfyPOS.Services.ConfigManager.Instance(Context.OrganizationID).LocationConfig[oBillsettlementmaster.locationid].sale_excludecoupondiscount)
                {
                    string excludeValueQuery = @"select (receivable),
                                                additionaldiscount*qty additionaldiscount,
                                                cast(attributes->>'rewarddiscountpercentage' as numeric) rewarddiscountpercentage,
                                                cast(attributes->>'coupondiscount' as numeric)*qty coupondiscount
                                                from billitems
                                                where billitems.billid in (" +
                                                            string.Join<long>(",", oBillsettlementmaster.Billsettlement.ConvertAll<long>(e => e.billid)) +
                                                            ")  and billitems.isactive=true and cast(attributes->>'coupondiscount' as numeric)>0";
                    using (var reader = ExecuteReader(excludeValueQuery, new SortedList<string, object>(), conn, transaction))
                    {
                        while (reader.Read())
                        {
                            if (reader["rewarddiscountpercentage"] == DBNull.Value)
                            {
                                receivable -= reader.IsDBNull(0) ? 0 : reader.GetDecimal(0);
                            }
                            else
                            {
                                var rewarddiscountpercentage = Convert.ToDecimal(reader["rewarddiscountpercentage"]);
                                var billitemreceivable = reader.IsDBNull(0) ? 0 : reader.GetDecimal(0);
                                decimal rewarddiscountvalue = 0;
                                if (billitemreceivable > 0 && (100 - rewarddiscountpercentage) > 0)
                                    rewarddiscountvalue = billitemreceivable / (100 - rewarddiscountpercentage) * rewarddiscountpercentage;
                                var additionaldiscount = Convert.ToDecimal(reader["additionaldiscount"]);
                                var coupondiscount = Convert.ToDecimal(reader["coupondiscount"]);
                                if (Math.Abs(coupondiscount - rewarddiscountvalue) > 5)
                                {
                                    receivable -= billitemreceivable;
                                }
                            }
                        }
                    }
                }

                // Exclude reward points for credit bills
                if (InfyPOS.Services.ConfigManager.Instance(Context.OrganizationID).LocationConfig[oBillsettlementmaster.locationid].sale_excludecrbill)
                {
                    string excludeValueQuery = @"select sum(receivable) from bill
                                                            where (isb2b=true or billtype=1) and id in (" +
                                                            string.Join<long>(",", oBillsettlementmaster.Billsettlement.ConvertAll<long>(e => e.billid)) +
                                                            ")";
                    using (var reader = ExecuteReader(excludeValueQuery, new SortedList<string, object>(), conn, transaction))
                    {
                        if (reader.Read())
                            receivable -= reader.IsDBNull(0) ? 0 : reader.GetDecimal(0);
                    }
                }

                // Exclude reward points for the settlement having credit note
                if (InfyPOS.Services.ConfigManager.Instance(Context.OrganizationID).LocationConfig[oBillsettlementmaster.locationid].sale_excludecreditnote)
                {
                    if (oBillsettlementmaster.paymentinfo.HasVoucher)
                    {
                        CustomerCouponsDataService couponData = new CustomerCouponsDataService() { Context = this.Context };
                        foreach (var item in oBillsettlementmaster.paymentinfo.voucherinformation)
                        {
                            List<CustomerCoupons> lstcoupon = couponData.Search(new CustomerCouponsCreteria() { code = item.code });
                            if (lstcoupon != null && lstcoupon.Count > 0 && lstcoupon.Exists(e=>e.vouchertype == CustomerCoupons.CouponType.CreditNote))
                            {
                                receivable -= item.amount;
                            }
                        }
                    }
                }

                if (!categorybased && InfyPOS.Services.ConfigManager.Instance(Context.OrganizationID).LocationConfig[oBillsettlementmaster.locationid].sale_rewardmode == 2)
                {
                    var slaps = InfyPOS.Services.ConfigManager.Instance(Context.OrganizationID).LocationConfig[oBillsettlementmaster.locationid].settlement_reward_slabs;
                    if (slaps.Count > 0)
                    {
                        decimal salesvalue = oBillsettlementmaster.Billsettlement.Sum(e => e.receivable);
                        string customersalesvalues = @"select sum(receivable) from bill where customerid =" + oBillsettlementmaster.customerid + " and bill.billtype != 9 and id not in (" + string.Join(",", oBillsettlementmaster.Billsettlement.ConvertAll(e => e.billid)) + ")";
                        using (var reader = ExecuteReader(customersalesvalues, new SortedList<string, object>(), conn, transaction))
                        {
                            if (reader.Read() && !reader.IsDBNull(0))
                            {
                                salesvalue += reader.GetDecimal(0);
                            }
                        }
                        decimal rewardunit = -1;
                        foreach (var sitem in slaps.OrderBy(e => e.from))
                        {
                            if (sitem.from <= salesvalue && sitem.upto >= salesvalue)
                            {
                                rewardunit = sitem.percentage;
                                break;
                            }
                        }
                        if (rewardunit == -1)
                        {
                            if (slaps.OrderBy(e => e.from).First().from > salesvalue)
                                rewardunit = slaps.OrderBy(e => e.from).First().percentage;
                            else
                                rewardunit = slaps.OrderBy(e => e.from).Last().percentage;
                        }
                        oBillsettlementmaster.rewardunit = rewardunit;
                    }
                }

                if (!categorybased && InfyPOS.Services.ConfigManager.Instance(Context.OrganizationID).LocationConfig[oBillsettlementmaster.locationid].sale_rewardmode == 3)
                {
                    var slaps = InfyPOS.Services.ConfigManager.Instance(Context.OrganizationID).LocationConfig[oBillsettlementmaster.locationid].settlement_reward_slabs;
                    if (slaps.Count > 0)
                    {
                        decimal salesvalue = oBillsettlementmaster.receivable;
                        decimal rewardunit = -1;
                        foreach (var sitem in slaps.OrderBy(e => e.from))
                        {
                            if (sitem.from <= salesvalue && sitem.upto >= salesvalue)
                            {
                                rewardunit = sitem.percentage;
                                break;
                            }
                        }
                        if (rewardunit == -1)
                        {
                            if (slaps.OrderBy(e => e.from).First().from > salesvalue)
                                rewardunit = slaps.OrderBy(e => e.from).First().percentage;
                            else
                                rewardunit = slaps.OrderBy(e => e.from).Last().percentage;
                        }
                        oBillsettlementmaster.rewardunit = rewardunit;
                    }
                }

                if (oBillsettlementmaster.rewardunit > 0 && (schemerewardpoints == 0 || (int)Math.Floor(receivable / oBillsettlementmaster.rewardunit) > schemerewardpoints))
                {
                    schemerewardpoints = (int)Math.Floor(receivable / oBillsettlementmaster.rewardunit);
                    if (oBillsettlementmaster.rewardunit < 100 && receivable > oBillsettlementmaster.rewardunit)
                    {
                        schemerewardpoints = Math.Round(receivable / oBillsettlementmaster.rewardunit, 1);
                    }

                    if (InfyPOS.Services.ConfigManager.Instance(Context.OrganizationID).LocationConfig[oBillsettlementmaster.locationid].sale_rewardincludediscount &&
                        InfyPOS.Services.ConfigManager.Instance(Context.OrganizationID).LocationConfig[oBillsettlementmaster.locationid].sale_rewardmode == 1)
                    {
                        string excludeValueQuery = @"select cast(billattributes->>'manualrewardpoint' as numeric) from bill where id in (" + string.Join<long>(",", oBillsettlementmaster.Billsettlement.ConvertAll<long>(e => e.billid)) + ")";
                        using (var reader = ExecuteReader(excludeValueQuery, new SortedList<string, object>(), conn, transaction))
                        {
                            while (reader.Read())
                            {
                                if (!reader.IsDBNull(0))
                                    schemerewardpoints += (int)reader.GetDecimal(0);
                            }
                        }
                    }
                }


                if (oBillsettlementmaster.customerid > 0 &&
                    oBillsettlementmaster.customerdob != DateTime.MinValue &&
                    InfyPOS.Services.ConfigManager.Instance(Context.OrganizationID).LocationConfig[oBillsettlementmaster.locationid].settlement_reward_birthday_mode > 0 &&
                    InfyPOS.Services.ConfigManager.Instance(Context.OrganizationID).LocationConfig[oBillsettlementmaster.locationid].settlement_birthday_multiple > 0)
                {
                    if (InfyPOS.Services.ConfigManager.Instance(Context.OrganizationID).LocationConfig[oBillsettlementmaster.locationid].settlement_reward_birthday_mode == 1)
                    {
                        if (oBillsettlementmaster.customerdob.Month == DateTimeNow.Month)
                        {
                            schemerewardpoints = InfyPOS.Services.ConfigManager.Instance(Context.OrganizationID).LocationConfig[oBillsettlementmaster.locationid].settlement_birthday_multiple * schemerewardpoints;
                        }
                    }
                    else
                    {
                        var customerdob = oBillsettlementmaster.customerdob.AddYears(DateTimeNow.Year - oBillsettlementmaster.customerdob.Year);
                        var customerdobfrom = DateTimeNow.Date.AddDays(InfyPOS.Services.ConfigManager.Instance(Context.OrganizationID).LocationConfig[oBillsettlementmaster.locationid].settlement_birthday_before * -1);
                        var customerdobafter = DateTimeNow.Date.AddDays(InfyPOS.Services.ConfigManager.Instance(Context.OrganizationID).LocationConfig[oBillsettlementmaster.locationid].settlement_birthday_after);
                        if (customerdob >= customerdobfrom &&
                            customerdob <= customerdobafter)
                        {
                            schemerewardpoints = InfyPOS.Services.ConfigManager.Instance(Context.OrganizationID).LocationConfig[oBillsettlementmaster.locationid].settlement_birthday_multiple * schemerewardpoints;
                        }
                    }
                }

                if (oBillsettlementmaster.customerid > 0 &&
                    InfyPOS.Services.ConfigManager.Instance(Context.OrganizationID).LocationConfig[oBillsettlementmaster.locationid].settlement_reward_first_enable &&
                    InfyPOS.Services.ConfigManager.Instance(Context.OrganizationID).LocationConfig[oBillsettlementmaster.locationid].settlement_reward_first > 0)
                {
                    bool isfirstpurchase = true;
                    using (var reader = ExecuteReader("select id from bill where customerid=" + oBillsettlementmaster.customerid + " and isactive = true and settled = true limit 1", null))
                    {
                        if (reader.Read())
                        {
                            isfirstpurchase = false;
                        }
                    }
                    if (isfirstpurchase)
                    {
                        schemerewardpoints += InfyPOS.Services.ConfigManager.Instance(Context.OrganizationID).LocationConfig[oBillsettlementmaster.locationid].settlement_reward_first;
                    }
                }


                // TO Remove reward points if bill value is lessthan the configured min bill value
                if (InfyPOS.Services.ConfigManager.Instance(Context.OrganizationID).LocationConfig[oBillsettlementmaster.locationid].sale_reward_minbillvalue > 0
                && InfyPOS.Services.ConfigManager.Instance(Context.OrganizationID).LocationConfig[oBillsettlementmaster.locationid].sale_reward_minbillvalue > receivable)
                {
                    schemerewardpoints = 0;
                }

                if (schemerewardpoints > 0)
                {
                    DateTime expiry = DateTimeNow.Date.AddDays(InfyPOS.Services.ConfigManager.Instance(Context.OrganizationID).LocationConfig[oBillsettlementmaster.locationid].settlement_reward_expiry);
                    if (Services.ConfigManager.Instance(Context.OrganizationID).LocationConfig[oBillsettlementmaster.locationid].settlement_reward_expirybydate)
                        expiry = Services.ConfigManager.Instance(Context.OrganizationID).LocationConfig[oBillsettlementmaster.locationid].settlement_reward_expirydate;
                    else if (InfyPOS.Services.ConfigManager.Instance(Context.OrganizationID).LocationConfig[oBillsettlementmaster.locationid].settlement_reward_expiry <= 0)
                        expiry = DateTime.MaxValue;
                    else
                        expiry = expiry.AddDays(1);

                    oBillsettlementmaster.paymentinfo.rewardpoint = schemerewardpoints;
                    CustomerDataService customerDataService = new CustomerDataService() { Context = this.Context };
                    oBillsettlementmaster.paymentinfo.avlrewardpoint = customerDataService.AddReward(transaction, conn,
                        oBillsettlementmaster.customerid,
                        schemerewardpoints,
                        expiry,
                        oBillsettlementmaster,
                        oBillsettlementmaster.Billsettlement.Sum(e => e.adjustablereward));
                }

                if (oBillsettlementmaster.Billsettlement.Sum(e => e.adjustablereward) > 0)
                {
                    ApplyReward(transaction, conn, oBillsettlementmaster.customerid,
                        oBillsettlementmaster.Billsettlement.Sum(e => e.adjustablereward), oBillsettlementmaster.locationid);

                    oBillsettlementmaster.paymentinfo.avlrewardpoint -= oBillsettlementmaster.Billsettlement.Sum(e => e.adjustablereward);
                }
            }
        }

        public List<Customerrewards> Search(CustomerrewardsCreteria _customerrewards)
        {
            List<Customerrewards> lst = new List<Customerrewards>();
            NpgsqlConnection conn = CreateConnection();
            try
            {
                conn.Open();
                lst = Search(_customerrewards, conn, null);
                conn.Close();
            }
            catch (Exception exp)
            {
                if (conn != null) conn.Close();
                WebAPI.Common.Logger.Instance(Context.OrganizationID).Log(exp, WebAPI.Common.Logger.LogType.Error, _customerrewards);
            }
            return lst;
        }

        public Customerrewards Insert(Customerrewards oCustomerrewards)
        {

            NpgsqlTransaction transaction = null;
            try
            {
                using (NpgsqlConnection conn = CreateConnection())
                {
                    conn.Open();
                    transaction = conn.BeginTransaction();
                    NpgsqlCommand command = GetCommand(SQL_Insert, conn, transaction);

                    Insert(transaction, conn, oCustomerrewards);
                    transaction.Commit();
                    conn.Close();
                }
            }
            catch (Exception exp)
            {
                WebAPI.Common.Logger.Instance(Context.OrganizationID).Log(exp, WebAPI.Common.Logger.LogType.Error, oCustomerrewards);
            }

            return oCustomerrewards;
        }

        public Customerrewards Insert(NpgsqlTransaction transaction, NpgsqlConnection conn, Customerrewards oCustomerrewards)
        {

            NpgsqlCommand command = GetCommand(SQL_Insert, conn, transaction);
            command.Parameters.Add("isactive", NpgsqlTypes.NpgsqlDbType.Boolean).Value = oCustomerrewards.isactive;
            command.Parameters.Add("createdby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oCustomerrewards.createdby;
            command.Parameters.Add("modifiedby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oCustomerrewards.modifiedby;
            command.Parameters.Add("version", NpgsqlTypes.NpgsqlDbType.Integer).Value = oCustomerrewards.version;
            command.Parameters.Add("createdon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = oCustomerrewards.createdon;
            command.Parameters.Add("modifiedon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = oCustomerrewards.modifiedon;
            command.Parameters.Add("transactiondate", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = oCustomerrewards.transactiondate;
            command.Parameters.Add("expiry", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = oCustomerrewards.expiry;
            command.Parameters.Add("customerid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oCustomerrewards.customerid;
            command.Parameters.Add("rewards", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oCustomerrewards.rewards;
            command.Parameters.Add("adjusted", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oCustomerrewards.adjusted;
            command.Parameters.Add("billvalue", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oCustomerrewards.billvalue;
            command.Parameters.Add("billid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oCustomerrewards.billid;
            command.Parameters.Add("locationid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oCustomerrewards.locationid;
            command.Parameters.Add("billno", NpgsqlTypes.NpgsqlDbType.Varchar).Value = string.IsNullOrEmpty(oCustomerrewards.billno) ? "" : oCustomerrewards.billno;
            command.Parameters.Add("remark", NpgsqlTypes.NpgsqlDbType.Varchar).Value = string.IsNullOrEmpty(oCustomerrewards.remark) ? "" : oCustomerrewards.remark;
            using (System.Data.IDataReader reader = command.ExecuteReader())
            {
                if (reader.Read())
                {
                    oCustomerrewards.id = Convert.ToInt64(reader[0].ToString());
                }
            }
            return oCustomerrewards;
        }

        public Customerrewards Update(Customerrewards oCustomerrewards)
        {

            NpgsqlTransaction transaction = null;
            try
            {
                using (NpgsqlConnection conn = CreateConnection())
                {
                    conn.Open();
                    transaction = conn.BeginTransaction();

                    Update(transaction, conn, oCustomerrewards);
                    transaction.Commit();
                    conn.Close();
                }
            }
            catch (Exception exp)
            {
                WebAPI.Common.Logger.Instance(Context.OrganizationID).Log(exp, WebAPI.Common.Logger.LogType.Error, oCustomerrewards);
            }

            return oCustomerrewards;
        }

        public Customerrewards Update(NpgsqlTransaction transaction, NpgsqlConnection conn, Customerrewards oCustomerrewards)
        {

            NpgsqlCommand command = GetCommand(SQL_Update, conn, transaction);
            command.Parameters.Add("id", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oCustomerrewards.id;
            command.Parameters.Add("isactive", NpgsqlTypes.NpgsqlDbType.Boolean).Value = oCustomerrewards.isactive;
            command.Parameters.Add("createdby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oCustomerrewards.createdby;
            command.Parameters.Add("modifiedby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oCustomerrewards.modifiedby;
            command.Parameters.Add("version", NpgsqlTypes.NpgsqlDbType.Integer).Value = oCustomerrewards.version;
            command.Parameters.Add("createdon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = oCustomerrewards.createdon;
            command.Parameters.Add("modifiedon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = oCustomerrewards.modifiedon;
            command.Parameters.Add("transactiondate", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = oCustomerrewards.transactiondate;
            command.Parameters.Add("expiry", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = oCustomerrewards.expiry;
            command.Parameters.Add("customerid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oCustomerrewards.customerid;
            command.Parameters.Add("rewards", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oCustomerrewards.rewards;
            command.Parameters.Add("adjusted", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oCustomerrewards.adjusted;
            command.Parameters.Add("billvalue", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oCustomerrewards.billvalue;
            command.Parameters.Add("locationid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oCustomerrewards.locationid;
            command.Parameters.Add("billno", NpgsqlTypes.NpgsqlDbType.Varchar).Value = string.IsNullOrEmpty(oCustomerrewards.billno) ? "" : oCustomerrewards.billno;
            command.Parameters.Add("remark", NpgsqlTypes.NpgsqlDbType.Varchar).Value = string.IsNullOrEmpty(oCustomerrewards.remark) ? "" : oCustomerrewards.remark;
            command.Parameters.Add("altered", NpgsqlTypes.NpgsqlDbType.Boolean).Value = oCustomerrewards.altered;
            command.Parameters.Add("audithistory", NpgsqlTypes.NpgsqlDbType.Json).Value = oCustomerrewards.audithistory_json == null ? "null" : oCustomerrewards.audithistory_json;
            using (System.Data.IDataReader reader = command.ExecuteReader())
            {
                if (reader.Read())
                {
                    oCustomerrewards.version = Convert.ToInt32(reader[0].ToString());
                }
            }

            return oCustomerrewards;
        }
        public CustomerBills UpdateRewards(CustomerBills oCustomerBills)
        {
            if (!Services.ConfigManager.Instance(Context.OrganizationID).Config.system_allowloyeltycustomerwithoutbill && string.IsNullOrEmpty(oCustomerBills.billno))
            {
                oCustomerBills.message = "Bill number mandatory";
                return oCustomerBills;
            }
            if (oCustomerBills.expiry == DateTime.MinValue || oCustomerBills.expiry < DateTime.Now)
            {
                oCustomerBills.message = "Expiry Date must be greater than today.";
                return oCustomerBills;
            }
            Customerrewards _rewards = null;
            List<string> message = new List<string>();
            if (oCustomerBills.id > 0)
            {
                List<Customerrewards> lst = Search(new CustomerrewardsCreteria() { id = oCustomerBills.id });
                if (lst != null && lst.Count > 0)
                    _rewards = lst[0];
            }
            NpgsqlTransaction transaction = null;
            try
            {
                using (NpgsqlConnection conn = CreateConnection())
                {
                    conn.Open();
                    transaction = conn.BeginTransaction();
                    BillDataService billDataService = new BillDataService() { Context = this.Context };
                    if (oCustomerBills.id == 0)
                    {
                        if (oCustomerBills.billno.IndexOf(',') > 0)
                        {
                            decimal totalamount = 0;
                            foreach (var bill in oCustomerBills.billno.Split(','))
                            {
                                var Sbill = billDataService.Search(new BillCreteria()
                                {
                                    billno = oCustomerBills.billno,
                                    locationid = oCustomerBills.locationid
                                });

                                if (Sbill.Count == 0)
                                {
                                    if (!Services.ConfigManager.Instance(Context.OrganizationID).Config.system_allowloyeltycustomerwithoutbill)
                                    {
                                        message.Add("Given bill number " + bill + " not available, can't add as per the configuration.");
                                        oCustomerBills.message = string.Join("\r\n", message);
                                        return oCustomerBills;
                                    }
                                }
                                else if (Sbill[0].customerid != 0)
                                {
                                    message.Add("Given bill number " + bill + " ,already contains customer information.");
                                    oCustomerBills.message = string.Join("\r\n", message);
                                    return oCustomerBills;
                                }
                                else if (Sbill[0].billtype == Bill.BillTypes.Cancel)
                                {
                                    message.Add("Cancelled bill number " + bill + ", Can't add to the reward system");
                                    oCustomerBills.message = string.Join("\r\n", message);
                                    return oCustomerBills;
                                }
                                else if (!Sbill[0].settled)
                                {
                                    message.Add("Unsettled bill number " + bill + ", Can't add to the reward system");
                                    oCustomerBills.message = string.Join("\r\n", message);
                                    return oCustomerBills;
                                }
                                totalamount += Sbill.Sum(e => e.receivable);
                            }

                            if (oCustomerBills.billvalue > totalamount)
                            {
                                oCustomerBills.message = "Given bill value higher than the actual bill value. Allowed only " + totalamount.ToString() + "/-";
                                return oCustomerBills;
                            }
                        }
                        else
                        {
                            List<Bill> bills = new List<Bill>();
                            if (!string.IsNullOrEmpty(oCustomerBills.billno))
                            {
                                List<string> billnos = new List<string>(oCustomerBills.billno.Split(','));
                                bills = billDataService.Search(new BillCreteria()
                                {
                                    billno = billnos.Count == 1 ? oCustomerBills.billno : null,
                                    billnos = billnos.Count == 1 ? null : billnos,
                                    locationid = oCustomerBills.locationid,
                                    isimport = true,
                                    removecancelled = true
                                });
                            }

                            if (bills != null && bills.Count == 1)
                            {
                                if (oCustomerBills.billvalue > bills[0].totalamount)
                                {
                                    oCustomerBills.message = "Given bill value higher than the actual bill value.";
                                    return oCustomerBills;
                                }
                                oCustomerBills.billid = bills[0].id;
                                if (bills[0].customerid == 0)
                                {
                                    message.Add("Customer associated with given bill no " + oCustomerBills.billno);
                                    ExecuteQuery("Update bill set customerid=" + oCustomerBills.customerid + " where id=" + bills[0].id+ "  and isactive=true", new SortedList<string, object>());
                                }
                                else if (bills[0].customerid != oCustomerBills.customerid)
                                {
                                    message.Add("Different customer " +
                                    (bills[0].customer != null ? bills[0].customer.name : "") + " associated with this bill - " + oCustomerBills.billno);
                                    if (!Services.ConfigManager.Instance(Context.OrganizationID).Config.system_allowloyeltycustomerchange)
                                    {
                                        message.Add("Can't change the customer as per the configuration.");
                                        oCustomerBills.message = string.Join("\r\n", message);
                                        return oCustomerBills;
                                    }
                                    var existingbillrewards = Search(new CustomerrewardsCreteria()
                                    {
                                        billid = bills[0].id
                                    });
                                    if (existingbillrewards.Count > 0 && existingbillrewards[0].adjusted > 0)
                                    {
                                        message.Add("This bill reward already consumed, can't change the customer");
                                        oCustomerBills.message = string.Join("\r\n", message);
                                        return oCustomerBills;
                                    }
                                    message.Add("This bill existing reward will be removed, and bill will be associated with new customer!.");
                                }
                            }
                            else
                            {
                                if (!Services.ConfigManager.Instance(Context.OrganizationID).Config.system_allowloyeltycustomerwithoutbill)
                                {
                                    message.Add("Given bill number not available, can't add as per the configuration.");
                                    oCustomerBills.message = string.Join("\r\n", message);
                                    return oCustomerBills;
                                }
                                if (!string.IsNullOrEmpty(oCustomerBills.billno))
                                {
                                    var rewardlist = Search(new CustomerrewardsCreteria()
                                    {
                                        billno = oCustomerBills.billno
                                    });
                                    if (rewardlist.Count > 0)
                                    {
                                        message.Add("Given bill number already added to another customer");
                                        oCustomerBills.message = string.Join("\r\n", message);
                                        return oCustomerBills;
                                    }
                                    else
                                    {
                                        message.Add("Given bill number not available, But added this rewards");
                                    }
                                }
                                else
                                {
                                    message.Add("Given bill number not available, But added this rewards");
                                }
                            }
                        }

                        if (FindMaximumLoyelty(oCustomerBills.locationid, oCustomerBills.billvalue) < oCustomerBills.loyelty)
                        {
                            message.Add("Loyelty value missmatch with given bill value / Bill Value must be equal to Points.");
                            oCustomerBills.message = string.Join("\r\n", message);
                            return oCustomerBills;
                        }
                        var ocustomer = new Customerrewards()
                        {
                            isactive = true,
                            version = 1,
                            createdby = Context.UserID,
                            modifiedby = Context.UserID,
                            createdon = DateTimeNow,
                            modifiedon = DateTimeNow,
                            billno = oCustomerBills.billno,
                            transactiondate = DateTimeNow,
                            expiry = oCustomerBills.expiry,
                            customerid = oCustomerBills.customerid,
                            rewards = oCustomerBills.loyelty,
                            billvalue = oCustomerBills.billvalue,
                            adjusted = oCustomerBills.adjusted,
                            billid = oCustomerBills.billid,
                            altered = true,
                            remark = string.Concat("Manual Reward Entry Using Loyalty management screen with Comments : ", string.IsNullOrEmpty(oCustomerBills.remark) ? "" : oCustomerBills.remark)
                        };
                        Insert(transaction, conn, ocustomer);
                        oCustomerBills.id = ocustomer.id;
                    }
                    else if (_rewards != null)
                    {
                        if (_rewards.audithistory == null) _rewards.audithistory = new List<Customerrewards.AuditHistory>();
                        _rewards.audithistory.Add(new Customerrewards.AuditHistory()
                        {
                            oldrewards = _rewards.rewards,
                            oldadjusted = _rewards.adjusted,
                            newadjusted = oCustomerBills.adjusted,
                            newrewards = oCustomerBills.loyelty,
                            transactiondate = DateTimeNow,
                            updatedby = Context.UserID
                        });

                        if (!Services.ConfigManager.Instance(Context.OrganizationID).Config.system_allowloyeltycustomerwithoutbill)
                        {
                            if (FindMaximumLoyelty(oCustomerBills.locationid, oCustomerBills.billvalue) < oCustomerBills.loyelty)
                            {
                                message.Add("Loyelty value missmatch with given bill value");
                                oCustomerBills.message = string.Join("\r\n", message);
                                return oCustomerBills;
                            }
                            var rewardlist = Search(new CustomerrewardsCreteria()
                            {
                                billno = oCustomerBills.billno
                            });
                            if (rewardlist.Count > 0 && rewardlist[0].id != oCustomerBills.id)
                            {
                                oCustomerBills.message = "Given bill no already associated with another reward entry";
                                return oCustomerBills;
                            }
                            List<string> billnos = new List<string>(oCustomerBills.billno.Split(','));
                            var bills = billDataService.Search(new BillCreteria()
                            {
                                billno = billnos.Count == 1 ? oCustomerBills.billno : null,
                                billnos = billnos.Count == 1 ? null : billnos,
                                locationid = oCustomerBills.locationid,
                                isimport = true,
                                removecancelled = true
                            });
                            if (bills.Count > 0)
                            {
                                if (bills[0].customerid > 0 &&
                                    bills[0].customerid != oCustomerBills.customerid)
                                {
                                    oCustomerBills.message = "This bill reward already consumed, can't change the customer";
                                    return oCustomerBills;
                                }
                                if (bills[0].customerid == 0)
                                {
                                    if (_rewards.billvalue > bills[0].totalamount)
                                    {
                                        oCustomerBills.message = "Given bill value higher than the actual bill value.";
                                        return oCustomerBills;
                                    }
                                    ExecuteQuery("Update bill set customerid=" + bills[0].customerid + " where id=" + bills[0].id+ "  and isactive=true", new SortedList<string, object>());
                                }
                            }
                            else
                            {
                                if (!Services.ConfigManager.Instance(Context.OrganizationID).Config.system_allowloyeltycustomerwithoutbill)
                                {
                                    message.Add("Given bill number not available, can't add as per the configuration.");
                                    oCustomerBills.message = string.Join("\r\n", message);
                                    return oCustomerBills;
                                }
                            }
                        }
                        _rewards.billno = oCustomerBills.billno;
                        _rewards.billvalue = oCustomerBills.billvalue;
                        _rewards.rewards = oCustomerBills.loyelty;
                        _rewards.adjusted = oCustomerBills.adjusted;
                        _rewards.expiry = oCustomerBills.expiry;
                        _rewards.remark = string.Concat("Manual Reward Correction Using Loyalty management screen with Comments : ", string.IsNullOrEmpty(oCustomerBills.remark) ? "" : oCustomerBills.remark);
                        _rewards.altered = true;
                        Update(transaction, conn, _rewards);
                    }
                    transaction.Commit();
                    conn.Close();
                }
            }
            catch (Exception exp)
            {
                WebAPI.Common.Logger.Instance(Context.OrganizationID).Log(exp, WebAPI.Common.Logger.LogType.Error, oCustomerBills);
            }

            return oCustomerBills;
        }


        public bool Delete(Customerrewards oCustomerrewards)
        {

            NpgsqlTransaction transaction = null;
            bool hasSuccessCount = false;
            try
            {
                using (NpgsqlConnection conn = CreateConnection())
                {
                    conn.Open();
                    transaction = conn.BeginTransaction();
                    hasSuccessCount = Delete(transaction, conn, oCustomerrewards);
                    transaction.Commit();
                    conn.Close();
                }
            }
            catch (Exception exp)
            {
                WebAPI.Common.Logger.Instance(Context.OrganizationID).Log(exp, WebAPI.Common.Logger.LogType.Error, oCustomerrewards);
            }
            return hasSuccessCount;
        }

        public bool Delete(NpgsqlTransaction transaction, NpgsqlConnection conn, Customerrewards oCustomerrewards)
        {
            NpgsqlCommand command = GetCommand(SQL_Delete, conn, transaction);
            command.Parameters.Add("isactive", NpgsqlTypes.NpgsqlDbType.Boolean).Value = false;
            command.Parameters.Add("modifiedby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = this.Context.UserID;
            command.Parameters.Add("modifiedon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = DateTimeNow;
            command.Parameters.Add("version", NpgsqlTypes.NpgsqlDbType.Integer).Value = oCustomerrewards.version;
            command.Parameters.Add("id", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oCustomerrewards.id;
            int successCount = command.ExecuteNonQuery();
            return successCount > 0;
        }

        public void ApplyManualReward(NpgsqlTransaction transaction, NpgsqlConnection conn, Bill oBill)
        {
            var rewards = oBill.billattributes.manualrewardpoint > 0 ? oBill.billattributes.manualrewardpoint : oBill.billattributes.manualrewardamount;
            var available = Search(new CustomerrewardsCreteria()
            {
                customerid = oBill.customerid,
                expiry = DateTimeNow.Date,
                locationid = oBill.locationid
            }).OrderBy(e => e.expiry).ToList();
            decimal adjusted = 0;

            const string SQL_ApplyReward = @"update public.customerrewards set version=version+1,modifiedby=@modifiedby,modifiedon=@modifiedon,adjusted=COALESCE(adjusted,0)+@adjusted 
                                                            where id=@id RETURNING adjusted";

            NpgsqlCommand command = GetCommand(SQL_ApplyReward, conn, transaction);
            command.Parameters.Add("modifiedby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = this.Context.UserID;
            command.Parameters.Add("modifiedon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = DateTimeNow;
            command.Parameters.Add("id", NpgsqlTypes.NpgsqlDbType.Bigint).Value = 0;
            command.Parameters.Add("adjusted", NpgsqlTypes.NpgsqlDbType.Numeric).Value = 0;
            foreach (var item in available)
            {
                var balance = rewards - adjusted;
                decimal claimed = 0;
                if ((item.rewards - item.adjusted) > balance)
                {
                    claimed = balance;
                }
                else
                {
                    claimed = (item.rewards - item.adjusted);
                }

                item.adjusted += claimed;
                adjusted += claimed;

                command.Parameters["id"].Value = item.id;
                command.Parameters["adjusted"].Value = claimed;

                using (System.Data.IDataReader reader = command.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        if (item.adjusted != reader.GetDecimal(0))
                            throw new Exception("Reward already claimed");
                    }
                }
                if (adjusted >= rewards)
                    break;
            }

            //if (adjusted != rewards)
            //    throw new Exception("Reward claimed mismatched");

            command.Parameters.Remove("adjusted");
            command.CommandText = "update customer set loyelty=loyelty-@adjusted where id=@id";
            command.Parameters.Add("adjusted", NpgsqlTypes.NpgsqlDbType.Numeric).Value = rewards;
            command.Parameters["id"].Value = oBill.customerid;
            command.ExecuteNonQuery();

        }

        public void ApplyReward(NpgsqlTransaction transaction, NpgsqlConnection conn, long customerid, decimal rewards, long locationid)
        {
            var available = Search(new CustomerrewardsCreteria()
            {
                customerid = customerid,
                expiry = DateTimeNow.Date,
                OnlySettled = true,
                locationid = locationid
            }, conn, transaction).OrderBy(e => e.expiry).ToList();
            decimal adjusted = 0;

            const string SQL_ApplyReward = @"update public.customerrewards set version=version+1,modifiedby=@modifiedby,modifiedon=@modifiedon,adjusted=COALESCE(adjusted,0)+@adjusted 
                                                            where id=@id RETURNING adjusted";

            NpgsqlCommand command = GetCommand(SQL_ApplyReward, conn, transaction);
            command.Parameters.Add("modifiedby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = this.Context.UserID;
            command.Parameters.Add("modifiedon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = DateTimeNow;
            command.Parameters.Add("id", NpgsqlTypes.NpgsqlDbType.Bigint).Value = 0;
            command.Parameters.Add("adjusted", NpgsqlTypes.NpgsqlDbType.Numeric).Value = 0;
            foreach (var item in available)
            {
                var balance = rewards - adjusted;
                decimal claimed = 0;
                if ((item.rewards - item.adjusted) > balance)
                {
                    claimed = balance;
                }
                else
                {
                    claimed = (item.rewards - item.adjusted);
                }

                item.adjusted += claimed;
                adjusted += claimed;

                command.Parameters["id"].Value = item.id;
                command.Parameters["adjusted"].Value = claimed;

                using (System.Data.IDataReader reader = command.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        if (item.adjusted != reader.GetDecimal(0))
                            throw new Exception("Reward already claimed");
                    }
                }

                if (adjusted >= rewards)
                    break;
            }
            //if (adjusted != rewards)
            //    throw new Exception("Reward claimed mismatched");
        }

        public void ApplyReward(NpgsqlTransaction transaction, NpgsqlConnection conn, Bill oBill)
        {
            if (InfyPOS.Services.ConfigManager.Instance(Context.OrganizationID).LocationConfig != null &&
                        InfyPOS.Services.ConfigManager.Instance(Context.OrganizationID).LocationConfig.ContainsKey(oBill.locationid))
            {
                decimal rewards = oBill.billattributes.rewardpoint;

                var available = Search(new CustomerrewardsCreteria()
                {
                    customerid = oBill.customerid,
                    expiry = DateTimeNow.Date,
                    OnlySettled = true,
                    locationid = oBill.locationid
                },conn,transaction).OrderBy(e => e.expiry).ToList();
                decimal adjusted = 0;

                const string SQL_ApplyReward = @"update public.customerrewards set version=version+1,modifiedby=@modifiedby,modifiedon=@modifiedon,adjusted=COALESCE(adjusted,0)+@adjusted 
                                                            where id=@id RETURNING adjusted";

                NpgsqlCommand command = GetCommand(SQL_ApplyReward, conn, transaction);
                command.Parameters.Add("modifiedby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = this.Context.UserID;
                command.Parameters.Add("modifiedon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = DateTimeNow;
                command.Parameters.Add("id", NpgsqlTypes.NpgsqlDbType.Bigint).Value = 0;
                command.Parameters.Add("adjusted", NpgsqlTypes.NpgsqlDbType.Numeric).Value = 0;
                foreach (var item in available)
                {
                    decimal balance = rewards - adjusted;
                    decimal claimed = 0;
                    if ((item.rewards - item.adjusted) > balance)
                    {
                        claimed = balance;
                    }
                    else
                    {
                        claimed = (item.rewards - item.adjusted);
                    }

                    item.adjusted += claimed;
                    adjusted += claimed;

                    command.Parameters["id"].Value = item.id;
                    command.Parameters["adjusted"].Value = claimed;

                    using (System.Data.IDataReader reader = command.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            if (Math.Abs(item.adjusted - reader.GetDecimal(0))>1)
                                throw new Exception("Reward already claimed");
                        }
                    }

                    if (adjusted >= rewards)
                        break;
                }
                //if (adjusted != rewards)
                //    throw new Exception("Reward claimed mismatched");
                command.Parameters.Remove("adjusted");
                command.CommandText = "update customer set loyelty=loyelty-@adjusted where id=@id";
                command.Parameters.Add("adjusted", NpgsqlTypes.NpgsqlDbType.Numeric).Value = adjusted;
                command.Parameters["id"].Value = oBill.customerid;
                command.ExecuteNonQuery();

            }
        }

        public void ReverseReward(NpgsqlTransaction transaction, NpgsqlConnection conn, long customerid, decimal rewards, long locationid)
        {
            var available = Search(new CustomerrewardsCreteria()
            {
                customerid = customerid,
                expiry = DateTimeNow.Date,
                OnlyAdjusted = true,
                locationid = locationid
            }, conn, transaction).OrderBy(e => e.expiry).ToList();
            decimal adjusted = 0;

            const string SQL_ApplyReward = @"update public.customerrewards set version=version+1,modifiedby=@modifiedby,modifiedon=@modifiedon,adjusted=COALESCE(adjusted,0)-@adjusted 
                                                            where id=@id RETURNING adjusted";

            NpgsqlCommand command = GetCommand(SQL_ApplyReward, conn, transaction);
            command.Parameters.Add("modifiedby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = this.Context.UserID;
            command.Parameters.Add("modifiedon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = DateTimeNow;
            command.Parameters.Add("id", NpgsqlTypes.NpgsqlDbType.Bigint).Value = 0;
            command.Parameters.Add("adjusted", NpgsqlTypes.NpgsqlDbType.Numeric).Value = 0;
            foreach (var item in available)
            {
                var balance = rewards - adjusted;
                decimal claimed = 0;
                if (item.adjusted > balance)
                {
                    claimed = balance;
                }
                else
                {
                    claimed = item.adjusted;
                }

                adjusted += claimed;

                command.Parameters["id"].Value = item.id;
                command.Parameters["adjusted"].Value = claimed;

                using (System.Data.IDataReader reader = command.ExecuteReader())
                {
                    if (reader.Read())
                    {

                    }
                }

                if (adjusted >= rewards)
                    break;
            }
        }


        public string PrintDocument(List<CustomerCoupons> coupon)
        {
            InfyPOS.Data.ConfigurationDataService config = new ConfigurationDataService();
            var reportformats = config.Search(new ConfigurationCreteria
            {
                key = "COUPONPRINT",
                locationid = coupon.First().locationid,
                organizationid = 0
            });
            if (reportformats.Count == 0)
            {
                reportformats = config.Search(new ConfigurationCreteria
                {
                    key = "COUPONPRINT"
                });
            }
            if (reportformats.Count == 0)
                return "";
            ImageData imageData = new ImageData() { Context = this.Context };
            var additional = new SortedDictionary<string, object>();
            ReportLibrary.DefaultPageSettings setting = new ReportLibrary.DefaultPageSettings();
            ReportLibrary.DotMatrixReporter labreport = new ReportLibrary.DotMatrixReporter(this.Context.OrganizationID, new ReportLibrary.DotMatrixReporter.PrinterConfig());
            labreport.format = ReportLibrary.DosFormatParser.ParseXML(this.Context.OrganizationID, reportformats[0].reportformat.format);

            List<string> imageids = labreport.format.GetImages();
            labreport.Additional = new SortedDictionary<string, object>();
            foreach (var imgid in imageids.FindAll(x => x.Length > 6))
            {
                var file = imageData.GetData(imgid.LongFromBase62());
                if (file != null && file.data != null)
                {
                    additional.Add("IMG-" + imgid, file.data);
                }
            }

            if (Context.EmployeeID > 0)
            {
                EmployeeDataService employeeDataService = new EmployeeDataService();
                var issuedemployees = employeeDataService.SearchHeader(new EmployeeCreteria()
                {
                    id = Context.EmployeeID
                });
                var issuedemployee = "";
                if (issuedemployees.Count>0)
                    issuedemployee = issuedemployees.First().name;
                coupon.ForEach(x => x.issuedemployee = issuedemployee);
            }

            ReferencelistDataService referencelistDataService = new ReferencelistDataService();
            var location = referencelistDataService.Search(new ReferencelistCreteria()
            {
                id = coupon.First().locationid
            }).First();

            var couponlocation = new CompanyLocation(location);
            couponlocation.name = location.name;
            couponlocation.phonenumber = location.additionalinfo == null ? "" : location.additionalinfo.formula;
            couponlocation.address = location.additionalinfo == null ? "" : location.additionalinfo.address;
            couponlocation.addresslines = new List<CompanyLocation.Address>();
            if (!string.IsNullOrEmpty(couponlocation.address))
            {
                var addresslist = couponlocation.address.Split(new string[] { "\r\n", "\r", "\n" }, StringSplitOptions.None);
                addresslist.ToList().ForEach(e =>
                {
                    couponlocation.addresslines.Add(new CompanyLocation.Address()
                    {
                        value = e
                    });
                });
            }
            coupon.ForEach(x => x.location = couponlocation);
            var document = labreport.format.RenderBytes(new ImageData(), coupon,
               coupon,
               additional, true);
            using (System.IO.MemoryStream ms = new System.IO.MemoryStream())
            {
                System.IO.BinaryWriter bw = new System.IO.BinaryWriter(ms);
                bw.Write(document);
                int c = 1;
                while (c < reportformats[0].noofcopy)
                {
                    bw.Write(document);
                    c++;
                }
                return ms.Length > 0 ? Convert.ToBase64String(ms.ToArray()) : "";
            }
        }

        public string GenerateCoupon(Customer customer)
        {
            if (!ValidateOTP(customer))
            {
                throw new Exception("Given OTP not matched");
            }
            CustomerDataService customerDataService = new CustomerDataService() { Context = this.Context };
            CustomerCouponsDataService voucherdata = new CustomerCouponsDataService() { Context = this.Context };
            var vouchercode = DateTimeNow.Year.ToString().Substring(2, 2) + WebAPI.Data.Token.RandomString(14, "0123456789");
            List<CustomerCoupons> vouchers = new List<CustomerCoupons>();
            
            var voucher = new CustomerCoupons()
            {
                issuedby = Context.UserID,
                issueto = customer.id,
                amount = customer.claimamount,
                adjustedrewards =Convert.ToInt64(customer.claimpoint),
                code = vouchercode,
                isactive = true,
                vouchertype = CustomerCoupons.CouponType.Coupon,
                issuedon = DateTime.Now,
                validupto = DateTime.MaxValue,
                minbillvalue = 0,
                acceptablelocationid = 0,
                unlimited = false,
                maxbilldiscountpercentage = 0,
                filterschemeid = 0,
                locationid = customer.locationid,
            };
            if (customer.couponlist != null)
            {
                foreach (var item in customer.couponlist)
                {
                    vouchercode = DateTimeNow.Year.ToString().Substring(2, 2) + WebAPI.Data.Token.RandomString(14, "0123456789");
                    voucher = new CustomerCoupons()
                    {
                        issuedby = Context.UserID,
                        issueto = customer.id,
                        amount = item.claimamount,
                        adjustedrewards = Convert.ToInt64(item.claimpoint),
                        code = vouchercode,
                        isactive = true,
                        vouchertype = CustomerCoupons.CouponType.Coupon,
                        issuedon = DateTime.Now,
                        validupto = DateTime.MaxValue,
                        minbillvalue = 0,
                        acceptablelocationid = 0,
                        unlimited = false,
                        maxbilldiscountpercentage = 0,
                        filterschemeid = 0,
                        locationid = customer.locationid,
                    };
                    vouchers.Add(voucher);
                    voucherdata.Insert(voucher);
                }
            }
            else
            {
                vouchers.Add(voucher);
                voucherdata.Insert(voucher);
            }
            var ncustomerinfo = customerDataService.Search(new CustomerCreteria() { id = customer.id }).First();
            vouchers.ForEach(x =>
            {
                x.claimdetail = customer;
                x.customer = ncustomerinfo;
            });
            return PrintDocument(vouchers);
        }
        public bool ValidateOTP(Customer item)
        {
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

        internal decimal GetCustomerReward(CustomerrewardsCreteria item)
        {
            var param = new SortedList<string, object>();
            param.Add("customerid", item.customerid);
            param.Add("expiry", DateTimeNow.Date);
            decimal reward = 0;
            using (var reader = ExecuteReader(@"select sum(rewards-COALESCE(adjusted,0)) from customerrewards
                                              where customerrewards.customerid=@customerid 
                                              and customerrewards.isactive = true 
                                              and expiry>=@expiry", param))
            {
                if (reader.Read())
                {
                    reward = reader[0] != DBNull.Value ? Convert.ToDecimal(reader[0]) : 0;
                }
            }

            CustomerRewardStatus returnvalue = new CustomerRewardStatus()
            {
                rewards = reward
            };

            if (InfyPOS.Services.ConfigManager.Instance(Context.OrganizationID).LocationConfig != null &&
                    InfyPOS.Services.ConfigManager.Instance(Context.OrganizationID).LocationConfig.ContainsKey(item.locationid))
            {
                if (InfyPOS.Services.ConfigManager.Instance(Context.OrganizationID).LocationConfig[item.locationid].settlement_reward_slabed)
                {
                    if (InfyPOS.Services.ConfigManager.Instance(Context.OrganizationID).LocationConfig[item.locationid].settlement_rewardclaim_slabs != null)
                    {
                        if (InfyPOS.Services.ConfigManager.Instance(Context.OrganizationID).LocationConfig[item.locationid].settlement_rewardclaim_slabs.Exists(x =>
                        x.from <= returnvalue.rewards && x.upto >= returnvalue.rewards))
                        {
                            var rewardpercent = InfyPOS.Services.ConfigManager.Instance(Context.OrganizationID).LocationConfig[item.locationid].settlement_rewardclaim_slabs.Find(x =>
                            x.from <= returnvalue.rewards && x.upto >= returnvalue.rewards).percentage;
                            returnvalue.available = Math.Round(returnvalue.rewards * rewardpercent / 100, 0);
                            returnvalue.convertable = returnvalue.available;
                            returnvalue.rewardpercent = rewardpercent;
                            if (InfyPOS.Services.ConfigManager.Instance(Context.OrganizationID).LocationConfig[item.locationid].settlement_reward_maxbillvaluepercentage > 0)
                            {
                                returnvalue.convertable = (long)Math.Round(item.billvalue * InfyPOS.Services.ConfigManager.Instance(Context.OrganizationID).LocationConfig[item.locationid].settlement_reward_maxbillvaluepercentage / 100, 0);
                                if (returnvalue.available < returnvalue.convertable)
                                    returnvalue.convertable = returnvalue.available;
                            }
                        }
                    }
                }
                else if (InfyPOS.Services.ConfigManager.Instance(Context.OrganizationID).LocationConfig[item.locationid].settlement_reward_value > 0)
                {
                    returnvalue.available = returnvalue.rewards / InfyPOS.Services.ConfigManager.Instance(Context.OrganizationID).LocationConfig[item.locationid].settlement_reward_value;
                    returnvalue.convertable = returnvalue.available;
                    if (InfyPOS.Services.ConfigManager.Instance(Context.OrganizationID).LocationConfig[item.locationid].settlement_reward_maxbillvaluepercentage > 0)
                    {
                        returnvalue.convertable = (long)Math.Round(item.billvalue * InfyPOS.Services.ConfigManager.Instance(Context.OrganizationID).LocationConfig[item.locationid].settlement_reward_maxbillvaluepercentage / 100, 0);
                        if (returnvalue.available < returnvalue.convertable)
                            returnvalue.convertable = returnvalue.available;
                    }
                }
            }
            return returnvalue.rewards;
        }
        internal CustomerRewardStatus SearchCustomerReward(CustomerrewardsCreteria item)
        {
            CustomerDataService customerDataService = new CustomerDataService() { Context = this.Context };
            var customer = customerDataService.Search(new CustomerCreteria()
            {
                mobileno = item.mobileno,
                loyeltyno = item.loyeltyno,
            });
            if (customer.Count == 0)
                throw new Exception("Customer Not found");
            if (customer.Count > 1)
                throw new Exception("Multiple Customer found");

            item.customerid = customer[0].id;
            decimal rewards = GetCustomerReward(item);
            CustomerRewardStatus returnvalue = new CustomerRewardStatus()
            {
                customername = customer[0].name,
                mobileno = customer[0].mobileno,
                loyeltyno = customer[0].loyeltyno,
                rewards = rewards,
                customerid = customer[0].id
            };
            return returnvalue;
        }

        /*
        internal void ProcessRewards(NpgsqlTransaction transaction, NpgsqlConnection conn, Bill oBill)
        {
            var receivable = oBill.receivable;
            if (InfyPOS.Services.ConfigManager.Instance(Context.OrganizationID).LocationConfig[oBill.locationid].sale_excludeproduct)
            {
                string excludeValueQuery = @"select sum(receivable) from billitems
                                                            inner join stock on stock.id = billitems.stockid
                                                            inner join products on products.id = stock.productid
                                                            where billitems.billid = "+oBill.id+" and cast(products.attributes->>'excludereward' as bool)=true";
                using (var reader = ExecuteReader(excludeValueQuery, new SortedList<string, object>(), conn, transaction))
                {
                    if (reader.Read())
                        receivable -= reader.IsDBNull(0) ? 0 : reader.GetDecimal(0);
                }
            }

            if (InfyPOS.Services.ConfigManager.Instance(Context.OrganizationID).LocationConfig[oBill.locationid].sale_excludeadditionaldiscount)
            {
                string excludeValueQuery = @"select sum(receivable) from billitems
                                                            where billitems.billid " + oBill.id + " and additionaldiscount>0";
                using (var reader = ExecuteReader(excludeValueQuery, new SortedList<string, object>(), conn, transaction))
                {
                    if (reader.Read())
                        receivable -= reader.IsDBNull(0) ? 0 : reader.GetDecimal(0);
                }
            }

            if (InfyPOS.Services.ConfigManager.Instance(Context.OrganizationID).LocationConfig[oBill.locationid].sale_rewardmode == 2)
            {
                var slaps = InfyPOS.Services.ConfigManager.Instance(Context.OrganizationID).LocationConfig[oBill.locationid].settlement_reward_slabs;
                if (slaps.Count > 0)
                {
                    decimal salesvalue = 0;
                    string customersalesvalues = @"select sum(receivable) from bill where customerid =" + oBill.customerid;
                    using (var reader = ExecuteReader(customersalesvalues, new SortedList<string, object>(), conn, transaction))
                    {
                        if (reader.Read() && !reader.IsDBNull(0))
                        {
                            salesvalue = reader.GetDecimal(0);
                        }
                    }
                    int rewardunit = -1;
                    foreach (var item in slaps.OrderBy(e => e.from))
                    {
                        if (salesvalue < item.from)
                        {
                            rewardunit = item.percentage;
                            break;
                        }
                    }
                    if (rewardunit == -1)
                        rewardunit = slaps.OrderBy(e => e.from).Last().percentage;
                    oBill.rewardunit = rewardunit;
                }
            }

            if (InfyPOS.Services.ConfigManager.Instance(Context.OrganizationID).LocationConfig[oBill.locationid].sale_rewardmode == 3)
            {
                var slaps = InfyPOS.Services.ConfigManager.Instance(Context.OrganizationID).LocationConfig[oBill.locationid].settlement_reward_slabs;
                if (slaps.Count > 0)
                {
                    decimal salesvalue = oBill.receivable;
                    int rewardunit = -1;
                    foreach (var item in slaps.OrderBy(e => e.from))
                    {
                        if (salesvalue < item.from)
                        {
                            rewardunit = item.percentage;
                            break;
                        }
                    }
                    if (rewardunit == -1)
                        rewardunit = slaps.OrderBy(e => e.from).Last().percentage;
                    oBill.rewardunit = rewardunit;
                }
            }
            int schemerewardpoints = 0;
            if (oBill.rewardunit > 0)
            {
                schemerewardpoints = (int)Math.Floor(receivable / oBill.rewardunit);
                if (InfyPOS.Services.ConfigManager.Instance(Context.OrganizationID).LocationConfig[oBill.locationid].sale_rewardincludediscount &&
                    InfyPOS.Services.ConfigManager.Instance(Context.OrganizationID).LocationConfig[oBill.locationid].sale_rewardmode == 1)
                {
                    string excludeValueQuery = @"select cast(billattributes->>'manualrewardpoint' as numeric) from bill where id = "+oBill.id;
                    using (var reader = ExecuteReader(excludeValueQuery, new SortedList<string, object>(), conn, transaction))
                    {
                        while (reader.Read())
                        {
                            if (!reader.IsDBNull(0))
                                schemerewardpoints += (int)reader.GetDecimal(0);
                        }
                    }
                }
            }


            if (schemerewardpoints > 0)
            {
                DateTime expiry = DateTimeNow.Date.AddDays(InfyPOS.Services.ConfigManager.Instance(Context.OrganizationID).LocationConfig[oBill.locationid].settlement_reward_expiry);
                if (InfyPOS.Services.ConfigManager.Instance(Context.OrganizationID).LocationConfig[oBill.locationid].settlement_reward_expiry <= 0)
                    expiry = DateTime.MaxValue;
                else
                    expiry = expiry.AddDays(1);

                oBill.rewardpoint = schemerewardpoints;
                CustomerDataService customerDataService = new CustomerDataService() { Context = this.Context };
                oBill.availablereward = customerDataService.AddReward(transaction, conn,
                    oBill.customerid,
                    schemerewardpoints,
                    expiry,
                    oBill,
                    oBill.billattributes == null ? 0 :
                    oBill.billattributes.newreward);
            }

            if ((oBill.billattributes == null ? 0 : oBill.billattributes.newreward) > 0)
            {
                ApplyReward(transaction, conn, oBill.customerid,
                    oBill.billattributes == null ? 0 :
                    oBill.billattributes.newreward, oBill.locationid);

                oBill.availablereward -= oBill.billattributes.newreward;
            }

        }*/

    }


}
