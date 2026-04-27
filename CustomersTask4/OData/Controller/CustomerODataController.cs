using CustomersTask4.Abstraction;
using CustomersTask4.Data;
using CustomersTask4.Domain;
using CustomersTask4.DTO;
using CustomersTask4.OData.CustomerHandlers.GetAll;
using CustomersTask4.OData.CustomerHandlers.GetById;
using CustomersTask4.OData.CustomerHandlers.GetCustomerAddressesHistoryById;
using CustomersTask4.OData.CustomerHandlers.GetCustomerHistoryById;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OData.Query;

namespace CustomersTask4.OData.Controller
{
    [Route("odata/[controller]")]
    [ApiController]
    public class CustomerODataController(IAppMeditor meditor,ApplicationDbContext db) : ControllerBase
    {
        [HttpGet]
        [EnableQuery]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<IActionResult> Get(CancellationToken cancellationToken)
        {
            var customers = await meditor.Send<IQueryable<CustomerDto>>(
                new GetAllCustomersOdataQuery(),
                cancellationToken);

            return Ok(customers);
        }
        [HttpGet("{id}")]
        [EnableQuery]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<IActionResult> GetById(string id,CancellationToken cancellationToken)
        {
            var customer = await meditor.Send<CustomerDto>(
                new GetCustomerByIdODataQuery(id),
                cancellationToken);

            return Ok(customer);
        }
        [HttpGet("history/{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [EnableQuery]
        public async Task<IActionResult> GetCustomerHistory(string id)
        {
            var customer=await meditor.Send<IQueryable<CustomerHistoryResponse>>(new GetCustomerHistoryByIdODataQuery(id));
            return Ok(customer);
        }

        [HttpGet("AddressHistory/{id}")]
        [EnableQuery]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        
        public async Task<IActionResult> GetCustomerAddressHistory(string id)
        {
            var customer = await meditor.Send<IQueryable<AddressDto>>(new GetCustomerAddressesHistoryByIdODataQuery(id));

            return Ok(customer);
        }
    }
}
