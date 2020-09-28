using Netension.Monitoring.Core.Diagnostics;
using Netension.Monitoring.Prometheus;
using Netension.NHibernate.Prometheus.Enumerations;
using Netension.NHibernate.Prometheus.Options;
using NHibernate;
using NHibernate.Event;
using System.Threading;
using System.Threading.Tasks;
using ILoggerFactory = Microsoft.Extensions.Logging.ILoggerFactory;
using Microsoft.Extensions.Logging;
using System;

namespace Netension.NHibernate.Prometheus.Listeners
{
    internal class DeleteMetricsListener : IPreDeleteEventListener, IPostDeleteEventListener
    {
        private const string OPERATION = "DELETE";
        private readonly ISummaryCollection _summaryCollection;
        private readonly StopwatchCollection _stopwatchCollection;
        private readonly NHibernateMetricsOptions _options;
        private readonly ILogger<DeleteMetricsListener> _logger;

        public DeleteMetricsListener(ISummaryCollection summaryCollection, StopwatchCollection stopwatchCollection, NHibernateMetricsOptions options, ILoggerFactory loggerFactory)
        {
            _summaryCollection = summaryCollection;
            _stopwatchCollection = stopwatchCollection;
            _options = options;
            _logger = loggerFactory.CreateLogger<DeleteMetricsListener>();
        }

        public void OnPostDelete(PostDeleteEvent @event)
        {
            var elapsedTime = _stopwatchCollection[@event.Id.ToString()];
            try
            {
                _summaryCollection.Observe($"{_options.Prefix}_{NHibernateMetricsEnumeration.SqlStatementExecuteDuration.Name}", elapsedTime.TotalMilliseconds, ((ISession)@event.Session)?.Connection?.Database ?? "UNKNOW", @event?.Persister?.EntityMetamodel?.Type?.Namespace ?? "UNKNOW", @event?.Persister?.EntityMetamodel?.Type?.Name ?? "UNKNOW", OPERATION, string.Empty);
            }
            catch (InvalidOperationException)
            {
                _logger.LogWarning("{metric} does not exist.", $"{_options.Prefix}_{NHibernateMetricsEnumeration.SqlStatementExecuteDuration.Name}");
            }
        }

        public async Task OnPostDeleteAsync(PostDeleteEvent @event, CancellationToken cancellationToken)
        {
            await Task.Run(() => OnPostDelete(@event), cancellationToken);
        }

        public bool OnPreDelete(PreDeleteEvent @event)
        {
            _stopwatchCollection.Start(@event.Id.ToString());
            return false;
        }

        public async Task<bool> OnPreDeleteAsync(PreDeleteEvent @event, CancellationToken cancellationToken)
        {
            return await Task.Run(() => OnPreDelete(@event), cancellationToken);
        }
    }
}
