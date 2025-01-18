using Domain.UserEntity;
using SharedKernel;

namespace Domain.RecipeEntity;

public sealed record Comment
{
    public string Content { get; init; }
    
    private Comment(string content)
    {
        Content = content;
    }

    public static Result<Comment> Create(string content)
    {
        if (string.IsNullOrWhiteSpace(content) || content.Length > 500)
        {
            return RecipeDomainErrors.CommentLengthOutOfRange;
        }

        return new Comment(content);
    }
}