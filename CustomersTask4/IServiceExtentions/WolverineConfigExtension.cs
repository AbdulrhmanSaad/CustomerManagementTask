using CustomersTask4.Consumers;
using CustomersTask4.Data;
using MassTransit;
using Wolverine;
using Wolverine.RabbitMQ;

namespace CustomersTask4.IServiceExtentions
{


    public static class WolverineConfigExtension
    {
        public static void AddWolverineConfig(this WebApplicationBuilder builder)
        {
            builder.Host.UseWolverine(opts =>
            {
                    opts.UseRabbitMq("amqp://localhost")
                        .AutoProvision()
                        .UseConventionalRouting();
            });
        }
    }
}
