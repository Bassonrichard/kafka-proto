using BenchmarkWorker;
using KafkaSerialisation.Configuration;
using KafkaSerialisation.Services;

IHost host = Host.CreateDefaultBuilder(args)
    .ConfigureServices((hostContext, services) =>
    {
        services.Configure<KafkaOptions>(hostContext.Configuration.GetSection(KafkaOptions.SectionName));

        services.AddTransient<IKafkaJsonService, KafkaJsonService>();
        services.AddTransient<IKafkaProtoService, KafkaProtoService>();
        services.AddTransient<IKafkaService, KafkaService>();

        services.AddHostedService<Worker>();
    })
    .Build();

host.Run();
