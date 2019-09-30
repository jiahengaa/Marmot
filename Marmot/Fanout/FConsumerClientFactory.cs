using Microsoft.Extensions.Options;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System;
using System.Collections.Generic;
using System.Text;

namespace Marmot.Fanout
{
    internal sealed class FConsumerClientFactory: IFConsumerClientFactory
    {
        private readonly IConnectionChannelPool connectionChannelPool;
        private readonly IOptions<RabbitMQOptions> rbOptions;

        public FConsumerClientFactory(IOptions<RabbitMQOptions> rbOptions,
            IConnectionChannelPool connectionChannelPool)
        {
            this.rbOptions = rbOptions;
            this.connectionChannelPool = connectionChannelPool;
        }

        public IFSubscribe Create(string exchangeName = "MarmotExchangeFanout", 
             Action<object, BasicDeliverEventArgs, IModel> consumerReceived = null,
             Action<object, ConsumerEventArgs, IModel> consumerRegistered = null,
             Action<object, ConsumerEventArgs, IModel> consumerUnregistered = null,
             Action<object, ConsumerEventArgs, IModel> consumerCancelled = null,
             Action<object, ShutdownEventArgs, IModel> consumerShutdown = null)
        {
            return new FSubscribe(
                exchangeName,
                connectionChannelPool,
                consumerReceived,
                consumerRegistered,
                consumerUnregistered,
                consumerCancelled,
                consumerShutdown
                );
        }
    }
}
