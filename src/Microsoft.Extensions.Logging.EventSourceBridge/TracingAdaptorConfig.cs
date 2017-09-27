using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.Tracing;

namespace Microsoft.Extensions.Logging.EventSourceBridge
{
    internal class TracingAdaptorConfig : ITracingAdaptorConfig, IObserver<EventSource>, IObserver<DiagnosticSource>
    {
        private object _lock = new object();
        private Dictionary<string, EventSourceEnableRequest> _eventSourceRequests = new Dictionary<string, EventSourceEnableRequest>();
        private Dictionary<string, DiagnosticSourceEnableRequest> _diagnosticSourceRequests = new Dictionary<string, DiagnosticSourceEnableRequest>();
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
            _eventSourceRequests[eventSourceName] = new EventSourceEnableRequest()
            {
                EventSourceName = eventSourceName,
                MinLevel = minLevel,
                Keywords = keywords,
                Arguments = arguments
            };

            return this;
        }

        public ITracingAdaptorConfig EnableDiagnosticSource(string diagnosticSourceName, IReadOnlyList<string> enabledEventNames)
        {
            // These should be called synchronously, and before any OnNext calls are made
            var events = enabledEventNames == null || enabledEventNames.Count == 0 ?
                null :
                new HashSet<string>(enabledEventNames);
            _diagnosticSourceRequests[diagnosticSourceName] = new DiagnosticSourceEnableRequest()
            {
                DiagnosticSourceName = diagnosticSourceName,
                Events = events
            };

            return this;
        }

        void IObserver<EventSource>.OnNext(EventSource eventSource)
        {
            _logger.LogTrace("EventSource '{eventSourceName}' (ID: '{eventSourceId}') created.", eventSource.Name, eventSource.Guid);

            // The dictionary is never modified after the first call to OnNext.
            // According to https://msdn.microsoft.com/en-us/library/xfhwa508%28v=vs.110%29.aspx, Dictionary READS are thread-safe,
            // multiple reads can occur simultaneously.
            if (_eventSourceRequests.TryGetValue(eventSource.Name, out var request))
            {
                var logger = _loggerFactory.CreateLogger(eventSource.Name);

                // Because of the weird way EventListener works, we don't need to hold on to the instance.
                _ = new LoggerEventListener(logger, eventSource, request.MinLevel, request.Keywords, request.Arguments);
            }
        }

        void IObserver<DiagnosticSource>.OnNext(DiagnosticSource value)
        {
            if (!(value is DiagnosticListener listener))
            {
                // We need a name, which only DiagnosticListener has
                return;
            }

            _logger.LogTrace("DiagnosticSource '{diagnosticSourceName}' created.", listener.Name);

            if (_diagnosticSourceRequests.TryGetValue(listener.Name, out var request))
            {
                var logger = _loggerFactory.CreateLogger(listener.Name);
                if (request.Events != null && request.Events.Count > 0)
                {
                    listener.Subscribe(new LoggerDiagnosticSourceObserver(logger), name => request.Events.Contains(name));
                }
                else
                {
                    listener.Subscribe(new LoggerDiagnosticSourceObserver(logger));
                }
            }
        }

        void IObserver<EventSource>.OnCompleted() { }

        void IObserver<EventSource>.OnError(Exception error) { }

        void IObserver<DiagnosticSource>.OnCompleted() { }

        void IObserver<DiagnosticSource>.OnError(Exception error) { }

        private struct DiagnosticSourceEnableRequest
        {
            public string DiagnosticSourceName { get; set; }
            public HashSet<string> Events { get; set; }
        }

        private struct EventSourceEnableRequest
        {
            public string EventSourceName { get; set; }
            public EventLevel MinLevel { get; set; }
            public EventKeywords Keywords { get; set; }
            public IDictionary<string, string> Arguments { get; set; }
        }
    }
}
