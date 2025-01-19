using Application.Users.Services;
using Application.Users.UseCases;
using Application.Users.UseCases.Register;
using Domain.UserEntity;
using Moq;

// ReSharper disable InconsistentNaming

namespace Application.Tests.UserUseCases;

public class UserRegisterTests
{
    private readonly Mock<IJwtService> _jwtServiceMock;
    private readonly Mock<IPasswordService> _passwordServiceMock;
    private readonly UserRegister _useCase;
    private readonly Mock<IUserRepository> _userRepositoryMock;

    public UserRegisterTests()
    {
        _userRepositoryMock = new Mock<IUserRepository>();
        _passwordServiceMock = new Mock<IPasswordService>();
        _jwtServiceMock = new Mock<IJwtService>();
        _useCase = new UserRegister(_userRepositoryMock.Object, _passwordServiceMock.Object, _jwtServiceMock.Object);
    }

    [Fact]
    public async Task InvalidUsername_ReturnsError()
    {
        // Arrange
        var dto = new UserDto("somesomesomesomesomeinvalidinvalidinvalidinvaliduseruserusername", "somepassword");

        // Act
        var result = await _useCase.RegisterAsync(dto);

        // Assert
        Assert.False(result.IsSuccess);
        _userRepositoryMock.Verify(repo => repo.SearchByUsernameAsync(It.IsAny<Username>()), Times.Never);
    }

    [Fact]
    public async Task UserAlreadyExists_ReturnsError()
    {
        // Arrange
        var dto = new UserDto("somevalidusername", "somepassword");
        var alreadyExistsUser =
            new User(Username.Create("somevalidusername").Value!, new Password("somevalidpassword"));
        _userRepositoryMock.Setup(repo => repo.SearchByUsernameAsync(It.IsAny<Username>()))
            .ReturnsAsync(alreadyExistsUser);

        // Act
        var result = await _useCase.RegisterAsync(dto);

        // Assert
        Assert.False(result.IsSuccess);
        _userRepositoryMock.Verify(repo => repo.SearchByUsernameAsync(It.IsAny<Username>()), Times.Once);
        _passwordServiceMock.Verify(service => service.VerifyAsync(It.IsAny<string>(), It.IsAny<Password>()),
            Times.Never);
    }

    [Fact]
    public async Task Success_ReturnsToken()
    {
        // Arrange
        const string expectedPassword = "somevalidpassword";
        const string expectedToken = "SomeJWTToken";
        var dto = new UserDto("somevalidusername", expectedPassword);
        _userRepositoryMock.Setup(repo => repo.SearchByUsernameAsync(It.IsAny<Username>())).ReturnsAsync((User)null!);
        _passwordServiceMock.Setup(service => service.CreateAsync(expectedPassword))
            .ReturnsAsync(new Password(expectedPassword));
        _userRepositoryMock.Setup(repo => repo.InsertAsync(It.IsAny<User>())).ReturnsAsync(new UserId(123));
        _jwtServiceMock.Setup(service => service.SignTokenAsync(It.IsAny<UserId>())).ReturnsAsync(expectedToken);

        // Act
        var result = await _useCase.RegisterAsync(dto);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(expectedToken, result.Value!);
        _userRepositoryMock.Verify(repo => repo.SearchByUsernameAsync(It.IsAny<Username>()), Times.Once);
        _passwordServiceMock.Verify(service => service.CreateAsync(It.IsAny<string>()), Times.Once);
        _jwtServiceMock.Verify(service => service.SignTokenAsync(It.IsAny<UserId>()), Times.Once);
    }
}