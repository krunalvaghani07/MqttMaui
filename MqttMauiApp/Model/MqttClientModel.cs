using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MqttMauiApp.Model
{
    public class MqttClientModel
    {
        public string ClientName { get; set; }
        public string ClientId { get; set; }
        public string Host { get; set; }
        public int Port { get; set; }
        public string UserName { get; set; }
        public string Password { get; set; }
        public string Type { get; set; }

        public static List<MqttClientModel> mqttClients = new List<MqttClientModel>()
                                                    {
                new MqttClientModel() {Type="tcp", Port=1883, ClientName = "tcpClient",ClientId="1",Host="broker.emqx.io",UserName="krunal",Password="vaghani" },
                new MqttClientModel() {Type= "tls", Port=1883, ClientName = "TlsClient",ClientId="2",Host="broker.emqx.io",UserName="krunal",Password="vaghani" },
                new MqttClientModel() {Type= "ws", Port=8083, ClientName = "WsClient",ClientId="2",Host="broker.emqx.io:8083/mqtt",UserName="krunal",Password="vaghani" },
                new MqttClientModel() {Type= "wss", Port=8083, ClientName = "WssClient",ClientId="2",Host="broker.emqx.io:8083/mqtt",UserName="krunal",Password="vaghani" }
                                                    };
    }
}
