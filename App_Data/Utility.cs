using Newtonsoft.Json;
using Npgsql;
using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Web;

namespace WebAPI.Data
{
    public static class Utility
    {
        public static string SafeSQL(string value)
        {
            // .Replace("0x", "") 
            if (string.IsNullOrEmpty(value)) return "";

            return value.Replace("'", "").Replace("\"", "").Replace(";", "").Replace("--", "")
                .Replace("/*", "").Replace("*/", "").Replace("CHAR(", "")
                .Replace("ASCII(", "");
        }
        public static System.Drawing.Image FromBytes(this InfyPOS.Common.ImageData source, byte[] data)
        {
            try
            {
                return (Bitmap)((new ImageConverter()).ConvertFrom(data));
            }
            catch(Exception exp)
            {
                return null;
            }
        }

        public static List<List<T>> ChunkBy<T>(this List<T> source, int chunkSize)
        {
            return source
                .Select((x, i) => new { Index = i, Value = x })
                .GroupBy(x => x.Index / chunkSize)
                .Select(x => x.Select(v => v.Value).ToList())
                .ToList();
        }
        public static string ToBase64(this byte[] source)
        {
            //        +/=
            string s = Convert.ToBase64String(source); // Regular base64 encoder
            s = s.Split('=')[0]; // Remove any trailing '='s
            s = s.Replace('+', '-'); // 62nd char of encoding
            s = s.Replace('/', '_'); // 63rd char of encoding
            return s;
        }

        public static byte[] FromBase64(this string source)
        {
            string s = source;
            s = s.Replace('-', '+'); // 62nd char of encoding
            s = s.Replace('_', '/'); // 63rd char of encoding
            switch (s.Length % 4) // Pad with trailing '='s
            {
                case 0: break; // No pad chars in this case
                case 2: s += "=="; break; // Two pad chars
                case 3: s += "="; break; // One pad char
                default:
                    return null;
            }
            return Convert.FromBase64String(s);
        }

        public static string CenterText(this string input, int length)
        {
            input = input == null ? "" : input.Trim();
            if (input.Length < length)
            {
                int spacewidth = length / 2 - input.Length / 2;
                if (spacewidth > 0)
                {
                    string sp = new string(' ', spacewidth);
                    input = sp + input + sp;
                }
                if (input.Length < length) input = input + (new string(' ', length - input.Length));
            }
            else
            {
                input = input.Substring(0, length);
            }
            return input;

        }
        public static string PadRightFixed(this string source,int length)
        {
            return source.PadRight(length).Substring(0, length);
        }

        public static string PadLeftFixed(this string source, int length)
        {
            return source.PadLeft(length).Substring(0, length);
        }

        public static IEnumerable<TSource> DistinctBy<TSource, TKey>
    (this IEnumerable<TSource> source, Func<TSource, TKey> keySelector)
        {
            HashSet<TKey> seenKeys = new HashSet<TKey>();
            foreach (TSource element in source)
            {
                if (seenKeys.Add(keySelector(element)))
                {
                    yield return element;
                }
            }
        }

        public static string ToJSON(this object obj)
        {
            return JsonConvert.SerializeObject(obj);
        }
        public static string Encrypt(this string source, string additionalKey)
        {
            return source;
        }
        public static string Decrypt(this string source, string additionalKey)
        {
            return source;
        }

        public static T ToEnum<T>(this string source) where T : struct
        {
            T t = new T();
            Enum.TryParse<T>(source, out t);
            return t;
        }

