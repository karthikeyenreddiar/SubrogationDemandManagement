# Subrogation Demand Management SaaS

## Project Status: MVP Complete

This repository contains the source code for the Subrogation Demand Management SaaS application.

### 📂 Documentation
- [Implementation Plan](docs/implementation_plan.md) - Detailed architecture and phase breakdown.
- [Task Checklist](docs/tasks.md) - Tracking of completed and pending tasks.

### 🚀 Getting Started

1. **Prerequisites:**
   - .NET 7 SDK
   - SQL Server LocalDB (or other SQL instance)
   - Azure Storage Emulator (Azurite) - Optional for Blob Storage dev
   - Azure Service Bus (or mock/dev setup)

2. **Running the Application:**
   - **API:** `dotnet run --project src/API/SubrogationDemandManagement.API.csproj`
     - Seeds mock data automatically in Development mode.
     - Swagger: `https://localhost:7001/swagger`
   - **UI:** `dotnet run --project src/UI/SubrogationDemandManagement.UI.csproj`
     - URL: `https://localhost:7002`
   - **Functions:** `dotnet run --project src/Functions/SubrogationDemandManagement.Functions.csproj`

3. **Running Tests:**
   - `dotnet test`

### 🏗 Architecture
The solution follows a Clean Architecture approach:
- **src/Domain:** Core entities and interfaces (No dependencies)
- **src/Services:** Business logic, Data access, External service integrations
- **src/API:** REST API endpoints
- **src/Functions:** Background workers
- **src/UI:** Blazor WebAssembly frontend
- **tests/:** Unit and Integration tests

### 💡 Key Features
- **Performance:** Optimized EF Core queries (NoTracking, Compiled Queries).
- **Scalability:** Decoupled background processing via Azure Service Bus.
- **UX:** Responsive Blazor UI with real-time status updates.
