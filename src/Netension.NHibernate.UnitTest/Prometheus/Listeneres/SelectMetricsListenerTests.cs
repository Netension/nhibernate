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
    public class SelectMetricsListenerTests
    {
        private readonly string _prefix = "metrics_test";
        private Mock<ISummaryManager> _summaryManagerMock;
        private StopwatchCollection _stopwatchCollection;
        private readonly ITestOutputHelper _outputHelper;

        public SelectMetricsListenerTests(ITestOutputHelper outputHelper)
        {
            _outputHelper = outputHelper;
        }

        private SelectMetricsListener CreateSUT()
        {
            var loggerFactory = new LoggerFactory().AddXUnit(_outputHelper);

            _summaryManagerMock = new Mock<ISummaryManager>();
            _stopwatchCollection = new StopwatchCollection(loggerFactory);

            NamingService.SetPrefix(_prefix);

            return new SelectMetricsListener(_summaryManagerMock.Object, _stopwatchCollection, loggerFactory);
        }

        [Fact(DisplayName = "Select metrics - Start stopwatch")]
        public async Task SelectMetrics_PreLoadAsync_StartStopwatch()
        {
            // Arrange
            var sut = CreateSUT();
            var @event = new PreLoadEvent(new Mock<IEventSource>().Object)
            {
                Id = Guid.NewGuid()
            };

            // Act
            await sut.OnPreLoadAsync(@event, default);

            // Assert
            Assert.NotEqual(TimeSpan.Zero, _stopwatchCollection[@event.Id.ToString()]);
        }

        [Fact(DisplayName = "Select metrics - Observe")]
        public async Task SelectMetrics_PostLoad_ObserveMetric()
        {
            // Arrange
            var sut = CreateSUT();
            var id = Guid.NewGuid();

            await sut.OnPreLoadAsync(new PreLoadEvent(new Mock<IEventSource>().Object)
            {
                Id = id
            }, default);

            // Act
            sut.OnPostLoad(new PostLoadEvent(new Mock<IEventSource>().Object)
            {
                Id = id
            });

            // Assert
            _summaryManagerMock.Verify(sc => sc.Observe(It.Is<string>(n => n.Equals($"{_prefix}_{NHibernateMetricsEnumeration.SqlStatementExecuteDuration.Name}")), It.IsAny<double>(), It.IsAny<string[]>()), Times.Once);
        }

        [Fact(DisplayName = "Select metrics - Observe not exists metric")]
        public async Task SelectMetrics_PostLoad_ObserveNotExistsMetric()
        {
            // Arrange
            var sut = CreateSUT();
            var id = Guid.NewGuid();

            _summaryManagerMock.Setup(sc => sc.Observe(It.IsAny<string>(), It.IsAny<double>(), It.IsAny<string[]>()))
                .Throws<InvalidOperationException>();

            await sut.OnPreLoadAsync(new PreLoadEvent(new Mock<IEventSource>().Object)
            {
                Id = id
            }, default);

            // Act
            sut.OnPostLoad(new PostLoadEvent(new Mock<IEventSource>().Object)
            {
                Id = id
            });

            // Assert - Not thrown exception
            Assert.True(true);
        }
    }
}
