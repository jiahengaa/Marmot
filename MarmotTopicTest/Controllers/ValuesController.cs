using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Marmot;
using Marmot.Topic;
using Microsoft.AspNetCore.Mvc;

namespace MarmotTopicTest.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ValuesController : ControllerBase
    {
        private readonly ITopicConsumerClientFactory topicConsumerClientFactory;
        private readonly IPublishMessageSender publishMessageSender;

        public ValuesController(ITopicConsumerClientFactory topicConsumerClientFactory, IPublishMessageSender publishMessageSender)
        {
            this.topicConsumerClientFactory = topicConsumerClientFactory;
            this.publishMessageSender = publishMessageSender;
        }
        // GET api/values
        [HttpGet]
        public void Get(string queueName = "queue1")
        {
            var topicConsumer =  topicConsumerClientFactory.Create("MarmotExchange", "topic", queueName, true, false, null);
            topicConsumer.ConsumerReceived = (sender, e) =>
            {
                Console.WriteLine($"Queue:{queueName},RoutingKey:{e.RoutingKey},Body:{ Encoding.UTF8.GetString(e.Body)}");
                Console.WriteLine();
                topicConsumer.Commit();
            };
            topicConsumer.SubScribe(new[] { "aaa", "ljh-publish", "FF.#" });
            topicConsumer.Listening(new TimeSpan() , new System.Threading.CancellationToken());

        }

        // GET api/values/5
        [HttpGet("{id}")]
        public ActionResult<string> Get(int id)
        {

            publishMessageSender.PublishAsync(new Person()
            {
                Id = "0",
                Name = "xiaoming"
            }, "MarmotExchange", "topic", "ljh-publish", true, false, null);


            publishMessageSender.PublishAsync(new Person()
            {
                Id = "0",
                Name = "xiaoming"
            }, "MarmotExchange", "topic", "aaa", true, false, null);

            publishMessageSender.PublishAsync(new Person()
            {
                Id = "1",
                Name = "xiaoming1"
            }, "MarmotExchange", "topic", "FF.933", true, false, null);


            return Ok("ok");
        }

        // POST api/values
        [HttpPost]
        public void Post([FromBody] string value)
        {
        }

        // PUT api/values/5
        [HttpPut("{id}")]
        public void Put(int id, [FromBody] string value)
        {
        }

        // DELETE api/values/5
        [HttpDelete("{id}")]
        public void Delete(int id)
        {
        }

        public class Person
        {
            public string Id { set; get; }
            public string Name { set; get; }
        }
    }
}
