using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace InfyPOS.Report
{
    public interface IBarcodeFormat
    {
        SortedDictionary<string, Object> KeyValue { get; }
        SortedDictionary<string, string> KeyFormat { get; }
        bool SpecialField(string field, SortedDictionary<string, Object> data);
    }
    public class CitizenBarcode
    {
        public static string Encrypt(string no,string config)
        {
            string encryptstring = string.IsNullOrEmpty(config) ? "OPURCHASEN" : config;
            string encryptvalue = "0123456789";
            if (encryptstring.Length != 10)
            {
                throw new Exception("Please configure 10 digint for encriptedvalue");
            }
            string result = "";
            foreach (var chr in no.ToCharArray())
            {
                if (encryptvalue.Contains(chr))
                {
                    result += encryptstring[encryptvalue.IndexOf(chr)];
                }
            }
            return result;
        }

        public static string Encrypt(int no, string config)
        {
            string encryptstring = string.IsNullOrEmpty(config) ? "0ABCDEFGHI" : config;
            char[] encrypt = encryptstring.ToCharArray();
            string result = "";
            foreach (var chr in no.ToString("F0").ToCharArray())
            {
                int number = int.Parse(chr.ToString());
                result += encrypt[number];
            }
            return result;
        }

        public List<string> Render(List<IBarcodeFormat> entitylist)
        {
            List<string> response = new List<string>();
            for (int i = 0; i < entitylist.Count; i = i + lines)
            {
                SortedDictionary<int, List<string>> formatedLines = new SortedDictionary<int, List<string>>();
                int index = formatableLines.ContainsKey(0) ? 0 : 1;
                int jmax = i + lines;
                for (int j = i; j < jmax && j < entitylist.Count; j++)
                {
                    formatedLines.Add(index, Render(formatableLines[index], entitylist[j]));
                    index++;
                }
                System.Text.StringBuilder sb = new System.Text.StringBuilder();
                sb.AppendLine(header);
                foreach(var item in formatedLines)
                {
                    foreach (var row in item.Value)
                    {
                        sb.AppendLine(row);
                    }
                }
                //for (int r = 0; r < linesperprint; r++)
                //{
                //    foreach (var kv in formatedLines)
                //    {
                //        sb.AppendLine(kv.Value[r]);
                //    }
                //}
                sb.Append(footer);
                if (!footer.Contains("</xpml>") && !footer.EndsWith("}"))
                    sb.Append("\r\n");
                response.Add(sb.ToString());
            }
            return response;
        }

        private List<string> Render(List<string> barcodeformat, IBarcodeFormat entity)
        {
            List<string> response = new List<string>();
            SortedDictionary<string, Object> data = entity.KeyValue;
            data.Add("COMPANY", companycode);
            SortedDictionary<string, string> format = entity.KeyFormat;
            foreach (var line in barcodeformat)
            {
                var responseline = line.ToString();
                string[] fieldList = responseline.Split(new string[] { "[", "]" }, StringSplitOptions.RemoveEmptyEntries);
                foreach(var field in fieldList)
                {
                    if (field.StartsWith("SPL:"))
                        entity.SpecialField(field, data);
                }
                foreach (var kv in data)
                {
                    if (responseline.Contains(kv.Key))
                    {
                        if (kv.Value == null)
                        {
                            responseline = responseline.Replace("[" + kv.Key + "]", "");
                        }
                        else if (format.ContainsKey(kv.Key))
                        {
                            responseline = responseline.Replace("[" + kv.Key + "]", string.Format("{0:" + format[kv.Key] + "}", kv.Value));
                        }
                        else
                        {
                            responseline = responseline.Replace("[" + kv.Key + "]", kv.Value.ToString());
                        }
                    }
                }
                response.Add(responseline);
            }
            return response;
        }


//        public string EncodeBarcode(string source)
//        {
//            var current_charset = "None";
//            var pos = 0;var sum = 0;
//            var skip = false;
//            string code = source;
//            for (int c = 0; c < source.Length; c++)
//            { 
//                if (skip)
//                {
//                    skip = false;
//                    continue;
//                }


//                //Only switch to char set C if next four chars are digits
//                if ((code.Substring(c).Length >= 4 && code.Substring(c, c + 4).All(e => char.IsDigit(e)) && current_charset != "CharSetC") ||
//                    (code.Substring(c).Length >= 2 && code.Substring(c, c + 2).All(e => char.IsDigit(e)) && current_charset == "CharSetC"))

//                    //#If char set C = current and next two chars ar digits, keep C 
//                    if (current_charset != "CharSetC")
//                    {
//                        # Switching to Character set C
//                        if pos:
//                             strCode += self.ValueEncodings[current_charset['Code C']]
//                             sum += pos * current_charset['Code C']
//                         else:
//                             strCode = self.ValueEncodings[self.CharSetC['START C']]
//                             sum = self.CharSetC['START C']
//                         current_charset = self.CharSetC
//                         pos += 1
//                            }
//                  else if (self.CharSetB.has_key(code[c]) and current_charset!= self.CharSetB and \
//                       not(self.CharSetA.has_key(code[c]) and current_charset == self.CharSetA))
//            {
//# If char in chrset A = current, then just keep that
//# Switching to Character set B
//                if pos:
//                         strCode += self.ValueEncodings[current_charset['Code B']]
//                         sum += pos * current_charset['Code B']
//                     else:
//                         strCode = self.ValueEncodings[self.CharSetB['START B']]
//                         sum = self.CharSetB['START B']
//                     current_charset = self.CharSetB
//                     pos += 1
//                    }
//                  else if (self.CharSetA.has_key(code[c]) and current_charset!= self.CharSetA and \
//                       not(self.CharSetB.has_key(code[c]) and current_charset == self.CharSetB))
//            { 
//                     #if char in chrset B== current, then just keep that
//                     # Switching to Character set A
//                     if pos:
//                         strCode += self.ValueEncodings[current_charset['Code A']]
//                         sum  += pos * current_charset['Code A']
//                     else:
//                         strCode += self.ValueEncodings[self.CharSetA['START A']]
//                         sum = self.CharSetA['START A']
//                     current_charset= self.CharSetA
//                     pos+=1
//                }

//                  if current_charset==self.CharSetC:
//                     val= self.CharSetC[code[c:c+2]]
//                     skip=True
//                  else:
//                     val=current_charset[code[c]]

//                  sum += pos * val
//                  strCode += self.ValueEncodings[val]
//                  pos+=1
                        
       
            
//          return strCode
//        }

        public string EncodeBarcodeSet(string source)
        {
            if (source.Length>2 && source.All(e => Char.IsNumber(e)))
            {
                return "!105" + source;
            }
            else if (source.All(e => Char.IsLetter(e)) || source.Length<=4)
            {
                return "!104" + source;
            }
            else
            {
                List<string> result = new List<string>();
                bool isdigit= true;
                foreach(char character in source)
                {
                    if(result.Count == 0)
                    {
                        isdigit = char.IsDigit(character);
                        result.Add(character.ToString());
                    }
                    else if(char.IsDigit(character) == isdigit)
                    {
                        result[result.Count - 1] = result[result.Count - 1] + character;
                    }
                    else 
                    {
                        isdigit = char.IsDigit(character);
                        result.Add(character.ToString());
                    }
                }
                //if(result)

            }
            return "";
        }
        string header = "";
        string footer = "";
        SortedDictionary<int, List<string>> formatableLines = null;
        int lines;
        string companycode = "";
        int linesperprint = 0;
        public CitizenBarcode(string companycode,string document,string mode, int lines)
        {
            this.lines = lines;
            this.companycode = companycode;
            bool iszebra = document.IndexOf("<ZBODY>") > 0 && document.IndexOf("</ZBODY>") > 0;
            string[] content = document.Split(new string[] { "<BODY>", "</BODY>", "<ZBODY>", "</ZBODY>" }, StringSplitOptions.RemoveEmptyEntries);
            if (content.Length == 3)
            {
                header = string.Join("\r\n", content[0].Split(new string[] { "\r\n", "\r", "\n" }, StringSplitOptions.RemoveEmptyEntries));
                footer = string.Join("\r\n", content[2].Split(new string[] { "\r\n", "\r", "\n" }, StringSplitOptions.RemoveEmptyEntries));

                List<string> linestring = new List<string>(content[1].Split(new string[] { "\r\n", "\r", "\n" }, StringSplitOptions.RemoveEmptyEntries));
                linesperprint = linestring.Count / lines;
                formatableLines = new SortedDictionary<int, List<string>>();
                if (iszebra)
                {
                    int index = 0;
                    for (int i = 0; i < linestring.Count; i++)
                    {
                        string indexstring = linestring[i].Substring(0, linestring[i].IndexOf(":"));
                        string printcontent = linestring[i].Substring(linestring[i].IndexOf(":") + 1);
                        if (int.TryParse(indexstring, out index))
                        {
                            if (!formatableLines.ContainsKey(index))
                                formatableLines.Add(index, new List<string>());
                            formatableLines[index].Add(printcontent);
                        }
                    }
                }
                else if (!string.IsNullOrEmpty(mode) && mode.ToLower() == "block")
                {
                    int index = 0;
                    for (int i=0;i< lines;i++)
                    {
                        for (int j = 0; j < linesperprint; j++)
                        {
                            if (!formatableLines.ContainsKey(i))
                                formatableLines.Add(i, new List<string>());
                            formatableLines[i].Add(linestring[index]);
                            index++;
                        }
                    }
                }
                else
                {
                    for (int i = 0; i < linestring.Count; i++)
                    {
                        int index = i % lines;
                        //if(!string.IsNullOrEmpty(mode) && mode.ToLower() == "block")
                        //{
                        //    index = (int)Math.Ceiling((decimal)(i + 1) / (decimal)linesperprint);
                        //}
                        if (!formatableLines.ContainsKey(index))
                            formatableLines.Add(index, new List<string>());
                        formatableLines[index].Add(linestring[i]);
                    }
                }
            }
        }

        public static void Test()
        {
            string document = System.IO.File.ReadAllText(@"C:\Users\ELAYARAJA\Desktop\TEST.prn");
            Models.Stock2Print entity = new Models.Stock2Print(0);
            entity.barcode = "123455";
            entity.price = 1000;
            List<IBarcodeFormat> printlist = new List<IBarcodeFormat>();
            printlist.Add(entity);
            printlist.Add(entity);
            printlist.Add(entity);
            printlist.Add(entity);
            CitizenBarcode barcodeprint = new CitizenBarcode("",document, "BLOCK", 2);
            barcodeprint.Render(printlist);
        }
    }
}