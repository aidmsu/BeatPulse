﻿using BeatPulse.Core;
using Microsoft.Azure.EventHubs;
using Microsoft.Extensions.Logging;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace BeatPulse.AzureServiceBus
{
    public class AzureEventHubLiveness : IBeatPulseLiveness
    {
        private readonly string _connectionString;
        private readonly string _eventHubName;
        private readonly ILogger<AzureEventHubLiveness> _logger;

        public AzureEventHubLiveness(string connectionString, string eventHubName, ILogger<AzureEventHubLiveness> logger = null)
        {
            _connectionString = connectionString ?? throw new ArgumentNullException(nameof(connectionString));
            _eventHubName = eventHubName ?? throw new ArgumentNullException(nameof(eventHubName));
            _logger = logger;
        }

        public async Task<(string, bool)> IsHealthy(LivenessExecutionContext context, CancellationToken cancellationToken = default)
        {
            try
            {
                _logger?.LogInformation($"{nameof(AzureEventHubLiveness)} is checking the Azure Event Hub.");

                var connectionStringBuilder = new EventHubsConnectionStringBuilder(_connectionString)
                {
                    EntityPath = _eventHubName
                };

                var eventHubClient = EventHubClient
                    .CreateFromConnectionString(connectionStringBuilder.ToString());

                await eventHubClient.GetRuntimeInformationAsync();

                _logger?.LogInformation($"The {nameof(AzureEventHubLiveness)} check success.");

                return (BeatPulseKeys.BEATPULSE_HEALTHCHECK_DEFAULT_OK_MESSAGE, true);
            }
            catch (Exception ex)
            {
                _logger?.LogWarning($"The {nameof(AzureEventHubLiveness)} check fail for {_connectionString} with the exception {ex.ToString()}.");

                var message = !context.IsDevelopment ? string.Format(BeatPulseKeys.BEATPULSE_HEALTHCHECK_DEFAULT_ERROR_MESSAGE, context.Name)
                    : $"Exception {ex.GetType().Name} with message ('{ex.Message}')";

                return (message, false);
            }
        }
    }
}
