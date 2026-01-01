var builder = DistributedApplication.CreateBuilder(args);
var redis = builder.AddRedis("redis")
    .WithRedisCommander();
builder.AddProject<Projects.MovieReservation_APi>("moviereservation-api")
    .WithReference(redis);

builder.Build().Run();
