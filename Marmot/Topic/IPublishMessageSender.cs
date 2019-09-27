using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Marmot.Topic
{
    public interface IPublishMessageSender
    {
        Task<bool> PublishAsync<T>(
            T content,
            string exchangeName = "Marmot",
            string exchangeType = "Topic",
            string keyName = "",
            bool durable = true,
            bool autoDelete = false,
            IDictionary<string, object> arguments = null
            );
    }
}
