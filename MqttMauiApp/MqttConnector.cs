using MqttMauiApp.Model;
using MQTTnet;
using MQTTnet.Client;
using MQTTnet.Packets;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;

namespace MqttMauiApp
{
    public class MqttConnector
    {
        public static IMqttClient _client;
        public static MqttClientOptions _options;
        private readonly MqttClientModel mqtt; 
        public MqttConnector(MqttClientModel mqttClient)
        {
            mqtt = mqttClient;
        }
       public IMqttClient Connect(MqttClientModel mqttClientModel)
        {
            try
            {
                var factory = new MqttFactory();
                _client = factory.CreateMqttClient();
                switch (mqttClientModel.Type)
                {
                    case "tcp":
                        _options = TcpMqttClientOptions("broker.emqx.io");
                        Console.WriteLine("tcp connection");
                        break;
                    case "tls":
                        _options = TlsMqttClientOptions("broker.emqx.io", @"D:\Krunal\MqttBoxWPF\MqttBoxWPF\broker.emqx.io-ca.crt");
                        Console.WriteLine("tls connection");
                        break;
                    case "ws":
                        _options = WsMqttClientOptions("broker.emqx.io:8083/mqtt");
                        Console.WriteLine("ws connection");
                        break;
                    case "wss":
                        _options = WssMqttClientOptions(
                            "broker.emqx.io:8084/mqtt",
                            @"D:\Krunal\MqttBoxWPF\MqttBoxWPF\broker.emqx.io-ca.crt"
                        );
                        Console.WriteLine("wss connection");
                        break;
                }
                //_options = new MqttClientOptionsBuilder()
                //    //.WithClientId(mqttClientModel.ClientId)
                //    .WithTcpServer(mqttClientModel.Host)
                //    .WithProtocolVersion(MQTTnet.Formatter.MqttProtocolVersion.V500)
                //    //.WithCredentials(username: mqttClientModel.UserName, password: mqttClientModel.Password)
                //    .WithCleanSession()
                //    .Build();

                _client.ConnectAsync(_options).Wait();
              return  _client;

            }
            catch(Exception ex)
            {
                Console.WriteLine("Error");
                return _client;
            }
        }
        public void PublishTopic(string topicname,string message,Qos qos)
        {
            if(message != null)
            {
                MqttApplicationMessage sendmessage;
                switch (qos)
                {
                    case Qos.Atleast:
                         sendmessage = new MqttApplicationMessageBuilder()
                           .WithTopic(topicname)
                           .WithPayload(message)
                           .WithQualityOfServiceLevel(MQTTnet.Protocol.MqttQualityOfServiceLevel.AtLeastOnce)
                           .WithRetainFlag()
                           .Build();
                        break;
                    case Qos.Almost:
                         sendmessage = new MqttApplicationMessageBuilder()
                           .WithTopic(topicname)
                           .WithPayload(message)
                           .WithQualityOfServiceLevel(MQTTnet.Protocol.MqttQualityOfServiceLevel.AtMostOnce)
                           .WithRetainFlag()
                           .Build();
                        break;
                    case Qos.Exactly:
                         sendmessage = new MqttApplicationMessageBuilder()
                           .WithTopic(topicname)
                           .WithPayload(message)
                           .WithQualityOfServiceLevel(MQTTnet.Protocol.MqttQualityOfServiceLevel.ExactlyOnce)
                           .WithRetainFlag()
                           .Build();
                        break;
                    default:
                        sendmessage = new MqttApplicationMessageBuilder()
                           .WithTopic(topicname)
                           .WithPayload(message)
                           .WithQualityOfServiceLevel(MQTTnet.Protocol.MqttQualityOfServiceLevel.AtMostOnce)
                           .WithRetainFlag()
                           .Build();
                        break;

                }
               
                if (_client.IsConnected)
                {

                    _client.PublishAsync(sendmessage);
                }
            }
           

        }
        public bool Subscribe(string topicname, Qos qos)
        {
            MqttTopicFilter topicbuilder;
            switch (qos)
            {
                case Qos.Atleast:
                    topicbuilder = new MqttTopicFilterBuilder().WithTopic(topicname).WithAtLeastOnceQoS().Build();
                    break;
                case Qos.Almost:
                    topicbuilder = new MqttTopicFilterBuilder().WithTopic(topicname).WithAtMostOnceQoS().Build();
                    break;
                case Qos.Exactly:
                    topicbuilder = new MqttTopicFilterBuilder().WithTopic(topicname).WithExactlyOnceQoS().Build();
                    break;
                default:
                    topicbuilder = new MqttTopicFilterBuilder().WithTopic(topicname).WithAtMostOnceQoS().Build();
                    break;

            }
            if (_client.IsConnected)
            {
                _client.SubscribeAsync(topicbuilder).Wait();
                return true;
            }
            else
            {
                return false;
            }
        }

        private static MqttClientOptions TcpMqttClientOptions(string url)
        {
            return new MqttClientOptionsBuilder()
                        .WithClientId("EMQX_" + Guid.NewGuid().ToString())
                        .WithTcpServer(url)
                        .WithProtocolVersion(MQTTnet.Formatter.MqttProtocolVersion.V500)
                        //.WithCredentials("user", "pass")
                        .WithCleanSession()
                        .Build();
        }

        private static MqttClientOptions TlsMqttClientOptions(string url, string caFile)
        {
            return new MqttClientOptionsBuilder()
                        .WithClientId("EMQX_" + Guid.NewGuid().ToString())
                        .WithTcpServer(url)
                         .WithProtocolVersion(MQTTnet.Formatter.MqttProtocolVersion.V500)
                        //.WithCredentials("user", "pass")
                        .WithCleanSession()
                        .WithTls(
                            new MqttClientOptionsBuilderTlsParameters()
                            {
                                UseTls = true,
                                SslProtocol = System.Security.Authentication.SslProtocols.Tls12,
                                Certificates = new List<X509Certificate>()
                                {
                                    // Download from https://www.emqx.com/en/mqtt/public-mqtt5-broker
                                    X509Certificate.CreateFromCertFile(caFile)
                                }
                            }
                        )
                        .Build();
        }

        private static MqttClientOptions WsMqttClientOptions(string url)
        {
            return new MqttClientOptionsBuilder()
                        .WithClientId("EMQX_" + Guid.NewGuid().ToString())
                        .WithWebSocketServer(url)
                         .WithProtocolVersion(MQTTnet.Formatter.MqttProtocolVersion.V500)
                        //.WithCredentials("user", "pass")
                        .WithCleanSession()
                        .Build();
        }

        private static MqttClientOptions WssMqttClientOptions(string url, string caFile)
        {
            return new MqttClientOptionsBuilder()
                        .WithClientId("EMQX_" + Guid.NewGuid().ToString())
                        .WithWebSocketServer(url)
                         .WithProtocolVersion(MQTTnet.Formatter.MqttProtocolVersion.V500)
                        //.WithCredentials("user", "pass")
                        .WithCleanSession()
                        .WithTls(
                            new MqttClientOptionsBuilderTlsParameters()
                            {
                                UseTls = true,
                                SslProtocol = System.Security.Authentication.SslProtocols.Tls12,
                                Certificates = new List<X509Certificate>()
                                {
                                    // Download from https://www.emqx.com/en/mqtt/public-mqtt5-broker
                                    X509Certificate.CreateFromCertFile(caFile)
                                }
                            }
                        )
                        .Build();
        }
    }
}
