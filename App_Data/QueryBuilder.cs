using System;
using System.Collections.Generic;
using System.Text;

namespace InfyPOS.Data
{
    public class QueryBuilder
    {
        public class Parameter
        {
            public string columnName;
            public string parameterName;
            public object value;
            public string Operator;
        }

        public string DefaultQuery = string.Empty;
        public SortedList<string, object> Parameters = new SortedList<string, object>();
        public string SortField = string.Empty;
        public string GroupBY = string.Empty;
        private string currentQuery = "";
        public bool EnablePaging = false;
        public int CurrentPageIndex = 1;
        public int Limit;
        private bool isLast = false;
        public bool MoveNext()
        {
            if (isLast) return false;
            CurrentPageIndex++;
            return true;
        }
        public bool MovePrevious()
        {
            if (CurrentPageIndex == 1) return false;
            CurrentPageIndex--;
            isLast = true;
            return true;
        }
        public string CurrentQuery
        {
            get
            {
                return currentQuery;
            }
            set
            {
                currentQuery = value;
            }
        }

        public void AddParameter(Parameter param)
        {
            AddParameter(param.columnName, param.parameterName, param.value, param.Operator);
        }

        public void AddInParamer(string columnname, string value,string orcondition)
        {
            if (CurrentQuery.ToLower().IndexOf(" where ") >= 0)
            {
                CurrentQuery = CurrentQuery + " And ";
            }
            else
            {
                CurrentQuery = CurrentQuery + " where ";
            }
            CurrentQuery = CurrentQuery + string.Format("({0}{1}({2}) or {3})", columnname, " in ", value,orcondition);
        }
        public void AddInParamer(string columnname, string value)
        {
            if (CurrentQuery.ToLower().IndexOf(" where ") >= 0)
            {
                CurrentQuery = CurrentQuery + " And ";
            }
            else
            {
                CurrentQuery = CurrentQuery + " where ";
            }
            CurrentQuery = CurrentQuery + string.Format("{0}{1}({2})", columnname, " in ", value);
        }

        public void AddNotInParamer(string columnname, string value)
        {
            if (CurrentQuery.ToLower().IndexOf(" where ") >= 0)
            {
                CurrentQuery = CurrentQuery + " And ";
            }
            else
            {
                CurrentQuery = CurrentQuery + " where ";
            }
            CurrentQuery = CurrentQuery + string.Format("{0}{1}({2})", columnname, " not in ", value);
        }

        public void AddParameter(string columnName, string parameterName, object value, string Operator)
        {
            if (parameterName != null)
            {
                if (parameterName.IndexOf("::", 0) > 0)
                {
                    if (Parameters.ContainsKey(parameterName.Split(new string[] { "::" }, StringSplitOptions.RemoveEmptyEntries)[0]))
                        return;
                    Parameters.Add(parameterName.Split(new string[] { "::" }, StringSplitOptions.RemoveEmptyEntries)[0], value);
                }
                else
                {
                    if (Parameters.ContainsKey(parameterName))
                        return;
                    Parameters.Add(parameterName, value);
                }

            }

            if (CurrentQuery.ToLower().IndexOf(" where ") >= 0)
            {
                CurrentQuery = CurrentQuery + " And ";
            }
            else
            {
                CurrentQuery = CurrentQuery + " where ";
            }
            CurrentQuery = CurrentQuery + string.Format("{0}{1}@{2}", columnName, Operator, parameterName);
        }
        
        public void Add2Query(string query)
        {
            if (CurrentQuery.ToLower().IndexOf(" where ") >= 0)
            {
                CurrentQuery = CurrentQuery + " And ";
            }
            else
            {
                CurrentQuery = CurrentQuery + " where ";
            }
            CurrentQuery = CurrentQuery + query;
        }
        public void AddORParameter(string parameterName, object value, string Operator, string orparameterName,string orcolumnname, object orvalue, string orOperator)
        {
            if (parameterName != null)
            {
                if (Parameters.ContainsKey(parameterName))
                    return;
                Parameters.Add(parameterName, value);
            }

            if (orparameterName != null)
            {
                if (!Parameters.ContainsKey(orparameterName))
                {
                    Parameters.Add(orparameterName, orvalue);
                }
            }

            if (CurrentQuery.ToLower().IndexOf(" where ") >= 0)
            {
                CurrentQuery = CurrentQuery + " And ";
            }
            else
            {
                CurrentQuery = CurrentQuery + " where ";
            }
            CurrentQuery = CurrentQuery + string.Format("({0}{1}@{0} or {4}{3}@{2})", parameterName, Operator, orparameterName, orOperator,orcolumnname);
        }

