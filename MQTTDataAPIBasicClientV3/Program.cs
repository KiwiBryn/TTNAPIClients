//---------------------------------------------------------------------------------
// Copyright (c) August 2020, devMobile Software
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
//     http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
//
// https://www.thethingsnetwork.org/docs/applications/mqtt/
//
//---------------------------------------------------------------------------------
#if PAYLOAD_RAW && PAYLOAD_FIELDS
#error Only one of PAYLOAD_RAW and PAYLOAD_FIELDS can be defined
#endif
#if !PAYLOAD_RAW && !PAYLOAD_FIELDS
#error At least one of PAYLOAD_RAW and PAYLOAD_FIELDS must be defined
#endif
namespace devMobile.TheThingsNetwork.MQTTDataAPIBasicClientV3
{
   using System;
#if PAYLOAD_FIELDS
   using System.Collections.Generic;
#endif
   using System.Diagnostics;
   using System.Threading;
   using System.Threading.Tasks;

   using MQTTnet;
   using MQTTnet.Client;
   using MQTTnet.Client.Disconnecting;
   using MQTTnet.Client.Options;
   using MQTTnet.Client.Publishing;
   using MQTTnet.Client.Receiving;
   using MQTTnet.Client.Subscribing;

   using Newtonsoft.Json;
   using Newtonsoft.Json.Linq;

   using devMobile.TheThingsNetwork.Models;

   class Program
   {
      private static IMqttClient mqttClient = null;
      private static IMqttClientOptions mqttOptions = null;
      private static string server;
      private static string applicationId;
      private static string password;
      private static string clientId;
      private static string deviceId;

