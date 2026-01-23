namespace CanteenBackend.Models
{
    public class Person
    {
        public int PersonId { get; set; }
        public string FullName { get; set; } = "";
        public string Barcode { get; set; } = "";
        public PersonRole Role { get; set; }
    }
}
