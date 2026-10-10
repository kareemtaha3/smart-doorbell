# Smart Doorbell System 🔔

An end-to-end event-driven Smart Doorbell solution built with **ASP.NET Core 9**, **Next.js 15**, **MQTT (Mosquitto)**, **MinIO Object Storage**, **SQL Server**, **Redis**, **SignalR**, and **WebPush Notifications**.

The system features real-time device communication between hardware simulators (Button, Camera, Chime) and the backend server, coupled with a web management dashboard.

---

## 🏗️ Architecture & Features

### 1. ASP.NET Core Backend (`smart doorbell backend`)
- **Clean Architecture**: Decoupled into `Domain`, `Application`, `Infrastructure`, and `Presentation` layers.
- **MQTT Integration**: Uses `MQTTnet` to subscribe to ring events and control IoT chime devices via Mosquitto.
- **MinIO Object Storage**: Stores visitor snapshot photos (`doorbell-snapshots`) and custom chime audio melodies (`doorbell-melodies`).
- **MinIO Webhook Handler**: Listens for MinIO object creation events (`/api/webhook/minio`) to trigger visitor alert dispatches.
- **Real-Time SignalR Hub**: Pushes live doorbell alerts, visitor snapshots, and session countdowns to connected web clients (`/hubs/doorbell`).
- **WebPush Notifications**: Delivers native browser notifications via VAPID keys regardless of whether the frontend dashboard tab is active.
- **Database & Cache**: Entity Framework Core with SQL Server (`DoorBellDB`) and Redis session cache. Automatic database seeding on startup.

### 2. Frontend Web Dashboard (`smart-doorbell-frontend`)
- **Next.js 15 App Router**: Built with React 19, TypeScript, and Tailwind CSS.
- **Live Alert Overlay**: Displays real-time doorbell press alerts, visitor photos, and a 15-second silence countdown timer.
- **Home & Member Management**: Switch between smart homes, invite family members, and manage roles (Owner / Member).
- **Custom Melodies Manager**: Upload custom `.mp3`/`.wav` audio files to MinIO and set the active main house chime melody.
- **Native Web Push Subscriptions**: One-click registration for browser push notifications using standard NIST P-256 VAPID keys.

### 3. IoT Hardware Simulators (`Simulators/`)
- **Button Simulator (`ButtonSimulator`)**: Simulates physical doorbell presses by publishing MQTT messages to `doorbell/ring`.
- **Camera Simulator (`CameraSimulator`)**: Reads images sequentially from a configurable local folder (`appsettings.json`), uploads snapshots to MinIO on ring trigger, and triggers backend notification dispatches.
- **Chime Simulator (`ChimeSimulator`)**: Subscribes to `doorbell/chime` over MQTT. Downloads and plays the home's main custom melody from MinIO, automatically falling back to a synthesized beep tone if no custom melody is set.

---

## 🛠️ Prerequisites

Ensure the following tools and services are installed and running locally:

- [.NET 9 SDK](https://dotnet.microsoft.com/download/dotnet/9.0)
- [Node.js (v18+)](https://nodejs.org/) & `npm`
- **Mosquitto MQTT Broker** (`localhost:1883`)
- **SQL Server** (`localhost:1433`)
- **MinIO Object Storage** (`localhost:9000` / Console `localhost:9001`)
- **Redis** (`localhost:6379`)

---

## 🚀 Quick Start Guide

### 1. Clone the Repository
```bash
git clone git@github.com:kareemtaha3/smart-doorbell.git
cd smart-doorbell
```

### 2. Configure MinIO Webhook Event (Podman / Docker)
To enable automatic snapshot notification dispatches on MinIO uploads:
```bash
# Enable webhook endpoint in MinIO
podman exec minio mc admin config set local notify_webhook:upload \
  enable=true endpoint="http://host.containers.internal:5147/api/webhook/minio"

# Restart MinIO service
podman exec -t minio mc admin service restart local

# Subscribe webhook to bucket event
podman exec minio mc event add local/doorbell-snapshots arn:minio:sqs::upload:webhook --event put
```

### 3. Run ASP.NET Core Backend
```bash
cd "smart doorbell backend"
dotnet run
```
The API will start at `http://localhost:5147` (Swagger available at `http://localhost:5147/swagger`). The database will automatically seed test users and devices on initial startup.

### 4. Run Next.js Frontend Dashboard
In a new terminal:
```bash
cd smart-doorbell-frontend
npm install
npm run dev
```
Open `http://localhost:3000` in your browser. Click **Enable Web Push** to subscribe to native browser notifications.

### 5. Run Simulators

- **Button Simulator**:
  ```bash
  cd Simulators/ButtonSimulator
  dotnet run
  ```
  Press `[Enter]` in the console to simulate ringing the doorbell.

- **Camera Simulator**:
  Configure `ImagesDirectoryPath` in `Simulators/CameraSimulator/appsettings.json` to point to a folder containing test images, then run:
  ```bash
  cd Simulators/CameraSimulator
  dotnet run
  ```

- **Chime Simulator**:
  ```bash
  cd Simulators/ChimeSimulator
  dotnet run
  ```

---

## 📄 License
MIT License.
