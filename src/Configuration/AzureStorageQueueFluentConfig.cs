using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Auth;
using Microsoft.WindowsAzure.Storage.Queue;

using Obvs.Configuration;
using Obvs.Serialization;
using Obvs.AzureStorageQueue.Exceptions;

namespace Obvs.AzureStorageQueue.Configuration {

    /*
     * 1x Service name ICanSpecifyAzureStorageQueueServiceName
     * 1x Credentials ICanSpecifyAzureStorageQueueCredentials
     * 1x Default queue name ICanSpecifyAzureStorageQueueDefaultName
     * Nx Queue names per message type ICanSpecifyAzureStorageQueueName
     * 1x Default request options ICanSpecifyAzureStorageQueueDefaultRequestOptions
     * Nx Request options per message type ICanSpecifyAzureStorageQueueRequestOptions
     * 1x Serialization config ICanSpecifySerializationConfig
     * 1x Create service endpoint 
     */

    /// <summary>
    /// Configurable service name
    /// </summary>
    /// <typeparam name="TMessage"></typeparam>
    /// <typeparam name="TCommand"></typeparam>
    /// <typeparam name="TEvent"></typeparam>
    /// <typeparam name="TRequest"></typeparam>
    /// <typeparam name="TResponse"></typeparam>
    public interface ICanSpecifyAzureStorageQueueServiceName<TMessage, TCommand, TEvent, TRequest, TResponse>
        where TMessage : class
    where TCommand : class, TMessage
    where TEvent : class, TMessage
    where TRequest : class, TMessage
    where TResponse : class, TMessage {

        /// <summary>
        /// Set the name of the service
        /// </summary>
        /// <param name="serviceName">Name of the service</param>
        /// <returns>Credentials configuration interface</returns>
        ICanSpecifyAzureStorageQueueCredentials<TMessage, TCommand, TEvent, TRequest, TResponse> Named(string serviceName);
    }


    public interface ICanSpecifyAzureStorageQueueDefaultMessageConfig<TMessage, TCommand, TEvent, TRequest, TResponse>
        where TMessage : class
    where TCommand : class, TMessage
    where TEvent : class, TMessage
    where TRequest : class, TMessage
    where TResponse : class, TMessage {

        /// <summary>
        /// Set the default name for the queue for messages to be sent
        /// </summary>
        /// <param name="name">Name of the default queue</param>
        /// <param name="queueRequestOptions">Name of the default queue</param>
        /// <returns></returns>
        ICanSpecifyAzureStorageQueueMessageConfig<TMessage, TCommand, TEvent, TRequest, TResponse> WithDefaultConfig(string name, QueueRequestOptions queueRequestOptions = null);
    }


    /// <summary>
    /// Configurable messages to be sent
    /// </summary>
    /// <typeparam name="TMessage"></typeparam>
    /// <typeparam name="TCommand"></typeparam>
    /// <typeparam name="TEvent"></typeparam>
    /// <typeparam name="TRequest"></typeparam>
    /// <typeparam name="TResponse"></typeparam>
    public interface ICanSpecifyAzureStorageQueueMessageConfig<TMessage, TCommand, TEvent, TRequest, TResponse>
        : ICanSpecifyAzureStorageQueueDefaultMessageConfig<TMessage, TCommand, TEvent, TRequest, TResponse>,
        ICanSpecifyEndpointSerializers<TMessage, TCommand, TEvent, TRequest, TResponse>
        where TMessage : class
    where TCommand : class, TMessage
    where TEvent : class, TMessage
    where TRequest : class, TMessage
    where TResponse : class, TMessage {

        /// <summary>
        /// Queue usage
        /// </summary>
        /// <param name="queueName">Queue name</param>
        /// <param name="requestOptions">Queue request options</param>
        /// <returns></returns>
        ICanSpecifyAzureStorageQueueMessageConfig<TMessage, TCommand, TEvent, TRequest, TResponse> WithCommandQueue(string queueName, QueueRequestOptions requestOptions = null);
        ICanSpecifyAzureStorageQueueMessageConfig<TMessage, TCommand, TEvent, TRequest, TResponse> WithEventQueue(string queueName, QueueRequestOptions requestOptions = null);
        ICanSpecifyAzureStorageQueueMessageConfig<TMessage, TCommand, TEvent, TRequest, TResponse> WithRequestQueue(string queueName, QueueRequestOptions requestOptions = null);
        ICanSpecifyAzureStorageQueueMessageConfig<TMessage, TCommand, TEvent, TRequest, TResponse> WithResponseQueue(string queueName, QueueRequestOptions requestOptions = null);
    }

