using System.Diagnostics.CodeAnalysis;

namespace Netension.NHibernate.Prometheus.Options
{
    [ExcludeFromCodeCoverage]
    public class NHibernateRecordCountMetricOptions
    {
        /// <summary>
        /// Interval of record count determination in seconds.
        /// </summary>
        public int Interval { get; set; }
    }
}
