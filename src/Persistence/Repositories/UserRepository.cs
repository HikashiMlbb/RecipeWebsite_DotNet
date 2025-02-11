using Application.Users.UseCases;
using Dapper;
using Domain.RecipeEntity;
using Domain.UserEntity;
using Persistence.Repositories.Dto;

namespace Persistence.Repositories;

public class UserRepository(DapperConnectionFactory dbFactory) : IUserRepository
{
    /// <returns>User with ID and Password</returns>
    public async Task<User?> SearchByUsernameAsync(Username usernameResultValue)
    {
        await using var db = dbFactory.Create();
        await db.OpenAsync();

        const string sql = "SELECT \"Id\" AS \"UserId\", \"Password\" FROM \"Users\" WHERE \"Username\" = @Username;";

        var result = await db.QueryAsync<UserDatabaseDto>(sql, new
        {
            Username = usernameResultValue.Value
        });

        if (result.FirstOrDefault() is not { } userDto) return null;

        return new User
        {
            Id = new UserId(userDto.UserId),
            Password = new Password(userDto.Password)
        };
    }

    public async Task<UserId> InsertAsync(User newUser)
    {
        await using var db = dbFactory.Create();
        await db.OpenAsync();

        const string sql = """
                           INSERT INTO "Users" ("Id", "Username", "Password", "Role") 
                           VALUES (DEFAULT, @Username, @Password, @Role) 
                           ON CONFLICT ("Username") DO NOTHING
                           RETURNING "Id";
                           """;
        var result = await db.QueryFirstOrDefaultAsync<int>(sql, new
        {
            Username = newUser.Username.Value,
            Password = newUser.Password.PasswordHash,
            Role = newUser.Role.ToString()
        });

        return new UserId(result);
    }

    /// <returns>Detailed User</returns>
    public async Task<User?> SearchByIdAsync(UserId userId)
    {
        await using var db = dbFactory.Create();
        await db.OpenAsync();

        var dictionary = new Dictionary<int, UserDatabaseDto>();

        const string sql = """
                           SELECT
                                users."Id" AS "UserId",
                                users."Username",
                                users."Password",
                                users."Role",
                                recipes."Id" AS "RecipeId",
                                recipes."Title",
                                recipes."ImageName",
                                recipes."Difficulty",
                                recipes."CookingTime",
                                recipes."Rating",
                                recipes."Votes"
                           FROM "Users" users
                           LEFT OUTER JOIN "Recipes" recipes ON recipes."AuthorId" = users."Id"
                           WHERE users."Id" = @Id
                           ORDER BY 
                               recipes."Votes" DESC,
                               recipes."Rating" DESC;
                           """;

        var result = (await db.QueryAsync<UserDatabaseDto, RecipeDatabaseDto, UserDatabaseDto>(sql,
            (userDto, recipeDto) =>
            {
                if (!dictionary.TryGetValue(userDto.UserId, out var user))
                {
                    dictionary.Add(userDto.UserId, userDto);
                    user = userDto;
                }

                // ReSharper disable once ConditionIsAlwaysTrueOrFalseAccordingToNullableAPIContract
                if (recipeDto is not null) user.Recipes.Add(recipeDto);
                return user;
            }, new
            {
                Id = userId.Value
            },
            splitOn: "RecipeId")).ToList();

        if (result.Count == 0) return null;

        var userDto = result.Distinct().Single();

        var recipes = userDto.Recipes.Count == 0
            ? Array.Empty<Recipe>()
            : userDto.Recipes.Select(x => new Recipe
            {
                Id = new RecipeId(x.RecipeId),
                Title = RecipeTitle.Create(x.Title).Value!,
                ImageName = new RecipeImageName(x.ImageName),
                Difficulty = Enum.Parse<RecipeDifficulty>(x.Difficulty, true),
                CookingTime = x.CookingTime,
                Rate = new Rate(x.Rating, x.Votes)
            }).ToList() as ICollection<Recipe>;

        return new User
        {
            Id = new UserId(userDto.UserId),
            Username = Username.Create(userDto.Username).Value!,
            Password = new Password(userDto.Password),
            Role = Enum.Parse<UserRole>(userDto.Role, true),
            Recipes = recipes
        };
    }

    public async Task UpdatePasswordAsync(UserId userId, Password newHashedPassword)
    {
        await using var db = dbFactory.Create();
        await db.OpenAsync();

        const string sql = "UPDATE \"Users\" SET \"Password\" = @Password WHERE \"Id\" = @Id";
        await db.ExecuteAsync(sql, new
        {
            Password = newHashedPassword.PasswordHash,
            Id = userId.Value
        });
    }
}