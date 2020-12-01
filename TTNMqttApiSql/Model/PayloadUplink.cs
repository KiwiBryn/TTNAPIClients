//---------------------------------------------------------------------------------
// Copyright (c) September 2020, devMobile Software
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
//---------------------------------------------------------------------------------
namespace devMobile.TheThingsNetwork.TTNMqttApiSql.Models
{
   using System;
   using System.Collections.Generic;

   using Newtonsoft.Json;
   using Newtonsoft.Json.Linq;

   // Production version of classes for unpacking HTTP payload https://json2csharp.com/
   public class GatewayV2 // https://github.com/TheThingsNetwork/ttn/blob/36761935d1867ce2cd70a80ceef197a124e2d276/core/types/gateway_metadata.go
   {
      [JsonProperty("gtw_id")]
      public string GatewayId { get; set; }
      [JsonProperty("timestamp")]
      public ulong Timestamp { get; set; }
      [JsonProperty("time")]
      public DateTime ReceivedAtUtc { get; set; }
      [JsonProperty("channel")]
      public int Channel { get; set; }
      [JsonProperty("rssi")]
      public int Rssi { get; set; }
      [JsonProperty("snr")]
      public double Snr { get; set; }
      [JsonProperty("rf_chain")]
      public int RFChain { get; set; }
      [JsonProperty("latitude")]
      public double Latitude { get; set; }
      [JsonProperty("longitude")]
      public double Longitude { get; set; }
      [JsonProperty("altitude")]
      public int Altitude { get; set; }
   }

   public class MetadataV2
   {
      [JsonProperty("airtime")]
      public long AirTime { get; set; }
      [JsonProperty("time")]
      public DateTime ReceivedAtUtc { get; set; }
      [JsonProperty("frequency")]
      public double Frequency { get; set; }
      [JsonProperty("modulation")]
      public string Modulation { get; set; }
      [JsonProperty("data_rate")]
      public string DataRate { get; set; }
      [JsonProperty("bit_rate")]
      public uint? BitRate { get; set; }
      [JsonProperty("coding_rate")]
      public string CodingRate { get; set; }
      [JsonProperty("gateways")]
      public List<GatewayV2> Gateways { get; set; }
      [JsonProperty("latitude")]
      public double? Latitude { get; set; }
      [JsonProperty("longitude")]
      public double? Longitude { get; set; }
      [JsonProperty("altitude")]
      public int? Altitude { get; set; }
      [JsonProperty("accuracy")]
      public int? Accuracy { get; set; }
   }

   public class PayloadUplinkV2
   {
      [JsonProperty("app_id")]
      public string ApplicationId { get; set; }
      [JsonProperty("dev_id")]
      public string DeviceId { get; set; }
      [JsonProperty("hardware_serial")]
      public string DeviceEui { get; set; }
      [JsonProperty("port")]
      public int Port { get; set; }
      [JsonProperty("counter")]
      public int Counter { get; set; }
      [JsonProperty("is_retry")]
      public bool IsRetry { get; set; }
      [JsonProperty("confirmed")]
      public bool IsConfirmed { get; set; }
      [JsonProperty("Payload_raw")]
      public string PayloadRaw { get; set; }
      // finally settled on an JToken
      [JsonProperty("payload_fields")]
      public JToken PayloadFields { get; set; }
      [JsonProperty("metadata")]
      public MetadataV2 Metadata { get; set; }
   }

}
