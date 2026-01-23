using CanteenBackend.Models;

namespace CanteenBackend.Services
{
    public class MealSessionState
    {
        private MealType _currentMeal = MealType.Breakfast;
        public MealType CurrentMeal => _currentMeal;

        public void SetMeal(MealType meal)
        {
            _currentMeal = meal;
        }
    }
}
