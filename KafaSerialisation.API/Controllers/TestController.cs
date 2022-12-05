using Confluent.Kafka;
using KafkaSerialisation.JsonModels;
using KafkaSerialisation.Services;
using KafkaServices.ProtoModels;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json;

namespace KafkaSerialisation.API.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class TestController : ControllerBase
    {
        private readonly ILogger<TestController> _logger;
        private readonly IKafkaProtoService _kafkaProtoService;
        private readonly IKafkaJsonService _kafkaJsonService;
        private readonly IKafkaService _kafkaService;

        public TestController(ILogger<TestController> logger,
            IKafkaProtoService kafkaProtoService,
            IKafkaJsonService kafkaJsonService,
            IKafkaService kafkaService)
        {
            _logger = logger;
            _kafkaProtoService = kafkaProtoService;
            _kafkaJsonService = kafkaJsonService;
            _kafkaService = kafkaService;
        }

        [HttpPost]
        [Route("Schemaless")]
        public async Task<IActionResult> PostSchemaless([FromBody] TestModelJson model)
        {
            _logger.LogInformation($"Model:{model}");
            var message = new Message<string, string>
            {
                Key = Guid.NewGuid().ToString(),
                Value = JsonSerializer.Serialize(model)
            };

            await _kafkaService.ProduceMessage("KafakProto", message);

            return Ok();
        }

        [HttpPost]
        [Route("Json")]
        public async Task<IActionResult> PostJson([FromBody] TestModelJson model)
        {
            _logger.LogInformation($"Model:{model}");
            var message = new Message<string, TestModelJson>
            {
                Key = Guid.NewGuid().ToString(),
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
                Key = Guid.NewGuid().ToString(),
                Value = model
            };

            await _kafkaProtoService.ProduceProtoMessage("KafakProto", message);

            return Ok();
        }
    }
}