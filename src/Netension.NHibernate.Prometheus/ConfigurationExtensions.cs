using Microsoft.Extensions.Logging;
using Netension.NHibernate.Prometheus.Builders;
using Netension.NHibernate.Prometheus.Options;
using NHibernate.Cfg;
using System;

namespace Netension.NHibernate.Prometheus
{
    public static class ConfigurationExtensions
    {
        public static NHibernateMetricsBuilder AddNHibernateMetrics(this Configuration configuration, Action<NHibernateMetricsOptions> optionsAction, ILoggerFactory loggerFactory)
        {
            var options = new NHibernateMetricsOptions();
            optionsAction.Invoke(options);

            return new NHibernateMetricsBuilder(configuration, options, loggerFactory);
        }

        public static NHibernateMetricsBuilder AddNHibernateMetrics(this Configuration configuration, ILoggerFactory loggerFactory)
        {
            return configuration.AddNHibernateMetrics((options) => { options.Prefix = "nhibernate"; }, loggerFactory);
        }
    }
}
