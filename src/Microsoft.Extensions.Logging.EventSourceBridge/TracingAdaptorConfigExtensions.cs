using System;
using System.Collections.Generic;
using System.Diagnostics.Tracing;
using Microsoft.Extensions.Logging.EventSourceBridge;

namespace Microsoft.Extensions.Logging
{
    public static class TracingAdaptorConfigExtensions
    {
        private static readonly IDictionary<string, string> _emptyDictionary = new Dictionary<string, string>();

        public static ITracingAdaptorConfig EnableEventSource(this ITracingAdaptorConfig config, string eventSourceName)
        {
            // REVIEW: Default level? Or maybe just remove this overload?
            return config.EnableEventSource(eventSourceName, EventLevel.Informational, EventKeywords.None, _emptyDictionary);
        }

        public static ITracingAdaptorConfig EnableEventSource(this ITracingAdaptorConfig config, string eventSourceName, EventLevel minLevel)
        {
            return config.EnableEventSource(eventSourceName, minLevel, EventKeywords.None, _emptyDictionary);
        }

        public static ITracingAdaptorConfig EnableEventSource(this ITracingAdaptorConfig config, string eventSourceName, EventLevel minLevel, EventKeywords keywords)
        {
            return config.EnableEventSource(eventSourceName, minLevel, keywords, _emptyDictionary);
        }

        public static ITracingAdaptorConfig EnableEventSource(this ITracingAdaptorConfig config, EventSource eventSource)
        {
            return config.EnableEventSource(eventSource.Name, EventLevel.Informational, EventKeywords.None, _emptyDictionary);
        }

        public static ITracingAdaptorConfig EnableEventSource(this ITracingAdaptorConfig config, EventSource eventSource, EventLevel minLevel)
        {
            return config.EnableEventSource(eventSource.Name, minLevel, EventKeywords.None, _emptyDictionary);
        }

        public static ITracingAdaptorConfig EnableEventSource(this ITracingAdaptorConfig config, EventSource eventSource, EventLevel minLevel, EventKeywords keywords)
        {
            return config.EnableEventSource(eventSource.Name, minLevel, keywords, _emptyDictionary);
        }


        public static ITracingAdaptorConfig EnableDiagnosticSource(this ITracingAdaptorConfig config, string diagnosticSourceName)
        {
            return config.EnableDiagnosticSource(diagnosticSourceName, Array.Empty<string>());
        }

        public static ITracingAdaptorConfig EnableDiagnosticSource(this ITracingAdaptorConfig config, string diagnosticSourceName, params string[] eventNames)
        {
            return config.EnableDiagnosticSource(diagnosticSourceName, eventNames);
        }
    }
}
