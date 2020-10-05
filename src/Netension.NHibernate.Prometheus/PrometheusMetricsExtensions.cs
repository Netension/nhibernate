using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Netension.Monitoring.Core.Diagnostics;
using Netension.Monitoring.Prometheus;
using Netension.NHibernate.Prometheus.Builders;
using Netension.NHibernate.Prometheus.Interceptors;
using Netension.NHibernate.Prometheus.Listeners;
using Netension.NHibernate.Prometheus.Services;
using NHibernate.Cfg;
using NHibernate.Event;
using NHibernate.Hql.Ast.ANTLR.Tree;
using System.Diagnostics.CodeAnalysis;

namespace Netension.NHibernate.Prometheus
{
    [ExcludeFromCodeCoverage]
    public static class PrometheusMetricsExtensions
    {
        public static Configuration AddInsertMetricListener(this Configuration configuration, ISummaryManager summaryManager, ILoggerFactory loggerFactory)
        {
            var listener = new InsertMetricsListener(summaryManager, new StopwatchCollection(loggerFactory), loggerFactory);

            configuration.AppendListeners(ListenerType.PreInsert, new IPreInsertEventListener[] { listener });
            configuration.AppendListeners(ListenerType.PostInsert, new IPostInsertEventListener[] { listener });

            return configuration;
        }

        public static Configuration AddDeleteMetricListener(this Configuration configuration, ISummaryManager summaryManager, ILoggerFactory loggerFactory)
        {
            var listener = new DeleteMetricsListener(summaryManager, new StopwatchCollection(loggerFactory), loggerFactory);

            configuration.AppendListeners(ListenerType.PreDelete, new IPreDeleteEventListener[] { listener });
            configuration.AppendListeners(ListenerType.PostDelete, new IPostDeleteEventListener[] { listener });

            return configuration;
        }

        public static Configuration AddSelectMetricListener(this Configuration configuration, ISummaryManager summaryManager, ILoggerFactory loggerFactory)
        {
            var listener = new SelectMetricsListener(summaryManager, new StopwatchCollection(loggerFactory), loggerFactory);

            configuration.AppendListeners(ListenerType.PreLoad, new IPreLoadEventListener[] { listener });
            configuration.AppendListeners(ListenerType.PostLoad, new IPostLoadEventListener[] { listener });

            return configuration;
        }

        public static Configuration AddUpdateMetricListener(this Configuration configuration, ISummaryManager summaryManager, ILoggerFactory loggerFactory)
        {
            var listener = new UpdateMetricsListener(summaryManager, new StopwatchCollection(loggerFactory), loggerFactory);

            configuration.AppendListeners(ListenerType.PreUpdate, new IPreUpdateEventListener[] { listener });
            configuration.AppendListeners(ListenerType.PostUpdate, new IPostUpdateEventListener[] { listener });

            return configuration;
        }

        public static IServiceCollection AddRecordCounter<TEntity>(this IServiceCollection services)
            where TEntity : class
        {
            return services.AddHostedService<RecordCounterService<TEntity>>();
        }

        public static Configuration AddTransactionCountListener(this Configuration configuration, ICounterManager counterManager, ILoggerFactory loggerFactory)
        {
            return configuration.SetInterceptor(new NHibernateMetricsInterceptor(loggerFactory, counterManager));
        }

        public static NHibernateMetricsBuilder RegistrateNHibernateMetrics(this IPrometheusMetricsRegistry registry)
        {
            return new NHibernateMetricsBuilder(registry);
        }
    }
}
