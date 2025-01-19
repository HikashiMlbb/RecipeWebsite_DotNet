using SharedKernel;

namespace Domain.UserEntity;

public sealed record Username
{
    private static readonly char[] UnallowedSymbols = @"!@#$%^&*()=+/|\ ".ToCharArray();

    private Username(string value)
    {
        Value = value;
    }

    public string Value { get; init; }

    public static Result<Username> Create(string value)
    {
        if (value.Length is < 4 or > 30) return UserDomainErrors.UsernameLengthOutOfRange;

        if (value.ToCharArray().Any(x => UnallowedSymbols.Contains(x)))
            return UserDomainErrors.UsernameUnallowedSymbols;

        return new Username(value);
    }
}