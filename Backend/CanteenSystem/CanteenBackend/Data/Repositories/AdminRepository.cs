using System;
using System.Data;
using CanteenBackend.Models;

namespace CanteenBackend.Data.Repositories
{
    /// <summary>
    /// Provides database operations for admin authentication and bulk people import.
    /// </summary>
    public class AdminRepository
    {
        private readonly SqlDataManager _db;

        public AdminRepository(SqlDataManager db)
        {
            _db = db;
        }

        // ============================================================
        // ADMIN AUTHENTICATION
        // ============================================================

        /// <summary>
        /// Retrieves an admin record by username using sp_GetAdminByUsername.
        /// </summary>
        public AdminRecord? GetAdminByUsername(string username)
        {
            var cmd = _db.CreateCommand("sp_GetAdminByUsername", CommandType.StoredProcedure);
            cmd.Parameters.AddWithValue("@Username", username);

            var table = _db.ExecuteQuery(cmd);

            if (table.Rows.Count == 0)
                return null;

            var row = table.Rows[0];

            return new AdminRecord
            {
                AdminId = Convert.ToInt32(row["AdminId"]),
                Username = row["Username"].ToString() ?? "",
                PasswordHash = (byte[])row["PasswordHash"],
                PasswordSalt = (byte[])row["PasswordSalt"]
            };
        }

        /// <summary>
        /// Creates a new admin using sp_CreateAdmin.
        /// </summary>
        public ScanResult CreateAdmin(string username, byte[] hash, byte[] salt)
        {
            var cmd = _db.CreateCommand("sp_CreateAdmin", CommandType.StoredProcedure);
            cmd.Parameters.AddWithValue("@Username", username);
            cmd.Parameters.AddWithValue("@PasswordHash", hash);
            cmd.Parameters.AddWithValue("@PasswordSalt", salt);

            var table = _db.ExecuteQuery(cmd);

            if (table.Rows.Count == 0)
                return new ScanResult(false, "Unknown error");

            var row = table.Rows[0];

            return new ScanResult(
                Convert.ToInt32(row["Success"]) == 1,
                row["Message"].ToString() ?? ""
            );
        }

        // ============================================================
        // BULK INSERT PEOPLE (CSV IMPORT)
        // ============================================================

        /// <summary>
        /// Inserts a person using sp_BulkInsertPeople.
        /// </summary>
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
                Convert.ToInt32(row["Success"]) == 1,
                row["Message"].ToString() ?? ""
            );
        }
    }
}
