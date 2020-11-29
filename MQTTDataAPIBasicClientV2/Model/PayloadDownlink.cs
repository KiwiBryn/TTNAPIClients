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
//---------------------------------------------------------------------------------
namespace devMobile.TheThingsNetwork.Models
{
   using System.Collections.Generic;
   using System.Runtime.Serialization;

   using Newtonsoft.Json;
   using Newtonsoft.Json.Converters;

   public enum DownlinkSchedule
   {
      Undefined = 0,
      [EnumMember(Value = "replace")]
      Replace,
      [EnumMember(Value = "first")]
      First,
      [EnumMember(Value = "last")]
      Last
   }

   public class DownlinkPayloadRawV2
   {
      [JsonProperty("port")]
      public int Port { get; set; }
      [JsonProperty("confirmed")]
      public bool IsConfirmed { get; set; }
      [JsonProperty("payload_raw")]
      public string PayloadRaw { get; set; }
      [JsonProperty("schedule")]
      [JsonConverter(typeof(StringEnumConverter))]
      public DownlinkSchedule Schedule { get; set; }
   }

   public class DownlinkPayloadFieldsV2
   {
      [JsonProperty("port")]
      public int Port { get; set; }
      [JsonProperty("confirmed")]
      public bool IsConfirmed { get; set; }
      [JsonProperty("payload_raw")]
      public string PayloadRaw { get; set; }
      [JsonProperty("payload_fields")]
      public Dictionary<string, object> PayloadFields { get; set; }
      [JsonProperty("schedule")]
      [JsonConverter(typeof(StringEnumConverter))]
      public DownlinkSchedule Schedule { get; set; }
   }
}
