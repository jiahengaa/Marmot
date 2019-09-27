using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;

namespace Marmot.Topic
{
    public interface ITopicMessageConsumer : IDisposable
    {
        Action<object, ConsumerEventArgs> ConsumerCancelled { set; get; }
        Action<object, ConsumerEventArgs> ConsumerUnregistered { set; get; }
        Action<object, ConsumerEventArgs> ConsumerRegistered { set; get; }
        Action<object, ShutdownEventArgs> ConsumerShutdown { set; get; }
        Action<object, BasicDeliverEventArgs> ConsumerReceived { set; get; }
        void SubScribe(IEnumerable<string> topics);

        void Listening(TimeSpan timeout, CancellationToken cancellationToken);

        void Commit();
        void Reject();
    }
}
