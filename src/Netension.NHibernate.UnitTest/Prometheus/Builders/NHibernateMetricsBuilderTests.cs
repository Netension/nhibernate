using FluentNHibernate.Cfg;
using Microsoft.Extensions.Logging;
using Moq;
using Netension.Monitoring.Prometheus;
using Netension.NHibernate.Prometheus.Builders;
using Netension.NHibernate.Prometheus.Enumerations;
using Netension.NHibernate.Prometheus.Listeners;
using Netension.NHibernate.Prometheus.Options;
using NHibernate.Cfg;
using System.Collections.Generic;
using Xunit;
using Xunit.Abstractions;

namespace Netension.NHibernate.UnitTest.Prometheus.Builders
{
    public class NHibernateMetricsBuilderTests
    {
        private const string PREFIX = "nhibernate_metrics_builder_test";
        private readonly ITestOutputHelper _outputHelper;
        private Configuration _configuration;
        private Mock<IPrometheusMetricsRegistry> _metricsRegistryMock;
        private Mock<ISummaryCollection> _summaryCollectionMock;

        public NHibernateMetricsBuilderTests(ITestOutputHelper outputHelper)
        {
            _outputHelper = outputHelper;
        }

        private NHibernateMetricsBuilder CreateSUT(string prefix)
        {
            _configuration = Fluently.Configure()
                                .BuildConfiguration();

            var loggerFactory = new LoggerFactory()
                                    .AddXUnit(_outputHelper);

            _metricsRegistryMock = new Mock<IPrometheusMetricsRegistry>();
            _summaryCollectionMock = new Mock<ISummaryCollection>();

            return new NHibernateMetricsBuilder(_configuration, new NHibernateMetricsOptions { Prefix = prefix }, loggerFactory, _metricsRegistryMock.Object, _summaryCollectionMock.Object);
        }

        [Fact]
        public void NHibernateMetricsBuilder_AddSelectStatementsDurationMetric_AppendPreListener()
        {
            // Arrange
            var sut = CreateSUT(PREFIX);

            // Act
            sut.AddSelectStatementsDurationMetric();

            // Assert
            Assert.Contains(_configuration.EventListeners.PreLoadEventListeners, plel => plel is SelectMetricsListener);
        }

        [Fact]
        public void NHibernateMetricsBuilder_AddSelectStatementsDurationMetric_AppendPostListener()
        {
            // Arrange
            var sut = CreateSUT(PREFIX);

            // Act
            sut.AddSelectStatementsDurationMetric();

            // Assert
            Assert.Contains(_configuration.EventListeners.PostLoadEventListeners, plel => plel is SelectMetricsListener);
        }

        [Fact]
        public void NHibernateMetricsBuilder_AddSelectStatementsDurationMetric_RegisterSummary()
        {
            // Arrange
            var sut = CreateSUT(PREFIX);

            // Act
            sut.AddSelectStatementsDurationMetric();

            // Assert
            _metricsRegistryMock.Verify(mr =>mr.RegisterSummary(It.Is<string>(n => n.Equals($"{PREFIX}_{NHibernateMetricsEnumeration.SqlStatementExecuteDuration.Name}")), It.Is<string>(d => d.Equals(NHibernateMetricsEnumeration.SqlStatementExecuteDuration.Description)), It.IsAny<IEnumerable<string>>()), Times.Once);
        }

        [Fact]
        public void NHibernateMetricsBuilder_AddInsertStatementsDurationMetric_AppendPreListener()
        {
            // Arrange
            var sut = CreateSUT(PREFIX);

            // Act
            sut.AddInsertStatementsDurationMetric();

            // Assert
            Assert.Contains(_configuration.EventListeners.PreInsertEventListeners, piel => piel is InsertMetricsListener);
        }

        [Fact]
        public void NHibernateMetricsBuilder_AddInsertStatementsDurationMetric_AppendPostListener()
        {
            // Arrange
            var sut = CreateSUT(PREFIX);

            // Act
            sut.AddInsertStatementsDurationMetric();

            // Assert
            Assert.Contains(_configuration.EventListeners.PostInsertEventListeners, piel => piel is InsertMetricsListener);
        }

