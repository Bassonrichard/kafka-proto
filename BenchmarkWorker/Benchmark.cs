using BenchmarkDotNet.Attributes;
using Confluent.Kafka;
using KafkaServices.Configuration;
using KafkaServices.JsonModels;
using KafkaServices.ProtoModels;
using KafkaServices.Services;


namespace BenchmarkWorker
{
    [MemoryDiagnoser]
    [Orderer(BenchmarkDotNet.Order.SummaryOrderPolicy.FastestToSlowest)]
    [RankColumn]
    public class Benchmark
    {
        private IKafkaProtoService _kafkaProtoService;
        private IKafkaJsonService _kafkaJsonService;
        private Message<string, TestModelJson> _jsonMessage;
        private Message<string, TestModelProto> _protoMessage;

        [GlobalSetup]
        public void GlobalSetup()
        {

            #region DI
            var host = Host.CreateDefaultBuilder()
            .ConfigureServices((hostContext, services) =>
            {
                services.Configure<KafkaOptions>(hostContext.Configuration.GetSection(KafkaOptions.SectionName));
                services.AddTransient<IKafkaJsonService, KafkaJsonService>();
                services.AddTransient<IKafkaProtoService, KafkaProtoService>();
            })
            .Build();

            _kafkaJsonService = host.Services.GetRequiredService<IKafkaJsonService>();
            _kafkaProtoService = host.Services.GetRequiredService<IKafkaProtoService>();
            #endregion

            #region Json Setup
            var jsonModel = new TestModelJson
            {
                Int = 1,
                Long = 2,
                Float = 3,
                Double = 4,
                String = "string",
                DateTime = DateTime.UtcNow,
                BoolModel = new BoolModelJson
                {
                    Bool = true
                },
                Strings = new List<string>
                {
                    "string",
                    "string"
                }
            };

            _jsonMessage = new Message<string, TestModelJson>
            {
                Key = "KafkaJson",
                Value = jsonModel
            };
            #endregion

            #region Proto setup
            var protoModel = new TestModelProto
            {
                Int = 1,
                Long = 2,
                Float = 3,
                Double = 4,
                Decimal = new DecimalValue
                {
                    Units = 0,
                    Nanos = 0,
                },
                String = "string",
                DateTime = Google.Protobuf.WellKnownTypes.Timestamp.FromDateTime(DateTime.UtcNow),
                BoolModel = new BoolModelProto
                {
                    Bool = true
                }
            };

            protoModel.Strings.Add("string");
            protoModel.Strings.Add("string");

            _protoMessage = new Message<string, TestModelProto>
            {
                Key = "KafakProto",
                Value = protoModel
            };
            #endregion
        }

        [Benchmark]
        public async Task JsonSerilization()
        {
            await _kafkaJsonService.ProduceJsonMessage(Guid.NewGuid().ToString(), _jsonMessage);
        }

        [Benchmark]
        public async Task ProtoSerilization()
        {
            await _kafkaProtoService.ProduceProtoMessage(Guid.NewGuid().ToString(), _protoMessage);

        }
    }
}
