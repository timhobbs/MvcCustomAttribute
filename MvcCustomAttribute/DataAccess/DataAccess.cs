using System.Data;
using System.Data.SqlServerCe;

namespace MvcCustomAttribute.DataAccess {

    /// <summary>
    /// Summary description for DataAccess
    /// </summary>
    public static class DataAccess {

        public static DataTable GetData(string connectionString, string sql) {
            DataTable source = new DataTable();
            using (SqlCeConnection myConnection = new SqlCeConnection(connectionString)) {
                SqlCeDataAdapter myCommand = new SqlCeDataAdapter(sql, myConnection);
                DataSet ds = new DataSet();
                myCommand.Fill(ds);
                source = ds.Tables[0];
            }

            return source;
        }
    }
}