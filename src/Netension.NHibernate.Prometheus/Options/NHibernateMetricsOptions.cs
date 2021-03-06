﻿using System.Diagnostics.CodeAnalysis;

namespace Netension.NHibernate.Prometheus.Options
{
    [ExcludeFromCodeCoverage]
    public class NHibernateMetricsOptions
    {
        /// <summary>
        /// Prefix of the NHibernate metrics. Default value is nhibernate.
        /// </summary>
        public string Prefix { get; set; }
    }
}
