import React from "react";
import { FaClock } from "react-icons/fa";
import { mealToString, Meal } from "../types/Meal";

interface HeaderProps {
    currentMeal: Meal;
    weekday: string;
    time: string;
}

const Header: React.FC<HeaderProps> = ({ currentMeal, weekday, time }) => {
    return (
        <header className="App-header">
            <p className="currentDay">{weekday}</p>

            <div className="meal-display">
                <span className="meal-label">Current meal: </span>
                <span className="meal-value">{mealToString(currentMeal)}</span>
            </div>

            <div className="clock-wrapper">
                <FaClock className="clock-icon" />
                <span className="time-value">{time}</span>
            </div>
        </header>
    );

};

export default Header;