
using System.Threading.RateLimiting;
using MailBackend.Background_Services;
using MailBackend.Data;
using MailBackend.Email;
using MailBackend.Middleware;
using MailBackend.Repositories;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;

namespace MailBackend
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);
            DotNetEnv.Env.Load();
            // Add services to the container.
            builder.Services.AddControllers();
            // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();

            builder.Services.AddRateLimiter(options =>
            {
                options.AddFixedWindowLimiter("FixedWindowPolicy", opt =>
                {
                    opt.Window = TimeSpan.FromMinutes(10);
                    opt.PermitLimit = 25;
                    opt.QueueLimit = 10;
                    opt.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
                });
            });

            builder.Services.AddTransient<IRepository,SQLRepository>();
            builder.Services.AddTransient<IEmailService, MailkitEmailService>();
            builder.Services.AddHostedService<StudentCleanupService>();

            builder.Services.AddDbContext<AppDbContext>(options =>
            options.UseNpgsql
            (   
               $"Host={(Environment.GetEnvironmentVariable("POSTGRES_HOST") ?? ContainerNameResolver.Resolve("database"))};" + 
               $"Port={Convert.ToInt32(Environment.GetEnvironmentVariable("POSTGRES_PORT"))};" + 
               $"Database={Environment.GetEnvironmentVariable("POSTGRES_DBNAME") ?? "mejl_baza"};Username={Environment.GetEnvironmentVariable("POSTGRES_USER")};" +
               $"Password={Environment.GetEnvironmentVariable("POSTGRES_PASSWORD")};Include Error Detail=true"
            ));
            var app = builder.Build();

            app.UseCors(builder => 
            {
                builder.AllowAnyOrigin();
                builder.AllowAnyHeader();
                builder.AllowAnyMethod();
            });
            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
                
                app.ApplyMigration();
            }

            app.UseRateLimiter();

            app.UseHttpsRedirection();

            app.UseAuthorization();
            app.UseWhen(context => context.Request.Path.StartsWithSegments("/api/MejlPoSiframa"), builder =>
            {
                builder.UseMiddleware<SecureEndpointMiddleware>();
            });
            app.MapControllers();
            app.Run();
        }
    }
}