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
    public class SelectMetricsListenerTests
    {
        private readonly string _prefix = "select_metrics_test";
        private Mock<ISummaryCollection> _summaryCollectionMock;
        private StopwatchCollection _stopwatchCollection;
        private readonly ITestOutputHelper _outputHelper;

        public SelectMetricsListenerTests(ITestOutputHelper outputHelper)
        {
            _outputHelper = outputHelper;
        }

        private SelectMetricsListener CreateSUT()
        {
            var loggerFactory = new LoggerFactory().AddXUnit(_outputHelper);

            _summaryCollectionMock = new Mock<ISummaryCollection>();
            _stopwatchCollection = new StopwatchCollection(loggerFactory);

            return new SelectMetricsListener(_summaryCollectionMock.Object, _stopwatchCollection, new NHibernateMetricsOptions { Prefix = _prefix }, loggerFactory);
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
            _summaryCollectionMock.Verify(sc => sc.Observe(It.Is<string>(n => n.Equals($"{_prefix}_{NHibernateMetricsEnumeration.SqlStatementExecuteDuration.Name}")), It.IsAny<double>(), It.IsAny<string[]>()), Times.Once);
        }

        [Fact(DisplayName = "Select metrics - Observe not exists metric")]
        public async Task SelectMetrics_PostLoad_ObserveNotExistsMetric()
        {
            // Arrange
            var sut = CreateSUT();
            var id = Guid.NewGuid();

            _summaryCollectionMock.Setup(sc => sc.Observe(It.IsAny<string>(), It.IsAny<double>(), It.IsAny<string[]>()))
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
