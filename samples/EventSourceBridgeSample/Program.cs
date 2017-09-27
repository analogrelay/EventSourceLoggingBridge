using System;
using System.Buffers;
using System.Diagnostics.Tracing;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace EventSourceBridgeSample
{
    class Program
    {
        static void Main(string[] args)
        {
            var serviceCollection = new ServiceCollection()
                .AddLogging(builder =>
                {
                    builder.SetMinimumLevel(LogLevel.Trace);
                    builder.AddProvider(new TestProvider());
                    builder.AddConsole();
                });

            // providers may be added to a LoggerFactory before any loggers are created


            var serviceProvider = serviceCollection.BuildServiceProvider();

            var loggerFactory = serviceProvider.GetRequiredService<ILoggerFactory>();
            loggerFactory.ImportEventSources();

            TestEventSource.Log.TestEvent("foo", 42);
            TestEventSource.Log.TestEvent("bar", 24);

            // ArrayPool has an EventSource
            // So does TPL
            Task.Run(() =>
            {
                var pool = ArrayPool<string>.Create();
                var pooledArray = pool.Rent(5);
                pool.Return(pooledArray);
                pooledArray = pool.Rent(5);
            }).Wait();

            Console.ReadLine();
        }

        private class TestEventSource : EventSource
        {
            internal static readonly TestEventSource Log = new TestEventSource();

            private TestEventSource()
            {
            }

            [Event(
                eventId: 1,
                Message = "The string value was {0} and the int value was {1}")]
            public void TestEvent(string stringValue, int intValue)
            {
                WriteEvent(1, stringValue, intValue);
            }
        }
    }

    internal class TestProvider : ILoggerProvider
    {
        public ILogger CreateLogger(string categoryName)
        {
            return new TestLogger();
        }

        public void Dispose()
        {
        }
    }

    internal class TestLogger : ILogger
    {
        public IDisposable BeginScope<TState>(TState state)
        {
            return null;
        }

        public bool IsEnabled(LogLevel logLevel)
        {
            return true;
        }

        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception exception, Func<TState, Exception, string> formatter)
        {
        }
    }
}
