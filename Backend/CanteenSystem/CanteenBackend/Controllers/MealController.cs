using Microsoft.AspNetCore.Mvc;
using CanteenBackend.Services;
using CanteenBackend.Models;

namespace CanteenBackend.Controllers
{
    /// <summary>
    /// Provides endpoints for managing the active meal session.
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    public class MealController : ControllerBase
    {
        private readonly MealSessionState _mealState;
        private readonly EventStream _eventStream;

        public MealController(MealSessionState mealState, EventStream eventStream)
        {
            _mealState = mealState;
            _eventStream = eventStream;
        }

        /// <summary>
        /// Gets the current active meal.
        /// </summary>
        [HttpGet]
        public ActionResult<MealType> GetMeal() => Ok(_mealState.CurrentMeal);


        /// <summary>
        /// Sets the active meal and broadcasts an SSE update.
        /// </summary>
        [HttpPost("{meal}")]
        public async Task<IActionResult> SetMeal(MealType meal)
        {
            _mealState.SetMeal(meal);
            var message = new SseMessage(
                evt: "meal-change",
                data: meal.ToString()
            );
            await _eventStream.BroadcastAsync(message);
            return Ok(new { Message = $"Meal updated.\nActive meal set to {meal}." });
        }
    }
}
