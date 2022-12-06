using Confluent.SchemaRegistry;
using KafkaSerialisation.Configuration;
using KafkaSerialisation.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var kafkaOptions = builder.Configuration.GetSection(KafkaOptions.SectionName);
builder.Services.Configure<KafkaOptions>(kafkaOptions);

var schemaRegistryConfig = new SchemaRegistryConfig
{
    Url = kafkaOptions.GetValue<string>("SchemaRegistryUrl")
};
builder.Services.AddSingleton<CachedSchemaRegistryClient>(new CachedSchemaRegistryClient(schemaRegistryConfig));

builder.Services.AddTransient<IKafkaJsonService, KafkaJsonService>();
builder.Services.AddTransient<IKafkaProtoService, KafkaProtoService>();
builder.Services.AddTransient<IKafkaService, KafkaService>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.DisplayRequestDuration();
    });
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();