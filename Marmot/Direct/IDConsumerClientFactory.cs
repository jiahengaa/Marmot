using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System;
using System.Collections.Generic;
using System.Text;

namespace Marmot.Direct
{
    public interface IDConsumerClientFactory
    {
        /// <summary>
        /// Create a Direct Consumer Instance
        /// </summary>
        /// <param name="exchangeName">exchangeName</param>
        /// <param name="queueName">queueName</param>
        /// <param name="consumerReceived">fire on HandleBasicDeliver</param>
        /// <param name="consumerRegistered">fire on HandleBasicConsumeOk</param>
        /// <param name="consumerUnregistered">fire on HandleBasicCancelOk</param>
        /// <param name="consumerCancelled">fire on the consumer gets cancelled.</param>
        /// <param name="consumerShutdown">fire on HandleModelShutdown</param>
        /// <param name="durable">durable</param>
        /// <param name="exclusive">exclusive</param>
        /// <param name="autoDelete">autoDelete</param>
        /// <returns></returns>
        IDReceiver Create(string exchangeName = "MarmotExchangeDirect",
             string queueName = "",
             Action<object, BasicDeliverEventArgs, IModel> consumerReceived = null,
             Action<object, ConsumerEventArgs, IModel> consumerRegistered = null,
             Action<object, ConsumerEventArgs, IModel> consumerUnregistered = null,
             Action<object, ConsumerEventArgs, IModel> consumerCancelled = null,
             Action<object, ShutdownEventArgs, IModel> consumerShutdown = null,
             bool durable = true, bool exclusive = false, bool autoDelete = false);
    }
}
