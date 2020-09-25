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
    internal class DeleteMetricsListener : IPreDeleteEventListener, IPostDeleteEventListener
    {
        private readonly ISummaryCollection _summaryCollection;
        private readonly StopwatchCollection _stopwatchCollection;
        private readonly NHibernateMetricsOptions _options;

        public DeleteMetricsListener(ISummaryCollection summaryCollection, StopwatchCollection stopwatchCollection, NHibernateMetricsOptions options)
        {
            _summaryCollection = summaryCollection;
            _stopwatchCollection = stopwatchCollection;
            _options = options;
        }

        public void OnPostDelete(PostDeleteEvent @event)
        {
            var elapsedTime = _stopwatchCollection[@event.Id.ToString()];
            _summaryCollection.Observe($"{_options.Prefix}_{NHibernateMetricsEnumeration.SqlStatementExecuteDuration.Name}", elapsedTime.TotalMilliseconds, ((ISession)@event.Session).Connection.Database, @event.Persister.EntityMetamodel.Type.Namespace, @event.Persister.EntityMetamodel.Type.Name, "DELETE", string.Empty);
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
