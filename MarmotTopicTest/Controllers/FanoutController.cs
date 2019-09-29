using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Marmot.Fanout;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace MarmotTopicTest.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class FanoutController : ControllerBase
    {
        private readonly IFConsumerClientFactory fConsumerClientFactory;
        private readonly IFPublish fPublish;
        public FanoutController(IFConsumerClientFactory fConsumerClientFactory,IFPublish fPublish)
        {
            this.fConsumerClientFactory = fConsumerClientFactory;
            this.fPublish = fPublish;
        }
        // GET: api/Fanout
        [HttpGet]
        public void Get()
        {
            var fanoutConsumer = fConsumerClientFactory.Create();

            fanoutConsumer.ConsumerReceived += (sender, e) =>
            {
                Console.WriteLine($"RoutingKey:{e.RoutingKey},Body:{ Encoding.UTF8.GetString(e.Body)}");
                Console.WriteLine();
            };

            fanoutConsumer.SubScribe();
            fanoutConsumer.Listening(new TimeSpan(), new System.Threading.CancellationToken());
        }

        // GET: api/Fanout/5
        [HttpGet("{id}", Name = "Get")]
        public async Task<bool> Get(int id)
        {
            return await fPublish.PublishAsync(3333, "MarmotExchangeFanout");
        }

        // POST: api/Fanout
        [HttpPost]
        public void Post([FromBody] string value)
        {
        }

        // PUT: api/Fanout/5
        [HttpPut("{id}")]
        public void Put(int id, [FromBody] string value)
        {
        }

        // DELETE: api/ApiWithActions/5
        [HttpDelete("{id}")]
        public void Delete(int id)
        {
        }
    }
}
