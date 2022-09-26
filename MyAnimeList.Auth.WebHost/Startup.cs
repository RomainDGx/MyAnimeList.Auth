using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;

namespace MyAnimeList.Auth.WebHost
{
    public class Startup
    {
        readonly IConfiguration _configuration;

        public Startup( IConfiguration configuration )
        {
            _configuration = configuration;
        }

        public void ConfigureServices( IServiceCollection services )
        {
            services.Configure<AuthService.AuthServiceOptions>( options =>
            {
                options.ClientId = _configuration.GetValue<string>( "AuthService:ClientId" );
                options.ClientSecret = _configuration.GetValue<string>( "AuthService:ClientSecret" );
                options.AuthServer = new Uri( _configuration.GetValue<string>( "AuthService:AuthServer" ) );
                options.RedirectUri = _configuration.GetValue<string>( "AuthService:RedirectUri" );
            } );
            services.AddScoped( typeof( AuthService ) );

            services.AddControllers();

            services.AddHttpClient();
        }

        public void Configure( IApplicationBuilder app, IWebHostEnvironment env )
        {
            if( env.IsDevelopment() )
            {
                app.UseDeveloperExceptionPage();
            }
            app.UseDefaultFiles();
            app.UseStaticFiles();
            app.UseRouting();
            app.UseEndpoints( endpoints =>
            {
                endpoints.MapControllers();
            } );
        }
    }
}
