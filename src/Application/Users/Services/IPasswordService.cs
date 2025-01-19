using Domain.UserEntity;

namespace Application.Users.Services;

public interface IPasswordService
{
    public Task<bool> VerifyAsync(string password, Password foundUserPassword);
    public Task<Password> CreateAsync(string dtoPassword);
}