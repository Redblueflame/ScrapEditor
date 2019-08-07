using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using ScrapEditor.LoginLogic;

namespace ScrapEditor
{
    public class Startup
    {
        public Startup(IConfiguration configurationService)
        {
            ConfigurationService = configurationService;
        }

        public IConfiguration ConfigurationService { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            var config = Configuration.LoadConfiguration("config.json");
            var api = new ScreenScraperAPI(config);
            var login = new LoginScreenScraper(api);
            var mvc = services.AddMvc();
            mvc.AddXmlSerializerFormatters();
            mvc.SetCompatibilityVersion(CompatibilityVersion.Version_2_2);
            services.AddSingleton(typeof(ILoginLogic), login);
            services.AddSwaggerDocument();
            
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
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            app.UseStaticFiles();
            app.UseHttpsRedirection();
            app.UseOpenApi();
            app.UseSwaggerUi3();
            
            app.UseMvc();
        }
    }
}