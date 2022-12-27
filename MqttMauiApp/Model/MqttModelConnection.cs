using MQTTnet.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MqttMauiApp.Model
{
    public class MqttModelConnection
    {
        public IMqttClient Client { get; set; }
        public MqttClientModel MqttClientModel { get; set; }
        public bool IsConnected { get; set; }
        public DateTime LastConnected { get; set; }
        public MqttConnector MqttConnector { get; set; }
    }
}
