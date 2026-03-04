using Mediator;

namespace CustomersTask4.CustomerHandler.Command.MigrateToMongo
{
    public class MigrateToMongoCommand(string from,string to) : IRequest<MigrateToMongoResult>
    {
        public string From { get; set; } = from;
        public string To { get; set; } = to;
    }

    public class MigrateToMongoResult
    {
        public int MigratedCount { get; init; }
        public int SkippedCount { get; init; }
    }
}