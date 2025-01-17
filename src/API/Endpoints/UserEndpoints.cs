namespace API.Endpoints;

public static class UserEndpoints
{
    public static void MapUserEndpoints(this IEndpointRouteBuilder app, string path)
    {
        var route = app.MapGroup(path).WithTags("User Endpoints:");
            
        route.MapPost("/login", () => "Login.");
        route.MapPost("/signup", () => "Sign up.");
        route.MapPut("/", () => "Change username and password.");
        route.MapGet("/{id:int}", () => "Get by ID.");
    }
}