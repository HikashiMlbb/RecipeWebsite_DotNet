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
    
    public async Task<Result<string>> Login(UserDto dto)
    {
        var usernameResult = Username.Create(dto.Username);

        if (!usernameResult.IsSuccess)
        {
            return new Error();
        }
        
        var foundUser = await _userRepository.SearchByName(usernameResult.Value!);

        if (foundUser is null)
        {
            return new Error();
        }

        var areEqual = await _passwordService.VerifyAsync(new Password(dto.Password), foundUser.Password);

        if (!areEqual)
        {
            return new Error();
        }

        return await _jwtService.SignTokenAsync(foundUser.Id);
    }
    
}