using BenchmarkWorker;
using KafkaServices.Configuration;
using KafkaServices.Services;

IHost host = Host.CreateDefaultBuilder(args)
    .ConfigureServices((hostContext, services) =>
    {
        services.Configure<KafkaOptions>(hostContext.Configuration.GetSection(KafkaOptions.SectionName));

        services.AddTransient<IKafkaJsonService, KafkaJsonService>();
        services.AddTransient<IKafkaProtoService, KafkaProtoService>();
        services.AddHostedService<Worker>();
    })
    .Build();

host.Run();
