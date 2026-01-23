using System;
using System.Data;
using CanteenBackend.Models;

namespace CanteenBackend.Data.Repositories
{
    /// <summary>
    /// Provides lookup operations for meal types stored in the database.
    /// </summary>
    public class MealRepository
    {
        private readonly SqlDataManager _db;

        /// <summary>
        /// Creates a new MealRepository using the provided SqlDataManager.
        /// </summary>
        public MealRepository(SqlDataManager db)
        {
            _db = db;
        }

        /// <summary>
        /// Retrieves a meal type by its ID.
        /// </summary>
        /// <param name="mealTypeId">The meal type ID (TINYINT).</param>
        /// <returns>The corresponding MealType enum value, or null if not found.</returns>
        public MealType? GetMealTypeById(byte mealTypeId)
        {
            var cmd = _db.CreateCommand(
                "SELECT MealTypeId FROM MealTypes WHERE MealTypeId = @Id",
                CommandType.Text
            );

            cmd.Parameters.AddWithValue("@Id", mealTypeId);

            var table = _db.ExecuteQuery(cmd);

            if (table.Rows.Count == 0)
                return null;

            return (MealType)mealTypeId;
        }

        /// <summary>
        /// Retrieves all meal types from the database.
        /// </summary>
        /// <returns>A list of MealType enum values.</returns>
        public List<MealType> GetAllMealTypes()
        {
            var cmd = _db.CreateCommand(
                "SELECT MealTypeId FROM MealTypes ORDER BY MealTypeId",
                CommandType.Text
            );

            var table = _db.ExecuteQuery(cmd);

            var list = new List<MealType>();

            foreach (DataRow row in table.Rows)
            {
                var id = Convert.ToByte(row["MealTypeId"]);
                list.Add((MealType)id);
            }

            return list;
        }
    }
}
