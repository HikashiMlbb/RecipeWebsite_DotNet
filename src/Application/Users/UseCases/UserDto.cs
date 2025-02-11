using System.ComponentModel.DataAnnotations;

namespace Application.Users.UseCases;

public record UserDto(
    [Required]string Username, 
    [Required]string Password);