using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DataHub.Models;
using InfluxDB.Collector;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using RepositoryFramework.Interfaces;

namespace DataHub.Controllers
{
    [Route("[controller]")]
    public class MessagesController : Controller
    {
        private IRepository<Models.MessageInfo> messsagesRepository;

        public MessagesController(
            //IRepository<Models.MessageInfo> messsagesRepository
            )
        {
            //this.messsagesRepository = messsagesRepository;
        }

        /// <param name="id"></param>
        /// <returns></returns>
        [HttpGet("{id}")]
        public virtual async Task<IActionResult> Get([FromRoute]string id)
        {
            var message = await messsagesRepository.GetByIdAsync(id);
            if (message == null)
            {
                return NotFound($"No data found for message id {id}");
            }

            return Ok(message);
        }

        //[HttpPost]
        //public virtual async Task<IActionResult> Post([FromBody]Models.MessageRequest messageRequest)
        //{
        //    if (messageRequest == null)
        //    {
        //        return BadRequest("No message data");
        //    }

        //    var id = Guid.NewGuid().ToString();
        //    var messageInfo = new Models.MessageInfo
        //    {
        //        Id = id,
        //        Source = messageRequest.Source,
        //        Entity = messageRequest.Entity,
        //        Time = messageRequest.Time,
        //        Payload = messageRequest.Payload,
        //        Uri = this.BuildLink($"/messages/{id}")
        //    };
        //    //await messsagesRepository.CreateAsync(messageInfo);
        //    await SaveMessageAsync(messageInfo);
        //    return Ok(messageInfo);
        //}

        /// <summary>
        /// Saves a message according to the entity
        /// </summary>
        /// <param name="messageRequest"></param>
        /// <returns>A newly created Data point</returns>
        /// <response code="201">Returns the newly created item</response>
        /// <response code="400">If the item is null</response>            
        [HttpPost]
        [ProducesResponseType(201)]
        [ProducesResponseType(400)]
        [HttpPost]
        public virtual IActionResult Post([FromBody]Models.MessageRequest messageRequest)
        {
            if (messageRequest == null)
            {
                return BadRequest("No message data");
            }

            var id = Guid.NewGuid().ToString();
            var messageInfo = new Models.MessageInfo
            {
                Id = id,
                Source = messageRequest.Source,
                Entity = messageRequest.Entity,
                Time = messageRequest.Time,
                Payload = messageRequest.Payload,
                Uri = this.BuildLink($"/messages/{id}")
            };
            //await messsagesRepository.CreateAsync(messageInfo);
            SaveMessage(messageInfo);
            return Ok(messageInfo);
        }
        private void SaveMessage(MessageInfo messageInfo)
        {
            Metrics.Collector = new CollectorConfiguration()
                            .Tag.With("host", Environment.GetEnvironmentVariable("COMPUTERNAME"))
                            .Batch.AtInterval(TimeSpan.FromSeconds(2))
                            .WriteTo.InfluxDB("http://localhost:8086", "data")
                            .CreateCollector();
            for(int i = 0; i < 100000; i++)
            {
                Metrics.Increment("iterations");

                Metrics.Write("cpu_time",
                    new Dictionary<string, object>
                    {
                    { "value",33 },
                    { "user","km" }
                    });

                Metrics.Measure("working_set", "wset");
            }
           
            
        }
    }
}
