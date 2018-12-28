using System;

namespace Obvs.AzureStorageQueue.Exceptions
{
    public class InvalidServiceNameException: Exception
    {
        public InvalidServiceNameException(string message = "Invalid service name", Exception innerException = null): base(message, innerException) {
            
        }
        
    }
}