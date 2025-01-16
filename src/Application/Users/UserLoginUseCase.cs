using Application.Common.Services;
using Application.Users.CommonDto;
using Application.Users.Repositories;
using Domain.UserEntity;
using SharedKernel;

namespace Application.Users.Login;

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

        var areEqual = _passwordService.Verify(new Password(dto.Password), foundUser.Password);

        if (!areEqual)
        {
            return new Error();
        }

        return _jwtService.SignToken(foundUser.Id);
    }
    
}