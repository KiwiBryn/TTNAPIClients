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
// SECURITY_ANONYMISE 
//---------------------------------------------------------------------------------
namespace devMobile.TheThingsNetwork.ApplicationAPIBasicClient
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
			Console.WriteLine("TheThingsNetwork.ApplicationAPIBasicClient starting");

			if (args.Length != 3)
			{
				Console.WriteLine("EndDeviceClient <baseURL> <apiKey> <collaborator>");
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

			string collaborator = args[2];
#if !SECURITY_ANONYMISE
			Console.WriteLine($"Collaborator: {collaborator}");
#endif
			Console.WriteLine();

			using (HttpClient httpClient = new HttpClient())
			{
				ApplicationRegistryClient applicationRegistryClient = new ApplicationRegistryClient(baseUrl, httpClient)
				{
					ApiKey = apiKey
				};

				try
				{
				#if FIELDS_MINIMUM
					string[] fieldMaskPathsApplication = { "attributes" }; // think this is the bare minimum required for integration
				#else
					string[] fieldMaskPathsApplication = { "name", "description", "attributes" };
				#endif
					V3Applications applications = await applicationRegistryClient.ListAsync(collaborator, field_mask_paths: fieldMaskPathsApplication);
					if ((applications != null) && (applications.Applications != null)) // If there are no applications returns null rather than empty list
					{
						foreach (V3Application application in applications.Applications)
						{
				#if FIELDS_MINIMUM
							Console.WriteLine($"Application ID:{application.Ids.Application_id}");
				#else
							Console.WriteLine($"Application ID:{application.Ids.Application_id} Name:{application.Name} Description:{application.Description}");
							Console.WriteLine($" CreatedAt: {application.Created_at:dd-MM-yy HH:mm:ss} UpdatedAt: {application.Updated_at:dd-MM-yy HH:mm:ss}");
				#endif
							if (application.Attributes != null)
							{
								Console.WriteLine("  Application attributes");

								foreach (KeyValuePair<string, string> attribute in application.Attributes)
								{
									Console.WriteLine($"    Key: {attribute.Key} Value: {attribute.Value}");
								}
							}
							Console.WriteLine();
						}
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
