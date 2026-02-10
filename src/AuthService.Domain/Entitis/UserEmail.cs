using System.ComponentModel.DataAnnotations;
namespace AuthService.Domain.Entitis;

public class UserEmail
{
    [Key]
    [MaxLength(16)]
    public string Id { get; set; } = string.Empty;

    [Required]
    [MaxLength(16)]
    public string UserId { get; set; } = string.Empty;
    
    [Required]
    public bool EmailVerified { get; set; } = false;
    
    [MaxLength(255)]
    public string EmailVerificationToken { get; set; }
    
    public DateTime EmailVerificationTokenExpiry { get; set; }

    //Relaciones
    public User User { get; set; } = null!;
}