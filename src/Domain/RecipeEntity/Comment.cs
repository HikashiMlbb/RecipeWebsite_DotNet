using System.Runtime.CompilerServices;
using Domain.UserEntity;
using SharedKernel;

[assembly: InternalsVisibleTo("Application.Tests")]

namespace Domain.RecipeEntity;

public sealed record Comment
{
    internal Comment(User user, string content, DateTimeOffset publishedAt)
    {
        AuthorId = user;
        Content = content;
        PublishedAt = publishedAt;
    }

    public User AuthorId { get; set; }
    public string Content { get; init; }
    public DateTimeOffset PublishedAt { get; init; }

    public static Result<Comment> Create(User author, string content)
    {
        if (string.IsNullOrWhiteSpace(content) || content.Length > 1500)
            return RecipeDomainErrors.CommentLengthOutOfRange;

        return Create(author, content, DateTimeOffset.Now);
    }
    
    public static Result<Comment> Create(User author, string content, DateTimeOffset publishedAt)
    {
        return new Comment(author, content, publishedAt);
    }
}