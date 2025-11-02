## How to Run the Application

### Prerequisites
- Windows 10/11
- .NET 6 SDK or newer
- Visual Studio 2022 (recommended)

### Setup

1. Clone the repository:

```bash
git clone https://github.com/roey-gilor/TeraCyteHomeAssignment.git
cd TeraCyteHomeAssignment

2. Copy config.example.json and rename it to config.private.json:
Windows:
copy config.example.json config.private.json

Mac / Linux:
cp config.example.json config.private.json

3. Edit config.private.json and insert your API credentials:
{
  "ApiBaseUrl": "your-server-url",
  "Username": "your-username",
  "Password": "your-password"
}

4. Open solution & run:
Open the solution in Visual Studio 2022
Click Run (WPF app will launch)

## Architecture & Approach

Overview:
The project is structured following the MVVM pattern and clean separation of concerns.
The viewer communicates with a backend inference API, receives image + AI inference results continuously, 
and updates the UI in real time.

Key Components: 
View (WPF) - XAML UI, data binding, visualization
ViewModel - Application state + UI logic
Services - API communication + polling logic
Models - Data objects mapped to API responses
Helpers - Observable Object, Config + logging utilities

Data Flow:
API - ApiClient - PollingService - MainViewModel - WPF UI

- ApiClient manages HTTP calls, authentication and token refresh
- PollingService continuously fetches new images + inference results
- MainViewModel updates UI properties, image, histogram, and history list
- UI auto-updates via WPF data binding

### Application Screenshot

<img width="1882" height="954" alt="screenshot1" src="https://github.com/user-attachments/assets/2da4e7b6-5559-42cc-9bfb-44d03975bc76" />


### History View

<img width="1894" height="864" alt="screenshot2" src="https://github.com/user-attachments/assets/d0250289-0fa5-41a9-b6ce-5697e1fcc877" />
