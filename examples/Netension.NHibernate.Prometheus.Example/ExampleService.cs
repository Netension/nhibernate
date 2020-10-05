using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Netension.NHibernate.Prometheus.Example.Entities;
using NHibernate;
using NHibernate.Linq;
using System;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using ILoggerFactory = Microsoft.Extensions.Logging.ILoggerFactory;

namespace Netension.NHibernate.Prometheus.Example
{
    public class ExampleService : IHostedService
    {
        private readonly ISession _session;
        private readonly ILogger<ExampleService> _logger;

        public ExampleService(ISession session, ILoggerFactory loggerFactory)
        {
            _session = session;
            _logger = loggerFactory.CreateLogger<ExampleService>();
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            await InsertDataAsync(cancellationToken);
            await SelectDataAsync(cancellationToken);
            await UpdateDataAsync(cancellationToken);
            await DeleteDataAsync(cancellationToken);
        }

        private async Task SelectDataAsync(CancellationToken cancellationToken)
        {
            _session.Clear();
            _logger.LogInformation("Selecting data");
            var entities = await _session.Query<ExampleEntity>().ToListAsync(cancellationToken);
            foreach (var entity in entities)
            {
                _logger.LogInformation("Entity {id}", entity.Value);
            }
        }

        private async Task UpdateDataAsync(CancellationToken cancellationToken)
        {
            _session.Clear();
            var namesGenerator = new NamesGenerator();
            using (var transaction = _session.BeginTransaction())
            {
                try
                {
                    foreach (var entity in await _session.Query<ExampleEntity>().ToListAsync(cancellationToken))
                    {
                        entity.Value = namesGenerator.GetRandomName();
                    }

                    await transaction.CommitAsync(cancellationToken);
                }
                catch
                {
                    await transaction.RollbackAsync(cancellationToken);
                    throw;
                }
            }
        }

        private async Task InsertDataAsync(CancellationToken cancellationToken)
        {
            _session.Clear();
            var namesGenerator = new NamesGenerator();
            using (var transaction = _session.BeginTransaction())
            {
                try
                {
                    for (int i = 0; i < 1000; i++)
                    {
                        var entity = new ExampleEntity { Id = Guid.NewGuid(), Value = namesGenerator.GetRandomName() };
                        _logger.LogInformation("Insert {id} example entity", entity.Id);
                        await _session.SaveAsync(entity);
                    }

                    await transaction.CommitAsync(cancellationToken);
                }
                catch
                {
                    await transaction.RollbackAsync(cancellationToken);
                    throw;
                }
            }
        }

        private async Task DeleteDataAsync(CancellationToken cancellationToken)
        {
            _session.Clear();
            using (var transaction = _session.BeginTransaction())
            {
                try
                {
                    foreach (var entity in await _session.Query<ExampleEntity>().ToListAsync(cancellationToken))
                    {
                        await _session.DeleteAsync(entity);
                    }

                    await transaction.RollbackAsync(cancellationToken);
                }
                catch
                {
                    await transaction.RollbackAsync(cancellationToken);
                    throw;
                }
            }
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }
    }
}
