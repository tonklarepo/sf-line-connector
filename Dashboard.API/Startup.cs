using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Security.Principal;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using Auth;
using Dashboard.API.Extensions;
using Dashboard.Services.IService;
using Dashboard.Services.Service;
//using Database.Models;
using DateTimeExtensions;
using ErrorHandling;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Server.IISIntegration;
//using Microsoft.CodeAnalysis.Host;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json.Converters;

namespace Dashboard.API
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
            //CORS
            services.AddCors(options =>
            {
                options.AddPolicy("CorsPolicy",
                    builder => builder.AllowAnyOrigin()
                        .AllowAnyMethod()
                        .AllowAnyHeader());
                //.AllowCredentials());

            });
            
            services.AddControllers();
            
            //Swagger
            services.AddSwaggerDocumentation();

            //Database
            //services.AddDbContext<DatabaseContext>(options => options.UseSqlServer(Configuration["DBConnectionString"]));

            //Identity
            //services.AddIdentity<AppUser, IdentityRole>()
            //.AddEntityFrameworkStores<DatabaseContext>()
            //.AddDefaultTokenProviders();

            services.Configure<IdentityOptions>(options =>
            {
                //Password settings
                options.Password.RequireDigit = false;
                options.Password.RequiredLength = 0;
                options.Password.RequireNonAlphanumeric = false;
                options.Password.RequireUppercase = false;
                options.Password.RequireLowercase = false;
                options.Password.RequiredUniqueChars = 0;

                // Lockout settings
                options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(30);
                options.Lockout.MaxFailedAccessAttempts = 10;
                options.Lockout.AllowedForNewUsers = true;

                // User settings
                options.User.RequireUniqueEmail = false;
            });

            //JWT
            JwtSecurityTokenHandler.DefaultInboundClaimTypeMap.Clear();
            services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;

            })
                .AddJwtBearer(options =>
                {
                    options.RequireHttpsMetadata = false;
                    options.SaveToken = true;
                    //TODO: Edit token setting for production
                    var key = Encoding.ASCII.GetBytes("bmEUMALKHciPaiABmUJfahurtRYYKOoLDTGpVTBC");
                    options.TokenValidationParameters = new TokenValidationParameters
                    {

                        ValidateIssuer = false,
                        ValidateAudience = false,
                        ValidateLifetime = false,
                        ValidateIssuerSigningKey = false,
                        ValidIssuer = Configuration["jwt:issuer"],
                        ValidAudience = Configuration["jwt:issuer"],
                        //IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(Configuration["jwt:secretKey"]))
                        IssuerSigningKey = new SymmetricSecurityKey(key)
                    };
                });

            //ASP.Net
            
            //Dependency Injections
            services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();

            //Configuration injection
            services.AddSingleton<IConfiguration>(Configuration);

            //LINE CONNECTOR Services
            services.AddScoped<ILineService, LineService>();
            services.AddScoped<ISFService, SFService>();
            //services.AddScoped<IProductService, ProductService>();
            //services.AddScoped<IBillingService, BillingService>();
            //services.AddScoped<IConfigService, ConfigService>();
            //services.AddScoped<IFlowAccountService, FlowAccountService>();

            services.AddMvc()
                .AddJsonOptions(options =>
                {
                    options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
                    options.JsonSerializerOptions.Converters.Add(new JsonDateTimeConverter());
                });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            app.UseStaticFiles();
            //app.UseSession();
            app.UseRouting();
            app.UseCors("CorsPolicy");
            app.UseSwaggerDocumentation(Configuration);
            app.UseMiddleware<ErrorHandlingMiddleware>();
                        
            app.UseAuthentication();
            app.UseAuthorization();
            app.UseEndpoints(t => { t.MapControllers(); });
        }
    }
}
