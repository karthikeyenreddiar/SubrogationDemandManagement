# 🛠️ Deployment Verification & Setup Checklist

This document tracks the configuration items that need to be updated with real values before the application can be successfully deployed and run.

## 1. GitHub Repository Secrets
These secrets must be added to your GitHub Repository under **Settings > Secrets and variables > Actions**.

| Secret Name | Description | How to Obtain |
|-------------|-------------|---------------|
| `AZURE_CLIENT_ID` | The App ID of the Service Principal for deployment | Run `az ad sp create-for-rbac --name "github-action-sp" --role contributor --scopes /subscriptions/{subscription-id} --sdk-auth`. Use `clientId` from output. |
| `AZURE_TENANT_ID` | The Tenant ID of your Azure Active Directory | Same command as above. Use `tenantId` from output. |
| `AZURE_SUBSCRIPTION_ID` | Your Azure Subscription ID | Run `az account show --query id -o tsv` |
| `SQL_ADMIN_PASSWORD` | Strong password for the Azure SQL Server SA account | Generate a strong complex password (min 12 chars, mixed case, numbers, special chars). |

**Note**: For OIDC (passwordless) authentication (recommended), follow [Configure OpenID Connect in Azure](https://learn.microsoft.com/en-us/azure/developer/github/connect-from-azure?tabs=azure-portal%2Clinux#create-an-azure-active-directory-application-and-service-principal).

## 2. Infrastructure Configuration (`infra/main.bicep`)
Update the Azure AD B2C settings in the `infra/main.bicep` file to ensure the deployed API has the correct configuration.

- [ ] **Open File**: `infra/main.bicep`
- [ ] **Find Section**: `module api` -> `appSettings`
- [ ] **Update Values**:

```bicep
{
  name: 'AzureAdB2C:Instance'
  value: 'https://<your-tenant-name>.b2clogin.com' // Update <your-tenant-name>
}
{
  name: 'AzureAdB2C:ClientId'
  value: '<your-b2c-app-client-id>' // Update with actual B2C App Registration Client ID
}
{
  name: 'AzureAdB2C:Domain'
  value: '<your-tenant-name>.onmicrosoft.com' // Update <your-tenant-name>
}
```

**Where to find these**:
1. Go to **Azure Portal > Azure AD B2C**.
2. **Domain**: Listed on the Overview page.
3. **App Registration**: Go to App Registrations > Select your App > Copy **Application (client) ID**.

## 3. Local Development Configuration (`src/API/appsettings.json`)
Update the local settings to run the API on your machine.

- [ ] **Open File**: `src/API/appsettings.json`
- [ ] **Update Section**: `AzureAdB2C`

```json
"AzureAdB2C": {
    "Instance": "https://<your-tenant-name>.b2clogin.com",
    "ClientId": "<your-client-id>",
    "Domain": "<your-tenant-name>.onmicrosoft.com",
    "SignedOutCallbackPath": "/signout/B2C_1_susi",
    "SignUpSignInPolicyId": "B2C_1_susi"
}
```

- [ ] **Update Connection Strings** (Optional if using localdb, required if using Cloud SQL dev DB)
- [ ] **Update SendGrid/Email** (If you have a key):
  - Add a `"SendGrid"` section or similar if utilizing the `IEmailService` locally (Currently managed via `SendEmailFunction` in Azure).

## 4. Federated Credentials (for GitHub Actions OIDC)
If using the OIDC method for GitHub Actions (as configured in `deploy.yml` with `permissions: id-token: write`), you must configure the Federated Credential in Azure.

1. Go to your **App Registration** (Service Principal) in Azure AD.
2. Select **Certificates & secrets** > **Federated credentials**.
3. Click **Add credential**.
4. Select **GitHub Actions deploying Azure resources**.
5. Enter:
   - **Organization**: `karth` (your GitHub username)
   - **Repository**: `SubrogationDemandManagement`
   - **Entity type**: `Branch`
   - **GitHub branch name**: `main`
6. Click **Add**.

## 5. SendGrid / Email Service Setup
The `SendEmailFunction` requires email sending capabilities.

- [ ] Create a SendGrid account (or other provider).
- [ ] Create a specific API Key.
- [ ] Add the API Key to the **Function App Configuration** in Azure (once deployed):
  - Key: `SendGridApiKey` (or match whatever your `EmailService` expects)
  - Value: `SG.xxxx...`
