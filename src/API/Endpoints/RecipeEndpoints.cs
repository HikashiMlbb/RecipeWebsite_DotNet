using System.Security.Claims;
using API.Poco;
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

        route.MapPost("/", Create).DisableAntiforgery();
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
        [FromForm] RecipeCreateEndpointDto dto,
        [FromForm(Name = "image")] IFormFile imageFile,
        [FromServices] RecipeCreate recipeCreate, 
        [FromServices] IHostEnvironment env,
        HttpContext context)
    {
        var userId = context.User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "1";
        
        if (imageFile.Length == 0 || (!imageFile.FileName.EndsWith(".jpg") && !imageFile.FileName.EndsWith(".png"))) 
            return Results.BadRequest("File format is not recognized.");
        
        var imageName = await SaveImage(imageFile, env.ContentRootPath);

        var recipeCreateDto = new RecipeCreateDto(
            AuthorId: int.Parse(userId),
            Title: dto.Title,
            Description: dto.Description,
            Instruction: dto.Instruction,
            ImageName: imageName,
            Difficulty: dto.Difficulty,
            CookingTime: dto.CookingTime,
            Ingredients: dto.Ingredients);
        
        var result = await recipeCreate.CreateAsync(recipeCreateDto);
        
         return result.IsSuccess 
             ? Results.Ok(result.Value!.Value) 
             : Results.BadRequest(result.Error);
    }

    #endregion

    #region Private endpoint additional functional

    private static async Task<string> SaveImage(IFormFile file, string root)
    {
        var imageName = Guid.NewGuid().ToString() + '.' + file.FileName.Split('.').Last();
        
        var directory = Path.Combine(root, "static");
        Directory.CreateDirectory(directory);
        await using var stream = File.Create(Path.Combine(directory, imageName));
        await file.CopyToAsync(stream);

        return imageName;
    }

    #endregion
}