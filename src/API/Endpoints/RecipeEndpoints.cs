using System.Security.Claims;
using API.Poco;
using Application.Recipes;
using Application.Recipes.Comment;
using Application.Recipes.Create;
using Application.Recipes.Rate;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace API.Endpoints;

public static class RecipeEndpoints
{
    public static void MapRecipeEndpoints(this IEndpointRouteBuilder app, string path)
    {
        var route = app.MapGroup(path).WithTags("Recipe Endpoints:").DisableAntiforgery();

        route.MapPost("/", Create);
        route.MapPost("/{recipeId:int}/rate", Rate);
        route.MapPost("/{recipeId:int}/comment", Comment);
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
        var userId = context.User.FindFirst(ClaimTypes.NameIdentifier)!.Value;
        
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

    [Authorize]
    private static async Task<IResult> Rate(
        [FromRoute]int recipeId,
        [FromForm]int stars,
        [FromServices]RecipeRate rateService,
        HttpContext context)
    {
        var userId = context.User.FindFirst(ClaimTypes.NameIdentifier)!.Value;
        var rateDto = new RecipeRateDto(int.Parse(userId), recipeId, stars);
        var result = await rateService.Rate(rateDto);

        if (result.IsSuccess) return Results.Ok((int)result.Value);
        
        if (result.Error == RecipeErrors.RecipeNotFound) return Results.NotFound();
        if (result.Error == RecipeErrors.StarsAreNotDefined) return Results.Problem(statusCode: 400, title: result.Error.Code, detail: result.Error.Description);

        return Results.Forbid();
    }

    [Authorize]
    private static async Task<IResult> Comment(
        [FromRoute]int recipeId,
        [FromForm]string content,
        [FromServices]RecipeComment commentService,
        HttpContext context)
    {
        var userId = context.User.FindFirstValue(ClaimTypes.NameIdentifier)!;

        var commentDto = new RecipeCommentDto(
            UserId: int.Parse(userId),
            RecipeId: recipeId,
            Content: content);
        
        var result = await commentService.Comment(commentDto);

        if (result.IsSuccess) return Results.Created();

        return result.Error == RecipeErrors.RecipeNotFound 
            ? Results.NotFound() 
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