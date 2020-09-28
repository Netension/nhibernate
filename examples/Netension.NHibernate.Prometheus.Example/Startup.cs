using FluentNHibernate.Cfg;
using FluentNHibernate.Cfg.Db;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Netension.NHibernate.Prometheus.Example.Entities;
using NHibernate.Tool.hbm2ddl;
using Prometheus;

namespace Netension.NHibernate.Prometheus.Example
{
    public class Startup
    {
        // This method gets called by the runtime. Use this method to add services to the container.
        // For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddSingleton(provider =>
            {
                return Fluently.Configure()
                        .Database(SQLiteConfiguration.Standard.ConnectionString(@"Data Source=.\Database\example.db;Version=3;"))
                                        .Mappings(mapping => mapping.FluentMappings.AddFromAssemblyOf<ExampleEntity>())
                        .ExposeConfiguration(cfg =>
                        {
                            new SchemaExport(cfg).Create(false, true);

                            cfg.AddNHibernateMetrics(provider.GetService<ILoggerFactory>())
                                .AddBaseMetrics();
                        })
                        .BuildConfiguration()
                        .BuildSessionFactory()
                        .OpenSession();
            });

            services.AddHostedService<ExampleService>();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            app.UseRouting();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapMetrics();
            });
        }
    }
}
