using System.ComponentModel.DataAnnotations;
 
namespace AuthService.Application.DTOs;
 
public class ForgotPasswordDto
{
    [Required(ErrorMessage = "El email es obligatorio")]
    [EmailAddress(ErrorMessage = "El email es invalido")]
    public string Email { get; set; } = string.Empty;
}