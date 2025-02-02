using Application.Users.Services;
using Domain.UserEntity;
using SharedKernel;

namespace Application.Users.UseCases.Register;

public class UserRegister
{
    private readonly IJwtService _jwtService;
    private readonly IPasswordService _passwordService;
    private readonly IUserRepository _userRepository;

    public UserRegister(IUserRepository userRepo, IPasswordService passwordService, IJwtService jwtService)
    {
        _userRepository = userRepo;
        _passwordService = passwordService;
        _jwtService = jwtService;
    }

    public async Task<Result<string>> RegisterAsync(UserDto dto)
    {
        var usernameCreateResult = Username.Create(dto.Username);
        if (usernameCreateResult.Value is not { } username) return usernameCreateResult.Error!;

        var foundUser = await _userRepository.SearchByUsernameAsync(username);
        if (foundUser is not null) return UserErrors.UserAlreadyExists;

        if (dto.Password is not { } userPassword) return UserErrors.PasswordIsIncorrect;
        var password = await _passwordService.CreateAsync(userPassword);
        var newUser = new User(username, password);
        var result = await _userRepository.InsertAsync(newUser);

        return await _jwtService.SignTokenAsync(result);
    }
}