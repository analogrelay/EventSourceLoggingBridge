using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace Microsoft.Extensions.Logging.EventSourceBridge
{
    internal class SingleSubscriberReplaySubject<T> : IObservable<T>
    {
        private IObserver<T> _subscriber;
        private List<T> _catchUpList = new List<T>();
        private object _lock = new object();

        public IDisposable Subscribe(IObserver<T> observer)
        {
            lock (_lock)
            {
                if (_subscriber != null)
                {
                    throw new InvalidOperationException("Only a single subscriber is allowed, and there is already a subscriber");
                }

                // Fire all the catch-up events
                foreach(var catchUp in _catchUpList)
                {
                    observer.OnNext(catchUp);
                }
                _catchUpList = null;
                _subscriber = observer;
            }

            // There is no unsubscribing. Mwahahahaha
            return null;
        }

        public void OnNext(T value)
        {
            if (_subscriber == null)
            {
                lock (_lock)
                {
                    if (_subscriber == null)
                    {
                        Debug.Assert(_catchUpList != null, "Catch-up list was null but subscriber was not yet present!");
                        _catchUpList.Add(value);
                        return;
                    }
                }
            }

            // The subscriber is here, we don't need to lock
            _subscriber.OnNext(value);
        }

        public void OnError(Exception ex)
        {
            // No support for errors.
        }

        public void OnCompleted()
        {
            // No support for completion.
        }
    }
}
