using Microsoft.Extensions.Logging;
using Netension.Monitoring.Core.Diagnostics;
using Netension.Monitoring.Prometheus;
using Netension.NHibernate.Prometheus.Builders;
using Netension.NHibernate.Prometheus.Listeners;
using NHibernate.Cfg;
using NHibernate.Event;
using System.Diagnostics.CodeAnalysis;

namespace Netension.NHibernate.Prometheus
{
    [ExcludeFromCodeCoverage]
    public static class MetricsRegistryExtensions
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

        public static NHibernateMetricsBuilder RegistrateNHibernateMetrics(this IPrometheusMetricsRegistry registry)
        {
            return new NHibernateMetricsBuilder(registry);
        }
    }
}
