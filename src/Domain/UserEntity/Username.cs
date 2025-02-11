using System.Text.RegularExpressions;
using SharedKernel;

namespace Domain.UserEntity;

public sealed record Username
{
#pragma warning disable SYSLIB1045
    private static readonly Regex AllowedSymbolsRegex = new(@"^[a-zA-Z][a-zA-Z|_\-0-9]*[a-zA-Z0-9]$");
#pragma warning restore SYSLIB1045

    internal Username(string value)
    {
        Value = value;
    }

    public string Value { get; init; }

    public static Result<Username> Create(string? value)
    {
        if (value is null || value.Length is < 4 or > 30) return UserDomainErrors.UsernameLengthOutOfRange;
        if (!AllowedSymbolsRegex.Match(value).Success) return UserDomainErrors.UsernameUnallowedSymbols;

        return new Username(value);
    }
}