import { useEffect, useState } from 'react'
import reactLogo from './assets/react.svg'
import viteLogo from '/vite.svg'
import './App.css'
import BubbleBackground from './components/BubbleBackground'
import Header from './components/Header'
import { Meal } from './types/Meal'

function App() {

  const [currentMeal, setCurrentMeal] = useState<Meal>(Meal.Breakfast); 
  const [dateState, setDateState] = useState(new Date()); const [dayState, setDayState] = useState(new Date());

  const weekDay = [ "Sunday", "Monday", "Tuesday", "Wednesday", "Thursday", "Friday", "Saturday", ];

  useEffect(() => {
  // Update immediately
  setDateState(new Date());

  // Calculate ms until next minute
  const now = new Date();
  const msUntilNextMinute = (60 - now.getSeconds()) * 1000;

  // First timeout aligns to the next minute
  const timeout = setTimeout(() => {
    setDateState(new Date());

    // After alignment, update every minute exactly at :00
    const interval = setInterval(() => {
      setDateState(new Date());
    }, 60000);

    // Cleanup interval when component unmounts
    return () => clearInterval(interval);
  }, msUntilNextMinute);

  // Cleanup timeout
  return () => clearTimeout(timeout);
}, []);
 
  useEffect(() => { const interval = setInterval(() => setDayState(new Date()), 25000); return () => clearInterval(interval); }, []);

  return (
    <div className="App">
      <Header
        currentMeal={currentMeal}
        weekday={weekDay[dayState.getDay()]}
        time={dateState.toLocaleString("en-us", {
          hour: "numeric",
          minute: "numeric",
          hour12: false,
        })}
      />
      <BubbleBackground/>
    </div>
  )
}

export default App
