# Email Functionality - Quick Start Guide

## Overview
This guide shows how to use the email sending functionality in the Subrogation Demand Management system.

## Prerequisites

1. **SendGrid Account**: Sign up at https://sendgrid.com
2. **API Key**: Generate an API key from SendGrid dashboard
3. **Configuration**: Add API key to `appsettings.json`

## Configuration

### appsettings.json
```json
{
  "SendGrid": {
    "ApiKey": "SG.your-api-key-here"
  }
}
```

### Azure Function Configuration (local.settings.json)
```json
{
  "IsEncrypted": false,
  "Values": {
    "AzureWebJobsStorage": "UseDevelopmentStorage=true",
    "FUNCTIONS_WORKER_RUNTIME": "dotnet-isolated",
    "ServiceBusConnection": "your-service-bus-connection-string",
    "SendGrid:ApiKey": "SG.your-api-key-here"
  }
}
```

## Usage

### 1. Send Email via API

**Endpoint**: `POST /api/DemandPackages/{id}/send`

**Request Body**:
```json
{
  "recipients": [
    "recipient1@example.com",
    "recipient2@example.com"
  ],
  "ccRecipients": [
    "cc@example.com"
  ],
  "subject": "Subrogation Demand Package",
  "body": "Please find attached the demand package for claim #12345."
}
```

**Response** (202 Accepted):
```json
{
  "message": "Email delivery queued",
  "packageId": "guid-here",
  "communicationId": "guid-here"
}
```

### 2. Check Email Status

Query the `CommunicationLogs` table using the `communicationId`:

```csharp
var log = await _communicationRepository.GetByIdAsync(communicationId);
Console.WriteLine($"Status: {log.Status}");
Console.WriteLine($"Sent At: {log.SentAt}");
Console.WriteLine($"Tracking ID: {log.DeliveryTrackingId}");
```

### 3. Get All Communications for a Package

```csharp
var communications = await _communicationRepository.GetByPackageIdAsync(packageId);
foreach (var comm in communications)
{
    Console.WriteLine($"{comm.CreatedAt}: {comm.Status} - {comm.EmailSubject}");
}
```

## Email Status Flow

```
Queued → Sending → Sent → Delivered
                ↓
              Failed
```

- **Queued**: Email request created, waiting for processing
- **Sending**: Azure Function is processing the email
- **Sent**: Email successfully sent to SendGrid
- **Delivered**: Email delivered to recipient (requires webhook)
- **Failed**: Email sending failed (check ErrorMessage field)

## Testing

### Unit Tests
```bash
dotnet test
```

### Manual Testing (Development)

1. **Without SendGrid API Key** (simulation mode):
   - Remove or empty the `SendGrid:ApiKey` setting
   - Emails will be logged but not actually sent
   - Status will still be updated to "Sent"

2. **With SendGrid API Key**:
   - Configure valid API key
   - Use your own email address as recipient
   - Check your inbox for the email

### Example Test Request (PowerShell)
```powershell
$packageId = "your-package-guid"
$body = @{
    recipients = @("your-email@example.com")
    subject = "Test Demand Package"
    body = "This is a test email from the Subrogation system."
} | ConvertTo-Json

Invoke-RestMethod -Uri "https://localhost:7001/api/DemandPackages/$packageId/send" `
    -Method Post `
    -Body $body `
    -ContentType "application/json"
```

## Troubleshooting

### Email Not Sending

1. **Check SendGrid API Key**:
   ```csharp
   // Look for this log message
   "SendGrid API Key is not configured"
   ```

2. **Check Service Bus Connection**:
   - Ensure Service Bus is running
   - Verify connection string is correct

3. **Check Communication Log**:
   ```sql
   SELECT * FROM CommunicationLogs 
   WHERE Status = 'Failed'
   ORDER BY CreatedAt DESC
   ```

4. **Check Azure Function Logs**:
   ```
   func host start --verbose
   ```

### Common Errors

#### "Package must be generated before sending"
- **Solution**: Call `POST /api/DemandPackages/{id}/generate` first

#### "SendGrid API Key is not configured"
- **Solution**: Add API key to configuration

#### "At least one recipient is required"
- **Solution**: Ensure recipients array is not empty

## Monitoring

### Application Insights Queries

**Failed Emails (Last 24 Hours)**:
```kusto
traces
| where timestamp > ago(24h)
| where customDimensions.Category contains "SendEmail"
| where severityLevel >= 3
| project timestamp, message, customDimensions.ErrorMessage
```

**Email Volume**:
```kusto
customEvents
| where name == "EmailSent"
| summarize Count=count() by bin(timestamp, 1h)
| render timechart
```

**Average Delivery Time**:
```kusto
traces
| where message contains "Email delivery completed"
| extend duration = toreal(customDimensions.Duration)
| summarize avg(duration), max(duration), min(duration) by bin(timestamp, 1h)
```

## Best Practices

1. **Always check package status** before sending
2. **Validate email addresses** on the client side
3. **Use meaningful subject lines** for better tracking
4. **Monitor communication logs** for failed deliveries
5. **Set up SendGrid webhooks** for delivery confirmations
6. **Use Azure Key Vault** for API keys in production
7. **Implement retry logic** for transient failures
8. **Add rate limiting** to prevent abuse

## Advanced Features

### Custom Email Templates

To implement custom templates:

1. Create template in SendGrid
2. Modify `SendGridEmailService` to use template ID
3. Pass template data in request

### Scheduled Emails

To schedule emails:

1. Add `ScheduledFor` field to `EmailDeliveryMessage`
2. Modify Azure Function to check schedule
3. Re-queue if not yet time to send

### Bulk Sending

For bulk operations:

1. Create batch endpoint
2. Queue multiple messages
3. Implement throttling to respect SendGrid limits

## Support

For issues or questions:
- Check logs in Application Insights
- Review SendGrid dashboard for delivery status
- Consult the implementation summary document

## Security Notes

⚠️ **Important**:
- Never commit API keys to source control
- Use Azure Key Vault in production
- Implement proper authentication on API endpoints
- Validate and sanitize all email content
- Respect GDPR and email regulations
- Implement unsubscribe functionality for marketing emails
