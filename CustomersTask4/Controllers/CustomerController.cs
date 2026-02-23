using CustomersTask4.CustomerHandler.Command.CreateCustomer;
using CustomersTask4.CustomerHandler.Command.DeleteCustomerCommand;
using CustomersTask4.CustomerHandler.Command.UpdateCustomer;
using CustomersTask4.CustomerHandler.Query.GetCustomerAddressesHistory;
using CustomersTask4.CustomerHandler.Query;
using CustomersTask4.CustomerHandler.Query.GetAllCustomers;
using CustomersTask4.CustomerHandler.Query.GetCustomerById;
using CustomersTask4.CustomerHandler.Query.GetCustomerHistory;
using CustomersTask4.Domain;
using CustomersTask4.DTO;
using CustomersTask4.Users;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace CustomersTask4.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
   // [Authorize]
    public class CustomerController(IMediator mediator) : ControllerBase
    {
        
        [HttpGet]

        public async Task<ActionResult<IEnumerable<CustomerDto>>> GetAll()
        {
           var customers=await mediator.Send(new GetAllCustomerQuery());
           return Ok(customers);
        }
        [HttpGet("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<CustomerDto>> GetCustomerById(int id)
        {
            var customer =await mediator.Send(new GetCustomerByIdQuery(id));
           
            return Ok(customer);
        }
        [HttpDelete("{id}")]
        [Authorize(Roles =UserRoles.Admin)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult> DeleteCustomer(int id)
        {
            await mediator.Send(new DeleteCustomerCommand(id));
            return Ok("Customer Deleted Successfully");
        }


        [HttpPost]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult> AddCustomer(CreateCustomerCommand command)
        {
            await mediator.Send(command);
            return Ok("Customer Added");
        }


        [HttpPut("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult> UpdateCustomer(UpdateCustomerCommand command, [FromRoute]int id)
        {
            command.Id = id;
            await mediator.Send(command);
            return Ok("Customer Updated");
        }
        [HttpGet("history/{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<CustomerHistoryResponse>> GetCustomerHistory(int id)
        {
            var customer = await mediator.Send(new GetCustomerHistoryQuery(id));
          
            return Ok(customer);
        }


        [HttpGet("AddressHistory/{id}")]
        [AllowAnonymous]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<CustomerHistoryResponse>> GetCustomerAddressHistory(int id)
        {
            var customer = await mediator.Send(new GetCustomerAddressesHistoryQuery(id));

            return Ok(customer);
        }

    }
}
