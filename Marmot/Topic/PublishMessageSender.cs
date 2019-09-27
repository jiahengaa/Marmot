using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using RabbitMQ.Client;
using RabbitMQ.Client.Framing;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Marmot.Topic
{
    internal sealed class PublishMessageSender: IPublishMessageSender
    {
        private readonly IConnectionChannelPool connectionChannelPool;
        private readonly ILogger logger;

        public PublishMessageSender(
            ILogger<PublishMessageSender> logger,
             IConnectionChannelPool connectionChannelPool
            )
        {
            this.logger = logger;
            this.connectionChannelPool = connectionChannelPool;
        }
        public string ServersAddress => connectionChannelPool.HostAddress;
        public Task<bool> PublishAsync<T>(
            T content, 
            string exchangeName = "Marmot", 
            string exchangeType = "Marmot.Topic", 
            string keyName = "", 
            bool durable = true,
            bool autoDelete = false,
            IDictionary<string, object> arguments = null
            )
        {
            var channel = connectionChannelPool.Hire();

            try
            {
                var body = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(content));
                var rbProps = new BasicProperties()
                {
                    DeliveryMode = 2
                };

                channel.ExchangeDeclare(exchangeName, exchangeType, durable, autoDelete, arguments);
                channel.BasicPublish(exchangeName, keyName, rbProps, body);
                return Task.FromResult(true);
            }
            catch(Exception ex)
            {
                return Task.FromResult(false);
            }
            finally
            {
                var fired = connectionChannelPool.Fire(channel);
                if (!fired)
                {
                    channel.Dispose();
                }
            }
        }
    }
}