        public static T Json2Object<T>(this string source)
        {
            return (T)JsonConvert.DeserializeObject(source, typeof(T));
        }
        public static DateTimeOffset Convert2Timezone(this DateTime source)
        {
            return (DateTimeOffset)source.ToUniversalTime();
        }
        public static string OTP(int lenthofpass)
        {
            string allowedChars = "";
            allowedChars = "a,b,c,d,e,f,g,h,i,j,k,l,m,n,o,p,q,r,s,t,u,v,w,x,y,z,";
            allowedChars += "A,B,C,D,E,F,G,H,I,J,K,L,M,N,O,P,Q,R,S,T,U,V,W,X,Y,Z,";
            allowedChars += "2,3,4,5,6,7,8,9";
            char[] sep = { ',' };
            string[] arr = allowedChars.Split(sep);
            string passwordString = "";
            string temp = "";
            Random rand = new Random();
            for (int i = 0; i < lenthofpass; i++)
            {
                temp = arr[rand.Next(0, arr.Length)];
                passwordString += temp;
            }
            return passwordString;
        }
        public static MemoryStream AddAutoPrint(this Stream pdfStream, bool ShowPrintDialog = true, int NumCopies = 1)
        {
            PdfSharp.Pdf.PdfDocument doc = PdfSharp.Pdf.IO.PdfReader.Open(pdfStream, PdfSharp.Pdf.IO.PdfDocumentOpenMode.Import);
            PdfSharp.Pdf.PdfDocument outputDocument = new PdfSharp.Pdf.PdfDocument();

            for (int idx = 0; idx < doc.PageCount; idx++)
            {
                PdfSharp.Pdf.PdfPage p = doc.Pages[idx];
                outputDocument.AddPage(p);
            }

            outputDocument.Info.Author = "author name";

            string JSScript = string.Empty;
            JSScript += "var pp = this.getPrintParams(); ";

            if (NumCopies > 0)
            {
                JSScript += "pp.NumCopies = " + NumCopies.ToString() + "; ";
            }

            if (!ShowPrintDialog)
            {
                JSScript += "pp.interactive = pp.constants.interactionLevel.automatic; ";
            }


            JSScript += "this.print({printParams: pp}); ";


            PdfSharp.Pdf.PdfDictionary dictJS = new PdfSharp.Pdf.PdfDictionary();
            dictJS.Elements["/S"] = new PdfSharp.Pdf.PdfName("/JavaScript");
            //dictJS.Elements["/JS"] = new PdfSharp.Pdf.PdfStringObject(outputDocument, "print(true);");
            //dictJS.Elements["/JS"] = new PdfSharp.Pdf.PdfStringObject(outputDocument, "this.print({bUI: false, bSilent: true, bShrinkToFit: true});");
            //dictJS.Elements["/JS"] = new PdfSharp.Pdf.PdfStringObject(outputDocument, "var pp = this.getPrintParams(); pp.NumCopies = 2; pp.interactive = pp.constants.interactionLevel.automatic; this.print({printParams: pp});");
            dictJS.Elements["/JS"] = new PdfSharp.Pdf.PdfStringObject(outputDocument, JSScript);


            outputDocument.Internals.AddObject(dictJS);

            PdfSharp.Pdf.PdfDictionary dict = new PdfSharp.Pdf.PdfDictionary();
            PdfSharp.Pdf.PdfArray a = new PdfSharp.Pdf.PdfArray();
            dict.Elements["/Names"] = a;
            a.Elements.Add(new PdfSharp.Pdf.PdfString("EmbeddedJS"));
            a.Elements.Add(PdfSharp.Pdf.Advanced.PdfInternals.GetReference(dictJS));

            outputDocument.Internals.AddObject(dict);

            PdfSharp.Pdf.PdfDictionary group = new PdfSharp.Pdf.PdfDictionary();
            group.Elements["/JavaScript"] = PdfSharp.Pdf.Advanced.PdfInternals.GetReference(dict);

            outputDocument.Internals.Catalog.Elements["/Names"] = group;

            MemoryStream ms = new MemoryStream();

            outputDocument.Save(ms, false);

            return ms;
        }

        public class Table
        {
            public override string ToString()
            {
                return name;
            }
            public string CreateTableScript()
            {
                if (columns == null || columns.Count == 0) return "";
                System.Text.StringBuilder stringBuilder = new System.Text.StringBuilder();
                //stringBuilder.Append(@"CREATE TABLE public.agent(");
                foreach (var item in columns)
                {
                    stringBuilder.AppendLine((stringBuilder.Length > 0 ? "," : "") + item.toscript());
                }
                if (string.IsNullOrEmpty(name))
                    return "";
                if (name == "triggers")
                    return "";
                stringBuilder.Insert(0, "CREATE TABLE public." + name + "(");
                stringBuilder.Append(");");
                return stringBuilder.ToString();
            }

            public string CreateDeleteScript()
            {
                if (columns == null || columns.Count == 0 || string.IsNullOrEmpty(name)) return "";


                return string.Format("delete from {0};", name);
            }


            public string name { get; set; }

            public List<Column> columns { get; set; }
            public List<Index> indexes { get; set; }
            public List<Routine> routines { get; set; }
            public List<Trigger> triggers { get; set; }

            public class Trigger
            {
                public string name { get; set; }
                public string create { get; set; }
                public string drop { get; set; }
            }
            public class Routine
            {
                public string name { get; set; }
                public string description { get; set; }
                public string drop { get; set; }
            }
            public class Column
            {
                public bool isautoincrement { get; set; }
                public string column_name { get; set; }
                public string data_type { get; set; }
                public int character_maximum_length { get; set; }
                public string defaultvalue { get; set; }
                public int numeric_precision { get; set; }
                public int numeric_scale { get; set; }


