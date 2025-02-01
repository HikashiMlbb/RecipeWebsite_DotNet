using Application.Users.UseCases;
using Application.Users.UseCases.Login;
using Microsoft.AspNetCore.Mvc;

namespace API.Endpoints;

public static class UserEndpoints
{
    public static void MapUserEndpoints(this IEndpointRouteBuilder app, string path)
    {
        var route = app.MapGroup(path).WithTags("User Endpoints:");

        route.MapPost("/login", Login);
        route.MapPost("/signup", () => "Sign up.");
        route.MapPut("/", () => "Change username and password.");
        route.MapGet("/{id:int}", () => "Get by ID.");
    }

    private static async Task<IResult> Login([FromBody]UserDto dto, [FromServices]UserLogin userLogin)
    {
        var loginResult = await userLogin.LoginAsync(dto);

        return loginResult.IsSuccess
            ? Results.Ok(loginResult.Value!)
            : Results.Problem(statusCode: 401, title: loginResult.Error!.Code, detail: loginResult.Error.Description);
    }
}