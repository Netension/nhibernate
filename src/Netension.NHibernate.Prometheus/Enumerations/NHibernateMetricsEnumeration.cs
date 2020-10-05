using Netension.Monitoring.Prometheus.Enumerations;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace Netension.NHibernate.Prometheus.Enumerations
{
    [ExcludeFromCodeCoverage]
    public class NHibernateMetricsEnumeration : PrometheusMetricEnumeration
    {
        public static NHibernateMetricsEnumeration SqlStatementExecuteDuration => new NHibernateMetricsEnumeration(1, "sql_statement_execution_duration", "Total time of SQL statements execution, in milliseconds. (SELECT, INSERT, UPDATE and DELETE)", new List<string> { "Database", "Namespace", "Entity", "Operation" });
        public static NHibernateMetricsEnumeration TotalTransactionsCount => new NHibernateMetricsEnumeration(2, "total_transactions_count", "Total count of transactions.", new List<string> { "Operation" });
        public static NHibernateMetricsEnumeration RecordCount => new NHibernateMetricsEnumeration(3, "record_count", "Number of records.", new List<string> { "Namespace", "Entity" });

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
