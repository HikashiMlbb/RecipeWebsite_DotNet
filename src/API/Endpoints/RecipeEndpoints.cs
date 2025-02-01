using System.Security.Claims;
using Application.Recipes.Create;
using Application.Users.UseCases;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace API.Endpoints;

public static class RecipeEndpoints
{
    public static void MapRecipeEndpoints(this IEndpointRouteBuilder app, string path)
    {
        var route = app.MapGroup(path).WithTags("Recipe Endpoints:");

        route.MapPost("/", Create);
        route.MapPost("/{id:int}/rate", () => "Rate the Recipe by ID.");
        route.MapPost("/{id:int}/comment", () => "Comment the Recipe by ID.");
        route.MapGet("/{id:int}", () => "Search Recipe by ID.");
        route.MapGet("/page", () => "Get Recipes by Pagination.");
        route.MapGet("/search", () => "Get Recipes by Query.");
        route.MapPut("/{id:int}", () => "Change Recipe.");
        route.MapDelete("/{id:int}", () => "Delete Recipe by ID.");
    }

    #region Private Implementation of Endpoints

    [Authorize]
    private static async Task<IResult> Create(
        [FromBody]RecipeCreateDto dto, 
        [FromServices]RecipeCreate recipeCreate, 
        HttpContext context)
    {
        var userId = context.User.FindFirst(ClaimTypes.NameIdentifier)!.Value;
        var recipeCreateDto = dto with { AuthorId = int.Parse(userId) };
        var result = await recipeCreate.CreateAsync(recipeCreateDto);
        
        return result.IsSuccess 
            ? Results.Ok(result.Value!.Value) 
            : Results.BadRequest(result.Error);
    }

    #endregion
}