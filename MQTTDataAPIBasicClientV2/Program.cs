/*
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
*/
#if PAYLOAD_RAW && PAYLOAD_FIELDS
#error Only one of PAYLOAD_RAW and PAYLOAD_FIELDS can be defined
#endif
#if !PAYLOAD_RAW && !PAYLOAD_FIELDS
#error At least one of PAYLOAD_RAW and PAYLOAD_FIELDS must be defined
#endif
namespace devMobile.TheThingsNetwork.MQTTDataAPIBasicClientV2
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

            // Different topics I have tried
            MqttClientSubscribeResult result;
            string uplinkTopic;
            string downlinkTopic;
            string downlinkAcktopic;
            string downlinkScheduledtopic;
            string downlinkSenttopic;

            //V2 topics
            uplinkTopic = $"{applicationId}/devices/+/up";
            //uplinkTopic = $"{applicationId}/devices/{deviceId}/up";
            downlinkTopic = $"{applicationId}/devices/{deviceId}/down";
            downlinkAcktopic = $"{applicationId}/devices/{deviceId}/events/down/acks";
            downlinkScheduledtopic = $"{applicationId}/devices/{deviceId}/events/down/scheduled";
            downlinkSenttopic = $"{applicationId}/devices/{deviceId}/events/down/sent";

            // Different QoS I have tried
            //result = await mqttClient.SubscribeAsync(uplinkTopic, MQTTnet.Protocol.MqttQualityOfServiceLevel.AtMostOnce);
            result = await mqttClient.SubscribeAsync(uplinkTopic, MQTTnet.Protocol.MqttQualityOfServiceLevel.AtLeastOnce);
            //result = await mqttClient.SubscribeAsync(uplinkTopic, MQTTnet.Protocol.MqttQualityOfServiceLevel.ExactlyOnce);

            result = await mqttClient.SubscribeAsync(downlinkAcktopic, MQTTnet.Protocol.MqttQualityOfServiceLevel.AtLeastOnce);
            result = await mqttClient.SubscribeAsync(downlinkScheduledtopic, MQTTnet.Protocol.MqttQualityOfServiceLevel.AtLeastOnce);
            result = await mqttClient.SubscribeAsync(downlinkSenttopic, MQTTnet.Protocol.MqttQualityOfServiceLevel.AtLeastOnce);

#if PAYLOAD_RAW
            DownlinkPayloadRawV2 downlinkPayloadRaw = new DownlinkPayloadRawV2()
            {
               IsConfirmed = false,
               PayloadRaw = "SGVsbG8=", //Hello in Base64
               //PayloadRaw = "MDA3MzI4NUQwMDY3MDBENw==",
               Port = 2,
               Schedule = DownlinkSchedule.Replace,
            };
            string payload = JsonConvert.SerializeObject(downlinkPayloadRaw);
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

            DownlinkPayloadFieldsV2 downlinkPayloadFields = new DownlinkPayloadFieldsV2()
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

               if ((DateTime.UtcNow - LastSentAt) > new TimeSpan(0, 10, 0))
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
            PayloadUplinkV2 payload = JsonConvert.DeserializeObject<PayloadUplinkV2>(e.ApplicationMessage.ConvertPayloadToString());

            Console.WriteLine();
            Console.WriteLine($"{DateTime.UtcNow:HH:mm:ss} Receive Start");
            Console.WriteLine($" ClientId:{e.ClientId} Topic:{e.ApplicationMessage.Topic} ReceivedAt:{payload.Metadata.ReceivedAtUtc} Payload:{payload.PayloadRaw}");
            if (payload.PayloadFields != null)
            {
               EnumerateChildren(1, payload.PayloadFields);
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

      private static void EnumerateChildren(int indent, JToken token)
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
