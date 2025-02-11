using Domain.UserEntity;

namespace Application.Users.UseCases;

public interface IUserRepository
{
    public Task<User?> SearchByUsernameAsync(Username usernameResultValue);
    public Task<UserId> InsertAsync(User newUser);
    public Task<User?> SearchByIdAsync(UserId userId);
    public Task UpdatePasswordAsync(UserId userId, Password newHashedPassword);
}