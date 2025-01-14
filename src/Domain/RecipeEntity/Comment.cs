using Domain.UserEntity;

namespace Domain.RecipeEntity;

public sealed record Comment(UserId AuthorId, RecipeId RecipeId, string Content);