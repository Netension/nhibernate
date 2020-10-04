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
    }
}
