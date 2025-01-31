using System.Runtime.CompilerServices;
using Domain.UserEntity;
using SharedKernel;

[assembly: InternalsVisibleTo("Application.Tests")]
[assembly: InternalsVisibleTo("Persistence.Tests")]

namespace Domain.RecipeEntity;

public sealed record Comment
{
    internal Comment(User user, string content, DateTimeOffset publishedAt)
    {
        Author = user;
        Content = content;
        PublishedAt = publishedAt;
    }

    public User Author { get; set; }
    public string Content { get; init; }
    public DateTimeOffset PublishedAt { get; init; }

    public static Result<Comment> Create(User author, string content)
    {
        return Create(author, content, DateTimeOffset.Now);
    }
    
    public static Result<Comment> Create(User author, string content, DateTimeOffset publishedAt)
    {
        if (string.IsNullOrWhiteSpace(content) || content.Length > 1500)
            return RecipeDomainErrors.CommentLengthOutOfRange;
        
        return new Comment(author, content, publishedAt);
    }
}