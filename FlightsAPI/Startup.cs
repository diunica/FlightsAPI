using FlightsAPI.Services;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Quartz;
using System.Linq;

namespace FlightsAPI
{
    public class Startup
    {
        public IConfiguration Configuration { get; }
        private IWebHostEnvironment env;

        public Startup(IConfiguration configuration, IWebHostEnvironment env)
        {
            Configuration = configuration;
            this.env = env;
        }

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddMvc(options =>
                options.EnableEndpointRouting = false)
                .AddNewtonsoftJson(options =>
                  options.SerializerSettings.ContractResolver =
                    new Newtonsoft.Json.Serialization.DefaultContractResolver());

            services.AddQuartz(q =>
            {
                q.UseMicrosoftDependencyInjectionJobFactory();
            });

            services.AddSingleton<IFlightService, FlightService>();
            services.AddSingleton<ExchangeRatesService>();
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseCors(policy =>
            {
                string[] allowedCors = Configuration.GetSection("AllowedCors").GetChildren().Select(c => c.Value).ToArray();
                if (allowedCors.Length > 0)
                {
                    policy.WithOrigins(allowedCors);
                }
                policy.AllowAnyHeader();
                policy.AllowAnyMethod();
                policy.WithExposedHeaders("WWW-Authenticate", "Digest");
            });

            app.UseRouting();

            app.UseAuthentication();
            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
    }
}
