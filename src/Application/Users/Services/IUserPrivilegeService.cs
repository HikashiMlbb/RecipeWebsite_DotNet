using Domain.UserEntity;

namespace Application.Users.Services;

public interface IUserPrivilegeService
{
    public bool IsAdminUsername(Username username);
}