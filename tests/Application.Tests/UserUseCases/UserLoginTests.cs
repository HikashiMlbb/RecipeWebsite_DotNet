using Application.Common.Services;
using Application.Users;
using Domain.UserEntity;
using Moq;

namespace Application.Tests.UserUseCases;

public class UserLoginTests
{
    private readonly Mock<IUserRepository> _userRepositoryMock;
    private readonly Mock<IPasswordService> _passwordServiceMock;
    private readonly Mock<IJwtService> _jwtServiceMock;
    private readonly UserLoginUseCase _useCase;

    public UserLoginTests()
    {
        _userRepositoryMock = new Mock<IUserRepository>();
        _passwordServiceMock = new Mock<IPasswordService>();
        _jwtServiceMock = new Mock<IJwtService>();
        _useCase = new UserLoginUseCase(_userRepositoryMock.Object, _passwordServiceMock.Object, _jwtServiceMock.Object);
    }
    
    [Fact]
    public async Task InvalidUsername_ReturnsError()
    {
        // Arrange
        var dto = new UserDto("inv", "password");

        // Act
        var result = await _useCase.Login(dto);

        // Assert
        Assert.False(result.IsSuccess);
        _userRepositoryMock.Verify(repo => repo.SearchByName(It.IsAny<Username>()), Times.Never); // Проверяем, что поиск пользователя не вызывался
    }
    
    [Fact]
    public async Task UserNotFound_ReturnsError()
    {
        // Arrange
        var dto = new UserDto("validusername", "password");
        _userRepositoryMock.Setup(repo => repo.SearchByName(It.IsAny<Username>())).ReturnsAsync((User)null);

        // Act
        var result = await _useCase.Login(dto);

        // Assert
        Assert.False(result.IsSuccess);
        _userRepositoryMock.Verify(repo => repo.SearchByName(It.IsAny<Username>()), Times.Once); // Проверяем, что поиск пользователя вызывался один раз
        _passwordServiceMock.Verify(service => service.Verify(It.IsAny<Password>(), It.IsAny<Password>()), Times.Never); // Проверяем, что проверка пароля не вызывалась
    }

    [Fact]
    public async Task InvalidPassword_ReturnsError()
    {
        // Arrange
        var validUsername = Username.Create("validuser").Value!;
        var validPassword = new Password("correctpassword");
        var dto = new UserDto(validUsername.Value, "wrongpassword");
        var user = new User(validUsername, validPassword);
        _userRepositoryMock.Setup(repo => repo.SearchByName(It.IsAny<Username>())).ReturnsAsync(user);
        _passwordServiceMock.Setup(service => service.Verify(It.IsAny<Password>(), It.IsAny<Password>())).Returns(false);

        // Act
        var result = await _useCase.Login(dto);

        // Assert
        Assert.False(result.IsSuccess);
        _userRepositoryMock.Verify(repo => repo.SearchByName(It.IsAny<Username>()), Times.Once);
        _passwordServiceMock.Verify(service => service.Verify(It.IsAny<Password>(), It.IsAny<Password>()), Times.Once);
        _jwtServiceMock.Verify(service => service.SignToken(It.IsAny<UserId>()), Times.Never);
    }
    
    [Fact]
    public async Task Success_ReturnsToken()
    {
        // Arrange
        var validUsername = Username.Create("validuser").Value!;
        var validPassword = new Password("correctpassword");
        var expectedJwtToken = "RandomMockOfJWTToken"; 
        var dto = new UserDto(validUsername.Value, "wrongpassword");
        var user = new User(validUsername, validPassword);
        _userRepositoryMock.Setup(repo => repo.SearchByName(It.IsAny<Username>())).ReturnsAsync(user);
        _passwordServiceMock.Setup(service => service.Verify(It.IsAny<Password>(), It.IsAny<Password>())).Returns(true);
        _jwtServiceMock.Setup(jwt => jwt.SignToken(It.IsAny<UserId>())).Returns(expectedJwtToken);

        // Act
        var result = await _useCase.Login(dto);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(expectedJwtToken, result.Value);
        _userRepositoryMock.Verify(repo => repo.SearchByName(It.IsAny<Username>()), Times.Once);
        _passwordServiceMock.Verify(service => service.Verify(It.IsAny<Password>(), It.IsAny<Password>()), Times.Once);
        _jwtServiceMock.Verify(service => service.SignToken(It.IsAny<UserId>()), Times.Once);
    }
}