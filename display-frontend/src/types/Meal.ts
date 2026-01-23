export const Meal = {
  Breakfast: 1,
  Lunch: 2,
  Dinner: 3,
  Snack: 4,
  Special: 5,
} as const;

export type Meal = (typeof Meal)[keyof typeof Meal];

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
