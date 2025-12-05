using Xunit;
using Moq;
using Microsoft.Extensions.Logging;
using SubrogationDemandManagement.Functions;
using SubrogationDemandManagement.Services.Email;
using SubrogationDemandManagement.Services.Data.Repositories;
using SubrogationDemandManagement.Services.Messaging.Messages;
using SubrogationDemandManagement.Domain.Models;
using System.Text.Json;

namespace SubrogationDemandManagement.Tests.Functions;

public class SendEmailFunctionTests
{
    [Fact]
    public void EmailDeliveryMessage_CanBeDeserialized()
    {
        // Arrange
        var message = new EmailDeliveryMessage
        {
            CommunicationId = Guid.NewGuid(),
            PackageId = Guid.NewGuid(),
            TenantId = Guid.NewGuid(),
            Recipients = new List<string> { "test@example.com" },
            Subject = "Test Subject",
            Body = "Test Body",
            PdfBlobPath = "packages/test.pdf",
            RequestedAt = DateTime.UtcNow
        };

        var json = JsonSerializer.Serialize(message);

        // Act
        var deserialized = JsonSerializer.Deserialize<EmailDeliveryMessage>(json);

        // Assert
        Assert.NotNull(deserialized);
        Assert.Equal(message.CommunicationId, deserialized.CommunicationId);
        Assert.Equal(message.PackageId, deserialized.PackageId);
        Assert.Equal(message.Subject, deserialized.Subject);
        Assert.Equal(message.Body, deserialized.Body);
        Assert.Single(deserialized.Recipients);
    }

    [Fact]
    public void EmailDeliveryMessage_WithMultipleRecipients_CanBeDeserialized()
    {
        // Arrange
        var message = new EmailDeliveryMessage
        {
            CommunicationId = Guid.NewGuid(),
            PackageId = Guid.NewGuid(),
            TenantId = Guid.NewGuid(),
            Recipients = new List<string> { "test1@example.com", "test2@example.com" },
            CcRecipients = new List<string> { "cc@example.com" },
            Subject = "Test Subject",
            Body = "Test Body",
            RequestedAt = DateTime.UtcNow
        };

        var json = JsonSerializer.Serialize(message);

        // Act
        var deserialized = JsonSerializer.Deserialize<EmailDeliveryMessage>(json);

        // Assert
        Assert.NotNull(deserialized);
        Assert.Equal(2, deserialized.Recipients.Count);
        Assert.Single(deserialized.CcRecipients);
    }
}
