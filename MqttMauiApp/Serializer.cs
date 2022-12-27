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
            string mainDir = MqttClientModel.MainDirPath;
            string json = JsonConvert.SerializeObject(PublisherSubscriber.PublisherSubscribers);
            //if (File.Exists($"{mainDir}//wwwroot//PubSubclient.json"))
            //{
            //    File.Delete($"{mainDir}//wwwroot//PubSubclient.json");
            //}
            //File.WriteAllText($"{mainDir}//wwwroot//PubSubclient.json", json);
        }
        public static void DeserializeList()
        {
            try
            {
                string mainDir = MqttClientModel.MainDirPath;
                if (File.Exists($"{mainDir}//wwwroot//PubSubclient.json"))
                {
                    string data = File.ReadAllText($"{mainDir}//wwwroot//PubSubclient.json");
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
