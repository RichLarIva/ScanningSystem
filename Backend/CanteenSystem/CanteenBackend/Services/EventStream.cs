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

        public void RemoveClient(StreamWriter client)
        {
            // ConcurrentBag does not support removal, so we rebuild it
            var newBag = new ConcurrentBag<StreamWriter>();

            foreach (var c in _clients)
            {
                if (c != client)
                    newBag.Add(c);
            }

            // Clear old bag
            while (_clients.TryTake(out _)) { }

            // Copy back
            foreach (var c in newBag)
                _clients.Add(c);
        }

        public async Task BroadcastAsync(SseMessage message)
        {
            var deadClients = new List<StreamWriter>();

            foreach (var client in _clients)
            {
                try
                {
                    await client.WriteAsync($"event: {message.Event}\n");
                    await client.WriteAsync($"data: {message.Data}\n\n");
                    await client.FlushAsync();
                }
                catch
                {
                    // Client disconnected
                    deadClients.Add(client);
                }
            }

            // Remove dead clients
            foreach (var dead in deadClients)
            {
                RemoveClient(dead);
            }
        }
    }
}
