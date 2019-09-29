using Microsoft.Extensions.Options;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;

namespace Marmot.Fanout
{
    internal sealed class FSubscribe: IFSubscribe
    {
        private readonly SemaphoreSlim connectionLock = new SemaphoreSlim(initialCount: 1, maxCount: 1);
        private readonly IConnectionChannelPool connectionChannelPool;
        private readonly RabbitMQOptions rabbitMQOptions;
        private IModel channel;

        private IConnection connection;

        private readonly string exchangeName;
        private string queueName;

        public Action<object, ConsumerEventArgs> ConsumerCancelled { set; get; }
        public Action<object, ConsumerEventArgs> ConsumerUnregistered { set; get; }
        public Action<object, ConsumerEventArgs> ConsumerRegistered { set; get; }
        public Action<object, ShutdownEventArgs> ConsumerShutdown { set; get; }
        public Action<object, BasicDeliverEventArgs> ConsumerReceived { set; get; }

        public FSubscribe(string exchange,IConnectionChannelPool connectionChannelPool, IOptions<RabbitMQOptions> rbOptions)
        {
            this.exchangeName = exchange;
            this.connectionChannelPool = connectionChannelPool;
            this.rabbitMQOptions = rbOptions.Value;
        }

        public string ServersAddress => rabbitMQOptions.HostName;
        public void Dispose()
        {
            channel?.Dispose();
            connection?.Dispose();
        }

        private void Connect()
        {
            if (connection != null)
            {
                return;
            }

            connectionLock.Wait();

            try
            {
                if (connection == null)
                {
                    connection = connectionChannelPool.GetConnection();
                    channel = connection.CreateModel();
                    channel.ExchangeDeclare(exchangeName, ExchangeType.Fanout);
                }
            }
            finally
            {
                connectionLock.Release();
            }
        }

        public void SubScribe()
        {
            Connect();
            queueName = channel.QueueDeclare().QueueName;
            channel.QueueBind(queueName, exchangeName, "");
        }

        public void Listening(TimeSpan timeout, CancellationToken cancellationToken)
        {
            Connect();

            var consumer = new EventingBasicConsumer(channel);
            consumer.Received += (sender, e) =>
            {
                ConsumerReceived?.Invoke(sender, e);
            };
            consumer.Shutdown += (sender, e) => { ConsumerShutdown?.Invoke(sender, e); };
            consumer.Registered += (sender, e) => { ConsumerRegistered?.Invoke(sender, e); };
            consumer.Unregistered += (sender, e) => { ConsumerUnregistered?.Invoke(sender, e); };
            consumer.ConsumerCancelled += (sender, e) => { ConsumerCancelled?.Invoke(sender, e); };
            channel.BasicConsume(queueName, autoAck:true, consumer);

            while (true)
            {
                cancellationToken.ThrowIfCancellationRequested();
                cancellationToken.WaitHandle.WaitOne(timeout);
            }
        }
    }
}
