using Domain.UserEntity;

namespace Domain.RecipeEntity;

public record Comment(UserId AuthorId, RecipeId RecipeId, string Content);