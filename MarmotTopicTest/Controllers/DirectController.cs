using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Marmot.Direct;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace MarmotTopicTest.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class DirectController : ControllerBase
    {
        private readonly IDConsumerClientFactory  dConsumerClientFactory;
        private readonly IDPublish dPublish;
        public DirectController(IDConsumerClientFactory dConsumerClientFactory, IDPublish dPublish)
        {
            this.dConsumerClientFactory = dConsumerClientFactory;
            this.dPublish = dPublish;
        }
        // GET: api/Direct
        [HttpGet]
        [Route("subscribe")]
        public void Get()
        {
            var directConsumer = dConsumerClientFactory.Create("MarmotExchangeDirect");//未自定义direct的消费者的queue名称时，自动创建的队列不会持久化消息

            directConsumer.ConsumerReceived += (sender, e) =>
            {
                var body = e.Body;
                var message = Encoding.UTF8.GetString(body);
                var routingKey = e.RoutingKey;
                Console.WriteLine(" [x] Received '{0}':'{1}'", routingKey, message);
            };

            directConsumer.SubScribe(new[] { "aaa", "bbb", "cc" });
            directConsumer.Listening(new TimeSpan(), new System.Threading.CancellationToken());
        }

        // GET: api/Direct/5
        [HttpGet]
        [Route("Publish")]
        public async Task<bool> Get(int id)
        {
            return await dPublish.PublishAsync(2333322,"aaa", "MarmotExchangeDirect");
        }

        // POST: api/Direct
        [HttpPost]
        public void Post([FromBody] string value)
        {
        }

        // PUT: api/Direct/5
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
