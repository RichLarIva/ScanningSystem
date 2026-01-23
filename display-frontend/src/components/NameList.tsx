import React from "react";
import NameDisplay from "./NameDisplay";

export interface NameItem {
  id: number;
  name: string;
  timeStamp: string;
}

interface NameListProps {
  names: NameItem[];
}

function NameList({ names }: NameListProps) {
  return (
    <div className="names">
      {names.map((name) => (
        <div key={name.id}>
          <NameDisplay
            id={name.id}
            name={name.name}
            timeStamp={name.timeStamp}
          />
        </div>
      ))}
    </div>
  );
}

export default NameList;
