namespace API.Endpoints;

public static class RecipeEndpoints
{
    public static void MapRecipeEndpoints(this IEndpointRouteBuilder app, string path)
    {
        var route = app.MapGroup(path).WithTags("Recipe Endpoints:");
            
        route.MapPost("/", () => "Create Recipe.");
        route.MapPost("/{id:int}/rate", () => "Rate the Recipe by ID.");
        route.MapPost("/{id:int}/comment", () => "Comment the Recipe by ID.");
        route.MapGet("/{id:int}", () => "Search Recipe by ID.");
        route.MapPut("/{id:int}", () => "Change Recipe.");
        route.MapDelete("/{id:int}", () => "Delete Recipe by ID.");
    }
}