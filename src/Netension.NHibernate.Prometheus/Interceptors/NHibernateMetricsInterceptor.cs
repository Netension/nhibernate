using Microsoft.Extensions.Logging;
using Netension.Monitoring.Prometheus;
using Netension.NHibernate.Prometheus.Enumerations;
using Netension.NHibernate.Prometheus.Services;
using NHibernate;
using ILoggerFactory = Microsoft.Extensions.Logging.ILoggerFactory;

namespace Netension.NHibernate.Prometheus.Interceptors
{
    public class NHibernateMetricsInterceptor : EmptyInterceptor
    {
        private readonly ILogger<NHibernateMetricsInterceptor> _logger;
        private readonly ICounterManager _counterManager;

        public NHibernateMetricsInterceptor(ILoggerFactory loggerFactory, ICounterManager counterManager)
        {
            _logger = loggerFactory.CreateLogger<NHibernateMetricsInterceptor>();
            _counterManager = counterManager;
        }

        public override void AfterTransactionCompletion(ITransaction tx)
        {
            base.AfterTransactionCompletion(tx);

            if (tx == null) return;

            _counterManager.Increase(NamingService.GetFullName(NHibernateMetricsEnumeration.TotalTransactionsCount.Name), tx.WasCommitted ? "COMMIT" : tx.WasRolledBack ? "ROLLBACK" : "UNKNOWN");
        }
    }
}
