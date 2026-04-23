using CustomersTask4.Exceptions;
using CustomersTask4.Services;

namespace CustomersTask4.CQRS.CustomerHandler.Command.Migration
{
    public class MigrationCommandHandler(IMigrateDatabases migrate)
     
    {
        public async Task<MigrationJobResult> Handle(
            MigrationCommand request,
            CancellationToken cancellationToken)
        {
            if (request.From.Equals("Sql",StringComparison.OrdinalIgnoreCase))
                return await migrate.MigrateFromSqlToMongo();

            else if (request.From.Equals("Mongo", StringComparison.OrdinalIgnoreCase))
                return await migrate.MigrateFromMongoToSql();

            else
                throw new NotFoundException("Invalid source database specified. Use 'Sql' or 'Mongo'.");
        }

    }
}