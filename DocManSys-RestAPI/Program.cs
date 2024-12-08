using DocManSys_RestAPI.Log4Net;
using DocManSys_RestAPI.Mappings;
using DocManSys_RestAPI.Models;
using DocManSys_RestAPI.Services;
using Elastic.Clients.Elasticsearch;
using FluentValidation;
using FluentValidation.AspNetCore;


namespace DocManSys_RestAPI {
    public class Program {
        private static async Task EnsureIndexExistsAsync(IHost app) {
            var elasticClient = app.Services.GetRequiredService<ElasticsearchClient>();

            // Check if the index exists
            var indexExists = await elasticClient.Indices.ExistsAsync("documents");
            if (!indexExists.Exists) {
                // If the index doesn't exist, create it with mappings (if needed)
                await elasticClient.Indices.CreateAsync<Document>(c => c
                    .Index("documents")
                    .Mappings(m => m
                        .Properties(p => p
                            .Text(d => d.OcrText)
                            .Text(d => d.Title)
                            .Text(d => d.Author)
                        )
                    )
                );
                Console.WriteLine("Elasticsearch index 'documents' created.");
            }
        }

        private static async Task EnsureElasticsearchIsHealthyAsync(IHost app) {
            var elasticClient = app.Services.GetRequiredService<ElasticsearchClient>();
            int retryCount = 0;
            int maxRetries = 15;
            TimeSpan delay = TimeSpan.FromSeconds(6);

            while (retryCount < maxRetries) {
                try {
                    var healthResponse = await elasticClient.Cluster.HealthAsync();

                    if (healthResponse.IsValidResponse && healthResponse.Status != HealthStatus.Red) {
                        Console.WriteLine("Elasticsearch is healthy.");
                        return; // Elasticsearch is healthy, exit
                    }
                }
                catch (Exception ex) {
                    Console.WriteLine($"Error connecting to Elasticsearch: {ex.Message}");
                }

                retryCount++;
                Console.WriteLine($"Retrying Elasticsearch connection... Attempt {retryCount}/{maxRetries}");
                await Task.Delay(delay);
            }

            throw new Exception("Elasticsearch did not become healthy after several retries.");
        }

        public static void Main(string[] args) {
            var builder = WebApplication.CreateBuilder(args);
            
            //builder.Configuration.AddJsonFile("rest.appsettings.json", optional: false, reloadOnChange: true);
            
            //logging
            builder.Logging.ClearProviders();
            builder.Logging.AddProvider(new Log4NetProvider());

            //Fluent validation
            builder.Services.AddFluentValidationAutoValidation();
            builder.Services.AddValidatorsFromAssemblyContaining<DocumentValidator>();

            // CORS konfigurieren, um Anfragen von localhost:80 (WebUI) zuzulassen
            builder.Services.AddCors(options => {
                options.AddPolicy("AllowWebUI",
                    policy => {
                        policy.WithOrigins("http://localhost") // Die URL deiner Web-UI
                            .AllowAnyHeader()
                            .AllowAnyOrigin()
                            .AllowAnyMethod();
                    });
            });

            builder.Services.AddControllers();
            builder.Services.AddSingleton<IMinioClientService, MinioClientService>();
            builder.Services.AddSingleton<IMessageQueueService, MessageQueueService>();
            builder.Services.AddHostedService<RabbitMqListenerService>();
            builder.Services.AddAutoMapper(typeof(MappingProfile));
            
            var elasticUri = builder.Configuration.GetConnectionString("ElasticSearch") ?? "http://elasticsearch:9200";
            builder.Services.AddSingleton(new ElasticsearchClient(new Uri(elasticUri)));
            
            builder.Services.AddControllers();

            //mapper
            // Add services to the container.


            //builder.Services.AddControllers();
            // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen(c => {
                var xmlFile = $"{System.Reflection.Assembly.GetExecutingAssembly().GetName().Name}.xml";
                var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);

                c.IncludeXmlComments(xmlPath);
            });

            builder.Services.AddHttpClient("DocManSys-DAL",
                client => { client.BaseAddress = new Uri("http://docmansys-dal:8082"); });

            var app = builder.Build();
            Task.Run(async () => await EnsureElasticsearchIsHealthyAsync(app)).GetAwaiter().GetResult();
            Task.Run(async () => await EnsureIndexExistsAsync(app)).GetAwaiter().GetResult();

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment()) {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            app.UseCors("AllowWebUI");

            app.UseHttpsRedirection();

            //app.Urls.Add("http://*:8081");
            app.UseAuthorization();

            app.MapControllers();

            app.Run();
        }
    }
}