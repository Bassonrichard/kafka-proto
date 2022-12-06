using BenchmarkWorker;
using Confluent.SchemaRegistry;
using KafkaSerialisation.Configuration;
using KafkaSerialisation.Services;

IHost host = Host.CreateDefaultBuilder(args)
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

        services.AddHostedService<Worker>();
    })
    .Build();

host.Run();
