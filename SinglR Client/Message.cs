using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SinglR_Client
{
    public class AddressMessage
    {
        public string AddressName { get; init; } = default!;
        public string AddressType { get; init; } = default!;
    }
    public interface ICustomerMessage
    {
    }
    public class CustomerCreatedMessage : ICustomerMessage
    {
        public string Name { get; init; } = default!;
        public string Phone { get; init; } = default!;
        public DateTime CreatedAt { get; init; }
        public string CreatedBy { get; init; } = default!;
        public List<AddressMessage> Addresses { get; init; } = [];
    }

    public class CustomerUpdatedMessage : ICustomerMessage
    {
        public string Id { get; init; } = default!;
        public string Name { get; init; } = default!;
        public string Phone { get; init; } = default!;
        public DateTime? ChangedAt { get; init; }
        public string? ChangedBy { get; init; }
        public List<AddressMessage> Addresses { get; init; } = [];
    }

    public class CustomerDeletedMessage : ICustomerMessage
    {
        public string Id { get; init; } = default!;
    }
}
