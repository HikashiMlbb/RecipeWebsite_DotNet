using SharedKernel;

namespace Application.Users.UseCases;

public static class UserErrors
{
    public static readonly Error UserIdNotFound = new("User.NotFound", "User with given ID has not been found.");
    public static readonly Error UsernameNotFound = new("User.NotFound", "User with given username has not been found.");

    public static readonly Error UserAlreadyExists =
        new("User.AlreadyExists", "User with given username already exists.");

    public static readonly Error PasswordIsIncorrect = new("User.Password", "Password is incorrect.");
}