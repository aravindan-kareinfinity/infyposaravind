using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using InfyPOS.Common;
using InfyPOS.Models;

namespace InfyPOS.Processors
{
    public class BarcodeManager
    {
        private long organizationid;
        public BarcodeManager(long organizationid)
        {
            this.organizationid = organizationid;
        }
        private static System.Collections.Concurrent.ConcurrentDictionary<long, BarcodeManager> instanceList = new System.Collections.Concurrent.ConcurrentDictionary<long, BarcodeManager>();

        public static BarcodeManager Instance(long organizationid)
        {
            BarcodeManager instance = null;
            if (instanceList.TryGetValue(organizationid, out instance))
                return instance;

            instance = new BarcodeManager(organizationid);
            if (!instanceList.TryAdd(organizationid, instance))
                instanceList.TryGetValue(organizationid, out instance);
            return instance;
        }

        public void ClearCache()
        {

        }

        System.Threading.ManualResetEvent mre = new System.Threading.ManualResetEvent(true);
        public bool Initialized { get; set; }


        public Barcode Get(int type, long companyid)
        {
            return new Barcode(organizationid, type, companyid);
        }
        public Barcode Get(BarcodeType type, long companyid)
        {
            return new Barcode(organizationid, type, companyid);
        }
        public enum BarcodeType
        {
            Base62,
            Base36,
            Base10,
            Pack10,
            PACKSERIAL
        }

        public class Barcode
        {
            public long organizationid { get; set; }
            public long companyid { get; set; }
            public BarcodeType Type { get; set; }
            public Barcode(long organizationid, int type, long companyid)
            {
                Type = (BarcodeType)type;
                this.companyid = companyid;
                this.organizationid = organizationid;
            }

            public Barcode(long organizationid, BarcodeType type, long companyid)
            {
                Type = type;
                this.companyid = companyid;
                this.organizationid = organizationid;
            }
            public int CodeValue { get; set; }
            public void SetValue(int code)
            {
                this.CodeValue = code;
            }


            public Common.SequenceNo GetSequenceNo()
            {
                if (companyid > 0 && !InfyPOS.Services.ConfigManager.Instance(organizationid).Config.system_multicompany)
                    companyid = 0;

                switch (Type)
                {
                    case BarcodeType.Base10:
                        return Common.SequenceManager.Instance(organizationid).Get("stock.barcode10", 0, companyid);
                    case BarcodeType.Base36:
                        return Common.SequenceManager.Instance(organizationid).Get("stock.barcode36", 0, companyid);
                    case BarcodeType.Pack10:
                        return Common.SequenceManager.Instance(organizationid).Get("stock.pack10", 0, companyid);
                    case BarcodeType.PACKSERIAL:
                        return Common.SequenceManager.Instance(organizationid).Get("stock.packserial", 0, companyid);
                }
                return Common.SequenceManager.Instance(organizationid).Get("stock.barcode62", 0, companyid);
            }


            public Common.SequenceNo GetSequenceNo(long locationid)
            {
                if (companyid > 0 && !InfyPOS.Services.ConfigManager.Instance(organizationid).Config.system_multicompany)
                    companyid = 0;

                switch (Type)
                {
                    case BarcodeType.Base10:
                        return Common.SequenceManager.Instance(organizationid).Get("stock.barcode10", locationid, companyid);
                    case BarcodeType.Base36:
                        return Common.SequenceManager.Instance(organizationid).Get("stock.barcode36", locationid, companyid);
                    case BarcodeType.Pack10:
                        return Common.SequenceManager.Instance(organizationid).Get("stock.pack10", locationid, companyid);
                    case BarcodeType.PACKSERIAL:
                        return Common.SequenceManager.Instance(organizationid).Get("stock.Serial", locationid, companyid);
                }
                return Common.SequenceManager.Instance(organizationid).Get("stock.barcode62", locationid, companyid);
            }

