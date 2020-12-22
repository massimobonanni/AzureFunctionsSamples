using Microsoft.Azure.Functions.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Text;
using AppConfigSyncFunction;
using Microsoft.Extensions.DependencyInjection;
using AppConfigSyncFunction.Interfaces;
using AppConfigSyncFunction.Services;

[assembly: FunctionsStartup(typeof(Startup))]
namespace AppConfigSyncFunction
{
    public class Startup : FunctionsStartup
    {
        public override void Configure(IFunctionsHostBuilder builder)
        {
            builder.Services.AddTransient<IAppConfigurationSyncService, AppConfigurationSyncService>();
            builder.Services.AddTransient<IEventsService, StorageQueueEventsService>();
        }
    }

}
