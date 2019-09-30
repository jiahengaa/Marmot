using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Marmot.Fanout
{
    public interface IFPublish
    {
        Task<bool> PublishAsync<T>(T content, string exchange= "MarmotExchangeFanout");
    }
}
