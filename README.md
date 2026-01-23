# ScanningSystem

This project is a modern remake and upgrade of my original graduation project.  
It has been redesigned with a cleaner architecture, improved backend logic, real authentication, and a more robust database structure to support real‑time meal scanning in a school canteen environment.

---

## What does this project provide?

The system consists of three major parts:

- **A .NET backend** that handles scanning logic, meal-type switching, admin authentication, CSV imports, and SSE events  
- **A React frontend** that displays live scan results in real time  
- **A SQL Server database** with stored procedures for scanning, reporting, and admin operations  
- **An admin layer** for principals/admins to manage people and view statistics

---

## Features

### 🔄 Real-Time Scanning
- Supports USB barcode scanners  
- Supports manual barcode entry  
- Backend validates scans and prevents duplicates  
- Instant feedback via SSE (Server-Sent Events)  
- Meal-type switching resets the session automatically  

### 🔐 Admin System
- JWT-based admin login  
- Secure password hashing (PBKDF2)  
- Protected admin endpoints  
- CSV bulk import of people  
- Admin-only reporting endpoints  

### 📊 Reporting
- Daily, weekly, and monthly summaries  
- People who haven’t scanned today  
- Meal-type statistics  
- Date-range reporting  

---

## Architecture Overview

![Architecture Diagram](/architecture.png "Architecture Diagram")

### Backend Responsibilities
- Holds current meal type in memory  
- Validates scans and prevents duplicates  
- Calls SQL stored procedures  
- Sends SSE events to the frontend  
- Provides reporting endpoints  
- Handles admin login + JWT authentication  
- Processes CSV imports  

### Frontend Responsibilities
- Displays live scan results  
- Shows time + weekday  
- Shows error popups  
- Resets on meal-change events  
- Reacts instantly to SSE events  
- Provides an admin UI (planned)  

---

## Database Schema

![Database Diagram](/DatabaseDiagram.png "Database Diagram")

### Tables
- **People** – Students, Teachers, Staff, etc.  
- **PersonRoles** – Role lookup  
- **MealTypes** – Breakfast, Lunch, Dinner, etc.  
- **CanteenScans** – Each scan event  
- **Admins** – Admin login accounts (username, hash, salt)  

### Key Constraints
- `UNIQUE (Barcode, MealType, ScanDay)` prevents duplicate scans for the same meal on the same day  

### Stored Procedures

#### Core Procedures
- `sp_RecordCanteenScan` – Validates and inserts a scan  
- `sp_DeleteScan` – Admin undo (useful for testing)  

#### Admin Procedures
- `sp_CreateAdmin` – Creates admin accounts  
- `sp_GetAdminByUsername` – Used for login  
- `sp_BulkInsertPeople` – CSV import  

#### Reporting Procedures
- `sp_GetScansToday`  
- `sp_GetMonthlyScanSummary`  
- `sp_GetWeeklyScanPattern`  
- `sp_GetPeopleNotScannedToday`  
- `sp_GetScansByDateRange`  

These return aggregated counts, not raw rows.

---

## Frontend (React)

The frontend is a live event display powered by Server-Sent Events (SSE).  
It no longer polls the backend — instead, it reacts instantly when the backend broadcasts:

- A successful scan  
- A duplicate scan  
- A meal-type change  

### What the frontend does
- Listens for SSE events  
- Updates instantly when a user scans  
- Shows duplicate-scan popups  
- Clears the list on meal change  
- Displays current time + weekday  
- Responsive layout  
- Keeps a local list of recent scans  

---

## Backend (C# .NET)

The backend acts as the central controller for the entire system.  
It handles scanning, meal sessions, authentication, CSV imports, and real-time updates.

### Backend Responsibilities
- Maintains current meal type in memory  
- Keeps an in-memory HashSet of scanned barcodes  
- Validates scans (scanner or keyboard input)  
- Calls SQL stored procedures  
- Sends SSE events to the frontend  
- Clears the in-memory list when the meal changes  
- Provides reporting endpoints  
- Handles admin login + JWT token generation  
- Processes CSV imports  

### Key Backend Components
- **MealSessionState** – Holds current meal type + scanned barcodes  
- **ScanController** – Handles scan requests  
- **MealController** – Changes meal type  
- **AdminController** – CSV import + admin-only actions  
- **AuthController** – Admin login  
- **EventStream** – SSE broadcasting  
- **AdminService** – CSV import logic  
- **AuthService** – Password hashing + JWT generation  
- **SqlDataManager** – Database access layer  

### Event Flow
1. A barcode is scanned  
2. Backend checks if it’s already scanned this meal  
3. If duplicate → sends `"duplicate-scan"` SSE event  
4. If valid → calls `sp_RecordCanteenScan`  
5. Sends `"scan-success"` SSE event with user details  
6. Frontend updates instantly  
7. When meal changes → backend clears memory + sends `"meal-change"` SSE event  

---

## Setup Instructions

### 1. Clone the repository
```bash
git clone https://github.com/RichLarIva/ScanningSystem.git
```

### 2. Configure SQL Server
Run the SQL initialization script in the `Database` folder to create tables + stored procedures.

### 3. Configure Backend
- Add connection string to  
  `Backend/CanteenSystem/CanteenBackend/appsettings.json`
- Add your JWT key under `"Jwt"`  
- Ensure `MealSessionState` is registered as a singleton  
- SSE endpoint is enabled by default  

### 4. Run Frontend
```bash
cd frontend
npm install
npm run dev
```

Frontend runs at:  
`http://localhost:5173`

---

## Licenses
This project is licensed under the MIT License.  
See the LICENSE file for details.

---

If you want, I can also help you:

- Add a **screenshots section**  
- Add a **demo GIF**  
- Add a **“Tech Stack”** section  
- Add a **“Future Plans”** section  
- Polish the README into a more “GitHub trending” style  

Just tell me what direction you want to take it.