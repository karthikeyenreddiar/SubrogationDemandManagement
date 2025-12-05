using Xunit;
using Moq;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;
using SubrogationDemandManagement.Services.Email;
using SendGrid;
using SendGrid.Helpers.Mail;
using System.Net;

namespace SubrogationDemandManagement.Tests.Services;

public class SendGridEmailServiceTests
{
    private readonly Mock<ILogger<SendGridEmailService>> _mockLogger;
    private readonly Mock<IConfiguration> _mockConfiguration;

    public SendGridEmailServiceTests()
    {
        _mockLogger = new Mock<ILogger<SendGridEmailService>>();
        _mockConfiguration = new Mock<IConfiguration>();
    }

    [Fact]
    public async Task SendEmailAsync_WithNoApiKey_ReturnsFailure()
    {
        // Arrange
        _mockConfiguration.Setup(c => c["SendGrid:ApiKey"]).Returns(string.Empty);

        var service = new SendGridEmailService(_mockLogger.Object, _mockConfiguration.Object);

        var request = new EmailRequest
        {
            From = "test@example.com",
            FromName = "Test Sender",
            To = new List<string> { "recipient@example.com" },
            Subject = "Test Subject",
            PlainTextBody = "Test body content"
        };

        // Act
        var result = await service.SendEmailAsync(request);

        // Assert
        Assert.False(result.Success);
        Assert.Equal("SendGrid API Key is not configured", result.ErrorMessage);
    }

    [Fact]
    public async Task SendEmailAsync_WithNoRecipients_ReturnsFailure()
    {
        // Arrange
        _mockConfiguration.Setup(c => c["SendGrid:ApiKey"]).Returns("test-api-key");

        var service = new SendGridEmailService(_mockLogger.Object, _mockConfiguration.Object);

        var request = new EmailRequest
        {
            From = "test@example.com",
            FromName = "Test Sender",
            To = new List<string>(), // Empty recipients
            Subject = "Test Subject",
            PlainTextBody = "Test body content"
        };

        // Act
        var result = await service.SendEmailAsync(request);

        // Assert
        Assert.False(result.Success);
        Assert.Contains("At least one recipient is required", result.ErrorMessage);
    }

    [Fact]
    public void EmailRequest_CanBeCreatedWithValidData()
    {
        // Arrange & Act
        var request = new EmailRequest
        {
            From = "test@example.com",
            FromName = "Test Sender",
            To = new List<string> { "recipient@example.com" },
            Subject = "Test Subject",
            PlainTextBody = "Test body content",
            HtmlBody = "<p>Test body content</p>"
        };

        // Assert
        Assert.Equal("test@example.com", request.From);
        Assert.Equal("Test Sender", request.FromName);
        Assert.Single(request.To);
        Assert.Equal("Test Subject", request.Subject);
        Assert.Equal("Test body content", request.PlainTextBody);
        Assert.Equal("<p>Test body content</p>", request.HtmlBody);
    }

    [Fact]
    public void EmailAttachment_CanBeCreatedWithValidData()
    {
        // Arrange & Act
        var content = System.Text.Encoding.UTF8.GetBytes("Test PDF content");
        var attachment = new EmailAttachment
        {
            FileName = "test.pdf",
            Content = content,
            ContentType = "application/pdf"
        };

        // Assert
        Assert.Equal("test.pdf", attachment.FileName);
        Assert.Equal(content, attachment.Content);
        Assert.Equal("application/pdf", attachment.ContentType);
    }

    [Fact]
    public void EmailResponse_Success_HasCorrectProperties()
    {
        // Arrange & Act
        var response = new EmailResponse
        {
            Success = true,
            MessageId = "msg-123",
            StatusCode = 200
        };

        // Assert
        Assert.True(response.Success);
        Assert.Equal("msg-123", response.MessageId);
        Assert.Equal(200, response.StatusCode);
        Assert.Null(response.ErrorMessage);
    }

    [Fact]
    public void EmailResponse_Failure_HasCorrectProperties()
    {
        // Arrange & Act
        var response = new EmailResponse
        {
            Success = false,
            ErrorMessage = "Send failed",
            StatusCode = 400
        };

        // Assert
        Assert.False(response.Success);
        Assert.Equal("Send failed", response.ErrorMessage);
        Assert.Equal(400, response.StatusCode);
        Assert.Null(response.MessageId);
    }
}
