using Microsoft.Azure.Management.AppService.Fluent;
using Microsoft.Azure.Management.ResourceManager.Fluent;
using Microsoft.Azure.Management.Storage.Fluent;
using Microsoft.Azure.Management.Storage.Fluent.Models;
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
                var currentKeyValue = rotationConfig.ExtractKeyFromConnectionString(currentSettings[rotationConfig.ResourceConfiguration.ConnectionStringName].Value);
                int currentKeyIndex;
                StorageAccountKey newKey = null;

                if (currentKeyValue == keys[0].Value)
                {
                    currentKeyIndex = 0;
                    newKey = keys[1];
                }
                else
                {
                    currentKeyIndex = 1;
                    newKey = keys[0];
                }

                var newConnectionString = GenerateConnectionStringForStorage(rotationConfig.ResourceConfiguration.StorageName,
                    newKey.Value);

                // Update the App Service with the new connection string
                await webApp
                    .Update()
                    .WithAppSetting(rotationConfig.ResourceConfiguration.ConnectionStringName, newConnectionString)
                    .ApplyAsync();

                // Regenerate the key in the storage
                await storage.RegenerateKeyAsync(keys[currentKeyIndex].KeyName);
                retval = true;

            }
            return retval;
        }

        private string GenerateConnectionStringForStorage(string storageName, string storageKey)
        {
            return $"DefaultEndpointsProtocol=https;AccountName={storageName};AccountKey={storageKey};EndpointSuffix=core.windows.net";
        }
    }
}
