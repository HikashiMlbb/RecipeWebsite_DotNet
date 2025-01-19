using SharedKernel;

namespace Domain.UserEntity;

public static class UserDomainErrors
{
    public static readonly Error UsernameLengthOutOfRange = new("Username.Length", "Username length is out of range.");

    public static readonly Error UsernameUnallowedSymbols =
        new("Username.Unallowed", "Username contains unallowed symbols.");
}