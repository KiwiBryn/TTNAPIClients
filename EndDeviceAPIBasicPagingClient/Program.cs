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
// SECURITY_ANONYMISE 
//---------------------------------------------------------------------------------
namespace devMobile.TheThingsNetwork.EndDeviceAPIBasicPagingClient
{
	using System;
	using System.Collections.Generic;
	using System.Net.Http;
	using System.Threading.Tasks;

	using devMobile.TheThingsNetwork.API;

	class Program
	{
		static async Task Main(string[] args)
		{
			Console.WriteLine("TheThingsNetwork.EndDeviceAPIBasicClient starting");

			if (args.Length != 4)
			{
				Console.WriteLine("EndDeviceClient <baseURL> <apiKey> <applicationid> <pagesize>");
				Console.WriteLine("Press <enter> to exit");
				Console.ReadLine();
				return;
			}

			string baseUrl = args[0];
#if !SECURITY_ANONYMISE
			Console.WriteLine($"baseURL: {baseUrl}");
#endif

			string apiKey = args[1];
#if !SECURITY_ANONYMISE
			Console.WriteLine($"apiKey: {apiKey}");
#endif

			string applicationID = args[2];
#if !SECURITY_ANONYMISE
			Console.WriteLine($"applicationID: {applicationID}");
#endif

			long pageSize = long.Parse(args[3]);
			Console.WriteLine($"Page size: {pageSize}");

			Console.WriteLine();

			using (HttpClient httpClient = new HttpClient())
			{
				EndDeviceRegistryClient endDeviceRegistryClient = new EndDeviceRegistryClient(baseUrl, httpClient)
				{
					ApiKey = apiKey
				};

				try
				{
					int page = 1;
#if FIELDS_MINIMUM
					string[] fieldMaskPathsDevice = { "attributes" }; // think this is the bare minimum required for integration
#else
					string[] fieldMaskPathsDevice = { "name", "description", "attributes" };
#endif
					V3EndDevices endDevices = await endDeviceRegistryClient.ListAsync(applicationID, field_mask_paths: fieldMaskPathsDevice, limit: pageSize, page: page);
					while ((endDevices != null) && (endDevices.End_devices != null))
					{
						Console.WriteLine($"Devices:{endDevices.End_devices.Count} Page:{page} Page size:{pageSize}");
						foreach (V3EndDevice endDevice in endDevices.End_devices)
						{
#if FIELDS_MINIMUM
							Console.WriteLine($"EndDevice ID:{endDevice.Ids.Device_id}");
#else
							Console.WriteLine($" Device ID:{endDevice.Ids.Device_id} Name:{endDevice.Name} Description:{endDevice.Description}");
							Console.WriteLine($"  CreatedAt: {endDevice.Created_at:dd-MM-yy HH:mm:ss} UpdatedAt: {endDevice.Updated_at:dd-MM-yy HH:mm:ss}");
#endif
							if (endDevice.Attributes != null)
							{
								Console.WriteLine("  EndDevice attributes");

								foreach (KeyValuePair<string, string> attribute in endDevice.Attributes)
								{
									Console.WriteLine($"    Key: {attribute.Key} Value: {attribute.Value}");
								}
							}
							Console.WriteLine();
						}
						page += 1;
						endDevices = await endDeviceRegistryClient.ListAsync(applicationID, field_mask_paths: fieldMaskPathsDevice, limit: pageSize, page: page);
					}
				}
				catch (Exception ex)
				{
					Console.WriteLine(ex.Message);
				}

				Console.WriteLine("Press <enter> to exit");
				Console.ReadLine();
			}
		}
	}
}
