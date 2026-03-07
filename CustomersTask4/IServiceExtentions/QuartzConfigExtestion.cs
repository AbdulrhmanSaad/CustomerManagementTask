using CustomersTask4.Jobs;
using Quartz;

namespace CustomersTask4.IServiceExtentions
{
    public static class QuartzConfigExtestion
    {
        public static void AddQuartzConfig(this WebApplicationBuilder builder)
        {
            builder.Services.AddQuartz(q =>
            {
                var jobKey = new JobKey("MigrationJob");

                q.AddJob<MigrationJob>(opts => opts
                    .WithIdentity(jobKey)
                    .StoreDurably()
                    .UsingJobData("RetryCount", 0));

                q.AddTrigger(opts => opts
                    .ForJob(jobKey)
                    .WithIdentity("MigrationTrigger")
                    .WithSimpleSchedule(s => s
                        .WithIntervalInMinutes(10)
                        .RepeatForever())
                    .StartNow());
            });

            builder.Services.AddQuartzHostedService(q =>
            {
                q.WaitForJobsToComplete = true;
            });
        }
    }
}
