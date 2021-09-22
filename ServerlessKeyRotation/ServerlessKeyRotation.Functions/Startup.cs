using Microsoft.Azure.Functions.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;
using ServerlessKeyRotation.Functions.Interfaces;
using ServerlessKeyRotation.Functions.Services;

[assembly: FunctionsStartup(typeof(ServerlessKeyRotation.Functions.Startup))]


namespace ServerlessKeyRotation.Functions
{
    public class Startup : FunctionsStartup
    {
        public override void Configure(IFunctionsHostBuilder builder)
        {
            builder.Services.AddScoped<IManagementService, AzureManagementService>();
        }
    }
}
