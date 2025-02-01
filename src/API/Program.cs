using API.Endpoints;
using API.Options;
using Application.Users.Services;
using Application.Users.UseCases;
using Application.Users.UseCases.GetById;
using Application.Users.UseCases.Login;
using Application.Users.UseCases.Register;
using Application.Users.UseCases.Update;
using dotenv.net;
using Infrastructure.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Persistence;
using Persistence.Repositories;

var builder = WebApplication.CreateBuilder(args);

if (builder.Environment.IsDevelopment())
{
    DotEnv.Load(new DotEnvOptions(envFilePaths: [Path.Combine(Environment.CurrentDirectory, "..", "..", ".env")]));
}

builder.Configuration.AddEnvironmentVariables();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

#region Authentication & Authorization

var jwtSettings = new JwtSettings();
builder.Configuration.GetSection(JwtSettings.Section).Bind(jwtSettings);

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

#endregion

#region Registration of Application Layer

builder.Services.AddScoped<UserLogin>();
builder.Services.AddScoped<UserRegister>();
builder.Services.AddScoped<UserGetById>();
builder.Services.AddScoped<UserUpdate>();

#endregion

#region Registration of Infrastructure Layer

var jwtDescriptorConfig = new JwtDescriptorConfig
{
    Issuer = jwtSettings.Issuer,
    Expires = jwtSettings.Expires,
    Audience = jwtSettings.Audience,
    SigningKey = jwtSettings.Key
};

builder.Services.AddScoped(typeof(JwtDescriptorConfig), _ => jwtDescriptorConfig);
builder.Services.AddScoped<IJwtService, JwtService>();
builder.Services.AddScoped<IPasswordService, PasswordService>();

#endregion

#region Registration of Persistence Layer

var connectionString = builder.Configuration.GetValue<string>("DATABASE_CONNECTION") ?? throw new ApplicationException("Database connection string has not been provided.");

builder.Services.AddScoped(typeof(DapperConnectionFactory), _ => new DapperConnectionFactory(connectionString));
builder.Services.AddScoped<IUserRepository, UserRepository>();

#endregion

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.MapUserEndpoints("/api/users");
app.MapRecipeEndpoints("/api/recipes");

using (var scope = app.Services.CreateScope())
{
    var factory = scope.ServiceProvider.GetRequiredService<DapperConnectionFactory>();
    DapperDatabase.Initialize(factory);
}

app.Run();