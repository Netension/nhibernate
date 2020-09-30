using FluentNHibernate.Mapping;
using System;

namespace Netension.NHibernate.Prometheus.Example.Entities
{
    public class ExampleEntity
    {
        public virtual Guid Id { get; set; }
        public virtual string Value { get; set; }
    }

    public class ExampleEntityMap : ClassMap<ExampleEntity>
    {
        public ExampleEntityMap()
        {
            Table("nhibernate_prometheus_example");

            Id(x => x.Id)
                .Column("id")
                .GeneratedBy.Assigned();

            Map(x => x.Value)
                .Column("value");
        }
    }
}
