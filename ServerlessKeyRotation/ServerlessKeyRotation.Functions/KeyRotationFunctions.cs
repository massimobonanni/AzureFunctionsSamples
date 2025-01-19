using System;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using ServerlessKeyRotation.Functions.Interfaces;

namespace ServerlessKeyRotation.Functions
{
    public class KeyRotationFunctions
    {
        private readonly IConfiguration configuration;
        private readonly IManagementService managementService;

        public KeyRotationFunctions(IConfiguration configuration, IManagementService managementService)
        {
            if (configuration == null)
                throw new ArgumentNullException(nameof(configuration));
            if (managementService == null)
                throw new ArgumentNullException(nameof(managementService));

            this.configuration = configuration;
            this.managementService = managementService;
        }

        [FunctionName("StorageKeyRotation")]
        public async Task Run([TimerTrigger("%rotationTimer%")] TimerInfo myTimer, ILogger log)
        {
            log.LogInformation($"Rotation key function: start at {DateTime.Now}");

            var rotationConfig = this.configuration.GetRotationKeysConfiguration("rotationKeysConfiguration");

            await this.managementService.RotateStorageKeyForAppServiceAsync(rotationConfig);
        }

    }
}
