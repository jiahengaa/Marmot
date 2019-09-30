using Microsoft.Extensions.Options;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

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

        private Action<object, ConsumerEventArgs, IModel> consumerCancelled { set; get; }
        private Action<object, ConsumerEventArgs, IModel> consumerUnregistered { set; get; }
        private Action<object, ConsumerEventArgs, IModel> consumerRegistered { set; get; }
        private Action<object, ShutdownEventArgs, IModel> consumerShutdown { set; get; }
        private Action<object, BasicDeliverEventArgs, IModel> consumerReceived { set; get; }

        public DReceiver(string exchange, string queue,bool durable,bool exclusive, bool autoDelete,
             IConnectionChannelPool connectionChannelPool, IOptions<RabbitMQOptions> rbOptions,
             Action<object, BasicDeliverEventArgs, IModel> consumerReceived = null,
             Action<object, ConsumerEventArgs, IModel> consumerRegistered = null,
             Action<object, ConsumerEventArgs, IModel> consumerUnregistered = null,
             Action<object, ConsumerEventArgs, IModel> consumerCancelled = null,
             Action<object, ShutdownEventArgs, IModel> consumerShutdown = null)
        {
            this.exchangeName = exchange;
            this.durable = durable;
            this.exclusive = exclusive;
            this.autoDelete = autoDelete;
            this.connectionChannelPool = connectionChannelPool;
            this.rabbitMQOptions = rbOptions.Value;

            this.consumerReceived = consumerReceived;
            this.consumerRegistered = consumerRegistered;
            this.consumerUnregistered = consumerUnregistered;
            this.consumerCancelled = consumerCancelled;
            this.consumerShutdown = consumerShutdown;

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

        public async Task StartListen(IEnumerable<string> routingKeys,TimeSpan timeout, CancellationToken cancellationToken,bool autoAck = true)
        {
           await Task.Factory.StartNew(() =>
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

                var consumer = new EventingBasicConsumer(channel);
                consumer.Received += (sender, e) =>
                {
                    consumerReceived?.Invoke(sender, e, autoAck?null: channel);
                };
                consumer.Shutdown += (sender, e) => { consumerShutdown?.Invoke(sender, e, channel); };
                consumer.Registered += (sender, e) => { consumerRegistered?.Invoke(sender, e, channel); };
                consumer.Unregistered += (sender, e) => { consumerUnregistered?.Invoke(sender, e, channel); };
                consumer.ConsumerCancelled += (sender, e) => { consumerCancelled?.Invoke(sender, e, channel); };
                channel.BasicConsume(queue, autoAck: autoAck, consumer);

                while (true)
                {
                    cancellationToken.ThrowIfCancellationRequested();
                    cancellationToken.WaitHandle.WaitOne(timeout);
                }
            }, cancellationToken);
        }
    }
}