                public string toscript()
                {
                    if (",methodName,killedBy,loginOn,logoutOn,".Contains("," + column_name + ","))
                        return character_maximum_length > 0 ?
                            string.Format("\"{0}\" {1} ({2}) {3}", column_name, data_type, character_maximum_length,
                            string.IsNullOrEmpty(defaultvalue) ? "" : (" DEFAULT " + defaultvalue)) :
                            string.Format("\"{0}\" {1}", column_name, data_type, character_maximum_length,
                            string.IsNullOrEmpty(defaultvalue) ? "" : (" DEFAULT " + defaultvalue));

                    if(numeric_precision > 0 &&
                            numeric_scale > 0)
                        return string.Format("{0} {1} ({2},{3}) {4}", column_name, data_type, numeric_precision, numeric_scale,
                        string.IsNullOrEmpty(defaultvalue) ? "" : (" DEFAULT " + defaultvalue));

                    return character_maximum_length > 0 ?
                        string.Format("{0} {1} ({2}) {3}", column_name, data_type, character_maximum_length,
                        string.IsNullOrEmpty(defaultvalue) ? "" : (" DEFAULT " + defaultvalue)) :
                        string.Format("{0} {1} {3}", column_name, data_type, character_maximum_length,
                        string.IsNullOrEmpty(defaultvalue) ? "" : (" DEFAULT " + defaultvalue));
                }
            }

            public class Index
            {
                public string name { get; set; }
                public string script { get; set; }
            }
        }

        public static byte[] DownloadSchema(string connectionstring)
        {
            var tables = CreateSchema(connectionstring);
            string json = JsonConvert.SerializeObject(tables);
            var encryptor = new WebAPI.Data.Cryptor.AESEncryption("InfyPOS", "Elayaraja");
            return encryptor.Encrypt(json);
        }

        private static void SyncSequenceNo(long organizationid)
        {
            string reset = @"ALTER SEQUENCE {0} RESTART WITH {1};";

            string query = @"select *
                                    from (select n.nspname,c.relname,
                                    (select substring(pg_catalog.pg_get_expr(d.adbin, d.adrelid) for 128)
                                    from pg_catalog.pg_attrdef d
                                    where d.adrelid=a.attrelid
                                    and d.adnum=a.attnum
                                    and a.atthasdef) as def
                                    from pg_class c, pg_attribute a, pg_namespace n
                                    where c.relkind='r'
                                    and c.oid=a.attrelid
                                    and n.oid=c.relnamespace
                                    and a.atthasdef
                                    and a.atttypid=20) x
                                    where x.def ~ '^nextval'
                                    order by nspname,relname;";

            List<Tuple<string, string>> list = new List<Tuple<string, string>>();

            InfyPOS.Common.BaseData baseData = new InfyPOS.Common.BaseData() { Context = new InfyPOS.Common.UserClaim() { OrganizationID = organizationid } };
            using (var reader = baseData.ExecuteReader(query, new SortedList<string, object>()))
            {
                while (reader.Read())
                {
                    list.Add(new Tuple<string, string>(reader.GetString(1),
                        reader.GetString(2).Split(new string[] { "'" }, StringSplitOptions.None)[0]));
                }
            }
        }

