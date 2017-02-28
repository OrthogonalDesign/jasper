﻿using System;
using System.Threading.Tasks;
using JasperBus.Tests.Runtime;
using Shouldly;
using Xunit;
using Xunit.Abstractions;

namespace JasperBus.Tests.Compilation
{
    public class simple_async_message_handlers : CompilationContext<AsyncHandler>
    {
        private ITestOutputHelper _output;

        public simple_async_message_handlers(Xunit.Abstractions.ITestOutputHelper output)
        {
            _output = output;
        }

        [Fact]
        public void can_compile_all()
        {
            AllHandlersCompileSuccessfully();
        }


    }

    public class AsyncHandler
    {
        public static Task Simple1(Message1 message)
        {
            return Task.CompletedTask;
        }
    }



}