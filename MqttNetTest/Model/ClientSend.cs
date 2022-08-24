using MQTTnet;
using MQTTnet.Client;
using MQTTnet.Extensions.ManagedClient;
using MQTTnet.Internal;
using MQTTnet.Server;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MqttNetTest.Model
{
    public class ClientSend
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

        public ClientSend()
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
                await Client.StopAsync();
            }
            catch (Exception) { }
        }

        private void Callbacks()
        {
            Client.ConnectedAsync += Connected;
            Client.DisconnectedAsync += Disconnected;
        }

        private void UnCallbacks()
        {
            Client.ConnectedAsync -= Connected;
            Client.DisconnectedAsync -= Disconnected;
        }

        private Task Connected(MqttClientConnectedEventArgs context)
        {
            _ = Task.Run(Send);
            return Task.CompletedTask;
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

        private async void Send()
        {
            MqttApplicationMessage message = new MqttApplicationMessage();
            Random r = new Random();

            while (true)
            {
                for(int i = 0; i < 20; i++)
                {
                    message.Retain = true;
                    message.QualityOfServiceLevel = MQTTnet.Protocol.MqttQualityOfServiceLevel.AtMostOnce;
                    message.Payload = Encoding.UTF8.GetBytes(r.Next(0, 100) + "");
                    message.Topic = "mqttnet/test/send/person " + i + "/data";

                    await PublishMessage(message);
                }

                await Task.Delay(1000);
            }
        }
    }
}