        public static List<Table> CreateSchema(string connectionstring)
        {
            /*
             * select column_name,
tables.table_name, 'update '||tables.table_name||' '||column_name||' = 1 '||' where '||column_name||' != 1;'  from INFORMATION_SCHEMA.TABLES tables
inner join INFORMATION_SCHEMA.COLUMNS columns on columns.table_name = tables.table_name
where tables.table_schema = 'public' and tables.table_type='BASE TABLE'
and column_name ilike '%company%'
order by tables.table_name;*/

            /*select '(select '''||tables.table_name||''',modifiedby from '
            ||tables.table_name||' order by id desc limit 1) union all '
            ,column_name, tables.table_name  from INFORMATION_SCHEMA.TABLES tables
            inner join INFORMATION_SCHEMA.COLUMNS columns on columns.table_name = tables.table_name
            where tables.table_schema = 'public' and tables.table_type='BASE TABLE' 
            and column_name ='modifiedby'
            order by tables.table_name*/


            string cleardb = @"CREATE OR REPLACE FUNCTION drop_all_indexes() RETURNS INTEGER AS $$
DECLARE
  i RECORD;
BEGIN
  FOR i IN 
    (SELECT relname FROM pg_class
       -- exclude all pkey, exclude system catalog which starts with 'pg_'
      WHERE relkind = 'i' AND relname NOT LIKE '%_pkey%' 
	 AND relname NOT LIKE 'pg_%' and
	  relname NOT LIKE 'hints_norm_and_app')
  LOOP
    -- RAISE INFO 'DROPING INDEX: %', i.relname;
    EXECUTE 'DROP INDEX ' || i.relname;
  END LOOP;
RETURN 1;
END;
$$ LANGUAGE plpgsql;

SELECT drop_all_indexes();

alter table company
drop column tallyconfig;
alter table referencelist
drop column additionalinfo;
delete from file;
alter table promotions
drop column rules;
alter table promotions
drop column benefit;
delete from users;
delete from usersession;
truncate kcs2loyelty;
truncate kcs3loyelty;
truncate kcsproducttax;
truncate kcs2stock;
truncate kcsstock;
truncate trace;
truncate bulkbillitems;
truncate bulkbill;
truncate stocktransaction;
truncate smshistory;
truncate gst_inbox;
truncate voucher;
truncate ledger;";

            string columnInfo = @"select column_name, data_type, character_maximum_length,
            tables.table_name,columns.column_default,numeric_precision,numeric_scale from INFORMATION_SCHEMA.TABLES tables
            inner join INFORMATION_SCHEMA.COLUMNS columns on columns.table_name = tables.table_name
            where tables.table_schema = 'public' and tables.table_type='BASE TABLE' order by tables.table_name;
            SELECT indexname,indexdef,tablename FROM pg_indexes WHERE schemaname = 'public';
            SELECT p.proname as name, pg_get_functiondef(p.oid) createquery FROM pg_proc p
            INNER JOIN pg_namespace ns ON p.pronamespace = ns.oid
            WHERE ns.nspname = 'public';

            select matviewname as name,'CREATE MATERIALIZED VIEW '|| matviewname || ' AS ' ||definition as createquery,
            'DROP MATERIALIZED VIEW '||matviewname||';' as dropquery
            from pg_matviews
            Union all
            select 
            table_name as name,
            'CREATE VIEW '||table_name||' AS '||view_definition as createquery,
            'DROP VIEW '||table_name||';' as dropquery
            from information_schema.views
            where table_schema not in ('information_schema', 'pg_catalog');

            select  trigger_name as name,'drop trigger '|| trigger_name ||' on '||event_object_table dropquery,
            'CREATE TRIGGER '||trigger_name||' '||action_timing||' '||event_manipulation||' ON '||event_object_table||' FOR EACH '||action_orientation||' '||action_statement createquery
            from information_schema.triggers t;";

            SortedDictionary<string, Table> tables = new SortedDictionary<string, Table>();
            using (NpgsqlConnection conn2 = new NpgsqlConnection(connectionstring))
            {
                conn2.Open();
                // Define a query returning a single row result set
                NpgsqlCommand command = new NpgsqlCommand(columnInfo, conn2);
                using (System.Data.IDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        Table.Column clmn = new Table.Column();
                        clmn.column_name = reader.GetString(0);
                        clmn.data_type = reader.GetString(1);


                        clmn.character_maximum_length = reader.IsDBNull(2) ? 0 : reader.GetInt32(2);
                        if (!string.IsNullOrEmpty(reader.IsDBNull(4) ? "" : reader.GetString(4)) &&
                            reader.GetString(4).StartsWith("nextval"))
                            clmn.data_type = "bigserial";
                        string tablename = reader.GetString(3);
                        string columndefault = reader.IsDBNull(4) ? "" : reader.GetString(4);
                        clmn.defaultvalue = columndefault.StartsWith("nextval(") ? "" : columndefault;
                        clmn.numeric_precision = reader.IsDBNull(5) ? 0 : reader.GetInt32(5);
                        clmn.numeric_scale = reader.IsDBNull(6) ? 0 : reader.GetInt32(6);

                        if (!tables.ContainsKey(tablename))
                            tables.Add(tablename, new Table()
                            {
                                columns = new List<Table.Column>(),
                                indexes = new List<Table.Index>(),
                                name = tablename
                            });

                        tables[tablename].columns.Add(clmn);
                    }
                    if (reader.NextResult())
                    {
                        while (reader.Read())
                        {
                            //indexname,indexdef,tablename
                            Table.Index index = new Table.Index();
                            index.name = reader.GetString(0);
                            index.script = reader.GetString(1);
                            string tablename = reader.GetString(2);
                            if (!tables.ContainsKey(tablename))
                                tables.Add(tablename, new Table()
                                {
                                    columns = new List<Table.Column>(),
                                    indexes = new List<Table.Index>(),
                                    name = tablename
                                });

                            tables[tablename].indexes.Add(index);
                        }
                    }

                    if (reader.NextResult())
                    {
                        tables.Add("Functions", new Table()
                        {
                            name = "Functions",
                            columns = new List<Table.Column>(),
                            indexes = new List<Table.Index>(),
                            routines = new List<Table.Routine>()
                        });
                        while (reader.Read())
                        {
                            tables["Functions"].routines.Add(new Table.Routine()
                            {
                                name = reader["name"].ToString(),
                                description = reader["createquery"].ToString()
                            });
                        }
                    }

                    if (reader.NextResult())
                    {
                        tables.Add("MaterialView", new Table()
                        {
                            name = "MaterialView",
                            columns = new List<Table.Column>(),
                            indexes = new List<Table.Index>(),
                            routines = new List<Table.Routine>()
                        });
                        while (reader.Read())
                        {
                            tables["MaterialView"].routines.Add(new Table.Routine()
                            {
                                name = reader["name"].ToString(),
                                description = reader["createquery"].ToString(),
                                drop = reader["dropquery"].ToString()
                            });
                        }
                    }

                    if (reader.NextResult())
                    {
                        tables.Add("Triggers", new Table()
                        {
                            name = "Triggers",
                            columns = new List<Table.Column>(),
                            indexes = new List<Table.Index>(),
                            triggers = new List<Table.Trigger>()
                        });
                        while (reader.Read())
                        {
                            tables["Triggers"].triggers.Add(new Table.Trigger()
                            {
                                name = reader["name"].ToString(),
                                create = reader["createquery"].ToString(),
                                drop = reader["dropquery"].ToString()
                            });
                        }
                    }
                }
                conn2.Close();
            }

