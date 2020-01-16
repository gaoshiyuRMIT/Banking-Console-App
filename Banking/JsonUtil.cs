using System;

using Newtonsoft.Json;

namespace Banking
{
    public class JsonUtil
    {
        public JsonSerializerSettings Settings => new JsonSerializerSettings
        {
            DateFormatString = Transaction.DateTimeFormat,
            DateTimeZoneHandling = DateTimeZoneHandling.Utc
        };

        public T Deserialize<T>(string json)
        {
            return JsonConvert.DeserializeObject<T>(json, Settings);
        }
    }
}
