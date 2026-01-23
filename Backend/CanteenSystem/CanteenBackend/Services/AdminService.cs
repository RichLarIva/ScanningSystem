using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using CanteenBackend.Data.Repositories;
using CanteenBackend.Models;

namespace CanteenBackend.Services
{
    /// <summary>
    /// Handles administrative operations such as CSV import of people.
    /// </summary>
    public class AdminService
    {
        private readonly AdminRepository _adminRepository;

        /// <summary>
        /// Creates a new AdminService with required dependencies.
        /// </summary>
        public AdminService(AdminRepository adminRepository)
        {
            _adminRepository = adminRepository;
        }

        /// <summary>
        /// Parses a CSV file and inserts each person into the database.
        /// </summary>
        /// <param name="file">The uploaded CSV file.</param>
        /// <returns>A list of ScanResult entries for each row processed.</returns>
        public async Task<List<ScanResult>> ImportPeopleFromCsvAsync(IFormFile file)
        {
            var results = new List<ScanResult>();

            using var stream = file.OpenReadStream();
            using var reader = new StreamReader(stream);

            while (!reader.EndOfStream)
            {
                var line = await reader.ReadLineAsync();
                if (string.IsNullOrWhiteSpace(line))
                    continue;

                var parts = line.Split(',');

                if (parts.Length < 3)
                {
                    results.Add(new ScanResult(false, $"Invalid row: {line}"));
                    continue;
                }

                var fullName = parts[0].Trim();
                var barcode = parts[1].Trim();
                var roleId = byte.TryParse(parts[2].Trim(), out var r) ? r : (byte)0;

                var result = _adminRepository.InsertPerson(fullName, barcode, roleId);
                results.Add(result);
            }

            return results;
        }
    }
}
