namespace ViamaticaApi.Application.DTOs.Users;

public class UserResponseDto
{
    public int UserId { get; set; }
    public string Username { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public int RolId { get; set; }
    public string RolName { get; set; } = string.Empty;
    public string StatusId { get; set; } = string.Empty;
    public string StatusDescription { get; set; } = string.Empty;
    public DateTime CreationDate { get; set; }
    public int? UserCreateId { get; set; }
    public string? UserCreateName { get; set; }
    public int? UserApprovalId { get; set; }
    public string? UserApprovalName { get; set; }
    public DateTime? DateApproval { get; set; }
    public bool Active { get; set; }
}
