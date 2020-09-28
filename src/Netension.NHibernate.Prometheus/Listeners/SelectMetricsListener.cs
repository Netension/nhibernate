using Microsoft.Extensions.Logging;
using Netension.Monitoring.Core.Diagnostics;
using Netension.Monitoring.Prometheus;
using Netension.NHibernate.Prometheus.Enumerations;
using Netension.NHibernate.Prometheus.Options;
using NHibernate;
using NHibernate.Event;
using System;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using ILoggerFactory = Microsoft.Extensions.Logging.ILoggerFactory;

[assembly:InternalsVisibleTo("Netension.NHibernate.UnitTest")]
namespace Netension.NHibernate.Prometheus.Listeners
{
    internal class SelectMetricsListener : IPreLoadEventListener, IPostLoadEventListener
    {
        private const string OPERATION = "SELECT";
        private readonly ISummaryCollection _summaryCollection;
        private readonly StopwatchCollection _stopwatchCollection;
        private readonly NHibernateMetricsOptions _options;
        private readonly ILogger<SelectMetricsListener> _logger;

        public SelectMetricsListener(ISummaryCollection summaryCollection, StopwatchCollection stopwatchCollection, NHibernateMetricsOptions options, ILoggerFactory loggerFactory)
        {
            _summaryCollection = summaryCollection;
            _stopwatchCollection = stopwatchCollection;
            _options = options;
            _logger = loggerFactory.CreateLogger<SelectMetricsListener>();
        }

        public void OnPostLoad(PostLoadEvent @event)
        {
            var elapsedTime = _stopwatchCollection[@event.Id.ToString()];
            try
            {
                _summaryCollection.Observe($"{_options.Prefix}_{NHibernateMetricsEnumeration.SqlStatementExecuteDuration.Name}", elapsedTime.TotalMilliseconds, ((ISession)@event.Session)?.Connection?.Database ?? "UNKNOWN", @event.Persister?.EntityMetamodel?.Type?.Namespace ?? "UNKNOWN", @event.Persister?.EntityMetamodel?.Type?.Name ?? "UNKNOWN", OPERATION);
            }
            catch (InvalidOperationException)
            {
                _logger.LogWarning("{metric} does not exist.", $"{_options.Prefix}_{NHibernateMetricsEnumeration.SqlStatementExecuteDuration.Name}");
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
