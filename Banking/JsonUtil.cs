using System;
using Newtonsoft.Json;

namespace Banking
{
    public class JsonUtil
    {
        public JsonSerializerSettings Settings => new JsonSerializerSettings
        {
            DateFormatString = "dd/MM/yyyy hh:mm:ss tt",
            DateTimeZoneHandling = DateTimeZoneHandling.Local
        };

        public T Deserialize<T>(string json)
        {
            return JsonConvert.DeserializeObject<T>(json, Settings);
        }
    }
}
