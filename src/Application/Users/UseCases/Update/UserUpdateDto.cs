namespace Application.Users.UseCases.Update;

public record UserUpdateDto(int Id, string? OldPassword, string? NewPassword);