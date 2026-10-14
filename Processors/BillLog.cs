using InfyPOS.Common;
using InfyPOS.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace InfyPOS.Processors
{
    public class BillLog :BaseData
    {
        long organizationid = 0;
        public BillLog(long organizationid)
        {
            this.organizationid = organizationid;
            this.Context = new UserClaim() { OrganizationID = organizationid };
        }

        private static System.Collections.Concurrent.ConcurrentDictionary<long, BillLog> instanceList = new System.Collections.Concurrent.ConcurrentDictionary<long, BillLog>();
        public static BillLog Instance(long organizationid)
        {
            BillLog instance = null;
            if (instanceList.TryGetValue(organizationid, out instance))
                return instance;

            instance = new BillLog(organizationid);
            if (!instanceList.TryAdd(organizationid, instance))
                instanceList.TryGetValue(organizationid, out instance);
            return instance;
        }

        public void Log(Bill bill, Npgsql.NpgsqlTransaction transaction, Npgsql.NpgsqlConnection conn)
        {
            string query = @"insert into log_bill(id,receivable,tax,rounding)
                             select id,receivable,tax,rounding from bill where id=@billid;

            insert into log_billitems(id,schemediscount,taxpercentage,schemeid,discount ,receivable ,tax,itemtax,rate,saleamount,price,salerate)
            select id,schemediscount,taxpercentage,schemeid,discount,receivable,tax,itemtax,rate,saleamount,price,salerate from billitems where billid=@billid and isactive=true";
            var param = new SortedList<string, object>();
            param.Add("billid", bill.id);
            ExecuteQuery(query, param,conn,transaction);
        }

        public void Log(Billsettlementmaster billsettlementmaster, Npgsql.NpgsqlTransaction transaction, Npgsql.NpgsqlConnection conn)
        {
            string query = @"insert into log_billsettlement(id,billamount,receivable,paid)
                             select id,billamount,receivable,paid from billsettlement where billsettlementmasterid=@billsettlementmasterid;

                            insert into log_billsettlementmaster(id,billamount,paidamount,receivable,paymentinfo)
                            select id,billamount,paidamount,receivable,paymentinfo from billsettlementmaster where id=@billsettlementmasterid;";

            var param = new SortedList<string, object>();
            param.Add("billsettlementmasterid", billsettlementmaster.id);
            ExecuteQuery(query, param, conn, transaction);
        }

    }
}
