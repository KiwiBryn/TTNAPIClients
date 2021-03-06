﻿//---------------------------------------------------------------------------------
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

   public enum DownlinkPriority
   {
      Undefined = 0,
      [EnumMember(Value = "LOWEST")]
      Lowest,
      [EnumMember(Value = "LOW")]
      Low,
      [EnumMember(Value = "BELOW_NORMAL")]
      BelowNormal,
      [EnumMember(Value = "NORMAL")]
      Normal,
      [EnumMember(Value = "ABOVE_NORMAL")]
      AboveNormal,
      [EnumMember(Value = "HIGH")]
      High,
      [EnumMember(Value = "HIGHEST")]
      Highest,
   }

   public class DownlinkAck
   {
      public string session_key_id { get; set; }
      public int f_port { get; set; }
      public int f_cnt { get; set; }
      public string frm_payload { get; set; }
      public bool confirmed { get; set; }
      [JsonConverter(typeof(StringEnumConverter))]
      public DownlinkPriority priority { get; set; }
      public List<string> correlation_ids { get; set; }
   }

   public class DownlinkPayloadV3
   {
      public EndDeviceIds end_device_ids { get; set; }
      public List<string> correlation_ids { get; set; }
      public DownlinkAck downlink_ack { get; set; }
   }
}


