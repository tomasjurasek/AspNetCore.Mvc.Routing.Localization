using System.Globalization;
using AspNetCore.Mvc.Routing.Localization.Extensions;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Localization;
using Microsoft.AspNetCore.Localization.Routing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace LocalizedRoutingSample.Mvc
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
            services.AddControllersWithViews();

            services.AddLocalizedRouting();
            services.AddSingleton<LocalizedRoutingTranslationTransformer>();

            services.AddLocalization();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }
            app.UseHttpsRedirection();
            app.UseStaticFiles();

            app.UseRouting();

            var supportedCultures = new[]
            {
                new CultureInfo("cs-CZ"),
                new CultureInfo("en-US"),
            };

            var options = new RequestLocalizationOptions
            {
                DefaultRequestCulture = new RequestCulture("en-US"),
                SupportedCultures = supportedCultures,
                SupportedUICultures = supportedCultures
            };

            options.RequestCultureProviders.Clear();
            options.RequestCultureProviders.Insert(0, new RouteDataRequestCultureProvider() { RouteDataStringKey = "culture" });
            app.UseRequestLocalization(options);

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapDynamicControllerRoute<LocalizedRoutingTranslationTransformer>("{culture=en-US}/{controller=Home}/{action=Index}/{id?}");
                endpoints.MapControllerRoute(
                    name: "default",
                    pattern: "{culture=en-US}/{controller=Home}/{action=Index}/{id?}");
            });
        }
    }
}
