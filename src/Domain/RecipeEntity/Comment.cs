using SharedKernel;

namespace Domain.RecipeEntity;

public sealed record Comment
{
    private Comment(string content)
    {
        Content = content;
    }

    public string Content { get; init; }

    public static Result<Comment> Create(string content)
    {
        if (string.IsNullOrWhiteSpace(content) || content.Length > 500)
            return RecipeDomainErrors.CommentLengthOutOfRange;

        return new Comment(content);
    }
}