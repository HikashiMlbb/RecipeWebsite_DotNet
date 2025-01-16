using Domain.UserEntity;
using SharedKernel;

namespace Application.Users.Repositories;

public interface IUserRepository
{
    public Task<User?> SearchByName(Username usernameResultValue);
}