    /// <summary>
    /// Queue connection credentials
    /// </summary>
    /// <typeparam name="TMessage"></typeparam>
    /// <typeparam name="TCommand"></typeparam>
    /// <typeparam name="TEvent"></typeparam>
    /// <typeparam name="TRequest"></typeparam>
    /// <typeparam name="TResponse"></typeparam>
    public interface ICanSpecifyAzureStorageQueueCredentials<TMessage, TCommand, TEvent, TRequest, TResponse>
        where TMessage : class
    where TCommand : class, TMessage
    where TEvent : class, TMessage
    where TRequest : class, TMessage
    where TResponse : class, TMessage {

        /// <summary>
        /// Configure from credentials
        /// </summary>
        /// <param name="accountName"></param>
        /// <param name="accountKey"></param>
        /// <returns></returns>
        ICanSpecifyAzureStorageQueueMessageConfig<TMessage, TCommand, TEvent, TRequest, TResponse> WithAccountCredentials(string accountName, string accountKey);

        /// <summary>
        /// Configure from credentials
        /// </summary>
        /// <param name="credentials">Credentials</param>
        /// <param name="useHttps">Use HTTPS?</param>
        /// <returns></returns>
        ICanSpecifyAzureStorageQueueMessageConfig<TMessage, TCommand, TEvent, TRequest, TResponse> WithStorageCredentials(StorageCredentials credentials, bool useHttps = true);

        /// <summary>
        /// Configure from account
        /// </summary>
        /// <param name="account">Account</param>
        /// <returns></returns>
        ICanSpecifyAzureStorageQueueMessageConfig<TMessage, TCommand, TEvent, TRequest, TResponse> WithStorageAccount(CloudStorageAccount account);

        /// <summary>
        /// Configure from cloud queue client
        /// </summary>
        /// <param name="client"></param>
        /// <returns></returns>
        ICanSpecifyAzureStorageQueueMessageConfig<TMessage, TCommand, TEvent, TRequest, TResponse> WithCloudClient(CloudQueueClient client);

        /// <summary>
        /// Configure from connection string
        /// </summary>
        /// <param name="connectionString">Connection string</param>
        /// <returns></returns>
        ICanSpecifyAzureStorageQueueMessageConfig<TMessage, TCommand, TEvent, TRequest, TResponse> WithConnectionString(string connectionString);

    }

