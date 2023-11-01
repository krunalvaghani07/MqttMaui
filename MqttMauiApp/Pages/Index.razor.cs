using MqttMauiApp.Model;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MqttMauiApp.Pages
{
    public partial class Index
    {
        string pubtopicName = "";
        string subtopicName = "";
        string pubMsg = "";
        Qos PubType;
        Qos SubType;
        bool IsSubOpen = false;
        bool IsClientOpen = false;
        bool IsDeleteClientOpen = false;
        List<MqttModelConnection> mqttClientConnector = new List<MqttModelConnection>();
        string ConfigFilePath;

        MqttModelConnection SelectedClient = new MqttModelConnection();
        MqttClientModel AddEditClient = new MqttClientModel();
        MqttClientModel DeleteClient = new MqttClientModel();
        PublisherSubscriber SelectedSubscriber = new PublisherSubscriber();
        string SelectedMsgType = "All";

        protected override async Task OnInitializedAsync()
        {
            LoadClients();
            if (MqttClientModel.mqttClients.Count > 0)
            {
                mqttClientConnector.Clear();
                foreach (var clientmodel in MqttClientModel.mqttClients)
                {
                    ConnectToMqtt(clientmodel);
                }

            }
            if (mqttClientConnector.Count > 0)
            {
                SelectedClient = mqttClientConnector[0];
                PubType = mqttClientConnector[0].MqttClientModel.QosType;
                SubType = mqttClientConnector[0].MqttClientModel.QosType;
            }
            Serializer.DeserializeList();
            PublisherSubscriber.PublisherSubscribers.ForEach(ch => ch.IsSubscribed = false);
            StateHasChanged();
        }

        private void LoadClients()
        {
            string mainDir = MqttClientModel.MainDirPath;
            if (File.Exists($@"{mainDir}//wwwroot//clientconfig.txt"))
            {
                var path = File.ReadAllText($@"{mainDir}//wwwroot//clientconfig.txt");
                if (File.Exists(path))
                {
                    ConfigFilePath = path;
                    var data = File.ReadAllText(ConfigFilePath);
                    MqttClientModel.mqttClients = JsonConvert.DeserializeObject<List<MqttClientModel>>(data);
                }
                else
                {
                    ConfigFilePath = $@"{mainDir}//wwwroot//client.json";
                    var data = File.ReadAllText(ConfigFilePath);
                    MqttClientModel.mqttClients = JsonConvert.DeserializeObject<List<MqttClientModel>>(data);
                }
            }
            else
            {
                ConfigFilePath = $@"{mainDir}//wwwroot//client.json";
                if (File.Exists($@"{mainDir}//wwwroot//client.json"))
                {
                    var data = File.ReadAllText($@"{mainDir}//wwwroot//client.json");
                    MqttClientModel.mqttClients = JsonConvert.DeserializeObject<List<MqttClientModel>>(data);
                    //mqttClientModels = MqttClientModel.mqttClients;
                }
            }
            MqttClientModel.ConfigPath = ConfigFilePath;
        }

        private void ConnectToMqtt(MqttClientModel clientmodel)
        {
            var clientModel = clientmodel;
            var mqttconnector = new MqttConnector(clientmodel);
            var client = mqttconnector.Connect(clientmodel);
            mqttClientConnector.Add(new MqttModelConnection() { MqttConnector = mqttconnector, Client = client, MqttClientModel = clientmodel, IsConnected = false });
            client.ConnectedAsync += async e =>
            {
                mqttClientConnector.Where(m => m.MqttClientModel == clientmodel).ToList().ForEach(ch => { ch.IsConnected = true; ch.LastConnected = DateTime.Now; });
                await InvokeAsync(() =>
                {
                    StateHasChanged();
                });
            };
            client.DisconnectedAsync += async e =>
            {
                mqttClientConnector.Where(m => m.MqttClientModel == clientmodel).ToList().ForEach(ch => { ch.IsConnected = false; });
                await InvokeAsync(() =>
                {
                    StateHasChanged();
                });
            };
            mqttconnector.StartConnecting();
            client.ApplicationMessageReceivedAsync += async e =>
            {
                PublisherSubscriber.PublisherSubscribers.Where(ps => ps.TopicName == e.ApplicationMessage.Topic &&
                     ps.IsSubscribed == true && ps.ClientId == clientModel.ClientId
                //  && !ps.RecievedMessages.Contains(e.ApplicationMessage)
                ).ToList().
                     ForEach(ch => ch.RecievedMessages.Add(new RecievedMessage(e.ApplicationMessage, DateTime.Now, e.ApplicationMessage.QualityOfServiceLevel)));
                File.WriteAllText($"{MqttClientModel.MainDirPath}//wwwroot//PubSubclient.json", JsonConvert.SerializeObject(PublisherSubscriber.PublisherSubscribers));

                await InvokeAsync(() =>
                {
                    StateHasChanged();
                });
            };
        }

        private async void OpenFilePicker()
        {
            var jsonFileType = new FilePickerFileType(new Dictionary<DevicePlatform, IEnumerable<string>>{
                { DevicePlatform.iOS, new[] { ".JSON" } },
                { DevicePlatform.Android, new[] { "application/JSON" } },
                { DevicePlatform.WinUI, new[] { ".json" } },
                { DevicePlatform.Tizen, new[] { ".JSON" } },
                { DevicePlatform.macOS, new[] { "json" } },
        });
            var result = await FilePicker.PickAsync(new PickOptions
            {
                FileTypes = jsonFileType,
                PickerTitle = "Select Config(json)File"
            });
            if (result == null)
            {
                return;
            }
            else
            {
                var stream = await result.OpenReadAsync();
                MqttClientModel.mqttClients = await System.Text.Json.JsonSerializer.DeserializeAsync<List<MqttClientModel>>(stream);
                ConfigFilePath = result.FullPath;
                MqttClientModel.ConfigPath = ConfigFilePath;
                if (MqttClientModel.mqttClients.Count > 0)
                {
                    mqttClientConnector.Clear();
                    foreach (var clientmodel in MqttClientModel.mqttClients)
                    {
                        ConnectToMqtt(clientmodel);
                    }

                }
                if (mqttClientConnector.Count > 0)
                {
                    SelectedClient = mqttClientConnector[0];
                    PubType = mqttClientConnector[0].MqttClientModel.QosType;
                    SubType = mqttClientConnector[0].MqttClientModel.QosType;
                }
                string mainDir = MqttClientModel.MainDirPath;
                string configpathsave = $@"{mainDir}//wwwroot//clientconfig.txt";
                //if (File.Exists(configpathsave))
                //{
                //    File.Delete(configpathsave);
                //}
                File.WriteAllText(configpathsave, MqttClientModel.ConfigPath);
                StateHasChanged();
            }
        }

        private async void ExportJson()
        {

            var pickedFolder = await FolderPicker.PickFolder();

            if (pickedFolder == null)
            {
                return;
            }
            else
            {
                string path = pickedFolder;
                string json = JsonConvert.SerializeObject(MqttClientModel.mqttClients);
                File.WriteAllText($@"{path}//client.json", json);
            }

        }

        private void DeleteModalOpen(MqttClientModel client)
        {
            IsDeleteClientOpen = true;
            DeleteClient = client;
            StateHasChanged();
        }

        private void DeleteSelectedClient(MqttClientModel client)
        {

            MqttClientModel.mqttClients.Remove(client);
            mqttClientConnector = mqttClientConnector.Where(m => m.MqttClientModel != client).ToList();
            string json = JsonConvert.SerializeObject(MqttClientModel.mqttClients);
            //if (File.Exists(ConfigFilePath))
            //{
            //    File.Delete(ConfigFilePath);
            //}
            File.WriteAllText(ConfigFilePath, json);
            if (mqttClientConnector.Count > 0)
            {
                SelectedClient = mqttClientConnector[0];
                PubType = mqttClientConnector[0].MqttClientModel.QosType;
                SubType = mqttClientConnector[0].MqttClientModel.QosType;
                InvokeAsync(StateHasChanged);
            }
            IsDeleteClientOpen = false;
            StateHasChanged();
        }
        #region For Toggling Client

        bool IsClientSelected(MqttModelConnection client)
            => SelectedClient == client;

        string DivClientStyle(MqttModelConnection client)
            => IsClientSelected(client) ? "background:rgb(27, 18, 18);color:white" : "background:white";

        void SetClientSelect(MqttModelConnection client)
        {

            SelectedClient = client;
            pubtopicName = "";
            pubMsg = "";
            PubType = client.MqttClientModel.QosType;
            SubType = client.MqttClientModel.QosType;
            SelectedSubscriber = new PublisherSubscriber();
            SelectedMsgType = "All";
        }

        #endregion

        #region For Toggling Subscriber

        bool IsSubSelected(PublisherSubscriber sub)
           => SelectedSubscriber == sub;

        string DivSubStyle(PublisherSubscriber sub)
            => IsSubSelected(sub) ? "background:rgb(48, 25, 52);color:white" : "background:white";

        void SetSubSelect(PublisherSubscriber sub)
        {
            SelectedMsgType = "All";
            SelectedSubscriber = sub;
        }
        #endregion

        #region For Toggling MsgType

        bool IsMsgTypeSelected(string msg)
        => SelectedMsgType == msg;

        string DivMsgStyle(string msg)
            => IsMsgTypeSelected(msg) ? "font-weight: bold;" : "font-weight: normal;";

        void SetMsgTypeSelect(string msg)
        {
            SelectedMsgType = msg;
        }

        #endregion

        protected async Task SaveClientSetting()
        {
            if (AddEditClient.ClientId != null)
            {
                var removeold = MqttClientModel.mqttClients.Where(s => s.ClientId == AddEditClient.ClientId).FirstOrDefault();
                MqttClientModel.mqttClients.Remove(removeold);
                var removeconnector = mqttClientConnector.Where(r => r.MqttClientModel.ClientId == AddEditClient.ClientId).FirstOrDefault();
                removeconnector.MqttConnector.Disconnect();
                mqttClientConnector.Remove(removeconnector);
            }
            else
            {
                AddEditClient.ClientId = "EMQX_" + Guid.NewGuid().ToString();
            }
            MqttClientModel.mqttClients.Add(AddEditClient);
            ConnectToMqtt(AddEditClient);
            SelectedClient = mqttClientConnector.Where(r => r.MqttClientModel.ClientId == AddEditClient.ClientId).FirstOrDefault();
            string json = JsonConvert.SerializeObject(MqttClientModel.mqttClients);
            //if (File.Exists(MqttClientModel.ConfigPath))
            //{
            //    File.Delete(MqttClientModel.ConfigPath);
            //}
            File.WriteAllText(MqttClientModel.ConfigPath, json);
            IsClientOpen = false;
            StateHasChanged();
        }

        private void OpenClientModal(bool flag)
        {
            AddEditClient = new MqttClientModel();
            if (flag)
            {
                AddEditClient = SelectedClient.MqttClientModel;
            }
            IsClientOpen = true;
            StateHasChanged();
        }

        private void Disconnect()
        {
            SelectedClient.MqttConnector.Disconnect();
            StateHasChanged();
        }

        private void Connect()
        {
            SelectedClient.MqttConnector.StartConnecting();
            StateHasChanged();
        }

        private void OpenSubModal()
        {
            IsSubOpen = true;
            subtopicName = "";
            SubType = SelectedClient.MqttClientModel.QosType;
            StateHasChanged();
        }

        private void CloseModals()
        {
            IsSubOpen = false;
            IsClientOpen = false;
            IsDeleteClientOpen = false;
        }

        private void AddPublisher()
        {
            if (SelectedClient.IsConnected)
            {
                if (pubMsg != "")
                {
                    if (PublisherSubscriber.PublisherSubscribers.Any(t => t.TopicName == pubtopicName && t.topicType == TopicType.Publisher && t.ClientId == SelectedClient.MqttClientModel.ClientId))
                    {
                        PublisherSubscriber.PublisherSubscribers.Where(ps => ps.TopicName == pubtopicName && ps.topicType == TopicType.Publisher && ps.ClientId == SelectedClient.MqttClientModel.ClientId).ToList().ForEach(ch => ch.SentMessages.Add(new SentMessage(pubMsg, DateTime.Now, PubType)));
                    }
                    else
                    {
                        PublisherSubscriber.PublisherSubscribers.Add(new PublisherSubscriber { Id = PublisherSubscriber.PublisherSubscribers.Count + 1, TopicName = pubtopicName, ClientId = SelectedClient.MqttClientModel.ClientId, topicType = TopicType.Publisher, SentMessages = new List<SentMessage>() { new SentMessage(pubMsg, DateTime.Now, PubType) } });
                    }
                    SelectedClient.MqttConnector.PublishTopic(pubtopicName, pubMsg, PubType);
                    File.WriteAllText($"{MqttClientModel.MainDirPath}//wwwroot//PubSubclient.json", JsonConvert.SerializeObject(PublisherSubscriber.PublisherSubscribers));
                }
            }
            StateHasChanged();
        }

        private void AddSubscriber()
        {
            if (SelectedClient.IsConnected)
            {
                if (!PublisherSubscriber.PublisherSubscribers.Any(t => t.TopicName == subtopicName && t.topicType == TopicType.Subscriber && t.ClientId == SelectedClient.MqttClientModel.ClientId))
                {
                    PublisherSubscriber.PublisherSubscribers.Add(new PublisherSubscriber { Id = PublisherSubscriber.PublisherSubscribers.Count + 1, IsSubscribed = true, TopicName = subtopicName, ClientId = SelectedClient.MqttClientModel.ClientId, QosType = SubType, topicType = TopicType.Subscriber, RecievedMessages = new List<RecievedMessage>() });
                }
                else
                {
                    PublisherSubscriber.PublisherSubscribers.Where(t => t.TopicName == subtopicName && t.topicType == TopicType.Subscriber && t.ClientId == SelectedClient.MqttClientModel.ClientId).ToList().ForEach(ch => ch.QosType = SubType);
                }
                SelectedClient.MqttConnector.Subscribe(subtopicName, SubType);
                File.WriteAllText($"{MqttClientModel.MainDirPath}//wwwroot//PubSubclient.json", JsonConvert.SerializeObject(PublisherSubscriber.PublisherSubscribers));

                IsSubOpen = false;
            }

            StateHasChanged();
        }

        private void SubscribeTopic(int id, string topicname, Qos qos)
        {
            PublisherSubscriber.PublisherSubscribers.Where(ps => ps.Id == id).ToList().ForEach(ch => { ch.IsSubscribed = true; });
            var flag = SelectedClient.MqttConnector.Subscribe(topicname, qos);
            StateHasChanged();
        }

        List<PublisherSubscriber> ArrangeMsg()
        {
            var lst = new List<PublisherSubscriber>();

            if (SelectedSubscriber.Id != 0)
            {
                var Send = PublisherSubscriber.PublisherSubscribers.Where(ps => ps.TopicName == SelectedSubscriber.TopicName && ps.topicType == TopicType.Publisher).ToList();
                var Rec = PublisherSubscriber.PublisherSubscribers.Where(ps => ps.TopicName == SelectedSubscriber.TopicName && ps.Id == SelectedSubscriber.Id && ps.topicType == TopicType.Subscriber).ToList();
                if (Send.Count > 0)
                {
                    Send.ForEach(sl => sl.SentMessages.ForEach(s => lst.Add(new PublisherSubscriber() { Id = sl.Id, ClientId = sl.ClientId, TopicName = sl.TopicName, topicType = TopicType.Publisher, QosType = s.QOS, PublishMessage = s.Msg, SendRecTime = s.PubTime })));
                }
                if (Rec.Count > 0)
                {
                    Rec.ForEach(sl => sl.RecievedMessages.ForEach(s => lst.Add(new PublisherSubscriber() { Id = sl.Id, ClientId = sl.ClientId, TopicName = sl.TopicName, topicType = TopicType.Subscriber, RecQOS = s.QOS, isRetain = s.Msg.Retain, PublishMessage = System.Text.Encoding.UTF8.GetString(s.Msg.Payload), SendRecTime = s.RecTime }))); //{ topicType = TopicType.Subscriber, QosType = Rec.QosType, RecQOS = s.QOS, isRetain = s.Msg.Retain, PublishMessage = System.Text.Encoding.UTF8.GetString(s.Msg.Payload), SendRecTime = s.RecTime }));
                }
            }
            else
            {
                var Send = PublisherSubscriber.PublisherSubscribers.Where(ps => ps.topicType == TopicType.Publisher).ToList();
                var Rec = PublisherSubscriber.PublisherSubscribers.Where(ps => ps.topicType == TopicType.Subscriber).ToList();
                if (Send.Count > 0)
                {
                    Send.ForEach(sl => sl.SentMessages.ForEach(s => lst.Add(new PublisherSubscriber() { Id = sl.Id, ClientId = sl.ClientId, TopicName = sl.TopicName, topicType = TopicType.Publisher, QosType = s.QOS, PublishMessage = s.Msg, SendRecTime = s.PubTime })));
                }
                if (Rec.Count > 0)
                {
                    Rec.ForEach(sl => sl.RecievedMessages.ForEach(s => lst.Add(new PublisherSubscriber() { Id = sl.Id, ClientId = sl.ClientId, TopicName = sl.TopicName, topicType = TopicType.Subscriber, RecQOS = s.QOS, isRetain = s.Msg.Retain, PublishMessage = System.Text.Encoding.UTF8.GetString(s.Msg.Payload), SendRecTime = s.RecTime }))); //{ topicType = TopicType.Subscriber, QosType = Rec.QosType, RecQOS = s.QOS, isRetain = s.Msg.Retain, PublishMessage = System.Text.Encoding.UTF8.GetString(s.Msg.Payload), SendRecTime = s.RecTime }));
                }
            }

            lst = lst.OrderBy(c => c.SendRecTime.Date).ThenBy(c => c.SendRecTime.TimeOfDay).ToList();

            return lst;
        }

        private void UnSubscribeTopic(PublisherSubscriber subscriber)
        {
            PublisherSubscriber.PublisherSubscribers.Where(ps => ps.Id == subscriber.Id).ToList().ForEach(ch => { ch.IsSubscribed = false; });
            StateHasChanged();
        }

        private void DeleteSub(PublisherSubscriber subscriber)
        {
            PublisherSubscriber.PublisherSubscribers.Remove(subscriber);
            File.WriteAllText($"{MqttClientModel.MainDirPath}//wwwroot//PubSubclient.json", JsonConvert.SerializeObject(PublisherSubscriber.PublisherSubscribers));
            StateHasChanged();
        }

        private void UpdatePubTopic(string topic)
        {
            pubtopicName = topic;
        }


    }
}
