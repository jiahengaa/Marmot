using RabbitMQ.Client;
using System;

namespace Marmot
{
    public class RabbitMQOptions
    {
        public const string DefaultPasswd = "guest";
        public const string DefaultUser = "guest";
        public const string DefaultVirtualHost = "/";

        public const int DefaultPoolSize = 50;
        public const string DefaultServiceName = "Marmot";
        public string HostName { set; get; } = "localhost";
        public string Password { set; get; } = DefaultPasswd;
        public string UserName { set; get; } = DefaultUser;
        public string VirtualHost { set; get; } = DefaultVirtualHost;

        public string ServiceName { set; get; } = DefaultServiceName;

        public int PoolSize { set; get; } = DefaultPoolSize;

        /// <summary>
        /// Port
        /// </summary>
        public int Port { set; get; } = 2333;
        /// <summary>
        /// RabbitMQ native connection factory config
        /// </summary>
        public Action<ConnectionFactory> ConnectionFactoryAction { set; get; }
    }
}
