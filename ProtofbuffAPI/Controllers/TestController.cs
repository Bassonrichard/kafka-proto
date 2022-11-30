using Confluent.Kafka;
using KafkaServices.JsonModels;
using KafkaServices.ProtoModels;
using KafkaServices.Services;
using Microsoft.AspNetCore.Mvc;

namespace ProtofbuffAPI.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class TestController : ControllerBase
    {
        private readonly ILogger<TestController> _logger;
        private readonly IKafkaProtoService _kafkaProtoService;
        private readonly IKafkaJsonService _kafkaJsonService;

        public TestController(ILogger<TestController> logger, IKafkaProtoService kafkaProtoService, IKafkaJsonService kafkaJsonService)
        {
            _logger = logger;
            _kafkaProtoService = kafkaProtoService;
            _kafkaJsonService = kafkaJsonService;
        }

        [HttpPost]
        [Route("Json")]
        public async Task<IActionResult> PostJson([FromBody] TestModelJson model)
        {
            _logger.LogInformation($"Model:{model}");
            var message = new Message<string, TestModelJson> 
            { 
                Key = "KafkaJson",
                Value = model
            };

            await _kafkaJsonService.ProduceJsonMessage("KafakJson", message);

            return Ok();
        }


        [HttpPost]
        [Route("Proto")]
        public async Task<IActionResult> PostProto([FromBody] TestModelProto model)
        {
            _logger.LogInformation($"Model:{model}");
            var message = new Message<string, TestModelProto>
            {
                Key = "KafakProto",
                Value = model
            };

            await _kafkaProtoService.ProduceProtoMessage("KafakProto", message);

            return Ok();
        }
    }
}