            public string GetValue(string source, int sequence)
            {
                if (Type == BarcodeType.Base36)
                    return sequence.ToBase36();
                if (Type == BarcodeType.Base62)
                    return sequence.ToBase62();

                int startIndex = source.LastIndexOfAny("ABCDEFGHIJKLMNOPQRSTUVWXYZ".ToCharArray());
                if (startIndex > 0)
                    return source.Substring(0, startIndex) + sequence.ToString("F0");
                return sequence.ToString("F0");
            }

            public int GetValue(string value)
            {
                if (Type == BarcodeType.Base36)
                    return value.IntFromBase36();
                if (Type == BarcodeType.Base62)
                    return value.IntFromBase62();
                int returnValue = 0;
                int startIndex = value.LastIndexOfAny("ABCDEFGHIJKLMNOPQRSTUVWXYZ".ToCharArray());
                if (startIndex >= 0)
                    value = value.Substring(startIndex + 1);
                if (int.TryParse(value, out returnValue))
                    return returnValue;

                return -1;
            }

            public string GetValue(UserClaim Context, SequenceNo sequence)
            {
                if (Type == BarcodeType.Base36)
                    return CodeValue.ToBase36();
                if (Type == BarcodeType.Base62)
                    return CodeValue.ToBase62();

                return sequence.GetFormatedValue(Context, null, CodeValue.ToString("F0"));
            }

            public string GetValue(UserClaim Context, SequenceNo sequence, int CodeValue)
            {
                if (Type == BarcodeType.Base36)
                    return CodeValue.ToBase36();
                if (Type == BarcodeType.Base62)
                    return CodeValue.ToBase62();

                return sequence.GetFormatedValue(Context, null, CodeValue.ToString("F0"));
            }


            public string CompressCode10(List<NewBarcodeID> jsonbarcodenew, List<string> initial)
            {
                List<string> barcodenew = new List<string>();
                if (initial != null && initial.Count > 0)
                    barcodenew.AddRange(initial);

                if (jsonbarcodenew.Count > 1)
                {
                    string firststring = jsonbarcodenew.First().code;
                    string laststring = jsonbarcodenew.First().code;
                    foreach (var item in jsonbarcodenew)
                    {
                        if (GetValue(item.code) - GetValue(laststring) > 1)
                        {
                            if (GetValue(item.code) == GetValue(laststring))
                                barcodenew.Add(item.code);
                            else
                                barcodenew.Add(firststring + "-" + laststring);
                            firststring = item.code;
                            laststring = item.code;
                        }
                        else
                        {
                            laststring = item.code;
                        }
                    }
                    if (firststring == laststring)
                        barcodenew.Add(firststring);
                    else
                        barcodenew.Add(firststring + "-" + laststring);
                }
                else if (jsonbarcodenew.Count == 1)
                {
                    barcodenew.Add(jsonbarcodenew[0].code);
                    return Newtonsoft.Json.JsonConvert.SerializeObject(barcodenew);
                }
                return Newtonsoft.Json.JsonConvert.SerializeObject(barcodenew);
            }

            public string CompressCode36(List<NewBarcodeID> jsonbarcodenew, List<string> initial)
            {
                List<string> barcodenew = new List<string>();
                if (initial != null && initial.Count > 0)
                    barcodenew.AddRange(initial);

                if (jsonbarcodenew.Count > 1)
                {
                    int first = jsonbarcodenew.First().code.IntFromBase36();
                    int lastno = jsonbarcodenew.First().code.IntFromBase36();
                    foreach (var item in jsonbarcodenew)
                    {
                        if (item.code.IntFromBase36() - lastno > 1)
                        {
                            if (first == lastno)
                                barcodenew.Add(first.ToBase36());
                            else
                                barcodenew.Add(first.ToBase36() + "-" + lastno.ToBase36());
                            first = item.code.IntFromBase36();
                            lastno = item.code.IntFromBase36();
                        }
                        else
                        {
                            lastno = item.code.IntFromBase36();
                        }
                    }
                    if (first == lastno)
                        barcodenew.Add(first.ToBase36());
                    else
                        barcodenew.Add(first.ToBase36() + "-" + lastno.ToBase36());
                }
                else if (jsonbarcodenew.Count == 1)
                {
                    barcodenew.Add(jsonbarcodenew[0].code);
                    return Newtonsoft.Json.JsonConvert.SerializeObject(barcodenew);
                }
                return Newtonsoft.Json.JsonConvert.SerializeObject(barcodenew);
            }

