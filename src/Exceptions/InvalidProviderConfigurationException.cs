using System;

namespace Obvs.AzureStorageQueue.Exceptions
{
    public class ProviderConfigurationException: Exception
    {
        
        public ProviderConfigurationException(string message = "Invalid provider configuration", Exception innerException = null)
            : base(message, innerException) {
            
        }
    }
}