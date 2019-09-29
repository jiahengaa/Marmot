using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;

namespace Marmot.Fanout
{
    public interface IFSubscribe : IDisposable
    {
        Action<object, ConsumerEventArgs> ConsumerCancelled { set; get; }
        Action<object, ConsumerEventArgs> ConsumerUnregistered { set; get; }
        Action<object, ConsumerEventArgs> ConsumerRegistered { set; get; }
        Action<object, ShutdownEventArgs> ConsumerShutdown { set; get; }
        Action<object, BasicDeliverEventArgs> ConsumerReceived { set; get; }
        string ServersAddress { get; }
        void SubScribe();
        void Listening(TimeSpan timeout, CancellationToken cancellationToken);
    }
}
