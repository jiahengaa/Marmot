using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using RabbitMQ.Client;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Marmot.Direct
{
    internal sealed class DPublish: IDPublish
    {
        private readonly IConnectionChannelPool connectionChannelPool;
        private readonly ILogger logger;

        public DPublish(ILogger<DPublish> logger,IConnectionChannelPool connectionChannelPool)
        {
            this.connectionChannelPool = connectionChannelPool;
            this.logger = logger;
        }

        public string ServersAddress => connectionChannelPool.HostAddress;

        public Task<bool> PublishAsync<T>(
            T content,
            string routingkey, string exchange = "MarmotExchangeDirect",bool durable = true,bool autoDeltete = false)
        {
            var channel = connectionChannelPool.Hire();

            try
            {
                var body = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(content));

                channel.ExchangeDeclare(exchange: exchange, type: ExchangeType.Direct,durable:durable,autoDelete:autoDeltete);

                channel.BasicPublish(exchange: exchange, routingKey: routingkey, mandatory: false, basicProperties: null, body: body);
                return Task.FromResult(true);
            }
            catch (Exception ex)
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
