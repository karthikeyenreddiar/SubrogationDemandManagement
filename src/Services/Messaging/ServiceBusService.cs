using Azure.Messaging.ServiceBus;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace SubrogationDemandManagement.Services.Messaging;

/// <summary>
/// Service Bus client for sending messages to queues
/// </summary>
public class ServiceBusService : IAsyncDisposable
{
    private readonly ServiceBusClient _client;
    private readonly ILogger<ServiceBusService> _logger;
    private readonly Dictionary<string, ServiceBusSender> _senders = new();

    public ServiceBusService(IConfiguration configuration, ILogger<ServiceBusService> logger)
    {
        _logger = logger;
        var connectionString = configuration["ServiceBus:ConnectionString"];
        
        if (string.IsNullOrEmpty(connectionString))
        {
            _logger.LogWarning("Service Bus connection string not configured. Messages will not be sent.");
            _client = null!;
        }
        else
        {
            _client = new ServiceBusClient(connectionString);
        }
    }

    /// <summary>
    /// Send message to a queue
    /// </summary>
    public async Task SendMessageAsync<T>(string queueName, T message) where T : class
    {
        if (_client == null)
        {
            _logger.LogWarning("Service Bus not configured. Skipping message send to queue {QueueName}", queueName);
            return;
        }

        try
        {
            var sender = GetOrCreateSender(queueName);
            var messageBody = JsonSerializer.Serialize(message);
            var serviceBusMessage = new ServiceBusMessage(messageBody)
            {
                ContentType = "application/json",
                MessageId = Guid.NewGuid().ToString()
            };

            await sender.SendMessageAsync(serviceBusMessage);
            
            _logger.LogInformation("Message sent to queue {QueueName}, MessageId: {MessageId}", 
                queueName, serviceBusMessage.MessageId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send message to queue {QueueName}", queueName);
            throw;
        }
    }

    /// <summary>
    /// Send message with custom properties
    /// </summary>
    public async Task SendMessageAsync<T>(
        string queueName, 
        T message, 
        Dictionary<string, object> properties) where T : class
    {
        if (_client == null)
        {
            _logger.LogWarning("Service Bus not configured. Skipping message send to queue {QueueName}", queueName);
            return;
        }

        try
        {
            var sender = GetOrCreateSender(queueName);
            var messageBody = JsonSerializer.Serialize(message);
            var serviceBusMessage = new ServiceBusMessage(messageBody)
            {
                ContentType = "application/json",
                MessageId = Guid.NewGuid().ToString()
            };

            // Add custom properties
            foreach (var prop in properties)
            {
                serviceBusMessage.ApplicationProperties.Add(prop.Key, prop.Value);
            }

            await sender.SendMessageAsync(serviceBusMessage);
            
            _logger.LogInformation("Message sent to queue {QueueName}, MessageId: {MessageId}", 
                queueName, serviceBusMessage.MessageId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send message to queue {QueueName}", queueName);
            throw;
        }
    }

    /// <summary>
    /// Schedule message for future delivery
    /// </summary>
    public async Task ScheduleMessageAsync<T>(
        string queueName, 
        T message, 
        DateTimeOffset scheduledEnqueueTime) where T : class
    {
        if (_client == null)
        {
            _logger.LogWarning("Service Bus not configured. Skipping scheduled message to queue {QueueName}", queueName);
            return;
        }

        try
        {
            var sender = GetOrCreateSender(queueName);
            var messageBody = JsonSerializer.Serialize(message);
            var serviceBusMessage = new ServiceBusMessage(messageBody)
            {
                ContentType = "application/json",
                MessageId = Guid.NewGuid().ToString()
            };

            await sender.ScheduleMessageAsync(serviceBusMessage, scheduledEnqueueTime);
            
            _logger.LogInformation("Message scheduled for queue {QueueName} at {ScheduledTime}", 
                queueName, scheduledEnqueueTime);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to schedule message to queue {QueueName}", queueName);
            throw;
        }
    }

    private ServiceBusSender GetOrCreateSender(string queueName)
    {
        if (!_senders.ContainsKey(queueName))
        {
            _senders[queueName] = _client.CreateSender(queueName);
        }
        return _senders[queueName];
    }

    public async ValueTask DisposeAsync()
    {
        foreach (var sender in _senders.Values)
        {
            await sender.DisposeAsync();
        }
        
        if (_client != null)
        {
            await _client.DisposeAsync();
        }
    }
}
