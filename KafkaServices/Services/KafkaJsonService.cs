using Confluent.Kafka;
using Confluent.SchemaRegistry;
using Confluent.SchemaRegistry.Serdes;
using KafkaServices.Configuration;
using Microsoft.Extensions.Options;

namespace KafkaServices.Services
{
    public interface IKafkaJsonService
    {
        Task ProduceJsonMessage<TKey, TValue>(string topicName, Message<TKey, TValue> message) where TValue : class;
    }

    public class KafkaJsonService : IKafkaJsonService
    {
        private readonly KafkaOptions _kafkaOptions;
        private readonly SchemaRegistryConfig _schemaRegistryConfig;
        private readonly ProducerConfig _producerConfig;

        public KafkaJsonService(IOptions<KafkaOptions> kafkaOptions)
        {
            _kafkaOptions    = kafkaOptions.Value;
            _schemaRegistryConfig = new SchemaRegistryConfig { Url = _kafkaOptions.SchemaRegistryUrl };
            _producerConfig  = new ProducerConfig { BootstrapServers = _kafkaOptions.BootstrapServers };
        }

        public async Task ProduceJsonMessage<TKey, TValue>(string topicName, Message<TKey, TValue> message) where TValue : class
        {
            using (var schemaRegistry = new CachedSchemaRegistryClient(_schemaRegistryConfig))
            using (var producer = new ProducerBuilder<TKey, TValue>(_producerConfig)
                    .SetValueSerializer(new JsonSerializer<TValue>(schemaRegistry))
                    .Build())
            {
                await producer.ProduceAsync(topicName, message);
            }
        }
    }
}
