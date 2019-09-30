using Microsoft.Extensions.Options;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Marmot.Fanout
{
    internal sealed class FSubscribe: IFSubscribe
    {
        private readonly SemaphoreSlim connectionLock = new SemaphoreSlim(initialCount: 1, maxCount: 1);
        private readonly IConnectionChannelPool connectionChannelPool;
        private IModel channel;

        private IConnection connection;

        private readonly string exchangeName;
        private string queueName;
        private ulong deliveryTag;

        private Action<object, ConsumerEventArgs, IModel> consumerCancelled { set; get; }
        private Action<object, ConsumerEventArgs, IModel> consumerUnregistered { set; get; }
        private Action<object, ConsumerEventArgs, IModel> consumerRegistered { set; get; }
        private Action<object, ShutdownEventArgs, IModel> consumerShutdown { set; get; }
        private Action<object, BasicDeliverEventArgs, IModel> consumerReceived { set; get; }

        public FSubscribe(
            string exchange,
            IConnectionChannelPool connectionChannelPool,
             Action<object, BasicDeliverEventArgs, IModel> consumerReceived = null,
             Action<object, ConsumerEventArgs, IModel> consumerRegistered = null,
             Action<object, ConsumerEventArgs, IModel> consumerUnregistered = null,
             Action<object, ConsumerEventArgs, IModel> consumerCancelled = null,
             Action<object, ShutdownEventArgs, IModel> consumerShutdown = null
            )
        {
            this.exchangeName = exchange;
            this.connectionChannelPool = connectionChannelPool;

            this.consumerCancelled = consumerCancelled;
            this.consumerReceived = consumerReceived;
            this.consumerRegistered = consumerRegistered;
            this.consumerShutdown = consumerShutdown;
            this.consumerUnregistered = consumerUnregistered;
        }
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

        public async Task StartListen(TimeSpan timeout, CancellationToken cancellationToken, bool autoAck = true)
        {
            await Task.Factory.StartNew(() =>
            {
                Connect();
                queueName = channel.QueueDeclare().QueueName;
                channel.QueueBind(queueName, exchangeName, "");

                var consumer = new EventingBasicConsumer(channel);
                consumer.Received += (sender, e) =>
                {
                    deliveryTag = e.DeliveryTag;
                    consumerReceived?.Invoke(sender, e, autoAck ? null: channel);
                };
                consumer.Shutdown += (sender, e) => { consumerShutdown?.Invoke(sender, e, channel); };
                consumer.Registered += (sender, e) => { consumerRegistered?.Invoke(sender, e, channel); };
                consumer.Unregistered += (sender, e) => { consumerUnregistered?.Invoke(sender, e, channel); };
                consumer.ConsumerCancelled += (sender, e) => { consumerCancelled?.Invoke(sender, e, channel); };
                channel.BasicConsume(queueName, autoAck: autoAck, consumer);

                while (true)
                {
                    cancellationToken.ThrowIfCancellationRequested();
                    cancellationToken.WaitHandle.WaitOne(timeout);
                }
            }, cancellationToken);
        }
    }
}