            return tables.Values.ToList();
        }

        public static List<Table> CreateSchema(byte[] content)
        {
            System.Text.StringBuilder sb = new System.Text.StringBuilder();
            var encryptor = new WebAPI.Data.Cryptor.AESEncryption("InfyPOS", "Elayaraja");
            string json = encryptor.Decrypt(content);
            return JsonConvert.DeserializeObject<List<Table>>(json);
        }

        public static byte[] CompareSchema(byte[] content, string connectionstring)
        {
            System.Text.StringBuilder sb = new System.Text.StringBuilder();
            var encryptor = new WebAPI.Data.Cryptor.AESEncryption("InfyPOS", "Elayaraja");
            string json = encryptor.Decrypt(content);
            var sourcetables = JsonConvert.DeserializeObject<List<Table>>(json);
            var destinationtables = CreateSchema(connectionstring);
            var newtables = sourcetables.FindAll(e => !destinationtables.Exists(f => f.name == e.name)).ToList();
            var oldtables = sourcetables.FindAll(e => destinationtables.Exists(f => f.name == e.name)).ToList();
            if (newtables.Count > 0)
            {
                foreach (var item in newtables)
                {
                    if (item.name == "Functions" || item.name == "Triggers") continue;
                    if (item.columns != null && item.columns.Count > 0)
                        sb.AppendLine(item.CreateTableScript());
                }
            }

            foreach (var item in oldtables)
            {
                if (item.columns == null || item.columns.Count == 0) continue;
                if (item.name == "Functions" || item.name == "Triggers" || item.name == "MaterialView") continue;
                System.Text.StringBuilder sbtable = new System.Text.StringBuilder();
                var destinationtable = destinationtables.Find(e => e.name == item.name);
                foreach (var column in item.columns)
                {
                    if (!destinationtable.columns.Exists(e => e.column_name == column.column_name))
                    {
                        if (sbtable.Length > 0)
                            sbtable.AppendLine(",");
                        if (column.character_maximum_length > 0)
                            sbtable.Append(string.Format(" ADD {1} {2}({3}) {4}", item.name, column.column_name, column.data_type,
                                column.character_maximum_length, string.IsNullOrEmpty(column.defaultvalue) ? "" : (" DEFAULT " + column.defaultvalue)));
                        else if (column.numeric_precision > 0 &&
                            column.numeric_scale > 0)
                            sbtable.Append(string.Format(" ADD {1} {2}({3},{4}) {5}", item.name, column.column_name, column.data_type,
                                                            column.numeric_precision, column.numeric_scale, string.IsNullOrEmpty(column.defaultvalue) ? "" : (" DEFAULT " + column.defaultvalue)));
                        else
                            sbtable.Append(string.Format(" ADD {1} {2} {3}", item.name, column.column_name, column.data_type,
                                 string.IsNullOrEmpty(column.defaultvalue) ? "" : (" DEFAULT " + column.defaultvalue)));
                    }
                    else
                    {
                        var destinationcolumn = destinationtable.columns.Find(e => e.column_name == column.column_name);
                        if (column.data_type != destinationcolumn.data_type ||
                            column.character_maximum_length > destinationcolumn.character_maximum_length ||
                            column.numeric_precision > destinationcolumn.numeric_precision ||
                            column.numeric_scale > destinationcolumn.numeric_scale)
                        {

                            if (column.data_type == "bigint" &&
                                destinationcolumn.data_type == "bigserial")
                                continue;

                            if (destinationcolumn.data_type == "bigint" &&
                                column.data_type == "bigserial")
                                continue;

                            if (sbtable.Length > 0)
                                sbtable.AppendLine(",");

                            if (column.character_maximum_length > 0)
                                sbtable.Append(string.Format(" ALTER COLUMN {1} TYPE {2}({3})", item.name, column.column_name, column.data_type,
                                    column.character_maximum_length));
                            else if (column.numeric_precision > destinationcolumn.numeric_precision ||
                            column.numeric_scale > destinationcolumn.numeric_scale)
                                sbtable.Append(string.Format(" ALTER COLUMN {1} TYPE {2}({3},{4})", item.name, column.column_name, column.data_type,
                                    column.numeric_precision, column.numeric_scale));
                            else
                                sbtable.Append(string.Format(" ALTER COLUMN {1} TYPE {2}", item.name, column.column_name,
                                    column.data_type));
                        }

                        if (column.defaultvalue != destinationcolumn.defaultvalue &&
                            !string.IsNullOrEmpty(column.defaultvalue))
                        {
                            if (sbtable.Length > 0)
                                sbtable.AppendLine(",");

                            sbtable.Append(string.Format(" ALTER COLUMN {1} SET DEFAULT {2}", item.name,
                                column.column_name, column.defaultvalue));
                        }
                    }
                }

                if (sbtable.Length > 0)
                    sb.AppendLine(string.Format("ALTER TABLE {0} {1};", item.name, sbtable.ToString()));

                foreach (var idx in item.indexes.FindAll(e => !destinationtable.indexes.Exists(f => f.name == e.name)))
                {
                    sb.AppendLine(idx.script + ";");
                }

                foreach (var idx in item.indexes.FindAll(e => destinationtable.indexes.Exists(f => f.name == e.name && f.script != e.script)))
                {
                    sb.AppendLine("drop index " + idx.name + ";");
                    sb.AppendLine(idx.script + ";");
                }
            }

            newtables = sourcetables.FindAll(e => !destinationtables.Exists(f => f.name == e.name)).ToList();
            oldtables = sourcetables.FindAll(e => destinationtables.Exists(f => f.name == e.name)).ToList();

            var cacheview = InfyPOS.Processors.DBViewCache.cacheview.Split(',').ToList();

            if (destinationtables.Exists(e => e.name == "Functions") &&
               sourcetables.Exists(e => e.name == "Functions"))
            {
                var oldfunctions = destinationtables.Find(e => e.name == "Functions").routines;
                var newfunctions = sourcetables.Find(e => e.name == "Functions").routines;

                foreach (var ft in newfunctions.FindAll(e => !oldfunctions.Exists(x => x.name == e.name)
                 || oldfunctions.Find(x => x.name == e.name).description != e.description))
                {
                    if (ft.description.IndexOf("FUNCTION public.getnextnumber") >= 0) continue;

                    if(oldfunctions.Exists(x => x.name == ft.name) &&
                       oldfunctions.Exists(x => x.name == ft.name &&
                       x.description == ft.description))
                    {
                        continue;
                    }

                    if (ft.description.IndexOf("'$libdir") >= 0)
                    {
                        int start = ft.description.IndexOf("$libdir/");
                        int end = ft.description.IndexOf("'", start);
                        string extension = ft.description.Substring(start+8, end - start-8);
                        if (sb.ToString().IndexOf(@"CREATE EXTENSION "+ extension) >= 0)
                            continue;
                        sb.AppendLine("CREATE EXTENSION "+ extension+";");
                    }
                    else
                    {
                        sb.AppendLine(ft.description + ";");
                    }
                }
            }
            else if (sourcetables.Exists(e => e.name == "Functions"))
            {
                var newfunctions = sourcetables.Find(e => e.name == "Functions").routines;
                foreach (var ft in newfunctions)
                {
                    if (ft.description.IndexOf("FUNCTION public.getnextnumber") >= 0) continue;
                    if (ft.description.IndexOf("'$libdir/tablefunc") >= 0)
                    {
                        sb.AppendLine("CREATE EXTENSION tablefunc;");
                    }
                    else
                    {
                        sb.AppendLine(ft.description + ";");
                    }
                }
            }


            if (destinationtables.Exists(e => e.name == "MaterialView") &&
                                    sourcetables.Exists(e => e.name == "MaterialView"))
            {
                var oldfunctions = destinationtables.Find(e => e.name == "MaterialView").routines;
                var newfunctions = sourcetables.Find(e => e.name == "MaterialView").routines;
                foreach (var ft in newfunctions.FindAll(e => !oldfunctions.Exists(x => x.name == e.name)))
                {
                    if (cacheview.Exists(x => x.ToLower() == ft.name.ToLower())) continue;
                    if (ft.description.StartsWith("CREATE VIEW ")) continue;
                    sb.AppendLine(ft.description + ";");
                }

                foreach (var ft in newfunctions.FindAll(e => oldfunctions.Exists(x => x.name == e.name)
                && oldfunctions.Find(x => x.name == e.name).description != e.description))
                {
                    if (cacheview.Exists(x => x.ToLower() == ft.name.ToLower())) continue;
                    if (ft.description.StartsWith("CREATE VIEW ")) continue;
                    sb.AppendLine(ft.drop + ";");
                    sb.AppendLine(ft.description + ";");
                }
            }
            else if (sourcetables.Exists(e => e.name == "MaterialView"))
            {
                var newfunctions = sourcetables.Find(e => e.name == "MaterialView").routines;
                foreach (var ft in newfunctions)
                {
                    if (cacheview.Exists(x => x.ToLower() == ft.name.ToLower())) continue;
                    if (ft.description.StartsWith("CREATE VIEW ")) continue;
                    sb.AppendLine(ft.description + ";");
                }
            }

            if (destinationtables.Exists(e => e.name == "Triggers") &&
                                    sourcetables.Exists(e => e.name == "Triggers"))
            {
                var oldfunctions = destinationtables.Find(e => e.name == "Triggers").triggers;
                var newfunctions = sourcetables.Find(e => e.name == "Triggers").triggers;
                foreach (var ft in newfunctions.FindAll(e => !oldfunctions.Exists(x => x.name == e.name)))
                {
                    sb.AppendLine(ft.create + ";");
                }

                foreach (var ft in newfunctions.FindAll(e => oldfunctions.Exists(x => x.name == e.name)
                && oldfunctions.Find(x => x.name == e.name).create != e.create))
                {
                    sb.AppendLine(ft.drop + ";");
                    sb.AppendLine(ft.create + ";");
                }
            }
            else if (sourcetables.Exists(e => e.name == "Triggers"))
            {
                var newfunctions = sourcetables.Find(e => e.name == "Triggers").triggers;
                foreach (var ft in newfunctions)
                {
                    sb.AppendLine(ft.create + ";");
                }
            }

            return System.Text.ASCIIEncoding.ASCII.GetBytes(sb.ToString());
        }

