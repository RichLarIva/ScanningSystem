import { useEffect, useState } from 'react'
import { useSSE } from "./hooks/useSSE";
import './App.css'
import BubbleBackground from './components/BubbleBackground'
import Header from './components/Header'
import { Meal } from './types/Meal'
import ScannedPopup from './components/ScannedPopup';
import NameList from './components/NameList';

function App() {

  const [names, setNames] = useState<any[]>([]);
  const [isOpen, setIsOpen] = useState<boolean>(false);
  const [currentMeal, setCurrentMeal] = useState<Meal>(Meal.Breakfast); 

  useSSE("https://localhost:7220/api/events", {
    "scan-success": (data) => {
      const now = new Date();
      const time = now.getHours() + ":" + now.getMinutes() + ":" + now.getSeconds();
      console.table(data);
      setNames((prev) => [
        {id: prev.length + 1, name: data.name, timeStamp: time},
        ...prev,
      ])
      console.log("hey data meal: " + data.meal);
      setCurrentMeal(data.meal);
    },

    "duplicate-scan": () =>
    {
      console.log("duplicate scan event received");
      setIsOpen(true);
      setTimeout(() => setIsOpen(false), 15000);
    }
  });

  const [dateState, setDateState] = useState(new Date()); 
  const [dayState, setDayState] = useState(new Date());
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
      {isOpen && ( <ScannedPopup/>)}
      <div className="name-list-container"> 
        <NameList names={names}/> 
      </div>
      <BubbleBackground/>
    </div>
  )
}

export default App
