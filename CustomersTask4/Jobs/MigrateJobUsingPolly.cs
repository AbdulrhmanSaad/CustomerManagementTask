using CustomersTask4.Services;
using Polly;
using Polly.Retry;
using Quartz;

namespace CustomersTask4.Jobs
{
    [DisallowConcurrentExecution]
    public class MigrateJobUsingPolly(
        IMigrateDatabases migrationService,
        IConfiguration configuration,
        ILogger<MigrationJob> logger) : IJob
    {
        private const int MaxRetries = 5;

        public async Task Execute(IJobExecutionContext context)
        {
            var provider = configuration["DatabaseProvidor"] ?? "Sql";

            logger.LogInformation(
                "MigrationJob triggered — active provider: {Provider}", provider);


            var retryPipeline = new ResiliencePipelineBuilder()
                .AddRetry(new RetryStrategyOptions
                {
                    MaxRetryAttempts = MaxRetries,
                    BackoffType = DelayBackoffType.Exponential,
                    Delay = TimeSpan.FromSeconds(2),
                    OnRetry = args =>
                    {
                        logger.LogWarning(
                            args.Outcome.Exception,
                            "MigrationJob retry {Attempt}/{MaxRetries} after {Delay}s",
                            args.AttemptNumber + 1,
                            MaxRetries,
                            args.RetryDelay.TotalSeconds);

                        return ValueTask.CompletedTask;
                    }
                })
                .Build();

            try
            {
                await retryPipeline.ExecuteAsync(async ct =>
                {
                    var result = provider == "Mongo"
                        ? await migrationService.MigrateFromSqlToMongo()
                        : await migrationService.MigrateFromMongoToSql();

                    logger.LogInformation(
                        "MigrationJob complete — Migrated: {Migrated}, Skipped: {Skipped}",
                        result.MigratedCount, result.SkippedCount);

                }, context.CancellationToken);
            }
            catch (Exception ex)
            {
                logger.LogError(
                    ex,
                    "MigrationJob failed after {MaxRetries} retries. Will retry on next scheduled run.",
                    MaxRetries);
            }
        }
    }
}
