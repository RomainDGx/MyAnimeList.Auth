using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;

namespace MyAnimeList.Auth.WebHost
{
    public class Program
    {
        public static void Main( string[] args )
        {
            CreateWebHostBuilder( args )
                .Build()
                .Run();
        }

        public static IHostBuilder CreateWebHostBuilder( string[] args )
        {
            return Host.CreateDefaultBuilder( args )
                       .ConfigureWebHost( web =>
                       {
                           web.UseKestrel()
                              //.UseIISIntegration()
                              .UseStartup<Startup>();
                       } );
        }
    }
}
