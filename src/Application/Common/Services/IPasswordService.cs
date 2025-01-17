using Domain.UserEntity;

namespace Application.Common.Services;

public interface IPasswordService
{
    public Task<bool> VerifyAsync(Password password, Password foundUserPassword);
    public Task<Password> CreateAsync(string dtoPassword);
}