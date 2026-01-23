using System.Collections.Concurrent;
using CanteenBackend.Models;

namespace CanteenBackend.Services
{
    public class EventStream
    {
        private readonly ConcurrentBag<StreamWriter> _clients = new();

        public void AddClient(StreamWriter client)
        {
            _clients.Add(client);
        }

        public async Task BroadcastAsync(SseMessage message)
        {
            foreach (var client in _clients) 
            { 
                await client.WriteAsync($"event: {message.Event}\n");
                await client.WriteAsync($"data: {message.Data}\n\n"); 
                await client.FlushAsync(); 
            }
        }
    }
}
