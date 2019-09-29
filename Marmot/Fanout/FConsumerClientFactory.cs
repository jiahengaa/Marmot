using Microsoft.Extensions.Options;
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

        public IFSubscribe Create(string exchangeName = "MarmotExchangeFanout")
        {
            return new FSubscribe(
                exchangeName,
                connectionChannelPool,
                rbOptions);
        }
    }
}
