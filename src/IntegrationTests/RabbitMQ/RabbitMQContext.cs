﻿using Servers;
using Servers.Docker;
using Xunit;

namespace IntegrationTests.RabbitMQ
{
    [Collection("rabbitmq")]
    public abstract class RabbitMQContext : IClassFixture<DockerFixture<RabbitMQContainer>>
    {
        public RabbitMQContext(DockerFixture<RabbitMQContainer> container)
        {
        }
    }
}
