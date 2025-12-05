# Project Tasks

## ✅ Completed Tasks

### Infrastructure
- [x] Create solution structure
- [x] Configure .gitignore
- [x] Set up README.md

### Domain & Data
- [x] Define domain entities (SubrogationCase, DemandPackage, etc.)
- [x] Configure EF Core DbContext
- [x] Implement Entity Configurations with indexes
- [x] Create Repositories (SubrogationCase, DemandPackage)
- [x] Create Database Migrations
- [x] Implement Data Seeder for development

### API
- [x] Create SubrogationCasesController
- [x] Create DemandPackagesController
- [x] Configure Swagger/OpenAPI
- [x] Implement Service Bus integration
- [x] Implement Blob Storage service

### Azure Functions
- [x] Create GeneratePDFFunction
- [x] Create SendEmailFunction
- [x] Configure Service Bus triggers

### UI (Blazor)
- [x] Setup Blazor WASM project
- [x] Create API Client
- [x] Implement Case List page
- [x] Implement Case Details page
- [x] Implement Navigation and Layout

### Testing
- [x] Setup Test Project (xUnit)
- [x] Implement Repository Tests
- [x] Implement Controller Tests

## 🚀 Upcoming Tasks

### Security
- [x] Configure Azure AD B2C (Code configuration complete, requires Portal setup)
- [x] Implement JWT Authentication in API
- [x] Add Authorization policies (Tenant isolation)

### Functional Implementation
- [x] Implement real PDF generation (QuestPDF/PdfSharp)
- [x] Implement real Email sending (SendGrid/ACS)
- [x] Add file upload capability in UI
- [ ] Implement "Parent System Sync" logic

### DevOps
- [ ] Create Bicep templates for Azure resources
- [ ] Setup GitHub Actions for Build & Test
- [ ] Setup GitHub Actions for Deployment

### UI Enhancements
- [ ] Add error handling and toast notifications
- [ ] Add loading skeletons/states
- [ ] Implement pagination controls in UI
- [ ] Add search and filter capabilities
