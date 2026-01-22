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