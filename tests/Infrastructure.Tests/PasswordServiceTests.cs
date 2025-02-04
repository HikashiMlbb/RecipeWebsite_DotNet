// ReSharper disable InconsistentNaming

using Application.Users.Services;
using Infrastructure.Services;

namespace Infrastructure.Tests;

public class PasswordServiceTests
{
    private readonly IPasswordService _service;

    public PasswordServiceTests()
    {
        _service = new PasswordService();
    }

    [Fact]
    public async Task PasswordIsIncorrect_ReturnsError()
    {
        // Arrange
        const string enteredPassword = "my random password";
        const string validPassword = "MyVerySecuredPassword1987_$";

        // Act
        var validHashedPassword = await _service.CreateAsync(validPassword);
        var result = await _service.VerifyAsync(enteredPassword, validHashedPassword);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public async Task PasswordIsCorrect_ReturnsTrue()
    {
        // Arrange
        const string enteredPassword = "MyVerySecuredPassword1987_$";
        const string validPassword = "MyVerySecuredPassword1987_$";

        // Act
        var validHashedPassword = await _service.CreateAsync(validPassword);
        var result = await _service.VerifyAsync(enteredPassword, validHashedPassword);

        // Assert
        Assert.True(result);
    }
}