using Domain.UserEntity;
using SharedKernel;

namespace Application.Users;

public interface IUserRepository
{
    public Task<User?> SearchByUsernameAsync(Username usernameResultValue);
    public Task<Result<UserId>> InsertAsync(User newUser);
    public Task<User?> SearchByIdAsync(UserId userId);
    public Task UpdatePasswordAsync(Password newHashedPassword);
}