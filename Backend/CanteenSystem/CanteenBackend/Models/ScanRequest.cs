namespace CanteenBackend.Models
{
    /// <summary>
    /// Represents a scan request coming from the frontend.
    /// </summary>
    public class ScanRequest
    {
        public string Barcode { get; set; } = string.Empty;
    }
}
