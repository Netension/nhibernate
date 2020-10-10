using Microsoft.Extensions.Logging;
using Netension.Monitoring.Core.Diagnostics;
using Netension.Monitoring.Prometheus;
using Netension.NHibernate.Prometheus.Enumerations;
using Netension.NHibernate.Prometheus.Services;
using NHibernate;
using NHibernate.Event;
using System;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using ILoggerFactory = Microsoft.Extensions.Logging.ILoggerFactory;

[assembly: InternalsVisibleTo("Netension.NHibernate.UnitTest")]
namespace Netension.NHibernate.Prometheus.Listeners
{
    internal class SelectMetricsListener : IPreLoadEventListener, IPostLoadEventListener
    {
        private const string OPERATION = "SELECT";

        private readonly ISummaryManager _summaryManager;
        private readonly StopwatchCollection _stopwatchCollection;
        private readonly ILogger<SelectMetricsListener> _logger;

        public SelectMetricsListener(ISummaryManager summaryManager, StopwatchCollection stopwatchCollection, ILoggerFactory loggerFactory)
        {
            _summaryManager = summaryManager;
            _stopwatchCollection = stopwatchCollection;
            _logger = loggerFactory.CreateLogger<SelectMetricsListener>();
        }

        public void OnPostLoad(PostLoadEvent @event)
        {
            var elapsedTime = _stopwatchCollection[@event.Id.ToString()];
            try
            {
                _summaryManager.Observe(NamingService.GetFullName(NHibernateMetricsEnumeration.SqlStatementExecuteDuration.Name), elapsedTime.TotalMilliseconds, ((ISession)@event.Session)?.Connection?.Database ?? "UNKNOWN", @event.Persister?.EntityMetamodel?.Type?.Namespace ?? "UNKNOWN", @event.Persister?.EntityMetamodel?.Type?.Name ?? "UNKNOWN", OPERATION);
            }
            catch (InvalidOperationException)
            {
                _logger.LogWarning("{metric} does not exist.", NamingService.GetFullName(NHibernateMetricsEnumeration.SqlStatementExecuteDuration.Name));
            }
        }

        public void OnPreLoad(PreLoadEvent @event)
        {
            _stopwatchCollection.Start(@event.Id.ToString());
        }

        public async Task OnPreLoadAsync(PreLoadEvent @event, CancellationToken cancellationToken)
        {
            await Task.Run(() => OnPreLoad(@event), cancellationToken);
        }
    }
}