            public string CompressCode62(List<NewBarcodeID> jsonbarcodenew, List<string> initial)
            {
                List<string> barcodenew = new List<string>();
                if (initial != null && initial.Count > 0)
                    barcodenew.AddRange(initial);

                if (jsonbarcodenew.Count > 1)
                {
                    int first = jsonbarcodenew.First().code.IntFromBase62();
                    int lastno = jsonbarcodenew.First().code.IntFromBase62();
                    if (first == -1)
                    {
                        barcodenew.AddRange(jsonbarcodenew.ConvertAll(e => e.code));
                    }
                    else
                    {
                        foreach (var item in jsonbarcodenew)
                        {
                            if (item.code.IntFromBase62() - lastno > 1)
                            {
                                if (first == lastno)
                                    barcodenew.Add(first.ToBase62());
                                else
                                    barcodenew.Add(first.ToBase62() + "-" + lastno.ToBase62());
                                first = item.code.IntFromBase62();
                                lastno = item.code.IntFromBase62();
                            }
                            else
                            {
                                lastno = item.code.IntFromBase62();
                            }
                            if (lastno == -1)
                                break;
                        }
                        if (first == -1 || lastno == -1)
                        {
                            barcodenew.AddRange(jsonbarcodenew.ConvertAll(e => e.code));
                        }
                        else
                        {
                            if (first == lastno)
                                barcodenew.Add(first.ToBase62());
                            else
                                barcodenew.Add(first.ToBase62() + "-" + lastno.ToBase62());
                        }
                    }
                }
                else if (jsonbarcodenew.Count == 1)
                {
                    barcodenew.Add(jsonbarcodenew[0].code);
                    return Newtonsoft.Json.JsonConvert.SerializeObject(barcodenew);
                }
                return Newtonsoft.Json.JsonConvert.SerializeObject(barcodenew);
            }

            internal string CompresCode(List<NewBarcodeID> list, List<string> inital)
            {
                if (Type == BarcodeType.Base36)
                    return CompressCode36(list, inital);
                else if (Type == BarcodeType.Base62)
                    return CompressCode62(list, inital);
                return CompressCode10(list, inital);
            }

            public interface ISPECIALBARCODE
            {
                string GetBarcode(SortedDictionary<string, string> attributeids,
                    int barcodeid,
                    decimal price);
            }
            public class PCODBCODESCODERATE : ISPECIALBARCODE
            {

                public string GetBarcode(SortedDictionary<string, string> attributeids, int barcodeid, decimal price)
                {
                    List<string> retlist = new List<string>();
                    if (attributeids.ContainsKey("PCODE") ||
                        !string.IsNullOrEmpty(attributeids["PCODE"]))
                    {
                        if (attributeids["PCODE"].Length >= 4)
                            retlist.Add(attributeids["PCODE"].Substring(0, 4));
                        else
                            retlist.Add(attributeids["PCODE"].PadLeft(4, '0'));
                    }
                    else
                    {
                        retlist.Add("0".PadLeft(4, '0'));
                    }
                    string barcode = barcodeid.ToString("F0");
                    if (barcode.Length >= 7)
                        retlist.Add(barcode.Substring(0, 7));
                    else
                        retlist.Add(barcode.PadLeft(7, '0'));

                    if (attributeids.ContainsKey("SCODE") ||
                        !string.IsNullOrEmpty(attributeids["SCODE"]))
                    {
                        if (attributeids["SCODE"].Length >= 2)
                            retlist.Add(attributeids["SCODE"].Substring(0, 2));
                        else
                            retlist.Add(attributeids["SCODE"].PadLeft(2, '0'));
                    }
                    else
                    {
                        retlist.Add("0".PadLeft(2, '0'));
                    }

                    string pricecode = price.ToString("F0");
                    if (barcode.Length >= 5)
                        retlist.Add(barcode.Substring(0, 5));
                    else
                        retlist.Add(barcode.PadLeft(5, '0'));

                    return string.Join("", retlist);
                }
            }

