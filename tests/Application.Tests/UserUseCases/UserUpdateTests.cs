using Application.Users.Services;
using Application.Users.UseCases;
using Application.Users.UseCases.Update;
using Domain.UserEntity;
using Moq;

// ReSharper disable InconsistentNaming

namespace Application.Tests.UserUseCases;

public class UserUpdateTests
{
    private readonly Mock<IPasswordService> _passwordServiceMock;
    private readonly UserUpdate _useCase;
    private readonly Mock<IUserRepository> _userRepoMock;

    public UserUpdateTests()
    {
        _userRepoMock = new Mock<IUserRepository>();
        _passwordServiceMock = new Mock<IPasswordService>();
        _useCase = new UserUpdate(_userRepoMock.Object, _passwordServiceMock.Object);
    }

    [Fact]
    public async Task UserNotFound_ReturnsError()
    {
        // Arrange
        var dto = new UserUpdateDto(13, "oldPassword", "newPassword");
        _userRepoMock.Setup(x => x.SearchByIdAsync(It.IsAny<UserId>())).ReturnsAsync((User)null!);

        // Act
        var result = await _useCase.UpdateAsync(dto);

        // Assert
        Assert.False(result.IsSuccess);
        _userRepoMock.Verify(x => x.SearchByIdAsync(It.IsAny<UserId>()), Times.Once);
        _passwordServiceMock.Verify(x => x.VerifyAsync(It.IsAny<string>(), It.IsAny<Password>()), Times.Never);
    }

    [Fact]
    public async Task OldPasswordIsIncorrect_ReturnsError()
    {
        // Arrange
        var user = new User(Username.Create("SomeUsername").Value!, new Password("oldPassword"));
        var dto = new UserUpdateDto(13, "oldPassword", "newPassword");
        _userRepoMock.Setup(x => x.SearchByIdAsync(It.IsAny<UserId>())).ReturnsAsync(user);
        _passwordServiceMock.Setup(x => x.VerifyAsync(It.IsAny<string>(), It.IsAny<Password>())).ReturnsAsync(false);

        // Act
        var result = await _useCase.UpdateAsync(dto);

        // Assert
        Assert.False(result.IsSuccess);
        _userRepoMock.Verify(x => x.SearchByIdAsync(It.IsAny<UserId>()), Times.Once);
        _passwordServiceMock.Verify(x => x.VerifyAsync(It.IsAny<string>(), It.IsAny<Password>()), Times.Once);
        _passwordServiceMock.Verify(x => x.CreateAsync(It.IsAny<string>()), Times.Never);
    }

    [Fact]
    public async Task SuccessfullyChangingPassword_ReturnsSuccess()
    {
        // Arrange
        var user = new User(Username.Create("SomeUsername").Value!, new Password("oldPassword"));
        var dto = new UserUpdateDto(13, "oldPassword", "newPassword");
        _userRepoMock.Setup(x => x.SearchByIdAsync(It.IsAny<UserId>())).ReturnsAsync(user);
        _passwordServiceMock.Setup(x => x.VerifyAsync(It.IsAny<string>(), It.IsAny<Password>())).ReturnsAsync(true);
        _passwordServiceMock.Setup(x => x.CreateAsync(It.IsAny<string>()))
            .ReturnsAsync(new Password("#$!Hashed_N3w_p@ssw0rd!$#"));
        _userRepoMock.Setup(x => x.UpdatePasswordAsync(It.IsAny<UserId>(), It.IsAny<Password>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _useCase.UpdateAsync(dto);

        // Assert
        Assert.True(result.IsSuccess);
        _userRepoMock.Verify(x => x.SearchByIdAsync(It.IsAny<UserId>()), Times.Once);
        _passwordServiceMock.Verify(x => x.VerifyAsync(It.IsAny<string>(), It.IsAny<Password>()), Times.Once);
        _passwordServiceMock.Verify(x => x.CreateAsync(It.IsAny<string>()), Times.Once);
        _userRepoMock.Verify(x => x.UpdatePasswordAsync(It.IsAny<UserId>(), It.IsAny<Password>()), Times.Once);
    }
}