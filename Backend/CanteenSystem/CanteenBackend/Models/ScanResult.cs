using CanteenBackend.Models;

public readonly struct ScanResult
{
    public bool Success { get; }
    public string Message { get; }
    public string? PersonName { get; }
    public MealType? Meal { get; }

    public ScanResult(bool success, string message, string? personName = null, MealType? meal = null)
    {
        Success = success;
        Message = message;
        PersonName = personName;
        Meal = meal;
    }
}
