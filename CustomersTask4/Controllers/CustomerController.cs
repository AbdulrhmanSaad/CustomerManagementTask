using CustomersTask4.CustomerHandler.Command.CreateCustomer;
using CustomersTask4.CustomerHandler.Command.DeleteCustomerCommand;
using CustomersTask4.CustomerHandler.Command.UpdateCustomer;
using CustomersTask4.CustomerHandler.Query;
using CustomersTask4.CustomerHandler.Query.GetCustomerById;
using CustomersTask4.CustomerHandler.Query.GetCustomerHistory;
using CustomersTask4.Domain;
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
  
    public class CustomerController(IMediator mediator) : ControllerBase
    {
        
        [HttpGet]
        [Authorize]
        public async Task<ActionResult<List<Customer>>> GetAll()
        {
           var customers=await mediator.Send(new GetAllCustomerQuery());
           return Ok(customers);
        }
        [HttpGet("{id}")]
        public async Task<ActionResult<Customer>> GetCustomerById(int id)
        {
            var customer =await mediator.Send(new GetCustomerByIdQuery(id));
            //if (customer == null)
            //    return NotFound();
            return Ok(customer);
        }
        [HttpDelete("{id}")]
        [Authorize(Roles =UserRoles.Admin)]
        public async Task<ActionResult> DeleteCustomer(int id)
        {
            await mediator.Send(new DeleteCustomerCommand(id));
            return Ok("Customer Deleted Successfully");
        }


        [HttpPost]
        public async Task<ActionResult> AddCustomer(CreateCustomerCommand command)
        {
            await mediator.Send(command);
            return Ok("Customer Added");
        }


        [HttpPut("{id}")]
        public async Task<ActionResult> UpdateCustomer(UpdateCustomerCommand command, [FromRoute]int id)
        {
            command.Id = id;
            await mediator.Send(command);
            return Ok("Customer Updated");
        }
        [HttpGet("history/{id}")]
        public async Task<ActionResult<CustomerHistory>> GetCustomerHistory(int id)
        {
            var customer = await mediator.Send(new GetCustomerHistoryQuery(id));
            //if (customer == null)
            //    return NotFound();
            return Ok(customer);
        }

    }
}
