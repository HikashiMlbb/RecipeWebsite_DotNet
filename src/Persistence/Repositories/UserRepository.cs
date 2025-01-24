using Application.Users.UseCases;
using Dapper;
using Domain.UserEntity;

namespace Persistence.Repositories;

public class UserRepository(DapperConnectionFactory dbFactory) : IUserRepository
{
    public Task<User?> SearchByUsernameAsync(Username usernameResultValue)
    {
        throw new NotImplementedException();
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