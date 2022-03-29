using System.Collections.Generic;

namespace MicroAutomation.Web.Models
{
    public class ServiceOption
    {
        public AuthenticationType Authentication { get; set; }
        public List<ServiceEndpointOption> Endpoints { get; set; }

        public ServiceOption()
        {
            Authentication = AuthenticationType.Default;
            Endpoints = new List<ServiceEndpointOption>()
        {
            new ServiceEndpointOption()
            {
                Scheme = "HTTP",
                Port = 5000
            }
        };
        }
    }
}