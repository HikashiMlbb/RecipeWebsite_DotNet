using Domain.RecipeEntity;

namespace Domain.UserEntity;

public class User
{
    public UserId Id { get; set; } = null!;
    public Username Username { get; set; }
    public Password Password { get; set; }
    public UserRole Role { get; set; }

    public ICollection<Recipe> Recipes = [];

    public User(
        Username username,
        Password password,
        UserRole role = UserRole.Classic,
        ICollection<Recipe>? recipes = null)
    {
        Username = username;
        Password = password;
        Role = role;
    }
}