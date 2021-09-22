using System;
using System.Collections.Generic;
using System.Text;

namespace ServerlessKeyRotation.Functions.Configuration
{
    public class RotationKeysConfiguration
    {
        public AuthConfiguration AuthConfiguration { get; set; }

        public ResourceConfiguration ResourceConfiguration { get; set; }

        public string GetAppServiceResourceId()
        {
            return $"/subscriptions/{AuthConfiguration.SubscriptionId}/resourcegroups/{ResourceConfiguration.ResourceGroupName}/providers/Microsoft.Web/sites/{ResourceConfiguration.AppServiceName}";
        }

        public string GetStorageResourceId()
        {
            return $"/subscriptions/{AuthConfiguration.SubscriptionId}/resourcegroups/{ResourceConfiguration.ResourceGroupName}/providers/Microsoft.Web/sites/{ResourceConfiguration.StorageName}";
        }
    }
}
