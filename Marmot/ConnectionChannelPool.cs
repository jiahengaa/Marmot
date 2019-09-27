using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;
using System.Text;
using System.Threading;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using RabbitMQ.Client;

namespace Marmot
{
    public class ConnectionChannelPool : IConnectionChannelPool, IDisposable
    {
        private readonly ILogger<ConnectionChannelPool> logger;
        private readonly ConcurrentQueue<IModel> pool;
        private IConnection connection;
        private readonly Func<IConnection> connectionFunctor;

        private static readonly object Lock = new object();
        private int count;
        private int maxSize;

        public string HostAddress { get; }

        public ConnectionChannelPool(ILogger<ConnectionChannelPool> logger, IOptions<RabbitMQOptions> rbOptions)
        {
            if (rbOptions.Value.PoolSize<=0)
            {
                throw new Exception("PoolSize must larger zero!");
            }
            this.logger = logger;
            this.maxSize = rbOptions.Value.PoolSize;
            pool = new ConcurrentQueue<IModel>();

            connectionFunctor = CreateConnection(rbOptions.Value);

            HostAddress = $"{rbOptions.Value.HostName}:{rbOptions.Value.Port}";
        }

        public void Dispose()
        {
            maxSize = 0;
            while(pool.TryDequeue(out var model))
            {
                model.Dispose();
            }
        }
        bool IConnectionChannelPool.Fire(IModel connection)
        {
            return Fire(connection);
        }
        public virtual bool Fire(IModel connection)
        {
            if(Interlocked.Increment(ref count) < maxSize)
            {
                pool.Enqueue(connection);
                return true;
            }

            Interlocked.Decrement(ref count);
            Debug.Assert(maxSize == 0 || pool.Count <= maxSize);
            return false;
        }

        public IConnection GetConnection()
        {
            if(connection != null && connection.IsOpen)
            {
                return connection;
            }

            connection = connectionFunctor();
            connection.ConnectionShutdown += (sender, e) => 
                {
                    logger.LogWarning($"RabbitMQ client connection closed.ReplyCode:{e.ReplyCode},ReplyText:{e.ReplyText}");
                };
            return connection;
        }

        IModel IConnectionChannelPool.Hire() {
            lock (Lock)
            {
                while(count > maxSize)
                {
                    Thread.SpinWait(1);
                }
                return Hire();
            }
        }

        public virtual IModel Hire()
        {
            if(pool.TryDequeue(out var model))
            {
                Interlocked.Decrement(ref count);
                Debug.Assert(count >= 0);
                return model;
            }

            try
            {
                model = GetConnection().CreateModel();
            }
            catch(Exception ex)
            {
                logger.LogError(ex, "RabbitMq channel model create failed!");
                Console.WriteLine(ex);
                throw;
            }

            return model;
        }

        private static Func<IConnection> CreateConnection(RabbitMQOptions rbOptions)
        {
            var serviceName = string.IsNullOrEmpty(rbOptions.ServiceName) ? Assembly.GetEntryAssembly()?.GetName().Name.ToLower() : rbOptions.ServiceName;

            var factory = new ConnectionFactory
            {
                VirtualHost = rbOptions.VirtualHost,
                Port = rbOptions.Port,
                UserName = rbOptions.UserName,
                Password = rbOptions.Password,
            };

            if (rbOptions.HostName.Contains(";"))
            {
                rbOptions.ConnectionFactoryAction?.Invoke(factory);
                return () =>
                {
                   return factory.CreateConnection(rbOptions.HostName.Split(new[] { ";" }, StringSplitOptions.RemoveEmptyEntries), serviceName);
                };
            }

            factory.HostName = rbOptions.HostName;
            rbOptions.ConnectionFactoryAction?.Invoke(factory);
            return () => factory.CreateConnection(serviceName);
        }
    }
}
