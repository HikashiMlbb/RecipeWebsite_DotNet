using Domain.UserEntity;

namespace Application.Common.Services;

public interface IPasswordService
{
    bool Verify(Password password, Password foundUserPassword);
}