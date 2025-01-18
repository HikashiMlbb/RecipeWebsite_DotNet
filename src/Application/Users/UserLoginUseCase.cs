using Application.Common.Services;
using Domain.UserEntity;
using SharedKernel;

namespace Application.Users;

public class UserLoginUseCase
{
    private readonly IUserRepository _userRepository;
    private readonly IPasswordService _passwordService;
    private readonly IJwtService _jwtService;
    
    public UserLoginUseCase(IUserRepository userRepository, IPasswordService passwordService, IJwtService jwtService)
    {
        _userRepository = userRepository;
        _passwordService = passwordService;
        _jwtService = jwtService;
    }
    
    public async Task<Result<string>> LoginAsync(UserDto dto)
    {
        var usernameResult = Username.Create(dto.Username);

        if (!usernameResult.IsSuccess)
        {
            return usernameResult.Error!;
        }
        
        var foundUser = await _userRepository.SearchByUsernameAsync(usernameResult.Value!);

        if (foundUser is null)
        {
            return UserErrors.UserNotFound;
        }

        var areEqual = await _passwordService.VerifyAsync(dto.Password, foundUser.Password);

        if (!areEqual)
        {
            return UserErrors.PasswordIsIncorrect;
        }

        return await _jwtService.SignTokenAsync(foundUser.Id);
    }
    
}