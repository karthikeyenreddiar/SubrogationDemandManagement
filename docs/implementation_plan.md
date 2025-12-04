# Subrogation Demand Management SaaS - Implementation Plan

## 1. Project Overview
The Subrogation Demand Management SaaS is a hybrid application designed to streamline the creation, management, and delivery of subrogation demand packages. It leverages a .NET 7 microservices-inspired architecture with a Blazor WebAssembly frontend and Azure Functions for background processing.

## 2. Architecture
- **Frontend:** Blazor WebAssembly (.NET 7)
- **API:** ASP.NET Core Web API (.NET 7)
- **Background Processing:** Azure Functions (.NET 7 Isolated Worker)
- **Database:** SQL Server (LocalDB for dev) with Entity Framework Core
- **Messaging:** Azure Service Bus
- **Storage:** Azure Blob Storage

## 3. Implementation Phases

### Phase 1: Foundation & Core Infrastructure (Completed)
- [x] Solution setup with Clean Architecture (Domain, Services, API, Functions, UI)
- [x] Domain modeling (SubrogationCase, DemandPackage, etc.)
- [x] Entity Framework Core setup with performance optimizations
- [x] Database migrations and seeding
- [x] Repository pattern implementation

### Phase 2: Backend Services & Integration (Completed)
- [x] API Controller implementation
- [x] Azure Service Bus integration for decoupled messaging
- [x] Azure Blob Storage service for document management
- [x] Azure Functions for PDF generation and Email delivery (Skeleton/Mock)
- [x] Unit Testing infrastructure (xUnit, Moq, InMemory DB)

### Phase 3: Frontend Development (Completed MVP)
- [x] Blazor WebAssembly project setup
- [x] API Client service
- [x] Case Management UI (List, Details, Create)
- [x] Demand Package management UI
- [x] Basic styling and layout

### Phase 4: Advanced Features & Polish (Next Steps)
- [ ] **Authentication & Authorization:** Integrate Azure AD B2C
- [ ] **PDF Generation Logic:** Implement actual PDF merging and manipulation using a library like PdfSharp or QuestPDF.
- [ ] **Email Service:** Integrate SendGrid or Azure Communication Services.
- [ ] **Tenant Management:** Admin UI for managing tenants.
- [ ] **Dashboard:** Analytics and reporting dashboard.
- [ ] **CI/CD:** GitHub Actions pipelines for deployment.

## 4. Technical Standards
- **Performance:** No-tracking queries by default, compiled queries for hot paths, async/await everywhere.
- **Testing:** High coverage for core business logic and repositories.
- **Code Quality:** Adherence to SOLID principles, dependency injection, and clean code practices.
