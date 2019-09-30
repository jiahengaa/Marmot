using Microsoft.Extensions.Options;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Marmot.Topic
{
    public sealed class TopicMessageConsumer: ITopicMessageConsumer
    {
        private readonly SemaphoreSlim connectionLock = new SemaphoreSlim(initialCount: 1, maxCount: 1);
        private readonly IConnectionChannelPool connectionChannelPool;
        private IModel channel;

        private IConnection connection;

        private readonly string exchangeName;
        private readonly string exchangeType;
        private readonly string queueName;
        private readonly bool durable;
        private readonly bool autoDelete;
        private readonly IDictionary<string, object> arguments;
        public TopicMessageConsumer(
            string exchangeName,
            string exchangeType,
            string queueName,
            bool durable,
            bool autoDelete,
            IDictionary<string, object> arguments,
            IConnectionChannelPool connectionChannelPool,
             Action<object, BasicDeliverEventArgs, IModel> consumerReceived = null,
             Action<object, ConsumerEventArgs, IModel> consumerRegistered = null,
             Action<object, ConsumerEventArgs, IModel> consumerUnregistered = null,
             Action<object, ConsumerEventArgs, IModel> consumerCancelled = null,
             Action<object, ShutdownEventArgs, IModel> consumerShutdown = null)
        {
            this.exchangeName = exchangeName;
            this.exchangeType = exchangeType;
            this.queueName = queueName;
            this.durable = durable;
            this.autoDelete = autoDelete;
            this.arguments = arguments;
            this.connectionChannelPool = connectionChannelPool;

            this.consumerReceived = consumerReceived;
            this.consumerRegistered = consumerRegistered;
            this.consumerUnregistered = consumerUnregistered;
            this.consumerCancelled = consumerCancelled;
            this.consumerShutdown = consumerShutdown;

        }

        private Action<object, ConsumerEventArgs, IModel> consumerCancelled { set; get; }
        private Action<object, ConsumerEventArgs, IModel> consumerUnregistered { set; get; }
        private Action<object,ConsumerEventArgs, IModel> consumerRegistered { set; get; }
        private Action<object,ShutdownEventArgs, IModel> consumerShutdown { set; get; }
        private Action<object, BasicDeliverEventArgs, IModel> consumerReceived { set; get; }

        public async Task StartListen(IEnumerable<string> topics,TimeSpan timeout, CancellationToken cancellationToken, bool autoAck = false)
        {
            await Task.Factory.StartNew(() =>
            {
                if (topics == null)
                {
                    throw new ArgumentNullException(nameof(topics));
                }

                Connect();

                foreach (var topic in topics)
                {
                    channel.QueueBind(queueName, exchangeName, topic);
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
                channel.BasicConsume(queueName, autoAck, consumer);

                while (true)
                {
                    cancellationToken.ThrowIfCancellationRequested();
                    cancellationToken.WaitHandle.WaitOne(timeout);
                }
            }, cancellationToken);
        }

        private void Connect()
        {
            if(connection != null)
            {
                return;
            }

            connectionLock.Wait();

            try
            {
                if(connection == null)
                {
                    connection = connectionChannelPool.GetConnection();
                    channel = connection.CreateModel();
                    channel.ExchangeDeclare(exchangeName, exchangeType, true);

                    channel.QueueDeclare(queueName, durable, exclusive: false, autoDelete, arguments);
                }
            }
            finally
            {
                connectionLock.Release();
            }
        }

        public void Dispose()
        {
            channel?.Dispose();
            connection?.Dispose();
        }
    }
}
