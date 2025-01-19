using Application.Recipes;
using Application.Recipes.Comment;
using Domain.RecipeEntity;
using Domain.UserEntity;
using Moq;

// ReSharper disable InconsistentNaming

namespace Application.Tests.RecipeUseCases;

public class RecipeCommentTests
{
    private readonly Mock<IRecipeRepository> _repoMock;
    private readonly RecipeComment _useCase;

    public RecipeCommentTests()
    {
        _repoMock = new Mock<IRecipeRepository>();
        _useCase = new RecipeComment(_repoMock.Object);
    }

    [Fact]
    public async Task RecipeNotFound_ReturnsError()
    {
        // Arrange
        var dto = new RecipeCommentDto(123, 456, "Hello, world!");
        _repoMock.Setup(x => x.SearchByIdAsync(It.IsAny<RecipeId>())).ReturnsAsync((Recipe)null!);

        // Act
        var result = await _useCase.Comment(dto);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal(RecipeErrors.RecipeNotFound, result.Error);
        _repoMock.Verify(x => x.SearchByIdAsync(It.IsAny<RecipeId>()), Times.Once);
        _repoMock.Verify(x => x.CommentAsync(It.IsAny<RecipeId>(), It.IsAny<UserId>(), It.IsAny<Comment>()),
            Times.Never);
    }

    [Fact]
    public async Task ContentIsInvalid_ReturnsError()
    {
        // Arrange
        var obj = new Recipe();
        var dto1 = new RecipeCommentDto(123, 456, "");
        var dto2 = new RecipeCommentDto(123, 456, "            ");
        var dto3 = new RecipeCommentDto(123, 456, new string('*', 1000));
        _repoMock.Setup(x => x.SearchByIdAsync(It.IsAny<RecipeId>())).ReturnsAsync(obj);

        // Act
        var result1 = await _useCase.Comment(dto1);
        var result2 = await _useCase.Comment(dto2);
        var result3 = await _useCase.Comment(dto3);

        // Assert
        Assert.False(result1.IsSuccess);
        Assert.Equal(RecipeDomainErrors.CommentLengthOutOfRange, result1.Error);
        Assert.False(result2.IsSuccess);
        Assert.Equal(RecipeDomainErrors.CommentLengthOutOfRange, result2.Error);
        Assert.False(result3.IsSuccess);
        Assert.Equal(RecipeDomainErrors.CommentLengthOutOfRange, result3.Error);

        _repoMock.Verify(x => x.SearchByIdAsync(It.IsAny<RecipeId>()), Times.Exactly(3));
        _repoMock.Verify(x => x.CommentAsync(It.IsAny<RecipeId>(), It.IsAny<UserId>(), It.IsAny<Comment>()),
            Times.Never);
    }

    [Fact]
    public async Task CommentSuccessfully_ReturnsSuccess()
    {
        // Arrange
        var obj = new Recipe();
        var dto = new RecipeCommentDto(123, 456, "Hello, world!");
        _repoMock.Setup(x => x.SearchByIdAsync(It.IsAny<RecipeId>())).ReturnsAsync(obj);

        // Act
        var result = await _useCase.Comment(dto);

        // Assert
        Assert.True(result.IsSuccess);
        _repoMock.Verify(x => x.SearchByIdAsync(It.IsAny<RecipeId>()), Times.Once);
        _repoMock.Verify(x => x.CommentAsync(It.IsAny<RecipeId>(), It.IsAny<UserId>(), It.IsAny<Comment>()),
            Times.Once);
    }
}