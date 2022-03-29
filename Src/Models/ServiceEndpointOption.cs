namespace MicroAutomation.Web.Models
{
    public class ServiceEndpointOption
    {
        public int? Port { get; set; }

        public string Scheme { get; set; }

        public string StoreName { get; set; }

        public string StoreLocation { get; set; }

        public string Subject { get; set; }

        public string FilePath { get; set; }

        public string Password { get; set; }
    }
}