﻿using System;
using System.Linq;
using Baseline.Dates;
using Jasper;
using Jasper.Messaging.Runtime;
using Jasper.Messaging.Transports;
using Jasper.Persistence.SqlServer;
using Jasper.Persistence.SqlServer.Persistence;
using Jasper.Util;
using Servers;
using Servers.Docker;
using Shouldly;
using Xunit;

namespace IntegrationTests.Persistence.SqlServer.Persistence
{
    [MessageAlias("Message1")]
    public class Message1
    {
        public Guid Id = Guid.NewGuid();
    }


    public class SqlServerBackedMessagePersistenceTests : SqlServerContext, IDisposable
    {
        private JasperRuntime theRuntime;
        private Envelope theEnvelope;
        private Envelope persisted;

        public SqlServerBackedMessagePersistenceTests(DockerFixture<SqlServerContainer> fixture) : base(fixture)
        {
            theRuntime = JasperRuntime.For(_ =>
            {

                _.Settings.PersistMessagesWithSqlServer(SqlServerContainer.ConnectionString);

            });

            theRuntime.RebuildMessageStorage();

            theEnvelope = ObjectMother.Envelope();
            theEnvelope.Message = new Message1();
            theEnvelope.ExecutionTime = DateTime.Today.ToUniversalTime().AddDays(1);

            theRuntime.Get<SqlServerBackedDurableMessagingFactory>().ScheduleJob(theEnvelope).Wait(3.Seconds());

            var persistor = theRuntime.Get<SqlServerEnvelopePersistor>();

            persisted = persistor.AllIncomingEnvelopes().FirstOrDefault(x => x.Id == theEnvelope.Id);
        }

        [Fact]
        public void should_persist_the_scheduled_envelope()
        {
            persisted.ShouldNotBeNull();
        }

        [Fact]
        public void should_be_in_scheduled_status()
        {
            persisted.Status.ShouldBe(TransportConstants.Scheduled);
        }

        [Fact]
        public void should_be_owned_by_any_node()
        {
            persisted.OwnerId.ShouldBe(TransportConstants.AnyNode);
        }

        public void Dispose()
        {
            theRuntime.Dispose();
        }
    }
}
