using System.Collections.Generic;
using System.Diagnostics.Tracing;

namespace Microsoft.Extensions.Logging
{
    public interface ITracingAdaptorConfig
    {
        ITracingAdaptorConfig EnableEventSource(string eventSourceName, EventLevel minLevel, EventKeywords keywords, IDictionary<string, string> arguments);
    }
}
