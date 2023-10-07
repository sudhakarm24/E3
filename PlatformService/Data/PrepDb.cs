using Microsoft.EntityFrameworkCore;
using PlatformService.Models;

namespace PlatformService.Data
{
    public static class PrepDb
    {
        public static void PrepPopulation(IApplicationBuilder app, bool isProd)
        { 
           using(var serviceScope = app.ApplicationServices.CreateScope())
           {
               SeedData(serviceScope.ServiceProvider.GetService<AppDbContext>(), isProd);
           }
        }
        private static void SeedData(AppDbContext context, bool isProd)
        {
            if(isProd){
                Console.WriteLine("--> apply migrations data");
                try{
                context.Database.Migrate();
                }
                catch(Exception ex){
                   Console.WriteLine($"--> could not apply migrations data: {ex.Message}");
                }
                
            }
            if(!context.Platforms.Any())
            {
                Console.WriteLine("--> seeding data");
                context.Platforms.AddRange(
                    new Platform(){Name = "Dot Net", Cost = "Free", Publisher = "Microsoft"},
                    new Platform(){Name = "SQL server express", Cost = "Free", Publisher = "Microsoft"},
                    new Platform(){Name = "Kubernetes", Cost = "Free", Publisher = "Cloud Native Computing Foundation"}
                );
                context.SaveChanges();
            }
            else
            {
                Console.WriteLine("--> we already have data");
            }
        }
    }
}