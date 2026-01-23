namespace CanteenBackend.Models
{
    /// <summary>
    /// Represents an admin user stored in the database.
    /// </summary>
    public class AdminRecord
    {
        public int AdminId { get; set; }
        public string Username { get; set; } = "";
        public byte[] PasswordHash { get; set; } = Array.Empty<byte>();
        public byte[] PasswordSalt { get; set; } = Array.Empty<byte>();
    }
}
