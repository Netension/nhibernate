using Microsoft.Extensions.Logging;
using Netension.Monitoring.Core.Diagnostics;
using Netension.Monitoring.Prometheus.Containers;
using Netension.NHibernate.Prometheus.Enumerations;
using Netension.NHibernate.Prometheus.Listeners;
using Netension.NHibernate.Prometheus.Options;
using NHibernate.Cfg;
using NHibernate.Event;
using System.Collections.Generic;
using System.Linq;
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

        public NHibernateMetricsBuilder AddCounterMetric(string name, string description)
        {
            return AddCounterMetric(name, description, Enumerable.Empty<string>());
        }

        public NHibernateMetricsBuilder AddCounterMetric(string name, string description, IEnumerable<string> labels)
        {
            var fullName = $"{_options.Prefix}_{name}";
            _logger.LogDebug("Register {name} {type} metrics.", fullName, "Counter");

            _metricsCollection.RegisterCounter(fullName, description, labels);

            return this;
        }

        public NHibernateMetricsBuilder AddGaugeMetric(string name, string description)
        {
            return AddGaugeMetric(name, description, Enumerable.Empty<string>());
        }

        public NHibernateMetricsBuilder AddGaugeMetric(string name, string description, IEnumerable<string> labels)
        {
            var fullName = $"{_options.Prefix}_{name}";
            _logger.LogDebug("Register {name} {type} metrics.", fullName, "Gauge");

            _metricsCollection.RegisterGauge(fullName, description, labels);

            return this;
        }

        public NHibernateMetricsBuilder AddHistogramMetric(string name, string description)
        {
            return AddHistogramMetric(name, description, Enumerable.Empty<double>(), Enumerable.Empty<string>());
        }

        public NHibernateMetricsBuilder AddHistogramMetric(string name, string description, IEnumerable<string> labels)
        {
            return AddHistogramMetric($"{_options.Prefix}_{name}", description, Enumerable.Empty<double>(), labels);
        }

        public NHibernateMetricsBuilder AddHistogramMetric(string name, string description, IEnumerable<double> buckets)
        {
            return AddHistogramMetric($"{_options.Prefix}_{name}", description, buckets, Enumerable.Empty<string>());
        }

        public NHibernateMetricsBuilder AddHistogramMetric(string name, string description, IEnumerable<double> buckets, IEnumerable<string> labels)
        {
            var fullName = $"{_options.Prefix}_{name}";
            _logger.LogDebug("Register {name} {type} metrics with.", fullName, "Histogram");

            _metricsCollection.RegisterHistogram(fullName, description, buckets, labels);

            return this;
        }

        public NHibernateMetricsBuilder AddSummaryMetric(string name, string description)
        {
            return AddSummaryMetric(name, description, Enumerable.Empty<string>());
        }

        public NHibernateMetricsBuilder AddSummaryMetric(string name, string description, IEnumerable<string> labels)
        {
            var fullName = $"{_options.Prefix}_{name}";
            _logger.LogDebug("Register {name} {type} metrics with.", fullName, "Counter");

            _metricsCollection.RegisterSummary(fullName, description, labels);

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
