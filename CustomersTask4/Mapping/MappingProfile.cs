using AutoMapper;
using CustomersTask4.CustomerHandler.Command.CreateCustomer;
using CustomersTask4.CustomerHandler.Command.UpdateCustomer;
using CustomersTask4.Domain;
using CustomersTask4.DTO;
using System.Text.Json;

namespace CustomersTask4.Mapping
{
    public class MappingProfile:Profile
    {
        public MappingProfile()
        {
            CreateMap<CreateCustomerCommand, Customer>()
                .AfterMap((c, d) =>
                {
                    d.Addresses = new List<Address>
                    {
                        new Address
                        {
                            AddressType = c.AddressType,
                            AddressName = c.HomeAddressLocation
                        },
                        new Address
                        {
                            AddressType = c.AddressType2,
                            AddressName = c.WorkAddressLocation
                        }
                    };
                    d.CreatedAt = DateTime.UtcNow;
                });

            CreateMap<Customer, CustomerDto>()
                .AfterMap((c, d) =>
                {
                    var homeAddress = c.Addresses.FirstOrDefault(a => a.AddressType == AddressType.Home);
                    var workAddress = c.Addresses.FirstOrDefault(a => a.AddressType == AddressType.Work);
                    if (homeAddress != null)
                    {
                        d.AddressType = homeAddress.AddressType.ToString();
                        d.HomeAddressLocation = homeAddress.AddressName;
                    }
                    if (workAddress != null)
                    {
                        d.AddressType2 = workAddress.AddressType.ToString();
                        d.WorkAddressLocation = workAddress.AddressName;
                    }
                });
            CreateMap<UpdateCustomerCommand,Customer>()
                .AfterMap((c, d) =>
                {
                    d.Addresses = new List<Address>
                    {
                        new Address
                        {
                            AddressType = Enum.Parse<AddressType>("0"),
                            AddressName = c.HomeAddressLocation,
                        },
                        new Address
                        {
                            AddressType = Enum.Parse<AddressType>("1"),
                            AddressName = c.WorkAddressLocation,


                        }

                    };
                    d.CreatedAt = DateTime.UtcNow;
                });

            CreateMap<Customer, Customer>()
               .ForMember(dest => dest.Addresses, opt => opt.MapFrom(src => src.Addresses));
            CreateMap<Address, Address>();

            CreateMap<CustomerHistory, CustomerHistoryDto>()
    .ForMember(dest => dest.OldValues,
        opt => opt.MapFrom(src =>
            string.IsNullOrEmpty(src.OldValues)
                ? null
                : JsonSerializer.Deserialize<CustomerSnapshotDto>(src.OldValues, (JsonSerializerOptions)null)
        ))
    .ForMember(dest => dest.NewValues,
        opt => opt.MapFrom(src =>
            string.IsNullOrEmpty(src.NewValues)
                ? null
                : JsonSerializer.Deserialize<CustomerSnapshotDto>(src.NewValues, (JsonSerializerOptions)null)
        ));



        }
    }
}
