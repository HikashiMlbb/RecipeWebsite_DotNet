using SharedKernel;

namespace Application.Users;

public static class UserErrors
{
    public static readonly Error UserNotFound = new("UserId.NotFound", "User with given ID has not been found.");
    public static readonly Error UserAlreadyExists = new("User.AlreadyExists", "User with given username already exists.");
    public static readonly Error PasswordIsIncorrect = new("User.Password", "Password is incorrect.");

}