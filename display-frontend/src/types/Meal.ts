export const Meal = {
  Breakfast: "Breakfast",
  Lunch: "Lunch",
  Dinner: "Dinner",
  Snack: "Snack",
  Special: "Special",
} as const;

export type Meal = (typeof Meal)[keyof typeof Meal];

/**
 * Convert a Meal enum value into a readable string.
 */
export function mealToString(meal: Meal): string {
  switch (meal) {
    case Meal.Breakfast:
      return "Breakfast";
    case Meal.Lunch:
      return "Lunch";
    case Meal.Dinner:
      return "Dinner";
    case Meal.Snack:
      return "Snack";
    case Meal.Special:
      return "Special meal";
    default:
      return "Unknown meal";
  }
}
