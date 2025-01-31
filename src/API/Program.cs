using API.Endpoints;
using API.Options;
using dotenv.net;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;

var builder = WebApplication.CreateBuilder(args);

if (builder.Environment.IsDevelopment())
{
    DotEnv.Load(new DotEnvOptions(envFilePaths: [Path.Combine(Environment.CurrentDirectory, "..", "..", ".env")]));
}

builder.Configuration.AddEnvironmentVariables();

var jwtSettings = new JwtSettings();
builder.Configuration.GetSection(JwtSettings.Section).Bind(jwtSettings);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = jwtSettings.Issuer,
            ValidAudience = jwtSettings.Audience,
            IssuerSigningKey = jwtSettings.GetKey()
        };
    });
builder.Services.AddAuthorization();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.MapUserEndpoints("/api/users");
app.MapRecipeEndpoints("/api/recipes");

app.Run();