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
    internal class SelectMetricsListener : IPreLoadEventListener, IPostLoadEventListener
    {
        private readonly ISummaryCollection _summaryCollection;
        private readonly StopwatchCollection _stopwatchCollection;
        private readonly NHibernateMetricsOptions _options;

        public SelectMetricsListener(ISummaryCollection summaryCollection, StopwatchCollection stopwatchCollection, NHibernateMetricsOptions options)
        {
            _summaryCollection = summaryCollection;
            _stopwatchCollection = stopwatchCollection;
            _options = options;
        }

        public void OnPostLoad(PostLoadEvent @event)
        {
            var elapsedTime = _stopwatchCollection[@event.Id.ToString()];
            _summaryCollection.Observe($"{_options.Prefix}_{NHibernateMetricsEnumeration.SqlStatementExecuteDuration.Name}", elapsedTime.TotalMilliseconds, ((ISession)@event.Session).Connection.Database, @event.Persister.EntityMetamodel.Type.Namespace, @event.Persister.EntityMetamodel.Type.Name, "SELECT", string.Empty);
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
