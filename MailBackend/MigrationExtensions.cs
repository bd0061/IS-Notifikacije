using System.Runtime.CompilerServices;
using MailBackend.Data;
using Microsoft.EntityFrameworkCore;

namespace MailBackend
{
    public static class MigrationExtensions
    {
        public static void ApplyMigration(this IApplicationBuilder app)
        {
            using IServiceScope scope = app.ApplicationServices.CreateScope();
            using AppDbContext context = scope.ServiceProvider.GetService<AppDbContext>();
            context.Database.Migrate();
        }
    }
}
