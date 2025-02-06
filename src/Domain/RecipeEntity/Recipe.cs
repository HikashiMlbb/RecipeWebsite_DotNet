using Domain.UserEntity;

namespace Domain.RecipeEntity;

public sealed class Recipe
{
    public Recipe(
        User author,
        RecipeTitle title,
        RecipeDescription description,
        RecipeInstruction instruction,
        RecipeImageName imageName,
        RecipeDifficulty difficulty,
        TimeSpan cookingTime,
        Rate? rate = null,
        ICollection<Ingredient>? ingredients = null,
        ICollection<Comment>? comments = null)
    {
        Author = author;
        Title = title;
        Description = description;
        Instruction = instruction;
        ImageName = imageName;
        Difficulty = difficulty;
        CookingTime = cookingTime;

        Rate = rate ?? Rate.Default;
        Ingredients = ingredients ?? [];
        Comments = comments ?? [];
    }

#pragma warning disable CS8618, CS9264
    internal Recipe()
#pragma warning restore CS8618, CS9264
    {
    }

    public RecipeId Id { get; set; } = null!;
    public User Author { get; set; }
    public RecipeTitle Title { get; set; }
    public RecipeDescription Description { get; set; }
    public RecipeInstruction Instruction { get; set; }
    public RecipeImageName ImageName { get; set; }
    public RecipeDifficulty Difficulty { get; set; }
    public DateTimeOffset PublishedAt { get; set; }
    public TimeSpan CookingTime { get; set; }

    public Rate Rate { get; set; }
    public ICollection<Ingredient> Ingredients { get; set; }
    public ICollection<Comment> Comments { get; set; }
}