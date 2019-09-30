using Marmot.Topic;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System;
using System.Collections.Generic;
using System.Text;

namespace Marmot
{
    public interface ITopicConsumerClientFactory
    {
        /// <summary>
        /// Create a Topc Consumer Instance
        /// </summary>
        /// <param name="exchangeName">exchangeName</param>
        /// <param name="exchangeType">exchangeType</param>
        /// <param name="queueName">queueName</param>
        /// <param name="durable">durable</param>
        /// <param name="autoDelete">autoDelete</param>
        /// <param name="consumerReceived">fire on HandleBasicDeliver</param>
        /// <param name="consumerRegistered">fire on HandleBasicConsumeOk</param>
        /// <param name="consumerUnregistered">fire on HandleBasicCancelOk</param>
        /// <param name="consumerCancelled">fire on the consumer gets cancelled.</param>
        /// <param name="consumerShutdown">fire on HandleModelShutdown</param>
        /// <param name="arguments">arguments</param>
        /// <returns></returns>
        ITopicMessageConsumer Create(
                    string exchangeName = "MarmotExchange",
                    string exchangeType = "Topic",
                    string queueName = "MarmotQueue",
                    bool durable = true,
                    bool autoDelete = false,
                    Action<object, BasicDeliverEventArgs, IModel> consumerReceived = null,
                    Action<object, ConsumerEventArgs, IModel> consumerRegistered = null,
                    Action<object, ConsumerEventArgs, IModel> consumerUnregistered = null,
                    Action<object, ConsumerEventArgs, IModel> consumerCancelled = null,
                    Action<object, ShutdownEventArgs, IModel> consumerShutdown = null,
                    IDictionary<string, object> arguments = null
                    );
    }
}
