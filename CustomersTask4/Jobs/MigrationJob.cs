using CustomersTask4.Services;
using Quartz;

namespace CustomersTask4.Jobs
{
    [DisallowConcurrentExecution]
    public class MigrationJob(
        IMigrateDatabases migrationService,
        IConfiguration configuration,
        ILogger<MigrationJob> logger) : IJob
    {
        private const int MaxRetries = 5;

        public async Task Execute(IJobExecutionContext context)
        {
            var provider = configuration["DatabaseProvidor"] ?? "Sql";

         
            var retryCount = context.MergedJobDataMap.GetIntValue("RetryCount");

            logger.LogInformation(
                "MigrationJob triggered — provider: {Provider}, attempt: {Attempt}",
                provider, retryCount + 1);

            try
            {
                var result = provider == "Mongo"
                    ? await migrationService.MigrateFromSqlToMongo()
                    : await migrationService.MigrateFromMongoToSql();

                logger.LogInformation(
                    "MigrationJob complete — Migrated: {Migrated}, Skipped: {Skipped}",
                    result.MigratedCount, result.SkippedCount);
            }
            catch (Exception ex)
            {
                retryCount++;

                if (retryCount <= MaxRetries)
                {
                    var delaySeconds = (int)Math.Pow(2,retryCount);

                    logger.LogWarning(
                        ex,
                        "MigrationJob failed — attempt {Attempt}/{MaxRetries}. Retrying in {Delay}s...",
                        retryCount, MaxRetries, delaySeconds);

                
                    var retryTrigger = TriggerBuilder.Create()
                        .WithIdentity($"retry-trigger-{retryCount}", "migration-retries")
                        .ForJob(context.JobDetail.Key)
                        .UsingJobData("RetryCount", retryCount)  
                        .StartAt(DateTimeOffset.UtcNow.AddSeconds(delaySeconds))
                        .Build();

                    await context.Scheduler.ScheduleJob(retryTrigger);
                }
                else
                {
                    logger.LogError(
                        ex,
                        "MigrationJob failed after {MaxRetries} retries. Will retry on next scheduled run.",
                        MaxRetries);
                }
            }
        }
    }
}