        public static List<string> GetSizeList(List<string> sizeList)
        {
            sizeList.RemoveAll(e => string.IsNullOrEmpty(e));
            foreach (var list in GetSizeOrders())
            {
                if (sizeList.All(e => list.Value.Exists(x => x == e)))
                {
                    return list.Value;
                }
            }
            return null;
        }

        public static List<Tuple<string, List<string>>> GetSizeGroups()
        {
            var ret = new List<Tuple<string, List<string>>>();
            ret.Add(new Tuple<string, List<string>>("1-1-16", new List<string>("1,2,3,4,5,6,8,10,11,12,13,14,16".Split(','))));
            ret.Add(new Tuple<string, List<string>>("45-5-90", new List<string>("45,50,55,60,65,70,75,80,85,90".Split(','))));
            ret.Add(new Tuple<string, List<string>>("22-2-38", new List<string>("22,24,26,28,30,32,34,36,39,38".Split(','))));
            ret.Add(new Tuple<string, List<string>>("28-2-50", new List<string>("28,30,32,34,36,38,40,42,44,46,48,50".Split(','))));
            ret.Add(new Tuple<string, List<string>>("36-2-50", new List<string>("36,38,39,40,41,42,44,46,48,50".Split(','))));
            ret.Add(new Tuple<string, List<string>>("75-5-115", new List<string>("75,80,85,90,95,100,105,110,115".Split(','))));
            ret.Add(new Tuple<string, List<string>>("3XS-4XL", new List<string>("3XS,XXS,XS,S,M,L,XL,XXL,XXXL,4XL".Split(','))));
            ret.Add(new Tuple<string, List<string>>("S-5XL", new List<string>("S,M,L,XL,XXL,XXXL,4XL,5XL".Split(','))));
            ret.Add(new Tuple<string, List<string>>("NO SIZE", new List<string>("NO SIZE".Split(','))));
            return ret;
        }

