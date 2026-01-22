# ScanningSystem
 This project is a modern remake and upgrade of my original graduation project.
 It has been redesigned with a cleaner architecture, improved backend logic, and a more robust database structure to support real‑time meal scanning in a school canteen environment.

## Alright what does this project provide?
The project provides:
- A backend service that handles scanning logic, meal-type switching, and SSE events
- A React frontend that displays live scan results
- A SQL Server database with stored procedures for scanning and reporting
- A reporting layer for principals/admins to view daily, weekly, and monthly statistics

## Features
### Real-Time Scanning
- Supports USB barcode scanners
- Supports manual barcode entry via keyboard
- Backend validates scans and prevents duplicates
- Instant feedback via SSE (Server-Sent Events)
---

## Architecture Overview
![Architecture Diagram](/architecture.png "Architecture Diagram")

### Backend Responsibilities
* Holds current meal type in memory
* Validates scans
* Calls SQL stored procedures
* Sends SSE events to frontend
* Provides reporting endpoints

### Frontend Responsibilities
* Displays live scan results
* Shows time + weekday
* Shows error popups
* Resets on meal-change events
---
## Database Schema
![Database Schema](/DatabaseDiagram.png "Database Schema")
### Tables
* People - Students, Teachers, Staff etc.
* PersonRoles - Role lookup (Student, Teacher, Staff, Admin)
* MealTypes - Breakfast, Lunch, Dinner, etc.
* CanteenScans - Each scan event

### Key Constraints
* ``UNIQUE (Barcode, MealType, ScanDay)`` Prevents duplicate scans for the same meal on the same day

### Stored Procedures
#### Core Procedures
* `sp_RecordCanteenScan` - Validates and inserts a scan
(rest are upcoming procedures for reporting etc.)
* `sp_DeleteScan` - Admin undo (for instance when testing the system)
#### Reporting Procedures
* `sp_GetScansToday`
* `sp_GetMonthlyScanSummary`
* `sp_GetWeeklyScanPattern`
* `sp_GetPeopleNotScannedToday`
* `sp_GetScansByDateRange`
These return counts, not raw rows.
---

 -- All sections below are currently planned but not done yet however I am writing the text so the info is already done --
## Frontend (React)
The frontend is a live event display powered by Server-Sent Events (SSE). It no longer polls the backend(!!!) instead, it reacts instantly when the backend broadcasts a scan event or a meal-change event.

### What the frontend does
* Listens for SSE events from the backend
* Updates instantly when a user scans
* Shows a popup when a duplicate scan is attempted
* Clears the list automatically when the meal type changes
* Displays current time and weekday
* Responsive design for different screen sizes
* Maintains a local list of recently scanned names

## Backend (C# .NET)
The backend acts as the central controller for the entire system. It handles all scanning logic, manages the current meal session, and pushes real-time updates to the frontend using Server-Sent Events (SSE). Rather than the old way of polling the javascript backend every second, and the C# backend sending websocket messages to JS backend when a card would be scanned.

### So What does the backend do?
* Maintains the current meal type in memory
* Keeps an in-memory HashSet of barcodes scanned during the current meal (O(1) lookups)
* Validates scans(scanner or keyboard input)
* Calls SQL stored procedures to record scans
* Sends SSE events to the frontend when:
    - A scan is successful
    - A scan is rejected (duplicate)
    - The meal type changes
* Clears the in-memory scan list when the meal changes
* Provides reporting endpoints for admin dashboards

### Key Backend Components
* **MealSessionState** - Holds the current meal type and the in-memory list of scanned barcodes.
* **ScanController** - Receives scan requests, checks duplicates, calls SQL, and broadcasts SSE events.
* **MealController** - Allows changing the current meal type and resets the scan list.
* **SSEService** - Manages Server-Sent Events connections and broadcasts messages to connected clients.
* **DatabaseService** - Encapsulates all database interactions, including calling stored procedures.

### Event Flow
1. A barcode is scanned (via USB scanner or keyboard input) sends to the backend.
2. Backend checks if the barcode is already scanned for this meal
3. If duplicate -> backend sends SSE event `"duplicate-scan"`
4. If valid -> backend calls `sp_RecordCanteenScan` and sends SSE event `"scan-success"` with user details
5. Frontend updates instantly based on the SSE events received without polling (WOW!!)
6. When meal changes -> backend clears in-memory list and sends `"meal-change"` SSE event to the frontend.

## Setup Instructions
### 1. Clone the repository
```bash
git clone https://github.com/RichLarIva/ScanningSystem.git
```

### 2. Configure SQL Server
Run the SQL initialization script located in the `Database` folder to create the necessary tables and stored procedures.
### 3. Configure Backend
* Add connection string to `Backend\CanteenSystem\CanteenBackend\appsettings.json`
* Register `MealSessionState` as a singleton
* Enable SSE endpoint
### 4. Run Frontend
```bash
cd frontend
npm install
npm run dev
```
The frontend runs on Vite's dev server by default at `http://localhost:5173`.

# Licenses
This project is licensed under the MIT License. See the LICENSE file for details.