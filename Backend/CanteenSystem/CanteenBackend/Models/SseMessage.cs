namespace CanteenBackend.Models
{
    public readonly struct SseMessage
    {
        public string Event { get; }
        public string Data { get; }

        public SseMessage(string evt, string data)
        {
            Event = evt;
            Data = data;
        }
    }
}
