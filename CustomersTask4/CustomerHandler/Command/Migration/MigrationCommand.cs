
namespace CustomersTask4.CustomerHandler.Command.Migration
{
    public class MigrationCommand(string from,string to)
    {
        public string From { get; set; } = from;
        public string To { get; set; } = to;
    }

    public class MigrationJobResult
    {
        public int MigratedCount { get; init; }
        public int SkippedCount { get; init; }
    }
}