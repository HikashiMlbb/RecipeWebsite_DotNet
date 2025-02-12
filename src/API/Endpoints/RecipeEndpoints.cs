using System.Security.Claims;
using API.Poco;
using API.Services;
using Application.Recipes;
using Application.Recipes.Comment;
using Application.Recipes.Create;
using Application.Recipes.Delete;
using Application.Recipes.GetById;
using Application.Recipes.GetByPage;
using Application.Recipes.GetByQuery;
using Application.Recipes.Rate;
using Application.Recipes.Update;
using Application.Users.UseCases.GetById;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
// ReSharper disable RedundantAnonymousTypePropertyName

namespace API.Endpoints;

public static class RecipeEndpoints
{
    public static void MapRecipeEndpoints(this IEndpointRouteBuilder app, string path)
    {
        var route = app.MapGroup(path).WithTags("Recipe Endpoints:").DisableAntiforgery();

        route.MapPost("/", Create);
        route.MapPost("/{recipeId:int}/rate", Rate);
        route.MapPost("/{recipeId:int}/comment", Comment);
        route.MapGet("/{recipeId:int}", SearchById);
        route.MapGet("/page", SearchByPage);
        route.MapGet("/search", SearchByQuery);
        route.MapPut("/{recipeId:int}", Update);
        route.MapDelete("/{recipeId:int}", Delete);
    }

    #region Private Implementation of Endpoints

    [Authorize]
    private static async Task<IResult> Create(
        [FromForm] RecipeCreateEndpointDto dto,
        [FromForm(Name = "image")] IFormFile imageFile,
        [FromServices] RecipeCreate recipeCreate, 
        [FromServices] IHostEnvironment env,
        [FromServices] FileService fileService,
        HttpContext context)
    {
        var userId = context.User.FindFirst(ClaimTypes.NameIdentifier)!.Value;
        
        if (imageFile.Length == 0 || (!imageFile.FileName.EndsWith(".jpg") && !imageFile.FileName.EndsWith(".png"))) 
            return Results.BadRequest("File format is not recognized.");
        
        var imageName = fileService.GenerateName(imageFile.FileName);

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
        
        if (!result.IsSuccess) Results.BadRequest(result.Error);

        _ = fileService.SaveImage(imageFile, imageName, env.ContentRootPath);
        return Results.Ok(result.Value!.Value);
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

    private static async Task<IResult> SearchById(
        [FromRoute]int recipeId, 
        [FromServices]RecipeGetById recipeService,
        [FromServices]UserGetById userService)
    {
        var result = await recipeService.GetRecipeAsync(recipeId);
        if (result is null) return Results.NotFound();

        return Results.Ok(new
        {
            Id = result.Id.Value,
            Author = new
            {
                Id = result.Author.Id.Value,
                Username = result.Author.Username.Value
            },
            Title = result.Title.Value,
            Description = result.Description.Value,
            Instruction = result.Instruction.Value,
            Image = result.ImageName.Value,
            Difficulty = result.Difficulty.ToString(),
            PublishedAt = result.PublishedAt,
            CookingTime = result.CookingTime.ToString(),
            Rating = result.Rate.Value,
            Votes = result.Rate.TotalVotes,
            Ingredients = result.Ingredients.Select(x => new
            {
                Name = x.Name,
                Count = x.Count,
                MeasurementUnit = x.UnitType.ToString()
            }),
            Comments = result.Comments.Select(x => new
            {
                UserId = x.Author.Id.Value,
                Content = x.Content,
                PublishedAt = x.PublishedAt
            })
        });
    }

    private static async Task<IResult> SearchByPage(
        [FromQuery]int page,
        [FromQuery]int pageSize,
        [FromQuery]string sortType,
        [FromServices]RecipeGetByPage recipeService)
    {
        var dto = new RecipeGetByPageDto(page, pageSize, sortType);
        var result = await recipeService.GetRecipesAsync(dto);
        
        return Results.Ok(result.Select(x => new
        {
            Id = x.Id.Value,
            Title = x.Title.Value,
            Image = x.ImageName.Value,
            Difficulty = x.Difficulty.ToString(),
            CookingTime = x.CookingTime,
            Rating = x.Rate.Value,
            Votes = x.Rate.TotalVotes
        }));
    }

    private static async Task<IResult> SearchByQuery(
        [FromQuery]string query,
        [FromServices]RecipeGetByQuery recipeService)
    {
        var result = await recipeService.GetRecipesAsync(query);
        
        return Results.Ok(result.Select(x => new
        {
            Id = x.Id.Value,
            Title = x.Title.Value,
            Image = x.ImageName.Value,
            Difficulty = x.Difficulty.ToString(),
            CookingTime = x.CookingTime,
            Rating = x.Rate.Value,
            Votes = x.Rate.TotalVotes
        }));
    }

    [Authorize]
    private static async Task<IResult> Update(
        [FromRoute]int recipeId,
        [FromForm]RecipeUpdateEndpointDto? dto,
        [FromForm(Name = "image")]IFormFile? image,
        [FromServices]RecipeUpdate recipeService,
        [FromServices]FileService fileService,
        HttpContext context,
        IHostEnvironment env)
    {
        if (dto is null) return Results.Ok();
        
        var userId = context.User.FindFirstValue(ClaimTypes.NameIdentifier)!;

        if (image is not null)
        {
            if (image.Length == 0 || (!image.FileName.EndsWith(".jpg") && !image.FileName.EndsWith(".png"))) 
                return Results.BadRequest("File format is not recognized.");
        }
        
        var imageName = image is null ? null : fileService.GenerateName(image.FileName);
        
        var updateDto = new RecipeUpdateDto(
            RecipeId: recipeId,
            UserId: int.Parse(userId),
            Title: dto.Title,
            Description: dto.Description,
            Instruction: dto.Instruction,
            ImageName: imageName,
            Difficulty: dto.Difficulty,
            CookingTime: dto.CookingTime,
            Ingredients: dto.Ingredients); 
        
        var result = await recipeService.UpdateAsync(updateDto);

        if (result.IsSuccess)
        {
            if (imageName is not null)
            {
                _ = fileService.SaveImage(image!, imageName, env.ContentRootPath);
            }
            return Results.Ok();
        }

        if (result.Error == RecipeErrors.RecipeNotFound) return Results.NotFound();
        if (result.Error == RecipeErrors.UserIsNotAuthor) return Results.Forbid();

        return Results.Problem(statusCode: 400, title: result.Error!.Code, detail: result.Error!.Description);
    }

    [Authorize]
    private static async Task<IResult> Delete(
        [FromRoute]int recipeId,
        [FromServices]RecipeDelete recipeService,
        HttpContext context)
    {
        var userId = context.User.FindFirstValue(ClaimTypes.NameIdentifier)!;
        var result = await recipeService.DeleteAsync(
            recipeId: recipeId, 
            userId: int.Parse(userId));

        if (result.IsSuccess) return Results.NoContent();

        return result.Error == RecipeErrors.RecipeNotFound
            ? Results.NotFound()
            : Results.Forbid();
    }
    #endregion
}