        [Fact]
        public void NHibernateMetricsBuilder_AddInsertStatementsDurationMetric_RegisterSummary()
        {
            // Arrange
            var sut = CreateSUT(PREFIX);

            // Act
            sut.AddInsertStatementsDurationMetric();

            // Assert
            _metricsRegistryMock.Verify(mr => mr.RegisterSummary(It.Is<string>(n => n.Equals($"{PREFIX}_{NHibernateMetricsEnumeration.SqlStatementExecuteDuration.Name}")), It.Is<string>(d => d.Equals(NHibernateMetricsEnumeration.SqlStatementExecuteDuration.Description)), It.IsAny<IEnumerable<string>>()), Times.Once);
        }

        [Fact]
        public void NHibernateMetricsBuilder_AddUpdateStatementsDurationMetric_AppendPreListener()
        {
            // Arrange
            var sut = CreateSUT(PREFIX);

            // Act
            sut.AddUpdateStatementsDurationMetric();

            // Assert
            Assert.Contains(_configuration.EventListeners.PreUpdateEventListeners, puel => puel is UpdateMetricsListener);
        }

        [Fact]
        public void NHibernateMetricsBuilder_AddUpdateStatementsDurationMetric_AppendPostListener()
        {
            // Arrange
            var sut = CreateSUT(PREFIX);

            // Act
            sut.AddUpdateStatementsDurationMetric();

            // Assert
            Assert.Contains(_configuration.EventListeners.PostUpdateEventListeners, puel => puel is UpdateMetricsListener);
        }

        [Fact]
        public void NHibernateMetricsBuilder_AddUpdateStatementsDurationMetric_RegisterSummary()
        {
            // Arrange
            var sut = CreateSUT(PREFIX);

            // Act
            sut.AddUpdateStatementsDurationMetric();

            // Assert
            _metricsRegistryMock.Verify(mr => mr.RegisterSummary(It.Is<string>(n => n.Equals($"{PREFIX}_{NHibernateMetricsEnumeration.SqlStatementExecuteDuration.Name}")), It.Is<string>(d => d.Equals(NHibernateMetricsEnumeration.SqlStatementExecuteDuration.Description)), It.IsAny<IEnumerable<string>>()), Times.Once);
        }

        [Fact]
        public void NHibernateMetricsBuilder_AddDeleteStatementsDurationMetric_AppendPreListener()
        {
            // Arrange
            var sut = CreateSUT(PREFIX);

            // Act
            sut.AddDeleteStatementsDurationMetric();

            // Assert
            Assert.Contains(_configuration.EventListeners.PreDeleteEventListeners, pdel => pdel is DeleteMetricsListener);
        }

        [Fact]
        public void NHibernateMetricsBuilder_AddDeleteStatementsDurationMetric_AppendPostListener()
        {
            // Arrange
            var sut = CreateSUT(PREFIX);

            // Act
            sut.AddDeleteStatementsDurationMetric();

            // Assert
            Assert.Contains(_configuration.EventListeners.PostDeleteEventListeners, pdel => pdel is DeleteMetricsListener);
        }

        [Fact]
        public void NHibernateMetricsBuilder_AddDeleteStatementsDurationMetric_RegisterSummary()
        {
            // Arrange
            var sut = CreateSUT(PREFIX);

            // Act
            sut.AddDeleteStatementsDurationMetric();

            // Assert
            _metricsRegistryMock.Verify(mr => mr.RegisterSummary(It.Is<string>(n => n.Equals($"{PREFIX}_{NHibernateMetricsEnumeration.SqlStatementExecuteDuration.Name}")), It.Is<string>(d => d.Equals(NHibernateMetricsEnumeration.SqlStatementExecuteDuration.Description)), It.IsAny<IEnumerable<string>>()), Times.Once);
        }
    }
}
