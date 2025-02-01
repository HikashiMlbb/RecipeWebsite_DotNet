using Application.Users.UseCases;
using Application.Users.UseCases.Login;
using Application.Users.UseCases.Register;
using Domain.UserEntity;
using Microsoft.AspNetCore.Mvc;

namespace API.Endpoints;

public static class UserEndpoints
{
    public static void MapUserEndpoints(this IEndpointRouteBuilder app, string path)
    {
        var route = app.MapGroup(path).WithTags("User Endpoints:");

        route.MapPost("/login", Login);
        route.MapPost("/signup", SignUp);
        route.MapPut("/", () => "Change username and password.");
        route.MapGet("/{id:int}", () => "Get by ID.");
    }

    #region Private Implementation of Endpoints

    private static async Task<IResult> Login([FromBody]UserDto dto, [FromServices]UserLogin userLogin)
    {
        var loginResult = await userLogin.LoginAsync(dto);

        return loginResult.IsSuccess
            ? Results.Ok(loginResult.Value!)
            : Results.Problem(statusCode: 401, title: loginResult.Error!.Code, detail: loginResult.Error.Description);
    }

    private static async Task<IResult> SignUp([FromBody]UserDto dto, [FromServices]UserRegister userRegister)
    {
        var signUpResult = await userRegister.RegisterAsync(dto);

        if (signUpResult.IsSuccess) return Results.Ok(signUpResult.Value);
        if (signUpResult.Error == UserErrors.UserAlreadyExists) return Results.Conflict(signUpResult.Error);
        if (signUpResult.Error == UserDomainErrors.UsernameUnallowedSymbols || signUpResult.Error == UserDomainErrors.UsernameLengthOutOfRange) return Results.BadRequest(signUpResult.Error);

        return Results.StatusCode(500);
    }
        
    #endregion
}