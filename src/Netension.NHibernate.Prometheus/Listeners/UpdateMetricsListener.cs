using Netension.Monitoring.Core.Diagnostics;
using Netension.Monitoring.Prometheus;
using Netension.NHibernate.Prometheus.Enumerations;
using Netension.NHibernate.Prometheus.Options;
using NHibernate;
using NHibernate.Event;
using System.Threading;
using System.Threading.Tasks;

namespace Netension.NHibernate.Prometheus.Listeners
{
    internal class UpdateMetricsListener : IPreUpdateEventListener, IPostUpdateEventListener
    {
        private readonly ISummaryCollection _summaryCollection;
        private readonly StopwatchCollection _stopwatchCollection;
        private readonly NHibernateMetricsOptions _options;

        public UpdateMetricsListener(ISummaryCollection summaryCollection, StopwatchCollection stopwatchCollection, NHibernateMetricsOptions options)
        {
            _summaryCollection = summaryCollection;
            _stopwatchCollection = stopwatchCollection;
            _options = options;
        }

        public void OnPostUpdate(PostUpdateEvent @event)
        {
            var elapsedTime = _stopwatchCollection[@event.Id.ToString()];
            _summaryCollection.Observe($"{_options.Prefix}_{NHibernateMetricsEnumeration.SqlStatementExecuteDuration.Name}", elapsedTime.TotalMilliseconds, ((ISession)@event.Session).Connection.Database, @event.Persister.EntityMetamodel.Type.Namespace, @event.Persister.EntityMetamodel.Type.Name, "UPDATE", string.Empty);
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
