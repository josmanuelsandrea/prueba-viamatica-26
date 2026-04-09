namespace ViamaticaApi.Domain.Entities;

public class User
{
    public int Userid { get; set; }
    public string Username { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public int RolRolid { get; set; }
    public DateTime Creationdate { get; set; } = DateTime.UtcNow;
    public int? Usercreate { get; set; }
    public int? Userapproval { get; set; }
    public DateTime? Dateapproval { get; set; }
    public string UserstatusStatusid { get; set; } = string.Empty;
    public bool Active { get; set; } = true;
    public DateTime? DeletedAt { get; set; }

    public Rol Rol { get; set; } = null!;
    public UserStatus UserStatus { get; set; } = null!;
    public User? CreatedByUser { get; set; }
    public User? ApprovedByUser { get; set; }
    public ICollection<UserCash> UserCashes { get; set; } = new List<UserCash>();
}
