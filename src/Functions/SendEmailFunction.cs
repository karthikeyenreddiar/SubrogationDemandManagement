using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using SubrogationDemandManagement.Services.Messaging.Messages;
using SubrogationDemandManagement.Services.Storage;
using SubrogationDemandManagement.Services.Data.Repositories;
using SubrogationDemandManagement.Domain.Models;
using SubrogationDemandManagement.Services.Email;
using System.Text.Json;

namespace SubrogationDemandManagement.Functions;

public class SendEmailFunction
{
    private readonly ILogger<SendEmailFunction> _logger;
    private readonly BlobStorageService _blobStorage;
    private readonly CommunicationLogRepository _communicationRepository;
    private readonly IEmailService _emailService;

    public SendEmailFunction(
        ILogger<SendEmailFunction> logger,
        BlobStorageService blobStorage,
        CommunicationLogRepository communicationRepository,
        IEmailService emailService)
    {
        _logger = logger;
        _blobStorage = blobStorage;
        _communicationRepository = communicationRepository;
        _emailService = emailService;
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
            
            // Update status to Sending
            await _communicationRepository.UpdateStatusAsync(message.CommunicationId, CommunicationStatus.Sending);
            
            // Download PDF attachment from Blob Storage
            List<EmailAttachment>? attachments = null;
            
            if (!string.IsNullOrEmpty(message.PdfBlobPath))
            {
                var pdfStream = await _blobStorage.DownloadPackageAsync(message.PdfBlobPath);
                var pdfFileName = Path.GetFileName(message.PdfBlobPath);
                
                using var ms = new MemoryStream();
                await pdfStream.CopyToAsync(ms);
                var fileBytes = ms.ToArray();
                
                attachments = new List<EmailAttachment>
                {
                    new EmailAttachment
                    {
                        FileName = pdfFileName,
                        Content = fileBytes,
                        ContentType = "application/pdf"
                    }
                };
                
                pdfStream.Dispose();
                _logger.LogInformation("Downloaded PDF attachment from {BlobPath}", message.PdfBlobPath);
            }

            // Send Email using IEmailService
            var emailRequest = new EmailRequest
            {
                From = "noreply@subrogationsaas.com",
                FromName = "Subrogation SaaS",
                To = message.Recipients,
                Cc = message.CcRecipients,
                Subject = message.Subject,
                PlainTextBody = message.Body,
                HtmlBody = $"<p>{message.Body}</p>",
                Attachments = attachments
            };

            var response = await _emailService.SendEmailAsync(emailRequest);
            
            if (response.Success)
            {
                _logger.LogInformation(
                    "Email sent successfully to {RecipientCount} recipients. MessageId: {MessageId}",
                    message.Recipients.Count, response.MessageId);
                
                // Update communication log
                await _communicationRepository.UpdateStatusAsync(
                    message.CommunicationId, 
                    CommunicationStatus.Sent,
                    response.MessageId);
            }
            else
            {
                _logger.LogError("Failed to send email. Error: {Error}", response.ErrorMessage);
                await _communicationRepository.UpdateErrorAsync(
                    message.CommunicationId, 
                    response.ErrorMessage ?? "Unknown error");
                throw new Exception($"Email send failed: {response.ErrorMessage}");
            }
            
            var duration = DateTime.UtcNow - startTime;
            
            _logger.LogInformation(
                "Email delivery completed for package {PackageId} in {Duration}ms",
                message.PackageId, duration.TotalMilliseconds);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Email delivery failed for message: {Message}", messageBody);
            throw; // Will trigger Service Bus retry
        }
    }
}
