using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Text;

namespace RabbitMQAzureMetrics.Extensions
{
    public static class JTokenExtension
    {
        public static T ValueFromPath<T>(this JToken jToken, string path)
        {
            var res = jToken.SelectToken(path);
            if (res == null) return default(T);
            return res.Value<T>();
        }
    }
}
