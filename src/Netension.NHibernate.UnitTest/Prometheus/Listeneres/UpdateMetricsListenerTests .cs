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
    public class UpdateMetricsListenerTests
    {
        private readonly string _prefix = "metrics_test";
        private Mock<ISummaryManager> _summaryManagerMock;
        private StopwatchCollection _stopwatchCollection;
        private readonly ITestOutputHelper _outputHelper;

        public UpdateMetricsListenerTests(ITestOutputHelper outputHelper)
        {
            _outputHelper = outputHelper;
        }

        private UpdateMetricsListener CreateSUT()
        {
            var loggerFactory = new LoggerFactory().AddXUnit(_outputHelper);

            _summaryManagerMock = new Mock<ISummaryManager>();
            _stopwatchCollection = new StopwatchCollection(loggerFactory);

            NamingService.SetPrefix(_prefix);

            return new UpdateMetricsListener(_summaryManagerMock.Object, _stopwatchCollection, loggerFactory);
        }

        [Fact(DisplayName = "Update metrics - Start stopwatch")]
        public async Task UpdateMetrics_PreUpdateAsync_StartStopwatch()
        {
            // Arrange
            var sut = CreateSUT();
            var @event = new PreUpdateEvent(null, Guid.NewGuid(), null, null, null, null);

            // Act
            await sut.OnPreUpdateAsync(@event, default);

            // Assert
            Assert.NotEqual(TimeSpan.Zero, _stopwatchCollection[@event.Id.ToString()]);
        }

        [Fact(DisplayName = "Update metrics - Observe")]
        public async Task UpdateMetrics_PostUpdate_ObserveMetric()
        {
            // Arrange
            var sut = CreateSUT();
            var id = Guid.NewGuid();

            await sut.OnPreUpdateAsync(new PreUpdateEvent(null, id, null, null, null, null), default);

            // Act
            await sut.OnPostUpdateAsync(new PostUpdateEvent(null, Guid.NewGuid(), null, null, null, null), default);

            // Assert
            _summaryManagerMock.Verify(sc => sc.Observe(It.Is<string>(n => n.Equals($"{_prefix}_{NHibernateMetricsEnumeration.SqlStatementExecuteDuration.Name}")), It.IsAny<double>(), It.IsAny<string[]>()), Times.Once);
        }

        [Fact(DisplayName = "Update metrics - Observe not exists metric")]
        public async Task UpdateMetrics_PosUpdate_ObserveNotExistsMetric()
        {
            // Arrange
            var sut = CreateSUT();
            var id = Guid.NewGuid();

            _summaryManagerMock.Setup(sc => sc.Observe(It.IsAny<string>(), It.IsAny<double>(), It.IsAny<string[]>()))
                .Throws<InvalidOperationException>();

            await sut.OnPreUpdateAsync(new PreUpdateEvent(null, id, null, null, null, null), default);

            // Act
            await sut.OnPostUpdateAsync(new PostUpdateEvent(null, Guid.NewGuid(), null, null, null, null), default);

            // Assert - Not thrown exception
            Assert.True(true);
        }
    }
}
