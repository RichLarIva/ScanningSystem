using System.Threading.Tasks;
using CanteenBackend.Data.Repositories;
using CanteenBackend.Models;

namespace CanteenBackend.Services
{
    /// <summary>
    /// Handles the full workflow of processing a canteen scan:
    /// - Validates the barcode
    /// - Retrieves the person
    /// - Checks in-memory duplicate state
    /// - Records the scan via stored procedure
    /// - Broadcasts SSE updates to all connected clients
    /// </summary>
    public class ScanService
    {
        private readonly ScanRepository _scanRepository;
        private readonly PersonRepository _personRepository;
        private readonly MealSessionState _mealState;
        private readonly EventStream _eventStream;

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
        /// Processes a barcode scan by validating the person, checking duplicates,
        /// recording the scan, and broadcasting SSE events.
        /// </summary>
        public async Task<ScanResult> ProcessScanAsync(string barcode)
        {
            // Lookup the person
            var person = _personRepository.GetByBarcode(barcode);
            if (person == null)
            {
                return new ScanResult(false, "Unknown barcode");
            }

            // ------------------------------------------------------------
            // In-memory duplicate detection (instant, no SQL call)
            // ------------------------------------------------------------
            if (_mealState.IsDuplicate(barcode))
            {
                var duplicateMessage = new SseMessage(
                    evt: "scan-duplicate",
                    data: $"{person.FullName} already scanned for {_mealState.CurrentMeal}"
                );

                await _eventStream.BroadcastAsync(duplicateMessage);

                return new ScanResult(false, "Already scanned for this meal.");
            }

            // ------------------------------------------------------------
            // Record the scan in the database
            // ------------------------------------------------------------
            var result = _scanRepository.RecordScan(barcode, _mealState.CurrentMeal);

            if (!result.Success)
            {
                // SQL rejected it (rare, but possible)
                return result;
            }

            // ------------------------------------------------------------
            // Add to in-memory list AFTER successful DB insert
            // ------------------------------------------------------------
            _mealState.AddScan(barcode);

            // ------------------------------------------------------------
            // Broadcast success event
            // ------------------------------------------------------------
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
