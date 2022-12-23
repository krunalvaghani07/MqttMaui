using MqttMauiApp.Model;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MqttMauiApp
{
    public static class Serializer
    {
        public static void SerializeList()
        {
            string mainDir = AppDomain.CurrentDomain.BaseDirectory;
            string json = JsonConvert.SerializeObject(PublisherSubscriber.PublisherSubscribers);
            if (File.Exists($"{mainDir}\\wwwroot\\PubSubclient.json"))
            {
                File.Delete($"{mainDir}\\wwwroot\\PubSubclient.json");
            }
            File.WriteAllText($"{mainDir}\\wwwroot\\PubSubclient.json", json);
        }
        public static void DeserializeList()
        {
            try
            {
                string mainDir = AppDomain.CurrentDomain.BaseDirectory;
                if (File.Exists($"{mainDir}\\wwwroot\\PubSubclient.json"))
                {
                    string data = File.ReadAllText($"{mainDir}\\wwwroot\\PubSubclient.json");
                    var format = "yyyy'-'MM'-'dd'T'HH':'mm':'ss.fffffffK"; // your datetime format
                    var dateTimeConverter = new IsoDateTimeConverter { DateTimeFormat = format };
                    if (data != "")
                    {
                        PublisherSubscriber.PublisherSubscribers = JsonConvert.DeserializeObject<List<PublisherSubscriber>>(data);
                    }
                }
            }
            catch (Exception ex)
            {

            }
        }
    }
}
