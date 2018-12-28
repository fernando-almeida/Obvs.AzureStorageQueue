using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reactive;
using System.Reactive.Concurrency;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Reactive.Threading.Tasks;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Queue;

using Obvs;
using Obvs.Serialization;

namespace Obvs.AzureStorageQueue {

    public class MessageDeserializationException<TMessage> : Exception {

        public TMessage BusMessage { get; private set; }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="busMessage">Bus message</param>
        /// <param name="message">Custom exception message</param>
        /// <param name="innerException">Inner exception</param>
        public MessageDeserializationException(
            TMessage busMessage,
            string message = null,
            Exception innerException = null
        ) : base(
            message == null ?
            "Could not deserialize message" :
            string.Join(Environment.NewLine, "Could not deserialize message", message),
            innerException) {
            BusMessage = busMessage;
        }
    }

    /// <summary>
    /// Message source
    /// </summary>
    /// <typeparam name="TMessage">Type of message</typeparam>
    public class MessageSource<TMessage> : IMessageSource<TMessage> where TMessage : class {
        private static readonly int MAX_MESSAGES_TO_RETRIEVE = 32;
        private static readonly int MIN_MESSAGES_TO_RETRIEVE = 1;
        private readonly CloudQueue _cloudQueue;

        private readonly IEnumerable<IMessageDeserializer<TMessage>> _messageDeserializers;

        private readonly IDictionary<Type, QueueRequestOptions> _queueRequestOptionsByMessageType;
        private readonly int _numMessagesToRetrieve;

        private readonly TimeSpan? _visibilityTimeout;
        private readonly OperationContext _operationContext;

        private readonly TimeSpan _pollingInterval = TimeSpan.FromSeconds(5);

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="cloudQueue">Queue</param>
        /// <param name="messageDeserializers">Message deserializer</param>
        /// <param name="queueRequestOptionsByMessageType">Queue request options</param>
        /// <param name="maxMessagesToRetrieve">Number of messages to retrieve</param>
        /// <param name="operationContext">Operation context</param>
        /// <param name="visibilityTimeout">Visibility timeout</param>
        /// <param name="pollingInterval">Message polling interval</param>
        public MessageSource(
            CloudQueue cloudQueue,
            IEnumerable<IMessageDeserializer<TMessage>> messageDeserializers,
            IDictionary<Type, QueueRequestOptions> queueRequestOptionsByMessageType = null,
            int maxMessagesToRetrieve = 1,
            OperationContext operationContext = null,
            TimeSpan? visibilityTimeout = null,
            TimeSpan? pollingInterval = null
        ) {
            if (cloudQueue == null) {
                throw new ArgumentNullException(nameof(cloudQueue));
            }
            if (messageDeserializers == null) {
                throw new ArgumentNullException(nameof(messageDeserializers));
            }
            if (!messageDeserializers.Any()) {
                throw new ArgumentException("No message deserializers provided");
            }
            if (maxMessagesToRetrieve < MIN_MESSAGES_TO_RETRIEVE || maxMessagesToRetrieve > MAX_MESSAGES_TO_RETRIEVE) {
                throw new ArgumentOutOfRangeException(
                    nameof(maxMessagesToRetrieve),
                    $"Messages to retrieve must be between {MIN_MESSAGES_TO_RETRIEVE} and {MAX_MESSAGES_TO_RETRIEVE}");
            }
            _messageDeserializers = messageDeserializers;
            _queueRequestOptionsByMessageType = queueRequestOptionsByMessageType;
            _numMessagesToRetrieve = maxMessagesToRetrieve;
            _cloudQueue = cloudQueue;
            _operationContext = operationContext;
            _visibilityTimeout = visibilityTimeout;
            if (pollingInterval.HasValue) {
                _pollingInterval = pollingInterval.Value;
            }
        }

        /// <summary>
        /// Get the deserializer for the given message
        /// </summary>
        /// <param name="message">Message</param>
        /// <returns>Message deserializer implementation to use</returns>
        private IMessageDeserializer<TMessage> GetDeserializer(CloudQueueMessage message) => _messageDeserializers.FirstOrDefault();

        /// <summary>
        /// Deserialize the message content
        /// </summary>
        /// <param name="message">Message to transform</param>
        /// <returns></returns>
        private TMessage DeserializeMessage(CloudQueueMessage message) {
            var messageDeserializer = GetDeserializer(message);
            if (messageDeserializer == null) {
                throw new NullReferenceException(nameof(messageDeserializer));
            }
            // Deserialize using the first successful serializer
            using(var stream = new MemoryStream(message.AsBytes)) {
                var deserializedMessage = messageDeserializer.Deserialize(stream);
                if (deserializedMessage == null) {
                    throw new MessageDeserializationException<CloudQueueMessage>(
                        message,
                        $"Could not deserialize message {messageDeserializer.GetTypeName()}");
                }

                return deserializedMessage;
            }
        }

        /// <summary>
        /// Get the list of messages currently available in the queue
        /// </summary>
        /// <returns>List of messages available in the queue</returns>
        private List<CloudQueueMessage> GetMessages() {
            QueueRequestOptions queueRequestOptions = null;
            _queueRequestOptionsByMessageType?.TryGetValue(typeof(TMessage), out queueRequestOptions);
            // bool queueCreated = false;
            // queueCreated = _cloudQueue.CreateIfNotExists(queueRequestOptions, _operationContext);
            return _cloudQueue.GetMessages(
                _numMessagesToRetrieve,
                _visibilityTimeout,
                queueRequestOptions,
                _operationContext).ToList();
        }

        /// <summary>
        /// Build an observable of the messages in the queue with an optional scheduler
        /// </summary>
        /// <param name="scheduler">Scheduler implementation used for polling</param>
        /// <returns>Observable of messages available in the queue</returns>
        public IObservable<TMessage> GetMessagesObservable(IScheduler scheduler = null) {
            scheduler = scheduler ?? Scheduler.Default;
            return Observable.Interval(_pollingInterval, scheduler)
                .SelectMany(_ => GetMessages())
                .Do(message => _cloudQueue.DeleteMessage(message))
                .Select(DeserializeMessage);
        }

        /// <inheritdoc />
        public IObservable<TMessage> Messages => GetMessagesObservable();

        /// <inheritdoc />
        public void Dispose() {
            
        }
    }
}