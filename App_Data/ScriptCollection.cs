using Npgsql;
using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.Web;

namespace InfyPOS.Common
{
    public class ScriptCollection : SAASData
    {
        public bool RunScript(string name)
        {
            string costsync = @"update stock set buyingprice=lrinvoiceitems.buyingprice,ibuyingprice=
                                    (case when lrinvoiceitems.purchasetax>0 and lrinvoiceitems.buyingqty>0
	                                then (lrinvoiceitems.purchasetax/lrinvoiceitems.buyingqty)::numeric else 0::numeric end )+lrinvoiceitems.buyingprice::numeric 
                                from lrinvoiceitems
                                where lrinvoiceitems.id = invoiceitemid and stock.buyingprice  != lrinvoiceitems.buyingprice;

                                update salablegoods set costprice = buyingprice,icostprice = ibuyingprice from stock
                                where stock.id = salablegoods.stockid and COALESCE(salablegoods.costprice,0) != stock.buyingprice";

            string query = "";

            if (name == "costsync")
                query = costsync;

            if(!string.IsNullOrEmpty(query))
                return ExecuteQuery(query, new SortedList<string, object>()) > 0;

            return false;
        }
    }

}