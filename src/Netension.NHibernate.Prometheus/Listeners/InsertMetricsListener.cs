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
    internal class InsertMetricsListener : IPreInsertEventListener, IPostInsertEventListener
    {
        private readonly ISummaryCollection _summaryCollection;
        private readonly StopwatchCollection _stopwatchCollection;
        private readonly NHibernateMetricsOptions _options;

        public InsertMetricsListener(ISummaryCollection summaryCollection, StopwatchCollection stopwatchCollection, NHibernateMetricsOptions options)
        {
            _summaryCollection = summaryCollection;
            _stopwatchCollection = stopwatchCollection;
            _options = options;
        }

        public void OnPostInsert(PostInsertEvent @event)
        {
            var elapsedTime = _stopwatchCollection[@event.Id.ToString()];
            _summaryCollection.Observe($"{_options.Prefix}_{NHibernateMetricsEnumeration.SqlStatementExecuteDuration.Name}", elapsedTime.TotalMilliseconds, ((ISession)@event.Session).Connection.Database, @event.Persister.EntityMetamodel.Type.Namespace, @event.Persister.EntityMetamodel.Type.Name, "INSERT", string.Empty);
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