            internal string GetSpecialValue(UserClaim Context,
                SequenceNo sequence,
                int newbarcode,
                MissingBarcode barcode,
                MissingStock stock)
            {
                if (Type == BarcodeType.Base36)
                    return newbarcode.ToBase36();
                if (Type == BarcodeType.Base62)
                    return newbarcode.ToBase62();


                var attribs = new SortedDictionary<string, string>();
                PCODBCODESCODERATE pCODBCODESCODERATE = new PCODBCODESCODERATE();
                if (InfyPOS.Services.ConfigManager.Instance(organizationid).Config.system_supportspecialbarcode)
                {
                    if (barcode != null)
                    {
                        attribs.Add("PCODE", GetCode("PRODUCT", barcode.productid));
                        attribs.Add("SCODE", GetCode("SIZE", barcode.sizeid));
                        return sequence.GetFormatedValue(Context, attribs, pCODBCODESCODERATE.GetBarcode(attribs, newbarcode,
                                barcode.sellingprice));
                    }
                    else if (stock != null)
                    {
                        attribs.Add("PCODE", GetCode("PRODUCT", stock.productid));
                        attribs.Add("SCODE", GetCode("SIZE", stock.sizeid));
                        return sequence.GetFormatedValue(Context, attribs, pCODBCODESCODERATE.GetBarcode(attribs, newbarcode,
                                barcode.sellingprice));
                    }
                }


                if ((!string.IsNullOrEmpty(sequence.prefix) &&
                    sequence.prefix.Contains("[PBID]")) || (!string.IsNullOrEmpty(sequence.Suffix) &&
                    sequence.Suffix.Contains("[PBID]")))
                {
                    attribs.Add("PBID", GetCode("PBID", barcode.productid));
                }

                if ((!string.IsNullOrEmpty(sequence.prefix) &&
                  sequence.prefix.Contains("[MNNO]")) || (!string.IsNullOrEmpty(sequence.Suffix) &&
                  sequence.Suffix.Contains("[MNNO]")))
                {
                    attribs.Add("MNNO", DateTime.Now.Month.ToString("F0").PadLeft(2, '0'));
                }

                if ((!string.IsNullOrEmpty(sequence.prefix) &&
                  sequence.prefix.Contains("[YRNO]")) || (!string.IsNullOrEmpty(sequence.Suffix) &&
                  sequence.Suffix.Contains("[YRNO]")))
                {
                    attribs.Add("YRNO", DateTime.Now.Year.ToString("F0").Substring(2, 2));
                }

                if (!string.IsNullOrEmpty(InfyPOS.Services.ConfigManager.Instance(organizationid).Config.system_monthencryption) &&
                   InfyPOS.Services.ConfigManager.Instance(organizationid).Config.system_monthencryption.Length == 12)
                {
                    if ((!string.IsNullOrEmpty(sequence.prefix) &&
                   sequence.prefix.Contains("[MNID]")) || (!string.IsNullOrEmpty(sequence.Suffix) &&
                   sequence.Suffix.Contains("[MNID]")))
                    {
                        attribs.Add("MNID", InfyPOS.Services.ConfigManager.Instance(organizationid).Config.system_monthencryption[DateTime.Now.Month - 1].ToString());
                    }
                }

                if (InfyPOS.Services.ConfigManager.Instance(organizationid).Config.system_yearencryptionfrom>0 &&
                    InfyPOS.Services.ConfigManager.Instance(organizationid).Config.system_yearencryption.Length == 10)
                {
                    if ((!string.IsNullOrEmpty(sequence.prefix) &&
                   sequence.prefix.Contains("[YRID]")) || (!string.IsNullOrEmpty(sequence.Suffix) &&
                   sequence.Suffix.Contains("[YRID]")))
                    {
                        var index = DateTime.Now.Year - InfyPOS.Services.ConfigManager.Instance(organizationid).Config.system_yearencryptionfrom;
                        attribs.Add("YRID", InfyPOS.Services.ConfigManager.Instance(organizationid).Config.system_yearencryption[index].ToString());
                    }
                }
                return sequence.GetFormatedValue(Context, attribs, newbarcode.ToString("F0"));
            }

