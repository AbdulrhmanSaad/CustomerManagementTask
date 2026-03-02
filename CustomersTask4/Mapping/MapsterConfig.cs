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
            TypeAdapterConfig<CreateCustomerCommand, Customer>
                .NewConfig()
                .Map(dest => dest.Addresses, src => src.Addresses.Select(a => new AddressDtoEnum
                {
                    AddressType = a.AddressType,
                    AddressName = a.AddressName
                }).ToList())
                .AfterMapping((src, dest) => { dest.CreatedAt = DateTime.UtcNow; });




            TypeAdapterConfig<Customer, CustomerDto>
                .NewConfig()
                .Map(dest => dest.Addresses, src => src.Addresses.Select(a => new Address
                {
                    AddressType = a.AddressType,
                    AddressName = a.AddressName
                }).ToList());


            TypeAdapterConfig<Customer, CustomerHistoryResponse>.NewConfig();

            TypeAdapterConfig<UpdateCustomerCommand, Customer>
                .NewConfig()
                .Map(dest => dest.Addresses, src => src.Addresses.Select(a => new Address
                {
                    AddressType = a.AddressType,
                    AddressName = a.AddressName
                }).ToList())
                .AfterMapping((src, dest) => { dest.ChangedAt = DateTime.UtcNow; });


            TypeAdapterConfig<Customer, Customer>
                .NewConfig()
                .Map(dest => dest.Addresses, src => src.Addresses);

            TypeAdapterConfig<Address, Address>.NewConfig();
        }
    }
}
