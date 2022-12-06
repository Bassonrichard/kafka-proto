using Confluent.Kafka;
using Confluent.Kafka.SyncOverAsync;
using Confluent.SchemaRegistry;
using Confluent.SchemaRegistry.Serdes;
using Google.Protobuf;
using KafkaSerialisation.Configuration;
using Microsoft.Extensions.Options;

namespace KafkaSerialisation.Services
{
    public interface IKafkaProtoService
    {
        Task ProduceProtoMessage<TKey, TValue>(string topicName, Message<TKey, TValue> message) where TValue : IMessage<TValue>, new();
        Task ConsumeMessages<TKey, TValue>(string topicName, CancellationToken ct) where TValue : class, IMessage<TValue>, new();
    }

    public class KafkaProtoService : IKafkaProtoService
    {
        private readonly KafkaOptions _kafkaOptions;
        private readonly ProducerConfig _producerConfig;
        private readonly ConsumerConfig _consumerConfig;
        private readonly CachedSchemaRegistryClient _cachedSchemaRegistryClient;

        public KafkaProtoService(IOptions<KafkaOptions> kafkaOptions, CachedSchemaRegistryClient cachedSchemaRegistryClient)
        {
            _kafkaOptions = kafkaOptions.Value;
            _cachedSchemaRegistryClient = cachedSchemaRegistryClient;
            _producerConfig = new ProducerConfig { BootstrapServers = _kafkaOptions.BootstrapServers };
            _consumerConfig = new ConsumerConfig
            {
                BootstrapServers = _kafkaOptions.BootstrapServers,
                GroupId = "protobuf-example-consumer-group"
            };
        }

        public async Task ProduceProtoMessage<TKey, TValue>(string topicName, Message<TKey, TValue> message) where TValue : IMessage<TValue>, new()
        {
            using (var producer =
                new ProducerBuilder<TKey, TValue>(_producerConfig)
                    .SetValueSerializer(new ProtobufSerializer<TValue>(_cachedSchemaRegistryClient))
                    .Build())
            {
                await producer
                      .ProduceAsync(topicName, message)
                      .ContinueWith(task => task.IsFaulted
                          ? $"error producing message: {task.Exception.Message}"
                          : $"produced to: {task.Result.TopicPartitionOffset}");
            }
        }

        public async Task ConsumeMessages<TKey, TValue>(string topicName, CancellationToken ct) where TValue : class, IMessage<TValue>, new()
        {
            await Task.Run(() =>
            {
                using (var consumer =
                    new ConsumerBuilder<TKey, TValue>(_consumerConfig)
                        .SetValueDeserializer(new ProtobufDeserializer<TValue>().AsSyncOverAsync())
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
