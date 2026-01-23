using System;
using System.Data;
using CanteenBackend.Models;

namespace CanteenBackend.Data.Repositories
{
    /// <summary>
    /// Provides data access methods for canteen scan operations.
    /// Wraps stored procedure calls and maps results to C# models.
    /// </summary>
    public class ScanRepository
    {
        private readonly SqlDataManager _db;

        public ScanRepository(SqlDataManager db)
        {
            _db = db;
        }

        /// <summary>
        /// Calls the sp_RecordCanteenScan stored procedure to record a scan event.
        /// </summary>
        public ScanResult RecordScan(string barcode, MealType meal)
        {
            var cmd = _db.CreateCommand("sp_RecordCanteenScan", CommandType.StoredProcedure);
            cmd.Parameters.AddWithValue("@Barcode", barcode);
            cmd.Parameters.AddWithValue("@MealType", (byte)meal);

            var table = _db.ExecuteQuery(cmd);

            if (table.Rows.Count == 0)
                return new ScanResult(false, "Unknown error");

            var row = table.Rows[0];

            bool success = Convert.ToInt32(row["Success"]) == 1;
            string message = row["Message"]?.ToString() ?? "";

            // Match the actual column name returned by the stored procedure
            string? personName = row.Table.Columns.Contains("PersonName")
                ? row["PersonName"]?.ToString()
                : null;

            return new ScanResult(
                success: success,
                message: message,
                personName: personName,
                meal: meal
            );
        }
    }
}
