using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ServerlessKeyRotation.Functions.Configuration
{
    public class RotationKeysConfiguration
    {
        public bool RestartWebApp { get; set; } = true;

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

        private const string AccountKeyPrefix = "AccountKey=";
        public string ExtractKeyFromConnectionString(string connectionString)
        {
            var parts = connectionString.Split(';');
            var accountKey = parts.FirstOrDefault(p => p.StartsWith(AccountKeyPrefix));
            if (accountKey != null)
                return accountKey.Substring(AccountKeyPrefix.Length);
            else
                return null;
        }
    }
}
