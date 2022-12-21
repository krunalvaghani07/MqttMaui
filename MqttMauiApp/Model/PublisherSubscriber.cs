using MQTTnet;
using MQTTnet.Protocol;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MqttMauiApp.Model
{
    public class PublisherSubscriber
    {
        public int Id { get; set; }
        public string ClientId { get; set; }
        public string TopicName { get; set; }
        public string PublishMessage { get; set; }
        public TopicType topicType { get; set; }
        public List<SentMessage>  SentMessages { get; set; }
        public List<RecievedMessage> RecievedMessages { get; set; }
        public Qos QosType { get; set; }
        public bool IsSubscribed { get; set; }
        public static List<PublisherSubscriber> PublisherSubscribers { get; set; } = new List<PublisherSubscriber>();
        public DateTime SendRecTime { get; set; }
        public bool isRetain { get; set; }
        public MqttQualityOfServiceLevel RecQOS { get; set; }
    }
    public struct SentMessage
    {
        public SentMessage(string msg, DateTime time, Qos qos)
        {
            Msg = msg;
            PubTime = time;
            QOS = qos;
        }

        public string Msg { get; private set; }
        public DateTime PubTime { get; private set; }
        public Qos QOS { get;  set; }
       
    }
    public struct RecievedMessage
    {
        public RecievedMessage(MqttApplicationMessage msg, DateTime time, MqttQualityOfServiceLevel qos)
        {
            Msg = msg;
            RecTime = time;
            QOS = qos;
        }
        public MqttQualityOfServiceLevel QOS { get;  set; }

        public MqttApplicationMessage Msg { get; private set; }
        public DateTime RecTime { get; private set; }
    }
    public enum TopicType
    {
         Publisher,
         Subscriber
    }
    public enum Qos
    {
        Atmost,
        Atleast,
        Exactly
    }
}
