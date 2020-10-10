using Microsoft.Extensions.Logging;
using Netension.Monitoring.Core.Diagnostics;
using Netension.Monitoring.Prometheus;
using Netension.NHibernate.Prometheus.Enumerations;
using Netension.NHibernate.Prometheus.Services;
using NHibernate;
using NHibernate.Event;
using System;
using System.Threading;
using System.Threading.Tasks;
using ILoggerFactory = Microsoft.Extensions.Logging.ILoggerFactory;

namespace Netension.NHibernate.Prometheus.Listeners
{
    internal class DeleteMetricsListener : IPreDeleteEventListener, IPostDeleteEventListener
    {
        private const string OPERATION = "DELETE";
        private readonly ISummaryManager _summaryManager;
        private readonly StopwatchCollection _stopwatchCollection;
        private readonly ILogger<DeleteMetricsListener> _logger;

        public DeleteMetricsListener(ISummaryManager summaryManager, StopwatchCollection stopwatchCollection, ILoggerFactory loggerFactory)
        {
            _summaryManager = summaryManager;
            _stopwatchCollection = stopwatchCollection;
            _logger = loggerFactory.CreateLogger<DeleteMetricsListener>();
        }

        public void OnPostDelete(PostDeleteEvent @event)
        {
            var elapsedTime = _stopwatchCollection[@event.Id.ToString()];
            try
            {
                _summaryManager.Observe(NamingService.GetFullName(NHibernateMetricsEnumeration.SqlStatementExecuteDuration.Name), elapsedTime.TotalMilliseconds, ((ISession)@event.Session)?.Connection?.Database ?? "UNKNOW", @event?.Persister?.EntityMetamodel?.Type?.Namespace ?? "UNKNOW", @event?.Persister?.EntityMetamodel?.Type?.Name ?? "UNKNOW", OPERATION);
            }
            catch (InvalidOperationException)
            {
                _logger.LogWarning("{metric} does not exist.", NamingService.GetFullName(NHibernateMetricsEnumeration.SqlStatementExecuteDuration.Name));
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
