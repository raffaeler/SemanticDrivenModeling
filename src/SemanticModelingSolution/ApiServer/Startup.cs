using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Formatters;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.OpenApi.Models;

namespace ApiServer
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
            // load the metadata configuration written in appsettings.json
            var metadataSection = Configuration.GetSection("Metadata");
            services.Configure<MetadataConfiguration>(metadataSection);
            var metadataConfiguration = metadataSection.Get<MetadataConfiguration>();

            // add the metadataservice in the DI
            var metadataServiceInstance = new MetadataService(metadataConfiguration);
            services.AddSingleton(metadataServiceInstance);

            // configure a sample repository
            services.AddTransient(typeof(RepositoryService));

            services.AddControllers();
            services.ConfigureOptions<SemanticFormatterConfiguration>();    // input/output formatters


            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo { Title = "ApiServer", Version = "v1" });
                //c.SchemaFilterDescriptors.Add(new Swashbuckle.AspNetCore.SwaggerGen.FilterDescriptor()
                //{
                //    Type = typeof(SdmOpenApiFilter),
                //    Arguments = new[] { metadataServiceInstance },
                //}) ;

                //c.MapType(typeof(SimpleDomain1.Order), () => new OpenApiSchema()
                //{
                //    Type = "OnlineOrder",
                //    Properties = new Dictionary<string, OpenApiSchema>(),
                //});

                c.SchemaGeneratorOptions.CustomTypeMappings.Add(typeof(SimpleDomain1.Order), () => new OpenApiSchema()
                {
                    Type = "OnlineOrder",
                    Properties = new Dictionary<string, OpenApiSchema>()
                    {
                        { "Id", new OpenApiSchema() { Type = "Guid" } },
                        { "Description", new OpenApiSchema() { Type = "string" } },
                    }
                });

            });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseSwagger();
                app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "ApiServer v1"));
            }

            app.UseRouting();

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
    }
}
