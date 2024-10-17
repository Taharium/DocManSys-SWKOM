
using DocManSys_RestAPI.Models;
using Microsoft.EntityFrameworkCore;
using PaperlessRest.Log4Net;

namespace DocManSys_RestAPI {
    public class Program {
        public static void Main(string[] args) {
            var builder = WebApplication.CreateBuilder(args);
            builder.Logging.ClearProviders();
            builder.Logging.AddProvider(new Log4NetProvider());

            // CORS konfigurieren, um Anfragen von localhost:80 (WebUI) zuzulassen
            builder.Services.AddCors(options =>
            {
                options.AddPolicy("AllowWebUI",
                    policy =>
                    {
                        policy.WithOrigins("http://localhost") // Die URL deiner Web-UI
                            .AllowAnyHeader()
                            .AllowAnyOrigin()
                            .AllowAnyMethod();
                    });
            });

            // Add services to the container.

            builder.Services.AddControllers();

            // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen(c =>
            {
                var xmlFile = $"{System.Reflection.Assembly.GetExecutingAssembly().GetName().Name}.xml";
                var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);

                c.IncludeXmlComments(xmlPath);
            });

            builder.Services.AddHttpClient("DocManSys-DAL", client => {
                client.BaseAddress = new Uri("http://docmansys-dal:8082");
            });

            var app = builder.Build();

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
