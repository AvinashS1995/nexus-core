using System;
using System.Data;
using System.Reflection.Metadata;

namespace NexusCore.Services
{
    public class BaseService
    {
        
        public List<TN> ConvertToList<TN>(DataTable dt)
        {
            var properties = typeof(TN).GetProperties();
            List<TN> collection = new List<TN>();
            if (dt != null)
            {
                if (dt.Rows.Count > 0)
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
            }
            return collection;
        }
    }
}

