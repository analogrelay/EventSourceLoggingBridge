using System;
using System.Collections.Generic;
using System.Diagnostics.Tracing;

namespace Microsoft.Extensions.Logging.EventSourceBridge
{
    internal class TracingAdaptorConfig : ITracingAdaptorConfig, IObserver<EventSource>
    {
        private object _lock = new object();
        private Dictionary<string, EventSourceEnableRequest> _requests = new Dictionary<string, EventSourceEnableRequest>();
        private readonly ILoggerFactory _loggerFactory;
        private readonly ILogger<TracingAdaptorConfig> _logger;

        public TracingAdaptorConfig(ILoggerFactory loggerFactory)
        {
            _loggerFactory = loggerFactory;
            _logger = _loggerFactory.CreateLogger<TracingAdaptorConfig>();
        }

        public ITracingAdaptorConfig EnableEventSource(string eventSourceName, EventLevel minLevel, EventKeywords keywords, IDictionary<string, string> arguments)
        {
            // These should be called synchronously, and before any OnNext calls are made
            _requests[eventSourceName] = new EventSourceEnableRequest()
            {
                EventSourceName = eventSourceName,
                MinLevel = minLevel,
                Keywords = keywords,
                Arguments = arguments
            };

            return this;
        }

        void IObserver<EventSource>.OnNext(EventSource eventSource)
        {
            _logger.LogTrace("EventSource '{eventSourceName}' (ID: '{eventSourceId}') created.", eventSource.Name, eventSource.Guid);

            // The dictionary is never modified after the first call to OnNext.
            // According to https://msdn.microsoft.com/en-us/library/xfhwa508%28v=vs.110%29.aspx, Dictionary READS are thread-safe,
            // multiple reads can occur simultaneously.
            if(_requests.TryGetValue(eventSource.Name, out var request))
            {
                var logger = _loggerFactory.CreateLogger(eventSource.Name);

                // Because of the weird way EventListener works, we don't need to hold on to the instance.
                _ = new LoggerEventListener(logger, eventSource, request.MinLevel, request.Keywords, request.Arguments);
            }
        }

        void IObserver<EventSource>.OnCompleted() { }

        void IObserver<EventSource>.OnError(Exception error) { }

        private struct EventSourceEnableRequest
        {
            public string EventSourceName { get; set; }
            public EventLevel MinLevel { get; set; }
            public EventKeywords Keywords { get; set; }
            public IDictionary<string, string> Arguments { get; set; }
        }
    }
}
