using BenchmarkDotNet.Attributes;
using Confluent.Kafka;
using Confluent.SchemaRegistry;
using KafkaSerialisation.Configuration;
using KafkaSerialisation.JsonModels;
using KafkaSerialisation.Services;
using KafkaServices.ProtoModels;
using System.Text.Json;

namespace BenchmarkWorker
{
    [MemoryDiagnoser]
    [Orderer(BenchmarkDotNet.Order.SummaryOrderPolicy.FastestToSlowest)]
    [RankColumn]
    public class Benchmark
    {
        private IKafkaProtoService _kafkaProtoService;
        private IKafkaJsonService _kafkaJsonService;
        private IKafkaService _kafkaService;


        private Message<string, TestModelJson> _jsonMessage;
        private Message<string, TestModelProto> _protoMessage;
        private Message<string, string> _schemalessMessage;

        [GlobalSetup]
        public void GlobalSetup()
        {

            #region DI
            var host = Host.CreateDefaultBuilder()
            .ConfigureServices((hostContext, services) =>
            {
                var kafkaOptions = hostContext.Configuration.GetSection(KafkaOptions.SectionName);
                services.Configure<KafkaOptions>(kafkaOptions);

                var schemaRegistryConfig = new SchemaRegistryConfig
                {
                    Url = kafkaOptions.GetValue<string>("SchemaRegistryUrl")
                };
                services.AddSingleton<CachedSchemaRegistryClient>(new CachedSchemaRegistryClient(schemaRegistryConfig));

                services.AddTransient<IKafkaJsonService, KafkaJsonService>();
                services.AddTransient<IKafkaProtoService, KafkaProtoService>();
                services.AddTransient<IKafkaService, KafkaService>();
            })
            .Build();

            _kafkaJsonService = host.Services.GetRequiredService<IKafkaJsonService>();
            _kafkaProtoService = host.Services.GetRequiredService<IKafkaProtoService>();
            _kafkaService = host.Services.GetRequiredService<IKafkaService>();
            #endregion

            #region Json setup
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
                Key = Guid.NewGuid().ToString(),
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
                Key = Guid.NewGuid().ToString(),
                Value = protoModel
            };
            #endregion

            #region Schema less setup
            var schemalessModel = new TestModelJson
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

            _schemalessMessage = new Message<string, string>
            {
                Key = Guid.NewGuid().ToString(),
                Value = JsonSerializer.Serialize(schemalessModel)
            };
            #endregion
        }

        [Benchmark]
        public async Task JsonSerilization()
        {
            await _kafkaJsonService.ProduceJsonMessage("KafkaJson", _jsonMessage);
        }

        [Benchmark]
        public async Task ProtoSerilization()
        {
            await _kafkaProtoService.ProduceProtoMessage("KafkaProto", _protoMessage);

        }

        [Benchmark]
        public async Task SchemalessSerilization()
        {
            await _kafkaService.ProduceMessage("KafkaSchemaless", _schemalessMessage);

        }
    }
}
