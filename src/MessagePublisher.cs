using System;
using System.Threading.Tasks;

using Microsoft.WindowsAzure.Storage.Queue;

using Obvs;
using Obvs.Serialization;

namespace Obvs.AzureStorageQueue {

    /// <summary>
    /// Message publisher
    /// </summary>
    /// <typeparam name="TMessage">Type of message to publish</typeparam>
    public class MessagePublisher<TMessage> : IMessagePublisher<TMessage> where TMessage : class {

        /// <summary>
        /// Cloud queue to publish messages to
        /// </summary>
        private readonly CloudQueue _cloudQueue;

        /// <summary>
        /// Cloud queue message creator
        /// </summary>
        private readonly Func<byte[], CloudQueueMessage> _cloudQueueMessageBuilderFn = content => new CloudQueueMessage(content);

        /// <summary>
        /// Message content serializer
        /// </summary>
        private readonly IMessageSerializer _messageSerializer;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="cloudQueue">Queue</param>
        /// <param name="messageSerializer">Message serializer</param>
        /// <param name="cloudQueueMessageBuilderFn">Cloud queue message builder functor</param>
        public MessagePublisher(
            CloudQueue cloudQueue,
            IMessageSerializer messageSerializer,
            Func<byte[], CloudQueueMessage> cloudQueueMessageBuilderFn = null
        ) {
            if (cloudQueue == null) {
                throw new ArgumentNullException(nameof(cloudQueue));
            }
            if (messageSerializer == null) { 
                throw new ArgumentNullException(nameof(messageSerializer));
            }
            _cloudQueueMessageBuilderFn = cloudQueueMessageBuilderFn ?? (content => new CloudQueueMessage(content));
            _cloudQueue = cloudQueue;
            _messageSerializer = messageSerializer;
        }

        /// <inheritdoc />
        public void Dispose() {

        }

        /// <inheritdoc />
        public Task PublishAsync(TMessage message) {
            var serializedMessage = _messageSerializer.Serialize(message);
            var cloudQueueMessage = _cloudQueueMessageBuilderFn(serializedMessage);
            return _cloudQueue.AddMessageAsync(cloudQueueMessage);
        }
    }
}