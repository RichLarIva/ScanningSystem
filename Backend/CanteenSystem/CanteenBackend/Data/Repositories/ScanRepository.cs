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

        /// <summary>
        /// Creates a new ScanRepository using the provided SqlDataManager.
        /// </summary>
        public ScanRepository(SqlDataManager db)
        {
            _db = db;
        }

        /// <summary>
        /// Calls the sp_RecordCanteenScan stored procedure to record a scan event.
        /// </summary>
        /// <param name="barcode">The scanned barcode.</param>
        /// <param name="meal">The meal type being recorded.</param>
        /// <returns>A ScanResult struct containing success status and message.</returns>
        public ScanResult RecordScan(string barcode, MealType meal)
        {
            var cmd = _db.CreateCommand("sp_RecordCanteenScan", CommandType.StoredProcedure);
            cmd.Parameters.AddWithValue("@Barcode", barcode);
            cmd.Parameters.AddWithValue("@MealType", (byte)meal);

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
