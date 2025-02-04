using Domain.RecipeEntity;

namespace Domain.UserEntity;

public sealed class User
{
    public ICollection<Recipe> Recipes = [];

#pragma warning disable CS8618, CS9264
    public User()
#pragma warning restore CS8618, CS9264
    {
    }

    public User(
        Username username,
        Password password,
        UserRole role = UserRole.Classic)
    {
        Username = username;
        Password = password;
        Role = role;
    }

    public UserId Id { get; set; } = null!;
    public Username Username { get; set; }
    public Password Password { get; set; }
    public UserRole Role { get; set; }
}