using System;
using System.Collections.Generic;
using System.Diagnostics.Tracing;
using System.Text;

namespace Microsoft.Extensions.Logging.EventSourceBridge
{
    // Listens for EventSources to be created and provides an IObservable for the creator to subscribe to.
    internal class EventSourceListener : EventListener
    {
        private SingleSubscriberReplaySubject<EventSource> _sources = new SingleSubscriberReplaySubject<EventSource>();

        public EventSourceListener()
        {
        }

        public IObservable<EventSource> Sources => _sources;

        protected override void OnEventSourceCreated(EventSource eventSource)
        {
            _sources.OnNext(eventSource);
        }

        protected override void OnEventWritten(EventWrittenEventArgs eventData)
        {
            // Do nothing.
        }
    }
}
