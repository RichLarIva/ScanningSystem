namespace CanteenBackend.Models
{
    public class CanteenScan
    {
        public int CanteenScanId { get; set; }
        public DateTime ScanDate { get; set; }
        public string Barcode { get; set; } = "";
        public MealType Meal { get; set; }
    }
}
