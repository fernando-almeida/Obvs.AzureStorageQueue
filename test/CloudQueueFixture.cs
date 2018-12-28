using System;
using Microsoft.WindowsAzure.Storage.Auth;
using Microsoft.WindowsAzure.Storage.Queue;

using Moq;

using Obvs.AzureStorageQueue.Tests.Extensions;

namespace Obvs.AzureStorageQueue.Tests
{
    public class CloudQueueFixture
    {
        public static readonly string STORAGE_ACCOUNT_NAME = Environment.GetEnvironmentVariable("STORAGE_ACCOUNT_NAME") ?? "testStorageAccount";
        public static readonly string QUEUE_NAME = Environment.GetEnvironmentVariable("STORAGE_QUEUE_NAME") ?? "testQueueName";
        public static readonly string STORAGE_ACCOUNT_KEY =
            string.IsNullOrEmpty(Environment.GetEnvironmentVariable("STORAGE_ACCOUNT_KEY"))
            ? "storageAccountKey".ToBase64String()
            : Environment.GetEnvironmentVariable("STORAGE_ACCOUNT_KEY").ToBase64String();

        public static readonly string DEFAULT_QUEUE_NAME = Environment.GetEnvironmentVariable("DEFAULT_QUEUE_NAME")
            ?? "testQueueName";

        /// <summary>
        /// Instance of the cloud queue client to use for testing
        /// </summary>
        /// <value></value>
        public CloudQueueClient Client {get; private set;}

        /// <summary>
        /// Constructor
        /// </summary>
        public CloudQueueFixture() {
            var uri = GetQueueUri();
            var storageCredentials = GetStorageCredentials();
            Client = new CloudQueueClient(uri, storageCredentials);
        }


        public static Uri GetQueueUri() => Utils.BuildQueueUri(STORAGE_ACCOUNT_NAME, QUEUE_NAME);

        public static StorageCredentials GetStorageCredentials() => new StorageCredentials(STORAGE_ACCOUNT_NAME, STORAGE_ACCOUNT_KEY);

        /// <summary>
        /// Get a reference to the queue with a given name
        /// </summary>
        /// <param name="queueName">Name of the queue</param>
        /// <returns></returns>
        public CloudQueue GetQueue(string queueName) => Client.GetQueueReference(queueName);

        /// <summary>
        /// Get an instance of the default queue
        /// </summary>
        /// <returns></returns>
        public CloudQueue DefaultQueue => GetQueue(DEFAULT_QUEUE_NAME);

        public Mock<CloudQueueClient> GetClientMock() {
            return new Mock<CloudQueueClient>(GetQueueUri(), GetStorageCredentials());
        }
        public Mock<CloudQueue> GetQueueMock() {
            return new Mock<CloudQueue>(GetQueueUri(), GetStorageCredentials());
        }

    }
}