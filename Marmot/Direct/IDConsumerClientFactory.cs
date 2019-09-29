using System;
using System.Collections.Generic;
using System.Text;

namespace Marmot.Direct
{
    public interface IDConsumerClientFactory
    {
        IDReceiver Create(string exchangeName = "MarmotExchangeDirect", string queueName = "", bool durable = true, bool exclusive = false, bool autoDelete = false);
    }
}
