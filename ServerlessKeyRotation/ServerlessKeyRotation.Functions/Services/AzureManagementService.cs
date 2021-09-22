using Microsoft.Azure.Management.AppService.Fluent;
using Microsoft.Azure.Management.ResourceManager.Fluent;
using Microsoft.Azure.Management.Storage.Fluent;
using Microsoft.Extensions.Logging;
using ServerlessKeyRotation.Functions.Configuration;
using ServerlessKeyRotation.Functions.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace ServerlessKeyRotation.Functions.Services
{
    public class AzureManagementService : IManagementService
    {
        private readonly ILogger logger;

        public AzureManagementService(ILoggerFactory loggerFactory)
        {
            if (loggerFactory == null)
                throw new ArgumentNullException(nameof(loggerFactory));

            this.logger = loggerFactory.CreateLogger(nameof(AzureManagementService));
        }

        public async Task<bool> RotateStorageKeyForAppServiceAsync(RotationKeysConfiguration rotationConfig)
        {
            var retval = false;

            var credentials = SdkContext.AzureCredentialsFactory
                    .FromServicePrincipal(rotationConfig.AuthConfiguration.ClientId, rotationConfig.AuthConfiguration.ClientSecret,
                        rotationConfig.AuthConfiguration.TenantId, AzureEnvironment.AzureGlobalCloud);

            var azure = Microsoft.Azure.Management.Fluent.Azure
                .Configure()
                .Authenticate(credentials)
                .WithSubscription(rotationConfig.AuthConfiguration.SubscriptionId);

            IWebApp webApp = await azure.WebApps.GetByIdAsync(rotationConfig.GetAppServiceResourceId());
            IStorageAccount storage = await azure.StorageAccounts.GetByIdAsync(rotationConfig.GetStorageResourceId());

            var keys = await storage.GetKeysAsync();

            var currentSettings = await webApp.GetAppSettingsAsync();
            if (currentSettings.ContainsKey(rotationConfig.ResourceConfiguration.ConnectionStringName))
            {
                logger.LogInformation("App Setting found");
                int? currentKeyIndex = null;
                var currentSetting = currentSettings[rotationConfig.ResourceConfiguration.ConnectionStringName].Value;

                if (currentSetting == GenerateConnectionStringForStorage(rotationConfig.ResourceConfiguration.StorageName, keys[0].Value))
                {
                    currentKeyIndex = 0;
                }
                else if (currentSetting == GenerateConnectionStringForStorage(rotationConfig.ResourceConfiguration.StorageName, keys[1].Value))
                {
                    currentKeyIndex = 1;
                }

                logger.LogInformation($"Current Key Index {currentKeyIndex}");

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

                    var newConnectionString = GenerateConnectionStringForStorage(rotationConfig.ResourceConfiguration.StorageName, newKey);
                    logger.LogInformation($"New connection String {newConnectionString}");

                    logger.LogInformation($"Update settings for appservice {rotationConfig.ResourceConfiguration.AppServiceName}");
                    var result = await webApp.Update().WithAppSetting(rotationConfig.ResourceConfiguration.ConnectionStringName, newConnectionString).ApplyAsync();

                    logger.LogInformation($"Restart appservice { rotationConfig.ResourceConfiguration.AppServiceName}");
                    await webApp.RestartAsync();

                    logger.LogInformation($"Regenerate key 'key{currentKeyIndex + 1}' in storage {rotationConfig.ResourceConfiguration.StorageName}");
                    var a = await storage.RegenerateKeyAsync($"key{currentKeyIndex + 1}");

                    retval = true;
                }
            }
            return retval;
        }

        private string GenerateConnectionStringForStorage(string storageName, string storageKey)
        {
            return $"DefaultEndpointsProtocol=https;AccountName={storageName};AccountKey={storageKey};EndpointSuffix=core.windows.net";
        }
    }
}
