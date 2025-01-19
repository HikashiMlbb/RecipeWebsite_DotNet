using Application.Users.UseCases;
using Application.Users.UseCases.GetById;
using Domain.UserEntity;
using Moq;

// ReSharper disable InconsistentNaming

namespace Application.Tests.UserUseCases;

public class UserGetByIdTests
{
    private readonly UserGetById _useCase;
    private readonly Mock<IUserRepository> _userRepoMock;

    public UserGetByIdTests()
    {
        _userRepoMock = new Mock<IUserRepository>();
        _useCase = new UserGetById(_userRepoMock.Object);
    }

    [Fact]
    public async Task NotFound_ReturnsNull()
    {
        // Arrange
        const int id = 15;
        _userRepoMock.Setup(x => x.SearchByIdAsync(new UserId(id))).ReturnsAsync((User)null!);

        // Act
        var result = await _useCase.GetUserAsync(id);

        // Assert
        Assert.Null(result);
        _userRepoMock.Verify(x => x.SearchByIdAsync(It.IsAny<UserId>()), Times.Once);
    }

    [Fact]
    public async Task Success_ReturnsUser()
    {
        // Arrange
        const int id = 15;
        var user = new User(Username.Create("SomeValidUsername").Value!, new Password("Password"))
            { Id = new UserId(id) };
        _userRepoMock.Setup(x => x.SearchByIdAsync(new UserId(id))).ReturnsAsync(user);

        // Act
        var result = await _useCase.GetUserAsync(id);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(id, result.Id.Value);
        _userRepoMock.Verify(x => x.SearchByIdAsync(It.IsAny<UserId>()), Times.Once);
    }
}