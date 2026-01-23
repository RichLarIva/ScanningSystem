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
        /// <param name="connectionString">The required SQL Server connection string.</param>
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
            if (_connection == null)
                throw new InvalidOperationException("SQL connection has not been established.");

            return new SqlCommand(query, _connection)
            {
                CommandTimeout = 60,
                CommandType = type
            };
        }

        /// <summary>
        /// Executes a SELECT query or stored procedure that returns rows.
        /// Returns the result as a DataTable.
        /// </summary>
        public DataTable ExecuteQuery(SqlCommand command)
        {
            var table = new DataTable();

            try
            {
                using var adapter = new SqlDataAdapter(command);
                adapter.Fill(table);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"SQL query error: {ex.Message}");
                throw;
            }
            return table;
        }

        /// <summary>
        /// Executes INSERT, UPDATE, DELETE, or stored procedures that do not return rows.
        /// Returns the number of affected rows.
        /// </summary>
        public int ExecuteNonQuery(SqlCommand command)
        {
            try
            {
                return command.ExecuteNonQuery();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"SQL NonQuery error: {ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// Executes a query that returns a single value (first column, first row).
        /// Useful for COUNT(), SCOPE_IDENTITY(), etc.
        /// </summary>
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

        /// <summary>
        /// Closes the SQL connection if it is open.
        /// </summary>
        public void Close()
        {
            if (_connection != null && _connection.State != ConnectionState.Closed)
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
