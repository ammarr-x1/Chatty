using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using PacmanMultiplayer.Hubs;
using PacmanMultiplayer.Services;
using Chatty.Data;
using Chatty.Services;
using Chatty.Hubs;
using System.Linq;
using Microsoft.AspNetCore.ResponseCompression;

namespace Chatty
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddRazorPages();
            services.AddServerSideBlazor();
            services.AddSingleton<ChatService>();
            services.AddScoped<UserSession>();
            services.AddSingleton<GameManager>();
            services.AddSingleton<GameLoopService>();
            services.AddHostedService<GameLoopService>();
            
            // Auth Architecture
            services.AddSingleton<MongoDbContext>();
            services.AddSingleton<Chatty.Data.IUserRepository, Chatty.Data.UserRepository>();
            services.AddSingleton<Chatty.Services.AuthService>();
            
            services.AddResponseCompression(opts => {
                opts.MimeTypes = ResponseCompressionDefaults.MimeTypes.Concat(new[] { "application/octet-stream" });
            });
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Error");
                app.UseHsts();
            }

            // app.UseHttpsRedirection(); // Disabled for local dev certificate issues
            app.UseStaticFiles();

            app.UseRouting();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapBlazorHub();
                endpoints.MapHub<ChatHub>("/chathub");
                endpoints.MapHub<GameHub>("/gamehub");
                endpoints.MapFallbackToPage("/_Host");
            });
        }
    }
}
