import { useState } from 'react'
import reactLogo from './assets/react.svg'
import viteLogo from '/vite.svg'
import './App.css'
import BubbleBackground from './components/BubbleBackground'

function App() {
  const [count, setCount] = useState(0)

  return (
    <>
      <BubbleBackground/>
    </>
  )
}

export default App
