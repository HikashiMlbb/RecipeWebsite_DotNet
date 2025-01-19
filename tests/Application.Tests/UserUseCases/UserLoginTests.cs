using Application.Users.Services;
using Application.Users.UseCases;
using Application.Users.UseCases.Login;
using Domain.UserEntity;
using Moq;

// ReSharper disable InconsistentNaming

namespace Application.Tests.UserUseCases;

public class UserLoginTests
{
    private readonly Mock<IJwtService> _jwtServiceMock;
    private readonly Mock<IPasswordService> _passwordServiceMock;
    private readonly UserLogin _useCase;
    private readonly Mock<IUserRepository> _userRepositoryMock;

    public UserLoginTests()
    {
        _userRepositoryMock = new Mock<IUserRepository>();
        _passwordServiceMock = new Mock<IPasswordService>();
        _jwtServiceMock = new Mock<IJwtService>();
        _useCase = new UserLogin(_userRepositoryMock.Object, _passwordServiceMock.Object, _jwtServiceMock.Object);
    }

    [Fact]
    public async Task InvalidUsername_ReturnsError()
    {
        // Arrange
        var dto = new UserDto("inv", "password");

        // Act
        var result = await _useCase.LoginAsync(dto);

        // Assert
        Assert.False(result.IsSuccess);
        _userRepositoryMock.Verify(repo => repo.SearchByUsernameAsync(It.IsAny<Username>()),
            Times.Never); // Проверяем, что поиск пользователя не вызывался
    }

    [Fact]
    public async Task UserNotFound_ReturnsError()
    {
        // Arrange
        var dto = new UserDto("validusername", "password");
        _userRepositoryMock.Setup(repo => repo.SearchByUsernameAsync(It.IsAny<Username>())).ReturnsAsync((User)null!);

        // Act
        var result = await _useCase.LoginAsync(dto);

        // Assert
        Assert.False(result.IsSuccess);
        _userRepositoryMock.Verify(repo => repo.SearchByUsernameAsync(It.IsAny<Username>()),
            Times.Once); // Проверяем, что поиск пользователя вызывался один раз
        _passwordServiceMock.Verify(service => service.VerifyAsync(It.IsAny<string>(), It.IsAny<Password>()),
            Times.Never); // Проверяем, что проверка пароля не вызывалась
    }

    [Fact]
    public async Task InvalidPassword_ReturnsError()
    {
        // Arrange
        var validUsername = Username.Create("validuser").Value!;
        var validPassword = new Password("correctpassword");
        var dto = new UserDto(validUsername.Value, "wrongpassword");
        var user = new User(validUsername, validPassword);
        _userRepositoryMock.Setup(repo => repo.SearchByUsernameAsync(It.IsAny<Username>())).ReturnsAsync(user);
        _passwordServiceMock.Setup(service => service.VerifyAsync(It.IsAny<string>(), It.IsAny<Password>()))
            .ReturnsAsync(false);

        // Act
        var result = await _useCase.LoginAsync(dto);

        // Assert
        Assert.False(result.IsSuccess);
        _userRepositoryMock.Verify(repo => repo.SearchByUsernameAsync(It.IsAny<Username>()), Times.Once);
        _passwordServiceMock.Verify(service => service.VerifyAsync(It.IsAny<string>(), It.IsAny<Password>()),
            Times.Once);
        _jwtServiceMock.Verify(service => service.SignTokenAsync(It.IsAny<UserId>()), Times.Never);
    }

    [Fact]
    public async Task Success_ReturnsToken()
    {
        // Arrange
        const string expectedJwtToken = "RandomMockOfJWTToken";
        var validUsername = Username.Create("validuser").Value!;
        var validPassword = new Password("correctpassword");
        var dto = new UserDto(validUsername.Value, "wrongpassword");
        var user = new User(validUsername, validPassword);
        _userRepositoryMock.Setup(repo => repo.SearchByUsernameAsync(It.IsAny<Username>())).ReturnsAsync(user);
        _passwordServiceMock.Setup(service => service.VerifyAsync(It.IsAny<string>(), It.IsAny<Password>()))
            .ReturnsAsync(true);
        _jwtServiceMock.Setup(jwt => jwt.SignTokenAsync(It.IsAny<UserId>())).ReturnsAsync(expectedJwtToken);

        // Act
        var result = await _useCase.LoginAsync(dto);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(expectedJwtToken, result.Value);
        _userRepositoryMock.Verify(repo => repo.SearchByUsernameAsync(It.IsAny<Username>()), Times.Once);
        _passwordServiceMock.Verify(service => service.VerifyAsync(It.IsAny<string>(), It.IsAny<Password>()),
            Times.Once);
        _jwtServiceMock.Verify(service => service.SignTokenAsync(It.IsAny<UserId>()), Times.Once);
    }
}