      static async Task Main(string[] args)
      {
         MqttFactory factory = new MqttFactory();
         mqttClient = factory.CreateMqttClient();

         if (args.Length != 5)
         {
            Console.WriteLine("[MQTT Server] [applicationId] [Password] [ClientID] [deviceID]");
            Console.WriteLine("Press <enter> to exit");
            Console.ReadLine();
            return;
         }

         server = args[0];
         applicationId = args[1];
         password = args[2];
         clientId = args[3];
         deviceId = args[4];

         try
         {
            mqttOptions = new MqttClientOptionsBuilder()
               .WithTcpServer(server)
               .WithCredentials(applicationId, password)
               .WithClientId(clientId)
               .WithTls()
               .Build();

            mqttClient.UseDisconnectedHandler(new MqttClientDisconnectedHandlerDelegate(e => MqttClient_Disconnected(e)));
            mqttClient.UseApplicationMessageReceivedHandler(new MqttApplicationMessageReceivedHandlerDelegate(e => MqttClient_ApplicationMessageReceived(e)));
            await mqttClient.ConnectAsync(mqttOptions);

            // V3 uplink topics, all EndDevices in application then just a specific device
            string uplinkTopic = uplinkTopic = $"v3/{applicationId}/devices/+/up";
            //string uplinkTopic = uplinkTopic = $"v3/{applicationId}/devices/{deviceId}/up";

            //downlinkTopic = $"v3/{applicationId}/devices/{deviceId}/down/push";
            //uplinkTopic = $"v3/+";
            //uplinkTopic = $"v3/#";
            //uplinkTopic = $"v3/{applicationId}/+"; //exception
            //uplinkTopic = $"v3/{applicationId}/*";
            //uplinkTopic = $"v3/devices/+";
            //uplinkTopic = $"v3/devices/#";
            //uplinkTopic = $"v3/devices/+/events/+";
            //uplinkTopic = $"v3/{applicationId}/devices/+/events/+";
            //uplinkTopic = $"v3/{applicationId}/devices/{deviceId}/events/update";
            //uplinkTopic = $"v3/{applicationId}/devices/{deviceId}/events/create";
            //uplinkTopic = $"v3/{applicationId}/devices/{deviceId}/events/delete";
            //uplinkTopic = $"v3/{applicationId}/devices/+/events/+";
            //uplinkTopic = $"v3/{applicationId}/devices/+/events/create";
            //uplinkTopic = $"v3/{applicationId}/devices/+/events/update";
            //uplinkTopic = $"v3/{applicationId}/devices/+/events/delete";
            //uplinkTopic = $"v3/{applicationId}/devices/+/events/+";
            //uplinkTopic = $"v3/{applicationId}/devices/{deviceId}/up";

            string downlinkTopic = $"v3/{applicationId}/devices/{deviceId}/down/push";
            string downlinkQueuedTopic = $"v3/{applicationId}/devices/{deviceId}/down/queued";
            string downlinkSentTopic = $"v3/{applicationId}/devices/{deviceId}/down/sent";
            string downlinkAckTopic = $"v3/{applicationId}/devices/{ deviceId}/down/ack";
            string downlinkNakTopic = $"v3/{applicationId}/devices/{ deviceId}/down/nack";
            string downlinkFailedTopic = $"v3/{applicationId}/devices/{deviceId}/down/sent";

            MqttClientSubscribeResult result;

            // Different QoS I have tried
            //result = await mqttClient.SubscribeAsync(uplinkTopic, MQTTnet.Protocol.MqttQualityOfServiceLevel.AtLeastOnce);
            //result = await mqttClient.SubscribeAsync(uplinkTopic, MQTTnet.Protocol.MqttQualityOfServiceLevel.AtMostOnce);
            //result = await mqttClient.SubscribeAsync(uplinkTopic, MQTTnet.Protocol.MqttQualityOfServiceLevel.ExactlyOnce);
            string uplinkTopic1 = $"v3/{applicationId}/devices/device10001/up";
            result = await mqttClient.SubscribeAsync(uplinkTopic1, MQTTnet.Protocol.MqttQualityOfServiceLevel.AtLeastOnce);
            string uplinkTopic2 = $"v3/{applicationId}/devices/device10002/up";
            result = await mqttClient.SubscribeAsync(uplinkTopic2, MQTTnet.Protocol.MqttQualityOfServiceLevel.AtLeastOnce);

            await mqttClient.SubscribeAsync(downlinkQueuedTopic, MQTTnet.Protocol.MqttQualityOfServiceLevel.AtLeastOnce);
            await mqttClient.SubscribeAsync(downlinkSentTopic, MQTTnet.Protocol.MqttQualityOfServiceLevel.AtLeastOnce);
            await mqttClient.SubscribeAsync(downlinkAckTopic, MQTTnet.Protocol.MqttQualityOfServiceLevel.AtLeastOnce);
            await mqttClient.SubscribeAsync(downlinkNakTopic, MQTTnet.Protocol.MqttQualityOfServiceLevel.AtLeastOnce);
            await mqttClient.SubscribeAsync(downlinkFailedTopic, MQTTnet.Protocol.MqttQualityOfServiceLevel.AtLeastOnce);

#if PAYLOAD_RAW
            DownlinkPayloadV3 downlinkPayloadRaw = new DownlinkPayloadV3()
            {
               end_device_ids = new EndDeviceIds()
               {
                    application_ids = new ApplicationIds()
                    {
                        application_id = "application1",
                    },
                   device_id = "device10001",
               },
               
               correlation_ids = new System.Collections.Generic.List<string>(),
               downlink_ack = new DownlinkAck()
               { 
                  confirmed = false,
                  frm_payload = "SGVsbG8=", //Hello in Base64 
                  //frm_payload = "MDA3MzI4NUQwMDY3MDBENw==", //Hello in Base64 
                  f_port =21,
                  correlation_ids = new System.Collections.Generic.List<string>()
                  {
                     Guid.NewGuid().ToString()                      
                  },
                  priority = DownlinkPriority.Normal,
                  f_cnt = 0,
                  session_key_id = ""
               }
            };
            string payload = JsonConvert.SerializeObject(downlinkPayloadRaw);

            payload = @"{""downlinks"": [{""f_port"": 15,""frm_payload"": ""vu8="",""priority"": ""HIGH"",""confirmed"": true,""correlation_ids"": [""1234567890""]}]}";

            //payload = @"{""downlinks"": [{""f_port"": 15,""frm_payload"": ""vu8="",""priority"": ""NORMAL""}]}";
#endif

#if PAYLOAD_FIELDS
            Dictionary<string, object> payloadFields = new Dictionary<string, object>();

            payloadFields.Add("value_0", 0.0);
            payloadFields.Add("value_1", 1.0);
            payloadFields.Add("value_2", 2.0);
            payloadFields.Add("value_3", 3.0);
            payloadFields.Add("value_4", 4.0);
            payloadFields.Add("value_5", 6.0);

            /*
            payloadFields.Add("value_10", -0.0);
            payloadFields.Add("value_11", -1.0);
            payloadFields.Add("value_12", -2.0);
            payloadFields.Add("value_13", -3.0);
            payloadFields.Add("value_14", -4.0);
            payloadFields.Add("value_15", -6.0);
            */

            /* None of these worked
            payloadFields.Add("digital_in_0", 1);
            payloadFields.Add("digital_in_1", 0);
            payloadFields.Add("digital_out_0", 1);
            payloadFields.Add("digital_out_1", 0);
            payloadFields.Add("analog_out_0", 1.23);
            payloadFields.Add("analog_out_1", 3.21);
            payloadFields.Add("relative_humidity_0", 69.5);
            payloadFields.Add("temperature_0", 21.4);

            Dictionary<string, object> payloadGpsFields = new Dictionary<string, object>();
            payloadGpsFields.Add("latitude", -43.4989);
            payloadGpsFields.Add("lonitude", 172.6011);
            payloadGpsFields.Add("altitude", 26.8);
            payloadFields.Add("gps_1", payloadGpsFields);
            */

            DownlinkPayloadFields downlinkPayloadFields = new DownlinkPayloadFields()
            {
               IsConfirmed = false,
               PayloadFields = payloadFields,
               Port = 3,
               Schedule = DownlinkSchedule.Replace,
            };

            string payload = JsonConvert.SerializeObject(downlinkPayloadFields);
#endif

            DateTime LastSentAt = DateTime.UtcNow;
            Console.WriteLine("Press any key to terminate wait");
            while (!Console.KeyAvailable)
            {
               Console.Write(".");

               if ((DateTime.UtcNow - LastSentAt) > new TimeSpan(0, 10, 10))
               {
                  var message = new MqttApplicationMessageBuilder()
                     .WithTopic(downlinkTopic)
                     .WithPayload(payload)
                     .WithAtLeastOnceQoS()
                  .Build();

                  LastSentAt = DateTime.UtcNow;

                  Console.WriteLine();
                  Console.WriteLine($"{DateTime.UtcNow:HH:mm:ss} PublishAsync Start");
                  MqttClientPublishResult publishResult = await mqttClient.PublishAsync(message);
                  Console.WriteLine($" Result:{publishResult.ReasonCode}");
                  Console.WriteLine($"{DateTime.UtcNow:HH:mm:ss} PublishAsync Finish");
               }

               Thread.Sleep(1000);
            }
         }
         catch (Exception ex)
         {
            Console.WriteLine(ex.Message);
         }

         Console.WriteLine("Press <enter> to exit");
         Console.ReadLine();
         return;
      }

