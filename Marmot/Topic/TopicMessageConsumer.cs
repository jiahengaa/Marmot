using Microsoft.Extensions.Options;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;

namespace Marmot.Topic
{
    internal sealed class TopicMessageConsumer: ITopicMessageConsumer
    {
        private readonly SemaphoreSlim connectionLock = new SemaphoreSlim(initialCount: 1, maxCount: 1);
        private readonly IConnectionChannelPool connectionChannelPool;
        private readonly RabbitMQOptions rabbitMQOptions;
        private ulong deliveryTag;
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
            IOptions<RabbitMQOptions> rbOptions)
        {
            this.exchangeName = exchangeName;
            this.exchangeType = exchangeType;
            this.queueName = queueName;
            this.durable = durable;
            this.autoDelete = autoDelete;
            this.arguments = arguments;
            this.connectionChannelPool = connectionChannelPool;
            this.rabbitMQOptions = rbOptions.Value;
        }

        public string ServersAddress => rabbitMQOptions.HostName;

        public Action<object, ConsumerEventArgs> ConsumerCancelled { set; get; }
        public Action<object, ConsumerEventArgs> ConsumerUnregistered { set; get; }
        public Action<object,ConsumerEventArgs> ConsumerRegistered { set; get; }
        public Action<object,ShutdownEventArgs> ConsumerShutdown { set; get; }
        public Action<object, BasicDeliverEventArgs> ConsumerReceived { set; get; }
        
        public void SubScribe(IEnumerable<string> topics)
        {
            if(topics == null)
            {
                throw new ArgumentNullException(nameof(topics));
            }

            Connect();

            foreach(var topic in topics)
            {
                channel.QueueBind(queueName, exchangeName, topic);
            }

        }

        public void Listening(TimeSpan timeout, CancellationToken cancellationToken)
        {
            Connect();

            var consumer = new EventingBasicConsumer(channel);
            consumer.Received += (sender, e) => 
            { 
                deliveryTag = e.DeliveryTag; 
                ConsumerReceived?.Invoke(sender, e);
            };
            consumer.Shutdown += (sender, e) => { ConsumerShutdown?.Invoke(sender, e); };
            consumer.Registered += (sender, e) => { ConsumerRegistered?.Invoke(sender, e); };
            consumer.Unregistered += (sender,e)=> { ConsumerUnregistered?.Invoke(sender, e); };
            consumer.ConsumerCancelled += (sender,e)=> { ConsumerCancelled?.Invoke(sender, e); };
            channel.BasicConsume(queueName, false, consumer);

            while (true)
            {
                cancellationToken.ThrowIfCancellationRequested();
                cancellationToken.WaitHandle.WaitOne(timeout);
            }
        }
        public void Commit()
        {
            channel.BasicAck(deliveryTag, false);
        }
        public void Reject()
        {
            channel.BasicReject(deliveryTag, true);
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
