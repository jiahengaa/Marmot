using RabbitMQ.Client;
using System;
using System.Collections.Generic;
using System.Text;

namespace Marmot
{
    public interface IConnectionChannelPool
    {
        string HostAddress { get; }
        IConnection GetConnection();

        IModel Hire();
        bool Fire(IModel connection);
    }
}
