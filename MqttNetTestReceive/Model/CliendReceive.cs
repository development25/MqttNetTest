using MQTTnet;
using MQTTnet.Client;
using MQTTnet.Extensions.ManagedClient;
using MQTTnet.Server;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MqttNetTest.Model
{
    public class CliendReceive
    {
        IManagedMqttClient Client { get; set; }
        private string UUID_ { get; set; }
        private string UUID
        {
            get => UUID_;
            set
            {
                if (UUID_ != value)
                {
                    UUID_ = value;
                }
            }
        }


        public CliendReceive()
        {
            UUID = Guid.NewGuid().ToString("N");
            Client = new MqttFactory().CreateManagedMqttClient();
            Callbacks();
        }


        public async Task Start()
        {
            await Stop();

            var options = new ManagedMqttClientOptions
            {
                PendingMessagesOverflowStrategy = MqttPendingMessagesOverflowStrategy.DropOldestQueuedMessage,
                ClientOptions = new MqttClientOptions()
                {
                    ClientId = UUID,
                    ChannelOptions = new MqttClientTcpOptions()
                    {
                        Server = "127.0.0.1",
                        Port = 1883
                    },
                    ProtocolVersion = MQTTnet.Formatter.MqttProtocolVersion.V500
                }
            };
            try
            {
                await Client.StartAsync(options);
            }
            catch (Exception)
            {
                await Stop();

                UnCallbacks();
                Client.Dispose();
            }
        }

        public async Task Stop()
        {
            try
            {
                await UnSubscriptions();
                await Client.StopAsync();
            }
            catch (Exception) { }
        }

        private async void Callbacks()
        {
            Client.ApplicationMessageReceivedAsync += MessageReceived;
            Client.ConnectedAsync += Connected;
            Client.DisconnectedAsync += Disconnected;

            await Start();
        }

        private void UnCallbacks()
        {
            Client.ApplicationMessageReceivedAsync -= MessageReceived;
            Client.ConnectedAsync -= Connected;
            Client.DisconnectedAsync -= Disconnected;
        }

        private async Task Subscriptions()
        {
            await Client.SubscribeAsync("mqttnet/test/send/#", MQTTnet.Protocol.MqttQualityOfServiceLevel.AtMostOnce);
        }

        private async Task UnSubscriptions()
        {
            await Client.UnsubscribeAsync("mqttnet/test/send/#");
        }

        private async Task Connected(MqttClientConnectedEventArgs context)
        {
            await Subscriptions();
        }

        private Task Disconnected(MqttClientDisconnectedEventArgs context)
        {
            return Task.CompletedTask;
        }

        public async Task PublishMessage(MqttApplicationMessage mqttMessage)
        {
            try
            {
                await Client.EnqueueAsync(mqttMessage);
            }
            catch (Exception) { }
        }

        private async Task MessageReceived(MqttApplicationMessageReceivedEventArgs context)
        {
            if (MqttTopicFilterComparer.Compare(context.ApplicationMessage.Topic, "mqttnet/test/send/#") == MqttTopicFilterCompareResult.IsMatch)
            {
                await ParseMessage(context.ApplicationMessage);
            }
        }

        public async Task ParseMessage(MqttApplicationMessage value)
        {
            var topicSplited = value.Topic.Split('/');
            var payloadString = value.ConvertPayloadToString();

            await ReceiveData(topicSplited, payloadString);
        }

        private async Task ReceiveData(string[] topic, string payload)
        {
            MqttApplicationMessage message = new MqttApplicationMessage();
            Random r = new Random();

            message = new MqttApplicationMessage();
            message.Retain = true;
            message.QualityOfServiceLevel = MQTTnet.Protocol.MqttQualityOfServiceLevel.AtMostOnce;
            message.Payload = Encoding.UTF8.GetBytes(r.Next(0, 100) + " + " + payload);
            message.Topic = "mqttnet/test/receive/person/" + topic[4] + "/data";

            await PublishMessage(message);

        }
    }
}
