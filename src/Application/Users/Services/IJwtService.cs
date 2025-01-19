using Domain.UserEntity;

namespace Application.Users.Services;

public interface IJwtService
{
    public Task<string> SignTokenAsync(UserId foundUserId);
}