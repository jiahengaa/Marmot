using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Marmot.Direct
{
    public interface IDReceiver
    {
        /// <summary>
        /// Start Listen
        /// </summary>
        /// <param name="routingKeys"></param>
        /// <param name="timeout"></param>
        /// <param name="cancellationToken"></param>
        /// <param name="autoAck"></param>
        /// <returns></returns>
        Task StartListen(IEnumerable<string> routingKeys, TimeSpan timeout, CancellationToken cancellationToken, bool autoAck = true);
    }
}
