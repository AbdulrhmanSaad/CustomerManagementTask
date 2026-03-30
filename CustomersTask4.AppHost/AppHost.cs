var builder = DistributedApplication.CreateBuilder(args);

var sqlDb=builder.AddSqlServer("sql-server")
     .WithEnvironment("ACCEPT_EULA", "Y")
    .WithDataVolume()
    .AddDatabase("CustomersManagmentDb");

var mongo = builder.AddMongoDB("mongo-db")
    .WithDataVolume();

var mongoDb = mongo.AddDatabase("CustomersManagmentsDb");

var rabbitMq = builder.AddRabbitMQ("CustomersManagmentMq")
    .WithManagementPlugin();

builder.AddProject<Projects.CustomersTask4>("customerstask4")
    .WaitFor(sqlDb)
    .WaitFor(mongo)
    .WithReference(sqlDb)
    .WithReference(mongoDb)  
    .WithReference(rabbitMq)
    .WithEnvironment("DatabaseProvidor", "Sql");

builder.Build().Run();
