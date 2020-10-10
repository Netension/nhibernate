using Moq;
using Netension.Monitoring.Prometheus;
using Netension.NHibernate.Prometheus.Builders;
using Netension.NHibernate.Prometheus.Enumerations;
using Netension.NHibernate.Prometheus.Services;
using System.Collections.Generic;
using System.Linq;
using Xunit;

namespace Netension.NHibernate.UnitTest.Prometheus.Builders
{
    public class NHibernateMetricsBuilderTests
    {
        private readonly string _prefix = "metrics_test";

        private Mock<IPrometheusMetricsRegistry> _prometheusMetricsRegistryMock;

        private NHibernateMetricsBuilder CreateSUT()
        {
            _prometheusMetricsRegistryMock = new Mock<IPrometheusMetricsRegistry>();

            NamingService.SetPrefix(_prefix);

            return new NHibernateMetricsBuilder(_prometheusMetricsRegistryMock.Object);
        }

        [Fact]
        public void NHibernarteMetricsBuilder_RegistrateStatementDuration()
        {
            // Arrange
            var sut = CreateSUT();

            // Act
            sut.RegistrateStatementDuration();

            // Assert
            _prometheusMetricsRegistryMock.Verify(pmr =>pmr.RegistrateSummary(It.Is<string>(n => n.Equals($"{_prefix}_{NHibernateMetricsEnumeration.SqlStatementExecuteDuration.Name}")), It.Is<string>(d => d.Equals(NHibernateMetricsEnumeration.SqlStatementExecuteDuration.Description)), It.Is<IEnumerable<string>>(l => l.SequenceEqual(NHibernateMetricsEnumeration.SqlStatementExecuteDuration.Labels))), Times.Once);
        }

        [Fact]
        public void NHibernarteMetricsBuilder_RegistrateTotalTransactionCount()
        {
            // Arrange
            var sut = CreateSUT();

            // Act
            sut.RegistrateTotalTransactionsCount();

            // Assert
            _prometheusMetricsRegistryMock.Verify(pmr => pmr.RegistrateCounter(It.Is<string>(n => n.Equals($"{_prefix}_{NHibernateMetricsEnumeration.TotalTransactionsCount.Name}")), It.Is<string>(d => d.Equals(NHibernateMetricsEnumeration.TotalTransactionsCount.Description)), It.Is<IEnumerable<string>>(l => l.SequenceEqual(NHibernateMetricsEnumeration.TotalTransactionsCount.Labels))), Times.Once);
        }

        [Fact]
        public void NHibernarteMetricsBuilder_RegistrateRecordCount()
        {
            // Arrange
            var sut = CreateSUT();

            // Act
            sut.RegistrateRecordCount();

            // Assert
            _prometheusMetricsRegistryMock.Verify(pmr => pmr.RegistrateGauge(It.Is<string>(n => n.Equals($"{_prefix}_{NHibernateMetricsEnumeration.RecordCount.Name}")), It.Is<string>(d => d.Equals(NHibernateMetricsEnumeration.RecordCount.Description)), It.Is<IEnumerable<string>>(l => l.SequenceEqual(NHibernateMetricsEnumeration.RecordCount.Labels))), Times.Once);
        }
    }
}
