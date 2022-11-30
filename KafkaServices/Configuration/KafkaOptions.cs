namespace KafkaServices.Configuration
{
    public class KafkaOptions
    {
        public const string SectionName = "Kafka";
        public string BootstrapServers { get; set; }
        public string SchemaRegistryUrl { get; set; }
    }
}
