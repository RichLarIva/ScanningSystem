using System;
using System.Data;
using Microsoft.Data.SqlClient;

namespace CanteenBackend.Data
{
    /// <summary>
    /// Handles SQL Server connections and execution of T-SQL commands.
    /// Designed for use with Windows Authentication (Trusted_Connection=True).
    /// </summary>
    public class SqlDataManager : IDisposable
    {
        private SqlConnection? _connection;

        /// <summary>
        /// Creates an empty manager. Call Connect() before executing commands.
        /// </summary>
        public SqlDataManager()
        { 
        }

        /// <summary>
        /// Opens a SQL Server connection using the provided connection string.
        /// </summary>
        /// <param name="connectionString">The Required Connection string</param>
        public void Connect(string connectionString)
        { 
            try
            {
                _connection = new SqlConnection(connectionString);
                _connection.Open();
                Console.WriteLine("Connected to SQL Server using Windows Authentication");
            }
            catch (SqlException ex)
            {
                Console.WriteLine($"SQL Connection Error: {ex.Message}");
                throw;
            }
        }
        /// <summary> 
        /// Creates a SqlCommand object bound to the active connection. 
        /// Supports both text queries and stored procedures. 
        /// </summary>
        public SqlCommand CreateCommand(string query, CommandType type = CommandType.Text)
        {
            if(_connection == null)
                throw new InvalidOperationException("SQL connection has not been established.");

            var cmd = new SqlCommand(query, _connection)
            {
                CommandTimeout = 60,
                CommandType = type
            };

            return cmd;
        }

        #region Stored Procedure Helpers
        /// <summary>
        /// Calls the AddScan to process a barcode scan.
        /// </summary>
        /// <param name="barcode">scanned barcode input</param>
        /// <returns>A DataTable containing the result of the scan operation.</returns>
        public DataTable Call_AddScan(string barcode)
        {
            var command = CreateCommand("sp_AddScan", CommandType.StoredProcedure);
            command.Parameters.AddWithValue("@Barcode", barcode);

            return ExecuteQuery(command);
        }


        #endregion

        #region Generic Execution Helpers

        public DataTable ExecuteQuery(SqlCommand command)
        {
            var table = new DataTable();

            try
            {
                using var adapter = new SqlDataAdapter(command);
                adapter.Fill(table);
            }
            catch(Exception ex)
            {
                Console.WriteLine($"SQL query error: {ex.Message}");
                throw;
            }
            return table;
        }

        public int ExecuteNonQuery(SqlCommand command)
        {
            try
            {
                return command.ExecuteNonQuery();
            }
            catch(Exception ex)
            {
                Console.WriteLine($"SQL scalar error: {ex.Message}");
                throw;
            }
        }

        public object? ExecuteScalar(SqlCommand command)
        {
            try
            {
                return command.ExecuteScalar();
            }
            catch (SqlException ex)
            {
                Console.WriteLine($"SQL Scalar Error: {ex.Message}");
                throw;
            }
        }

        #endregion

        /// <summary>
        /// Closes the SQL connection if it is open.
        /// </summary>
        public void Close()
        {
            if(_connection != null && _connection.State != ConnectionState.Closed)
            {
                _connection.Close();
            }
        }

        /// <summary>
        /// Ensures the connection is closed and disposed when the object is destroyed.
        /// </summary>
        public void Dispose()
        {
            Close();
            _connection?.Dispose();
        }
    }
}