            internal string GetSpecialValue(UserClaim Context,
                SequenceNo sequence,
                int barcodeno,
                Lrinvoiceitemsbarcode item)
            {
                if (Type == BarcodeType.Base36)
                    return sequence.GetFormatedValue(Context, new SortedDictionary<string, string>(), barcodeno.ToBase36());
                    //return barcodeno.ToBase36();
                if (Type == BarcodeType.Base62)
                    return sequence.GetFormatedValue(Context, new SortedDictionary<string, string>(), barcodeno.ToBase62());
                    //return barcodeno.ToBase62();
                var attribs = new SortedDictionary<string, string>();
                if (InfyPOS.Services.ConfigManager.Instance(organizationid).Config.system_supportspecialbarcode)
                {
                    attribs.Add("PCODE", GetCode("PRODUCT", item.productid));
                    attribs.Add("SCODE", GetCode("SIZE", item.Details.sizeid));
                    attribs.Add("PBID", GetCode("PBID", item.productid));
                    PCODBCODESCODERATE pCODBCODESCODERATE = new PCODBCODESCODERATE();
                    return sequence.GetFormatedValue(Context, attribs, pCODBCODESCODERATE.GetBarcode(attribs, barcodeno,
                        item.sellingprice));
                }

                if ((!string.IsNullOrEmpty(sequence.prefix) &&
                    sequence.prefix.Contains("[PBID]")) || (!string.IsNullOrEmpty(sequence.Suffix) &&
                    sequence.Suffix.Contains("[PBID]")))
                {
                    attribs.Add("PBID", GetCode("PBID", item.productid));
                }

                if ((!string.IsNullOrEmpty(sequence.prefix) &&
                   sequence.prefix.Contains("[MNNO]")) || (!string.IsNullOrEmpty(sequence.Suffix) &&
                   sequence.Suffix.Contains("[MNNO]")))
                {
                    attribs.Add("MNNO", DateTime.Now.Month.ToString("F0").PadLeft(2, '0'));
                }

                if ((!string.IsNullOrEmpty(sequence.prefix) &&
                  sequence.prefix.Contains("[YRNO]")) || (!string.IsNullOrEmpty(sequence.Suffix) &&
                  sequence.Suffix.Contains("[YRNO]")))
                {
                    attribs.Add("YRNO", DateTime.Now.Year.ToString("F0").Substring(2, 2));
                }

                if (!string.IsNullOrEmpty(InfyPOS.Services.ConfigManager.Instance(organizationid).Config.system_monthencryption) &&
                    InfyPOS.Services.ConfigManager.Instance(organizationid).Config.system_monthencryption.Length == 12)
                {
                    if ((!string.IsNullOrEmpty(sequence.prefix) &&
                   sequence.prefix.Contains("[MNID]")) || (!string.IsNullOrEmpty(sequence.Suffix) &&
                   sequence.Suffix.Contains("[MNID]")))
                    {
                        attribs.Add("MNID", InfyPOS.Services.ConfigManager.Instance(organizationid).Config.system_monthencryption[DateTime.Now.Month - 1].ToString());
                    }
                }

                if (InfyPOS.Services.ConfigManager.Instance(organizationid).Config.system_yearencryptionfrom > 0 &&
                   InfyPOS.Services.ConfigManager.Instance(organizationid).Config.system_yearencryption.Length == 10)
                {
                    if ((!string.IsNullOrEmpty(sequence.prefix) &&
                   sequence.prefix.Contains("[YRID]")) || (!string.IsNullOrEmpty(sequence.Suffix) &&
                   sequence.Suffix.Contains("[YRID]")))
                    {
                        var index = DateTime.Now.Year - InfyPOS.Services.ConfigManager.Instance(organizationid).Config.system_yearencryptionfrom;
                        attribs.Add("YRID", InfyPOS.Services.ConfigManager.Instance(organizationid).Config.system_yearencryption[index].ToString());
                    }
                }

                return sequence.GetFormatedValue(Context, attribs, barcodeno.ToString("F0"));
            }

