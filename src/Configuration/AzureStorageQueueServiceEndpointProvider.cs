using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;

using Obvs.Configuration;
using Obvs.Serialization;

using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Queue;

namespace Obvs.AzureStorageQueue.Configuration {

    /// <summary>
    /// Azure storage queue endpoint provider
    /// </summary>
    /// <typeparam name="TServiceMessage"></typeparam>
    /// <typeparam name="TMessage"></typeparam>
    /// <typeparam name="TCommand"></typeparam>
    /// <typeparam name="TEvent"></typeparam>
    /// <typeparam name="TRequest"></typeparam>
    /// <typeparam name="TResponse"></typeparam>
    public class AzureStorageQueueServiceEndpointProvider<TServiceMessage, TMessage, TCommand, TEvent, TRequest, TResponse> : ServiceEndpointProviderBase<TMessage, TCommand, TEvent, TRequest, TResponse>
        where TMessage : class
    where TServiceMessage : class
    where TCommand : class, TMessage
    where TEvent : class, TMessage
    where TRequest : class, TMessage
    where TResponse : class, TMessage {
        private static readonly string DEFAULT_EVENTS_QUEUE_SUFFIX = "events";
        private static readonly string DEFAULT_REQUESTS_QUEUE_SUFFIX = "requests";
        private static readonly string DEFAULT_RESPONSES_QUEUE_SUFFIX = "responses";
        private static readonly string DEFAULT_COMMANDS_QUEUE_SUFFIX = "commands";

        /// <summary>
        /// Name of the service
        /// </summary>
        private readonly string _serviceName;

        /// <summary>
        /// Message serializer implementation
        /// </summary>
        private readonly IMessageSerializer _serializer;

        /// <summary>
        /// Message deserializer factory
        /// </summary>
        private readonly IMessageDeserializerFactory _deserializerFactory;

        /// <summary>
        /// Cloud queue client
        /// </summary>
        private readonly CloudQueueClient _cloudQueueClient;

        /// <summary>
        /// Queue names by message type
        /// </summary>
        private readonly IDictionary<Type, string> _queueNamesByMessageType;

        /// <summary>
        /// Maximum number of messages to receive
        /// </summary>
        private readonly int _maxMessagesToDequeue;

        private readonly IDictionary<Type, QueueRequestOptions> _queueRequestOptionsByMessageType;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="serviceName">Service name</param>
        /// <param name="queueNamesByMessageType">Queue names by message type</param>
        /// <param name="cloudQueueClient">Cloud queue client</param>
        /// <param name="serializer">Serializer</param>
        /// <param name="deserializerFactory">Deserializer factory</param>
        /// <param name="queueRequestOptionsByMessageType">Queue request options</param>
        /// <param name="maxMessagesToDequeue">Queue request options</param>
        public AzureStorageQueueServiceEndpointProvider(
            string serviceName,
            IDictionary<Type, string> queueNamesByMessageType,
            CloudQueueClient cloudQueueClient,
            IMessageSerializer serializer,
            IMessageDeserializerFactory deserializerFactory,
            IDictionary<Type, QueueRequestOptions> queueRequestOptionsByMessageType = null,
            int maxMessagesToDequeue = 1
        ) : base(serviceName) {
            if (string.IsNullOrEmpty(serviceName)) {
                throw new ArgumentNullException(nameof(serviceName));
            }
            if (cloudQueueClient == null) {
                throw new ArgumentNullException(nameof(cloudQueueClient));
            }
            if (serializer == null) {
                throw new ArgumentNullException(nameof(serializer));
            }
            if (deserializerFactory == null) {
                throw new ArgumentNullException(nameof(deserializerFactory));
            }
            _serviceName = serviceName;
            _cloudQueueClient = cloudQueueClient;
            _queueNamesByMessageType = queueNamesByMessageType;
            _serializer = serializer;
            _deserializerFactory = deserializerFactory;
            _queueRequestOptionsByMessageType = queueRequestOptionsByMessageType;
            _maxMessagesToDequeue = maxMessagesToDequeue;
        }

        private string GetQueueName<T>(string serviceName, string suffix) {
            string queueName;
            if (_queueNamesByMessageType == null || !_queueNamesByMessageType.TryGetValue(typeof(T), out queueName)) {
                queueName = $"{serviceName}-{suffix}";
            }
            NameValidator.ValidateQueueName(queueName);
            return queueName;
        }

        /// <inheritdoc />
        public override IServiceEndpoint<TMessage, TCommand, TEvent, TRequest, TResponse> CreateEndpoint() {
            var endpoint = new ServiceEndpoint<TMessage, TCommand, TEvent, TRequest, TResponse>(
                    CreateSource<TRequest>(_cloudQueueClient, GetQueueName<TRequest>(_serviceName, DEFAULT_REQUESTS_QUEUE_SUFFIX)),
                    CreateSource<TCommand>(_cloudQueueClient, GetQueueName<TCommand>(_serviceName, DEFAULT_COMMANDS_QUEUE_SUFFIX)),
                    CreatePublisher<TEvent>(_cloudQueueClient, GetQueueName<TEvent>(_serviceName, DEFAULT_EVENTS_QUEUE_SUFFIX)),
                    CreatePublisher<TResponse>(_cloudQueueClient, GetQueueName<TResponse>(_serviceName, DEFAULT_RESPONSES_QUEUE_SUFFIX)),
                    typeof(TServiceMessage));
            return endpoint;
        }

        /// <inheritdoc />
        private IMessageSource<TSourceMessage> CreateSource<TSourceMessage>(
            CloudQueueClient cloudQueueClient,
            string messageCategory
        ) where TSourceMessage : class, TMessage {
            var sourceMessageType = typeof(TSourceMessage);
            var queueName = GetQueueName<TSourceMessage>(_serviceName, DEFAULT_REQUESTS_QUEUE_SUFFIX);
            var cloudQueue = cloudQueueClient.GetQueueReference(queueName);
            var deserializers = _deserializerFactory.Create<TSourceMessage, TServiceMessage>().ToList();
            return new MessageSource<TSourceMessage>(
                cloudQueue,
                deserializers,
                _queueRequestOptionsByMessageType,
                _maxMessagesToDequeue);
        }

        /// <inheritdoc />
        private IMessagePublisher<T> CreatePublisher<T>(CloudQueueClient cloudQueueClient, string destination) where T : class, TMessage {
            var cloudQueue = cloudQueueClient.GetQueueReference(destination);
            var messagePublisher = new MessagePublisher<T>(cloudQueue, _serializer);
            return messagePublisher;
        }

        /// <inheritdoc />
        public override IServiceEndpointClient<TMessage, TCommand, TEvent, TRequest, TResponse> CreateEndpointClient() {
            var serviceEndpointClient = new ServiceEndpointClient<TMessage, TCommand, TEvent, TRequest, TResponse>(
                    CreateSource<TEvent>(_cloudQueueClient, GetQueueName<TEvent>(_serviceName, DEFAULT_EVENTS_QUEUE_SUFFIX)),
                    CreateSource<TResponse>(_cloudQueueClient, GetQueueName<TResponse>(_serviceName, DEFAULT_RESPONSES_QUEUE_SUFFIX)),
                    CreatePublisher<TRequest>(_cloudQueueClient, GetQueueName<TRequest>(_serviceName, DEFAULT_REQUESTS_QUEUE_SUFFIX)),
                    CreatePublisher<TCommand>(_cloudQueueClient, GetQueueName<TCommand>(_serviceName, DEFAULT_COMMANDS_QUEUE_SUFFIX)),
                    typeof(TServiceMessage));
            return serviceEndpointClient;
        }

        /// <summary>
        /// Get the name of the queue to userfor a given source message type
        /// </summary>
        /// <typeparam name="TSourceMessage"></typeparam>
        /// <returns></returns>
        private string GetQueueName<TSourceMessage>() {
            var sourceMessageType = typeof(TSourceMessage);
            string queueName;
            if (_queueNamesByMessageType == null || !_queueNamesByMessageType.TryGetValue(sourceMessageType, out queueName)) {
                throw new InvalidOperationException($"Queue name not found for type {sourceMessageType.AssemblyQualifiedName}");
            }
            return queueName;
        }

    }
}