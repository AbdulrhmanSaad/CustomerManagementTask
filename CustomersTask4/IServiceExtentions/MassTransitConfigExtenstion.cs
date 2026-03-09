using CustomersTask4.Consumers;
using CustomersTask4.Data;
using MassTransit;

namespace CustomersTask4.IServiceExtentions
{
    public static class MassTransitConfigExtenstion
    {
        public static void AddMassTransitConfig(this WebApplicationBuilder builder) {

            var rabbitConfig = builder.Configuration.GetSection(nameof(RabbitMqConfig)).Get<RabbitMqConfig>();
            builder.Services.AddMassTransit(option =>
            {
                option.AddConsumer<CustomerConsumer>();

                option.UsingRabbitMq((context, cfg) =>
                {
                    cfg.Host(rabbitConfig!.Server, "/", s =>
                    {
                        s.Username(rabbitConfig.Username);
                        s.Password(rabbitConfig.Password);
                    });
                    cfg.ConfigureEndpoints(context);
                    cfg.Exclusive = false;
                    cfg.Durable = false;

                });
            });

        }
    }
}
