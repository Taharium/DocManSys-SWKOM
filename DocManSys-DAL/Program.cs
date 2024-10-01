
using DocManSys_DAL.Data;
using Microsoft.EntityFrameworkCore;

namespace DocManSys_DAL {
    public class Program {
        public static void Main(string[] args) {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.
            builder.Services.AddControllers();
            builder.Services.AddDbContext<DocumentContext>(options => 
                options.UseNpgsql(builder.Configuration.GetConnectionString("DataBaseConnection")));

            //Repositories registration

            var app = builder.Build();

            using (var scope = app.Services.CreateScope()) {
                var context = scope.ServiceProvider.GetRequiredService<DocumentContext>();

                try {
                    Console.WriteLine("Trying to connect to database...");

                    while (!context.Database.CanConnect()) {
                        Console.WriteLine("Wait for database to be ready...");
                        Thread.Sleep(1000); // Warte 1 Sekunde
                    }

                    Console.WriteLine("Connection to database is successful.");

                    context.Database.EnsureCreated();
                    Console.WriteLine("Database migrations successfully used.");
                } catch (Exception ex) {
                    Console.WriteLine($"Error at using Migrations: {ex.Message}");
                }
            }


            app.MapControllers();

            app.Run();
        }
    }
}
