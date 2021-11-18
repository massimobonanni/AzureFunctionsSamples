https://docs.microsoft.com/en-us/dotnet/azure/sdk/azure-sdk-for-dotnet

 ## Create identity for the function

 az ad sp create-for-rbac --sdk-auth --name AzureAutomation

 az ad sp create-for-rbac --sdk-auth --name ServerlessKeyRotator --role "Contributor" --scopes "/subscriptions/60504780-44c1-4d01-b84f-baca1d9239ed/resourceGroups/EventAutomationDemo-rg"