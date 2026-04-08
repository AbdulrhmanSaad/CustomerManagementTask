using Asp.Versioning;
using CustomersTask4.Abstraction;
using CustomersTask4.CustomerHandler.Command.CreateCustomer;
using CustomersTask4.CustomerHandler.Command.DeleteCustomerCommand;
using CustomersTask4.CustomerHandler.Command.Migration;
using CustomersTask4.CustomerHandler.Command.UpdateCustomer;
using CustomersTask4.CustomerHandler.Query.GenerateCustomerPDF;
using CustomersTask4.CustomerHandler.Query.GetAllCustomers;
using CustomersTask4.CustomerHandler.Query.GetCustomerAddressesHistory;
using CustomersTask4.CustomerHandler.Query.GetCustomerById;
using CustomersTask4.CustomerHandler.Query.GetCustomerHistory;
using CustomersTask4.DTO;
using CustomersTask4.Services;
using CustomersTask4.Users;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NSwag.Annotations;
using System.Threading.Tasks;
using Wolverine;

namespace CustomersTask4.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    [ApiVersion("1")]
    public class CustomerController(
        IAppMeditor mediator,
        IServiceScopeFactory scopeFactory,
        ILogger<CustomerController> logger,
        ILocalizationService localization
        ) : ControllerBase
    {
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<ActionResult<IEnumerable<CustomerDto>>> GetAll()
        {
            var customers = await mediator.Send<IEnumerable<CustomerDto>>(new GetAllCustomerQuery());
            return Ok(customers);
        }

        [HttpGet("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<ActionResult<CustomerDto>> GetCustomerById(string id)
        {
            var customer = await mediator.Send<CustomerDto>(new GetCustomerByIdQuery(id));
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
            return Ok(localization.Localize("Customer Deleted Successfully"));
        }

        [HttpPost]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult> AddCustomer(CreateCustomerCommand command)
        {
            await mediator.Send(command);
            return Ok(localization.Localize("Customer Added from version 1"));
        }

        [HttpPut("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<ActionResult> UpdateCustomer(UpdateCustomerCommand command, [FromRoute] string id)
        {
            command.Id = id;
            await mediator.Send(command);
            return Ok(localization.Localize("Customer Updated"));
        }

        [HttpGet("history/{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<ActionResult<CustomerHistoryResponse>> GetCustomerHistory(string id)
        {
            var customer = await mediator.Send<CustomerHistoryResponse>(new GetCustomerHistoryQuery(id));
            return Ok(customer);
        }

        [HttpGet("AddressHistory/{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<ActionResult<IEnumerable<AddressDto>>> GetCustomerAddressHistory(string id)
        {
            var customer = await mediator.Send<IEnumerable<AddressDto>>(new GetCustomerAddressesHistoryQuery(id));
            return Ok(customer);
        }

        [AllowAnonymous]
        [HttpPost("CustomerReport")]
        public async Task<ActionResult> GenerateCustomerReportPdf(GenerateCustomerPDFQuery query)
        {
            var pdf =await mediator.Send<byte[]>(query);

            if (pdf == null || pdf.Length == 0)
                return BadRequest(localization.Localize("Failed to generate report"));

            return File(pdf, "application/pdf", "CustomersReport.pdf");
        }


        [HttpPost("migrate")]
        [Authorize(Roles = UserRoles.Admin)]
        [ProducesResponseType(StatusCodes.Status202Accepted)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public ActionResult Migrate(MigrationCommand request)
        {
            {
                _ = Task.Run(async () =>
                {
                    using var scope = scopeFactory.CreateScope();
                    var backgroundMediator = scope.ServiceProvider.GetRequiredService<IAppMeditor>();

                    try
                    {
                        logger.LogInformation("Background migration started");
                        var result = await backgroundMediator.Send<MigrationJobResult>(request);
                        logger.LogInformation(
                            "Background migration complete — Migrated: {Migrated}, Skipped: {Skipped}",
                            result.MigratedCount, result.SkippedCount);
                    }
                    catch (Exception ex)
                    {
                        logger.LogError(ex, "Background migration failed");
                    }
                });

                return Ok(localization.Localize("Migration started in background. Check logs for progress."));
            }
        }
    }
}
