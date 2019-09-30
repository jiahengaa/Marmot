using Marmot.Topic;
using Microsoft.Extensions.Options;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System;
using System.Collections.Generic;
using System.Text;

namespace Marmot
{
    public sealed class TopicConsumerClientFactory: ITopicConsumerClientFactory
    {
        private readonly IConnectionChannelPool connectionChannelPool;

        public TopicConsumerClientFactory(
            IConnectionChannelPool connectionChannelPool)
        {
            this.connectionChannelPool = connectionChannelPool;
        }
        public ITopicMessageConsumer Create(
            string exchangeName = "MarmotExchange",
            string exchangeType = "Topic",
            string queueName = "MarmotQueue",
            bool durable = true,
            bool autoDelete = false,
            Action<object, BasicDeliverEventArgs, IModel> consumerReceived = null,
            Action<object, ConsumerEventArgs, IModel> consumerRegistered = null,
            Action<object, ConsumerEventArgs, IModel> consumerUnregistered = null,
            Action<object, ConsumerEventArgs, IModel> consumerCancelled = null,
            Action<object, ShutdownEventArgs, IModel> consumerShutdown = null,
            IDictionary<string, object> arguments = null
            )
        {
            return new TopicMessageConsumer(
                exchangeName, 
                exchangeType, 
                queueName, 
                durable, 
                autoDelete, 
                arguments, 
                connectionChannelPool,
                consumerReceived,
                consumerRegistered,
                consumerUnregistered,
                consumerCancelled,
                consumerShutdown);
        }
    }
}
