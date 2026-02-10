using System.ComponentModel.DataAnnotations;

namespace AuthService.Domain.Entitis;

public class UserPasswordReset
{
    [Key]
    [MaxLength(16)]
    public string Id { get; set; } = string.Empty;

    [Required]
    [MaxLength(16)]
    public string UserId { get; set; } = string.Empty;

    [MaxLength(255)]
    public string PasswordResetToken { get; set; }

    public DateTime PasswordResetTokenExpiry { get; set; }

    //Relaciones
    public User User { get; set; } = null!;
}