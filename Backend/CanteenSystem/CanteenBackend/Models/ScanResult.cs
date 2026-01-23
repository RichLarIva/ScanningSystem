namespace CanteenBackend.Models
{
    public readonly struct  ScanResult
    {
        public bool Success { get; }
        public string Message { get; }

        public ScanResult(bool success, string message)
        {
            Success = success;
            Message = message;
        }
    }
}