        public void AddORParameter(string parameterName, object value, string Operator, string orCondition)
        {
            if (parameterName != null)
            {
                if (Parameters.ContainsKey(parameterName))
                    return;
                Parameters.Add(parameterName, value);
            }

            if (CurrentQuery.ToLower().IndexOf(" where ") >= 0)
            {
                CurrentQuery = CurrentQuery + " And ";
            }
            else
            {
                CurrentQuery = CurrentQuery + " where ";
            }
            CurrentQuery = CurrentQuery + string.Format("({0}{1}@{0} or {2})", parameterName, Operator, orCondition);
        }

        public Npgsql.NpgsqlCommand GetCommand(Npgsql.NpgsqlConnection conn)
        {
            return GetCommand(conn, null);
        }
        public Npgsql.NpgsqlCommand GetCommand(Npgsql.NpgsqlConnection conn, Npgsql.NpgsqlTransaction trans)
        {
            Npgsql.NpgsqlCommand command = null;
            if (trans == null)
                command = new Npgsql.NpgsqlCommand(CurrentQuery, conn);
            else
                command = new Npgsql.NpgsqlCommand(CurrentQuery, conn, trans);
            UpdateParameterValues(command);

            if (!string.IsNullOrEmpty(GroupBY))
            {
                command.CommandText = command.CommandText + " GROUP by " + GroupBY;
            }
            if (!string.IsNullOrEmpty(SortField))
            {
                command.CommandText = command.CommandText + " Order by " + SortField;
            }
            if (Limit > 0)
            {
                command.CommandText = command.CommandText + " LIMIT " + Limit;
            }
            return command;
        }

        public void UpdateParameterValues(Npgsql.NpgsqlCommand command)
        {
            foreach (var item in Parameters)
            {
                if (item.Value == null)
                {
                    command.Parameters.AddWithValue(item.Key, DBNull.Value);
                }
                else
                {
                    command.Parameters.AddWithValue(item.Key, item.Value);
                }
                if (item.Value != null && item.Value.GetType() == typeof(DateTime))
                    command.Parameters[item.Key].NpgsqlDbType = NpgsqlTypes.NpgsqlDbType.Timestamp;
            }
        }

        public void AddParameter(string columnName, string parameterName, object value, string Operator, string query)
        {
            if (parameterName != null)
                Parameters.Add(parameterName, value);
            if (currentQuery.IndexOf("where") >= 0)
            {
                currentQuery = CurrentQuery + " And ";
            }
            else
            {
                currentQuery = CurrentQuery + " where ";
            }
            currentQuery = CurrentQuery + string.Format("{0}{1}{2}", columnName, Operator, query);
        }

        public void InValue(string columnName1, string columnName2, string parameterName, object value)
        {
            if (parameterName != null)
                Parameters.Add(parameterName, value);
            if (currentQuery.IndexOf("where") >= 0)
            {
                currentQuery = CurrentQuery + " And ";
            }
            else
            {
                currentQuery = CurrentQuery + " where ";
            }
            currentQuery = CurrentQuery + string.Format("@{0} between {1} and {2}", parameterName, columnName1, columnName2);
        }

        public void AddParameter(string parameterName, object value, string Operator)
        {
            AddParameter(parameterName, parameterName.Replace(".","_"), value, Operator);
        }

        //internal void AddJoinParameter(string table, string parameterName, object value, string Operator)
        //{

        //}

        internal void AddConditionalParameter(string condition, string parametername, object value)
        {
            if (parametername != null)
                Parameters.Add(parametername, value);
            if (currentQuery.IndexOf("where") >= 0)
            {
                currentQuery = CurrentQuery + " And ";
            }
            else
            {
                currentQuery = CurrentQuery + " where ";
            }
            currentQuery = CurrentQuery + condition;
        }

        //public System.Data.DataSet ExecuteDataset()
        //{
        //    System.Data.DataSet dset = DBLookupUtils.ExecuteDataSet(GetExecutableQuery(), Paramters);
        //    if (dset.Tables[0].Rows.Count < Common.PrintUtils.PageLength)
        //        isLast = true;
        //    return dset;
        //}

        //private string GetExecutableQuery()
        //{
        //    if (EnablePaging)
        //    {
        //        string newQuery = CurrentQuery;
        //        if (CurrentQuery.ToLower().StartsWith("select "))
        //        {
        //            newQuery = CurrentQuery.Substring(6);
        //        }
        //        return string.Format(@";WITH pg AS (select ROW_NUMBER() OVER (ORDER BY {0}) AS Row,{1})
        //        Select * from pg where Row between {2} and {3}", SortField, newQuery,
        //                      Common.PrintUtils.PageStartFrom(CurrentPageIndex), Common.PrintUtils.PageStartTo(CurrentPageIndex));
        //    }
        //    else if (!string.IsNullOrEmpty(SortField))
        //    {
        //        return CurrentQuery + " order by " + SortField;
        //    }
        //    else
        //    {
        //        return CurrentQuery;
        //    }

        //}

    }
}
