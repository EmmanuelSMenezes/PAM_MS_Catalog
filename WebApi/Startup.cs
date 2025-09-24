using Application.Service;
using Domain.Model;
using FluentValidation.AspNetCore;
using FluentValidation;
using Infrastructure.Repository;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using NSwag;
using NSwag.Generation.Processors.Security;
using Serilog;
using System.IO;
using System.Linq;
using System.Text;
using Application.Service.Interfaces;
using Infrastructure.Repository.Interfaces;
using Domain.Model.Product.Validation;
using Domain.Model.Product.Request;
using Domain.Model.Product.Image.Request;

namespace MS_Catalog
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

            services.AddMvc(option => option.EnableEndpointRouting = false);
            services.AddControllers().AddFluentValidation();
            services.AddControllers();
            services.AddCors();
            services.AddLogging();

            var key = Encoding.ASCII.GetBytes(Configuration.GetSection("MSCatalogSettings").GetSection("PrivateSecretKey").Value);
            services.AddAuthentication(x =>
            {
                x.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                x.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(x =>
            {
                x.RequireHttpsMetadata = false;
                x.SaveToken = true;
                x.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(key),
                    ValidateIssuer = false,
                    ValidateAudience = false
                };
            });

            // Add framework services.
            services.AddSwaggerDocument(config =>
            {
                config.PostProcess = document =>
                {
                    document.Info.Version = "V1";
                    document.Info.Title = "PAM - Microservice Catalog";
                    document.Info.Description = "API's Documentation of Microservice Catalog of PAM Plataform";
                };

                config.AddSecurity("JWT", Enumerable.Empty<string>(), new OpenApiSecurityScheme
                {
                    Type = OpenApiSecuritySchemeType.ApiKey,
                    Name = "Authorization",
                    In = OpenApiSecurityApiKeyLocation.Header,
                });

                config.OperationProcessors.Add(
                    new AspNetCoreOperationSecurityScopeProcessor("JWT"));
            });

            string logFilePath = Configuration.GetSection("LogSettings").GetSection("LogFilePath").Value;
            string logFileName = Configuration.GetSection("LogSettings").GetSection("LogFileName").Value;

            string connectionString = Configuration.GetSection("MSCatalogSettings").GetSection("ConnectionString").Value;
            string privateSecretKey = Configuration.GetSection("MSCatalogSettings").GetSection("PrivateSecretKey").Value;
            string tokenValidationMinutes = Configuration.GetSection("MSCatalogSettings").GetSection("TokenValidationMinutes").Value;

            HttpEndPoints httpEndPoints = new HttpEndPoints()
            {
                MSStorageBaseUrl = Configuration.GetSection("HttpEndPoints").GetSection("MSStorageBaseUrl").Value
            };


            services.AddSingleton((ILogger)new LoggerConfiguration()
              .MinimumLevel.Debug()
              .WriteTo.File(Path.Combine(logFilePath, logFileName), rollingInterval: RollingInterval.Day)
              .WriteTo.Console(Serilog.Events.LogEventLevel.Debug)
              .CreateLogger());

            services.AddScoped<ICategoryRepository, CategoryRepository>(
                provider => new CategoryRepository(connectionString, provider.GetService<ILogger>()));

           
            services.AddScoped<ICategoryService, CategoryService>(
                provider => new CategoryService(
                    provider.GetService<ICategoryRepository>(),
                    provider.GetService<ILogger>(),
                    privateSecretKey,
                    tokenValidationMinutes
                )
            );

            services.AddScoped<IProductPartnerRepository, ProductPartnerRepository>(
                 provider => new ProductPartnerRepository(connectionString, provider.GetService<ILogger>()));


            services.AddScoped<IProductPartnerService, ProductPartnerService>(
                provider => new ProductPartnerService(
                    provider.GetService<IProductPartnerRepository>(),
                    provider.GetService<ILogger>(),
                    privateSecretKey,
                    httpEndPoints,
                    tokenValidationMinutes
                )
            );

            services.AddScoped<IProductRepository, ProductRepository>(
                 provider => new ProductRepository(connectionString, provider.GetService<ILogger>()));

            services.AddScoped<IProductService, ProductService>(
                provider => new ProductService(
                    provider.GetService<IProductRepository>(),
                    provider.GetService<ILogger>(),
                    privateSecretKey,
                    httpEndPoints,
                    tokenValidationMinutes
                )
            );

            services.AddTransient<IValidator<CreateImageProductRequest>, CreateImageProductRequestValidator>();
            services.AddTransient<IValidator<CreateProductRequest>, CreateProductRequestValidator>();
            services.AddTransient<IValidator<UpdateProductRequest>, UpdateProductRequestValidator>();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
    
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseOpenApi();
            // add the Swagger generator and the Swagger UI middlewares   
            app.UseSwaggerUi3();

            app.UseCors(builder =>
                builder.AllowAnyHeader().AllowAnyMethod().AllowAnyOrigin());

            app.UseAuthentication();

            app.UseAuthorization();

            app.UseMvc();


        }
    }
}