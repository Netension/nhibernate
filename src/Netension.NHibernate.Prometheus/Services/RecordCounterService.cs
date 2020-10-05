using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Netension.Monitoring.Prometheus;
using Netension.NHibernate.Prometheus.Enumerations;
using NHibernate;
using NHibernate.Linq;
using System;
using System.Threading;
using System.Threading.Tasks;
using ILoggerFactory = Microsoft.Extensions.Logging.ILoggerFactory;

namespace Netension.NHibernate.Prometheus.Services
{
    public class RecordCounterService<TEntity> : BackgroundService
        where TEntity : class
    {
        private readonly ISession _session;
        private readonly IGaugeManager _gaugeManager;
        private readonly ILogger<RecordCounterService<TEntity>> _logger;

        public RecordCounterService(ISession session, IGaugeManager gaugeManager, ILoggerFactory loggerFactory)
        {
            _session = session;
            _gaugeManager = gaugeManager;
            _logger = loggerFactory.CreateLogger<RecordCounterService<TEntity>>();
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogDebug("Start record counter service for {entity}.", typeof(TEntity).Name);

            while (true)
            {
                _logger.LogDebug("Query {entity} record count.", typeof(TEntity).Name);
                var recordCount = await _session.Query<TEntity>().CountAsync(stoppingToken);

                _gaugeManager.Set(NHibernateMetricsEnumeration.RecordCount.Name, recordCount, typeof(TEntity).Namespace, typeof(TEntity).Name);

                await Task.Delay(TimeSpan.FromSeconds(5));
            }
        }
    }
}