        public static List<string> SortSizes(List<string> unordered)
        {
            var all = GetSizes();

            var _sizeOrderMap = all
                .Distinct()                // avoid duplicates
                .Select((v, i) => new { v, i })
                .ToDictionary(x => x.v, x => x.i);
            return unordered
                .OrderBy(x => _sizeOrderMap.ContainsKey(x) ? _sizeOrderMap[x] : int.MaxValue)
                .ToList();
        }
        public static List<string> GetSizes()
        {
            List<string> ret = new List<string>();
            ret.AddRange("0,1,2,3,4,5,6,7,8,9,10,11,12,13,14,16,18,20".Split(','));
            ret.AddRange("22,24,26,28,30,32,34,36,39,38,40,42,44".Split(','));
            ret.AddRange("46,48,50,52,55,60,65,70,75,80,85,90,95,100,105,110".Split(','));
            ret.AddRange("7-75,7-80,7-85,7-90,7-95,7-100,8-70,8-75,8-80,8-85,8-90,8-95,8-100".Split(','));
            ret.AddRange("S,M,L,XL,2XL,XXL,3XL,XXXL".Split(','));
            ret.AddRange("S,M,L,XL,2XL,3XL".Split(','));
            ret.AddRange("S,M,L,XL,2XL,XXL,3XL,XXXL,4XL,5XL,6XL,7XL".Split(','));
            ret.AddRange("3XS,XXS,XS,S,M,L,XL,2XL,3XL,4XL,5XL,6XL,7XL".Split(','));
            ret.AddRange("3XS,XXS,XS,S,M,L,XL,2XL,XXL,3XL,XXXL,4XL,5XL,6XL,7XL".Split(','));
            return ret;
        }

