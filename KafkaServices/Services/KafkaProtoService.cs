using Confluent.Kafka;
using Confluent.SchemaRegistry;
using Confluent.SchemaRegistry.Serdes;
using Google.Protobuf;
using KafkaServices.Configuration;
using Microsoft.Extensions.Options;

namespace KafkaServices.Services
{
    public interface IKafkaProtoService
    {
        Task ProduceProtoMessage<TKey, TValue>(string topicName, Message<TKey, TValue> message) where TValue : IMessage<TValue>, new();
    }

    public class KafkaProtoService : IKafkaProtoService
    {
        private readonly KafkaOptions _kafkaOptions;
        private readonly CachedSchemaRegistryClient _schemaRegistry;
        private readonly ProducerConfig _producerConfig;

        public KafkaProtoService(IOptions<KafkaOptions> kafkaOptions)
        {
            _kafkaOptions = kafkaOptions.Value;
            _schemaRegistry = new CachedSchemaRegistryClient(new SchemaRegistryConfig { Url = _kafkaOptions.SchemaRegistryUrl });
            _producerConfig = new ProducerConfig { BootstrapServers = _kafkaOptions.BootstrapServers };
        }

        public async Task ProduceProtoMessage<TKey, TValue>(string topicName, Message<TKey, TValue> message) where TValue : IMessage<TValue>, new()
        {
            using (var producer = new ProducerBuilder<TKey, TValue>(_producerConfig)
                    .SetValueSerializer(new ProtobufSerializer<TValue>(_schemaRegistry))
                    .Build())
            {
                await producer.ProduceAsync(topicName, message);
            }
        }
    }
}
