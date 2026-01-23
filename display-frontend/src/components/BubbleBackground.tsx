// Inspired by https://codepen.io/bajzarpa/pen/woYNXp
import React from "react";
import "../styles/BubbleBackground.css";

const random = (min: number, max: number): number =>
  Math.floor(Math.random() * (max - min + 1) + min);

const BubbleBackground: React.FC = () => {
  const bubbles = Array.from({ length: 300 }, (_, i) => {
    const size = random(5, 100);
    const left = `${random(1, 100)}vw`;
    const bottom = `${random(1, 100)}vh`;
    const duration = `${random(7, 40)}s`;
    const bgPos = i % 2 === 0 ? "top right" : "center";
    const animationName = `move${i}`;
    const finalBottom = `${random(0, 100)}vh`;
    const translateX = `${random(-100, 200)}px`;

    // Inject keyframes dynamically
    const styleSheet = document.styleSheets[0];
    const keyframes = `
      @keyframes ${animationName} {
        0% {
          bottom: -100px;
        }
        100% {
          bottom: ${finalBottom};
          transform: translate(${translateX}, 0);
          opacity: 0;
        }
      }
    `;
    styleSheet.insertRule(keyframes, styleSheet.cssRules.length);

    const bubbleStyle: React.CSSProperties = {
      width: `${size}px`,
      height: `${size}px`,
      left,
      bottom,
      animation: `${animationName} ${duration} infinite`,
      background: `radial-gradient(ellipse at ${bgPos}, hsl(278, 49%, 76%) 0%, hsl(249, 65%, 51%) 46%, hsl(281, 91%, 22%) 100%)`,
    };

    return <div key={i} className="bubble" style={bubbleStyle} />;
  });

  return <div className="canvas">{bubbles}</div>;
};

export default BubbleBackground;
