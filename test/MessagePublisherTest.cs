using Xunit;

using Moq;

using Obvs;
using Obvs.Serialization;
using Obvs.Types;

using Microsoft.WindowsAzure.Storage.Queue;

using System;
using System.Threading.Tasks;
using System.Collections.Generic;

// using System.Reactive.Concurrency;

namespace Obvs.AzureStorageQueue.Tests
{
    public class MessagePublisherTest: IClassFixture<CloudQueueFixture>
    {
        private readonly CloudQueueFixture _cloudQueueClientFixture;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="cloudQueueClientFixture"></param>
        public MessagePublisherTest(CloudQueueFixture cloudQueueClientFixture) {
            _cloudQueueClientFixture = cloudQueueClientFixture;
        }

        [Fact]
        public void Test_CreateMessagePublisher_InvalidCloudQueueNull() {
            CloudQueue cloudQueue = null;
            IMessageSerializer messageSerializer = null;
            Func<byte[], CloudQueueMessage> cloudQueueMessageFn = null;
            Assert.Throws(
                typeof(ArgumentNullException),
                () => new MessagePublisher<ITestServiceMessage>(
                    cloudQueue,
                    messageSerializer,
                    cloudQueueMessageFn));
        }

        [Fact]
        public void Test_CreateMessagePublisher_InvalidMessageSerializerNull() {
            CloudQueue cloudQueue = _cloudQueueClientFixture.DefaultQueue;
            IMessageSerializer messageSerializer = null;
            Func<byte[], CloudQueueMessage> cloudQueueMessageFn = null;
            Assert.Throws(
                typeof(ArgumentNullException),
                () => new MessagePublisher<ITestServiceMessage>(
                    cloudQueue,
                    messageSerializer,
                    cloudQueueMessageFn));
        }


        [Fact]
        public async Task Test_MessagePublisher_Succeeds()
        {
            var queueMock = new Mock<CloudQueue>(
                    CloudQueueFixture.GetQueueUri(),
                    CloudQueueFixture.GetStorageCredentials());
            queueMock.Setup(x => x.AddMessageAsync(It.IsAny<CloudQueueMessage>()))
                    .Returns(Task.CompletedTask);
            queueMock.CallBase = false;
            var messageSerializer = Mock.Of<IMessageSerializer>();
            Func<byte[], CloudQueueMessage> cloudQueueMessageFn = null;
            var messagePublisher = new MessagePublisher<ITestServiceMessage>(
                queueMock.Object,
                messageSerializer,
                cloudQueueMessageFn);
            var testCommand = new TestCommand1();
            await messagePublisher.PublishAsync(testCommand);
            queueMock.Verify(x => x.AddMessageAsync(It.IsAny<CloudQueueMessage>()), Times.Once());
        }

    }
}