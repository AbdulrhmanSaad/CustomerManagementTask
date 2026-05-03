using CustomersTask4.Abstraction;
using CustomersTask4.CQRS.CustomerHandler.Command.CreateCustomer;
using CustomersTask4.CQRS.CustomerHandler.Command.DeleteCustomer;
using CustomersTask4.CQRS.CustomerHandler.Command.Migration;
using CustomersTask4.CQRS.CustomerHandler.Command.UpdateCustomer;
using CustomersTask4.CQRS.CustomerHandler.Query.GenerateCustomerPDF;
using CustomersTask4.CQRS.CustomerHandler.Query.GetAllCustomers;
using CustomersTask4.CQRS.CustomerHandler.Query.GetCustomerAddressesHistory;
using CustomersTask4.CQRS.CustomerHandler.Query.GetCustomerById;
using CustomersTask4.CQRS.CustomerHandler.Query.GetCustomerHistory;
using CustomersTask4.DTO;
using CustomersTask4.Users;
using Shared.Services;
namespace CustomersTask4.MinimalApi
{
    public static class CustomerManagmentEndPoints
    {
        public static void MapCustomerEndpoints(this IEndpointRouteBuilder app)
        {
            var group=app.MapGroup("customer/")
                .RequireRateLimiting("fixed")
                .RequireAuthorization();

            group.MapGet("", GetAll);

            group.MapGet("{id}", GetCustomerById);

            group.MapDelete("{id}", DeleteCustomer)
               .RequireAuthorization(policy => policy.RequireRole(((int)UserRoles.Admin).ToString()))
               .Produces(StatusCodes.Status200OK)
               .Produces(StatusCodes.Status404NotFound)
               .Produces(StatusCodes.Status403Forbidden);

            group.MapPost("", AddCustomer)
               .Produces(StatusCodes.Status200OK)
               .Produces(StatusCodes.Status403Forbidden)
               .Produces(StatusCodes.Status404NotFound);

            group.MapPut("{id}", UpdateCustomer)
               .Produces(StatusCodes.Status200OK)
               .Produces(StatusCodes.Status400BadRequest)
               .Produces(StatusCodes.Status403Forbidden);

            group.MapGet("/history/{id}", GetCustomerHistory)
               .Produces<CustomerHistoryResponse>(StatusCodes.Status200OK)
               .Produces(StatusCodes.Status404NotFound)
               .Produces(StatusCodes.Status403Forbidden);

            group.MapPost("/report", GenerateCustomerReportPdf)
               .AllowAnonymous();

            group.MapGet("/address-history/{id}", GetCustomerAddressHistory)
               .Produces<IEnumerable<AddressDto>>(StatusCodes.Status200OK)
               .Produces(StatusCodes.Status404NotFound)
               .Produces(StatusCodes.Status403Forbidden);

            group.MapPost("/migrate", Migrate)
               .RequireAuthorization(policy => policy.RequireRole(((int)UserRoles.Admin).ToString()))
               .Produces(StatusCodes.Status202Accepted)
               .Produces(StatusCodes.Status403Forbidden);


        }


        public static async Task<IResult> GetAll(
           IAppMeditor mediator,
           ILocalizationService localization)
        {
            var customers = await mediator.Send<IEnumerable<CustomerDto>>(new GetAllCustomerQuery());
            return TypedResults.Ok(customers);
        }

        public static async Task<IResult> GetCustomerById(
           IAppMeditor mediator,
           ILocalizationService localization,
           string id)
        {
            var customer = await mediator.Send<CustomerDto>(new GetCustomerByIdQuery(id));
            return TypedResults.Ok(customer);
        }


        public static async Task<IResult> DeleteCustomer(string id, IAppMeditor mediator,
           ILocalizationService localization)
        {
            await mediator.Send(new DeleteCustomerCommand(id));
            return TypedResults.Ok(localization.Localize("Customer Deleted Successfully"));
        }

        public static async Task<IResult> AddCustomer(
        IAppMeditor mediator,
        ILocalizationService localization,
        CreateCustomerCommand command)
        {
            await mediator.Send(command);

            return TypedResults.Ok(
                localization.Localize("Customer Added"));
        }

        public static async Task<IResult> UpdateCustomer(
            IAppMeditor mediator,
            ILocalizationService localization,
            string id,
            UpdateCustomerCommand command)
        {
            command.Id = id;

            await mediator.Send(command);

            return TypedResults.Ok(
                localization.Localize("Customer Updated"));
        }

        public static async Task<IResult> GetCustomerHistory(
            IAppMeditor mediator,
            string id)
        {
            var result = await mediator.Send<CustomerHistoryResponse>(
                new GetCustomerHistoryQuery(id));

            return TypedResults.Ok(result);
        }
        public static async Task<IResult> GetCustomerAddressHistory(
                IAppMeditor mediator,
                string id)
        {
            var result = await mediator.Send<IEnumerable<AddressDto>>(
                new GetCustomerAddressesHistoryQuery(id));

            return TypedResults.Ok(result);
        }

        public static async Task<IResult> GenerateCustomerReportPdf(
            IAppMeditor mediator,
            ILocalizationService localization,
            GenerateCustomerPDFQuery query)
        {
            var pdf = await mediator.Send<byte[]>(query);

            if (pdf is null || pdf.Length == 0)
            {
                return TypedResults.BadRequest(
                    localization.Localize("Failed to generate report"));
            }

            return TypedResults.File(
                pdf,
                contentType: "application/pdf",
                fileDownloadName: "CustomersReport.pdf");
        }
        public static IResult Migrate(MigrationCommand request,
            IServiceScopeFactory scopeFactory,
            ILoggerFactory loggerFactory,
            ILocalizationService localization)
        {
            var logger = loggerFactory.CreateLogger("Migration");
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

            return TypedResults.Ok(localization.Localize("Migration started in background. Check logs for progress."));
        }
    }
}
