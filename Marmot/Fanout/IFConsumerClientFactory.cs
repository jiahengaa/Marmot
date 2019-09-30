using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System;
using System.Collections.Generic;
using System.Text;

namespace Marmot.Fanout
{
    /// <summary>
    /// Fanout Client Facotory
    /// </summary>
    public interface IFConsumerClientFactory
    {
        /// <summary>
        /// Create a Fanout Client Instance
        /// </summary>
        /// <param name="exchangeName">exchangeName</param>
        /// <param name="consumerReceived">fire on HandleBasicDeliver</param>
        /// <param name="consumerRegistered">fire on HandleBasicConsumeOk</param>
        /// <param name="consumerUnregistered">fire on HandleBasicCancelOk</param>
        /// <param name="consumerCancelled">fire on the consumer gets cancelled.</param>
        /// <param name="consumerShutdown">fire on HandleModelShutdown</param>
        /// <returns></returns>
        IFSubscribe Create(string exchangeName = "MarmotExchangeFanout",
             Action<object, BasicDeliverEventArgs, IModel> consumerReceived = null,
             Action<object, ConsumerEventArgs, IModel> consumerRegistered = null,
             Action<object, ConsumerEventArgs, IModel> consumerUnregistered = null,
             Action<object, ConsumerEventArgs, IModel> consumerCancelled = null,
             Action<object, ShutdownEventArgs, IModel> consumerShutdown = null);
    }
}
