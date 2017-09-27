using System;
using System.Diagnostics;
using System.Diagnostics.Tracing;
using Microsoft.Extensions.Logging.EventSourceBridge;

namespace Microsoft.Extensions.Logging
{
    /// <summary>
    /// Provides extension methods 
    /// </summary>
    public static class EventSourceBridgeLoggerFactoryExtensions
    {
        /// <summary>
        /// Attaches a listener to the specified <see cref="EventSource"/> and pipes the events sent on that source to the logger providers.
        /// </summary>
        /// <param name="loggerFactory">The <see cref="ILoggerFactory"/> to forward events to</param>
        /// <returns>The logger factory</returns>
        public static ILoggerFactory ImportEventSources(this ILoggerFactory loggerFactory, Action<ITracingAdaptorConfig> configure)
        {
            var eventSources = new EventSourceListener();

            var builder = new TracingAdaptorConfig(loggerFactory);

            configure(builder);

            eventSources.Sources.Subscribe(builder);
            DiagnosticListener.AllListeners.Subscribe(builder);

            return loggerFactory;
        }
    }
}
