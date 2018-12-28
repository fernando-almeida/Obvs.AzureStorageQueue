using System;

using Obvs;
using Obvs.Types;
using Obvs.Configuration;

namespace Obvs.AzureStorageQueue.Configuration {
    public static class AzureStorageQueueConfigExtensions {

        /// <summary>
        /// Extension method to create an instance of a fluent configurator for Azure storage queue
        /// </summary>
        /// <param name="canAddEndpoint"></param>
        /// <typeparam name="TServiceMessage"></typeparam>
        /// <typeparam name="TMessage"></typeparam>
        /// <typeparam name="TCommand"></typeparam>
        /// <typeparam name="TEvent"></typeparam>
        /// <typeparam name="TRequest"></typeparam>
        /// <typeparam name="TResponse"></typeparam>
        /// <returns></returns>
        public static ICanSpecifyAzureStorageQueueCredentials<TMessage, TCommand, TEvent, TRequest, TResponse> WithAzureStorageQueueEndpoint<TServiceMessage, TMessage, TCommand, TEvent, TRequest, TResponse>(this ICanAddEndpoint<TMessage, TCommand, TEvent, TRequest, TResponse> canAddEndpoint) 
            where TServiceMessage : class
            where TMessage : class
            where TCommand : class, TMessage
            where TEvent : class, TMessage
            where TRequest : class, TMessage
            where TResponse : class, TMessage
        {
            return new AzureStorageQueueFluentConfig<TServiceMessage, TMessage, TCommand, TEvent, TRequest, TResponse>(canAddEndpoint);
        }

        /// <summary>
        /// Extension method to create an instance of a fluent configurator for Azure storage queue
        /// </summary>
        /// <param name="canAddEndpoint"></param>
        /// <typeparam name="TServiceMessage"></typeparam>
        /// <returns></returns>
        public static ICanSpecifyAzureStorageQueueServiceName<IMessage, ICommand, IEvent, IRequest, IResponse> WithAzureStorageQueueEndpoint<TServiceMessage>(this ICanAddEndpoint<IMessage, ICommand, IEvent, IRequest, IResponse> canAddEndpoint) 
            where TServiceMessage : class
        {
            return new AzureStorageQueueFluentConfig<TServiceMessage, IMessage, ICommand, IEvent, IRequest, IResponse>(canAddEndpoint);
        }
    }
}