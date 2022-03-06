using ServerlessKeyRotation.Functions.Configuration;
using System;
using System.Collections.Generic;
using System.Text;

namespace Microsoft.Extensions.Configuration
{
    public static class ConfigurationExtensions
    {
        public static RotationKeysConfiguration GetRotationKeysConfiguration(this IConfiguration config,string settingName)
        {
            var result = new RotationKeysConfiguration()
            {
                AuthConfiguration = new AuthConfiguration(),
                ResourceConfiguration = new ResourceConfiguration()
            };

            result.RestartWebApp= config.GetValue<bool>($"{settingName}:restartWebApp");

            result.AuthConfiguration.ClientId=config.GetValue<string>($"{settingName}:authConfiguration:clientId");
            result.AuthConfiguration.ClientSecret = config.GetValue<string>($"{settingName}:authConfiguration:clientSecret");
            result.AuthConfiguration.SubscriptionId = config.GetValue<string>($"{settingName}:authConfiguration:subscriptionId");
            result.AuthConfiguration.TenantId = config.GetValue<string>($"{settingName}:authConfiguration:tenantId");

            result.ResourceConfiguration.AppServiceName = config.GetValue<string>($"{settingName}:resourceConfiguration:appServiceName");
            result.ResourceConfiguration.ConnectionStringName = config.GetValue<string>($"{settingName}:resourceConfiguration:connectionStringName");
            result.ResourceConfiguration.ResourceGroupName = config.GetValue<string>($"{settingName}:resourceConfiguration:resourceGroupName");
            result.ResourceConfiguration.StorageName = config.GetValue<string>($"{settingName}:resourceConfiguration:storageName");

            return result;
        }
    }
}
