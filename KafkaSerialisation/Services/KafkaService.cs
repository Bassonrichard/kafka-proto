using Confluent.Kafka;
using Confluent.SchemaRegistry.Serdes;
using Google.Protobuf;
using KafkaSerialisation.Configuration;
using Microsoft.Extensions.Options;

namespace KafkaSerialisation.Services
{
    public interface IKafkaService
    {
        Task ProduceMessage<TKey, TValue>(string topicName, Message<TKey, TValue> message) where TValue : class;
        Task ConsumeMessages<TKey, TValue>(string topicName, CancellationToken ct) where TValue : class;
    }

    public class KafkaService : IKafkaService
    {
        private readonly KafkaOptions _kafkaOptions;
        private readonly ProducerConfig _producerConfig;
        private readonly ConsumerConfig _consumerConfig;

        public KafkaService(IOptions<KafkaOptions> kafkaOptions)
        {
            _kafkaOptions = kafkaOptions.Value;
            _producerConfig = new ProducerConfig { BootstrapServers = _kafkaOptions.BootstrapServers };
            _consumerConfig = new ConsumerConfig
            {
                BootstrapServers = _kafkaOptions.BootstrapServers,
                GroupId = "example-consumer-group"
            };
        }

        public async Task ProduceMessage<TKey, TValue>(string topicName, Message<TKey, TValue> message) where TValue : class
        {
            using (var producer = new ProducerBuilder<TKey, TValue>(_producerConfig).Build())
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
