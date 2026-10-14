using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using WebAPI.Data;

namespace InfyPOS.Processors
{
    public class RandomManager: Common.BaseData
    {
        public enum Type
        {
            Numbers,
            Alphabet,
            AlphaNumeric
        }
        int length;
        string source;
        string query;
        public RandomManager(int length,Type type,string column,string table)
        {
            this.length = length;
            switch(type)
            {
                case Type.Alphabet:
                    source = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
                    break;
                case Type.AlphaNumeric:
                    source = "0123456789ABCDEFGHIJKLMNOPQRSTUVWXYZ";
                    break;
                case Type.Numbers:
                    source = "0123456789";
                    break;
            }
            this.query = string.Format("select {0} from {1} where {0}=@VALUE LIMIT 1", column, table);
        }

        public string Generate()
        {
            var list = new SortedList<string, object>();
            var newcode = Token.RandomString(length, source);
            bool exist = true;
            while (exist)
            {
                exist = false;
                list.Clear();
                list.Add("@VALUE", newcode);
                using (var reader = ExecuteReader(query, list))
                {
                    while(reader.Read())
                    {
                        newcode = Token.RandomString(length, source);
                        exist = true;
                    }
                }
            }
            return newcode;
        }
    }
}