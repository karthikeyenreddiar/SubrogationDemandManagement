using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using SendGrid;
using SendGrid.Helpers.Mail;

namespace SubrogationDemandManagement.Services.Email;

/// <summary>
/// SendGrid implementation of IEmailService
/// </summary>
public class SendGridEmailService : IEmailService
{
    private readonly ILogger<SendGridEmailService> _logger;
    private readonly string _apiKey;
    private readonly ISendGridClient _client;

    public SendGridEmailService(
        ILogger<SendGridEmailService> logger,
        IConfiguration configuration,
        ISendGridClient? client = null)
    {
        _logger = logger;
        _apiKey = configuration["SendGrid:ApiKey"] ?? string.Empty;
        // Only create client if API key is provided and no client was injected
        _client = client ?? (string.IsNullOrEmpty(_apiKey) ? null! : new SendGridClient(_apiKey));
    }

    public async Task<EmailResponse> SendEmailAsync(EmailRequest request)
    {
        try
        {
            if (string.IsNullOrEmpty(_apiKey))
            {
                _logger.LogWarning("SendGrid API Key is not configured. Email not sent.");
                return new EmailResponse
                {
                    Success = false,
                    ErrorMessage = "SendGrid API Key is not configured",
                    StatusCode = 0
                };
            }

            // Validate request
            if (!request.To.Any())
            {
                throw new ArgumentException("At least one recipient is required", nameof(request));
            }

            // Create SendGrid message
            var from = new EmailAddress(request.From, request.FromName);
            var to = new EmailAddress(request.To.First());
            var subject = request.Subject;
            var plainTextContent = request.PlainTextBody;
            var htmlContent = request.HtmlBody ?? $"<p>{request.PlainTextBody}</p>";

            var msg = MailHelper.CreateSingleEmail(from, to, subject, plainTextContent, htmlContent);

            // Add additional recipients
            if (request.To.Count > 1)
            {
                foreach (var recipient in request.To.Skip(1))
                {
                    msg.AddTo(new EmailAddress(recipient));
                }
            }

            // Add CC recipients
            if (request.Cc?.Any() == true)
            {
                foreach (var cc in request.Cc)
                {
                    msg.AddCc(new EmailAddress(cc));
                }
            }

            // Add BCC recipients
            if (request.Bcc?.Any() == true)
            {
                foreach (var bcc in request.Bcc)
                {
                    msg.AddBcc(new EmailAddress(bcc));
                }
            }

            // Add attachments
            if (request.Attachments?.Any() == true)
            {
                foreach (var attachment in request.Attachments)
                {
                    var fileContent = Convert.ToBase64String(attachment.Content);
                    msg.AddAttachment(attachment.FileName, fileContent, attachment.ContentType);
                }
            }

            // Send email
            var response = await _client.SendEmailAsync(msg);

            if (response.IsSuccessStatusCode)
            {
                var messageId = response.Headers.GetValues("X-Message-Id").FirstOrDefault();
                _logger.LogInformation(
                    "Email sent successfully to {RecipientCount} recipients. MessageId: {MessageId}",
                    request.To.Count, messageId);

                return new EmailResponse
                {
                    Success = true,
                    MessageId = messageId,
                    StatusCode = (int)response.StatusCode
                };
            }
            else
            {
                var body = await response.Body.ReadAsStringAsync();
                _logger.LogError(
                    "Failed to send email. Status: {Status}, Body: {Body}",
                    response.StatusCode, body);

                return new EmailResponse
                {
                    Success = false,
                    ErrorMessage = $"SendGrid returned status {response.StatusCode}: {body}",
                    StatusCode = (int)response.StatusCode
                };
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Exception occurred while sending email");
            return new EmailResponse
            {
                Success = false,
                ErrorMessage = ex.Message,
                StatusCode = 0
            };
        }
    }
}
