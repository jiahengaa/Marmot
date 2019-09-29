using Microsoft.Extensions.Options;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;

namespace Marmot.Direct
{
    internal sealed class DReceiver: IDReceiver
    {
        private readonly SemaphoreSlim connectionLock = new SemaphoreSlim(initialCount: 1, maxCount: 1);
        private readonly IConnectionChannelPool connectionChannelPool;
        private readonly RabbitMQOptions rabbitMQOptions;
        private IModel channel;

        private IConnection connection;

        private readonly string exchangeName;
        private readonly bool durable;
        private readonly bool autoDelete;
        private readonly bool exclusive;
        private string queue;

        public Action<object, ConsumerEventArgs> ConsumerCancelled { set; get; }
        public Action<object, ConsumerEventArgs> ConsumerUnregistered { set; get; }
        public Action<object, ConsumerEventArgs> ConsumerRegistered { set; get; }
        public Action<object, ShutdownEventArgs> ConsumerShutdown { set; get; }
        public Action<object, BasicDeliverEventArgs> ConsumerReceived { set; get; }

        public DReceiver(string exchange, string queue,bool durable,bool exclusive, bool autoDelete, IConnectionChannelPool connectionChannelPool, IOptions<RabbitMQOptions> rbOptions)
        {
            this.exchangeName = exchange;
            this.durable = durable;
            this.exclusive = exclusive;
            this.autoDelete = autoDelete;
            this.connectionChannelPool = connectionChannelPool;
            this.rabbitMQOptions = rbOptions.Value;

            this.queue = queue;
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
                    channel.ExchangeDeclare(exchangeName, ExchangeType.Direct,durable,autoDelete);
                }
            }
            finally
            {
                connectionLock.Release();
            }
        }

        public void SubScribe(IEnumerable<string> routingKeys)
        {
            if (routingKeys == null)
            {
                throw new ArgumentNullException(nameof(routingKeys));
            }

            Connect();

            //如果是自动生成的queue，则在消费者掉线后，删掉自动生成的queue，不做持久化
            bool isAutoQueueName = string.IsNullOrEmpty(queue);
            queue = isAutoQueueName ? channel.QueueDeclare().QueueName : queue;
            if (!isAutoQueueName)
            {
                channel.QueueDeclare(queue, durable, exclusive, autoDelete);
            }

            foreach (var routingKey in routingKeys)
            {
                channel.QueueBind(queue, exchangeName, routingKey);
            }

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
            channel.BasicConsume(queue, autoAck: true, consumer);

            while (true)
            {
                cancellationToken.ThrowIfCancellationRequested();
                cancellationToken.WaitHandle.WaitOne(timeout);
            }
        }
    }
}
