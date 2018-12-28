using System;

namespace Obvs.AzureStorageQueue.Exceptions
{
    public class InvalidQueueConfigurationException: Exception
    {
        
        public InvalidQueueConfigurationException(string message = "Invalid queue configuration", Exception innerException = null)
            : base(message, innerException) {
            
        }
    }
}