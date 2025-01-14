using SharedKernel;

namespace Domain.RecipeEntity;

public static class RecipeDomainErrors
{
    public static readonly Error TitleLengthOutOfRange = new ("Title.Length", "Title length is out of range.");
    public static readonly Error DescriptionLengthOutOfRange = new("Description.Length", "Description length is out of range.");
    public static readonly Error InstructionLengthOutOfRange = new("Description.Length", "Description length is out of range.");

}
