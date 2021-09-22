using System;
using System.Threading.Tasks;
using Microsoft.Azure.Management.AppService.Fluent;
using Microsoft.Azure.Management.ResourceManager.Fluent;
using Microsoft.Azure.Management.Storage.Fluent;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.Extensions.Logging;

namespace ServerlessKeyRotation.Functions
{
    public  class KeyRotationFunctions
    {
        

        [FunctionName("StorageKeyRotation")]
        public async Task Run([TimerTrigger("0 */2 * * * *")]TimerInfo myTimer, ILogger log)
        {
            log.LogInformation($"C# Timer trigger function executed at: {DateTime.Now}");

			var credentials = SdkContext.AzureCredentialsFactory
					.FromServicePrincipal(clientId,
						clientSecret,
						tenantId,
						AzureEnvironment.AzureGlobalCloud);

			var azure = Microsoft.Azure.Management.Fluent.Azure
				.Configure()
				.Authenticate(credentials)
				.WithSubscription(subscriptionId);

			IWebApp webApp = await azure.WebApps
				.GetByIdAsync("/subscriptions/60504780-44c1-4d01-b84f-baca1d9239ed/resourcegroups/EventAutomationDemo-rg/providers/Microsoft.Web/sites/FrontEndWebSite");
			IStorageAccount storage = await azure.StorageAccounts.GetByIdAsync("/subscriptions/60504780-44c1-4d01-b84f-baca1d9239ed/resourceGroups/EventAutomationDemo-rg/providers/Microsoft.Storage/storageAccounts/frontendstore");

			var keys = await storage.GetKeysAsync();

			var currentSettings = await webApp.GetAppSettingsAsync();
			if (currentSettings.ContainsKey(connectionStringName))
			{
				log.LogTrace("App Setting found");
				int? currentKeyIndex = null;
				var currentSetting = currentSettings[connectionStringName].Value;

				if (currentSetting == GenerateConnectionStringForStorage(storageName, keys[0].Value))
				{
					currentKeyIndex = 0;
				}
				else if (currentSetting == GenerateConnectionStringForStorage(storageName, keys[1].Value))
				{
					currentKeyIndex = 1;
				}

				log.LogTrace($"Current Key Index {currentKeyIndex}");

				if (currentKeyIndex.HasValue)
				{
					string newKey = null;
					if (currentKeyIndex == 0)
					{
						newKey = keys[1].Value;
					}
					else
					{
						newKey = keys[0].Value;
					}

					var newConnectionString = GenerateConnectionStringForStorage(storageName, newKey);
					log.LogTrace($"New connection String {newConnectionString}");

					var result = await webApp.Update().WithAppSetting(connectionStringName, newConnectionString).ApplyAsync();
					await webApp.RestartAsync();

					log.LogTrace($"Regenerate key 'key{currentKeyIndex + 1}'");
					var a = await storage.RegenerateKeyAsync($"key{currentKeyIndex + 1}");

				}
			}
		}

		private string GenerateConnectionStringForStorage(string storageName, string storageKey)
		{
			return $"DefaultEndpointsProtocol=https;AccountName={storageName};AccountKey={storageKey};EndpointSuffix=core.windows.net";
		}
	}
}
