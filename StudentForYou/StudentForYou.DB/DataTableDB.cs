﻿using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;

namespace StudentForYou.DB
{
    public class DataTableDB
    {
        Lazy<MySqlConnection> lazyConnection;
        ConnectionManager conManager;
       public DataTableDB()
        {
         conManager = new ConnectionManager();
         lazyConnection = new Lazy<MySqlConnection>(() => new MySqlConnection(GetConnectionString()));
        }
        public string GetConnectionString()
        {
            var builder = new MySqlConnectionStringBuilder();
            builder.Server = "remotemysql.com";
            builder.Port = 3306;
            builder.Database = "aeMNgY2VO3";
            builder.UserID = "aeMNgY2VO3";
            builder.Password = "aOY0TMNsRw";
            return builder.ConnectionString;
        }
        
        public T GetItem<T>(string query)
        {
            using (var con = conManager.OpenConnection(lazyConnection))
            {
                using (var cmd = new MySqlCommand(query, con))
                {
                    var dt = new DataTable();
                    dt.Load(cmd.ExecuteReader());
                    T item = GetConvertedDataTableItem<T>(dt.Rows[0]);
                    conManager.CloseConnection(con);
                    return item;
                }
            }
        }

        public List<T> GetList<T>(string query)
        {
            var dt = GetDataTable(query);
            return ConvertDataTable<T>(dt);
        }

        public DataTable GetDataTable(string query)
        {
            using (var con = conManager.OpenConnection(lazyConnection))
            {
                using (var cmd = new MySqlCommand(query, con))
                {
                    var dt = new DataTable();
                    dt.Load(cmd.ExecuteReader());
                    conManager.CloseConnection(con);
                    return dt;
                }
            }
        }

        public List<T> ConvertDataTable<T>(DataTable dt)
        {
            return dt.Rows.Cast<DataRow>().Select(row => GetConvertedDataTableItem<T>(row)).ToList();
        }

        public T GetConvertedDataTableItem<T>(DataRow dr)
        {
            Type temp = typeof(T);
            T obj = Activator.CreateInstance<T>();

            foreach (DataColumn column in dr.Table.Columns)
            {
                foreach (PropertyInfo pro in temp.GetProperties())
                {
                    if (pro.Name == column.ColumnName)
                        pro.SetValue(obj, dr[column.ColumnName], null);
                    else
                        continue;
                }
            }
            return obj;
        }

        public void UpdateIncreaseByNumber(string table, string whatToIncrease, string whereString, int id, int number)
        {
            var con = conManager.OpenConnection(lazyConnection);

            var query = "select * from " + table;
            var update = new MySqlCommand();
            update.Connection = con;
            update.CommandType = CommandType.Text;
            update.CommandText = "UPDATE " + table + " SET " + whatToIncrease + " = " + whatToIncrease + " + " + number + " WHERE " + whereString + " = @" + whereString;
            update.Parameters.Add(new MySqlParameter("@" + whereString, MySqlDbType.Int32, 50, whereString));

            var da = new MySqlDataAdapter(query, con);
            da.UpdateCommand = update;

            var ds = new DataSet(); 
            da.Fill(ds, table);

            ds.Tables[0].AsEnumerable().Where(x => x.Field<int>(whereString) == id).FirstOrDefault()[whatToIncrease] = number;

            da.Update(ds.Tables[0]);
            conManager.CloseConnection(con);
            da.Dispose();
        }

        public void DeleteRow(string table, string whereString, int id)
        {
            var con = conManager.OpenConnection(lazyConnection);
             
            var query = "select * from " + table + " where " + whereString + " = " + id;
            var delete = new MySqlCommand();
            delete.Connection = con;
            delete.CommandType = CommandType.Text;
            delete.CommandText = "delete from " + table + " where " + whereString + " = @" + whereString;
            delete.Parameters.Add(new MySqlParameter("@" + whereString, MySqlDbType.Int32, 50, whereString));

            var da = new MySqlDataAdapter(query, con);
            da.DeleteCommand = delete;

            var ds = new DataSet();
            da.Fill(ds, table);

            System.Diagnostics.Debug.WriteLine(ds.Tables[0].Rows.Count);
            foreach (DataRow row in ds.Tables[0].Rows)
                row.Delete();

            da.Update(ds.Tables[0]);
            conManager.CloseConnection(con);
            da.Dispose();
        }
        
        public int GetListAmount<T>(List<T> list)
        {
           // int sum = numbers.Aggregate(func: (result, item) => result + item);
            return list.AsEnumerable().Count();
        }
    }
}
