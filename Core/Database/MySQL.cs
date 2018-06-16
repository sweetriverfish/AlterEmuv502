using System;
using System.Collections.Generic;
using MySql.Data.MySqlClient;

namespace Core.Database
{
    public class MySQL : Database
    {

        public MySQL(string host, ushort port, string user, string password, string database)
            : base(database)
        {

            var builder = new MySqlConnectionStringBuilder
            {
                Server = host,
                Port = port,
                UserID = user,
                Password = password,
                Database = database,
                Pooling = true,
                MinimumPoolSize = 10,
                MaximumPoolSize = 100
            };

            connectionString = builder.GetConnectionString(true);
        }

        public override bool Open()
        {
            try
            {
                using (var connection = OpenConnection())
                {
                    return true;
                }
            }
            catch (Exception)
            {
                return false;
            }
        }

        public MySqlConnection OpenConnection()
        {
            var connection = new MySqlConnection(connectionString);
            connection.Open();
            return connection;
        }

        public void AsyncQuery(string query)
        {
            try
            {
                MySqlCommand cmd = new MySqlCommand(query, OpenConnection());
                cmd.BeginExecuteNonQuery(AsyncQueryCallback, cmd);
            }
            catch { }
        }

        private void AsyncQueryCallback(IAsyncResult iAr)
        {
            try
            {
                MySqlCommand cmd = (MySqlCommand)iAr.AsyncState;
                cmd.EndExecuteNonQuery(iAr);
            }
            catch { }
        }

        public void Query(string Query)
        {
            try { new MySqlCommand(Query, OpenConnection()).ExecuteScalar(); }
            catch (Exception ex) { Console.WriteLine("Error '" + ex.Message + "' at '" + Query + "'"); }
        }

        public int Insert(string table, Dictionary<string, object> values)
        {
            if (values.Count <= 0)
                throw new Exception("Please provide atleast some data to insert.");

            string query = string.Concat("INSERT INTO ", table, "(", string.Join(",", values.Keys), ") VALUES ");
            string valuesString = string.Empty;

            if (values.Count > 0)
            {
                foreach (KeyValuePair<string, object> entry in values)
                {
                    valuesString = string.Concat(valuesString, "@", entry.Key, ",");
                }
                query = string.Concat(query, "(", valuesString.Remove(valuesString.Length - 1), ")");
            }

            try
            {
                MySqlCommand cmd = new MySqlCommand(query, OpenConnection());

                foreach (KeyValuePair<string, object> entry in values)
                {
                    cmd.Parameters.AddWithValue("@" + entry.Key, entry.Value);
                }

                cmd.ExecuteNonQuery();
            }
            catch { }
            return 0;
        }

        public void AsyncInsert(string table, Dictionary<string, object> values)
        {
            if (values.Count <= 0)
                throw new Exception("Please provide atleast some data to insert.");

            string query = string.Concat("INSERT INTO ", table, "(", string.Join(",", values.Keys), ") VALUES ");
            string valuesString = string.Empty;

            if (values.Count > 0)
            {
                foreach (KeyValuePair<string, object> entry in values)
                {
                    valuesString = string.Concat(valuesString, "@", entry.Key, ",");
                }
                query = string.Concat(query, "(", valuesString.Remove(valuesString.Length - 1), ")");
            }

            try
            {
                MySqlCommand cmd = new MySqlCommand(query, OpenConnection());

                foreach (KeyValuePair<string, object> entry in values)
                {
                    cmd.Parameters.AddWithValue("@" + entry.Key, entry.Value);
                }

                cmd.BeginExecuteNonQuery(AsyncInsertCallback, cmd);
            }
            catch { }
        }

        private void AsyncInsertCallback(IAsyncResult iAr)
        {
            try
            {
                MySqlCommand cmd = (MySqlCommand)iAr.AsyncState;
                cmd.EndExecuteNonQuery(iAr);
            }
            catch { }
        }

        public void Update(string table, Dictionary<string, object> val, Dictionary<string, object> where)
        {
            string query = string.Concat("UPDATE ", table, " SET ");
            string valuesString = string.Empty;

            byte index = 0;
            foreach (KeyValuePair<string, object> entry in val)
            {
                if (index == 0)
                {
                    valuesString = string.Concat(valuesString, entry.Key, "=@", entry.Key);
                    index++;
                }
                else
                {
                    valuesString = string.Concat(valuesString, ", ", entry.Key, "=@", entry.Key);
                }
            }
            query = string.Concat(query, valuesString);

            if (where.Count > 0)
            {
                index = 0;
                foreach (KeyValuePair<string, object> entry in where)
                {
                    if (index == 0)
                    {
                        valuesString = string.Concat(valuesString, entry.Key, "=@", entry.Key);
                        index++;
                    }
                    else
                    {
                        valuesString = string.Concat(valuesString, " AND ", entry.Key, "=@", entry.Key);
                    }
                }
                query = string.Concat(query, " WHERE ", valuesString);
            }

            try
            {
                MySqlCommand cmd = new MySqlCommand(query, OpenConnection());

                foreach (KeyValuePair<string, object> entry in where)
                {
                    cmd.Parameters.AddWithValue("@" + entry.Key, entry.Value);
                }

                foreach (KeyValuePair<string, object> entry in val)
                {
                    cmd.Parameters.AddWithValue("@" + entry.Key, entry.Value);
                }

                cmd.ExecuteNonQuery();

            }
            catch { }
        }

        public MySqlDataReader Select(string[] keys, string table, Dictionary<string, object> values)
        {
            string query = string.Concat("SELECT ", string.Join(",", keys), " FROM ", table);
            string valuesString = string.Empty;

            if (values.Count > 0)
            {
                byte index = 0;
                foreach (KeyValuePair<string, object> entry in values)
                {
                    if (index == 0)
                    {
                        valuesString = string.Concat(valuesString, entry.Key, "=@", entry.Key);
                        index++;
                    }
                    else
                    {
                        valuesString = string.Concat(valuesString, " AND ", entry.Key, "=@", entry.Key);
                    }
                }
                query = string.Concat(query, " WHERE ", valuesString);
            }

            try
            {
                MySqlCommand cmd = new MySqlCommand(query, OpenConnection());

                foreach (KeyValuePair<string, object> entry in values)
                {
                    cmd.Parameters.AddWithValue("@" + entry.Key, entry.Value);
                }

                return cmd.ExecuteReader();

            }
            catch (Exception e) { Console.WriteLine(e); }

            return null;
        }
    }
}
