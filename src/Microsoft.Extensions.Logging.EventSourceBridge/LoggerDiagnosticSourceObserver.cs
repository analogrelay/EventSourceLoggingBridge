using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Microsoft.Extensions.Logging.EventSourceBridge
{
    internal class LoggerDiagnosticSourceObserver : IObserver<KeyValuePair<string, object>>
    {
        private ILogger _logger;

        public LoggerDiagnosticSourceObserver(ILogger logger)
        {
            _logger = logger;
        }

        void IObserver<KeyValuePair<string, object>>.OnCompleted()
        {
        }

        void IObserver<KeyValuePair<string, object>>.OnError(Exception error)
        {
        }

        void IObserver<KeyValuePair<string, object>>.OnNext(KeyValuePair<string, object> value)
        {
            // REVIEW: How to get the level?
            // REVIEW: What to do about event id?
            _logger.Log(
                LogLevel.Trace,
                new EventId(0, value.Key),
                value.Value,
                exception: null,
                formatter: CreateFormatter(value.Key));
        }

        private static Func<object, Exception, string> CreateFormatter(string name)
        {
            return (state, _) =>
            {
                // TODO: Reflection.Emit with a cache?

                // Check for a message property
                var properties = state.GetType().GetTypeInfo().DeclaredProperties;
                var messageProperty = properties.FirstOrDefault(p => p.Name.Equals("Message") && p.PropertyType == typeof(string));

                if (messageProperty != null)
                {
                    return (string)messageProperty.GetValue(state);
                }
                else
                {
                    return MessageUtils.GenerateMessage(name, new ReflectionKeyValuePairEnumerable(state, properties));
                }
            };
        }

        private struct ReflectionKeyValuePairEnumerable : IEnumerable<KeyValuePair<string, object>>
        {
            private readonly object _instance;
            private readonly IEnumerable<PropertyInfo> _properties;

            public ReflectionKeyValuePairEnumerable(object instance, IEnumerable<PropertyInfo> properties)
            {
                _instance = instance;
                _properties = properties;
            }

            public IEnumerator<KeyValuePair<string, object>> GetEnumerator()
            {
                return new Enumerator(_instance, _properties.GetEnumerator());
            }

            IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
        }

        private struct Enumerator : IEnumerator<KeyValuePair<string, object>>
        {
            private object _instance;
            private IEnumerator<PropertyInfo> _propertyEnumerator;

            public KeyValuePair<string, object> Current => new KeyValuePair<string, object>(_propertyEnumerator.Current.Name, _propertyEnumerator.Current.GetValue(_instance));
            object IEnumerator.Current => Current;

            public Enumerator(object instance, IEnumerator<PropertyInfo> propertyEnumerator)
            {
                _instance = instance;
                _propertyEnumerator = propertyEnumerator;
            }

            public void Dispose() => _propertyEnumerator.Dispose();
            public bool MoveNext() => _propertyEnumerator.MoveNext();
            public void Reset() => _propertyEnumerator.Reset();
        }
    }
}
