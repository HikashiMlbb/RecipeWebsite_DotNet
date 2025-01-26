using Application.Users.UseCases;
using Dapper;
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

        const string sql = "SELECT Id AS UserId, Password FROM Users WHERE Username = @Username;";

        var result = await db.QueryAsync<UserDatabaseDto>(sql, new
        {
            @Username = usernameResultValue.Value
        });

        if (result.FirstOrDefault() is not { } userDto)
        {
            return null;
        }

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
                           INSERT INTO Users (Id, Username, Password, Role) 
                           VALUES (DEFAULT, @Username, @Password, @Role) 
                           ON CONFLICT (Username) DO NOTHING
                           RETURNING Id;
                           """;
        var result = await db.QueryFirstOrDefaultAsync<int>(sql, new
        {
            @Username = newUser.Username.Value,
            @Password = newUser.Password.PasswordHash,
            @Role = newUser.Role.ToString()
        });

        return new UserId(result);
    }

    public Task<User?> SearchByIdAsync(UserId userId)
    {
        throw new NotImplementedException();
    }

    public Task UpdatePasswordAsync(Password newHashedPassword)
    {
        throw new NotImplementedException();
    }
}