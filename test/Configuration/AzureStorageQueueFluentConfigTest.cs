using System;
using System.Linq;
using System.Reflection;
using System.Collections.Generic;
using System.Text;

using Microsoft.WindowsAzure.Storage.Queue;

using Obvs.AzureStorageQueue.Exceptions;
using Obvs.AzureStorageQueue.Configuration;
using Obvs.AzureStorageQueue.Tests.Extensions;
using Obvs.Types;
using Obvs.Serialization;
using Moq;

using Xunit;

namespace Obvs.AzureStorageQueue.Tests.Configuration {

    /// <summary>
    /// Test Azure storage queue fluent configuration
    /// </summary>
    public class AzureStorageQueueFluentConfigTest {

        private IMessageSerializer _messageSerializer;
        private IMessageDeserializerFactory _messageDeserializerFactory;

        public AzureStorageQueueFluentConfigTest() {
            _messageSerializer = new Mock<IMessageSerializer>().Object;
            // TODO Figure out how to mock the generic Create method
            var TestCommandDeserializerMock = new Mock<IMessageDeserializer<TestCommand1>>();
            TestCommandDeserializerMock.SetReturnsDefault<TestCommand1>(new TestCommand1());
            TestCommandDeserializerMock.Setup(x => x.GetTypeName()).Returns(typeof(TestCommand1).Name);
            var messageDeserializerFactoryMock = new Mock<IMessageDeserializerFactory>();
            messageDeserializerFactoryMock.Setup(x => x.Create<TestCommand1, TestMessage>(
                    It.Is<Func<Assembly, bool>>(null),
                    It.Is<Func<Type, bool>>(null)))
                .Returns(
                    Obvs.Configuration.MessageTypes.Get<TestCommand1, TestMessage>(null, null)
                        .Select(type => type.MakeGenericType(type))
                        .Select(genericDeserializer => Activator.CreateInstance(genericDeserializer) as IMessageDeserializer<TestCommand1>));
            _messageDeserializerFactory = new TestMessageDeserializerFactory();
        }

        [Fact]
        public void Test_FluentConfigAsClient_Succeeds() {
            var defaultQueueRequestOptions = new QueueRequestOptions {

            };
            var queueRequestOptionsCmd1 = new QueueRequestOptions {

            };
            var serviceBusClient = ServiceBus.Configure()
                .WithAzureStorageQueueEndpoint<ITestServiceMessage>()
                .Named("service-name")
                .WithAccountCredentials("accountName", "accountKey".ToBase64String())
                .WithCommandQueue("queue1", queueRequestOptionsCmd1)
                .WithEventQueue("events", queueRequestOptionsCmd1)
                .SerializedWith(_messageSerializer, _messageDeserializerFactory)
                .AsClient();
        }

        [Fact]
        public void Test_ServiceBusClientCreation_ThrowsInvalidServiceName() {
            Action action = () =>  ServiceBus.Configure()
                .WithAzureStorageQueueEndpoint<TestMessage>()
                .Named(null);
            Assert.Throws(typeof(InvalidServiceNameException), action);
        }

        [Fact]
        public void Test_ServiceBusClientCreation_ThrowsInvalidAccountCredentials() {
            Action action = () => ServiceBus.Configure()
                .WithAzureStorageQueueEndpoint<TestMessage>()
                .Named("service-name")
                .WithAccountCredentials(null, "accountKey");
            Assert.Throws(typeof(InvalidCredentialsConfigurationException), action);
                
        }

        [Fact]
        public void Test_ServiceBusClientCreation_ThrowsInvalidConnectionStringCredentials() {
            Action action = () => ServiceBus.Configure()
                .WithAzureStorageQueueEndpoint<TestMessage>()
                .Named("service-name")
                .WithConnectionString("invalidconnstring");
            Assert.Throws(typeof(InvalidCredentialsConfigurationException), action);
        }

        [Fact]
        public void Test_ServiceBusClientCreation_ThrowsInvalidStorageCredentials() {
            Action action = () => ServiceBus.Configure()
                .WithAzureStorageQueueEndpoint<TestMessage>()
                .Named("service-name")
                .WithStorageCredentials(null);
            Assert.Throws(typeof(InvalidCredentialsConfigurationException), action);
        }

        [Fact]
        public void Test_ServiceBusClientCreation_ThrowsInvalidStorageAccount() {
            Action action = () => ServiceBus.Configure()
                .WithAzureStorageQueueEndpoint<TestMessage>()
                .Named("service-name")
                .WithStorageAccount(null);
            Assert.Throws(typeof(InvalidCredentialsConfigurationException), action);
        }

        [Fact]
        public void Test_ServiceBusClientCreation_ThrowsInvalidCloudClient() {
            Action action = () => ServiceBus.Configure()
                .WithAzureStorageQueueEndpoint<TestMessage>()
                .Named("service-name")
                .WithCloudClient(null);
            Assert.Throws(typeof(InvalidCredentialsConfigurationException), action);
        }
    }
}