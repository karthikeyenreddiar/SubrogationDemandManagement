using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using SubrogationDemandManagement.Services.Messaging.Messages;
using SubrogationDemandManagement.Services.Storage;
using System.Text.Json;

namespace SubrogationDemandManagement.Functions;

public class SendEmailFunction
{
    private readonly ILogger<SendEmailFunction> _logger;
    private readonly BlobStorageService _blobStorage;

    public SendEmailFunction(
        ILogger<SendEmailFunction> logger,
        BlobStorageService blobStorage)
    {
        _logger = logger;
        _blobStorage = blobStorage;
    }

    /// <summary>
    /// Azure Function triggered by Service Bus queue to send demand emails
    /// </summary>
    [Function("SendEmail")]
    public async Task Run(
        [ServiceBusTrigger("email-delivery", Connection = "ServiceBusConnection")] 
        string messageBody)
    {
        var startTime = DateTime.UtcNow;
        
        _logger.LogInformation("Email delivery started. Message: {Message}", messageBody);
        
        try
        {
            // Deserialize message
            var message = JsonSerializer.Deserialize<EmailDeliveryMessage>(messageBody);
            if (message == null)
            {
                _logger.LogError("Failed to deserialize email delivery message");
                throw new InvalidOperationException("Invalid message format");
            }

            _logger.LogInformation(
                "Processing email delivery for package {PackageId} to {RecipientCount} recipients",
                message.PackageId, message.Recipients.Count);
            
            // Download PDF attachment from Blob Storage
            Stream? pdfStream = null;
            if (!string.IsNullOrEmpty(message.PdfBlobPath))
            {
                pdfStream = await _blobStorage.DownloadPackageAsync(message.PdfBlobPath);
                _logger.LogInformation("Downloaded PDF attachment from {BlobPath}", message.PdfBlobPath);
            }

            // TODO: Implement actual email sending logic
            // 1. Use Azure Communication Services to send email
            // 2. Attach PDF from blob storage
            // 3. Track delivery status
            // 4. Update CommunicationLog in database
            
            // For now, simulate email sending
            await Task.Delay(300); // Simulate work
            
            _logger.LogInformation(
                "Email sent successfully to {Recipients}",
                string.Join(", ", message.Recipients));
            
            // TODO: Update communication log with delivery status
            // await _communicationRepository.UpdateStatusAsync(
            //     message.CommunicationId, 
            //     CommunicationStatus.Sent);
            
            var duration = DateTime.UtcNow - startTime;
            
            _logger.LogInformation(
                "Email delivery completed for package {PackageId} in {Duration}ms",
                message.PackageId, duration.TotalMilliseconds);
            
            // Cleanup
            pdfStream?.Dispose();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Email delivery failed for message: {Message}", messageBody);
            throw; // Will trigger Service Bus retry
        }
    }
}