            public CodeCache codecache = null;
            public string GetCode(string code, long id)
            {
                if (codecache == null)
                    codecache = new CodeCache();

                return codecache.GetCode(organizationid, code, id);
            }
            public class CodeCache
            {
                public SortedDictionary<long, string> productcodes { get; set; }
                public SortedDictionary<long, string> sizecodes { get; set; }
                public SortedDictionary<long, string> bidcodes { get; set; }
                public CodeCache()
                {
                    productcodes = new SortedDictionary<long, string>();
                    sizecodes = new SortedDictionary<long, string>();
                    bidcodes = new SortedDictionary<long, string>();
                }

                internal string GetCode(long organizationid, string code, long id)
                {
                    Data.ProductsDataService productsDataService = new Data.ProductsDataService() { Context = new UserClaim() { OrganizationID = organizationid } };
                    if (code == "PRODUCT")
                    {
                        if (!productcodes.ContainsKey(id))
                        {
                            string prdid = "";
                            using (var reader = productsDataService.ExecuteReader("select Products.code from Products where Products.id=" + id, null))
                            {
                                if (reader.Read())
                                {
                                    prdid = reader["code"] == DBNull.Value ? "" : reader["code"].ToString();
                                }
                            }
                            productcodes.Add(id, prdid);
                        }

                        return productcodes[id];
                    }
                    if (code == "SIZE")
                    {
                        if (!sizecodes.ContainsKey(id))
                        {
                            string szid = "";
                            using (var reader = productsDataService.ExecuteReader("select referencelist.code from referencelist where referencelist.id=" + id, null))
                            {
                                if (reader.Read())
                                {
                                    szid = reader["code"] == DBNull.Value ? "" : reader["code"].ToString();
                                }
                            }
                            sizecodes.Add(id, szid);
                        }
                        return sizecodes[id];
                    }
                    if (code == "PBID")
                    {
                        if (!bidcodes.ContainsKey(id))
                        {
                            string pbid = "";
                            using (var reader = productsDataService.ExecuteReader("select referencelist.code from products inner join referencelist on referencelist.id = barcodeid where products.id=" + id, null))
                            {
                                if (reader.Read())
                                {
                                    pbid = reader["code"] == DBNull.Value ? "" : reader["code"].ToString();
                                }
                            }
                            if (string.IsNullOrEmpty(pbid))
                            {
                                using (var reader = productsDataService.ExecuteReader("select products.name from products where products.id=" + id, null))
                                {
                                    if (reader.Read())
                                    {
                                        pbid = reader["name"] == DBNull.Value ? "" : reader["name"].ToString();
                                    }
                                }
                                throw new Exception("Please assign barcde id for " + pbid);
                            }
                            bidcodes.Add(id, pbid);
                        }
                        return bidcodes[id];
                    }

                    return "";
                }
            }
        }

        public class NewBarcodeID
        {
            public long id { get; set; }
            public string code { get; set; }
        }
    }
}