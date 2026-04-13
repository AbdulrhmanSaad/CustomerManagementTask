var builder = DistributedApplication.CreateBuilder(args);

//var sqlDb=builder.AddSqlServer("sql-server")
//     .WithEnvironment("ACCEPT_EULA", "Y")
//    .WithDataVolume()
//    .AddDatabase("CustomersManagmentDb");

//var mongo = builder.AddMongoDB("mongo-db")
//    .WithDataVolume();

//var mongoDb = mongo.AddDatabase("CustomersManagmentsDb");


var sqlDb = builder.AddConnectionString("DefaultConnection");
var mongo = builder.AddConnectionString("mongoDb");


var rabbitMq = builder.AddRabbitMQ("CustomersManagmentMq")
    .WithManagementPlugin();

var redis = builder.AddRedis("redis",port:6379)
    .WithDataVolume("redisData");


builder.AddProject<Projects.CustomersTask4>("customerstask4")
    //.WaitFor(sqlDb)
    //.WaitFor(mongo)
    .WithReference(sqlDb)
    .WithReference(mongo)
    .WithReference(rabbitMq)
    .WithReference(redis)
    .WaitFor(redis)
    .WaitFor(rabbitMq)
    .WithEnvironment("DatabaseProvidor", "Sql");

builder.AddProject<Projects.AuthServer>("authserver");

builder.Build().Run();
