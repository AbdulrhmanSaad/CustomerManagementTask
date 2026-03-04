using CustomersTask4.Exceptions;
using CustomersTask4.Services;
using Mediator;

namespace CustomersTask4.CustomerHandler.Command.MigrateToMongo
{
    public class MigrateToMongoCommandHandler(IMigrateDatabases migratetoMongo)
        : IRequestHandler<MigrateToMongoCommand, MigrateToMongoResult>
    {
        public async ValueTask<MigrateToMongoResult> Handle(
            MigrateToMongoCommand request,
            CancellationToken cancellationToken)
        {
            if (request.From.Equals("Sql",StringComparison.OrdinalIgnoreCase))
                return await migratetoMongo.MigrateFromSqlToMongo();

            else if (request.From.Equals("Mongo", StringComparison.OrdinalIgnoreCase))
                return await migratetoMongo.MigrateFromMongoToSql();

            else
                throw new NotFoundException("Invalid source database specified. Use 'Sql' or 'Mongo'.");
        }

    }
}