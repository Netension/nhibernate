using Microsoft.Extensions.Logging;
using Netension.Monitoring.Prometheus;
using Netension.NHibernate.Prometheus.Enumerations;
using Netension.NHibernate.Prometheus.Options;
using NHibernate;
using ILoggerFactory = Microsoft.Extensions.Logging.ILoggerFactory;

namespace Netension.NHibernate.Prometheus.Interceptors
{
    public class NHibernateMetricsInterceptor : EmptyInterceptor
    {
        private readonly object _logger;
        private readonly ICounterManager _counterManager;
        private readonly NHibernateMetricsOptions _options;

        public NHibernateMetricsInterceptor(ILoggerFactory loggerFactory, ICounterManager counterManager, NHibernateMetricsOptions options)
        {
            _logger = loggerFactory.CreateLogger<NHibernateMetricsInterceptor>();
            _counterManager = counterManager;
            _options = options;
        }

        public override void AfterTransactionCompletion(ITransaction tx)
        {
            base.AfterTransactionCompletion(tx);

            if (tx == null) return;

            _counterManager.Increase($"{_options.Prefix}_{NHibernateMetricsEnumeration.TotalTransactionsCount.Name}", tx.WasCommitted ? "COMMIT" : tx.WasRolledBack ? "ROLLBACK" : "UNKNOWN");
        }
    }
}
