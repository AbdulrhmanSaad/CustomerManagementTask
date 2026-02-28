using CustomersTask4.Domain;
using System.Diagnostics.CodeAnalysis;
namespace CustomersTask4.DTO
{

   
    
    public class AddressDto
    {
        public string AddressType { get; set; }
        public string AddressName { get; set; }
    }


 


    public class CustomerHistoryResponse
    {
        public string Name { get; set; }
        public string Phone { get; set; }
        public DateTime CreatedAt { get; set; }
        public string CreatedBy { get; set; }
        public DateTime ChangedAt { get; set; }

        public string ChangedBy { get; set; }
    }

    public class AddressDtoEnum
    {
        public AddressType AddressType { get; set; }
        public string AddressName { get; set; }
    }
}
