using CanteenBackend.Models;
using System.Collections.Generic;

namespace CanteenBackend.Services
{
    /// <summary>
    /// Holds the current meal type and tracks scanned barcodes
    /// for the active meal session.
    /// </summary>
    public class MealSessionState
    {
        private MealType _currentMeal = MealType.Breakfast;
        public MealType CurrentMeal => _currentMeal;

        // In-memory O(1) lookup for duplicate detection
        private readonly HashSet<string> _scannedBarcodes = new();

        /// <summary>
        /// Checks if a barcode has already been scanned this meal.
        /// </summary>
        public bool IsDuplicate(string barcode)
        {
            return _scannedBarcodes.Contains(barcode);
        }

        /// <summary>
        /// Adds a barcode to the in-memory scanned list.
        /// </summary>
        public void AddScan(string barcode)
        {
            _scannedBarcodes.Add(barcode);
        }

        /// <summary>
        /// Changes the meal type and resets the scanned list.
        /// </summary>
        public void SetMeal(MealType meal)
        {
            _currentMeal = meal;
            _scannedBarcodes.Clear();
        }
    }
}
