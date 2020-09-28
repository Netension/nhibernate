using Microsoft.Extensions.Logging;
using Netension.Monitoring.Core.Diagnostics;
using Netension.Monitoring.Prometheus.Containers;
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
        private readonly ILogger<NHibernateMetricsBuilder> _logger;
        private readonly PrometheusMetricsCollection _metricsCollection;

        public NHibernateMetricsBuilder(Configuration configuration, NHibernateMetricsOptions options, ILoggerFactory loggerFactory)
        {
            _metricsCollection = PrometheusMetricsCollection.Instance;
            if (_metricsCollection == null) _metricsCollection = new PrometheusMetricsCollection(loggerFactory);

            _configuration = configuration;
            _options = options;
            _loggerFactory = loggerFactory;
            _logger = loggerFactory.CreateLogger<NHibernateMetricsBuilder>();
        }

        public NHibernateMetricsBuilder WithoutPrefix()
        {
            _options.Prefix = string.Empty;
            _logger.LogDebug("Do not use NHibernate metric's prefix.");

            return this;
        }

        public NHibernateMetricsBuilder WithPrefix(string prefix)
        {
            _options.Prefix = prefix;
            _logger.LogDebug("Set NHibernate metric's prefix to {prefix}.", prefix);

            return this;
        }

        public NHibernateMetricsBuilder AddInsertStatementsDurationMetric()
        {
            _logger.LogDebug("Register {operation} statements duration metric.", "INSERT");

            var listener = new InsertMetricsListener(_metricsCollection, new StopwatchCollection(_loggerFactory), _options);
            _configuration.AppendListeners(ListenerType.PreInsert, new IPreInsertEventListener[] { listener });
            _configuration.AppendListeners(ListenerType.PostInsert, new IPostInsertEventListener[] { listener });

            return  AddSummaryMetric($"{_options.Prefix}_{NHibernateMetricsEnumeration.SqlStatementExecuteDuration.Name}".TrimStart('_'), NHibernateMetricsEnumeration.SqlStatementExecuteDuration.Description, NHibernateMetricsEnumeration.SqlStatementExecuteDuration.Labels);
        }

        public NHibernateMetricsBuilder AddSelectStatementsDurationMetric()
        {
            _logger.LogDebug("Register {operation} statements duration metric.", "SELECT");

            var listener = new SelectMetricsListener(_metricsCollection, new StopwatchCollection(_loggerFactory), _options);
            _configuration.AppendListeners(ListenerType.PreLoad, new IPreLoadEventListener[] { listener });
            _configuration.AppendListeners(ListenerType.PostLoad, new IPostLoadEventListener[] { listener });

            return AddSummaryMetric($"{_options.Prefix}_{NHibernateMetricsEnumeration.SqlStatementExecuteDuration.Name}".TrimStart('_'), NHibernateMetricsEnumeration.SqlStatementExecuteDuration.Description, NHibernateMetricsEnumeration.SqlStatementExecuteDuration.Labels);
        }

        public NHibernateMetricsBuilder AddUpdateStatementsDurationMetric()
        {
            _logger.LogDebug("Register {operation} statements duration metric.", "UPDATE");

            var listener = new UpdateMetricsListener(_metricsCollection, new StopwatchCollection(_loggerFactory), _options);
            _configuration.AppendListeners(ListenerType.PreUpdate, new IPreUpdateEventListener[] { listener });
            _configuration.AppendListeners(ListenerType.PostUpdate, new IPostUpdateEventListener[] { listener });

            return AddSummaryMetric($"{_options.Prefix}_{NHibernateMetricsEnumeration.SqlStatementExecuteDuration.Name}".TrimStart('_'), NHibernateMetricsEnumeration.SqlStatementExecuteDuration.Description, NHibernateMetricsEnumeration.SqlStatementExecuteDuration.Labels);
        }

        public NHibernateMetricsBuilder AddDeleteStatementsDurationMetric()
        {
            _logger.LogDebug("Register {operation} statements duration metric.", "DELETE");

            var listener = new DeleteMetricsListener(_metricsCollection, new StopwatchCollection(_loggerFactory), _options);
            _configuration.AppendListeners(ListenerType.PreDelete, new IPreDeleteEventListener[] { listener });
            _configuration.AppendListeners(ListenerType.PostDelete, new IPostDeleteEventListener[] { listener });

            return AddSummaryMetric($"{_options.Prefix}_{NHibernateMetricsEnumeration.SqlStatementExecuteDuration.Name}".TrimStart('_'), NHibernateMetricsEnumeration.SqlStatementExecuteDuration.Description, NHibernateMetricsEnumeration.SqlStatementExecuteDuration.Labels);
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
