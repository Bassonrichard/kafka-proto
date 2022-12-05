using Confluent.Kafka;
using Confluent.Kafka.SyncOverAsync;
using Confluent.SchemaRegistry;
using Confluent.SchemaRegistry.Serdes;
using KafkaSerialisation.Configuration;
using Microsoft.Extensions.Options;

namespace KafkaSerialisation.Services
{
    public interface IKafkaJsonService
    {
        Task ProduceJsonMessage<TKey, TValue>(string topicName, Message<TKey, TValue> message) where TValue : class;
        Task ConsumeMessages<TKey, TValue>(string topicName, CancellationToken ct) where TValue : class;
    }

    public class KafkaJsonService : IKafkaJsonService
    {
        private readonly KafkaOptions _kafkaOptions;
        private readonly SchemaRegistryConfig _schemaRegistryConfig;
        private readonly ProducerConfig _producerConfig;
        private readonly ConsumerConfig _consumerConfig;
        private readonly CachedSchemaRegistryClient _cachedSchemaRegistryClient;

        public KafkaJsonService(IOptions<KafkaOptions> kafkaOptions)
        {
            _kafkaOptions = kafkaOptions.Value;
            _schemaRegistryConfig = new SchemaRegistryConfig { Url = _kafkaOptions.SchemaRegistryUrl};
            _cachedSchemaRegistryClient = new CachedSchemaRegistryClient(_schemaRegistryConfig);
            _producerConfig = new ProducerConfig { BootstrapServers = _kafkaOptions.BootstrapServers };
            _consumerConfig = new ConsumerConfig
            {
                BootstrapServers = _kafkaOptions.BootstrapServers,
                GroupId = "json-example-consumer-group"
            };
        }

        public async Task ProduceJsonMessage<TKey, TValue>(string topicName, Message<TKey, TValue> message) where TValue : class
        {
            using (var producer =
                new ProducerBuilder<TKey, TValue>(_producerConfig)
                    .SetValueSerializer(new JsonSerializer<TValue>(_cachedSchemaRegistryClient))
                    .Build())
            {
                await producer
                      .ProduceAsync(topicName, message)
                      .ContinueWith(task => task.IsFaulted
                          ? $"error producing message: {task.Exception.Message}"
                          : $"produced to: {task.Result.TopicPartitionOffset}");
            }
        }

        public async Task ConsumeMessages<TKey, TValue>(string topicName, CancellationToken ct) where TValue : class
        {
            await Task.Run(() =>
            {
                using (var consumer =
                    new ConsumerBuilder<TKey, TValue>(_consumerConfig)
                        .SetValueDeserializer(new JsonDeserializer<TValue>().AsSyncOverAsync())
                        .SetErrorHandler((_, e) => Console.WriteLine($"Error: {e.Reason}"))
                        .Build())
                {
                    consumer.Subscribe(topicName);

                    try
                    {
                        while (true)
                        {
                            try
                            {
                                var consumeResult = consumer.Consume(ct);
                                var message = consumeResult.Message.Value;
                                Console.WriteLine($"key: {consumeResult.Message.Key}");
                            }
                            catch (ConsumeException e)
                            {
                                Console.WriteLine($"Consume error: {e.Error.Reason}");
                            }
                        }
                    }
                    catch (OperationCanceledException)
                    {
                        consumer.Close();
                    }
                }
            });
        }
    }
}
