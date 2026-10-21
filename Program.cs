DotNetEnv.Env.Load();

var builder = WebApplication.CreateBuilder(args);

builder.Services.ConfigurationServices(builder.Configuration);



var app = builder.Build();

app.MapApplication();
app.Run();