using System;
using MySql.Data.MySqlClient;
using NexusCore.Common;
using System.Data;
using System.Reflection.Metadata;
using Serilog;

namespace NexusCore.Repositories
{
    public abstract class BaseRepository<T> where T : class
    {
        public DbContext _context { get; set; }
        protected BaseRepository(DbContext mySqlDatabase)
        {
            this._context = mySqlDatabase;
        }
        public MySqlCommand CreateCommand(string strFlag = "Old")
        {
            //return _context._connection.CreateCommand();
            var cmd = ((strFlag == "Old") ? _context._connection.CreateCommand() : _context._readReplicaConnection1.CreateCommand());
            return cmd;
        }
        public void AddParameter(MySqlCommand command, string parametername, object value)
        {
            command.Parameters.Add(new MySqlParameter("@" + parametername, value));
        }

        public async Task<DataTable> ExecuteDataTabelAsync(MySqlCommand command)
        {
            DataTable dt = new DataTable();
            try
            {
                if (command.Connection.State != ConnectionState.Open)
                {
                    command.Connection.Open();
                }
                var reader = await command.ExecuteReaderAsync();
                dt.Load(reader);
            }
            catch (Exception ex)
            {
                Log.Information("Error ExecuteDataTabelAsync :" + ex.Message);
                //LogWriter.LogWriteException(ex);
            }
            finally
            {
                _context.ConnectionClosed();
            }
            return dt;
        }

        public DataTable NoAsyncExecuteDataTabelAsync(MySqlCommand command)
        {
            DataTable dt = new DataTable();
            try
            {
                if (command.Connection.State != ConnectionState.Open)
                {
                    command.Connection.Open();
                }
                var reader = command.ExecuteReader();
                dt.Load(reader);
            }
            catch (Exception ex)
            {
                Log.Information("Error NoAsyncExecuteDataTabelAsync :" + ex.Message);
                //LogWriter.LogWriteException(ex);
            }
            finally
            {
                _context.ConnectionClosed();
            }
            return dt;
        }

        public async Task<DataSet> ExecuteDataSetAsync(MySqlCommand command)
        {
            DataSet ds = new DataSet();
            try
            {
                if (command.Connection.State != ConnectionState.Open)
                {
                    command.Connection.Open();
                }
                MySqlDataAdapter da = new MySqlDataAdapter();
                da.SelectCommand = command;

                await da.FillAsync(ds);
            }
            catch (Exception ex)
            {
                Log.Information("Error ExecuteDataSetAsync (" + command.CommandText + ") :" + ex.Message);
                //LogWriter.LogWriteException(ex);
            }
            finally
            {
                _context.ConnectionClosed();
            }
            return ds;
        }


        //public async Task<object> ExecuteScalarAsync(MySqlCommand command)
        //{
        //    object result = 0;
        //    try
        //    {
        //        if (command.Connection.State != ConnectionState.Open)
        //        {
        //            command.Connection.Open();
        //        }
        //        result = await command.ExecuteScalarAsync();
        //    }
        //    catch (Exception ex)
        //    {
        //        result = -1;
        //        LogWriter lg = new LogWriter();
        //        lg.LogWrite(ex.Message.ToString());
        //        // LogWriter.LogWriteException(ex);
        //    }
        //    finally
        //    {
        //        _context.ConnectionClosed();
        //    }
        //    return result;
        //}

        public async Task<object> ExecuteNonQueryAsync(MySqlCommand command)
        {
            object result = null;
            try
            {
                if (command.Connection.State != ConnectionState.Open)
                {
                    command.Connection.Open();
                }
                result = await command.ExecuteNonQueryAsync();
            }
            catch (Exception ex)
            {
                //Log.Information("Error ExecuteNonQueryAsync :" + ex.Message);
                //LogWriter.LogWriteException(ex);
            }
            finally
            {
                _context.ConnectionClosed();
            }
            return result;
        }

        public object ExecuteNonQuery(MySqlCommand command)
        {
            object result = null;
            try
            {
                if (command.Connection.State != ConnectionState.Open)
                {
                    command.Connection.Open();
                }
                result = command.ExecuteNonQuery();
            }
            catch (Exception ex)
            {
                //Log.Information("Error ExecuteNonQuery :" + ex.Message);
                //LogWriter.LogWriteException(ex);
            }
            finally
            {
                _context.ConnectionClosed();
            }
            return result;
        }

        public List<T> ToList(DataTable dt)
        {
            var columnNames = dt.Columns.Cast<DataColumn>().Select(c => c.ColumnName.ToLower()).ToList();
            var properties = typeof(T).GetProperties();
            return dt.AsEnumerable().Select(row =>
            {
                var objT = Activator.CreateInstance<T>();
                foreach (var pro in properties)
                {
                    if (columnNames.Contains(pro.Name.ToLower()))
                    {
                        try
                        {
                            pro.SetValue(objT, row[pro.Name]);
                        }
                        catch (Exception ex)
                        {
                            //LogWriter.LogWriteException(ex);
                        }
                    }
                }
                return objT;
            }).ToList();

        }

        //Convert the given database table data to given list object
        public List<TN> ConvertToList<TN>(DataTable dt)
        {
            var properties = typeof(TN).GetProperties();
            List<TN> collection = new List<TN>();
            if (dt != null && dt.Rows.Count > 0)
            {
                var columnNames = dt.Columns.Cast<DataColumn>().Select(c => c.ColumnName.ToLower()).ToList();
                foreach (DataRow row in dt.Rows)
                {
                    var objT = Activator.CreateInstance<TN>();
                    foreach (var pro in properties)
                    {
                        try
                        {
                            if (columnNames.Contains("c" + pro.Name.ToLower()))
                                pro.SetValue(objT, row["c" + pro.Name.ToLower()]);
                            else if (columnNames.Contains("n" + pro.Name.ToLower()))
                            {
                                if (row["n" + pro.Name.ToLower()] == null || row["n" + pro.Name.ToLower()] == DBNull.Value)
                                    pro.SetValue(objT, Convert.ChangeType(0, pro.PropertyType), null);
                                else
                                    pro.SetValue(objT, Convert.ChangeType(row["n" + pro.Name.ToLower()], pro.PropertyType), null);
                            }
                            else if (columnNames.Contains("b" + pro.Name.ToLower()))
                                if (pro.PropertyType.FullName == "System.Boolean")
                                    pro.SetValue(objT, Convert.ToInt16(row["b" + pro.Name.ToLower()]) == 1);
                                else pro.SetValue(objT, row["b" + pro.Name.ToLower()].ToString());
                            else if (columnNames.Contains(pro.Name.ToLower()))
                                pro.SetValue(objT, row[pro.Name.ToLower()]);
                        }
                        catch (Exception ex)
                        {
                            //LogWriter.LogWriteException(ex);
                        }
                    }
                    collection.Add(objT);
                }
            }
            return collection;
        }

   
        public List<TN> ConvertToList<TN>(DataSet ds)
        {
            DataTable dt = ds != null && ds.Tables.Count > 0 ? ds.Tables[0] : new DataTable();
            return ConvertToList<TN>(dt);
        }

    }
}

