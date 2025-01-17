using Domain.UserEntity;

namespace Application.Common.Services;

public interface IJwtService
{
    public Task<string> SignTokenAsync(UserId foundUserId);
}