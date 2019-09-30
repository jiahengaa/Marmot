using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Marmot.Topic
{
    /// <summary>
    /// Topic Consumer Instance
    /// </summary>
    public interface ITopicMessageConsumer : IDisposable
    {
        /// <summary>
        /// Start listen
        /// </summary>
        /// <param name="topics"></param>
        /// <param name="timeout"></param>
        /// <param name="cancellationToken"></param>
        /// <param name="autoAck"></param>
        /// <returns></returns>
        Task StartListen(IEnumerable<string> topics, TimeSpan timeout, CancellationToken cancellationToken, bool autoAck = false);
    }
}
