using Marmot.Topic;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Text;

namespace Marmot
{
    internal sealed class TopicConsumerClientFactory: ITopicConsumerClientFactory
    {
        private readonly IConnectionChannelPool connectionChannelPool;
        private readonly IOptions<RabbitMQOptions> rbOptions;

        public TopicConsumerClientFactory(
            IOptions<RabbitMQOptions> rbOptions,
            IConnectionChannelPool connectionChannelPool)
        {
            this.rbOptions = rbOptions;
            this.connectionChannelPool = connectionChannelPool;
        }
        public ITopicMessageConsumer Create(
            string exchangeName = "MarmotExchange",
            string exchangeType = "Topic",
            string queueName = "MarmotQueue",
            bool durable = true,
            bool autoDelete = false,
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
                rbOptions);
        }
    }
}
