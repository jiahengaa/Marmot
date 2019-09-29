using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;

namespace Marmot.Direct
{
    public interface IDReceiver
    {
        Action<object, ConsumerEventArgs> ConsumerCancelled { set; get; }
        Action<object, ConsumerEventArgs> ConsumerUnregistered { set; get; }
        Action<object, ConsumerEventArgs> ConsumerRegistered { set; get; }
        Action<object, ShutdownEventArgs> ConsumerShutdown { set; get; }
        Action<object, BasicDeliverEventArgs> ConsumerReceived { set; get; }
        string ServersAddress { get; }
        void SubScribe(IEnumerable<string> routingKeys);
        void Listening(TimeSpan timeout, CancellationToken cancellationToken);
    }
}
