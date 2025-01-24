using Application.Users.UseCases;
using Domain.UserEntity;

namespace Persistence.Repositories;

public class UserRepository : IUserRepository
{
    public Task<User?> SearchByUsernameAsync(Username usernameResultValue)
    {
        throw new NotImplementedException();
    }

    public Task<UserId> InsertAsync(User newUser)
    {
        throw new NotImplementedException();
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