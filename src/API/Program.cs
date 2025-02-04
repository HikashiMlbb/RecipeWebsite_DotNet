using API.Endpoints;
using API.Options;
using Application.Recipes;
using Application.Recipes.Comment;
using Application.Recipes.Create;
using Application.Recipes.GetById;
using Application.Recipes.GetByPage;
using Application.Recipes.Rate;
using Application.Users.Services;
using Application.Users.UseCases;
using Application.Users.UseCases.GetById;
using Application.Users.UseCases.Login;
using Application.Users.UseCases.Register;
using Application.Users.UseCases.Update;
using dotenv.net;
using Infrastructure.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.FileProviders;
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

builder.Services.AddScoped<RecipeCreate>();
builder.Services.AddScoped<RecipeRate>();
builder.Services.AddScoped<RecipeComment>();
builder.Services.AddScoped<RecipeGetById>();
builder.Services.AddScoped<RecipeGetByPage>();

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
builder.Services.AddScoped<IRecipeRepository, RecipeRepository>();

#endregion

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

var staticDirectoryPath = Path.Combine(builder.Environment.ContentRootPath, "static");
Directory.CreateDirectory(staticDirectoryPath);
app.UseStaticFiles(new StaticFileOptions
{
    FileProvider = new PhysicalFileProvider(staticDirectoryPath),
    RequestPath = "/static"
});

app.MapUserEndpoints("/api/users");
app.MapRecipeEndpoints("/api/recipes");

using (var scope = app.Services.CreateScope())
{
    var factory = scope.ServiceProvider.GetRequiredService<DapperConnectionFactory>();
    DapperDatabase.Initialize(factory);
}

app.Run();