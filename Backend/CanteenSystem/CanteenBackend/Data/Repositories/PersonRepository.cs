using System.Data;
using CanteenBackend.Models;

namespace CanteenBackend.Data.Repositories
{
    /// <summary>
    /// Provides lookup operations for people in the database.
    /// </summary>
    public class PersonRepository
    {
        private readonly SqlDataManager _db;

        /// <summary>
        /// Creates a new PersonRepository using the provided SqlDataManager.
        /// </summary>
        public PersonRepository(SqlDataManager db)
        {
            _db = db;
        }

        /// <summary>
        /// Retrieves a person by their barcode.
        /// </summary>
        /// <param name="barcode">The barcode to search for.</param>
        /// <returns>A Person object if found, otherwise null.</returns>
        public Person? GetByBarcode(string barcode)
        {
            var cmd = _db.CreateCommand(
                "SELECT PersonId, FullName, Barcode, RoleId FROM People WHERE Barcode = @Barcode",
                CommandType.Text
            );

            cmd.Parameters.AddWithValue("@Barcode", barcode);

            var table = _db.ExecuteQuery(cmd);
            if (table.Rows.Count == 0)
                return null;

            var row = table.Rows[0];

            return new Person
            {
                PersonId = Convert.ToInt32(row["PersonId"]),
                FullName = row["FullName"].ToString() ?? "",
                Barcode = row["Barcode"].ToString() ?? "",
                Role = (PersonRole)Convert.ToByte(row["RoleId"])
            };
        }
    }
}
