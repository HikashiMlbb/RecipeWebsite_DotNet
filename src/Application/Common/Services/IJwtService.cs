using Domain.UserEntity;

namespace Application.Common.Services;

public interface IJwtService
{
    string SignToken(UserId foundUserId);
}