using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Configuration;
using System.Windows;

namespace Finance_Manager
{
    // SELECT [account].[AccountID], SUM([transaction].Amount)  
    // FROM[transaction] INNER JOIN[account] ON[transaction].AccountID = [account].AccountID
    // GROUP BY[account].AccountID

    /// <summary>
    /// Provides methods for querying, updating, or altering SQL databases. 
    /// </summary>
    public static class SqlHelper
    {
        private static readonly string connectionString = ConfigurationManager.ConnectionStrings["ConnectionString"].ConnectionString;
        
        private static SqlConnection GetSqlConnection()
        {
            SqlConnection sqlConnection = new SqlConnection();
            try { sqlConnection.ConnectionString = connectionString; sqlConnection.Open(); } 
            catch (Exception ex) { MessageBox.Show(ex.Message); }
            return sqlConnection;
        }

        public static int ExecuteNonQuery(string command, params SqlParameter[] parameters)
        {
            using (SqlConnection sqlConnection = GetSqlConnection())
            {
                using (SqlCommand sqlCommand = new SqlCommand(command, sqlConnection))
                {
                    sqlCommand.Parameters.AddRange(parameters);
                    try { return sqlCommand.ExecuteNonQuery(); } 
                    catch (Exception ex) { MessageBox.Show(ex.Message); return 0; }
                }
            }
        }

        public static object ExecuteScalar(string command, params SqlParameter[] parameters)
        {
            using (SqlConnection sqlConnection = GetSqlConnection())
            {
                using (SqlCommand sqlCommand = new SqlCommand(command, sqlConnection))
                {
                    sqlCommand.Parameters.AddRange(parameters);
                    return sqlCommand.ExecuteScalar();
                }
            }
        }

        public static SqlDataReader ExecuteReader(string command, params SqlParameter[] parameters)
        {
            SqlConnection sqlConnection = GetSqlConnection();
            using (SqlCommand sqlCommand = new SqlCommand(command, sqlConnection))
            {
                sqlCommand.Parameters.AddRange(parameters);
                SqlDataReader reader = sqlCommand.ExecuteReader(CommandBehavior.CloseConnection);
                return reader;
            }
        }

        public static DataSet FillDataSet(DataSet dataset, string query, params SqlParameter[] parameters)
        {
            using (SqlConnection sqlConnection = GetSqlConnection())
            {
                SqlCommand sqlCommand = new SqlCommand(query, sqlConnection);
                sqlCommand.Parameters.AddRange(parameters);

                SqlDataAdapter adapter = new SqlDataAdapter
                {
                    SelectCommand = sqlCommand
                };
                try { adapter.Fill(dataset); } 
                catch (Exception ex) { MessageBox.Show(ex.Message); }
                return dataset;
            }
        }

        public static DataTable FillDataTable(DataTable dataTable, string query, params SqlParameter[] parameters)
        {
            using (SqlConnection sqlConnection = GetSqlConnection())
            {
                SqlCommand sqlCommand = new SqlCommand(query, sqlConnection);
                sqlCommand.Parameters.AddRange(parameters);

                SqlDataAdapter adapter = new SqlDataAdapter
                {
                    SelectCommand = sqlCommand
                };
                try { adapter.Fill(dataTable); }
                catch (Exception ex) { MessageBox.Show(ex.Message); }
                return dataTable;
            }
        }

        public static void BulkWriteToServer(DataTable dataTable)
        {
            using (SqlConnection sqlConnection = GetSqlConnection())
            {
                using (SqlBulkCopy sqlBulkCopy = new SqlBulkCopy(sqlConnection))
                {
                    sqlBulkCopy.DestinationTableName = dataTable.TableName;
                    try { sqlBulkCopy.WriteToServer(dataTable); } 
                    catch (Exception ex) { MessageBox.Show(ex.Message); }
                }
            }
        }
    
        public static int CommitTransaction(string[] commands, params SqlParameter[][] parameters)
        {
            using (SqlConnection sqlConnection = GetSqlConnection())
            {
                using (SqlCommand sqlCommand = sqlConnection.CreateCommand())
                {
                    SqlTransaction sqlTransaction = sqlConnection.BeginTransaction();
                    sqlCommand.Connection = sqlConnection;
                    sqlCommand.Transaction = sqlTransaction;
                    try
                    {
                        int numberRowsAffect = 0;
                        foreach (var cp in commands.Zip(parameters, Tuple.Create))
                        {
                            sqlCommand.CommandText = cp.Item1;
                            sqlCommand.Parameters.AddRange(cp.Item2);
                            numberRowsAffect += sqlCommand.ExecuteNonQuery();
                        }
                        sqlTransaction.Commit();
                        return numberRowsAffect;
                    }
                    catch (Exception exCommit)
                    {
                        try
                        {
                            sqlTransaction.Rollback();
                            MessageBox.Show(exCommit.Message);
                        }
                        catch (Exception exRollback) { MessageBox.Show(exRollback.Message); }
                        return 0;
                    }
                }
            }
        }
    }
}
