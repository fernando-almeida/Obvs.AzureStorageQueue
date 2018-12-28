using System;

namespace Obvs.AzureStorageQueue.Exceptions
{
    public class InvalidCredentialsConfigurationException: Exception
    {
        
        public InvalidCredentialsConfigurationException(string message = "Invalid credentials configuration", Exception innerException = null)
            : base(message, innerException) {
            
        }
    }
}