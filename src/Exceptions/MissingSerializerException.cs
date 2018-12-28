using System;

namespace Obvs.AzureStorageQueue.Exceptions
{
    public class InvalidMessageSerializerException: Exception
    {
        public InvalidMessageSerializerException(string message = "Invalid message serializer", Exception innerException = null): base(message, innerException) {
            
        }
        
    }
}