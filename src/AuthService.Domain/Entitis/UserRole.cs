using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
namespace AuthService.Domain.Entitis;
public class UserRole
{
    [Key]
    [MaxLength(16)]
    public string Id { get; set; } = string.Empty;

    [Required]
    [MaxLength(16)]
    [ForeignKey(nameof(User))]
    public string UserId { get; set; } = string.Empty;

    [Required]
    [MaxLength(16)]
    [ForeignKey(nameof(Role))]
    public string RoleId { get; set; } = string.Empty;

    [Required]
    public DateTime AssignedAt { get; set; }

    //Relaciones
    public User User { get; set; } = null!;
    public Role Role { get; set; } = null!;
}
