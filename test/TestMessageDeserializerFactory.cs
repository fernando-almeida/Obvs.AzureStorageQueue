using System;
using System.Linq;
using System.Collections.Generic;
using System.Reflection;
using Obvs.Serialization;
using Obvs.Configuration;
using System.IO;

namespace Obvs.AzureStorageQueue.Tests
{
    public class TestDeserializer<TMessage> : IMessageDeserializer<TMessage> where TMessage: class
    {
        public TMessage Deserialize(Stream source)
        {
            var message = Activator.CreateInstance(typeof(TMessage)) as TMessage;
            return message;
        }

        public string GetTypeName()
        {
            return this.GetType().Name;
        }
    }

    public class TestMessageDeserializerFactory : IMessageDeserializerFactory
    {
        public IEnumerable<IMessageDeserializer<TMessage>> Create<TMessage, TServiceMessage>(
            Func<Assembly, bool> assemblyFilter = null,
            Func<Type, bool> typeFilter = null
    )   where TMessage : class
        where TServiceMessage : class
        {
            var messageTypes = MessageTypes.Get<TMessage, TServiceMessage>(assemblyFilter, typeFilter);
            return messageTypes.Select(deserializerGeneric => new TestDeserializer<TMessage>())
                                .ToArray();
        }
    }
}