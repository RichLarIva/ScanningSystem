using System;
using System.Data;
using CanteenBackend.Models;

namespace CanteenBackend.Data.Repositories
{
    /// <summary>
    /// Provides administrative database operations such as bulk inserting people.
    /// </summary>
    public class AdminRepository
    {
        private readonly SqlDataManager _db;

        /// <summary>
        /// Creates a new AdminRepository using the provided SqlDataManager.
        /// </summary>
        public AdminRepository(SqlDataManager db)
        {
            _db = db;
        }

        /// <summary>
        /// Calls the sp_BulkInsertPeople stored procedure to insert a single person.
        /// </summary>
        /// <param name="fullName">The person's full name.</param>
        /// <param name="barcode">The person's barcode.</param>
        /// <param name="roleId">The role ID (TINYINT).</param>
        /// <returns>A ScanResult-like struct indicating success or failure.</returns>
        public ScanResult InsertPerson(string fullName, string barcode, byte roleId)
        {
            var cmd = _db.CreateCommand("sp_BulkInsertPeople", CommandType.StoredProcedure);
            cmd.Parameters.AddWithValue("@FullName", fullName);
            cmd.Parameters.AddWithValue("@Barcode", barcode);
            cmd.Parameters.AddWithValue("@RoleId", roleId);

            var table = _db.ExecuteQuery(cmd);

            if (table.Rows.Count == 0)
                return new ScanResult(false, "Unknown error");

            var row = table.Rows[0];

            return new ScanResult(
                success: Convert.ToInt32(row["Success"]) == 1,
                message: row["Message"].ToString() ?? ""
            );
        }
    }
}
