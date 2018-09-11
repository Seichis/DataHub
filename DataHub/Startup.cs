using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using DataHub.Entities;
using DataHub.Repositories;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using RepositoryFramework.EntityFramework;
using RepositoryFramework.Interfaces;
using Swashbuckle.AspNetCore.Swagger;

namespace DataHub
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
            services.AddScoped(
                typeof(DbContext),
                sp =>
                {
                    var context = new LocalDBContext();
                    Seed(context);
                    return context;
                });

            services.AddScoped(
                typeof(IEntitiesRepository),
                sp => new EntitiesRepository());

            services.AddScoped(
                typeof(IBlobRepository),
                sp => new BlobRepository());

            services.AddScoped(
                typeof(IQueryableRepository<Models.FileInfo>),
                sp => new EntityFrameworkRepository<Models.FileInfo>(sp.GetService<DbContext>()));

            services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_2_1);
            // Register the Swagger generator, defining 1 or more Swagger documents
            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new Info
                {
                    Version = "v1",
                    Title = "DataHub API",
                    Description = "A simple DataHub API",
                    TermsOfService = "None",
                    Contact = new Contact
                    {
                        Name = "HT/KM",
                        Email = string.Empty,
                        
                    },
                    License = new License
                    {
                        Name = "",
                        Url = ""
                    }
                });
                var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
                var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
                c.IncludeXmlComments(xmlPath);
            });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            // Enable middleware to serve generated Swagger as a JSON endpoint.
            app.UseSwagger();

            // Enable middleware to serve swagger-ui (HTML, JS, CSS, etc.), 
            // specifying the Swagger JSON endpoint.
            app.UseSwaggerUI(c =>
            {
                c.SwaggerEndpoint("/swagger/v1/swagger.json", "DataHub V1");
                c.RoutePrefix = string.Empty;
            });
            app.UseMvc();
        }
        private void Seed(LocalDBContext dbContext)
        {
            dbContext.Database.EnsureDeleted();
            dbContext.Database.EnsureCreated();
            var ts1 = dbContext.TimeSeries.Add(new TimeSeries
            {
                Id = "1",
                Source = "Hist",
                TimeSeriesTagId = "ABC123",
                Timestamp = DateTime.Now,
                Value = "123.2345",
                Tag = new TimeSeriesTag
                {
                    Id = "ABC123",
                    Name = "Some tag",
                    Units = "Pa"
                }
            });
            var ts2 = dbContext.TimeSeries.Add(new TimeSeries
            {
                Id = "2",
                Source = "Hist",
                TimeSeriesTagId = "ABC234",
                Timestamp = DateTime.Now,
                Value = "23.23456",
                Tag = new TimeSeriesTag
                {
                    Id = "ABC234",
                    Name = "Some other tag",
                    Units = "Pa"
                }
            });
            dbContext.Files.Add(
                new Models.FileInfo
                {
                    Entity = "SensorData",
                    Format = "CSV",
                    Filename = "ABC123.CSV",
                    Id = "1",
                    Source = "Hist"
                });
            dbContext.Files.Add(
                new Models.FileInfo
                {
                    Entity = "SensorData",
                    Format = "CSV",
                    Filename = "ABC124.CSV",
                    Id = "2",
                    Source = "Hist"
                });
            dbContext.FunctionalAssets.Add(
                new FunctionalAsset
                {
                    Id = "1",
                    Location = "Location 1",
                    Name = "Site 1",
                    TagNumber = "A1",
                    Assets = new List<Asset>
                    {
                        new FunctionalAsset
                        {
                            Id = "2",
                            Location = "Location 1",
                            Name = "Some part",
                            TagNumber = "A1-B1",
                            TimeSeriesTags = new List<TimeSeriesTag> { ts1.Entity.Tag },
                            Assets = new List<Asset>
                            {
                                new SerialAsset
                                {
                                    Id = "3",
                                    Name = "Some product",
                                    Producer = "Some producer",
                                    SerialNumber = "ABC-123-DEF-456-GHI-789"
                                }
                            }
                        },
                        new FunctionalAsset
                        {
                            Id = "4",
                            Location = "Location 1",
                            Name = "Some other part",
                            TagNumber = "A1-B2",
                            TimeSeriesTags = new List<TimeSeriesTag> { ts2.Entity.Tag },
                            Assets = new List<Asset>
                            {
                                new SerialAsset
                                {
                                    Id = "5",
                                    Name = "Some other product",
                                    Producer = "Some other producer",
                                    SerialNumber = "DEF-456-GHI-789-ABC-123"
                                }
                            }
                        }
                    }
                });
            dbContext.SaveChanges();
        }
    }
}
