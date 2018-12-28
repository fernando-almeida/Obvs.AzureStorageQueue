using System;

namespace Obvs.AzureStorageQueue
{
    public static class Utils
    {
        /// <summary>
        /// Build Azure Storage queue URL
        /// </summary>
        /// <param name="storageAccountName">Name of the storage account</param>
        /// <param name="queueName">Name of the queue</param>
        /// <returns>Uri for the queue </returns>
        public static Uri BuildQueueUri(string storageAccountName, string queueName)
            =>  new Uri($"http://{storageAccountName}.queue.core.windows.net/{queueName}");
    }
}
