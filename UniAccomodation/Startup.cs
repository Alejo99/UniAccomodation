using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using UniAccomodation.Data;
using Microsoft.EntityFrameworkCore;
using UniAccomodation.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Authentication;
using UniAccomodation.Infrastructure.Authentication;
using UniAccomodation.Infrastructure.Authorization;
using Microsoft.AspNetCore.Authorization;
using UniAccomodation.Configuration;

namespace UniAccomodation
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
            #region Config options

            //Config
            services.AddOptions();
            //Paging options
            services.Configure<MyPagingOptions>(Configuration.GetSection("Paging"));

            #endregion

            #region DBContext and repository

            //Application DB context, uses identity framework
            services.AddDbContext<UniAccomodationDbContext>(options =>
                options.UseSqlServer(Configuration.GetConnectionString("UniAccomodationDb")));
            //Advert repository
            services.AddScoped<IAdvertRepository, EFAdvertRepository>();

            #endregion

            #region Identity config

            //Identity with options
            services
                .AddIdentity<ApplicationUser, IdentityRole>(opts =>
                {
                    opts.User.RequireUniqueEmail = true;
                    opts.Password.RequiredLength = 6;
                    opts.Password.RequireNonAlphanumeric = false;
                    opts.Password.RequireLowercase = false;
                    opts.Password.RequireUppercase = false;
                    opts.Password.RequireDigit = false;
                })
                .AddEntityFrameworkStores<UniAccomodationDbContext>();

            #endregion
            
            #region Authorization config

            //Requirement config
            services.AddAuthorization(options =>
            {
                options.AddPolicy("CanAccessAdvert", policybuilder => 
                    policybuilder.AddRequirements(new IsAdvertOwnerRequirement()));
            });

            //Handler config
            services.AddSingleton<IAuthorizationHandler, IsAdvertOwnerAuthorizationHandler>();

            #endregion

            services.AddTransient<IClaimsTransformation, ClaimsTransformer>();

            //Cookie config
            services.ConfigureApplicationCookie(opts =>
            {
                // These are the default paths, in case we want to change them
                opts.LoginPath = "/Account/Login";
                opts.AccessDeniedPath = "/Account/AccessDenied";
            });

            services.AddMvc();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseStatusCodePages();
                app.UseBrowserLink();
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
            }

            app.UseStaticFiles();

            app.UseAuthentication();

            app.UseMvc(routes =>
            {
                routes.MapRoute(
                    name: "default",
                    template: "{controller=Home}/{action=Index}/{id?}");
            });
        }
    }
}
