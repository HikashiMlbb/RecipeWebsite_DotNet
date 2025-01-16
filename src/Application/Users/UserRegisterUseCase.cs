using Application.Common.Services;
using Application.Users.CommonDto;
using Application.Users.Repositories;
using Domain.UserEntity;
using SharedKernel;

namespace Application.Users.Register;

public class UserRegisterUseCase
{
    private readonly IUserRepository _userRepository;
    private readonly IPasswordService _passwordService;
    private readonly IJwtService _jwtService;
    
    public async Task<Result<string>> Register(UserDto dto)
    {
        var usernameCreateResult = Username.Create(dto.Username);

        if (!usernameCreateResult.IsSuccess)
        {
            return new Error();
        }
        
        return "Hello!";
    }
}