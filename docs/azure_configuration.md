# Azure Configuration Guide

This document outlines the step-by-step configuration required in the Azure Portal to support the Subrogation Demand Management application.

## 1. Azure AD B2C Setup

### Step 1.1: Create B2C Tenant
1. Log in to the [Azure Portal](https://portal.azure.com).
2. Search for "Azure AD B2C".
3. Click **Create** and follow the wizard to create a new Azure AD B2C Tenant.
   - **Organization name**: e.g., "Subrogation SaaS"
   - **Initial domain name**: e.g., "subrogationsaas" (This will be `<your-tenant-name>.onmicrosoft.com`)
4. Link the B2C Tenant to your main Azure subscription.

### Step 1.2: Register the API Application
1. Switch to your new B2C Tenant directory.
2. Go to **App registrations** > **New registration**.
3. **Name**: `SubrogationAPI`
4. **Supported account types**: "Accounts in any identity provider or organizational directory (for authenticating users with user flows)".
5. **Redirect URI**: Select "Single-page application (SPA)" and add:
   - `https://localhost:7002/authentication/login-callback` (Local Dev)
   - `https://jwt.ms` (For testing tokens)
6. Click **Register**.
7. Copy the **Application (client) ID**. You will need this for `appsettings.json`.

### Step 1.3: Configure User Flows
1. Go to **User flows** in the B2C menu.
2. Click **New user flow**.
3. Select **Sign up and sign in**.
4. **Version**: Recommended.
5. **Name**: `B2C_1_susi`
6. **Identity providers**: Select "Email signup".
7. **User attributes and token claims**:
   - Click **Show more**.
   - Select attributes to collect: `Given Name`, `Surname`, `Email Address`.
   - Select claims to return: `Given Name`, `Surname`, `Email Addresses`, `User's Object ID`.
8. Click **Create**.

### Step 1.4: Add Custom TenantId Attribute
To support multi-tenancy, we need a custom attribute to store the user's Tenant ID.
1. Go to **User attributes**.
2. Click **Add**.
3. **Name**: `TenantId` (It will be saved as `extension_TenantId`).
4. **Data Type**: String.
5. Click **Create**.
6. Go back to your User Flow (`B2C_1_susi`).
7. Click **User attributes** and check `TenantId`.
8. Click **Application claims** and check `TenantId` (so it appears in the token).

### Step 1.5: Update Application Configuration
Update `src/API/appsettings.json` with your values:
```json
"AzureAdB2C": {
  "Instance": "https://<your-tenant-name>.b2clogin.com",
  "ClientId": "<Application (client) ID from Step 1.2>",
  "Domain": "<your-tenant-name>.onmicrosoft.com",
  "SignedOutCallbackPath": "/signout/B2C_1_susi",
  "SignUpSignInPolicyId": "B2C_1_susi"
}
```

## 2. Azure Service Bus Setup

### Step 2.1: Create Namespace
1. Search for "Service Bus" in the Azure Portal.
2. Click **Create**.
3. **Subscription**: Your subscription.
4. **Resource Group**: Create new (e.g., `rg-subrogation-dev`).
5. **Namespace name**: Unique name (e.g., `sb-subrogation-dev`).
6. **Pricing tier**: Standard (Basic does not support Topics if we need them later, but Basic is fine for Queues).
7. Click **Review + create** > **Create**.

### Step 2.2: Create Queues
1. Go to the created Service Bus Namespace.
2. Under **Entities**, click **Queues**.
3. Create a queue named `pdf-generation`.
4. Create a queue named `email-delivery`.

### Step 2.3: Get Connection String
1. Go to **Shared access policies**.
2. Click **RootManageSharedAccessKey**.
3. Copy the **Primary Connection String**.
4. Update `src/API/appsettings.json` and `src/Functions/local.settings.json`:
```json
"ServiceBus": {
  "ConnectionString": "<Your-Connection-String>"
}
```

## 3. Azure Blob Storage Setup

### Step 3.1: Create Storage Account
1. Search for "Storage accounts".
2. Click **Create**.
3. **Resource Group**: Same as above.
4. **Storage account name**: Unique name (e.g., `stsubrogationdev`).
5. **Redundancy**: LRS (Locally-redundant storage) is cheapest for dev.
6. Click **Review + create** > **Create**.

### Step 3.2: Create Containers
1. Go to the created Storage Account.
2. Under **Data storage**, click **Containers**.
3. Create a container named `documents` (Private access).
4. Create a container named `packages` (Private access).

### Step 3.3: Get Connection String
1. Go to **Access keys**.
2. Copy **Connection string** for key1.
3. Update `src/Functions/local.settings.json`:
```json
"BlobStorage": {
  "ConnectionString": "<Your-Connection-String>"
}
```
(Note: The API currently uses the BlobStorageService but defaults to development storage if not configured. You can add a `BlobStorage` section to `appsettings.json` if needed).

## 4. Application Insights (Optional)
1. Search for "Application Insights".
2. Create new resource.
3. Copy **Connection String**.
4. Update `appsettings.json` and `local.settings.json`.
