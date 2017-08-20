using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Prometheus;
using Prometheus.Advanced;

namespace sample_app
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddMvc();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
            }

            app.UseStaticFiles();

            app.UseMvc(routes =>
            {
                routes.MapRoute(
                    name: "default",
                    template: "{controller=Home}/{action=Index}/{id?}");
            });

            var collectorRegistry = DefaultCollectorRegistry.Instance;
            collectorRegistry.RegisterOnDemandCollectors(new [] {
                new DotNetStatsCollector()
            });

            app.Map("/metrics", appConfig => {
                appConfig.Run(async context =>
                {
                    var acceptHeader = context.Request.Headers["Accept"];
                    var contentType = ScrapeHandler.GetContentType(acceptHeader);

                    context.Response.ContentType = contentType;

                    using (var outputStream = context.Response.Body)
                    {
                        ScrapeHandler.ProcessScrapeRequest(collectorRegistry.CollectAll(), 
                            contentType, outputStream);
                    }

                    await Task.FromResult(0).ConfigureAwait(false);
                });
            });
        }
    }
}
