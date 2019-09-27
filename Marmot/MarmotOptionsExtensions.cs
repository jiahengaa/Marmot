using Marmot.Topic;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Text;

namespace Marmot
{
    public static class MarmotOptionsExtensions
    {
        public static IServiceCollection UseMarmotMQ(this IServiceCollection services, string hostName)
        {
            return services.UseMarmotMQ(opt => { opt.HostName = hostName; }) ;
        }

        public static IServiceCollection UseMarmotMQ(this IServiceCollection services, Action<RabbitMQOptions> configure)
        {
            if(configure == null)
            {
                throw new ArgumentNullException(nameof(configure));
            }

            services.Configure(configure);

            services.AddSingleton<IConnectionChannelPool, ConnectionChannelPool>();
            services.AddSingleton<ITopicConsumerClientFactory, TopicConsumerClientFactory>();
            services.AddSingleton<IPublishMessageSender, PublishMessageSender>();

            return services;
        }
    }
}
