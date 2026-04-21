using Indigo_task.Services;
using Microsoft.OpenApi;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddSingleton<ITemperatureService, TemperatureService>();

builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo { Title = "Temperature API", Version = "v1" });

    // Tell Swagger about the API key header
    options.AddSecurityDefinition("ApiKey", new OpenApiSecurityScheme
    {
        Name = "X-Api-Key",
        Type = SecuritySchemeType.ApiKey,
        In = ParameterLocation.Header,
        Description = "Enter your API key in the field below."
    });

    options.OperationFilter<Indigo_task.Infrastructure.ApiKeyOperationFilter>();
});

var app = builder.Build();

// Force the singleton to be created immediately so calculations run on startup
app.Services.GetRequiredService<ITemperatureService>();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

if (!app.Environment.IsDevelopment())
    app.UseHttpsRedirection();

app.UseMiddleware<Indigo_task.Middleware.ApiKeyMiddleware>();
app.MapControllers();

app.Run();
