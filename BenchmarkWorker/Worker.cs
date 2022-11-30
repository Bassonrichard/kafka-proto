using BenchmarkDotNet.Running;
using KafkaServices.Services;

namespace BenchmarkWorker
{
    public class Worker : BackgroundService
    {
        private readonly ILogger<Worker> _logger;

        public Worker(ILogger<Worker> logger, IKafkaProtoService kafkaProtoService, IKafkaJsonService kafkaJsonService)
        {
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            var summary = BenchmarkRunner.Run<Benchmark>();
        }
    }
}