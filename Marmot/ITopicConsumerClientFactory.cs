using Marmot.Topic;
using System;
using System.Collections.Generic;
using System.Text;

namespace Marmot
{
    public interface ITopicConsumerClientFactory
    {
        ITopicMessageConsumer Create(
            string exchangeName = "MarmotExchange",
            string exchangeType = "Topic",
            string queueName = "MarmotQueue",
            bool durable = true,
            bool autoDelete = false,
            IDictionary<string, object> arguments = null
            );
    }
}
