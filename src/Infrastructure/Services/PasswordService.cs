using Application.Users.Services;
using Domain.UserEntity;

namespace Infrastructure.Services;

public class PasswordService : IPasswordService
{
    public async Task<bool> VerifyAsync(string password, Password foundUserPassword)
    {
        return await Task.Run(() => BCrypt.Net.BCrypt.Verify(password, foundUserPassword.PasswordHash));
    }

    public async Task<Password> CreateAsync(string dtoPassword)
    {
        return await Task.Run(() => new Password(BCrypt.Net.BCrypt.HashPassword(dtoPassword)));
    }
}