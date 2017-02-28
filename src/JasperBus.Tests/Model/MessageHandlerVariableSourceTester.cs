﻿using JasperBus.Model;
using JasperBus.Runtime;
using JasperBus.Tests.Runtime;
using Shouldly;
using Xunit;

namespace JasperBus.Tests.Model
{
    public class MessageHandlerVariableSourceTester
    {
        [Fact]
        public void responds_to_envelope()
        {
            var source = new MessageHandlerVariableSource(typeof(Message1));

            source.Matches(typeof(Envelope)).ShouldBeTrue();

            var variable = source.Create(typeof(Envelope));

            variable.VariableType.ShouldBe(typeof(Envelope));

            variable.Usage.ShouldBe("context.Envelope");
        }

        [Fact]
        public void responds_to_the_message()
        {
            var source = new MessageHandlerVariableSource(typeof(Message1));

            source.Matches(typeof(Message1)).ShouldBeTrue();

            var variable = source.Create(typeof(Message1));

            variable.VariableType.ShouldBe(typeof(Message1));

            variable.Usage.ShouldBe("message1");
        }
    }
}