      private static void MqttClient_ApplicationMessageReceived(MqttApplicationMessageReceivedEventArgs e)
      {
         if (e.ApplicationMessage.Topic.EndsWith("/up"))
         {
            PayloadUplinkV3 payload = JsonConvert.DeserializeObject<PayloadUplinkV3>(e.ApplicationMessage.ConvertPayloadToString());

            Console.WriteLine();
            Console.WriteLine($"{DateTime.UtcNow:HH:mm:ss} Receive Start");
            Console.WriteLine($" ClientId:{e.ClientId} Topic:{e.ApplicationMessage.Topic}");
            Console.WriteLine($" ApplicationID:{payload.end_device_ids.application_ids.application_id} DeviceID:{payload.end_device_ids.device_id} Port:{payload.uplink_message.f_port} Payload:{payload.uplink_message.frm_payload}");
            if ( payload.uplink_message.decoded_payload != null)
            {
               EnumerateChildren(0, payload.uplink_message.decoded_payload);
            }

            Console.WriteLine($"{DateTime.UtcNow:HH:mm:ss} Receive Finish");
         }
         else
         {
            Console.WriteLine($"{DateTime.UtcNow:HH:mm:ss} ClientId:{e.ClientId} Topic:{e.ApplicationMessage.Topic}");
         }
      }

      private static async void MqttClient_Disconnected(MqttClientDisconnectedEventArgs e)
      {
         Debug.WriteLine($"Disconnected {e.ReasonCode}");
         await Task.Delay(TimeSpan.FromSeconds(5));

         try
         {
            await mqttClient.ConnectAsync(mqttOptions);
         }
         catch (Exception ex)
         {
            Debug.WriteLine("Reconnect failed {0}", ex.Message);
         }
      }

      static void EnumerateChildren(int indent, JToken token)
      {
         string prepend = string.Empty.PadLeft(indent);

         if (token is JProperty)
            if (token.First is JValue)
            {
               JProperty property = (JProperty)token;
               Console.WriteLine($"{prepend} Name:{property.Name} Value:{property.Value}");
            }
            else
            {
               JProperty property = (JProperty)token;
               Console.WriteLine($"{prepend}Name:{property.Name}");
               indent = indent + 3;
            }

         foreach (JToken token2 in token.Children())
         {
            EnumerateChildren(indent, token2);
         }
      }
   }
}
