/*
//---------------------------------------------------------------------------------
// Copyright (c) November 2020, devMobile Software
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
namespace devMobile.TheThingsNetwork.TTNMqttApiSql
{
   using System;
   using System.Collections.Generic;
   using System.Data;
   using System.Data.SqlClient;
   using System.Diagnostics;
   using System.IO;
   using System.Linq;
   using System.Reflection;
   using System.Threading.Tasks;

   using Microsoft.Extensions.Configuration;

   using MQTTnet;
   using MQTTnet.Client;
   using MQTTnet.Client.Disconnecting;
   using MQTTnet.Client.Options;
   using MQTTnet.Client.Receiving;
 
   using Dapper;
   using log4net;
   using log4net.Config;
   using Newtonsoft.Json;
   using Newtonsoft.Json.Linq;

   using devMobile.TheThingsNetwork.TTNMqttApiSql.Models;

   class Program
   {
      private static readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
      private static IMqttClient mqttClient = null;
      private static IMqttClientOptions mqttOptions = null;
      private static IConfiguration configuration = null;
      private static Dictionary<string, string> storedProcedureMappings;

      static async Task Main(string[] args)
      {
         MqttFactory factory = new MqttFactory();
         mqttClient = factory.CreateMqttClient();

         var logRepository = LogManager.GetRepository(Assembly.GetEntryAssembly());
         XmlConfigurator.Configure(logRepository, new FileInfo("log4net.config"));

         try
         {
            configuration = new ConfigurationBuilder()
             .SetBasePath(Directory.GetCurrentDirectory())
             .AddJsonFile("appsettings.json", true, true)
             .Build();

            storedProcedureMappings = new Dictionary<string, string>();
            configuration.GetSection("StoredProcedureMappings").Bind(storedProcedureMappings);

            string mqttServer = configuration.GetSection("MqttServer").Value;
            string mqttPassword = configuration.GetSection("MqttPassword").Value;
            string applicationId = configuration.GetSection("ApplicationId").Value;
            string clientId = configuration.GetSection("clientId").Value;

            log.InfoFormat($"MQTT Server:{mqttServer} ApplicationID:{applicationId}");

            mqttOptions = new MqttClientOptionsBuilder()
               .WithTcpServer(mqttServer)
               .WithCredentials(applicationId, mqttPassword)
               .WithClientId(clientId)
               .WithTls()
               .Build();

            mqttClient.UseDisconnectedHandler(new MqttClientDisconnectedHandlerDelegate(e => MqttClient_Disconnected(e)));
            mqttClient.UseApplicationMessageReceivedHandler(new MqttApplicationMessageReceivedHandlerDelegate(e => MqttClient_ApplicationMessageReceived(e)));

            await mqttClient.ConnectAsync(mqttOptions);
   
            string uplinkTopic = $"{applicationId}/devices/+/up";
            await mqttClient.SubscribeAsync(uplinkTopic, MQTTnet.Protocol.MqttQualityOfServiceLevel.AtLeastOnce);

            Console.WriteLine("Press any key to terminate wait");
            while (!Console.KeyAvailable)
            {
               await Task.Delay(1000);
            }
         }
         catch (Exception ex)
         {
            log.Error( "Apllication startup failed", ex);
         }
      }

      private static void MqttClient_ApplicationMessageReceived(MqttApplicationMessageReceivedEventArgs e)
      {
         PayloadUplinkV2 payload;

         log.InfoFormat($"Receive Start Topic:{e.ApplicationMessage.Topic}");

         string connectionString = configuration.GetSection("TTNDatabase").Value;

         try
         {
            payload = JsonConvert.DeserializeObject<PayloadUplinkV2>(e.ApplicationMessage.ConvertPayloadToString());
         }
         catch (Exception ex)
         {
            log.Error("DeserializeObject failed", ex);
            return;
         }

         try
         {
            if (payload.PayloadFields != null)
            {
               var parameters = new DynamicParameters();

               EnumerateChildren(parameters, payload.PayloadFields);

               log.Debug($"Parameters:{parameters.ParameterNames.Aggregate((i, j) => i + ',' + j)}");

               foreach (string storedProcedure in storedProcedureMappings.Keys)
               {
                  if (Enumerable.SequenceEqual(parameters.ParameterNames, storedProcedureMappings[storedProcedure].Split(',', StringSplitOptions.RemoveEmptyEntries)))
                  {
                     log.Info($"Payload fields processing with:{storedProcedure}");

                     using (SqlConnection db = new SqlConnection(connectionString))
                     {
                        parameters.Add("@ReceivedAtUtc", payload.Metadata.ReceivedAtUtc);
                        parameters.Add("@DeviceID", payload.DeviceId);
                        parameters.Add("@DeviceEui", payload.DeviceEui);
                        parameters.Add("@ApplicationID", payload.ApplicationId);
                        parameters.Add("@IsConfirmed", payload.IsConfirmed);
                        parameters.Add("@IsRetry", payload.IsRetry);
                        parameters.Add("@Port", payload.Port);

                        db.Execute(sql: storedProcedure, param: parameters, commandType: CommandType.StoredProcedure);
                     }
                  }
               }
            }
            else
            {
               foreach (string storedProcedure in storedProcedureMappings.Keys)
               {
                  if (string.Compare(storedProcedureMappings[storedProcedure], "payload_raw", true) == 0)
                  {
                     log.Info($"Payload raw processing with:{storedProcedure}");

                     using (SqlConnection db = new SqlConnection(connectionString))
                     {
                        var parameters = new DynamicParameters();

                        parameters.Add("@ReceivedAtUtc", payload.Metadata.ReceivedAtUtc);
                        parameters.Add("@DeviceID", payload.DeviceId);
                        parameters.Add("@DeviceEui", payload.DeviceEui);
                        parameters.Add("@ApplicationID", payload.ApplicationId);
                        parameters.Add("@IsConfirmed", payload.IsConfirmed);
                        parameters.Add("@IsRetry", payload.IsRetry);
                        parameters.Add("@Port", payload.Port);
                        parameters.Add("@Payload", payload.PayloadRaw);

                        db.Execute(sql: storedProcedure, param: parameters, commandType: CommandType.StoredProcedure);
                     }
                  }
               }
            }
         }
         catch (Exception ex)
         {
            log.Error("Message processing failed", ex);
         }
      }

      private static void EnumerateChildren(DynamicParameters parameters, JToken token, string prefix ="")
      {
         if (token is JProperty)
            if (token.First is JValue)
            {
               JProperty property = (JProperty)token;
               parameters.Add($"@{prefix}{property.Name}", property.Value.ToString());
            }
            else
            {
               JProperty property = (JProperty)token;
               prefix += property.Name;
            }

         foreach (JToken token2 in token.Children())
         {
            EnumerateChildren(parameters,token2, prefix);
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
   }
}
