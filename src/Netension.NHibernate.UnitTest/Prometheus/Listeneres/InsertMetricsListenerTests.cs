using Microsoft.Extensions.Logging;
using Moq;
using Netension.Monitoring.Core.Diagnostics;
using Netension.Monitoring.Prometheus;
using Netension.NHibernate.Prometheus.Enumerations;
using Netension.NHibernate.Prometheus.Listeners;
using Netension.NHibernate.Prometheus.Services;
using NHibernate.Event;
using System;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;

namespace Netension.NHibernate.UnitTest.Prometheus.Listeneres
{
    public class InsertMetricsListenerTests
    {
        private readonly string _prefix = "metrics_test";
        private Mock<ISummaryManager> _summaryManagerMock;
        private StopwatchCollection _stopwatchCollection;
        private readonly ITestOutputHelper _outputHelper;

        public InsertMetricsListenerTests(ITestOutputHelper outputHelper)
        {
            _outputHelper = outputHelper;
        }

        private InsertMetricsListener CreateSUT()
        {
            var loggerFactory = new LoggerFactory().AddXUnit(_outputHelper);

            _summaryManagerMock = new Mock<ISummaryManager>();
            _stopwatchCollection = new StopwatchCollection(loggerFactory);

            NamingService.SetPrefix(_prefix);

            return new InsertMetricsListener(_summaryManagerMock.Object, _stopwatchCollection, loggerFactory);
        }

        [Fact(DisplayName = "Insert metrics - Start stopwatch")]
        public async Task InsertMetrics_PreInsertAsync_StartStopwatch()
        {
            // Arrange
            var sut = CreateSUT();
            var @event = new PreInsertEvent(null, Guid.NewGuid(), null, null, null);

            // Act
            await sut.OnPreInsertAsync(@event, default);

            // Assert
            Assert.NotEqual(TimeSpan.Zero, _stopwatchCollection[@event.Id.ToString()]);
        }

        [Fact(DisplayName = "Insert metrics - Observe")]
        public async Task InsertMetrics_PostInsertAsync_ObserveMetric()
        {
            // Arrange
            var sut = CreateSUT();
            var id = Guid.NewGuid();

            await sut.OnPreInsertAsync(new PreInsertEvent(null, id, null, null, null), default);

            // Act
            await sut.OnPostInsertAsync(new PostInsertEvent(null, id, null, null, null), default);

            // Assert
            _summaryManagerMock.Verify(sc => sc.Observe(It.Is<string>(n => n.Equals($"{_prefix}_{NHibernateMetricsEnumeration.SqlStatementExecuteDuration.Name}")), It.IsAny<double>(), It.IsAny<string[]>()), Times.Once);
        }

        [Fact(DisplayName = "Insert metrics - Observe not exists metric")]
        public async Task InsertMetrics_PostInstertAsync_ObserveNotExistsMetric()
        {
            // Arrange
            var sut = CreateSUT();
            var id = Guid.NewGuid();

            _summaryManagerMock.Setup(sc => sc.Observe(It.IsAny<string>(), It.IsAny<double>(), It.IsAny<string[]>()))
                .Throws<InvalidOperationException>();

            await sut.OnPreInsertAsync(new PreInsertEvent(null, id, null, null, null), default);

            // Act
            await sut.OnPostInsertAsync(new PostInsertEvent(null, id, null, null, null), default);

            // Assert - Not thrown exception
            Assert.True(true);
        }
    }
}
