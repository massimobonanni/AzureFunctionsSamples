namespace ServerlessKeyRotation.Functions.Configuration
{
    public class ResourceConfiguration
    {
        public string ConnectionStringName { get; set; }
        public string StorageName { get; set; }
        public string AppServiceName { get; set; }
        public string ResourceGroupName { get; set; }
    }
}