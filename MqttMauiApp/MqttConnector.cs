﻿using MqttMauiApp.Model;
using MQTTnet;
using MQTTnet.Client;
using MQTTnet.Packets;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
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
                string path = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
                var factory = new MqttFactory();
                _client = factory.CreateMqttClient();
                switch (mqttClientModel.Type)
                {
                    case ProtocolType.mqtt_tcp:
                        _options = TcpMqttClientOptions(mqttClientModel);
                        break;
                    case ProtocolType.mqtt_tls:
                        _options = TlsMqttClientOptions(mqttClientModel, $@"{path}\wwwroot\broker.emqx.io-ca.crt");
                        break;
                    case ProtocolType.ws:
                        _options = WsMqttClientOptions(mqttClientModel);
                        break;
                    case ProtocolType.wss:
                        _options = WssMqttClientOptions(
                           mqttClientModel,
                            $@"{path}\wwwroot\broker.emqx.io-ca.crt"
                        );
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

        private static MqttClientOptions TcpMqttClientOptions(MqttClientModel mqttClientModel)
        {
            return new MqttClientOptionsBuilder()
                        .WithClientId(mqttClientModel.ClientId)
                        .WithTcpServer(mqttClientModel.Host)
                        .WithProtocolVersion(MQTTnet.Formatter.MqttProtocolVersion.V500)
                        .WithCredentials(mqttClientModel.UserName, mqttClientModel.Password)
                        .WithCleanSession()
                        .Build();
        }

        private static MqttClientOptions TlsMqttClientOptions(MqttClientModel mqttClientModel, string caFile)
        {
            return new MqttClientOptionsBuilder()
                        .WithClientId(mqttClientModel.ClientId)
                        .WithTcpServer(mqttClientModel.Host)
                         .WithProtocolVersion(MQTTnet.Formatter.MqttProtocolVersion.V500)
                         .WithCredentials(mqttClientModel.UserName, mqttClientModel.Password)
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

        private static MqttClientOptions WsMqttClientOptions(MqttClientModel mqttClientModel)
        {
            return new MqttClientOptionsBuilder()
                        .WithClientId(mqttClientModel.ClientId)
                        .WithWebSocketServer(mqttClientModel.Host)
                         .WithProtocolVersion(MQTTnet.Formatter.MqttProtocolVersion.V500)
                         .WithCredentials(mqttClientModel.UserName, mqttClientModel.Password)
                        .WithCleanSession()
                        .Build();
        }

        private static MqttClientOptions WssMqttClientOptions(MqttClientModel mqttClientModel, string caFile)
        {
            return new MqttClientOptionsBuilder()
                         .WithClientId(mqttClientModel.ClientId)
                        .WithWebSocketServer(mqttClientModel.Host)
                         .WithProtocolVersion(MQTTnet.Formatter.MqttProtocolVersion.V500)
                         .WithCredentials(mqttClientModel.UserName, mqttClientModel.Password)
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
