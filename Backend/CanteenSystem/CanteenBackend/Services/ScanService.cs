using System.Threading.Tasks;
using CanteenBackend.Data.Repositories;
using CanteenBackend.Models;

namespace CanteenBackend.Services
{
    /// <summary>
    /// Handles the full workflow of processing a canteen scan:
    /// - Validates the barcode
    /// - Retrieves the person
    /// - Records the scan via stored procedure
    /// - Broadcasts SSE updates to all connected clients
    /// </summary>
    public class ScanService
    {
        private readonly ScanRepository _scanRepository;
        private readonly PersonRepository _personRepository;
        private readonly MealSessionState _mealState;
        private readonly EventStream _eventStream;

        /// <summary>
        /// Creates a new ScanService with required dependencies.
        /// </summary>
        public ScanService(
            ScanRepository scanRepository,
            PersonRepository personRepository,
            MealSessionState mealState,
            EventStream eventStream)
        {
            _scanRepository = scanRepository;
            _personRepository = personRepository;
            _mealState = mealState;
            _eventStream = eventStream;
        }

        /// <summary>
        /// Processes a barcode scan by validating the person, recording the scan,
        /// and broadcasting an SSE event to all connected clients.
        /// Also broadcasts a duplicate-scan event if the person already scanned today.
        /// </summary>
        /// <param name="barcode">The scanned barcode.</param>
        /// <returns>A ScanResult indicating success or failure.</returns>
        public async Task<ScanResult> ProcessScanAsync(string barcode)
        {
            // Lookup the person
            var person = _personRepository.GetByBarcode(barcode);
            if (person == null)
            {
                return new ScanResult(false, "Unknown barcode");
            }

            // Record the scan in the database
            var result = _scanRepository.RecordScan(barcode, _mealState.CurrentMeal);

            // Duplicate scan case
            if (!result.Success && result.Message.Contains("Already scanned"))
            {
                var duplicateMessage = new SseMessage(
                    evt: "scan-duplicate",
                    data: $"{person.FullName} already scanned for {_mealState.CurrentMeal}"
                );

                await _eventStream.BroadcastAsync(duplicateMessage);
                return result;
            }

            // Normal scan case
            var message = new SseMessage(
                evt: "scan",
                data: $"{person.FullName} scanned for {_mealState.CurrentMeal}"
            );

            await _eventStream.BroadcastAsync(message);

            return result;
        }

        /// <summary>
        /// Broadcasts a reset event to all connected SSE clients.
        /// Displays should clear their name lists when receiving this event.
        /// </summary>
        public async Task BroadcastResetAsync()
        {
            var resetMessage = new SseMessage(
                evt: "reset",
                data: "clear"
            );

            await _eventStream.BroadcastAsync(resetMessage);
        }
    }
}
