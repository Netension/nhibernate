using Microsoft.Extensions.Logging;
using Netension.Monitoring.Core.Diagnostics;
using Netension.Monitoring.Prometheus;
using Netension.NHibernate.Prometheus.Enumerations;
using Netension.NHibernate.Prometheus.Listeners;
using Netension.NHibernate.Prometheus.Options;
using NHibernate.Cfg;
using NHibernate.Event;
using ILoggerFactory = Microsoft.Extensions.Logging.ILoggerFactory;

namespace Netension.NHibernate.Prometheus.Builders
{
    public class NHibernateMetricsBuilder
    {
        private readonly Configuration _configuration;
        private readonly NHibernateMetricsOptions _options;
        private readonly ILoggerFactory _loggerFactory;
        private readonly ISummaryCollection _summaryCollection;
        private readonly IPrometheusMetricsRegistry _metricsRegistry;
        private readonly ILogger<NHibernateMetricsBuilder> _logger;

        public NHibernateMetricsBuilder(Configuration configuration, NHibernateMetricsOptions options, ILoggerFactory loggerFactory, IPrometheusMetricsRegistry metricsRegistry, ISummaryCollection summaryCollection)
        {
            _configuration = configuration;
            _options = options;
            _loggerFactory = loggerFactory;
            _summaryCollection = summaryCollection;
            _metricsRegistry = metricsRegistry;
            _logger = loggerFactory.CreateLogger<NHibernateMetricsBuilder>();
        }

        public NHibernateMetricsBuilder AddInsertStatementsDurationMetric()
        {
            _logger.LogDebug("Register {operation} statements duration metric.", "INSERT");

            var listener = new InsertMetricsListener(_summaryCollection, new StopwatchCollection(_loggerFactory), _options, _loggerFactory);
            _configuration.AppendListeners(ListenerType.PreInsert, new IPreInsertEventListener[] { listener });
            _configuration.AppendListeners(ListenerType.PostInsert, new IPostInsertEventListener[] { listener });

            _metricsRegistry.RegisterSummary($"{_options.Prefix}_{NHibernateMetricsEnumeration.SqlStatementExecuteDuration.Name}".TrimStart('_'), NHibernateMetricsEnumeration.SqlStatementExecuteDuration.Description, NHibernateMetricsEnumeration.SqlStatementExecuteDuration.Labels);

            return this;
        }

        public NHibernateMetricsBuilder AddSelectStatementsDurationMetric()
        {
            _logger.LogDebug("Register {operation} statements duration metric.", "SELECT");

            var listener = new SelectMetricsListener(_summaryCollection, new StopwatchCollection(_loggerFactory), _options, _loggerFactory);
            _configuration.AppendListeners(ListenerType.PreLoad, new IPreLoadEventListener[] { listener });
            _configuration.AppendListeners(ListenerType.PostLoad, new IPostLoadEventListener[] { listener });

            _metricsRegistry.RegisterSummary($"{_options.Prefix}_{NHibernateMetricsEnumeration.SqlStatementExecuteDuration.Name}".TrimStart('_'), NHibernateMetricsEnumeration.SqlStatementExecuteDuration.Description, NHibernateMetricsEnumeration.SqlStatementExecuteDuration.Labels);

            return this;
        }

        public NHibernateMetricsBuilder AddUpdateStatementsDurationMetric()
        {
            _logger.LogDebug("Register {operation} statements duration metric.", "UPDATE");

            var listener = new UpdateMetricsListener(_summaryCollection, new StopwatchCollection(_loggerFactory), _options, _loggerFactory);
            _configuration.AppendListeners(ListenerType.PreUpdate, new IPreUpdateEventListener[] { listener });
            _configuration.AppendListeners(ListenerType.PostUpdate, new IPostUpdateEventListener[] { listener });

            _metricsRegistry.RegisterSummary($"{_options.Prefix}_{NHibernateMetricsEnumeration.SqlStatementExecuteDuration.Name}".TrimStart('_'), NHibernateMetricsEnumeration.SqlStatementExecuteDuration.Description, NHibernateMetricsEnumeration.SqlStatementExecuteDuration.Labels);

            return this;
        }

        public NHibernateMetricsBuilder AddDeleteStatementsDurationMetric()
        {
            _logger.LogDebug("Register {operation} statements duration metric.", "DELETE");

            var listener = new DeleteMetricsListener(_summaryCollection, new StopwatchCollection(_loggerFactory), _options, _loggerFactory);
            _configuration.AppendListeners(ListenerType.PreDelete, new IPreDeleteEventListener[] { listener });
            _configuration.AppendListeners(ListenerType.PostDelete, new IPostDeleteEventListener[] { listener });

            _metricsRegistry.RegisterSummary($"{_options.Prefix}_{NHibernateMetricsEnumeration.SqlStatementExecuteDuration.Name}".TrimStart('_'), NHibernateMetricsEnumeration.SqlStatementExecuteDuration.Description, NHibernateMetricsEnumeration.SqlStatementExecuteDuration.Labels);

            return this;
        }

        public NHibernateMetricsBuilder AddBaseMetrics()
        {
            AddSelectStatementsDurationMetric()
                .AddInsertStatementsDurationMetric()
                .AddUpdateStatementsDurationMetric()
                .AddDeleteStatementsDurationMetric();

            return this;
        }
    }
}
