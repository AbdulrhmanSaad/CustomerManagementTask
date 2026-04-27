using CustomersTask4.Domain;
using Microsoft.OData.Edm;
using Microsoft.OData.ModelBuilder;

namespace CustomersTask4.OData.Configration
{
    public static class ODataConfig
    {
        public static IEdmModel GetEdmModel()
        {
            var builder = new ODataConventionModelBuilder();
            builder.EntitySet<Customer>("Customers");
            builder.EntitySet<Address>("Addresses");
            return builder.GetEdmModel();
        }
    }
}
