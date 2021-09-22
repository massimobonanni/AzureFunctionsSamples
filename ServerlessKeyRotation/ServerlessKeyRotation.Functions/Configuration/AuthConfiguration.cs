namespace ServerlessKeyRotation.Functions.Configuration
{
    public class AuthConfiguration
    {
        public string ClientId { get; set; }
        public string ClientSecret { get; set; }
        public string SubscriptionId { get; set; }
        public string TenantId { get; set; }
    }
}