        static SortedDictionary<string, List<string>> GetSizeOrders()
        {
            var ret = new SortedDictionary<string, List<string>>();
            ret.Add("KIDS-1", new List<string>("0,1,2,3,4,5,6,7,8,9,10,12,13,14,15,16".Split(',')));
            ret.Add("INNER-1", new List<string>("45,50,55,60,65,70,75,80,85,90,95,100,105".Split(',')));
            ret.Add("MALE-1", new List<string>("22,24,26,28,30,32,34,36,39,38,40,42,44".Split(',')));
            ret.Add("MALE-2", new List<string>("30,32,34,36,38,39,40,41,42,44,46,48,50,52".Split(',')));
            ret.Add("KIDS-2", new List<string>("26,28,30,32,34,36,38,40,42,44,46,48,50,52".Split(',')));
            ret.Add("INNER-2", new List<string>("50,55,60,65,70,75,80,85,90,95,100,105,110".Split(',')));
            ret.Add("TSHIRT", new List<string>("3XS,XXS,XS,S,M,L,XL,XXL,XXXL,4XL,5XL,6XL,7XL".Split(',')));
            ret.Add("SHIRT-1", new List<string>("S,M,L,XL,2XL".Split(',')));
            ret.Add("SHIRT-2", new List<string>("S,M,L,XL,2XL,3XL,4XL".Split(',')));
            ret.Add("SHIRT-3", new List<string>("S,M,L,XL,2XL,3XL,4XL,5XL,6XL".Split(',')));
            ret.Add("GIRL", new List<string>("7-75,7-80,7-85,7-90,7-95,7-100,8-70,8-75,8-80,8-85,8-90,8-95,8-100".Split(',')));
            return ret;
        }



        private static DataTable CreateTable(ClosedXML.Excel.IXLWorksheet workSheet, bool hascolumn)
        {
            bool firstRow = true;
            DataTable dt = new DataTable();
            bool isrow = true;
            foreach (var row in workSheet.Rows())
            {
                //Use the first row to add columns to DataTable.
                if (firstRow)
                {
                    foreach (var cell in row.Cells())
                    {
                        dt.Columns.Add(cell.Value.ToString());
                    }
                    firstRow = false;
                    isrow = (!hascolumn);
                }


                if (isrow)
                {
                    //Add rows to DataTable.
                    dt.Rows.Add();
                    int i = 0;
                    foreach (var cell in row.Cells())
                    {
                        if (dt.Columns.Count >= cell.Address.ColumnNumber)
                            dt.Rows[dt.Rows.Count - 1][i] = cell.Value.ToString().Trim();
                        i++;
                    }
                }
                isrow = true;
            }
            return dt;
        }

        public static InfyPOS.Controllers.ImageController.ExcelDocument OpenExcel(Byte[] buffer, bool hasColumn)
        {
            InfyPOS.Controllers.ImageController.ExcelDocument exceldoc = new InfyPOS.Controllers.ImageController.ExcelDocument();
            using (MemoryStream ms = new MemoryStream(buffer))
            {
                using (var workBook = new ClosedXML.Excel.XLWorkbook(ms))
                {

                    var worksheet = workBook.Worksheets.First();
                    var datatable = CreateTable(worksheet, hasColumn);

                    foreach (DataColumn column in datatable.Columns)
                    {
                        exceldoc.columns.Add(column.ColumnName);
                    }

                    foreach (DataRow dr in datatable.Rows)
                    {
                        InfyPOS.Controllers.ImageController.ExcelDocument.Row row = new InfyPOS.Controllers.ImageController.ExcelDocument.Row();
                        foreach (var colmvalue in dr.ItemArray)
                        {
                            row.values.Add(colmvalue == null ? "" : colmvalue.ToString());
                        }
                        exceldoc.rows.Add(row);
                    }
                }
            }
            return exceldoc;
        }
        public static List<string> GetSizeOrder(List<string> sizeList)
        {
            List<string> stringlist = new List<string>("S,M,L,XL,XXL,XXXL,4XL,5XL,6XL".Split(','));
            return sizeList.OrderBy(e =>
            {
                int no = 0;
                if (int.TryParse(e, out no))
                    return no;
                else if (stringlist.Exists(x => x == e.ToUpper()))
                    return stringlist.IndexOf(e.ToUpper()) * -1;
                return 0;
            }).ToList();
        }
    }

    public static class DateTimeDayOfMonthExtensions
    {
        public static DateTime FirstDayOfMonth(this DateTime value)
        {
            return new DateTime(value.Year, value.Month, 1);
        }

        public static int DaysInMonth(this DateTime value)
        {
            return DateTime.DaysInMonth(value.Year, value.Month);
        }


        public static DateTime LastDayOfMonth(this DateTime value)
        {
            return new DateTime(value.Year, value.Month, value.DaysInMonth());
        }
    }
}