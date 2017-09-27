using System;
using System.Buffers;
using System.Diagnostics;
using System.Diagnostics.Tracing;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace EventSourceBridgeSample
{
    class Program
    {
        public static readonly string ArrayPoolEventSourceName = "System.Buffers.ArrayPoolEventSource";
        public static readonly string TplEventSourceName = "System.Threading.Tasks.TplEventSource";
        public static readonly string ConcurrentCollectionsEventSourceName = "System.Collections.Concurrent.ConcurrentCollectionsEventSource";
        public static readonly string FrameworkEventSourceName = "System.Diagnostics.Eventing.FrameworkEventSource";
        public static readonly string SystemNetPrimitivesEventSourceName = "Microsoft-System-Net-Primitives";
        public static readonly string SystemNetHttpEventSourceName = "Microsoft-System-Net-Http";
        public static readonly string DiagnosticSourceEventSourceName = "Microsoft-Diagnostics-DiagnosticsSource";
        public static readonly string HttpDiagnosticSourceName = "HttpHandlerDiagnosticListener";


        static void Main(string[] args)
        {
            var serviceCollection = new ServiceCollection()
                .AddLogging(builder =>
                {
                    builder.SetMinimumLevel(LogLevel.Trace);
                    builder.AddProvider(new TestProvider());
                    builder.AddConsole();
                });

            var serviceProvider = serviceCollection.BuildServiceProvider();

            var loggerFactory = serviceProvider.GetRequiredService<ILoggerFactory>();
            loggerFactory.ImportEventSources(config =>
                config
                    .EnableDiagnosticSource("TestDiagnosticListener", "ThisOneIsEnabled")
                    .EnableEventSource(TestEventSource.Log)
                    .EnableEventSource(ArrayPoolEventSourceName, EventLevel.Verbose));

            TestEventSource.Log.TestEvent("foo", 42);
            TestEventSource.Log.TestEvent("bar", 24);

            // Create a test diagnostic source
            var testListener = new DiagnosticListener("TestDiagnosticListener");
            WriteEvent(testListener, "ThisOneIsEnabled");
            WriteEvent(testListener, "ThisOneIsNotEnabled");

            // ArrayPool has an EventSource
            // So does TPL, but we haven't attached to it.
            Task.Run(() =>
            {
                var pool = ArrayPool<string>.Create();
                var pooledArray = pool.Rent(5);
                pool.Return(pooledArray);
                pooledArray = pool.Rent(5);
            }).Wait();

            // Http Client also has one. Do an Http
            var client = new HttpClient();
            var resp = client.GetAsync("https://www.bing.com").Result;
            resp.EnsureSuccessStatusCode();
        }

        private static void WriteEvent(DiagnosticListener testListener, string eventName)
        {
            Console.WriteLine($"{eventName}? {testListener.IsEnabled(eventName)}");
            if (testListener.IsEnabled(eventName))
            {
                testListener.Write(eventName, new
                {
                    Message = $"Here's a message from {eventName}",
                    Key = "value"
                });
            }
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
