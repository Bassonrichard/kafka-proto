using BenchmarkDotNet.Running;
using KafkaSerialisation.JsonModels;
using KafkaSerialisation.Services;
using KafkaServices.ProtoModels;
using System.Diagnostics;

namespace BenchmarkWorker
{
    public class Worker : BackgroundService
    {
        private readonly ILogger<Worker> _logger;
        private readonly IKafkaProtoService _kafkaProtoService;
        private readonly IKafkaJsonService _kafkaJsonService;
        private readonly IKafkaService _kafkaService;

        public Worker(ILogger<Worker> logger, IKafkaProtoService kafkaProtoService, IKafkaJsonService kafkaJsonService, IKafkaService kafkaService)
        {
            _logger = logger;
            _kafkaProtoService = kafkaProtoService;
            _kafkaJsonService = kafkaJsonService;
            _kafkaService = kafkaService;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {

            var summary = BenchmarkRunner.Run<Benchmark>();

            //var tasks = new List<Task>();
            //tasks.Add(Task.Run(async () => await _kafkaProtoService.ConsumeMessages<string, TestModelProto>("KafkaProto", stoppingToken)));
            //tasks.Add(Task.Run(async () => await _kafkaJsonService.ConsumeMessages<string, TestModelJson>("KafkaJson", stoppingToken)));
            //tasks.Add(Task.Run(async () => await _kafkaService.ConsumeMessages<string, TestModelJson>("KafkaSchemaless", stoppingToken)));
            //await Task.WhenAll(tasks);
        }
    }
}