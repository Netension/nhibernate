using Netension.Monitoring.Prometheus;
using Netension.NHibernate.Prometheus.Enumerations;
using Netension.NHibernate.Prometheus.Services;
using NHibernate;
using ILoggerFactory = Microsoft.Extensions.Logging.ILoggerFactory;

namespace Netension.NHibernate.Prometheus.Interceptors
{
    public class NHibernateMetricsInterceptor : EmptyInterceptor
    {
        private readonly ICounterManager _counterManager;

        public NHibernateMetricsInterceptor(ILoggerFactory loggerFactory, ICounterManager counterManager)
        {
            _counterManager = counterManager;
        }

        public override void AfterTransactionCompletion(ITransaction tx)
        {
            base.AfterTransactionCompletion(tx);

            if (tx == null) return;

            string operation = "UNKNOWN";
            if (tx.WasCommitted) operation = "COMMIT";
            else if (tx.WasRolledBack) operation = "ROLLBACK";

            _counterManager.Increase(NamingService.GetFullName(NHibernateMetricsEnumeration.TotalTransactionsCount.Name), operation);
        }
    }
}
