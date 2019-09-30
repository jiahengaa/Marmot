using Microsoft.Extensions.Options;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System;
using System.Collections.Generic;
using System.Text;

namespace Marmot.Direct
{
    internal sealed class DConsumerClientFactory: IDConsumerClientFactory
    {
        private readonly IConnectionChannelPool connectionChannelPool;
        private readonly IOptions<RabbitMQOptions> rbOptions;

        public DConsumerClientFactory(IOptions<RabbitMQOptions> rbOptions,
            IConnectionChannelPool connectionChannelPool)
        {
            this.rbOptions = rbOptions;
            this.connectionChannelPool = connectionChannelPool;
        }

        public IDReceiver Create(string exchangeName = "MarmotExchangeDirect",
             string queueName = "", 
             Action<object, BasicDeliverEventArgs,IModel> consumerReceived = null,
             Action<object, ConsumerEventArgs, IModel> consumerRegistered = null,
             Action<object, ConsumerEventArgs, IModel> consumerUnregistered = null,
             Action<object, ConsumerEventArgs, IModel> consumerCancelled = null,
             Action<object, ShutdownEventArgs, IModel> consumerShutdown = null,
             bool durable = true,bool exclusive = false, bool autoDelete = false)
        {
            return new DReceiver(
                exchangeName,
                string.IsNullOrEmpty(queueName)?"":queueName,
                durable,
                exclusive,
                autoDelete,
                connectionChannelPool,
                rbOptions,
                consumerReceived,
                consumerRegistered,
                consumerUnregistered,
                consumerCancelled,
                consumerShutdown);
        }
    }
}
