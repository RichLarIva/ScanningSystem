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
