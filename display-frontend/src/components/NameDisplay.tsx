import React from "react";

interface NameDisplayProps {
    id: number;
    name: string;
    timeStamp: string;
}

const NameDisplay: React.FC<NameDisplayProps> = ({id, name, timeStamp}) => {
    return (
    <div className="name">
        <h3>{id}</h3>
        <h3>{name}</h3>
        <h3>{timeStamp}</h3>
    </div>
    )
}

export default NameDisplay;