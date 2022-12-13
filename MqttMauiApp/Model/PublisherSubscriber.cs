using MQTTnet;
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
        public List<string> SentMessages { get; set; }
        public List<MqttApplicationMessage> RecievedMessages { get; set; }
        public Qos QosType { get; set; }
        public bool IsSubscribed { get; set; }
        
    }
    public enum TopicType
    {
         Publisher,
         Subscriber
    }
    public enum Qos
    {
        Almost,
        Atleast,
        Exactly
    }
}
