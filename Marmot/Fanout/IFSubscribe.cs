using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Marmot.Fanout
{
    /// <summary>
    /// Fanout Consumer
    /// </summary>
    public interface IFSubscribe : IDisposable
    {
        /// <summary>
        /// Start listening
        /// </summary>
        /// <param name="timeout">Blocks the current thread until the current instance receives a signal, using a System.TimeSpan to specify the time interval.</param>
        /// <param name="cancellationToken">Propagates notification that operations should be canceled.</param>
        /// <param name="autoAck">default true</param>
        /// <returns></returns>
        Task StartListen(TimeSpan timeout, CancellationToken cancellationToken,bool autoAck = true);
    }
}