    internal class AzureStorageQueueFluentConfig<TServiceMessage, TMessage, TCommand, TEvent, TRequest, TResponse>:
        ICanSpecifyAzureStorageQueueServiceName<TMessage, TCommand, TEvent, TRequest, TResponse>,
        ICanSpecifyAzureStorageQueueCredentials<TMessage, TCommand, TEvent, TRequest, TResponse>,
        ICanSpecifyAzureStorageQueueMessageConfig<TMessage, TCommand, TEvent, TRequest, TResponse>,
        ICanCreateEndpointAsClientOrServer<TMessage, TCommand, TEvent, TRequest, TResponse>
        where TMessage : class
    where TServiceMessage : class
    where TCommand : class, TMessage
    where TEvent : class, TMessage
    where TRequest : class, TMessage
    where TResponse : class, TMessage {
        private readonly ICanAddEndpoint<TMessage, TCommand, TEvent, TRequest, TResponse> _canAddEndpoint;
        private string _serviceName;
        private string _defaultQueueName;
        private Dictionary<Type, string> _queueNamesByMessageType = new Dictionary<Type, string>();
        private IMessageSerializer _serializer;
        private IMessageDeserializerFactory _deserializerFactory;
        private CloudQueueClient _cloudQueueClient;
        private QueueRequestOptions _defaultRequestOptions;
        private Dictionary<Type, QueueRequestOptions> _queueRequestOptionsByMessageType = new Dictionary<Type, QueueRequestOptions>();

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="canAddEndpoint"></param>
        public AzureStorageQueueFluentConfig(ICanAddEndpoint<TMessage, TCommand, TEvent, TRequest, TResponse> canAddEndpoint) {
            _canAddEndpoint = canAddEndpoint;
        }

        /// <inheritdoc />
        public ICanSpecifyAzureStorageQueueCredentials<TMessage, TCommand, TEvent, TRequest, TResponse> Named(string serviceName) {
            if (string.IsNullOrEmpty(serviceName)) {
                throw new InvalidServiceNameException(innerException: new ArgumentNullException(serviceName));
            }
            _serviceName = serviceName;
            return this;
        }

        #region "ICanCreateEndpointAsClientOrServer implementation"
        public ICanAddEndpointOrLoggingOrCorrelationOrCreate<TMessage, TCommand, TEvent, TRequest, TResponse> AsClient() {
            var provider = CreateProvider();
            return _canAddEndpoint.WithClientEndpoints(provider);
        }

        public ICanAddEndpointOrLoggingOrCorrelationOrCreate<TMessage, TCommand, TEvent, TRequest, TResponse> AsServer() {
            var provider = CreateProvider();
            return _canAddEndpoint.WithServerEndpoints(provider);
        }

        public ICanAddEndpointOrLoggingOrCorrelationOrCreate<TMessage, TCommand, TEvent, TRequest, TResponse> AsClientAndServer() {
            var provider = CreateProvider();
            return _canAddEndpoint.WithEndpoints(provider);
        }
        #endregion

        /// <summary>
        /// Create provider
        /// </summary>
        /// <returns>Instance of AzureStorageQueueServiceEndpointProvider</returns>
        private AzureStorageQueueServiceEndpointProvider<TServiceMessage, TMessage, TCommand, TEvent, TRequest, TResponse> CreateProvider() {
            if (_cloudQueueClient == null) {
                throw new ProviderConfigurationException(
                    innerException: new NullReferenceException(nameof(_cloudQueueClient)));
            }
            if (_defaultQueueName == null && !_queueNamesByMessageType.Any()) {
                throw new ProviderConfigurationException(
                    innerException: new ArgumentException("At least one queue name mapping must be specified"));
            }

            return new AzureStorageQueueServiceEndpointProvider<TServiceMessage, TMessage, TCommand, TEvent, TRequest, TResponse>(
                _serviceName,
                _queueNamesByMessageType,
                _cloudQueueClient,
                _serializer,
                _deserializerFactory,
                _queueRequestOptionsByMessageType);
        }

        #region "Credentials configuration"
        /// <inheritdoc />
        public ICanSpecifyAzureStorageQueueMessageConfig<TMessage, TCommand, TEvent, TRequest, TResponse> WithConnectionString(string connectionString) {
            CloudStorageAccount account;
            try {
                account = CloudStorageAccount.Parse(connectionString);
            } catch (Exception ex) {
                throw new InvalidCredentialsConfigurationException("Invalid connection string", ex);
            }
            return WithStorageAccount(account);
        }

        /// <inheritdoc />
        public ICanSpecifyAzureStorageQueueMessageConfig<TMessage, TCommand, TEvent, TRequest, TResponse> WithAccountCredentials(string accountName, string accountKey) {
            StorageCredentials storageCredentials;
            try {
                storageCredentials = new StorageCredentials(accountName, accountKey);
            } catch (Exception ex) {
                throw new InvalidCredentialsConfigurationException("Invalid storage credentials", ex);
            }
            return WithStorageCredentials(storageCredentials);
            
        }

        /// <inheritdoc />
        public ICanSpecifyAzureStorageQueueMessageConfig<TMessage, TCommand, TEvent, TRequest, TResponse> WithStorageCredentials(StorageCredentials credentials, bool useHttps = true) {
            if (credentials == null) {
                throw new InvalidCredentialsConfigurationException(innerException: new ArgumentNullException(nameof(credentials)));
            }
            return WithStorageAccount(new CloudStorageAccount(credentials, useHttps));
        }

        /// <inheritdoc />
        public ICanSpecifyAzureStorageQueueMessageConfig<TMessage, TCommand, TEvent, TRequest, TResponse> WithStorageAccount(CloudStorageAccount account) {
            if (account == null) {
                throw new InvalidCredentialsConfigurationException(innerException: new ArgumentNullException(nameof(account)));
            }
            return WithCloudClient(account.CreateCloudQueueClient());
        }

        /// <inheritdoc />
        public ICanSpecifyAzureStorageQueueMessageConfig<TMessage, TCommand, TEvent, TRequest, TResponse> WithCloudClient(CloudQueueClient client) {
            if (client == null) {
                throw new InvalidCredentialsConfigurationException(innerException: new ArgumentNullException(nameof(client)));
            }
            _cloudQueueClient = client;
            return this;
        }
        #endregion

        #region "Message configuration"

        public ICanSpecifyAzureStorageQueueMessageConfig<TMessage, TCommand, TEvent, TRequest, TResponse> WithDefaultConfig(string queueName, QueueRequestOptions queueRequestOptions = null) {
        if (string.IsNullOrEmpty(queueName)) {
                throw new ArgumentNullException(nameof(queueName));
            }
            _defaultQueueName = queueName;
            _defaultRequestOptions = queueRequestOptions;
            return this;
        }

        private ICanSpecifyAzureStorageQueueMessageConfig<TMessage, TCommand, TEvent, TRequest, TResponse> WithMessageConfig<T>(string queueName, QueueRequestOptions requestOptions = null) where T : TMessage {
            if (string.IsNullOrEmpty(queueName)) {
                throw new ArgumentNullException(nameof(queueName));
            }
            var messageType = typeof(T);
            _queueNamesByMessageType[messageType] = queueName;
            _queueRequestOptionsByMessageType[messageType] = requestOptions ?? _defaultRequestOptions;
            return this;
        }

        public ICanSpecifyAzureStorageQueueMessageConfig<TMessage, TCommand, TEvent, TRequest, TResponse> WithCommandQueue(string queueName, QueueRequestOptions requestOptions = null)
        {
            return WithMessageConfig<TCommand>(queueName, requestOptions);
        }

        public ICanSpecifyAzureStorageQueueMessageConfig<TMessage, TCommand, TEvent, TRequest, TResponse> WithEventQueue(string queueName, QueueRequestOptions requestOptions = null)
        {
            return WithMessageConfig<TEvent>(queueName, requestOptions);
        }

        public ICanSpecifyAzureStorageQueueMessageConfig<TMessage, TCommand, TEvent, TRequest, TResponse> WithRequestQueue(string queueName, QueueRequestOptions requestOptions = null)
        {
            return WithMessageConfig<TRequest>(queueName, requestOptions);
        }

        public ICanSpecifyAzureStorageQueueMessageConfig<TMessage, TCommand, TEvent, TRequest, TResponse> WithResponseQueue(string queueName, QueueRequestOptions requestOptions = null)
        {
            return WithMessageConfig<TResponse>(queueName, requestOptions);
        }
        #endregion

        #region "Serializers"
        public ICanCreateEndpointAsClientOrServer<TMessage, TCommand, TEvent, TRequest, TResponse> SerializedWith(IMessageSerializer serializer, IMessageDeserializerFactory deserializerFactory) {
            _serializer = serializer;
            _deserializerFactory = deserializerFactory;
            return this;
        }
        #endregion

        /// <inheritdoc />
        public ICanCreateEndpointAsClientOrServer<TMessage, TCommand, TEvent, TRequest, TResponse> FilterMessageTypeAssemblies(Func<Assembly, bool> assemblyFilter = null, Func<Type, bool> typeFilter = null) {
            throw new NotImplementedException();
        }

    }
}