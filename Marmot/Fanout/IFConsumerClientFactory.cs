using System;
using System.Collections.Generic;
using System.Text;

namespace Marmot.Fanout
{
    public interface IFConsumerClientFactory
    {
        IFSubscribe Create(string exchangeName = "MarmotExchangeFanout");
    }
}
