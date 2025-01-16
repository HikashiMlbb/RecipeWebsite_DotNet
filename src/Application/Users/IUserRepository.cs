using Domain.UserEntity;
using SharedKernel;

namespace Application.Users;

public interface IUserRepository
{
    public Task<User?> SearchByName(Username usernameResultValue);
    public Task<Result<UserId>> Insert(User newUser);
}