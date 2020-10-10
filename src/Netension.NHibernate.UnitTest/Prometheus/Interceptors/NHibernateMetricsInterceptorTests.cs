using Microsoft.Extensions.Logging;
using Moq;
using Netension.Monitoring.Prometheus;
using Netension.NHibernate.Prometheus.Enumerations;
using Netension.NHibernate.Prometheus.Interceptors;
using Netension.NHibernate.Prometheus.Services;
using NHibernate;
using Xunit;
using Xunit.Abstractions;

namespace Netension.NHibernate.UnitTest.Prometheus.Interceptors
{
    public class NHibernateMetricsInterceptorTests
    {
        private readonly string _prefix = "metrics_test";

        private readonly ITestOutputHelper _outputHelper;
        private Mock<ICounterManager> _counterManagerMock;

        public NHibernateMetricsInterceptorTests(ITestOutputHelper outputHelper)
        {
            _outputHelper = outputHelper;
        }

        private NHibernateMetricsInterceptor CreateSUT()
        {
            var loggerFactory = new LoggerFactory().AddXUnit(_outputHelper);

            _counterManagerMock = new Mock<ICounterManager>();

            NamingService.SetPrefix(_prefix);

            return new NHibernateMetricsInterceptor(loggerFactory, _counterManagerMock.Object);
        }

        [Fact]
        public void NHibernateMetricsInterceptor_AfterTransactionCompletion_Commit()
        {
            // Arrange
            var sut = CreateSUT();

            var transactionMock = new Mock<ITransaction>();
            transactionMock.Setup(t => t.WasCommitted).Returns(true);

            // Act
            sut.AfterTransactionCompletion(transactionMock.Object);

            // Assert
            _counterManagerMock.Verify(cmm => cmm.Increase(It.Is<string>(n => n.Equals($"{_prefix}_{NHibernateMetricsEnumeration.TotalTransactionsCount.Name}")), It.Is<string[]>(l => l[0].Equals("COMMIT"))), Times.Once);
        }

        [Fact]
        public void NHibernateMetricsInterceptor_AfterTransactionCompletion_Rollback()
        {
            // Arrange
            var sut = CreateSUT();

            var transactionMock = new Mock<ITransaction>();
            transactionMock.Setup(t => t.WasRolledBack).Returns(true);

            // Act
            sut.AfterTransactionCompletion(transactionMock.Object);

            // Assert
            _counterManagerMock.Verify(cmm => cmm.Increase(It.Is<string>(n => n.Equals($"{_prefix}_{NHibernateMetricsEnumeration.TotalTransactionsCount.Name}")), It.Is<string[]>(l => l[0].Equals("ROLLBACK"))), Times.Once);
        }

        [Fact]
        public void NHibernateMetricsInterceptor_AfterTransactionCompletion_TransactionNull()
        {
            // Arrange
            var sut = CreateSUT();

            // Act
            sut.AfterTransactionCompletion(null);

            // Assert - Do not throw exception
            Assert.True(true);
        }
    }
}
