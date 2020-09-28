using Netension.Monitoring.Core.Diagnostics;
using Netension.Monitoring.Prometheus;
using Netension.NHibernate.Prometheus.Enumerations;
using Netension.NHibernate.Prometheus.Options;
using NHibernate;
using NHibernate.Event;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using ILoggerFactory=Microsoft.Extensions.Logging.ILoggerFactory;
using System;

namespace Netension.NHibernate.Prometheus.Listeners
{
    internal class UpdateMetricsListener : IPreUpdateEventListener, IPostUpdateEventListener
    {
        private const string OPERATION = "UPDATE";
        private readonly ISummaryCollection _summaryCollection;
        private readonly StopwatchCollection _stopwatchCollection;
        private readonly NHibernateMetricsOptions _options;
        private readonly ILogger<UpdateMetricsListener> _logger;

        public UpdateMetricsListener(ISummaryCollection summaryCollection, StopwatchCollection stopwatchCollection, NHibernateMetricsOptions options, ILoggerFactory loggerFactory)
        {
            _summaryCollection = summaryCollection;
            _stopwatchCollection = stopwatchCollection;
            _options = options;
            _logger = loggerFactory.CreateLogger<UpdateMetricsListener>();
        }

        public void OnPostUpdate(PostUpdateEvent @event)
        {
            var elapsedTime = _stopwatchCollection[@event.Id.ToString()];
            try
            {
                _summaryCollection.Observe($"{_options.Prefix}_{NHibernateMetricsEnumeration.SqlStatementExecuteDuration.Name}", elapsedTime.TotalMilliseconds, ((ISession)@event.Session)?.Connection?.Database ?? "UNKNOW", @event?.Persister?.EntityMetamodel?.Type?.Namespace ?? "UNKNOW", @event?.Persister?.EntityMetamodel?.Type?.Name ?? "UNKNOW", OPERATION);
            }
            catch (InvalidOperationException)
            {
                _logger.LogWarning("{metric} does not exist.", $"{_options.Prefix}_{NHibernateMetricsEnumeration.SqlStatementExecuteDuration.Name}");
            }
        }

        public async Task OnPostUpdateAsync(PostUpdateEvent @event, CancellationToken cancellationToken)
        {
            await Task.Run(() => OnPostUpdate(@event), cancellationToken);
        }

        public bool OnPreUpdate(PreUpdateEvent @event)
        {
            _stopwatchCollection.Start(@event.Id.ToString());
            return false;
        }

        public async Task<bool> OnPreUpdateAsync(PreUpdateEvent @event, CancellationToken cancellationToken)
        {
            return await Task.Run(() => OnPreUpdate(@event), cancellationToken);
        }
    }
}
