using System.Collections.Generic;
using System.Text;

namespace Microsoft.Extensions.Logging.EventSourceBridge
{
    internal static class MessageUtils
    {
        public static string GenerateMessage<T>(string eventName, T payload) where T: IEnumerable<KeyValuePair<string, object>>
        {
            var builder = new StringBuilder();
            builder.Append(eventName);
            builder.Append(": ");
            foreach (var pair in payload)
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
