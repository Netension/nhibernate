using Microsoft.Extensions.Logging;
using Netension.Monitoring.Core.Diagnostics;
using Netension.Monitoring.Prometheus;
using Netension.NHibernate.Prometheus.Enumerations;
using Netension.NHibernate.Prometheus.Options;
using NHibernate;
using NHibernate.Event;
using System;
using System.Threading;
using System.Threading.Tasks;
using ILoggerFactory = Microsoft.Extensions.Logging.ILoggerFactory;

namespace Netension.NHibernate.Prometheus.Listeners
{
    internal class InsertMetricsListener : IPreInsertEventListener, IPostInsertEventListener
    {
        private const string STATEMENT = "INSERT";
        private readonly ISummaryCollection _summaryCollection;
        private readonly StopwatchCollection _stopwatchCollection;
        private readonly NHibernateMetricsOptions _options;
        private readonly ILogger<InsertMetricsListener> _logger;

        public InsertMetricsListener(ISummaryCollection summaryCollection, StopwatchCollection stopwatchCollection, NHibernateMetricsOptions options, ILoggerFactory loggerFactory)
        {
            _summaryCollection = summaryCollection;
            _stopwatchCollection = stopwatchCollection;
            _options = options;
            _logger = loggerFactory.CreateLogger<InsertMetricsListener>();
        }

        public void OnPostInsert(PostInsertEvent @event)
        {
            var elapsedTime = _stopwatchCollection[@event.Id.ToString()];

            try
            {
                _summaryCollection.Observe($"{_options.Prefix}_{NHibernateMetricsEnumeration.SqlStatementExecuteDuration.Name}", elapsedTime.TotalMilliseconds, ((ISession)@event.Session)?.Connection?.Database ?? "UNKNOW", @event.Persister?.EntityMetamodel?.Type?.Namespace ?? "UNKNOW", @event.Persister?.EntityMetamodel?.Type?.Name ?? "UNKNOW", STATEMENT);
            }
            catch (InvalidOperationException)
            {
                _logger.LogError("{metric} does not exist.", $"{_options.Prefix}_{NHibernateMetricsEnumeration.SqlStatementExecuteDuration.Name}");
            }
        }

        public async Task OnPostInsertAsync(PostInsertEvent @event, CancellationToken cancellationToken)
        {
            await Task.Run(() => OnPostInsert(@event), cancellationToken);
        }

        public bool OnPreInsert(PreInsertEvent @event)
        {
            _stopwatchCollection.Start(@event.Id.ToString());
            return false;
        }

        public async Task<bool> OnPreInsertAsync(PreInsertEvent @event, CancellationToken cancellationToken)
        {
            return await Task.Run(() => OnPreInsert(@event), cancellationToken);
        }
    }
}
