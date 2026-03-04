using CustomersTask4.Abstraction;
using CustomersTask4.CustomerHandler.Command.CreateCustomer;
using CustomersTask4.CustomerHandler.Command.DeleteCustomerCommand;
using CustomersTask4.CustomerHandler.Command.MigrateToMongo;
using CustomersTask4.CustomerHandler.Command.UpdateCustomer;
using CustomersTask4.CustomerHandler.Query.GetCustomerAddressesHistory;
using CustomersTask4.CustomerHandler.Query.GetAllCustomers;
using CustomersTask4.CustomerHandler.Query.GetCustomerById;
using CustomersTask4.CustomerHandler.Query.GetCustomerHistory;
using CustomersTask4.DTO;
using CustomersTask4.Users;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CustomersTask4.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class CustomerController(
        IAppMeditor mediator,
        IServiceScopeFactory scopeFactory,
        ILogger<CustomerController> logger) : ControllerBase
    {
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<ActionResult<IEnumerable<CustomerDto>>> GetAll()
        {
            var customers = await mediator.Send(new GetAllCustomerQuery());
            return Ok(customers);
        }

        [HttpGet("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<ActionResult<CustomerDto>> GetCustomerById(string id)
        {
            var customer = await mediator.Send(new GetCustomerByIdQuery(id));
            return Ok(customer);
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = UserRoles.Admin)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<ActionResult> DeleteCustomer(string id)
        {
            await mediator.Send(new DeleteCustomerCommand(id));
            return Ok("Customer Deleted Successfully");
        }

        [HttpPost]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<ActionResult> AddCustomer(CreateCustomerCommand command)
        {
            await mediator.Send(command);
            return Ok("Customer Added");
        }

        [HttpPut("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<ActionResult> UpdateCustomer(UpdateCustomerCommand command, [FromRoute] string id)
        {
            command.Id = id;
            await mediator.Send(command);
            return Ok("Customer Updated");
        }

        [HttpGet("history/{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<ActionResult<CustomerHistoryResponse>> GetCustomerHistory(string id)
        {
            var customer = await mediator.Send(new GetCustomerHistoryQuery(id));
            return Ok(customer);
        }

        [HttpGet("AddressHistory/{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<ActionResult<CustomerHistoryResponse>> GetCustomerAddressHistory(string id)
        {
            var customer = await mediator.Send(new GetCustomerAddressesHistoryQuery(id));
            return Ok(customer);
        }

        [HttpPost("migrate")]
        //[Authorize(Roles = UserRoles.Admin)]
        [ProducesResponseType(StatusCodes.Status202Accepted)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public ActionResult Migrate(MigrateToMongoCommand request)
        {
            _ = Task.Run(async () =>
            {
                using var scope = scopeFactory.CreateScope();
                var backgroundMediator = scope.ServiceProvider.GetRequiredService<IAppMeditor>();

                try
                {
                    logger.LogInformation("Background migration started");
                    var result = await backgroundMediator.Send(request);
                    logger.LogInformation(
                        "Background migration complete — Migrated: {Migrated}, Skipped: {Skipped}",
                        result.MigratedCount, result.SkippedCount);
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, "Background migration failed");
                }
            });

            return Ok("Migration started in background. Check logs for progress.");
        }
    }
}
