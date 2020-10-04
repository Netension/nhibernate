using Microsoft.Extensions.Logging;
using Moq;
using Netension.Monitoring.Core.Diagnostics;
using Netension.Monitoring.Prometheus;
using Netension.NHibernate.Prometheus.Enumerations;
using Netension.NHibernate.Prometheus.Listeners;
using Netension.NHibernate.Prometheus.Options;
using NHibernate.Event;
using System;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;

namespace Netension.NHibernate.UnitTest.Prometheus.Listeneres
{
    public class DeleteMetricsListenerTests
    {
        private readonly string _prefix = "delete_metrics_test";
        private Mock<ISummaryCollection> _summaryCollectionMock;
        private StopwatchCollection _stopwatchCollection;
        private readonly ITestOutputHelper _outputHelper;

        public DeleteMetricsListenerTests(ITestOutputHelper outputHelper)
        {
            _outputHelper = outputHelper;
        }

        private DeleteMetricsListener CreateSUT()
        {
            var loggerFactory = new LoggerFactory().AddXUnit(_outputHelper);

            _summaryCollectionMock = new Mock<ISummaryCollection>();
            _stopwatchCollection = new StopwatchCollection(loggerFactory);

            return new DeleteMetricsListener(_summaryCollectionMock.Object, _stopwatchCollection, new NHibernateMetricsOptions { Prefix = _prefix }, loggerFactory);
        }

        [Fact(DisplayName = "Delete metrics - Start stopwatch")]
        public async Task DeleteMetrics_PreDeleteAsync_StartStopwatch()
        {
            // Arrange
            var sut = CreateSUT();
            var @event = new PreDeleteEvent(null, Guid.NewGuid(), null, null, null);

            // Act
            await sut.OnPreDeleteAsync(@event, default);

            // Assert
            Assert.NotEqual(TimeSpan.Zero, _stopwatchCollection[@event.Id.ToString()]);
        }

        [Fact(DisplayName = "Select metrics - Observe")]
        public async Task SelectMetrics_PostDeleteAsync_ObserveMetric()
        {
            // Arrange
            var sut = CreateSUT();
            var id = Guid.NewGuid();

            await sut.OnPreDeleteAsync(new PreDeleteEvent(null, id, null, null, null), default);

            // Act
            await sut.OnPostDeleteAsync(new PostDeleteEvent(null, id, null, null, null), default);

            // Assert
            _summaryCollectionMock.Verify(sc => sc.Observe(It.Is<string>(n => n.Equals($"{_prefix}_{NHibernateMetricsEnumeration.SqlStatementExecuteDuration.Name}")), It.IsAny<double>(), It.IsAny<string[]>()), Times.Once);
        }

        [Fact(DisplayName = "Select metrics - Observe not exists metric")]
        public async Task SelectMetrics_PostDeleteAsync_ObserveNotExistsMetric()
        {
            // Arrange
            var sut = CreateSUT();
            var id = Guid.NewGuid();

            _summaryCollectionMock.Setup(sc => sc.Observe(It.IsAny<string>(), It.IsAny<double>(), It.IsAny<string[]>()))
                .Throws<InvalidOperationException>();

            await sut.OnPreDeleteAsync(new PreDeleteEvent(null, id, null, null, null), default);

            // Act
            await sut.OnPostDeleteAsync(new PostDeleteEvent(null, id, null, null, null), default);

            // Assert - Not thrown exception
            Assert.True(true);
        }
    }
}
