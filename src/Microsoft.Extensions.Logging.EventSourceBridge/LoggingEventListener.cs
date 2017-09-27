using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.Tracing;
using System.Linq;
using System.Text;

namespace Microsoft.Extensions.Logging.EventSourceBridge
{
    internal class LoggerEventListener : EventListener
    {
        private ILogger _logger;

        public LoggerEventListener(ILogger logger, EventSource eventSource, EventLevel minLevel, EventKeywords keywords, IDictionary<string, string> arguments)
        {
            _logger = logger;

            EnableEvents(eventSource, minLevel, keywords, arguments);
        }

        protected override void OnEventWritten(EventWrittenEventArgs eventData)
        {
            var eventName = eventData.Opcode == EventOpcode.Info ?
                eventData.EventName :
                $"{eventData.EventName}/{eventData.Opcode}";
            _logger.Log(
                MapLogLevel(eventData.Level),
                new EventId(eventData.EventId, eventName),
                new EventData(eventData),
                exception: null,
                formatter: FormatEventData);
        }

        private LogLevel MapLogLevel(EventLevel level)
        {
            switch (level)
            {
                case EventLevel.Critical:
                    return LogLevel.Critical;
                case EventLevel.Error:
                    return LogLevel.Error;
                case EventLevel.Warning:
                    return LogLevel.Warning;
                case EventLevel.Informational:
                    return LogLevel.Information;
                case EventLevel.Verbose:
                    return LogLevel.Debug;
                default:
                    return LogLevel.Trace;
            }
        }

        // We never pass an exception to the logger, so the Exception argument will always be null
        private string FormatEventData(EventData evt, Exception _)
        {
            try
            {
                if (!string.IsNullOrEmpty(evt.Event.Message))
                {
                    // Ugh, we have to ToArray the payload to put it in string.Format. But let's do it only if the type isn't actually an object[]
                    return string.Format(evt.Event.Message, evt.Event.Payload.ToArray());
                }

                var messageIndex = evt.Event.PayloadNames.IndexOf("message");
                if(messageIndex >= 0)
                {
                    Debug.Assert(messageIndex < evt.Event.Payload.Count && messageIndex > 0);
                    return evt.Event.Payload[messageIndex] as string ?? string.Empty;
                }

                return GenerateMessage(evt);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error formatting event: {providerName}({provderId})/{eventName}({eventId})", evt.Event.EventSource.Name, evt.Event.EventSource.Guid, evt.Event.EventName, evt.Event.EventId);

                // Swallow the exception.
                return string.Empty;
            }
        }

        private string GenerateMessage(EventData evt)
        {
            var builder = new StringBuilder();
            builder.Append(evt.Event.EventName);
            builder.Append(":");
            foreach (var pair in evt)
            {
                builder.Append(pair.Key);
                builder.Append("=");
                builder.Append(pair.Value.ToString());
                builder.Append(",");
            }
            builder.Length -= 1;
            return builder.ToString();
        }
    }
}
