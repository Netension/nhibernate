using Microsoft.Extensions.Logging;
using Netension.Monitoring.Prometheus;
using Netension.Monitoring.Prometheus.Containers;
using Netension.NHibernate.Prometheus.Builders;
using Netension.NHibernate.Prometheus.Options;
using NHibernate.Cfg;
using System;

namespace Netension.NHibernate.Prometheus
{
    public static class ConfigurationExtensions
    {
        public static NHibernateMetricsBuilder AddNHibernateMetrics(this Configuration configuration, Action<NHibernateMetricsOptions> optionsAction, ILoggerFactory loggerFactory, IPrometheusMetricsRegistry metricsRegistry, ISummaryCollection summaryCollection)
        {
            var options = new NHibernateMetricsOptions();
            optionsAction.Invoke(options);

            return new NHibernateMetricsBuilder(configuration, options, loggerFactory, metricsRegistry, summaryCollection);
        }

        public static NHibernateMetricsBuilder AddNHibernateMetrics(this Configuration configuration, ILoggerFactory loggerFactory, IPrometheusMetricsRegistry metricsRegistry, ISummaryCollection summaryCollection)
        {
            return configuration.AddNHibernateMetrics((options) => { options.Prefix = "nhibernate"; }, loggerFactory, metricsRegistry, summaryCollection);
        }
    }
}
