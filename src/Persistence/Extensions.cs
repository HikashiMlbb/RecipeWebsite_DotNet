using Persistence.Repositories.Dto;

namespace Persistence;

public static class Extensions
{
    public static bool IsAbsent(this ICollection<IngredientDatabaseDto> collection, long id)
    {
        return collection.SingleOrDefault(x => x.IngredientId == id) is null;
    }
    
    public static bool IsAbsent(this ICollection<CommentDatabaseDto> collection, long id)
    {
        return collection.SingleOrDefault(x => x.CommentId == id) is null;
    }
}