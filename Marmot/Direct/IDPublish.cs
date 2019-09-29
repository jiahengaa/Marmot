using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Marmot.Direct
{
    public interface IDPublish
    {
        string ServersAddress { get; }
        Task<bool> PublishAsync<T>(T content, string routingkey, string exchange = "MarmotExchangeDirect", bool durable = true, bool autoDeltete = false);
    }
}
