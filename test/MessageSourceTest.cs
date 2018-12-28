using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reactive;
using System.Reactive.Linq;
using System.Reactive.Threading.Tasks;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.Reactive.Testing;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Queue;

using Moq;

using Obvs.AzureStorageQueue;
using Obvs.Serialization;

using Xunit;

namespace Obvs.AzureStorageQueue.Tests {

    /// <summary>
    /// Message source test class
    /// </summary>
    public class MessageSourceTest
        : ReactiveTest, IClassFixture<CloudQueueFixture> {
            private readonly CloudQueueFixture _cloudQueueClientFixture;

            public MessageSourceTest(CloudQueueFixture cloudQueueClientFixture) {
                _cloudQueueClientFixture = cloudQueueClientFixture;
            }

            [Fact]
            public void Test_MessageSourceCreation_InvalidNullQueue() {
                Assert.Throws(typeof(ArgumentNullException), () => new MessageSource<ITestServiceMessage>(
                    null as CloudQueue,
                    null as IEnumerable<IMessageDeserializer<ITestServiceMessage>>,
                    null as IDictionary<Type, QueueRequestOptions>,
                    1));
            }

            [Fact]
            public void Test_MessageSourceCreation_InvalidNullDeserializers() {
                var queue = _cloudQueueClientFixture.DefaultQueue;
                Assert.Throws(typeof(ArgumentNullException), () => new MessageSource<ITestServiceMessage>(
                    queue,
                    null as IEnumerable<IMessageDeserializer<ITestServiceMessage>>,
                    null as IDictionary<Type, QueueRequestOptions>,
                    1));
            }

            [Fact]
            public void Test_MessageSourceCreation_EmptyDeserializers() {
                var queue = _cloudQueueClientFixture.DefaultQueue;
                var messageDeserializers = new List<IMessageDeserializer<ITestServiceMessage>>();
                IDictionary<Type, QueueRequestOptions> queueRequestOptions = null;
                Assert.Throws(typeof(ArgumentException), () => new MessageSource<ITestServiceMessage>(
                    queue,
                    messageDeserializers,
                    queueRequestOptions,
                    1));
            }

            [Fact]
            public void Test_MessageSourceCreation_InvalidNumMessagesToRetrieve() {
                var queue = _cloudQueueClientFixture.DefaultQueue;
                var messageDeserializers = new List<IMessageDeserializer<ITestServiceMessage>>();
                messageDeserializers.Add(Mock.Of<IMessageDeserializer<ITestServiceMessage>>());
                IDictionary<Type, QueueRequestOptions> queueRequestOptions = null;
                Assert.Throws(typeof(ArgumentOutOfRangeException), () => new MessageSource<ITestServiceMessage>(
                    queue,
                    messageDeserializers,
                    queueRequestOptions, -3));
            }

            [Fact]
            public void Test_MessageSourceCreation_Success() {
                var queueMock = _cloudQueueClientFixture.GetQueueMock();
                var messages = new List<CloudQueueMessage> {
                    new CloudQueueMessage("Test1"),
                    new CloudQueueMessage("Test2"),
                    new CloudQueueMessage("Test3")
                };
                queueMock.Setup(x => x.GetMessagesAsync(
                    It.IsAny<int>(),
                    It.IsAny<TimeSpan?>(),
                    It.IsAny<QueueRequestOptions>(),
                    It.IsAny<OperationContext>(),
                    It.IsAny<CancellationToken>()
                )).Returns(Task.FromResult(messages as IEnumerable<CloudQueueMessage>));
                queueMock.Setup(x => x.GetMessages(
                    It.IsAny<int>(),
                    It.IsAny<TimeSpan?>(),
                    It.IsAny<QueueRequestOptions>(),
                    It.IsAny<OperationContext>()
                )).Returns((int numMessagesToRetrieve, TimeSpan? visibilityTimeout, QueueRequestOptions options, OperationContext context) => messages.Take(numMessagesToRetrieve));
                queueMock.Setup(x => x.DeleteMessage(
                    It.IsAny<CloudQueueMessage>(),
                    It.IsAny<QueueRequestOptions>(),
                    It.IsAny<OperationContext>()
                )).Callback((CloudQueueMessage message, QueueRequestOptions options, OperationContext context) => messages.Remove(message) );
                queueMock.CallBase = false;
                var messageDeserializers = new List<IMessageDeserializer<ITestServiceMessage>>();
                var deserializerMock = new Mock<IMessageDeserializer<ITestServiceMessage>>();
                var irrelevantTestCommand = new TestCommand1();
                deserializerMock.Setup(x => x.Deserialize(It.IsAny<Stream>())).Returns(irrelevantTestCommand);
                messageDeserializers.Add(deserializerMock.Object);
                IDictionary<Type, QueueRequestOptions> queueRequestOptions = null;
                var maxMessagesToRetrieve = 2;
                var pollingIntervalTicks = 1000;
                var messageSource = new MessageSource<ITestServiceMessage>(
                    queueMock.Object,
                    messageDeserializers,
                    queueRequestOptions,
                    maxMessagesToRetrieve: maxMessagesToRetrieve,
                    pollingInterval: TimeSpan.FromTicks(pollingIntervalTicks));
                var messageActionFnMock = new Mock<Action<ITestServiceMessage>>();
                var rxTestScheduler = new TestScheduler();
                var numPolls = 2;
                (long created, long subscribed, long disposed) = (0, pollingIntervalTicks / 2, pollingIntervalTicks * numPolls);
                var observer = rxTestScheduler.Start(
                    () => messageSource.GetMessagesObservable(rxTestScheduler),
                    created: created,
                    subscribed: subscribed,
                    disposed: disposed
                );

                var expectedMessages = Enumerable.Range(1, numPolls)
                    .Where(pollIndex => subscribed + pollIndex * pollingIntervalTicks <= disposed)
                    .SelectMany(pollIndex => Enumerable.Range(1, maxMessagesToRetrieve)
                                            .Select(messageIndex => new { PollIndex = pollIndex, MessageIndex = messageIndex }))
                    .Select(tuple => new Recorded<Notification<ITestServiceMessage>>(
                        subscribed + tuple.PollIndex * pollingIntervalTicks,
                        Notification.CreateOnNext<ITestServiceMessage>(irrelevantTestCommand)))
                    .ToList();
                observer.Messages.AssertEqual(expectedMessages);
            }

        }
}