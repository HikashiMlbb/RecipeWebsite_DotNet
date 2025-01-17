namespace Application.Users;

public record UserUpdateDto(int Id, string OldPassword, string NewPassword);