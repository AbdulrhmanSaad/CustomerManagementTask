using CustomersTask4.CustomerHandler.Command.CreateCustomer;
using CustomersTask4.CustomerHandler.Command.UpdateCustomer;
using CustomersTask4.Domain;
using CustomersTask4.DTO;
using Mapster;
using System.Diagnostics.CodeAnalysis;
namespace CustomersTask4.Mapping
{

    [ExcludeFromCodeCoverage]
    public static class MapsterConfig
    {
        public static void Register()
        {
            // CreateCustomerCommand -> Customer
            TypeAdapterConfig<CreateCustomerCommand, Customer>
                .NewConfig()
                .Map(dest => dest.Addresses, src => src.Addresses.Select(a => new Address
                {
                    AddressType = a.AddressType,
                    AddressName = a.AddressName
                }).ToList())
                .AfterMapping((src, dest) =>
                {
                    dest.CreatedAt = DateTime.UtcNow;
                });

            // Customer -> CustomerDto
            TypeAdapterConfig<Customer, CustomerDto>
                .NewConfig()
                .Map(dest => dest.Addresses, src => src.Addresses.Select(a => new Address
                {
                    AddressType = a.AddressType,
                    AddressName = a.AddressName
                }).ToList());


            // Customer -> CustomerHistoryResponse
            TypeAdapterConfig<Customer, CustomerHistoryResponse>.NewConfig();

            // UpdateCustomerCommand -> Customer
            TypeAdapterConfig<UpdateCustomerCommand, Customer>
                .NewConfig()
                .AfterMapping((src, dest) =>
                {
                    dest.Addresses = new List<Address>
                    {
                    new Address
                    {
                        AddressType = AddressType.Home,
                        AddressName = src.HomeAddressLocation
                    },
                    new Address
                    {
                        AddressType = AddressType.Work,
                        AddressName = src.WorkAddressLocation
                    }
                    };

                    dest.ChangedAt = DateTime.UtcNow;
                });

            // Customer -> Customer (clone)
            TypeAdapterConfig<Customer, Customer>
                .NewConfig()
                .Map(dest => dest.Addresses, src => src.Addresses);

            // Address -> Address
            TypeAdapterConfig<Address, Address>.NewConfig();
        }
    }
}
