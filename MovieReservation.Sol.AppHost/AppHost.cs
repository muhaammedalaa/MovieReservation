var builder = DistributedApplication.CreateBuilder(args);

builder.AddProject<Projects.MovieReservation_APi>("moviereservation-api");

builder.Build().Run();
