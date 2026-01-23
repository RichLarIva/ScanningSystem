using System;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;

namespace ScannerProgram
{
    internal class Program
    {
        private static readonly HttpClient Http = new()
        {
            BaseAddress = new Uri("http://localhost:5000") // Backend URL
        };

        private static async Task Main(string[] args)
        {
            Console.WriteLine("Scanner client initialized. Ready to scan...");

            while (true)
            {
                string? barcode = Console.ReadLine();

                if (string.IsNullOrWhiteSpace(barcode))
                {
                    continue; // ignore empty inputs.
                }

                await SendScanAsync(barcode);
            }
        }

        private static async Task SendScanAsync(string barcode)
        {
            try
            {
                var response = await Http.PostAsJsonAsync("/api/scan", new { Barcode = barcode });

                if(response.IsSuccessStatusCode)
                {
                    Console.WriteLine($"Sent: {barcode}");
                }
                else
                {
                    Console.WriteLine($"Error sending barcode: {response.StatusCode}");
                }
            }
            catch(Exception ex)
            {
                Console.WriteLine($"Failed to send scan: {ex.Message}");
            }
        }
    }
}