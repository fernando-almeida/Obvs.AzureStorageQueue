using System;
using System.Reflection;

using Microsoft.WindowsAzure.Storage.Queue;

using Moq;

using Obvs.AzureStorageQueue.Configuration;
using Obvs.Serialization;
using Obvs.Types;

using Xunit;

namespace Obvs.AzureStorageQueue.Tests.Configuration {

    /// <summary>
    /// Test Azure storage queue service endpoint provider
    /// </summary>
    public class AzureStorageQueueServiceEndpointProviderTest : IClassFixture<CloudQueueFixture> {
        private static readonly string TEST_SERVICE_NAME = "testservice";


        /// <summary>
        /// Cloud queue client fixture
        /// </summary>
        private readonly CloudQueueFixture _cloudQueueClientFixture;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="cloudQueueClientFixture">Cloud queue client fixture</param>
        public AzureStorageQueueServiceEndpointProviderTest(CloudQueueFixture cloudQueueClientFixture) {
            _cloudQueueClientFixture = cloudQueueClientFixture;
        }

        [Fact]
        public void Test_CreateServiceEndpointProvider_FailsNullServiceName() {
            string serviceName = null;
            Assert.Throws(
                typeof(ArgumentNullException),
                () => new AzureStorageQueueServiceEndpointProvider<IMessage, IMessage, TestCommand, TestEvent, TestRequest, TestResponse>(
                    serviceName,
                    null,
                    null,
                    null,
                    null));
        }

        [Fact]
        public void Test_CreationOfServiceEndpoint_FailsInvalidClient() {
            Assert.Throws(
                typeof(ArgumentNullException),
                () => new AzureStorageQueueServiceEndpointProvider<IMessage, IMessage, TestCommand, TestEvent, TestRequest, TestResponse>(
                    TEST_SERVICE_NAME,
                    null,
                    null as CloudQueueClient,
                    null,
                    null));
        }

        [Fact]
        public void Test_CreationOfServiceEndpoint_InvalidSerializer() {
            IMessageSerializer serializer = null;
            Assert.Throws(
                typeof(ArgumentNullException),
                () => new AzureStorageQueueServiceEndpointProvider<IMessage, IMessage, TestCommand, TestEvent, TestRequest, TestResponse>(
                    TEST_SERVICE_NAME,
                    null,
                    _cloudQueueClientFixture.Client,
                    serializer,
                    null));
        }

        [Fact]
        public void Test_CreationOfServiceEndpoint_InvalidDeserializerFactory() {
            var serializer = Mock.Of<IMessageSerializer>();
            IMessageDeserializerFactory deserializerFactory = null;
            Assert.Throws(
                typeof(ArgumentNullException),
                () => new AzureStorageQueueServiceEndpointProvider<ITestServiceMessage, TestMessage, TestCommand, TestEvent, TestRequest, TestResponse>(
                    TEST_SERVICE_NAME,
                    null,
                    _cloudQueueClientFixture.Client,
                    serializer,
                    deserializerFactory));
        }

        [Fact]
        private AzureStorageQueueServiceEndpointProvider<ITestServiceMessage, IMessage, TestCommand, TestEvent, TestRequest, TestResponse> CreateValidStorageQueueServiceProvider() {
            var serializer = Mock.Of<IMessageSerializer>();
            var messageDeserializerFactory = new TestMessageDeserializerFactory();
            var serviceProvider = new AzureStorageQueueServiceEndpointProvider<ITestServiceMessage, IMessage, TestCommand, TestEvent, TestRequest, TestResponse>(
                TEST_SERVICE_NAME,
                null,
                _cloudQueueClientFixture.Client,
                serializer,
                messageDeserializerFactory);
            return serviceProvider;
        }

        [Fact]
        public void Test_CreationOfServiceEndpoint_Success() {
            var serviceProvider = CreateValidStorageQueueServiceProvider();
            var endpoint = serviceProvider.CreateEndpoint();
            Assert.NotNull(endpoint);
            var endpointClient = serviceProvider.CreateEndpointClient();
            Assert.NotNull(endpointClient);
        }

    }
}