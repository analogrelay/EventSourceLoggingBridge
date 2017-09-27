using System.Collections.Generic;
using System.Diagnostics.Tracing;
using Microsoft.Extensions.Logging.EventSourceBridge;

namespace Microsoft.Extensions.Logging
{
    public static class TracingAdaptorConfigExtensions
    {
        private static readonly IDictionary<string, string> _emptyDictionary = ReadOnlyEmptyDictionary<string, string>.Instance;

        public static ITracingAdaptorConfig EnableEventSource(this ITracingAdaptorConfig config, string eventSourceName)
        {
            // REVIEW: Default level? Or maybe just remove this overload?
            return config.EnableEventSource(eventSourceName, EventLevel.Informational, EventKeywords.None, ReadOnlyEmptyDictionary<string, string>.Instance);
        }

        public static ITracingAdaptorConfig EnableEventSource(this ITracingAdaptorConfig config, string eventSourceName, EventLevel minLevel)
        {
            return config.EnableEventSource(eventSourceName, minLevel, EventKeywords.None, ReadOnlyEmptyDictionary<string, string>.Instance);
        }

        public static ITracingAdaptorConfig EnableEventSource(this ITracingAdaptorConfig config, string eventSourceName, EventLevel minLevel, EventKeywords keywords)
        {
            return config.EnableEventSource(eventSourceName, minLevel, keywords, ReadOnlyEmptyDictionary<string, string>.Instance);
        }
    }
}
