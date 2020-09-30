using Netension.Monitoring.Prometheus.Enumerations;
using System.Collections.Generic;

namespace Netension.NHibernate.Prometheus.Enumerations
{
    public class NHibernateMetricsEnumeration : PrometheusMetricEnumeration
    {
        public static NHibernateMetricsEnumeration SqlStatementExecuteDuration => new NHibernateMetricsEnumeration(1, "sql_statement_execution_duration", "Total time of SQL statements execution, in milliseconds. (SELECT, INSERT, UPDATE and DELETE)", new List<string> { "Database", "Namespace", "Entity", "Operation" });

        public NHibernateMetricsEnumeration(int id, string name, string description) 
            : base(id, name, description)
        {
        }

        public NHibernateMetricsEnumeration(int id, string name, string description, IEnumerable<string> labels) 
            : base(id, name, description, labels)
        {
        }
    }
}
