using Netension.Monitoring.Prometheus;
using Netension.NHibernate.Prometheus.Enumerations;

namespace Netension.NHibernate.Prometheus.Builders
{
    public class NHibernateMetricsBuilder
    {
        private readonly IPrometheusMetricsRegistry _prometheusMetricsRegistry;

        public NHibernateMetricsBuilder(IPrometheusMetricsRegistry prometheusMetricsRegistry)
        {
            _prometheusMetricsRegistry = prometheusMetricsRegistry;
        }

        public NHibernateMetricsBuilder RegistrateStatementDuration()
        {
            _prometheusMetricsRegistry.RegistrateSummary(NHibernateMetricsEnumeration.SqlStatementExecuteDuration.Name, NHibernateMetricsEnumeration.SqlStatementExecuteDuration.Description, NHibernateMetricsEnumeration.SqlStatementExecuteDuration.Labels);

            return this;
        }

        public NHibernateMetricsBuilder RegistrateTotalTransactionsCount()
        {
            _prometheusMetricsRegistry.RegistrateCounter(NHibernateMetricsEnumeration.TotalTransactionsCount.Name, NHibernateMetricsEnumeration.TotalTransactionsCount.Description, NHibernateMetricsEnumeration.TotalTransactionsCount.Labels);

            return this;
        }

        public NHibernateMetricsBuilder RegistrateRecordCount()
        {
            _prometheusMetricsRegistry.RegistrateGauge(NHibernateMetricsEnumeration.RecordCount.Name, NHibernateMetricsEnumeration.RecordCount.Description, NHibernateMetricsEnumeration.RecordCount.Labels);

            return this;
        }
    }
}
