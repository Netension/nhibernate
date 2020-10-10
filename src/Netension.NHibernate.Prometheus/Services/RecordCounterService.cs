using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Netension.Monitoring.Prometheus;
using Netension.NHibernate.Prometheus.Enumerations;
using Netension.NHibernate.Prometheus.Options;
using NHibernate;
using NHibernate.Linq;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Threading;
using System.Threading.Tasks;
using ILoggerFactory = Microsoft.Extensions.Logging.ILoggerFactory;

namespace Netension.NHibernate.Prometheus.Services
{
    [ExcludeFromCodeCoverage]
    internal class RecordCounterService<TEntity> : BackgroundService
        where TEntity : class
    {
        private readonly ISession _session;
        private readonly IGaugeManager _gaugeManager;
        private readonly NHibernateRecordCountMetricOptions _options;
        private readonly ILogger<RecordCounterService<TEntity>> _logger;

        public RecordCounterService(ISession session, IGaugeManager gaugeManager, ILoggerFactory loggerFactory, IOptions<NHibernateRecordCountMetricOptions> optionsAccessor)
        {
            _session = session;
            _gaugeManager = gaugeManager;
            _options = optionsAccessor.Value;
            _logger = loggerFactory.CreateLogger<RecordCounterService<TEntity>>();
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogDebug("Start record counter service for {entity}.", typeof(TEntity).Name);

            while (true)
            {
                try
                {
                    _logger.LogDebug("Query {entity} record count.", typeof(TEntity).Name);
                    var recordCount = await _session.Query<TEntity>().CountAsync(stoppingToken);

                    _gaugeManager.Set(NamingService.GetFullName(NHibernateMetricsEnumeration.RecordCount.Name), recordCount, typeof(TEntity).Namespace, typeof(TEntity).Name);

                    await Task.Delay(TimeSpan.FromSeconds(_options.Interval), stoppingToken);
                }
                catch
                {
                    _logger.LogError("Exception during query {entity} record count.", typeof(TEntity).Name);
                }
            }
        }
    }
}
