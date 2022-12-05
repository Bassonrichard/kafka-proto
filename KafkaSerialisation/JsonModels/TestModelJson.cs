namespace KafkaSerialisation.JsonModels
{
    public class TestModelJson
    {
        public int Int { get; set; }

        public long Long { get; set; }

        public float Float { get; set; }

        public double Double { get; set; }

        public decimal Decimal { get; set; }

        public string String { get; set; }

        public DateTime DateTime { get; set; }

        public BoolModelJson BoolModel { get; set; }

        public List<string> Strings { get; set; }
    }
}
